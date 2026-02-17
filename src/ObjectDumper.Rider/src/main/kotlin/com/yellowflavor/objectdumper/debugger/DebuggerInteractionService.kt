package com.yellowflavor.objectdumper.debugger

import com.intellij.openapi.project.Project
import com.intellij.xdebugger.XDebugSession
import com.intellij.xdebugger.XDebuggerManager
import com.intellij.ide.plugins.PluginManagerCore
import com.intellij.openapi.extensions.PluginId
import com.yellowflavor.objectdumper.settings.ObjectDumperSettings
import java.io.File
import java.nio.file.Paths
import java.util.*

class DebuggerInteractionService(private val project: Project) {
    
    private val settings = ObjectDumperSettings.getInstance()
    
    fun getCurrentDebugSession(): XDebugSession? {
        return XDebuggerManager.getInstance(project).currentSession
    }
    
    fun isDebugging(): Boolean {
        return getCurrentDebugSession() != null
    }
    
    fun getTimeoutSeconds(): Int {
        return settings.commonOperationTimeoutSeconds
    }
    
    fun getSerializerLibraryPath(targetFramework: String): String? {
        val pluginPath = getPluginPath()
        val frameworkDir = getFrameworkDirectory(targetFramework) ?: return null
        
        val path = Paths.get(pluginPath, "InjectableLibs", frameworkDir, "YellowFlavor.Serialization.dll").toString()
        
        // Verify file exists
        return if (File(path).exists()) path else null
    }
    
    fun getPluginPath(): String {
        try {
            // Use IntelliJ Platform API to get plugin path
            val pluginId = PluginId.getId("com.yellowflavor.objectdumper")
            val plugin = PluginManagerCore.getPlugin(pluginId)
            if (plugin != null) {
                return plugin.pluginPath.toString()
            }
        } catch (_: Exception) {
            // Fallback to classloader approach
        }
        
        // Fallback: try to get from classloader
        try {
            val pluginClass = this::class.java
            val classPath = pluginClass.protectionDomain?.codeSource?.location?.toURI()?.path
            
            if (classPath != null) {
                var file = File(classPath)
                
                // If we're in a JAR file, get its parent directory
                if (file.isFile && file.name.endsWith(".jar")) {
                    file = file.parentFile
                }
                
                // Navigate up to find the plugin root (should contain InjectableLibs)
                while (file != null && !File(file, "InjectableLibs").exists()) {
                    file = file.parentFile
                }
                
                if (file != null) {
                    return file.absolutePath
                }
            }
        } catch (_: Exception) {
            // Ignore and return empty
        }
        
        return ""
    }
    
    private fun getFrameworkDirectory(targetFramework: String): String? {
        val lowerFramework = targetFramework.lowercase(Locale.getDefault())
        
        // The VS extension gets a FrameworkName like ".NETCoreApp,Version=v8.0;.NETCoreApp,Version=v8.0"
        // Split by semicolons and try to match each part
        val parts = targetFramework.split(";").map { it.trim().trim('"') }.filter { it.isNotBlank() }
        
        for (part in parts) {
            val lowerPart = part.lowercase(Locale.getDefault())
            
            // Skip .NETStandard entries first (they are lower priority per VS extension logic)
            if (lowerPart.contains("netstandard") || lowerPart.contains(".net standard")) {
                continue
            }
            
            val dir = matchFrameworkPart(lowerPart)
            if (dir != null) return dir
        }
        
        // Try .NETStandard entries as fallback
        for (part in parts) {
            val lowerPart = part.lowercase(Locale.getDefault())
            if (lowerPart.contains("netstandard") || lowerPart.contains(".net standard")) {
                val dir = matchFrameworkPart(lowerPart)
                if (dir != null) return dir
            }
        }
        
        // If no semicolons, try the whole string
        return matchFrameworkPart(lowerFramework)
    }
    
    private fun matchFrameworkPart(lowerPart: String): String? {
        return when {
            lowerPart.contains(".netframework") -> {
                val version = extractVersion(lowerPart)
                if (version >= Version(4, 5)) "net45" else null
            }
            lowerPart.contains(".netcoreapp") -> {
                val version = extractVersion(lowerPart)
                when {
                    version >= Version(6, 0) -> "net6.0"
                    version >= Version(3, 1) -> "netcoreapp3.1"
                    version >= Version(2, 0) -> "netcoreapp2.0"
                    else -> null
                }
            }
            lowerPart.contains("netstandard") || lowerPart.contains(".net standard") -> {
                val version = extractVersion(lowerPart)
                if (version >= Version(2, 0)) "netstandard2.0" else null
            }
            // Handle modern .NET (5.0+) - RuntimeInformation format is ".NET X.Y.Z"
            lowerPart.contains(".net ") && !lowerPart.contains("standard") && !lowerPart.contains("core") && !lowerPart.contains("framework") -> {
                val version = extractVersion(lowerPart)
                when {
                    version >= Version(6, 0) -> "net6.0"
                    version >= Version(5, 0) -> "net6.0"
                    else -> null
                }
            }
            // Handle netX.Y format (e.g., "net6.0", "net7.0")
            lowerPart.startsWith("net") && !lowerPart.contains("standard") && !lowerPart.contains("core") && !lowerPart.contains("framework") -> {
                val version = extractVersion(lowerPart)
                when {
                    version >= Version(6, 0) -> "net6.0"
                    version >= Version(5, 0) -> "net6.0"
                    else -> null
                }
            }
            else -> null
        }
    }
    
