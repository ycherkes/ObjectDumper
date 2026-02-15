package com.yellowflavor.objectdumper.actions

class DumpAsYamlAction : DumpAsAction("yaml", "yaml", "YAML") {
    override fun isFormatEnabled(): Boolean = settings.yamlEnabled
}
