﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using ObjectDumper.OptionPages;

namespace ObjectDumper
{
    internal class LanguageService
    {

        private readonly DTE2 _dte;
        private readonly AsyncPackage _package;

        public LanguageService(DTE2 dte, AsyncPackage package)
        {
            _dte = dte;
            _package = package;
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
            var settings = GetBase64EncodedSettings(format);
            var runFormatterExpression = _dte.Debugger.GetExpression($@"ObjectFormatter.Formatter.Format({expression}, ""{format}"", ""{settings}"")");

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

        private string GetBase64EncodedSettings(string format)
        {
            IBaseSettings settings;

            switch (format)
            {
                case "cs":
                    settings = (IBaseSettings)_package.GetDialogPage(typeof(ObjectDumperCSharpOptionPage));
                    break;
                case "vb":
                    settings = (IBaseSettings)_package.GetDialogPage(typeof(ObjectDumperVisualBasicOptionPage));
                    break;
                case "json":
                    settings = (IBaseSettings)_package.GetDialogPage(typeof(ObjectDumperJsonOptionPage));
                    break;
                case "xml":
                    settings = (IBaseSettings)_package.GetDialogPage(typeof(ObjectDumperXmlOptionPage));
                    break;
                case "yaml":
                    settings = (IBaseSettings)_package.GetDialogPage(typeof(ObjectDumperYamlOptionPage));
                    break;
                default:
                    settings = new BaseSettings();
                break;
            }

            return settings.ToJson().ToBase64();
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
