package com.yellowflavor.objectdumper.debugger

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.diagnostic.Logger
import com.intellij.xdebugger.XDebugSession
import com.intellij.xdebugger.evaluation.XDebuggerEvaluator
import com.intellij.xdebugger.frame.XValue
import com.intellij.xdebugger.frame.XValueNode
import com.intellij.xdebugger.frame.XValuePlace
import com.intellij.xdebugger.frame.presentation.XValuePresentation
import java.util.concurrent.CompletableFuture
import java.util.concurrent.TimeUnit
import javax.swing.Icon
import kotlin.coroutines.Continuation
import kotlin.coroutines.CoroutineContext
import kotlin.coroutines.EmptyCoroutineContext

class ExpressionEvaluator(private val debugSession: XDebugSession) {
    
    companion object {
        private val LOG = Logger.getInstance(ExpressionEvaluator::class.java)
        
        // Error patterns that Rider sends through the 'evaluated' callback instead of 'errorOccurred'
        private val ERROR_PATTERNS = listOf(
            "does not exist in the current context",
            "Implicit evaluation is disabled",
            "error CS",
            "Cannot evaluate expression",
            "is inaccessible due to its protection level",
            "is not a valid expression",
            "Evaluation timed out"
        )
        
        /**
         * Checks if a value is the COROUTINE_SUSPENDED sentinel.
         * Uses class name + toString check instead of identity (===) to avoid classloader issues.
         */
        private fun isCoroutineSuspended(value: Any?): Boolean {
            if (value == null) return false
            return value.javaClass.name == "kotlin.coroutines.intrinsics.CoroutineSingletons" &&
                   value.toString() == "COROUTINE_SUSPENDED"
        }
    }
    
    fun evaluateExpression(expression: String, timeoutSeconds: Int = 30): CompletableFuture<EvaluationResult> {
        val future = CompletableFuture<EvaluationResult>()
        val timeoutMs = (timeoutSeconds * 1000).toLong()
        
        LOG.info("ObjectDumper: evaluating expression (timeout=${timeoutSeconds}s): ${expression.take(200)}...")
        
        ApplicationManager.getApplication().invokeLater {
            val evaluator = debugSession.currentStackFrame?.evaluator
            
            if (evaluator == null) {
                LOG.warn("ObjectDumper: no evaluator available from current stack frame")
                future.complete(EvaluationResult(false, "No evaluator available", null))
                return@invokeLater
            }
            
            // Try DotNetEvaluator with forceEvaluate=true (suspend function approach for Rider 2025.3+)
            try {
                val dotNetEvaluatorClass = Class.forName("com.jetbrains.rider.debugger.evaluation.DotNetEvaluator")
                if (dotNetEvaluatorClass.isInstance(evaluator)) {
                    LOG.info("ObjectDumper: evaluator is DotNetEvaluator (${evaluator.javaClass.name})")
                    
                    val forceEvalSuccess = tryForceEvaluate(evaluator, expression, future, timeoutMs)
                    if (forceEvalSuccess) {
                        return@invokeLater
                    }
                    
                    LOG.warn("ObjectDumper: forceEvaluate not available, falling back to callback-based evaluate")
                } else {
                    LOG.info("ObjectDumper: evaluator is ${evaluator.javaClass.name}, not DotNetEvaluator")
                }
            } catch (e: Exception) {
                LOG.warn("ObjectDumper: failed to use DotNetEvaluator: ${e.javaClass.simpleName}: ${e.message}", e)
            }
            
            // Fallback: use base XDebuggerEvaluator.evaluate with callback
            evaluateWithCallback(evaluator, expression, future, timeoutMs)
        }
        
        return future
    }
    
