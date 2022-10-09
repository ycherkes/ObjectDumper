using Embedded.Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Text;
using YellowFlavor.Serialization.Embedded.CodeDom;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.Common;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.Compiler;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.VisualBasic;
using YellowFlavor.Serialization.Implementation.Settings;

namespace YellowFlavor.Serialization.Implementation
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
            DateTimeInstantiation = DateTimeInstantiation.New,
            DateKind = DateKind.ConvertToUtc,
            UseNamedArgumentsForReferenceRecordTypes = false,
            GetPropertiesBindingFlags = BindingFlags.Instance | BindingFlags.Public,
            WritablePropertiesOnly = true
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

            ICodeGenerator generator = new VBCodeGenerator();

            var stringBuilder = new StringBuilder();
            using (var sourceWriter = new StringWriter(stringBuilder))
            {
                generator.GenerateCodeFromStatement(variableDeclaration, sourceWriter, new CodeGeneratorOptions());
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
            newSettings.DateTimeInstantiation = vbSettings.DateTimeInstantiation;
            newSettings.DateKind = vbSettings.DateKind;
            newSettings.UseNamedArgumentsForReferenceRecordTypes = vbSettings.UseNamedArgumentsForReferenceRecordTypes;
            newSettings.GetPropertiesBindingFlags = vbSettings.GetPropertiesBindingFlags;
            newSettings.WritablePropertiesOnly = vbSettings.WritablePropertiesOnly;
            newSettings.GetFieldsBindingFlags = vbSettings.GetFieldsBindingFlags;
            newSettings.SortDirection = vbSettings.SortDirection;

            return newSettings;
        }
    }
}
