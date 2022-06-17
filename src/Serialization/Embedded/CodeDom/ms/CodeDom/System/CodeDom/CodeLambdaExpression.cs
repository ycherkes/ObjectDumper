// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom
{
    public class CodeLambdaExpression : CodeExpression
    {
        public CodeExpressionCollection Parameters { get; set; } = new CodeExpressionCollection();

        public CodeLambdaExpression() { }

        public CodeLambdaExpression(CodeExpression lambdaExpression, params CodeExpression[] parameters)
            
        {
            LambdaExpression = lambdaExpression;
            Parameters = new CodeExpressionCollection(parameters.ToArray());
        }

        public CodeExpression LambdaExpression { get; set; }
    }
}
