using EnvDTE;
using EnvDTE80;
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
        private readonly OutputWindowPane _owp;

        public InteractionService(DTE2 dte, Debugger debugger, ObjectDumperOptionPage optionPage)
        {
            _debugger = debugger ?? throw new ArgumentNullException(nameof(debugger));
            _optionsPage = optionPage ?? throw new ArgumentNullException(nameof(optionPage));
            _ = dte ?? throw new ArgumentNullException(nameof(dte));

            var win = dte.Windows.Item(Constants.vsWindowKindOutput);
            var ow = (OutputWindow)win.Object;
            _owp = ow.OutputWindowPanes.Add("ObjectDumper");

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
            _owp.OutputString($"Get expression provider by language: {Language}\r\n");

            var isProviderFound = _expressionProvidersByLanguage.TryGetValue(Language, out var expressionProvider);

            if (!isProviderFound)
            {
                return (false, $"Unsupported language: {Language}");
            }

            _owp.OutputString("Expression provider successfully found.\r\n");

            var isSerializerInjected = IsSerializerInjected(expressionProvider);

            if (isSerializerInjected)
            {
                _owp.OutputString("Serializer already injected.\r\n");
                return (true, null);
            }

            _owp.OutputString("Serializer hasn't been injected yet. Trying to inject it.\r\n");

            var dllLocation = Path.GetDirectoryName(new Uri(typeof(ObjectDumperPackage).Assembly.CodeBase, UriKind.Absolute).LocalPath);

            _owp.OutputString($"ObjectDumper package location: {dllLocation}\r\n");

            var (isValid, targetFrameworkName) = GetTargetFrameworkName(expressionProvider);

            if (!isValid)
            {
                _owp.OutputString($"Target framework is not valid: {targetFrameworkName}\r\n");
                return (false, targetFrameworkName);
            }

            var (success, directoryName) = GetSerializerDirectoryName(targetFrameworkName);

            if (!success)
                return (false, directoryName);

            _owp.OutputString($"Serializer directoryName: {directoryName} \r\n");

            var serializerFileName = Path.Combine(dllLocation, "InjectableLibs", directoryName, "YellowFlavor.Serialization.dll");

            var loadAssemblyExpressionText = expressionProvider.GetLoadAssemblyExpressionText(serializerFileName);
            var evaluationResult = _debugger.GetExpression(loadAssemblyExpressionText);

            return (evaluationResult.IsValidValue, evaluationResult.Value);
        }

        private (bool success, string directoryName) GetSerializerDirectoryName(string targetFrameworkName)
        {
            _owp.OutputString($"Get serializer directory name by targetFrameworkName: {targetFrameworkName} \r\n");

            targetFrameworkName = targetFrameworkName?.Trim('"');

            if (string.IsNullOrWhiteSpace(targetFrameworkName))
            {
                _owp.OutputString($"TargetFrameworkName is null or whitespace: {targetFrameworkName} \r\n");
                return (false, $"Wrong TargetFramework: {targetFrameworkName}");
            }

            var match = Regex.Match(targetFrameworkName, "(?<frameworkName>.+?),Version=v(?<frameworkVersion>\\d+((\\.\\d+)*)?)", RegexOptions.Compiled);

            if (!match.Success ||
                !match.Groups["frameworkVersion"].Success ||
                !match.Groups["frameworkName"].Success ||
                !Version.TryParse(match.Groups["frameworkVersion"].Value, out var version))
            {
                _owp.OutputString($"Regex match.Success: {match.Success}\r\n");
                _owp.OutputString($"Regex match.Groups[\"frameworkVersion\"]: {match.Groups["frameworkVersion"].Success}\r\n");
                _owp.OutputString($"Regex match.Groups[\"frameworkName\"]: {match.Groups["frameworkName"].Success}\r\n");
                _owp.OutputString($"Version.TryParse: {Version.TryParse(match.Groups["frameworkVersion"].Value, out _)}\r\n");
                return (false, $"Wrong TargetFramework: {targetFrameworkName}");
            }

            _owp.OutputString($"Regex match.Groups[\"frameworkVersion\"].Value: {match.Groups["frameworkVersion"].Value}\r\n");
            _owp.OutputString($"Regex match.Groups[\"frameworkName\"].Value: {match.Groups["frameworkName"].Value}\r\n");
            _owp.OutputString($"Version: {version}\r\n");

            switch (match.Groups["frameworkName"].Value.ToLowerInvariant())
            {
                case ".netframework":
                    return version < new Version(4, 5)
                        ? (false, "The .NET Framework with a version lower than 4.5 is not supported.")
                        : (true, "net45");

                case ".netcoreapp":
                    if (version < new Version(2, 0))
                    {
                        return (false, "The .NET Core with a version lower than 2.0 is not supported.");
                    }

                    if (version < new Version(3, 1))
                    {
                        return (true, "netstandard2.0");
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
