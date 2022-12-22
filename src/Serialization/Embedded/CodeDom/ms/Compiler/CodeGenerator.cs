// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.IO;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.Common;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.Resources;

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.Compiler
{
    public abstract class CodeGenerator : ICodeGenerator
    {
        private ExposedTabStringIndentedTextWriter _output;
        private CodeGeneratorOptions _options;

        private bool _inNestedBinary;

        protected int Indent
        {
            get => _output.Indent;
            set => _output.Indent = value;
        }

        protected abstract string NullToken { get; }

        protected TextWriter Output => _output;

        protected CodeGeneratorOptions Options => _options;

        void ICodeGenerator.GenerateCodeFromExpression(CodeExpression e, TextWriter w, CodeGeneratorOptions o)
        {
            bool setLocal = false;
            if (_output != null && w != _output.InnerWriter)
            {
                throw new InvalidOperationException(SR.CodeGenOutputWriter);
            }
            if (_output == null)
            {
                setLocal = true;
                _options = o ?? new CodeGeneratorOptions();
                _output = new ExposedTabStringIndentedTextWriter(w, _options.IndentString);
            }

            try
            {
                GenerateExpression(e);
            }
            finally
            {
                if (setLocal)
                {
                    _output = null;
                    _options = null;
                }
            }
        }

        void ICodeGenerator.GenerateCodeFromStatement(CodeStatement e, TextWriter w, CodeGeneratorOptions o)
        {
            bool setLocal = false;
            if (_output != null && w != _output.InnerWriter)
            {
                throw new InvalidOperationException(SR.CodeGenOutputWriter);
            }
            if (_output == null)
            {
                setLocal = true;
                _options = o ?? new CodeGeneratorOptions();
                _output = new ExposedTabStringIndentedTextWriter(w, _options.IndentString);
            }

            try
            {
                GenerateStatement(e);
            }
            finally
            {
                if (setLocal)
                {
                    _output = null;
                    _options = null;
                }
            }
        }

        protected void GenerateExpression(CodeExpression e)
        {
            if (e is CodeStatementExpression)
            {
                GenerateStatementExpression((CodeStatementExpression)e);
            }
            else if (e is CodeSeparatedExpressionCollection)
            {
                GenerateSeparatedExpressionCollection((CodeSeparatedExpressionCollection)e);
            }
            else if (e is CodeArrayCreateExpression)
            {
                GenerateArrayCreateExpression((CodeArrayCreateExpression)e);
            }
            else if (e is CodeArrayDimensionExpression)
            {
                GenerateCodeArrayDimensionExpression((CodeArrayDimensionExpression)e);
            }
            else if (e is CodeFlagsBinaryOperatorExpression)
            {
                GenerateFlagsBinaryOperatorExpression((CodeFlagsBinaryOperatorExpression)e);
            }
            else if (e is CodeBinaryOperatorExpression)
            {
                GenerateBinaryOperatorExpression((CodeBinaryOperatorExpression)e);
            }
            else if (e is CodeCastExpression)
            {
                GenerateCastExpression((CodeCastExpression)e);
            }
            else if (e is CodeFieldReferenceExpression)
            {
                GenerateFieldReferenceExpression((CodeFieldReferenceExpression)e);
            }
            else if (e is CodeVariableReferenceExpression)
            {
                GenerateVariableReferenceExpression((CodeVariableReferenceExpression)e);
            }
            else if (e is CodeSnippetExpression)
            {
                GenerateSnippetExpression((CodeSnippetExpression)e);
            }
            else if (e is CodeMethodInvokeExpression)
            {
                GenerateMethodInvokeExpression((CodeMethodInvokeExpression)e);
            }
            else if (e is CodeMethodReferenceExpression)
            {
                GenerateMethodReferenceExpression((CodeMethodReferenceExpression)e);
            }
            else if (e is CodeObjectCreateAndInitializeExpression)
            {
                GenerateObjectCreateAndInitializeExpression((CodeObjectCreateAndInitializeExpression)e);
            }
            else if (e is CodeNamedArgumentExpression na)
            {
                GenerateNamedArgumentExpression(na);
            }
            else if (e is CodeValueTupleCreateExpression)
            {
                GenerateValueTupleCreateExpression((CodeValueTupleCreateExpression)e);
            }
            else if (e is CodeObjectCreateExpression)
            {
                GenerateObjectCreateExpression((CodeObjectCreateExpression)e);
            }
            else if (e is CodeLambdaExpression)
            {
                GenerateLambdaExpression((CodeLambdaExpression)e);
            }
            else if (e is CodePrimitiveExpression)
            {
                GeneratePrimitiveExpression((CodePrimitiveExpression)e);
            }
            else if (e is CodePropertyReferenceExpression)
            {
                GeneratePropertyReferenceExpression((CodePropertyReferenceExpression)e);
            }
            else if (e is CodeTypeReferenceExpression)
            {
                GenerateTypeReferenceExpression((CodeTypeReferenceExpression)e);
            }
            else if (e is CodeTypeOfExpression)
            {
                GenerateTypeOfExpression((CodeTypeOfExpression)e);
            }
            else if (e is CodeDefaultValueExpression)
            {
                GenerateDefaultValueExpression((CodeDefaultValueExpression)e);
            }
            else if (e is CodeAssignExpression)
            {
                GenerateCodeAssignExpression((CodeAssignExpression)e);
            }
            else if (e is CodeImplicitKeyValuePairCreateExpression)
            {
                GenerateCodeImplicitKeyValuePairCreateExpression((CodeImplicitKeyValuePairCreateExpression)e);
            }
            else
            {
                if (e == null)
                {
                    throw new ArgumentNullException(nameof(e));
                }
                else
                {
                    throw new ArgumentException(SR.Format(SR.InvalidElementType, e.GetType().FullName), nameof(e));
                }
            }
        }



        protected void GenerateStatement(CodeStatement e)
        {
            if (e is CodeCommentStatement)
            {
                GenerateCommentStatement((CodeCommentStatement)e);
            }
            else if (e is CodeConditionStatement)
            {
                GenerateConditionStatement((CodeConditionStatement)e);
            }
            else if (e is CodeAssignStatement)
            {
                GenerateAssignStatement((CodeAssignStatement)e);
            }
            else if (e is CodeExpressionStatement)
            {
                GenerateExpressionStatement((CodeExpressionStatement)e);
            }
            else if (e is CodeSnippetStatement)
            {
                // Don't indent snippet statements, in order to preserve the column
                // information from the original code.  This improves the debugging
                // experience.
                int savedIndent = Indent;
                Indent = 0;

                GenerateSnippetStatement((CodeSnippetStatement)e);

                // Restore the indent
                Indent = savedIndent;
            }
            else if (e is CodeVariableDeclarationStatement)
            {
                GenerateVariableDeclarationStatement((CodeVariableDeclarationStatement)e);
            }
            else
            {
                throw new ArgumentException(SR.Format(SR.InvalidElementType, e.GetType().FullName), nameof(e));
            }
        }

        protected void GenerateStatements(CodeStatementCollection stmts)
        {
            foreach (CodeStatement stmt in stmts)
            {
                ((ICodeGenerator)this).GenerateCodeFromStatement(stmt, _output.InnerWriter, _options);
            }
        }

        protected abstract void OutputType(CodeTypeReference typeRef);

        protected virtual void OutputIdentifier(string ident)
        {
            Output.Write(ident);
        }

        protected virtual void OutputExpressionList(CodeExpressionCollection expressions)
        {
            OutputExpressionList(expressions, newlineBetweenItems: false);
        }

        protected virtual void OutputExpressionList(CodeExpressionCollection expressions, bool newlineBetweenItems,
            bool newLineContinuation = true)
        {
            bool first = true;
            Indent++;
            foreach (CodeExpression current in expressions)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (newlineBetweenItems)
                        ContinueOnNewLine(",", newLineContinuation);
                    else
                        Output.Write(", ");
                }
                ((ICodeGenerator)this).GenerateCodeFromExpression(current, _output.InnerWriter, _options);
            }
            Indent--;
        }

        protected virtual void OutputOperator(CodeBinaryOperatorType op)
        {
            switch (op)
            {
                case CodeBinaryOperatorType.Add:
                    Output.Write('+');
                    break;
                case CodeBinaryOperatorType.Subtract:
                    Output.Write('-');
                    break;
                case CodeBinaryOperatorType.Multiply:
                    Output.Write('*');
                    break;
                case CodeBinaryOperatorType.Divide:
                    Output.Write('/');
                    break;
                case CodeBinaryOperatorType.Modulus:
                    Output.Write('%');
                    break;
                case CodeBinaryOperatorType.Assign:
                    Output.Write('=');
                    break;
                case CodeBinaryOperatorType.IdentityInequality:
                    Output.Write("!=");
                    break;
                case CodeBinaryOperatorType.IdentityEquality:
                    Output.Write("==");
                    break;
                case CodeBinaryOperatorType.ValueEquality:
                    Output.Write("==");
                    break;
                case CodeBinaryOperatorType.BitwiseOr:
                    Output.Write('|');
                    break;
                case CodeBinaryOperatorType.BitwiseAnd:
                    Output.Write('&');
                    break;
                case CodeBinaryOperatorType.BooleanOr:
                    Output.Write("||");
                    break;
                case CodeBinaryOperatorType.BooleanAnd:
                    Output.Write("&&");
                    break;
                case CodeBinaryOperatorType.LessThan:
                    Output.Write('<');
                    break;
                case CodeBinaryOperatorType.LessThanOrEqual:
                    Output.Write("<=");
                    break;
                case CodeBinaryOperatorType.GreaterThan:
                    Output.Write('>');
                    break;
                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    Output.Write(">=");
                    break;
            }
        }

        protected abstract void GenerateArrayCreateExpression(CodeArrayCreateExpression e);

        protected abstract void GenerateCodeArrayDimensionExpression(CodeArrayDimensionExpression e);

        protected void GenerateFlagsBinaryOperatorExpression(CodeFlagsBinaryOperatorExpression e)
        {
            if (e.Expressions.Count == 0) return;

            bool isFirst = true;

            foreach (CodeExpression expression in e.Expressions)
            {
                if (isFirst)
                {
                    GenerateExpression(e.Expressions[0]);
                    isFirst = false;
                }
                else
                {
                    Output.Write(' ');
                    OutputOperator(e.Operator);
                    Output.Write(' ');
                    GenerateExpression(expression);
                }
            }
        }

        protected virtual void GenerateBinaryOperatorExpression(CodeBinaryOperatorExpression e)
        {
            bool indentedExpression = false;
            Output.Write('(');

            GenerateExpression(e.Left);
            Output.Write(' ');

            if (e.Left is CodeBinaryOperatorExpression || e.Right is CodeBinaryOperatorExpression)
            {
                // In case the line gets too long with nested binary operators, we need to output them on
                // different lines. However we want to indent them to maintain readability, but this needs
                // to be done only once;
                if (!_inNestedBinary)
                {
                    indentedExpression = true;
                    _inNestedBinary = true;
                    Indent += 3;
                }
                ContinueOnNewLine("");
            }

            OutputOperator(e.Operator);

            Output.Write(' ');
            GenerateExpression(e.Right);

            Output.Write(')');
            if (indentedExpression)
            {
                Indent -= 3;
                _inNestedBinary = false;
            }
        }

        protected virtual void ContinueOnNewLine(string st, bool newLineContinuation = true) => Output.WriteLine(st);

        protected abstract void GenerateCastExpression(CodeCastExpression e);
        protected abstract void GenerateFieldReferenceExpression(CodeFieldReferenceExpression e);
        protected abstract void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e);
        protected abstract void GenerateSnippetExpression(CodeSnippetExpression e);
        protected abstract void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e);
        protected abstract void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e);
        protected abstract void GenerateValueTupleCreateExpression(CodeValueTupleCreateExpression e);
        protected abstract void GenerateCodeImplicitKeyValuePairCreateExpression(CodeImplicitKeyValuePairCreateExpression e);
        protected abstract void GenerateStatementExpression(CodeStatementExpression e);
        protected abstract void GenerateSeparatedExpressionCollection(CodeSeparatedExpressionCollection e);
        protected abstract void GenerateObjectCreateExpression(CodeObjectCreateExpression e);
        protected abstract void GenerateLambdaExpression(CodeLambdaExpression e);
        protected abstract void GenerateObjectCreateAndInitializeExpression(CodeObjectCreateAndInitializeExpression e);
        protected abstract void GenerateNamedArgumentExpression(CodeNamedArgumentExpression e);
        protected abstract void GenerateCodeAssignExpression(CodeAssignExpression e);
        protected virtual void GeneratePrimitiveExpression(CodePrimitiveExpression e)
        {
            if (e.Value == null)
            {
                Output.Write(NullToken);
            }
            else if (e.Value is string)
            {
                Output.Write(QuoteSnippetString((string)e.Value));
            }
            else if (e.Value is char)
            {
                Output.Write("'" + e.Value + "'");
            }
            else if (e.Value is byte)
            {
                Output.Write(((byte)e.Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is short)
            {
                Output.Write(((short)e.Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is int)
            {
                Output.Write(((int)e.Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is long)
            {
                Output.Write(((long)e.Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is float)
            {
                GenerateSingleFloatValue((float)e.Value);
            }
            else if (e.Value is double)
            {
                GenerateDoubleValue((double)e.Value);
            }
            else if (e.Value is decimal)
            {
                GenerateDecimalValue((decimal)e.Value);
            }
            else if (e.Value is bool)
            {
                if ((bool)e.Value)
                {
                    Output.Write("true");
                }
                else
                {
                    Output.Write("false");
                }
            }
            else
            {
                throw new ArgumentException(SR.Format(SR.InvalidPrimitiveType, e.Value.GetType().ToString()));
            }
        }

        protected virtual void GenerateSingleFloatValue(float s) => Output.Write(s.ToString("R", CultureInfo.InvariantCulture));

        protected virtual void GenerateDoubleValue(double d) => Output.Write(d.ToString("R", CultureInfo.InvariantCulture));

        protected virtual void GenerateDecimalValue(decimal d) => Output.Write(d.ToString(CultureInfo.InvariantCulture));

        protected virtual void GenerateDefaultValueExpression(CodeDefaultValueExpression e)
        {
        }

        protected abstract void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e);

        protected virtual void GenerateTypeReferenceExpression(CodeTypeReferenceExpression e)
        {
            OutputType(e.Type);
        }

        protected virtual void GenerateTypeOfExpression(CodeTypeOfExpression e)
        {
            Output.Write("typeof(");
            OutputType(e.Type);
            Output.Write(')');
        }

        protected abstract void GenerateExpressionStatement(CodeExpressionStatement e);
        protected virtual void GenerateCommentStatement(CodeCommentStatement e)
        {
            if (e.Comment == null)
            {
                throw new ArgumentException(SR.Format(SR.Argument_NullComment, nameof(e)), nameof(e));
            }
            GenerateComment(e.Comment);
        }

        protected virtual void GenerateCommentStatements(CodeCommentStatementCollection e)
        {
            foreach (CodeCommentStatement comment in e)
            {
                GenerateCommentStatement(comment);
            }
        }

        protected abstract void GenerateComment(CodeComment e);
        protected abstract void GenerateConditionStatement(CodeConditionStatement e);
        protected abstract void GenerateAssignStatement(CodeAssignStatement e);
        protected virtual void GenerateSnippetStatement(CodeSnippetStatement e) => Output.WriteLine(e.Value);
        protected abstract void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e);
        protected abstract bool IsValidIdentifier(string value);

        protected virtual void ValidateIdentifier(string value)
        {
            if (!IsValidIdentifier(value))
            {
                throw new ArgumentException(SR.Format(SR.InvalidIdentifier, value));
            }
        }

        protected abstract string CreateEscapedIdentifier(string value);
        protected abstract string CreateValidIdentifier(string value);
        protected abstract string GetTypeOutput(CodeTypeReference value);
        protected abstract string QuoteSnippetString(string value);

        public static bool IsValidLanguageIndependentIdentifier(string value) => CSharpHelpers.IsValidTypeNameOrIdentifier(value, false);

        internal static bool IsValidLanguageIndependentTypeName(string value) => CSharpHelpers.IsValidTypeNameOrIdentifier(value, true);
    }
}
