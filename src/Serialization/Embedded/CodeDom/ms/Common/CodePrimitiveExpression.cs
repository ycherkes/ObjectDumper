// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom
{
    public class CodePrimitiveExpression : CodeExpression
    {
        public CodePrimitiveExpression() { }

        public CodePrimitiveExpression(object value)
        {
            Value = value;
        }

        public object Value { get; set; }
    }
}
