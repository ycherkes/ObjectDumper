package com.yellowflavor.objectdumper.actions

class DumpAsJsonAction : DumpAsAction("json", "json", "JSON") {
    override fun isFormatEnabled(): Boolean = settings.jsonEnabled
}
