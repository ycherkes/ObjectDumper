// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ObjectFormatter.CodeDom.Embedded.ms.CodeDom.System.CodeDom
{
    public class CodeValueTupleCreateExpression : CodeExpression
    {
        public CodeValueTupleCreateExpression() { }

        public CodeValueTupleCreateExpression(params CodeExpression[] parameters)
        {
            Parameters.AddRange(parameters);
        }

        public CodeExpressionCollection Parameters { get; } = new CodeExpressionCollection();
    }
}