    private fun extractVersion(frameworkString: String): Version {
        val versionRegex = Regex("""\d+(\.\d+)*""")
        val match = versionRegex.find(frameworkString)
        
        return if (match != null) {
            val parts = match.value.split(".")
            when (parts.size) {
                1 -> Version(parts[0].toInt(), 0)
                2 -> Version(parts[0].toInt(), parts[1].toInt())
                else -> Version(parts[0].toInt(), parts[1].toInt(), parts[2].toInt())
            }
        } else {
            Version(0, 0)
        }
    }
    
    /**
     * Build per-format options JSON matching the VS extension's ObjectDumperOptionPage.ToJson() format.
     * The ObjectSerializer.SerializeToFile expects different JSON properties depending on the format.
     * Properties are serialized in alphabetical order with PascalCase names and PascalCase enum string values,
     * matching the C# Newtonsoft.Json StringEnumConverter output.
     */
    fun buildOptionsJson(format: String): String {
        return when (format) {
            "cs" -> buildCSharpOptionsJson()
            "vb" -> buildVisualBasicOptionsJson()
            "json" -> buildJsonOptionsJson()
            "xml" -> buildXmlOptionsJson()
            "yaml" -> buildYamlOptionsJson()
            else -> "{}"
        }
    }

    private fun buildCSharpOptionsJson(): String {
        val s = settings
        val getFieldsFlags = s.computeBindingFlags(s.csharpGetFieldsBindingFlagsModifiers, s.csharpGetFieldsBindingFlagsInstanceOrStatic)
        val getPropertiesFlags = s.computeBindingFlags(s.csharpGetPropertiesBindingFlagsModifiers, s.csharpGetPropertiesBindingFlagsInstanceOrStatic)
        val sortDirection = s.csharpSortDirection?.jsonValue?.let { "\"$it\"" } ?: "null"
        val indentString = escapeJsonString(s.csharpIndentString)
        val integralFormat = escapeJsonString(s.csharpIntegralNumericFormat)

        return buildString {
            append("{")
            append("\"DateKind\":\"${s.csharpDateKind.jsonValue}\"")
            append(",\"DateTimeInstantiation\":\"${s.csharpDateTimeInstantiation.jsonValue}\"")
            append(",\"GenerateVariableInitializer\":${s.csharpGenerateVariableInitializer}")
            append(",\"GetFieldsBindingFlags\":${getFieldsFlags ?: "null"}")
            append(",\"GetPropertiesBindingFlags\":${getPropertiesFlags ?: "null"}")
            append(",\"IgnoreDefaultValues\":${s.csharpIgnoreDefaultValues}")
            append(",\"IgnoreNullValues\":${s.csharpIgnoreNullValues}")
            append(",\"IgnoreReadonlyProperties\":${s.csharpIgnoreReadonlyProperties}")
            append(",\"IndentString\":\"$indentString\"")
            append(",\"IntegralNumericFormat\":\"$integralFormat\"")
            append(",\"MaxCollectionSize\":${s.csharpMaxCollectionSize}")
            append(",\"MaxDepth\":${s.commonMaxDepth}")
            append(",\"PrimitiveCollectionLayout\":\"${s.csharpPrimitiveCollectionLayout.jsonValue}\"")
            append(",\"SortDirection\":$sortDirection")
            append(",\"UseNamedArgumentsInConstructors\":${s.csharpUseNamedArgumentsInConstructors}")
            append(",\"UsePredefinedConstants\":${s.csharpUsePredefinedConstants}")
            append(",\"UsePredefinedMethods\":${s.csharpUsePredefinedMethods}")
            append(",\"UseFullTypeName\":${s.csharpUseFullTypeName}")
            append("}")
        }
    }

