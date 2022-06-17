using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using ObjectDumper.Extensions;
using ObjectDumper.Options;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ObjectDumper.DebuggeeInteraction
{
    internal class InteractionService
    {

        private readonly DTE2 _dte;
        private readonly ObjectDumperOptionPage _optionsPage;

        public InteractionService(DTE2 dte, AsyncPackage package)
        {
            _dte = dte;
            _optionsPage = (ObjectDumperOptionPage)package.GetDialogPage(typeof(ObjectDumperOptionPage));
        }

        private string Language => _dte.Debugger.CurrentStackFrame.Language;

        public (bool success, string evaluationResult) InjectFormatter()
        {
            var isFormatterInjected = IsFormatterInjected();

            if (isFormatterInjected)
            {
                return (true, null);
            }

            var dllLocation = Path.GetDirectoryName(new Uri(typeof(ObjectDumperPackage).Assembly.CodeBase, UriKind.Absolute).LocalPath);

            var targetFrameworkEvaluationResult = GetEntryAssemblyTargetFramework();

            if (!targetFrameworkEvaluationResult.isValid)
            {
                return (false, targetFrameworkEvaluationResult.value);
            }

            var targetFramework = targetFrameworkEvaluationResult.value;

            var isNetCoreMustBeInjected = targetFramework.IndexOf("NETCore", StringComparison.OrdinalIgnoreCase) >= 0
                                          || targetFramework.IndexOf("NETStandard", StringComparison.OrdinalIgnoreCase) >= 0;

            var formatterFileName = Path.Combine(dllLocation,
                "Binaries",
                isNetCoreMustBeInjected ? "netcoreapp3.1" : "net45",
                "YellowFlavor.Serialization.ObjectSerializer.dll");

            var loadAssembly = Language == "Basic"
                ? $"System.Reflection.Assembly.LoadFile(\"{formatterFileName}\")"
                : $"System.Reflection.Assembly.LoadFile(@\"{formatterFileName}\")";

            var loadAssemblyExpression = _dte.Debugger.GetExpression(loadAssembly);

            return (loadAssemblyExpression.IsValidValue, loadAssemblyExpression.Value);
        }

        public (bool success, string value) GetFormattedValue(string expression, string format)
        {
            var settings = _optionsPage.ToJson(format).ToBase64();
            var runFormatterExpression = _dte.Debugger.GetExpression($@"YellowFlavor.Serialization.ObjectSerializer.Serialize({expression}, ""{format}"", ""{settings}"")", Timeout: _optionsPage.CommonOperationTimeoutSeconds * 1000);

            var (isDecoded, decodedValue) = runFormatterExpression.Value.Trim('"').Base64Decode();

            if (isDecoded)
            {
                return (true, decodedValue);
            }

            decodedValue = Regex.Unescape(decodedValue);

            if (Language == "Basic")
            {
                decodedValue = decodedValue
                    .Replace("\" & vbCrLf & \"", Environment.NewLine)
                    .Replace("\"\"", "\"");
            }

            return (false, decodedValue);
        }

        private bool IsFormatterInjected()
        {
            var isFormatterInjected = Language == "Basic"
                ? "NameOf(YellowFlavor.Serialization.ObjectSerializer.Serialize)"
                : "nameof(YellowFlavor.Serialization.ObjectSerializer.Serialize)";

            return _dte.Debugger.GetExpression(isFormatterInjected).IsValidValue;
        }

        private (bool isValid, string value) GetEntryAssemblyTargetFramework()
        {

            var targetFramework = Language == "Basic"
                ? "GetType(System.String).Assembly.Location"
                : "typeof(System.String).Assembly.Location";

            var targetFrameworkExpression = _dte.Debugger.GetExpression(targetFramework);

            return (targetFrameworkExpression.IsValidValue, targetFrameworkExpression.Value);
        }
    }
}
