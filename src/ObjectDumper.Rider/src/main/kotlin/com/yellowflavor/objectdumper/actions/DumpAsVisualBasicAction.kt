package com.yellowflavor.objectdumper.actions

class DumpAsVisualBasicAction : DumpAsAction("vb", "vb", "Visual Basic") {
    override fun isFormatEnabled(): Boolean = settings.visualBasicEnabled
}
