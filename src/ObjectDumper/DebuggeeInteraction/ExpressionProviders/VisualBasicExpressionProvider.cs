﻿namespace ObjectDumper.DebuggeeInteraction.ExpressionProviders
{
    internal class VisualBasicExpressionProvider : IExpressionProvider
    {
        public string GetStringTypeAssemblyLocationExpressionText()
        {
            return "GetType(System.String).Assembly.Location";
        }

        public string GetIsSerializerInjectedExpressionText()
        {
            return "NameOf(YellowFlavor.Serialization.ObjectSerializer.Serialize)";
        }

        public string GetSerializedValueExpressionText(string expression, string format, string settings)
        {
            return $@"YellowFlavor.Serialization.ObjectSerializer.Serialize({expression}, ""{format}"", ""{settings}"")";
        }

        public string GetLoadAssemblyExpressionText(string serializerFileName)
        {
            return $"System.Reflection.Assembly.LoadFile(\"{serializerFileName}\")";
        }
    }
}
