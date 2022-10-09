// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.Common
{
    public class CodeComment : CodeObject
    {
        private string _text;

        public CodeComment() { }

        public CodeComment(string text)
        {
            Text = text;
        }

        public CodeComment(string text, bool docComment)
        {
            Text = text;
            DocComment = docComment;
        }

        public bool DocComment { get; set; }

        public string Text
        {
            get { return _text ?? string.Empty; }
            set { _text = value; }
        }

        public bool NoNewLine { get; set; } = false;
    }
}
