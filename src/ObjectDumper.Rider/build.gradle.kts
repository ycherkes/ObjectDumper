import org.jetbrains.changelog.Changelog
import org.jetbrains.intellij.platform.gradle.TestFrameworkType

plugins {
    id("java")
    id("org.jetbrains.kotlin.jvm") version "1.9.21"
    id("org.jetbrains.intellij.platform") version "2.0.0"
    id("org.jetbrains.changelog") version "2.2.0"
}

group = "com.yellowflavor"
version = "0.0.2"

repositories {
    mavenCentral()
    
    intellijPlatform {
        defaultRepositories()
    }
}

dependencies {
    intellijPlatform {
        rider("2023.3")
        
        instrumentationTools()
        testFramework(TestFrameworkType.Platform)
    }
}

intellijPlatform {
    projectName = "ObjectDumper"
    
    pluginConfiguration {
        id = "com.yellowflavor.objectdumper"
        name = "Object Dumper"
        version = project.version.toString()
        description = """
            Reflection-based Rider extension for exporting in-memory objects during debugging to 
            C# Object Initialization Code, JSON, Visual Basic Object Initialization Code, XML, and YAML string.
            
            "Dump as" commands are available via context menu in the Debug tool window.
            The result will be printed to a new editor tab, Debug Console, or copied to the clipboard.
        """.trimIndent()
        
        changeNotes = """
            Initial release with support for:
            - Dump as C#
            - Dump as JSON
            - Dump as XML
            - Dump as VB
            - Dump as YAML
        """.trimIndent()
        
        ideaVersion {
            sinceBuild = "233"
            untilBuild = provider { null }
        }
        
        vendor {
            name = "Reffinert"
            email = "ycherkes@outlook.com"
            url = "https://github.com/ycherkes/ObjectDumper"
        }
    }
    
    publishing {
        token = providers.environmentVariable("PUBLISH_TOKEN")
    }
}

// Task to copy serialization libraries before building the plugin
tasks.register<Copy>("copySerializationLibs") {
    description = "Copies YellowFlavor.Serialization.dll files from the main ObjectDumper project"
    group = "build"
    
    val objectDumperRoot = project.projectDir.parentFile
    val sourceLibsPath = file("$objectDumperRoot/src/ObjectDumper/InjectableLibs")
    val targetLibsPath = file("src/main/resources/InjectableLibs")
    
    from(sourceLibsPath) {
        include("**/*.dll")
    }
    into(targetLibsPath)
    
    // Only copy if source exists
    onlyIf {
        sourceLibsPath.exists()
    }
    
    doFirst {
        if (!sourceLibsPath.exists()) {
            logger.warn("Source libraries not found at: $sourceLibsPath")
            logger.warn("Please build the main ObjectDumper project first")
            logger.warn("The plugin will be built without serialization libraries")
        }
    }
}

// Ensure serialization libs are copied before preparing the plugin
tasks.named("prepareSandbox") {
    dependsOn("copySerializationLibs")
}

tasks.named("buildPlugin") {
    dependsOn("copySerializationLibs")
}

kotlin {
    jvmToolchain(17)
}

tasks {
    prepareSandbox {
        from("src/main/resources/InjectableLibs") {
            into("${intellijPlatform.projectName.get()}/InjectableLibs")
        }
    }
    
    buildPlugin {
        duplicatesStrategy = DuplicatesStrategy.EXCLUDE
        from("src/main/resources/InjectableLibs") {
            into("InjectableLibs")
        }
    }
}