    /**
     * Attempts to call DotNetEvaluator.evaluate(expression, forceEvaluate=true, ...) which is a
     * Kotlin suspend function in Rider 2025.3+. Uses reflection with a manual Continuation to bridge
     * the suspend function result to our CompletableFuture.
     *
     * The method signature in Rider 2025.3 is:
     *   suspend fun evaluate(expression: String, forceEvaluate: Boolean, extraInfo: ExtraInfo?,
     *       nameAliases: List<NameAlias>?, preCalculatedPinToTopState: PinToTopState?,
     *       createDelegatedValue: Boolean): IDotNetValue
     *
     * Kotlin compiles this to:
     *   evaluate$default(DotNetEvaluator, String, boolean, ExtraInfo, List, PinToTopState, boolean,
     *       Continuation, int, Object) -> Object
     *
     * Returns true if the method was found and invoked, false otherwise.
     */
    private fun tryForceEvaluate(
        evaluator: XDebuggerEvaluator,
        expression: String,
        future: CompletableFuture<EvaluationResult>,
        timeoutMs: Long
    ): Boolean {
        // Log all 'evaluate' methods for diagnostics
        val allEvaluateMethods = evaluator.javaClass.methods.filter { 
            it.name == "evaluate" || it.name.startsWith("evaluate\$") 
        }
        for (m in allEvaluateMethods) {
            LOG.info("ObjectDumper: found method: ${m.name}(${m.parameterTypes.map { it.simpleName }.joinToString(", ")}) -> ${m.returnType.simpleName}")
        }
        
        // Look for evaluate$default with Continuation parameter (suspend function)
        val defaultMethod = allEvaluateMethods.find { method ->
            method.name.startsWith("evaluate\$default") &&
            method.parameterTypes.any { p -> Continuation::class.java.isAssignableFrom(p) }
        }
        
        if (defaultMethod != null) {
            LOG.info("ObjectDumper: found suspend evaluate\$default: ${defaultMethod.parameterTypes.map { it.simpleName }.joinToString(", ")}")
            invokeSuspendEvaluateDefault(evaluator, defaultMethod, expression, future, timeoutMs)
            return true
        }
        
        // Look for the direct evaluate method with Continuation (non-$default version)
        val suspendMethod = allEvaluateMethods.find { method ->
            method.name == "evaluate" &&
            method.parameterTypes.size >= 3 &&
            method.parameterTypes[0] == String::class.java &&
            method.parameterTypes.any { p -> Continuation::class.java.isAssignableFrom(p) }
        }
        
        if (suspendMethod != null) {
            LOG.info("ObjectDumper: found suspend evaluate: ${suspendMethod.parameterTypes.map { it.simpleName }.joinToString(", ")}")
            invokeSuspendEvaluateDirect(evaluator, suspendMethod, expression, future, timeoutMs)
            return true
        }
        
        return false
    }
    
    /**
     * Invokes evaluate$default(DotNetEvaluator, String, boolean, ExtraInfo, List, PinToTopState,
     *     boolean, Continuation, int, Object) via reflection.
     *
     * Parameter positions:
     *   [0] DotNetEvaluator (this)  — static method, receiver is first param
     *   [1] String (expression)
     *   [2] boolean (forceEvaluate)
     *   [3] ExtraInfo? (extraInfo) — default
     *   [4] List? (nameAliases) — default
     *   [5] PinToTopState? (pinToTopState) — default
     *   [6] boolean (createDelegatedValue) — default
     *   [7] Continuation (continuation)
     *   [8] int (default bitmask)
     *   [9] Object (unused marker)
     */
    private fun invokeSuspendEvaluateDefault(
        evaluator: XDebuggerEvaluator,
        method: java.lang.reflect.Method,
        expression: String,
        future: CompletableFuture<EvaluationResult>,
        timeoutMs: Long
    ) {
        val paramTypes = method.parameterTypes
        LOG.info("ObjectDumper: evaluate\$default has ${paramTypes.size} params")
        
        // Find the Continuation parameter index
        val continuationIndex = paramTypes.indexOfFirst { Continuation::class.java.isAssignableFrom(it) }
        if (continuationIndex < 0) {
            LOG.error("ObjectDumper: Continuation parameter not found in evaluate\$default")
            future.complete(EvaluationResult(false, "Internal error: Continuation parameter not found", null))
            return
        }
        
        // Create a Continuation that bridges to our CompletableFuture
        val continuation = createContinuation(future, timeoutMs)
        
        // Build arguments array
        val args = arrayOfNulls<Any?>(paramTypes.size)
        
        // Fill in known positions based on the expected signature
        // Position 0: receiver (DotNetEvaluator instance) — $default is static
        args[0] = evaluator
        // Position 1: expression (String)
        args[1] = expression
        // Position 2: forceEvaluate (boolean) — THIS IS THE KEY
        args[2] = true
        // Positions 3-6: defaulted parameters (null/false)
        for (i in 3 until continuationIndex) {
            args[i] = when (paramTypes[i]) {
                Boolean::class.javaPrimitiveType, java.lang.Boolean::class.java -> false
                else -> null
            }
        }
        // Continuation
        args[continuationIndex] = continuation
        // Default bitmask: bits for extraInfo(2), nameAliases(3), pinToTopState(4), createDelegatedValue(5)
        // = 0b00111100 = 60 = 0x3C
        if (continuationIndex + 1 < paramTypes.size && paramTypes[continuationIndex + 1] == Int::class.javaPrimitiveType) {
            args[continuationIndex + 1] = 60  // bitmask: skip defaults for params 2,3,4,5
        }
        // Last param (Object marker) stays null
        
        LOG.info("ObjectDumper: invoking evaluate\$default with args: [evaluator, '${expression.take(80)}...', forceEvaluate=true, null, null, null, false, continuation, bitmask=60, null]")
        
        try {
            method.isAccessible = true
            val result = method.invoke(null, *args)  // static method
            handleSuspendResult(result, future, timeoutMs)
        } catch (e: java.lang.reflect.InvocationTargetException) {
            val cause = e.cause ?: e
            LOG.error("ObjectDumper: evaluate\$default invocation failed: ${cause.javaClass.simpleName}: ${cause.message}", cause)
            if (!future.isDone) {
                future.complete(EvaluationResult(false, "Force evaluate failed: ${cause.message}", null))
            }
        } catch (e: Exception) {
            LOG.error("ObjectDumper: evaluate\$default call failed: ${e.javaClass.simpleName}: ${e.message}", e)
            if (!future.isDone) {
                future.complete(EvaluationResult(false, "Force evaluate error: ${e.message}", null))
            }
        }
    }
    
