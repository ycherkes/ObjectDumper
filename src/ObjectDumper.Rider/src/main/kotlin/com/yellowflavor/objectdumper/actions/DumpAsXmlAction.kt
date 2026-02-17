package com.yellowflavor.objectdumper.actions

class DumpAsXmlAction : DumpAsAction("xml", "xml", "XML") {
    override fun isFormatEnabled(): Boolean = settings.xmlEnabled
}
