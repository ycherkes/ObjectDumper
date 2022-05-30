using System;
using System.IO;
using System.Text.RegularExpressions;
using EnvDTE80;

namespace ObjectDumper
{
    internal class LanguageService
    {

        private readonly DTE2 _dte;

        public LanguageService(DTE2 dte)
        {
            _dte = dte;
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

            if(!targetFrameworkEvaluationResult.isValid)
            {
                return (false, targetFrameworkEvaluationResult.value);
            }

            var targetFramework = targetFrameworkEvaluationResult.value;

            var isNetCoreMustBeInjected = targetFramework.IndexOf("NETCore", StringComparison.OrdinalIgnoreCase) >= 0
                                          || targetFramework.IndexOf("NETStandard", StringComparison.OrdinalIgnoreCase) >= 0;

            var formatterFileName = Path.Combine(dllLocation,
                "Formatter",
                isNetCoreMustBeInjected ? "netcoreapp3.1" : "net45",
                "ObjectFormatter.dll");

            var loadAssembly = Language == "Basic"
                ? $"System.Reflection.Assembly.LoadFile(\"{formatterFileName}\")"
                : $"System.Reflection.Assembly.LoadFile(@\"{formatterFileName}\")";

            var loadAssemblyExpression = _dte.Debugger.GetExpression(loadAssembly);

            return (loadAssemblyExpression.IsValidValue, loadAssemblyExpression.Value);
        }

        public (bool success, string value) GetFormattedValue(string expression, string format)
        {
            var runFormatterExpression = _dte.Debugger.GetExpression($@"ObjectFormatter.Formatter.Format({expression}, ""{format}"")");

            var (isDecoded, decodedValue) = runFormatterExpression.Value.Trim('"').Base64Decode();

            if(isDecoded)
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

        //public string GetFormattedValueLastChance(string expression, string format)
        //{
        //    var dllLocation = Path.GetDirectoryName(new Uri(typeof(ObjectDumperPackage).Assembly.CodeBase, UriKind.Absolute).LocalPath);

        //    var targetFrameworkEvaluationResult = GetEntryAssemblyTargetFramework();

        //    if (!targetFrameworkEvaluationResult.isValid)
        //    {
        //        return targetFrameworkEvaluationResult.value;
        //    }

        //    var targetFramework = targetFrameworkEvaluationResult.value;

        //    var isNetCoreMustBeInjected = targetFramework.IndexOf("NETCore", StringComparison.OrdinalIgnoreCase) >= 0
        //                                  || targetFramework.IndexOf("NETStandard", StringComparison.OrdinalIgnoreCase) >= 0;

        //    var formatterFileName = Path.Combine(dllLocation,
        //        "Formatter",
        //        isNetCoreMustBeInjected ? "netcoreapp3.1" : "net45",
        //        "ObjectFormatter.dll");

        //    var loadAssemblyAndRunFormatter = Language == "Basic"
        //        ? $"System.Reflection.Assembly.LoadFile(\"{formatterFileName}\").GetType(\"ObjectFormatter.Formatter\").GetMethod(\"Format\").Invoke(Nothing, New Object(){{" + expression + ",\"" + format + "\"})"
        //        : $"System.Reflection.Assembly.LoadFile(@\"{formatterFileName}\").GetType(\"ObjectFormatter.Formatter\").GetMethod(\"Format\").Invoke(null, new object[]{{" + expression + ",\"" + format + "\"})";

        //    var runFormatterExpression = _dte.Debugger.GetExpression(loadAssemblyAndRunFormatter);

        //    var (isDecoded, decodedValue) = runFormatterExpression.Value.Trim('"').Base64Decode();

        //    if (isDecoded)
        //    {
        //        return decodedValue;
        //    }

        //    decodedValue = Regex.Unescape(decodedValue);

        //    if (Language == "Basic")
        //    {
        //        decodedValue = decodedValue
        //            .Replace("\" & vbCrLf & \"", Environment.NewLine)
        //            .Replace("\"\"", "\"");
        //    }

        //    return decodedValue;
        //}

        private bool IsFormatterInjected()
        {
            var isFormatterInjected = Language == "Basic"
                ? "NameOf(ObjectFormatter.Formatter.Format)"
                : "nameof(ObjectFormatter.Formatter.Format)";

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