    private fun buildVisualBasicOptionsJson(): String {
        val s = settings
        val getFieldsFlags = s.computeBindingFlags(s.vbGetFieldsBindingFlagsModifiers, s.vbGetFieldsBindingFlagsInstanceOrStatic)
        val getPropertiesFlags = s.computeBindingFlags(s.vbGetPropertiesBindingFlagsModifiers, s.vbGetPropertiesBindingFlagsInstanceOrStatic)
        val sortDirection = s.vbSortDirection?.jsonValue?.let { "\"$it\"" } ?: "null"
        val indentString = escapeJsonString(s.vbIndentString)
        val integralFormat = escapeJsonString(s.vbIntegralNumericFormat)

        return buildString {
            append("{")
            append("\"DateKind\":\"${s.vbDateKind.jsonValue}\"")
            append(",\"DateTimeInstantiation\":\"${s.vbDateTimeInstantiation.jsonValue}\"")
            append(",\"GenerateVariableInitializer\":${s.vbGenerateVariableInitializer}")
            append(",\"GetFieldsBindingFlags\":${getFieldsFlags ?: "null"}")
            append(",\"GetPropertiesBindingFlags\":${getPropertiesFlags ?: "null"}")
            append(",\"IgnoreDefaultValues\":${s.vbIgnoreDefaultValues}")
            append(",\"IgnoreNullValues\":${s.vbIgnoreNullValues}")
            append(",\"IgnoreReadonlyProperties\":${s.vbIgnoreReadonlyProperties}")
            append(",\"IndentString\":\"$indentString\"")
            append(",\"IntegralNumericFormat\":\"$integralFormat\"")
            append(",\"MaxCollectionSize\":${s.vbMaxCollectionSize}")
            append(",\"MaxDepth\":${s.commonMaxDepth}")
            append(",\"PrimitiveCollectionLayout\":\"${s.vbPrimitiveCollectionLayout.jsonValue}\"")
            append(",\"SortDirection\":$sortDirection")
            append(",\"UseNamedArgumentsInConstructors\":${s.vbUseNamedArgumentsInConstructors}")
            append(",\"UsePredefinedConstants\":${s.vbUsePredefinedConstants}")
            append(",\"UsePredefinedMethods\":${s.vbUsePredefinedMethods}")
            append(",\"UseFullTypeName\":${s.vbUseFullTypeName}")
            append("}")
        }
    }

    private fun buildJsonOptionsJson(): String {
        val s = settings
        return buildString {
            append("{")
            append("\"DateTimeZoneHandling\":\"${s.jsonDateTimeZoneHandling.jsonValue}\"")
            append(",\"IgnoreDefaultValues\":${s.jsonIgnoreDefaultValues}")
            append(",\"IgnoreNullValues\":${s.jsonIgnoreNullValues}")
            append(",\"MaxDepth\":${s.commonMaxDepth}")
            append(",\"NamingStrategy\":\"${s.jsonNamingStrategy.jsonValue}\"")
            append(",\"SerializeEnumAsString\":${s.jsonSerializeEnumAsString}")
            append(",\"TypeNameHandling\":\"${s.jsonTypeNameHandling.jsonValue}\"")
            append("}")
        }
    }

    private fun buildXmlOptionsJson(): String {
        val s = settings
        return buildString {
            append("{")
            append("\"DateTimeZoneHandling\":\"${s.xmlDateTimeZoneHandling.jsonValue}\"")
            append(",\"IgnoreDefaultValues\":${s.xmlIgnoreDefaultValues}")
            append(",\"IgnoreNullValues\":${s.xmlIgnoreNullValues}")
            append(",\"MaxDepth\":${s.commonMaxDepth}")
            append(",\"NamingStrategy\":\"${s.xmlNamingStrategy.jsonValue}\"")
            append(",\"SerializeEnumAsString\":${s.xmlSerializeEnumAsString}")
            append(",\"UseFullTypeName\":${s.xmlUseFullTypeName}")
            append("}")
        }
    }

    private fun buildYamlOptionsJson(): String {
        val s = settings
        return buildString {
            append("{")
            append("\"MaxDepth\":${s.commonMaxDepth}")
            append(",\"NamingConvention\":\"${s.yamlNamingConvention.jsonValue}\"")
            append("}")
        }
    }

    private fun escapeJsonString(value: String): String {
        return value
            .replace("\\", "\\\\")
            .replace("\"", "\\\"")
            .replace("\n", "\\n")
            .replace("\r", "\\r")
            .replace("\t", "\\t")
    }
}

data class Version(val major: Int, val minor: Int, val build: Int = 0) : Comparable<Version> {
    override fun compareTo(other: Version): Int {
        if (major != other.major) return major.compareTo(other.major)
        if (minor != other.minor) return minor.compareTo(other.minor)
        return build.compareTo(other.build)
    }
}
