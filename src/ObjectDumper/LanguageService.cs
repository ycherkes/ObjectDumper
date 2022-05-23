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

        public bool InjectFormatter()
        {
            var isFormatterInjected = IsFormatterInjected();

            if (isFormatterInjected)
            {
                return true;
            }

            var dllLocation = Path.GetDirectoryName(new Uri(typeof(ObjectDumperPackage).Assembly.CodeBase, UriKind.Absolute).LocalPath);

            var targetFramework = GetEntryAssemblyTargetFramework()?.Trim('"');

            if (targetFramework == null)
            {
                return false;
            }

            var isNetCoreMustBeInjected = targetFramework.StartsWith(".NETCoreApp", StringComparison.OrdinalIgnoreCase)
                                          || targetFramework.StartsWith(".NETStandard", StringComparison.OrdinalIgnoreCase);

            var formatterFileName = Path.Combine(dllLocation,
                "Formatter",
                isNetCoreMustBeInjected ? "netcoreapp3.1" : "net45",
                "ObjectFormatter.dll");

            var loadAssembly = Language == "Basic"
                ? $"System.Reflection.Assembly.LoadFile(\"{formatterFileName}\")"
                : $"System.Reflection.Assembly.LoadFile(@\"{formatterFileName}\")";

            var loadAssemblyExpression = _dte.Debugger.GetExpression(loadAssembly);

            return loadAssemblyExpression.IsValidValue;
        }

        public string GetFormattedValue(string expression, string format)
        {
            var runFormatterExpression = _dte.Debugger.GetExpression($@"ObjectFormatter.Formatter.Format({expression}, ""{format}"")");

            var (isDecoded, decodedValue) = runFormatterExpression.Value.Trim('"').Base64Decode();

            if(isDecoded)
            {
                return decodedValue;
            }

            decodedValue = Regex.Unescape(decodedValue);

            if (Language == "Basic")
            {
                decodedValue = decodedValue
                    .Replace("\" & vbCrLf & \"", Environment.NewLine)
                    .Replace("\"\"", "\"");
            }

            return decodedValue;
        }

        private bool IsFormatterInjected()
        {
            var isFormatterInjected = Language == "Basic"
                ? "NameOf(ObjectFormatter.Formatter.Format)"
                : "nameof(ObjectFormatter.Formatter.Format)";

            return _dte.Debugger.GetExpression(isFormatterInjected).IsValidValue;
        }

        private string GetEntryAssemblyTargetFramework()
        {
            var targetFramework = Language == "Basic"
                ? "System.Linq.Enumerable.First(Of System.Runtime.Versioning.TargetFrameworkAttribute)(System.Linq.Enumerable.OfType(Of System.Runtime.Versioning.TargetFrameworkAttribute)(If(System.Reflection.Assembly.GetEntryAssembly(), System.Reflection.Assembly.GetCallingAssembly()).GetCustomAttributes(True))).FrameworkName"
                : "System.Linq.Enumerable.First(System.Linq.Enumerable.OfType<System.Runtime.Versioning.TargetFrameworkAttribute>((System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetCallingAssembly()).GetCustomAttributes(true))).FrameworkName";

            var targetFrameworkExpression = _dte.Debugger.GetExpression(targetFramework);

            return targetFrameworkExpression.IsValidValue 
                ? targetFrameworkExpression.Value 
                : null;
        }
    }
}
