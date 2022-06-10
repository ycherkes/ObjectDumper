using System.IO;
using System.Text;
using Newtonsoft.Json.Embedded;
using ObjectFormatter.CodeDom.Embedded;
using ObjectFormatter.CodeDom.Embedded.ms.CodeDom.System.CodeDom;
using ObjectFormatter.CodeDom.Embedded.ms.CodeDom.System.CodeDom.Compiler;
using ObjectFormatter.CodeDom.Embedded.ms.Common.src.Sys.CodeDom;
using ObjectFormatter.Implementation.Settings;

namespace ObjectFormatter.Implementation
{
    internal class CSharpFormatter: IFormatter
    {
        private static VisitorOptions CsharpDumpOptions => new()
        {
            IgnoreDefaultValues = true,
            IgnoreNullValues = true,
            MaxDepth = 100,
            ExcludeTypes = new[] { "Avro.Schema" }
        };

        public string Format(object obj, string settings)
        {
            var visitorOptions = GetCsharpSettings(settings);
            var objVisitor = new ObjectVisitor(visitorOptions);
            var expression = objVisitor.Visit(obj);
            var variableDeclaration = new CodeVariableDeclarationStatement(new CodeImplicitlyTypedTypeReference(), 
                obj != null ? ReflectionUtils.ComposeVariableName(obj.GetType()) : "nullValue")
            {
                InitExpression = expression
            };

            CodeDomProvider provider = CodeDomProvider.CreateProvider("csharp");

            CodeGeneratorOptions options = new CodeGeneratorOptions
            {
                BracingStyle = "C"
            };
            var stringBuilder = new StringBuilder();
            using (var sourceWriter = new StringWriter(stringBuilder))
            {
                provider.GenerateCodeFromStatement(variableDeclaration, sourceWriter, options);
            }
            var result = stringBuilder.ToString();
            return result;
        }

        private static VisitorOptions GetCsharpSettings(string settings)
        {
            var newSettings = CsharpDumpOptions;
            if (settings == null) return newSettings;

            var csharpSettings = JsonConvert.DeserializeObject<CSharpSettings>(settings);
            newSettings.IgnoreDefaultValues = csharpSettings.IgnoreDefaultValues;
            newSettings.IgnoreNullValues = csharpSettings.IgnoreNullValues;
            newSettings.UseTypeFullName = csharpSettings.UseFullTypeName;
            newSettings.MaxDepth = csharpSettings.MaxDepth;

            return newSettings;
        }
    }
}
