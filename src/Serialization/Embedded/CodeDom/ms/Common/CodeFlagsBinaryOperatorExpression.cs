// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom
{
    public class CodeFlagsBinaryOperatorExpression : CodeExpression
    {
        public CodeFlagsBinaryOperatorExpression() { }

        public CodeFlagsBinaryOperatorExpression(CodeBinaryOperatorType op, params CodeExpression[] expressions)
        {
            Operator = op;
            Expressions = new CodeExpressionCollection(expressions);
        }

        public CodeExpressionCollection Expressions { get; set; }

        public CodeBinaryOperatorType Operator { get; set; }
    }
}
