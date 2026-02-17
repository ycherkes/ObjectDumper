package com.yellowflavor.objectdumper.actions

class DumpAsCSharpAction : DumpAsAction("cs", "cs", "C#") {
    override fun isFormatEnabled(): Boolean = settings.csharpEnabled
}