    /**
     * Invokes the direct evaluate(String, boolean, ..., Continuation) method via reflection.
     */
    private fun invokeSuspendEvaluateDirect(
        evaluator: XDebuggerEvaluator,
        method: java.lang.reflect.Method,
        expression: String,
        future: CompletableFuture<EvaluationResult>,
        timeoutMs: Long
    ) {
        val paramTypes = method.parameterTypes
        LOG.info("ObjectDumper: direct evaluate has ${paramTypes.size} params")
        
        val continuationIndex = paramTypes.indexOfFirst { Continuation::class.java.isAssignableFrom(it) }
        if (continuationIndex < 0) {
            LOG.error("ObjectDumper: Continuation parameter not found in evaluate")
            future.complete(EvaluationResult(false, "Internal error: Continuation parameter not found", null))
            return
        }
        
        val continuation = createContinuation(future, timeoutMs)
        
        val args = arrayOfNulls<Any?>(paramTypes.size)
        args[0] = expression      // String expression
        args[1] = true             // boolean forceEvaluate
        // Fill remaining params before Continuation with defaults
        for (i in 2 until continuationIndex) {
            args[i] = when (paramTypes[i]) {
                Boolean::class.javaPrimitiveType, java.lang.Boolean::class.java -> false
                else -> null
            }
        }
        args[continuationIndex] = continuation
        
        LOG.info("ObjectDumper: invoking evaluate with forceEvaluate=true, ${paramTypes.size} params")
        
        try {
            method.isAccessible = true
            val result = method.invoke(evaluator, *args)  // instance method
            handleSuspendResult(result, future, timeoutMs)
        } catch (e: java.lang.reflect.InvocationTargetException) {
            val cause = e.cause ?: e
            LOG.error("ObjectDumper: evaluate invocation failed: ${cause.javaClass.simpleName}: ${cause.message}", cause)
            if (!future.isDone) {
                future.complete(EvaluationResult(false, "Force evaluate failed: ${cause.message}", null))
            }
        } catch (e: Exception) {
            LOG.error("ObjectDumper: evaluate call failed: ${e.javaClass.simpleName}: ${e.message}", e)
            if (!future.isDone) {
                future.complete(EvaluationResult(false, "Force evaluate error: ${e.message}", null))
            }
        }
    }
    
    /**
     * Creates a Continuation<Any?> that extracts the string value from the IDotNetValue result
     * and completes the given CompletableFuture.
     */
    private fun createContinuation(
        future: CompletableFuture<EvaluationResult>,
        timeoutMs: Long
    ): Continuation<Any?> {
        return object : Continuation<Any?> {
            override val context: CoroutineContext = EmptyCoroutineContext
            
            override fun resumeWith(result: Result<Any?>) {
                LOG.info("ObjectDumper: Continuation.resumeWith called, isSuccess=${result.isSuccess}")
                
                if (result.isFailure) {
                    val exception = result.exceptionOrNull()
                    LOG.error("ObjectDumper: suspend evaluate failed with exception", exception)
                    if (!future.isDone) {
                        future.complete(EvaluationResult(false, "Evaluation exception: ${exception?.message}", null))
                    }
                    return
                }
                
                val value = result.getOrNull()
                extractValueFromDotNetValue(value, future, timeoutMs)
            }
        }
    }
    
