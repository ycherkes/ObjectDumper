namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.Common
{
    public class CodeArrayDimensionExpression : CodeExpression
    {
        public CodeExpressionCollection Initializers { get; } = new();

        public CodeArrayDimensionExpression(CodeExpression[] initializers)
        {
            Initializers.AddRange(initializers);
        }
    }
}
