// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom.Compiler;

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.Microsoft.VisualBasic
{
    internal sealed partial class VBCodeGenerator
    {
        protected override CompilerResults FromFileBatch(CompilerParameters options, string[] fileNames)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
