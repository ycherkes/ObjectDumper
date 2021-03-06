// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using YellowFlavor.Serialization.Embedded.CodeDom.ms.Common.src.Sys.CodeDom;

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom
{
    public class CodeDelegateCreateExpression : CodeExpression
    {
        private CodeTypeReference _delegateType;
        private string _methodName;

        public CodeDelegateCreateExpression() { }

        public CodeDelegateCreateExpression(CodeTypeReference delegateType, CodeExpression targetObject, string methodName)
        {
            _delegateType = delegateType;
            TargetObject = targetObject;
            _methodName = methodName;
        }

        public CodeTypeReference DelegateType
        {
            get { return _delegateType ?? (_delegateType = new CodeTypeReference("")); }
            set { _delegateType = value; }
        }

        public CodeExpression TargetObject { get; set; }

        public string MethodName
        {
            get { return _methodName ?? string.Empty; }
            set { _methodName = value; }
        }
    }
}
