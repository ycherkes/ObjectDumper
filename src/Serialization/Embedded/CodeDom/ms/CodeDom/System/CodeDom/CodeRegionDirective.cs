// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom
{
    public class CodeRegionDirective : CodeDirective
    {
        private string _regionText;

        public CodeRegionDirective() { }

        public CodeRegionDirective(CodeRegionMode regionMode, string regionText)
        {
            RegionText = regionText;
            RegionMode = regionMode;
        }

        public string RegionText
        {
            get { return _regionText ?? string.Empty; }
            set { _regionText = value; }
        }

        public CodeRegionMode RegionMode { get; set; }
    }
}
