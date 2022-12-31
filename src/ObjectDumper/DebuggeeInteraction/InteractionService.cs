using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using ObjectDumper.DebuggeeInteraction.ExpressionProviders;
using ObjectDumper.Extensions;
using ObjectDumper.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ObjectDumper.DebuggeeInteraction
{
    internal class InteractionService
    {
        private readonly ObjectDumperOptionPage _optionsPage;
        private readonly Dictionary<string, IExpressionProvider> _expressionProvidersByLanguage;
        private readonly Debugger _debugger;

        public InteractionService(DTE2 dte, Package package)
        {
            _debugger = dte.Debugger;
            _optionsPage = (ObjectDumperOptionPage)package.GetDialogPage(typeof(ObjectDumperOptionPage));

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

            var dllLocation = Path.GetDirectoryName(new Uri(typeof(ObjectDumperPackage).Assembly.CodeBase, UriKind.Absolute).LocalPath);

            var (isValid, assemblyLocation) = GetStringTypeAssemblyLocation(expressionProvider);

            if (!isValid)
            {
                return (false, assemblyLocation);
            }

            var isNetStandardMustBeInjected = assemblyLocation.IndexOf("NETStandard", StringComparison.OrdinalIgnoreCase) >= 0;

            var isNetCoreMustBeInjected = assemblyLocation.IndexOf("NETCore", StringComparison.OrdinalIgnoreCase) >= 0;

            var isNetCore6OrAbove = isNetCoreMustBeInjected && ExtractVersionNumber(assemblyLocation).Major >= 6;

            var directoryName = isNetStandardMustBeInjected
                ? "netstandard2.0"
                : isNetCore6OrAbove
                    ? "net6.0"
                    : isNetCoreMustBeInjected
                        ? "netcoreapp3.1"
                        : "net45";

            var serializerFileName = Path.Combine(dllLocation,
            "InjectableLibs",
            directoryName,
            "YellowFlavor.Serialization.dll");

            var loadAssemblyExpressionText = expressionProvider.GetLoadAssemblyExpressionText(serializerFileName);
            var evaluationResult = _debugger.GetExpression(loadAssemblyExpressionText);

            return (evaluationResult.IsValidValue, evaluationResult.Value);
        }

        private static Version ExtractVersionNumber(string input)
        {
            var regex = new Regex(@"\d+(\.\d+)+");
            var match = regex.Match(input);
            Version.TryParse(match.Value, out var version);
            return version;
        }

        public string GetSerializedValue(string expression, string format)
        {
            var isProviderFound = _expressionProvidersByLanguage.TryGetValue(Language, out var expressionComposer);

            if (!isProviderFound)
            {
                return $"Unsupported language: {Language}";
            }

            var settings = _optionsPage.ToJson(format).ToBase64();
            var serializeExpressionText = expressionComposer.GetSerializedValueExpressionText(expression, format, settings);
            var evaluationResult = _debugger.GetExpression(serializeExpressionText, Timeout: _optionsPage.CommonOperationTimeoutSeconds * 1000);
            var trimmedValue = evaluationResult.Value.Trim('"');

            if (evaluationResult.IsValidValue)
            {
                return trimmedValue.Base64Decode();
            }

            trimmedValue = Regex.Unescape(trimmedValue);

            if (Language == "Basic")
            {
                trimmedValue = trimmedValue
                    .Replace("\" & vbCrLf & \"", Environment.NewLine)
                    .Replace("\"\"", "\"");
            }

            return trimmedValue;
        }

        private bool IsSerializerInjected(IExpressionProvider expressionProvider)
        {
            var isSerializerInjectedExpressionText = expressionProvider.GetIsSerializerInjectedExpressionText();
            return _debugger.GetExpression(isSerializerInjectedExpressionText).IsValidValue;
        }

        private (bool isValid, string value) GetStringTypeAssemblyLocation(IExpressionProvider expressionProvider)
        {
            var assemblyLocationExpressionText = expressionProvider.GetStringTypeAssemblyLocationExpressionText();
            var evaluationResult = _debugger.GetExpression(assemblyLocationExpressionText);
            return (evaluationResult.IsValidValue, evaluationResult.Value);
        }
    }
}
