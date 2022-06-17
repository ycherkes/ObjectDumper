using Newtonsoft.Json.Embedded;
using ObjectFormatter.CodeDom.Embedded;
using ObjectFormatter.CodeDom.Embedded.ms.CodeDom.System.CodeDom;
using ObjectFormatter.CodeDom.Embedded.ms.CodeDom.System.CodeDom.Compiler;
using ObjectFormatter.CodeDom.Embedded.ms.Common.src.Sys.CodeDom;
using ObjectFormatter.Implementation.Settings;
using System.IO;
using System.Text;

namespace ObjectFormatter.Implementation
{
    internal class VisualBasicSerializer : ISerializer
    {
        private static VisitorOptions VisualBasicVisitorOptions => new()
        {
            IgnoreDefaultValues = true,
            IgnoreNullValues = true,
            MaxDepth = 25,
            ExcludeTypes = new[] { "Avro.Schema" },
            UseTypeFullName = false,
            ConvertDateTimeToUtc = true
        };

        public string Serialize(object obj, string settings)
        {
            var visitorOptions = GetVbSettings(settings);
            var objVisitor = new ObjectVisitor(visitorOptions);
            var expression = objVisitor.Visit(obj);
            var variableDeclaration = new CodeVariableDeclarationStatement(new CodeImplicitlyTypedTypeReference(),
                obj != null ? ReflectionUtils.ComposeVariableName(obj.GetType()) : "nullValue")
            {
                InitExpression = expression
            };

            CodeDomProvider provider = CodeDomProvider.CreateProvider("visualbasic");

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

        private static VisitorOptions GetVbSettings(string settings)
        {
            var newSettings = VisualBasicVisitorOptions;
            if (settings == null) return newSettings;

            var vbSettings = JsonConvert.DeserializeObject<VbSettings>(settings);
            newSettings.IgnoreDefaultValues = vbSettings.IgnoreDefaultValues;
            newSettings.IgnoreNullValues = vbSettings.IgnoreNullValues;
            newSettings.UseTypeFullName = vbSettings.UseFullTypeName;
            newSettings.MaxDepth = vbSettings.MaxDepth;
            newSettings.ConvertDateTimeToUtc = vbSettings.ConvertDateTimeToUtc;

            return newSettings;
        }
    }
}
