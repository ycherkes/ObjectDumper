// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.Common.src.Sys.CodeDom;

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom
{
    public class CodeObjectCreateAndInitializeExpression : CodeObjectCreateExpression
    {
        public CodeExpressionCollection InitializeExpressions { get; set; } = new CodeExpressionCollection();

        public CodeObjectCreateAndInitializeExpression() { }

        public CodeObjectCreateAndInitializeExpression(CodeTypeReference createType, IEnumerable<CodeExpression> initializeExpressions = null, params CodeExpression[] parameters)
            : base(createType, parameters)
        {
            if (initializeExpressions != null)
            {
                InitializeExpressions = new CodeExpressionCollection(initializeExpressions.ToArray());
            }
        }

        public CodeObjectCreateAndInitializeExpression(string createType, IEnumerable<CodeExpression> initializeExpressions = null, params CodeExpression[] parameters)
            : base(createType, parameters)
        {
            if (initializeExpressions != null)
            {
                InitializeExpressions = new CodeExpressionCollection(initializeExpressions.ToArray());
            }
        }

        public CodeObjectCreateAndInitializeExpression(Type createType, IEnumerable<CodeExpression> initializeExpressions = null, params CodeExpression[] parameters)
            : base(createType, parameters)
        {
            if (initializeExpressions != null)
            {
                InitializeExpressions = new CodeExpressionCollection(initializeExpressions.ToArray());
            }
        }
    }
}
