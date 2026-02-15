package com.yellowflavor.objectdumper.actions

import com.intellij.notification.NotificationGroupManager
import com.intellij.notification.NotificationType
import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.actionSystem.CommonDataKeys
import com.intellij.openapi.actionSystem.PlatformDataKeys
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.project.Project
import com.yellowflavor.objectdumper.debugger.DebuggerInteractionService
import com.yellowflavor.objectdumper.debugger.SerializationExecutor
import com.yellowflavor.objectdumper.debugger.SerializationResult
import com.yellowflavor.objectdumper.output.OutputHandler
import com.yellowflavor.objectdumper.settings.ObjectDumperSettings

abstract class DumpAsAction(
    private val format: String,
    private val fileExtension: String,
    private val formatName: String
) : AnAction() {
    
    protected val settings = ObjectDumperSettings.getInstance()
    
    override fun update(e: AnActionEvent) {
        val project = e.project
        if (project == null) {
            e.presentation.isEnabledAndVisible = false
            return
        }
        
        val debuggerService = DebuggerInteractionService(project)
        val isDebugging = debuggerService.isDebugging()
        
        e.presentation.isEnabledAndVisible = isDebugging && isFormatEnabled()
    }
    
    abstract fun isFormatEnabled(): Boolean
    
    override fun actionPerformed(e: AnActionEvent) {
        val project = e.project ?: return
        
        // Get variable name on EDT (required for editor/tree access)
        val variableName = getVariableName(e)
        if (variableName == null) {
            showNotification(project, "No variable selected. Please select a variable in the debug window or editor.", NotificationType.WARNING)
            return
        }
        
        // Now execute dump operation on background thread
        ApplicationManager.getApplication().executeOnPooledThread {
            try {
                val debuggerService = DebuggerInteractionService(project)
                
                if (!debuggerService.isDebugging()) {
                    showNotification(project, "Not in debug mode", NotificationType.WARNING)
                    return@executeOnPooledThread
                }
                
                val debugSession = debuggerService.getCurrentDebugSession()
                if (debugSession == null) {
                    showNotification(project, "No active debug session", NotificationType.ERROR)
                    return@executeOnPooledThread
                }
                
                showNotification(project, "Dumping '$variableName' as $formatName...", NotificationType.INFORMATION)
                
                val result = performDump(project, debugSession, variableName)
            
                when (result) {
                    is SerializationResult.Success -> {
                        val outputHandler = OutputHandler(project)
                        outputHandler.handleOutput(result.content, formatName, fileExtension)
                        showNotification(project, "Object dumped successfully as $formatName", NotificationType.INFORMATION)
                    }
                    is SerializationResult.Error -> {
                        showNotification(project, "Failed to dump object: ${result.message}", NotificationType.ERROR)
                    }
                }
                
            } catch (ex: Exception) {
                showNotification(project, "Error: ${ex.message ?: "Unknown error"}", NotificationType.ERROR)
                ex.printStackTrace()
            }
        }
    }
    
    private fun getVariableName(e: AnActionEvent): String? {
        // First, try to get from debug tree (variables/watches window)
        val tree = e.getData(PlatformDataKeys.CONTEXT_COMPONENT)
        if (tree != null) {
            val treeName = tree.javaClass.simpleName
            // Check if it's a debug tree component
            if (treeName.contains("Tree") || treeName.contains("Debug")) {
                // Try to get selection path
                try {
                    val treeClass = tree.javaClass
                    val getSelectionPathMethod = treeClass.getMethod("getSelectionPath")
                    val selectionPath = getSelectionPathMethod.invoke(tree)
                    
                    if (selectionPath != null) {
                        val pathClass = selectionPath.javaClass
                        val getLastPathComponentMethod = pathClass.getMethod("getLastPathComponent")
                        val node = getLastPathComponentMethod.invoke(selectionPath)
                        
                        if (node != null) {
                            // Try to get the name from the node
                            val nodeClass = node.javaClass
                            try {
                                val getNameMethod = nodeClass.getMethod("getName")
                                val name = getNameMethod.invoke(node)
                                if (name != null && name.toString().isNotBlank()) {
                                    return name.toString()
                                }
                            } catch (e: Exception) {
                                // Try toString as fallback
                                val nodeStr = node.toString()
                                if (nodeStr.isNotBlank() && !nodeStr.startsWith("[")) {
                                    // Extract variable name from node string if possible
                                    val match = Regex("^([a-zA-Z_][a-zA-Z0-9_]*)").find(nodeStr)
                                    if (match != null) {
                                        return match.groupValues[1]
                                    }
                                }
                            }
                        }
                    }
                } catch (e: Exception) {
                    // Debug tree access failed, try editor
                }
            }
        }
        
        // Second, try to get from editor selection (for inline debugging)
        val editor = e.getData(CommonDataKeys.EDITOR)
        if (editor != null) {
            val selectionModel = editor.selectionModel
            val selectedText = selectionModel.selectedText
            if (!selectedText.isNullOrBlank()) {
                return selectedText.trim()
            }
            
            // Try to get word at caret
            val document = editor.document
            val caretModel = editor.caretModel
            val offset = caretModel.offset
            val text = document.charsSequence
            
            // Find word boundaries
            var start = offset
            var end = offset
            
            while (start > 0 && (text[start - 1].isLetterOrDigit() || text[start - 1] == '_' || text[start - 1] == '.')) {
                start--
            }
            while (end < text.length && (text[end].isLetterOrDigit() || text[end] == '_' || text[end] == '.')) {
                end++
            }
            
            if (end > start) {
                val word = text.substring(start, end).toString().trim()
                if (word.isNotEmpty()) {
                    return word
                }
            }
        }
        
        // If no editor context, return null (will need to handle debug tree selection separately)
        return null
    }
    
    private fun performDump(
        project: Project,
        debugSession: com.intellij.xdebugger.XDebugSession,
        variableName: String
    ): SerializationResult {
        try {
            // Create serialization executor
            val executor = SerializationExecutor(project, debugSession)
            
            // Perform serialization
            return executor.serialize(variableName, format)
            
        } catch (e: Exception) {
            return SerializationResult.Error("Exception during serialization: ${e.message ?: "Unknown error"}")
        }
    }
    
    protected fun showNotification(project: Project, content: String, type: NotificationType) {
        NotificationGroupManager.getInstance()
            .getNotificationGroup("ObjectDumper.Notifications")
            .createNotification(content, type)
            .notify(project)
    }
}
