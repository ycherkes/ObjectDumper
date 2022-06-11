// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using ObjectFormatter.CodeDom.Embedded.ms.CodeDom.System.CodeDom.Compiler;

namespace ObjectFormatter.CodeDom.Embedded.ms.CodeDom.Microsoft.CSharp
{
    internal sealed partial class CSharpCodeGenerator
    {
        private CompilerResults FromFileBatch(CompilerParameters options, string[] fileNames)
        {
            throw new PlatformNotSupportedException();
        }
    }
}