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
                {"C#", cSharpFSharpProvider},
                {"F#", cSharpFSharpProvider},
                {"Basic", new VisualBasicExpressionProvider()}
            };
        }

        private string Language => _debugger.CurrentStackFrame.Language;

        public (bool success, string evaluationResult) InjectFormatter()
        {
            var isServiceFound = _expressionProvidersByLanguage.TryGetValue(Language, out var expressionProvider);

            if (!isServiceFound)
            {
                return (false, $"Unsupported language: {Language}");
            }

            var isFormatterInjected = IsSerializerInjected(expressionProvider);

            if (isFormatterInjected)
            {
                return (true, null);
            }

            var dllLocation = Path.GetDirectoryName(new Uri(typeof(ObjectDumperPackage).Assembly.CodeBase, UriKind.Absolute).LocalPath);

            var (isValid, assemblyLocation) = GetStringTypeAssemblyLocation(expressionProvider);

            if (!isValid)
            {
                return (false, assemblyLocation);
            }

            var isNetCoreMustBeInjected = assemblyLocation.IndexOf("NETCore", StringComparison.OrdinalIgnoreCase) >= 0
                                          || assemblyLocation.IndexOf("NETStandard", StringComparison.OrdinalIgnoreCase) >= 0;

            var serializerFileName = Path.Combine(dllLocation,
                "InjectableLibs",
                isNetCoreMustBeInjected ? "netcoreapp3.1" : "net45",
                "YellowFlavor.Serialization.dll");

            var loadAssemblyExpressionText = expressionProvider.GetLoadAssemblyExpressionText(serializerFileName);
            var evaluationResult = _debugger.GetExpression(loadAssemblyExpressionText);

            return (evaluationResult.IsValidValue, evaluationResult.Value);
        }

        public (bool success, string value) GetSerializedValue(string expression, string format)
        {
            var isServiceFound = _expressionProvidersByLanguage.TryGetValue(Language, out var expressionComposer);

            if (!isServiceFound)
            {
                return (false, $"Unsupported language: {Language}");
            }

            var settings = _optionsPage.ToJson(format).ToBase64();
            var serializeExpressionText = expressionComposer.GetSerializedValueExpressionText(expression, format, settings);
            var evaluationResult = _debugger.GetExpression(serializeExpressionText, Timeout: _optionsPage.CommonOperationTimeoutSeconds * 1000);
            var trimmedValue = evaluationResult.Value.Trim('"');

            if (evaluationResult.IsValidValue)
            {
                return (true, trimmedValue.Base64Decode());
            }

            trimmedValue = Regex.Unescape(trimmedValue);

            if (Language == "Basic")
            {
                trimmedValue = trimmedValue
                    .Replace("\" & vbCrLf & \"", Environment.NewLine)
                    .Replace("\"\"", "\"");
            }

            return (false, trimmedValue);
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
