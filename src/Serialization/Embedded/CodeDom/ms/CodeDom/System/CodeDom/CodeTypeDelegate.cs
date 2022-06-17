// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.Common.src.Sys.CodeDom;

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom
{
    public class CodeTypeDelegate : CodeTypeDeclaration
    {
        private CodeTypeReference _returnType;

        public CodeTypeDelegate()
        {
            TypeAttributes &= ~TypeAttributes.ClassSemanticsMask;
            TypeAttributes |= TypeAttributes.Class;
            BaseTypes.Clear();
            BaseTypes.Add(new CodeTypeReference("System.Delegate"));
        }

        public CodeTypeDelegate(string name) : this()
        {
            Name = name;
        }

        public CodeTypeReference ReturnType
        {
            get { return _returnType ?? (_returnType = new CodeTypeReference("")); }
            set { _returnType = value; }
        }

        public CodeParameterDeclarationExpressionCollection Parameters { get; } = new CodeParameterDeclarationExpressionCollection();
    }
}
