package com.yellowflavor.objectdumper.output

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.fileEditor.FileEditorManager
import com.intellij.openapi.ide.CopyPasteManager
import com.intellij.openapi.project.Project
import com.intellij.testFramework.LightVirtualFile
import com.intellij.xdebugger.XDebuggerManager
import com.yellowflavor.objectdumper.settings.DumpDestination
import com.yellowflavor.objectdumper.settings.ObjectDumperSettings
import java.awt.datatransfer.StringSelection
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter

class OutputHandler(private val project: Project) {
    
    private val settings = ObjectDumperSettings.getInstance()
    
    fun handleOutput(content: String, formatName: String, fileExtension: String) {
        when (settings.dumpDestination) {
            DumpDestination.NEW_TAB -> outputToNewTab(content, formatName, fileExtension)
            DumpDestination.CLIPBOARD -> outputToClipboard(content)
            DumpDestination.DEBUG_CONSOLE -> outputToDebugConsole(content)
        }
    }
    
    private fun outputToNewTab(content: String, formatName: String, fileExtension: String) {
        ApplicationManager.getApplication().invokeLater {
            val timestamp = LocalDateTime.now().format(DateTimeFormatter.ofPattern("yyyyMMdd_HHmmss"))
            val fileName = "ObjectDump_${formatName}_$timestamp.$fileExtension"
            
            val virtualFile = LightVirtualFile(fileName, content)
            virtualFile.isWritable = true
            
            FileEditorManager.getInstance(project).openFile(virtualFile, true)
        }
    }
    
    private fun outputToClipboard(content: String) {
        ApplicationManager.getApplication().invokeLater {
            CopyPasteManager.getInstance().setContents(StringSelection(content))
        }
    }
    
    private fun outputToDebugConsole(content: String) {
        ApplicationManager.getApplication().invokeLater {
            val debugSession = XDebuggerManager.getInstance(project).currentSession
            debugSession?.consoleView?.print(content + "\n", com.intellij.execution.ui.ConsoleViewContentType.NORMAL_OUTPUT)
        }
    }
}
