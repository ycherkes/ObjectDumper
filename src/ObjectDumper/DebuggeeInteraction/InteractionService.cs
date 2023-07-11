using EnvDTE;
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

        public InteractionService(Debugger debugger, ObjectDumperOptionPage optionPage)
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

            var dllLocation = Path.GetDirectoryName(new Uri(typeof(ObjectDumperPackage).Assembly.CodeBase, UriKind.Absolute).LocalPath);

            var (isValid, targetFrameworkName) = GetTargetFrameworkName(expressionProvider);

            if (!isValid)
            {
                return (false, targetFrameworkName);
            }

            var (success, directoryName) = GetSerializerDirectoryName(targetFrameworkName);

            if (!success)
                return (false, directoryName);

            var serializerFileName = Path.Combine(dllLocation, "InjectableLibs", directoryName, "YellowFlavor.Serialization.dll");

            var loadAssemblyExpressionText = expressionProvider.GetLoadAssemblyExpressionText(serializerFileName);
            var evaluationResult = _debugger.GetExpression(loadAssemblyExpressionText);

            return (evaluationResult.IsValidValue, evaluationResult.Value);
        }

        private static (bool success, string directoryName) GetSerializerDirectoryName(string targetFrameworkName)
        {
            targetFrameworkName = targetFrameworkName?.Trim('"');

            if (string.IsNullOrWhiteSpace(targetFrameworkName))
            {
                return (false, $"Wrong TargetFramework: {targetFrameworkName}");
            }

            var match = Regex.Match(targetFrameworkName, "(?<frameworkName>.+?),\\s*Version\\s*=\\s*v(?<frameworkVersion>\\d+(\\.\\d+)+?)", RegexOptions.Compiled);

            if (!match.Success ||
                !match.Groups["frameworkVersion"].Success ||
                !match.Groups["frameworkName"].Success ||
                !Version.TryParse(match.Groups["frameworkVersion"].Value, out var version))
            {
                return (false, $"Wrong TargetFramework: {targetFrameworkName}");
            }

            switch (match.Groups["frameworkName"].Value.ToLowerInvariant())
            {
                case ".netframework":
                    return version < new Version(4, 6, 1)
                        ? (false, "The .NET Framework with a version lower than 4.6.1 is not supported.")
                        : (true, "net461");

                case ".netcoreapp":
                    if (version < new Version(2, 0))
                    {
                        return (false, "The .NET Core with a version lower than 2.0 is not supported.");
                    }

                    if (version < new Version(3, 1))
                    {
                        return (true, "netcoreapp2.0");
                    }

                    return version >= new Version(6, 0)
                        ? (true, "net6.0")
                        : (true, "netcoreapp3.1");

                case ".netstandard":
                    return version < new Version(2, 0)
                        ? (false, "The .NET Standard with a version lower than 2.0 is not supported.")
                        : (true, "netstandard2.0");

                default:
                    return (false, $"Unsupported TargetFramework: {targetFrameworkName}");
            }
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

        private (bool isValid, string value) GetTargetFrameworkName(IExpressionProvider expressionProvider)
        {
            var targetFrameworkExpressionText = expressionProvider.GetTargetFrameworkExpressionText();
            var evaluationResult = _debugger.GetExpression(targetFrameworkExpressionText);
            return (evaluationResult.IsValidValue, evaluationResult.Value);
        }
    }
}