    /**
     * Handles the return value from a suspend function call via reflection.
     * If the function suspended, the result will be COROUTINE_SUSPENDED and the
     * Continuation will be called later. If it returned immediately, we process the result now.
     */
    private fun handleSuspendResult(
        result: Any?,
        future: CompletableFuture<EvaluationResult>,
        timeoutMs: Long
    ) {
        if (isCoroutineSuspended(result)) {
            LOG.info("ObjectDumper: evaluate suspended, waiting for Continuation callback...")
            // The Continuation we passed will be invoked when the result is ready.
            // Set up a timeout in case the continuation is never called.
            ApplicationManager.getApplication().executeOnPooledThread {
                Thread.sleep(timeoutMs)
                if (!future.isDone) {
                    LOG.warn("ObjectDumper: force evaluate timeout after ${timeoutMs}ms")
                    future.complete(EvaluationResult(false, "Force evaluate timed out after ${timeoutMs}ms", null))
                }
            }
            return
        }
        
        // The function returned immediately (didn't suspend)
        LOG.info("ObjectDumper: evaluate returned immediately (type: ${result?.javaClass?.simpleName})")
        extractValueFromDotNetValue(result, future, timeoutMs)
    }
    
    /**
     * Extracts the display string from an IDotNetValue (or XValue) result.
     * IDotNetValue in Rider extends XNamedValue which extends XValue, so we can use
     * computePresentation to get the display text, or try reflection to get the value directly.
     */
    private fun extractValueFromDotNetValue(
        value: Any?,
        future: CompletableFuture<EvaluationResult>,
        timeoutMs: Long
    ) {
        if (value == null) {
            LOG.info("ObjectDumper: evaluate returned null")
            if (!future.isDone) {
                future.complete(EvaluationResult(true, "", null))
            }
            return
        }
        
        LOG.info("ObjectDumper: extracting value from ${value.javaClass.name}")
        
        // Log available methods on the result for diagnostics
        logAvailableMethods(value)
        
        // Strategy 1: If it's an XValue, use computePresentation (our existing approach)
        if (value is XValue) {
            LOG.info("ObjectDumper: result is XValue, using computePresentation")
            extractFromXValue(value, future, timeoutMs)
            return
        }
        
        // Strategy 2: Try common method names via reflection
        val stringValue = tryExtractStringViaReflection(value)
        if (stringValue != null) {
            LOG.info("ObjectDumper: extracted value via reflection: '${stringValue.take(200)}'")
            val isError = isErrorValue(stringValue)
            if (isError) {
                LOG.warn("ObjectDumper: detected error pattern in value: '${stringValue.take(200)}'")
            }
            if (!future.isDone) {
                future.complete(EvaluationResult(!isError, stringValue, null))
            }
            return
        }
        
        // Strategy 3: Use toString as last resort
        val toStringValue = value.toString()
        LOG.info("ObjectDumper: using toString: '${toStringValue.take(200)}'")
        val isError = isErrorValue(toStringValue)
        if (!future.isDone) {
            future.complete(EvaluationResult(!isError, toStringValue, null))
        }
    }
    
    /**
     * Tries to extract a string representation from an object via known method names.
     */
    private fun tryExtractStringViaReflection(value: Any): String? {
        // Try methods commonly found on IDotNetValue / debugger value objects
        val methodNames = listOf(
            "getPresentation", "getValue", "getValueString", "getValuePresentation",
            "getRawValue", "getDisplayText", "getText", "getResult"
        )
        
        for (methodName in methodNames) {
            try {
                val method = value.javaClass.getMethod(methodName)
                val result = method.invoke(value)
                if (result is String && result.isNotBlank()) {
                    LOG.info("ObjectDumper: extracted value via $methodName()")
                    return result
                }
            } catch (_: NoSuchMethodException) {
                // Try next
            } catch (e: Exception) {
                LOG.info("ObjectDumper: $methodName() threw: ${e.javaClass.simpleName}: ${e.message}")
            }
        }
        
        return null
    }
    
    /**
     * Logs available methods on an object for diagnostics (first invocation only).
     */
    private fun logAvailableMethods(value: Any) {
        try {
            val methods = value.javaClass.methods
                .filter { it.parameterCount == 0 && it.returnType != Void.TYPE }
                .filter { !it.name.startsWith("wait") && !it.name.startsWith("notify") && it.name != "hashCode" && it.name != "getClass" }
                .sortedBy { it.name }
            
            LOG.info("ObjectDumper: IDotNetValue methods (${methods.size}):")
            for (m in methods.take(30)) {
                LOG.info("ObjectDumper:   ${m.name}() -> ${m.returnType.simpleName}")
            }
        } catch (e: Exception) {
            LOG.info("ObjectDumper: could not enumerate methods: ${e.message}")
        }
    }
    
