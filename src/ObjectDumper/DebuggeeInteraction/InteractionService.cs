﻿using EnvDTE80;
using ObjectDumper.DebuggeeInteraction.ExpressionProviders;
using ObjectDumper.Extensions;
using ObjectDumper.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace ObjectDumper.DebuggeeInteraction;

internal class InteractionService
{
    private readonly ObjectDumperOptionPage _optionsPage;
    private readonly Dictionary<string, IExpressionProvider> _expressionProvidersByLanguage;
    private readonly EnvDTE.Debugger _debugger;

    public InteractionService(EnvDTE.Debugger debugger, ObjectDumperOptionPage optionPage)
    {
        _debugger = debugger ?? throw new ArgumentNullException(nameof(debugger));
        _optionsPage = optionPage ?? throw new ArgumentNullException(nameof(optionPage));

        var cSharpFSharpProvider = new CSharpFSharpExpressionProvider();
        _expressionProvidersByLanguage = new Dictionary<string, IExpressionProvider>(StringComparer.OrdinalIgnoreCase)
        {
            { "C#", cSharpFSharpProvider },
            { "F#", cSharpFSharpProvider },
            { "Basic", new VisualBasicExpressionProvider() }
        };
    }

    private string Language => _debugger.CurrentStackFrame.Language;

    public (bool success, string evaluationResult) InjectSerializer()
    {
        var isProviderFound = _expressionProvidersByLanguage.TryGetValue(Language, out var expressionProvider);

        if (!isProviderFound)
        {
            return (false, $"Unsupported language: {Language}");
        }

        var isSerializerInjected = IsSerializerInjected(expressionProvider);

        if (isSerializerInjected)
        {
            return (true, null);
        }

        var (isValid, targetFrameworkName) = GetTargetFrameworkName(expressionProvider);

        if (!isValid)
        {
            return (false, targetFrameworkName);
        }

        var (success, directoryName) = GetSerializerDirectoryName(targetFrameworkName);

        if (!success)
            return (false, directoryName);

        var dllLocation = Path.GetDirectoryName(new Uri(typeof(ObjectDumperPackage).Assembly.CodeBase, UriKind.Absolute).LocalPath);
        var serializerFileName = Path.Combine(dllLocation, "InjectableLibs", directoryName, "YellowFlavor.Serialization.dll");

        var loadAssemblyExpressionText = expressionProvider.GetLoadAssemblyExpressionText(serializerFileName);
        var evaluationResult = _debugger.GetExpression(loadAssemblyExpressionText);

        return (evaluationResult.IsValidValue, evaluationResult.Value);
    }

    private static (bool success, string directoryName) GetSerializerDirectoryName(string targetFrameworkName)
    {
        var targetFramework = targetFrameworkName?
            .Split(new []{';'}, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new FrameworkName(x.Trim('"')))
            .OrderBy(fn => fn.Identifier.ToLowerInvariant() == "netstandard" ? 1 : 0)
            .ThenByDescending(fn => fn.Version)
            .FirstOrDefault();

        if (targetFramework == null)
        {
            return (false, $"Wrong TargetFramework: {targetFrameworkName}");
        }

        switch (targetFramework.Identifier.ToLowerInvariant())
        {
            case ".netframework":
                return targetFramework.Version < new Version(4, 5)
                    ? (false, "The .NET Framework with a version lower than 4.5 is not supported.")
                    : (true, "net45");

            case ".netcoreapp":
                if (targetFramework.Version < new Version(2, 0))
                {
                    return (false, "The .NET Core with a version lower than 2.0 is not supported.");
                }

                if (targetFramework.Version < new Version(3, 1))
                {
                    return (true, "netcoreapp2.0");
                }

                return targetFramework.Version >= new Version(6, 0)
                    ? (true, "net6.0")
                    : (true, "netcoreapp3.1");

            case ".netstandard":
                return targetFramework.Version < new Version(2, 0)
                    ? (false, "The .NET Standard with a version lower than 2.0 is not supported.")
                    : (true, "netstandard2.0");

            default:
                return (false, $"Unsupported TargetFramework: {targetFramework}");
        }
    }

    public (string value, bool isFilePath) Serialize(string expression, string format)
    {
        var isProviderFound = _expressionProvidersByLanguage.TryGetValue(Language, out var expressionComposer);

        if (!isProviderFound)
        {
            return ($"Unsupported language: {Language}", false);
        }

        var fileName = GetTempFileName();

        var settings = string.Join(";", format, fileName, _optionsPage.ToJson(format))
                                   .ToBase64();
        
        var serializeExpressionText = expressionComposer.GetSerializedValueExpressionText(expression, settings);

        var evaluationResult = _debugger.GetExpression(serializeExpressionText,
                Timeout: _optionsPage.CommonOperationTimeoutSeconds * 1000);

        if (evaluationResult.IsValidValue)
        {
            return (fileName, true);
        }

        try
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
        catch
        {
            // ignored temp file deletion error
        }

        var trimmedValue = evaluationResult.Value.Trim('"');
        trimmedValue = Regex.Unescape(trimmedValue);

        if (Language == "Basic")
        {
            trimmedValue = trimmedValue
                .Replace("\" & vbCrLf & \"", Environment.NewLine)
                .Replace("\"\"", "\"");
        }

        return (trimmedValue, false);
    }

    private string GetTempFileName()
    {
        var isUwpApplication = IsUwpApplication();

        if (!isUwpApplication)
        {
            return Path.GetTempFileName();
        }
        
        var tempFileNameExpression = _debugger.GetExpression("System.IO.Path.GetTempFileName()");

        if (!tempFileNameExpression.IsValidValue)
        {
            return Path.GetTempFileName();
        }

        return Regex.Unescape(tempFileNameExpression.Value.Trim('"'));

    }

    private bool IsUwpApplication()
    {
        if (_debugger.DebuggedProcesses.Count <= 0)
        {
            return false;
        }

        var process = (Process2)_debugger.DebuggedProcesses.Item(1);
        
        return process.Name.Contains(@"\AppX\");
    }

    private bool IsSerializerInjected(IExpressionProvider expressionProvider)
    {
        var isSerializerInjectedExpressionText = expressionProvider.GetIsSerializerInjectedExpressionText();
        return _debugger.GetExpression(isSerializerInjectedExpressionText).IsValidValue;
    }

    private (bool isValid, string value) GetTargetFrameworkName(IExpressionProvider expressionProvider)
    {
        var targetFrameworkExpressionText = expressionProvider.GetTargetFrameworkExpressionText();
        var evaluationResult = _debugger.GetExpression(targetFrameworkExpressionText);
        return (evaluationResult.IsValidValue, evaluationResult.Value);
    }
}