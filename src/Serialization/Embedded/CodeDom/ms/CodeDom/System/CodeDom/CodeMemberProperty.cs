// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using YellowFlavor.Serialization.Embedded.CodeDom.ms.Common.src.Sys.CodeDom;

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom
{
    public class CodeMemberProperty : CodeTypeMember
    {
        private CodeTypeReference _type;
        private bool _hasGet;
        private bool _hasSet;
        private CodeTypeReferenceCollection _implementationTypes = null;

        public CodeTypeReference PrivateImplementationType { get; set; }

        public CodeTypeReferenceCollection ImplementationTypes => _implementationTypes ?? (_implementationTypes = new CodeTypeReferenceCollection());

        public CodeTypeReference Type
        {
            get { return _type ?? (_type = new CodeTypeReference("")); }
            set { _type = value; }
        }

        public bool HasGet
        {
            get { return _hasGet || GetStatements.Count > 0; }
            set
            {
                _hasGet = value;
                if (!value)
                {
                    GetStatements.Clear();
                }
            }
        }

        public bool HasSet
        {
            get { return _hasSet || SetStatements.Count > 0; }
            set
            {
                _hasSet = value;
                if (!value)
                {
                    SetStatements.Clear();
                }
            }
        }

        public CodeStatementCollection GetStatements { get; } = new CodeStatementCollection();

        public CodeStatementCollection SetStatements { get; } = new CodeStatementCollection();

        public CodeParameterDeclarationExpressionCollection Parameters { get; } = new CodeParameterDeclarationExpressionCollection();
    }
}
