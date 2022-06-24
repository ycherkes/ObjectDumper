using Embedded.Newtonsoft.Json;
using System.IO;
using System.Text;
using YellowFlavor.Serialization.Embedded.CodeDom;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom.Compiler;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.Common.src.Sys.CodeDom;
using YellowFlavor.Serialization.Implementation.Settings;

namespace YellowFlavor.Serialization.Implementation
{
    internal class CSharpSerializer : ISerializer
    {
        private static VisitorOptions CsharpVisitorOptions => new()
        {
            IgnoreDefaultValues = true,
            IgnoreNullValues = true,
            MaxDepth = 25,
            ExcludeTypes = new[] { "Avro.Schema" },
            UseTypeFullName = false,
            DateTimeInstantiation = DateTimeInstantiation.New,
            DateKind = DateKind.ConvertToUtc
        };

        public string Serialize(object obj, string settings)
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
            var newSettings = CsharpVisitorOptions;
            if (settings == null) return newSettings;

            var csharpSettings = JsonConvert.DeserializeObject<CSharpSettings>(settings);
            newSettings.IgnoreDefaultValues = csharpSettings.IgnoreDefaultValues;
            newSettings.IgnoreNullValues = csharpSettings.IgnoreNullValues;
            newSettings.UseTypeFullName = csharpSettings.UseFullTypeName;
            newSettings.MaxDepth = csharpSettings.MaxDepth;
            newSettings.DateTimeInstantiation = csharpSettings.DateTimeInstantiation;
            newSettings.DateKind = csharpSettings.DateKind;

            return newSettings;
        }
    }
}
