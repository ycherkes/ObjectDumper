// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom
{
    public class CodeImplicitKeyValuePairCreateExpression : CodeExpression
    {
        public CodeExpression Key { get; }
        public CodeExpression Value { get; }
        public CodeImplicitKeyValuePairCreateExpression() { }

        public CodeImplicitKeyValuePairCreateExpression(CodeExpression key, CodeExpression value)
        {
            Key = key;
            Value = value;
        }
    }
}
