package com.yellowflavor.objectdumper.debugger

import com.intellij.openapi.diagnostic.Logger
import com.intellij.openapi.project.Project
import com.intellij.xdebugger.XDebugSession
import com.yellowflavor.objectdumper.extensions.Base64Extensions.toBase64
import java.io.File
import java.nio.file.Files

class SerializationExecutor(
    private val project: Project,
    private val debugSession: XDebugSession
) {
    
    private val evaluator = ExpressionEvaluator(debugSession)
    private val debuggerService = DebuggerInteractionService(project)
    
    companion object {
        private val LOG = Logger.getInstance(SerializationExecutor::class.java)
    }
    
    fun serialize(variableName: String, format: String): SerializationResult {
        val timeoutSeconds = debuggerService.getTimeoutSeconds()
        
        // Step 1: Inject serializer (load DLL if not already loaded)
        val injectResult = injectSerializer(timeoutSeconds)
        if (injectResult != null) {
            return injectResult
        }
        
        // Step 2: Create temp file for output
        val tempFile = createTempFile()
        
        try {
            // Step 3: Build per-format options JSON (matching VS extension format)
            val optionsJson = debuggerService.buildOptionsJson(format)
            
            // Step 4: Build the settings string: "format;filePath;optionsJson" then Base64-encode the WHOLE thing
            val settingsRaw = "$format;${tempFile.absolutePath};$optionsJson"
            val settingsBase64 = settingsRaw.toBase64()
            
            LOG.info("ObjectDumper: serializing '$variableName' as '$format', settings raw: $settingsRaw")
            
            // Step 5: Build single-expression serialization call (no multi-statement blocks)
            val serializationExpression = """YellowFlavor.Serialization.ObjectSerializer.SerializeToFile($variableName, "$settingsBase64")"""
            
            LOG.info("ObjectDumper: evaluating serialization expression: $serializationExpression")
            
            val result = evaluator.evaluateExpressionSync(
                serializationExpression,
                timeoutSeconds
            )
            
            LOG.info("ObjectDumper: serialization result - success=${result.success}, value='${result.value}'")
            
            // Step 6: Read result from temp file
            if (tempFile.exists() && tempFile.length() > 0) {
                val content = tempFile.readText()
                return SerializationResult.Success(content)
            }
            
            // If evaluation failed, report the error
            if (!result.success) {
                return SerializationResult.Error("Serialization failed: ${result.value}")
            }
            
            return SerializationResult.Error("Serialization produced no output. Evaluation result: ${result.value}")
        } catch (e: Exception) {
            LOG.error("ObjectDumper: exception during serialization", e)
            return SerializationResult.Error("Exception during serialization: ${e.message}")
        } finally {
            // Clean up temp file
            try {
                if (tempFile.exists()) {
                    tempFile.delete()
                }
            } catch (_: Exception) {
                // Ignore cleanup errors
            }
        }
    }
    
    /**
     * Injects the serializer DLL into the debuggee process if not already loaded.
     * Returns null on success, or a SerializationResult.Error on failure.
     */
    private fun injectSerializer(timeoutSeconds: Int): SerializationResult? {
        // Check if serializer is already injected by trying to resolve the type
        val checkExpression = "nameof(YellowFlavor.Serialization.ObjectSerializer.SerializeToFile)"
        val checkResult = evaluator.evaluateExpressionSync(checkExpression, 5)
        
        LOG.info("ObjectDumper: injection check - success=${checkResult.success}, value='${checkResult.value}'")
        
        if (checkResult.success && !checkResult.value.contains("does not exist", ignoreCase = true)) {
            // Already injected
            LOG.info("ObjectDumper: serializer already injected")
            return null
        }
        
        // Determine target framework
        val targetFramework = getTargetFramework()
        LOG.info("ObjectDumper: detected target framework: $targetFramework")
        
        if (targetFramework == null) {
            return SerializationResult.Error("Could not determine target framework. Make sure you're debugging a .NET application.")
        }
        
        // Get DLL path
        val dllPath = debuggerService.getSerializerLibraryPath(targetFramework)
        LOG.info("ObjectDumper: DLL path resolved to: $dllPath")
        
        if (dllPath == null || !File(dllPath).exists()) {
            return SerializationResult.Error("""
                Failed to find serialization library for framework: $targetFramework
                DLL path: $dllPath
                Plugin path: ${debuggerService.getPluginPath()}
            """.trimIndent())
        }
        
        // Copy DLL to temp directory accessible by debuggee process
        val tempDllPath = copyDllToTempDirectory(dllPath)
        if (tempDllPath == null) {
            return SerializationResult.Error("Failed to copy DLL to temp directory from: $dllPath")
        }
        
        LOG.info("ObjectDumper: copied DLL to: $tempDllPath")
        
        // Load the assembly - use forward slashes or escaped backslashes in the path
        // The @"..." verbatim string in C# handles backslashes correctly
        val loadExpression = """System.Reflection.Assembly.LoadFile(@"$tempDllPath")"""
        LOG.info("ObjectDumper: loading assembly with expression: $loadExpression")
        
        val loadResult = evaluator.evaluateExpressionSync(loadExpression, timeoutSeconds)
        LOG.info("ObjectDumper: load result - success=${loadResult.success}, value='${loadResult.value}'")
        
        if (!loadResult.success) {
            return SerializationResult.Error("Failed to load serialization library: ${loadResult.value}\nDLL path: $tempDllPath")
        }
        
        // Verify the assembly was actually loaded by checking nameof again
        val verifyResult = evaluator.evaluateExpressionSync(checkExpression, 5)
        LOG.info("ObjectDumper: post-load verification - success=${verifyResult.success}, value='${verifyResult.value}'")
        
        if (!verifyResult.success || verifyResult.value.contains("does not exist", ignoreCase = true)) {
            return SerializationResult.Error(
                "Assembly loaded but type not accessible. Load result: ${loadResult.value}\n" +
                "Verify result: ${verifyResult.value}\n" +
                "DLL path: $tempDllPath\n" +
                "Framework: $targetFramework"
            )
        }
        
        return null
    }
    
    private fun getTargetFramework(): String? {
        // Use the same expression as the VS extension to get target framework
        val expression = "((System.Runtime.Versioning.TargetFrameworkAttribute)System.Attribute.GetCustomAttribute(System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetExecutingAssembly(), typeof(System.Runtime.Versioning.TargetFrameworkAttribute)))?.FrameworkName + \";\" + ((System.Runtime.Versioning.TargetFrameworkAttribute)System.Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(System.Runtime.Versioning.TargetFrameworkAttribute)))?.FrameworkName"
        val result = evaluator.evaluateExpressionSync(expression, 10)
        
        LOG.info("ObjectDumper: framework detection result - success=${result.success}, value='${result.value}'")
        
        if (result.success && result.value.isNotBlank()) {
            val framework = result.value.trim('"').trim()
            if (framework.isNotBlank()) {
                return framework
            }
        }
        
        // Fallback: try RuntimeInformation
        val fallbackExpression = "System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription"
        val fallbackResult = evaluator.evaluateExpressionSync(fallbackExpression, 10)
        
        LOG.info("ObjectDumper: framework fallback result - success=${fallbackResult.success}, value='${fallbackResult.value}'")
        
        if (fallbackResult.success && fallbackResult.value.isNotBlank()) {
            val framework = fallbackResult.value.trim('"').trim()
            if (framework.isNotBlank()) {
                return framework
            }
        }
        
        return null
    }
    
    private fun copyDllToTempDirectory(sourcePath: String): String? {
        try {
            val sourceFile = File(sourcePath)
            if (!sourceFile.exists()) {
                LOG.warn("ObjectDumper: source DLL does not exist: $sourcePath")
                return null
            }

            // Compute SHA-256 hash of the source DLL to create a unique directory per DLL version
            val hash = computeFileHash(sourceFile)
            LOG.info("ObjectDumper: source DLL hash: $hash")

            // Create directory: <temp>/ObjectDumper/<hash>/
            val tempDir = File(System.getProperty("java.io.tmpdir"))
            val targetDir = File(tempDir, "ObjectDumper${File.separator}$hash")
            val targetFile = File(targetDir, "YellowFlavor.Serialization.dll")

            // Skip copy if the file already exists with the same hash
            if (targetFile.exists() && targetFile.length() == sourceFile.length()) {
                LOG.info("ObjectDumper: DLL already exists at ${targetFile.absolutePath}, skipping copy")
                return targetFile.absolutePath
            }

            // Create directory and copy
            targetDir.mkdirs()
            sourceFile.copyTo(targetFile, overwrite = true)

            LOG.info("ObjectDumper: copied DLL ${sourceFile.length()} bytes to ${targetFile.absolutePath}")

            return targetFile.absolutePath
        } catch (e: Exception) {
            LOG.error("ObjectDumper: failed to copy DLL", e)
            return null
        }
    }

    private fun computeFileHash(file: File): String {
        val digest = java.security.MessageDigest.getInstance("SHA-256")
        file.inputStream().use { input ->
            val buffer = ByteArray(8192)
            var bytesRead: Int
            while (input.read(buffer).also { bytesRead = it } != -1) {
                digest.update(buffer, 0, bytesRead)
            }
        }
        return digest.digest().joinToString("") { "%02x".format(it) }
    }
    
    private fun createTempFile(): File {
        return Files.createTempFile("objectdumper_", ".tmp").toFile()
    }
}

sealed class SerializationResult {
    data class Success(val content: String) : SerializationResult()
    data class Error(val message: String) : SerializationResult()
}