    /**
     * Extracts string value from an XValue via computePresentation.
     */
    private fun extractFromXValue(
        xValue: XValue,
        future: CompletableFuture<EvaluationResult>,
        timeoutMs: Long
    ) {
        val valueFuture = CompletableFuture<String>()
        
        xValue.computePresentation(object : XValueNode {
            override fun setPresentation(icon: Icon?, presentation: XValuePresentation, hasChildren: Boolean) {
                val valueText = StringBuilder()
                val typeText = presentation.type
                
                presentation.renderValue(object : XValuePresentation.XValueTextRenderer {
                    override fun renderValue(value: String) { valueText.append(value) }
                    override fun renderValue(value: String, key: com.intellij.openapi.editor.colors.TextAttributesKey) { valueText.append(value) }
                    override fun renderStringValue(value: String) { valueText.append(value) }
                    override fun renderStringValue(value: String, additionalSpecialCharsToHighlight: String?, maxLength: Int) { valueText.append(value) }
                    override fun renderNumericValue(value: String) { valueText.append(value) }
                    override fun renderKeywordValue(value: String) { valueText.append(value) }
                    override fun renderComment(comment: String) { valueText.append(comment) }
                    override fun renderSpecialSymbol(symbol: String) { valueText.append(symbol) }
                    override fun renderError(error: String) { valueText.append(error) }
                })
                
                LOG.info("ObjectDumper: presentation type='$typeText', value='${valueText.toString().take(200)}'")
                valueFuture.complete(valueText.toString())
            }
            
            override fun setPresentation(icon: Icon?, type: String?, value: String, hasChildren: Boolean) {
                LOG.info("ObjectDumper: simple presentation type='$type', value='${value.take(200)}'")
                valueFuture.complete(value)
            }
            
            override fun setFullValueEvaluator(fullValueEvaluator: com.intellij.xdebugger.frame.XFullValueEvaluator) {}
        }, XValuePlace.TREE)
        
        valueFuture.whenComplete { finalValue, error ->
            if (error != null) {
                LOG.error("ObjectDumper: error getting presentation", error)
                if (!future.isDone) {
                    future.complete(EvaluationResult(false, "Error getting presentation: ${error.message}", null))
                }
            } else {
                val isError = isErrorValue(finalValue)
                if (isError) {
                    LOG.warn("ObjectDumper: detected error pattern in evaluated value: '${finalValue.take(200)}'")
                }
                if (!future.isDone) {
                    future.complete(EvaluationResult(!isError, finalValue, xValue))
                }
            }
        }
        
        // Timeout for the presentation future
        ApplicationManager.getApplication().executeOnPooledThread {
            Thread.sleep(timeoutMs)
            if (!valueFuture.isDone) {
                LOG.warn("ObjectDumper: presentation timeout after ${timeoutMs}ms")
                valueFuture.complete("")
            }
        }
    }
    
    /**
     * Evaluates using the standard XDebuggerEvaluator callback approach (fallback).
     */
    private fun evaluateWithCallback(
        evaluator: XDebuggerEvaluator,
        expression: String,
        future: CompletableFuture<EvaluationResult>,
        timeoutMs: Long
    ) {
        val callback = object : XDebuggerEvaluator.XEvaluationCallback {
            override fun evaluated(result: XValue) {
                LOG.info("ObjectDumper: callback evaluated (XValue type: ${result.javaClass.simpleName})")
                extractFromXValue(result, future, timeoutMs)
            }
            
            override fun errorOccurred(errorMessage: String) {
                LOG.info("ObjectDumper: callback error: $errorMessage")
                future.complete(EvaluationResult(false, errorMessage, null))
            }
        }
        
        evaluator.evaluate(expression, callback, debugSession.currentStackFrame?.sourcePosition)
    }
    
    fun evaluateExpressionSync(expression: String, timeoutSeconds: Int = 30): EvaluationResult {
        return try {
            evaluateExpression(expression, timeoutSeconds).get(timeoutSeconds.toLong(), TimeUnit.SECONDS)
        } catch (e: Exception) {
            LOG.error("ObjectDumper: evaluateExpressionSync exception", e)
            EvaluationResult(false, "Evaluation timeout or error: ${e.message}", null)
        }
    }
    
    private fun isErrorValue(value: String): Boolean {
        if (value.isBlank()) return false
        return ERROR_PATTERNS.any { pattern -> value.contains(pattern, ignoreCase = true) }
    }
}

data class EvaluationResult(
    val success: Boolean,
    val value: String,
    val xValue: XValue?
)
