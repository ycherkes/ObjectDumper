// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom.Compiler;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.Common.src.Sys;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.Common.src.Sys.CodeDom;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.Resources;

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.Microsoft.CSharp
{
    internal sealed class CSharpCodeGenerator : ICodeGenerator
    {
        private ExposedTabStringIndentedTextWriter _output;
        private CodeGeneratorOptions _options;
        private bool _inNestedBinary = false;

        private const int MaxLineLength = int.MaxValue;
        private int Indent
        {
            get => _output.Indent;
            set => _output.Indent = value;
        }

        private string NullToken => "null";

        private CodeGeneratorOptions Options => _options;

        private TextWriter Output => _output;

        private string QuoteSnippetStringCStyle(string value)
        {
            var b = new StringBuilder(value.Length + 5);

            var indentObj = new Indentation(_output, Indent + 1);

            b.Append('\"');

            int i = 0;
            while (i < value.Length)
            {
                switch (value[i])
                {
                    case '\r':
                        b.Append("\\r");
                        break;
                    case '\t':
                        b.Append("\\t");
                        break;
                    case '\"':
                        b.Append("\\\"");
                        break;
                    case '\'':
                        b.Append("\\\'");
                        break;
                    case '\\':
                        b.Append("\\\\");
                        break;
                    case '\0':
                        b.Append("\\0");
                        break;
                    case '\n':
                        b.Append("\\n");
                        break;
                    case '\u2028':
                    case '\u2029':
                        AppendEscapedChar(b, value[i]);
                        break;

                    default:
                        b.Append(value[i]);
                        break;
                }

                if (i > 0 && i % MaxLineLength == 0)
                {
                    //
                    // If current character is a high surrogate and the following 
                    // character is a low surrogate, don't break them. 
                    // Otherwise when we write the string to a file, we might lose 
                    // the characters.
                    // 
                    if (char.IsHighSurrogate(value[i]) && (i < value.Length - 1) && char.IsLowSurrogate(value[i + 1]))
                    {
                        b.Append(value[++i]);
                    }

                    b.Append("\" +");
                    b.Append(Environment.NewLine);
                    b.Append(indentObj.IndentationString);
                    b.Append('\"');
                }
                ++i;
            }

            b.Append('\"');

            return b.ToString();
        }

        private string QuoteSnippetStringVerbatimStyle(string value)
        {
            var b = new StringBuilder(value.Length + 5);

            b.Append("@\"");

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\"')
                    b.Append("\"\"");
                else
                    b.Append(value[i]);
            }

            b.Append('\"');

            return b.ToString();
        }

        private string QuoteSnippetString(string value)
        {
            // If the string is short, use C style quoting (e.g "\r\n")
            // Also do it if it is too long to fit in one line
            // If the string contains '\0', verbatim style won't work.
            if (value.Length < 256 || value.Length > 1500 || (value.IndexOf('\0') != -1))
                return QuoteSnippetStringCStyle(value);

            // Otherwise, use 'verbatim' style quoting (e.g. @"foo")
            return QuoteSnippetStringVerbatimStyle(value);
        }

        private void ContinueOnNewLine(string st) => Output.WriteLine(st);

        private void OutputIdentifier(string ident) => Output.Write(CreateEscapedIdentifier(ident));

        private void OutputType(CodeTypeReference typeRef) => Output.Write(GetTypeOutput(typeRef));

        private void GenerateArrayCreateExpression(CodeArrayCreateExpression e)
        {
            Output.Write("new ");

            CodeExpressionCollection init = e.Initializers;
            if (init.Count > 0)
            {
                OutputType(e.CreateType);
                if (e.CreateType.ArrayRank == 0)
                {
                    // Unfortunately, many clients are already calling this without array
                    // types. This will allow new clients to correctly use the array type and
                    // not break existing clients. For VNext, stop doing this.
                    Output.WriteLine("[]");
                }
                else
                {
                    Output.WriteLine("");
                }
                //Indent--;
                Output.WriteLine("{");
                OutputExpressionList(init, newlineBetweenItems: true);
                Output.WriteLine();
                Output.Write("}");
            }
            else
            {
                Output.Write(GetBaseTypeOutput(e.CreateType));

                Output.Write('[');
                if (e.SizeExpression != null)
                {
                    GenerateExpression(e.SizeExpression);
                }
                else
                {
                    Output.Write(e.Size);
                }
                Output.Write(']');

                int nestedArrayDepth = e.CreateType.NestedArrayDepth;
                for (int i = 0; i < nestedArrayDepth - 1; i++)
                {
                    Output.Write("[]");
                }
            }
        }

        private void GenerateBinaryOperatorExpression(CodeBinaryOperatorExpression e)
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

        private void GenerateCastExpression(CodeCastExpression e)
        {
            if (e.SimpleParentheses)
            {
                Output.Write("(");
                OutputType(e.TargetType);
                Output.Write(")");
                GenerateExpression(e.Expression);
            }
            else
            {
                Output.Write("((");
                OutputType(e.TargetType);
                Output.Write(")(");
                GenerateExpression(e.Expression);
                Output.Write("))");
            }
        }

        private void GenerateCodeAssignExpression(CodeAssignExpression e)
        {
            GenerateExpression(e.Left);
            Output.Write(" = ");
            GenerateExpression(e.Right);
        }

        private void GenerateDefaultValueExpression(CodeDefaultValueExpression e)
        {
            Output.Write("default(");
            OutputType(e.Type);
            Output.Write(')');
        }

        private void GenerateFieldReferenceExpression(CodeFieldReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                GenerateExpression(e.TargetObject);
                Output.Write('.');
            }
            OutputIdentifier(e.FieldName);
        }

        private void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e) =>
            OutputIdentifier(e.VariableName);

        private void GenerateSnippetExpression(CodeSnippetExpression e)
        {
            Output.Write(e.Value);
        }

        private void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e)
        {
            GenerateMethodReferenceExpression(e.Method);
            Output.Write('(');
            OutputExpressionList(e.Parameters);
            Output.Write(')');
        }

        private void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                if (e.TargetObject is CodeBinaryOperatorExpression)
                {
                    Output.Write('(');
                    GenerateExpression(e.TargetObject);
                    Output.Write(')');
                }
                else
                {
                    GenerateExpression(e.TargetObject);
                }
                Output.Write('.');
            }
            OutputIdentifier(e.MethodName);

            if (e.TypeArguments.Count > 0)
            {
                Output.Write(GetTypeArgumentsOutput(e.TypeArguments));
            }
        }

        private void GenerateStatement(CodeStatement e)
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

        private void GenerateStatementExpression(CodeStatementExpression e)
        {
            GenerateStatement(e.Statement);
        }

        private void GenerateStatements(CodeStatementCollection stmts)
        {
            foreach (CodeStatement stmt in stmts)
            {
                ((ICodeGenerator)this).GenerateCodeFromStatement(stmt, _output.InnerWriter, _options);
            }
        }

        private void GenerateObjectCreateAndInitializeExpression(CodeObjectCreateAndInitializeExpression e)
        {
            Output.Write("new ");
            OutputType(e.CreateType);
            if (e.Parameters.Count > 0 || e.InitializeExpressions.Count == 0)
            {
                Output.Write('(');
                OutputExpressionList(e.Parameters);
                Output.Write(')');
            }

            if (e.InitializeExpressions.Count > 0)
            {
                Output.WriteLine();
                Output.WriteLine('{');
                OutputExpressionList(e.InitializeExpressions, newlineBetweenItems: true);
                Output.WriteLine();
                Output.Write("}");
            }
        }

        private void GenerateValueTupleCreateExpression(CodeValueTupleCreateExpression e)
        {
            Output.Write('(');
            OutputExpressionList(e.Parameters);
            Output.Write(')');
        }
        private void GenerateObjectCreateExpression(CodeObjectCreateExpression e)
        {
            Output.Write("new ");
            OutputType(e.CreateType);
            Output.Write('(');
            OutputExpressionList(e.Parameters);
            Output.Write(')');
        }

        private void GeneratePrimitiveExpression(CodePrimitiveExpression e)
        {
            if (e.Value is char)
            {
                GeneratePrimitiveChar((char)e.Value);
            }
            else if (e.Value is sbyte)
            {
                // C# has no literal marker for types smaller than Int32                
                Output.Write(((sbyte)e.Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is ushort)
            {
                // C# has no literal marker for types smaller than Int32, and you will
                // get a conversion error if you use "u" here.
                Output.Write(((ushort)e.Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (e.Value is uint)
            {
                Output.Write(((uint)e.Value).ToString(CultureInfo.InvariantCulture));
                Output.Write('u');
            }
            else if (e.Value is ulong)
            {
                Output.Write(((ulong)e.Value).ToString(CultureInfo.InvariantCulture));
                Output.Write("ul");
            }
            else
            {
                GeneratePrimitiveExpressionBase(e);
            }
        }

        private void GeneratePrimitiveExpressionBase(CodePrimitiveExpression e)
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

        private void GeneratePrimitiveChar(char c)
        {
            Output.Write('\'');
            switch (c)
            {
                case '\r':
                    Output.Write("\\r");
                    break;
                case '\t':
                    Output.Write("\\t");
                    break;
                case '\"':
                    Output.Write("\\\"");
                    break;
                case '\'':
                    Output.Write("\\\'");
                    break;
                case '\\':
                    Output.Write("\\\\");
                    break;
                case '\0':
                    Output.Write("\\0");
                    break;
                case '\n':
                    Output.Write("\\n");
                    break;
                case '\u2028':
                case '\u2029':
                case '\u0084':
                case '\u0085':
                    AppendEscapedChar(null, c);
                    break;

                default:
                    if (char.IsSurrogate(c))
                    {
                        AppendEscapedChar(null, c);
                    }
                    else
                    {
                        Output.Write(c);
                    }
                    break;
            }
            Output.Write('\'');
        }

        private void AppendEscapedChar(StringBuilder b, char value)
        {
            if (b == null)
            {
                Output.Write("\\u");
                Output.Write(((int)value).ToString("X4", CultureInfo.InvariantCulture));
            }
            else
            {
                b.Append("\\u");
                b.Append(((int)value).ToString("X4", CultureInfo.InvariantCulture));
            }
        }

        private void GenerateExpressionStatement(CodeExpressionStatement e)
        {
            GenerateExpression(e.Expression);
            Output.WriteLine(';');
        }

        private void GenerateComment(CodeComment e)
        {
            string commentLineStart = e.DocComment ? "///" : "//";
            Output.Write(commentLineStart);
            Output.Write(' ');

            string value = e.Text;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\u0000')
                {
                    continue;
                }

                Output.Write(value[i]);

                if (value[i] == '\r')
                {
                    if (i < value.Length - 1 && value[i + 1] == '\n')
                    {
                        // if next char is '\n', skip it
                        Output.Write('\n');
                        i++;
                    }

                    _output.InternalOutputTabs();
                    Output.Write(commentLineStart);
                }
                else if (value[i] == '\n')
                {
                    _output.InternalOutputTabs();
                    Output.Write(commentLineStart);
                }
                else if (value[i] == '\u2028' || value[i] == '\u2029' || value[i] == '\u0085')
                {
                    Output.Write(commentLineStart);
                }
            }
            if (!e.NoNewLine)
            {
                Output.WriteLine();
            }
        }

        private void GenerateCommentNoNewLine(CodeComment e)
        {
            string commentLineStart = e.DocComment ? "///" : "//";
            Output.Write(commentLineStart);
            Output.Write(' ');

            string value = e.Text;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\u0000')
                {
                    continue;
                }

                Output.Write(value[i]);

                if (value[i] == '\r')
                {
                    if (i < value.Length - 1 && value[i + 1] == '\n')
                    {
                        // if next char is '\n', skip it
                        Output.Write('\n');
                        i++;
                    }

                    _output.InternalOutputTabs();
                    Output.Write(commentLineStart);
                }
                else if (value[i] == '\n')
                {
                    _output.InternalOutputTabs();
                    Output.Write(commentLineStart);
                }
                else if (value[i] == '\u2028' || value[i] == '\u2029' || value[i] == '\u0085')
                {
                    Output.Write(commentLineStart);
                }
            }
        }

        private void GenerateCommentStatement(CodeCommentStatement e)
        {
            if (e.Comment == null)
            {
                throw new ArgumentException(SR.Format(SR.Argument_NullComment, nameof(e)), nameof(e));
            }
            GenerateComment(e.Comment);
        }
        private void GenerateCommentStatements(CodeCommentStatementCollection e)
        {
            foreach (CodeCommentStatement comment in e)
            {
                GenerateCommentStatement(comment);
            }
        }

        private void GenerateConditionStatement(CodeConditionStatement e)
        {
            Output.Write("if (");
            GenerateExpression(e.Condition);
            Output.Write(')');
            OutputStartingBrace();
            Indent++;
            GenerateStatements(e.TrueStatements);
            Indent--;

            CodeStatementCollection falseStatemetns = e.FalseStatements;
            if (falseStatemetns.Count > 0)
            {
                Output.Write('}');
                if (Options.ElseOnClosing)
                {
                    Output.Write(' ');
                }
                else
                {
                    Output.WriteLine();
                }
                Output.Write("else");
                OutputStartingBrace();
                Indent++;
                GenerateStatements(e.FalseStatements);
                Indent--;
            }
            Output.WriteLine('}');
        }

        private void GenerateAssignStatement(CodeAssignStatement e)
        {
            GenerateExpression(e.Left);
            Output.Write(" = ");
            GenerateExpression(e.Right);
            Output.WriteLine(';');
        }

        private void GenerateSnippetStatement(CodeSnippetStatement e)
        {
            Output.WriteLine(e.Value);
        }

        private void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e)
        {
            OutputTypeNamePair(e.Type, e.Name);
            if (e.InitExpression != null)
            {
                Output.Write(" = ");
                GenerateExpression(e.InitExpression);
            }
            Output.WriteLine(';');
        }

        private void GenerateExpression(CodeExpression e)
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
            else if (e is CodeImplicitKeyValuePairCreateExpression)
            {
                GenerateCodeImplicitKeyValuePairCreateExpression((CodeImplicitKeyValuePairCreateExpression)e);
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
            else
            {
                if (e == null)
                {
                    throw new ArgumentNullException(nameof(e));
                }

                throw new ArgumentException(SR.Format(SR.InvalidElementType, e.GetType().FullName), nameof(e));
            }
        }

        private void GenerateNamedArgumentExpression(CodeNamedArgumentExpression na)
        {
            Output.Write(na.Name);
            Output.Write(": ");
            GenerateExpression(na.Value);
        }

        private void GenerateFlagsBinaryOperatorExpression(CodeFlagsBinaryOperatorExpression e)
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

        private void GenerateSeparatedExpressionCollection(CodeSeparatedExpressionCollection e)
        {
            var collectionLength = e.ExpressionCollection.Count;
            int current = 0;

            foreach (CodeExpression codeExpression in e.ExpressionCollection)
            {
                current++;
                GenerateExpression(codeExpression);
                if (current < collectionLength)
                {
                    Output.Write(e.Separator);
                }
            }
        }

        private void GenerateCodeImplicitKeyValuePairCreateExpression(CodeImplicitKeyValuePairCreateExpression e)
        {
            Output.WriteLine('{');
            OutputExpressionList(new CodeExpressionCollection(new[] { e.Key, e.Value }), true);
            Output.WriteLine();
            Output.Write('}');
        }

        private void GenerateLambdaExpression(CodeLambdaExpression codeLambdaExpression)
        {
            if (codeLambdaExpression.Parameters.Count != 1)
            {
                Output.Write('(');
            }
            bool first = true;
            foreach (CodeExpression current in codeLambdaExpression.Parameters)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Output.Write(", ");
                }
                GenerateExpression(current);
            }

            if (codeLambdaExpression.Parameters.Count != 1)
            {
                Output.Write(')');
            }
            Output.Write(" => ");
            GenerateExpression(codeLambdaExpression.LambdaExpression);
        }

        private void GenerateSingleFloatValue(float s)
        {
            if (float.IsNaN(s))
            {
                Output.Write("float.NaN");
            }
            else if (float.IsNegativeInfinity(s))
            {
                Output.Write("float.NegativeInfinity");
            }
            else if (float.IsPositiveInfinity(s))
            {
                Output.Write("float.PositiveInfinity");
            }
            else
            {
                Output.Write(s.ToString(CultureInfo.InvariantCulture));
                Output.Write('F');
            }
        }

        private void GenerateDoubleValue(double d)
        {
            if (double.IsNaN(d))
            {
                Output.Write("double.NaN");
            }
            else if (double.IsNegativeInfinity(d))
            {
                Output.Write("double.NegativeInfinity");
            }
            else if (double.IsPositiveInfinity(d))
            {
                Output.Write("double.PositiveInfinity");
            }
            else
            {
                Output.Write(d.ToString("R", CultureInfo.InvariantCulture));
                // always mark a double as being a double in case we have no decimal portion (e.g write 1D instead of 1 which is an int)
                Output.Write('D');
            }
        }

        private void GenerateDecimalValue(decimal d)
        {
            Output.Write(d.ToString(CultureInfo.InvariantCulture));
            Output.Write('m');
        }

        private void OutputOperator(CodeBinaryOperatorType op)
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

        private void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                GenerateExpression(e.TargetObject);
                Output.Write('.');
            }
            OutputIdentifier(e.PropertyName);
        }

        private void GenerateTypeReferenceExpression(CodeTypeReferenceExpression e) =>
            OutputType(e.Type);

        private void GenerateTypeOfExpression(CodeTypeOfExpression e)
        {
            Output.Write("typeof(");
            OutputType(e.Type);
            Output.Write(')');
        }

        private void OutputExpressionList(CodeExpressionCollection expressions)
        {
            OutputExpressionList(expressions, false /*newlineBetweenItems*/);
        }

        private void OutputExpressionList(CodeExpressionCollection expressions, bool newlineBetweenItems)
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
                        ContinueOnNewLine(",");
                    else
                        Output.Write(", ");
                }
                ((ICodeGenerator)this).GenerateCodeFromExpression(current, _output.InnerWriter, _options);
            }
            Indent--;
        }

        private void OutputTypeNamePair(CodeTypeReference typeRef, string name)
        {
            OutputType(typeRef);
            Output.Write(' ');
            OutputIdentifier(name);
        }

        public bool IsValidIdentifier(string value)
        {
            // identifiers must be 1 char or longer
            //
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (value.Length > 512)
            {
                return false;
            }

            // identifiers cannot be a keyword, unless they are escaped with an '@'
            //
            if (value[0] != '@')
            {
                if (CSharpHelpers.IsKeyword(value))
                {
                    return false;
                }
            }
            else
            {
                value = value.Substring(1);
            }

            return CodeGenerator.IsValidLanguageIndependentIdentifier(value);
        }

        public void ValidateIdentifier(string value)
        {
            if (!IsValidIdentifier(value))
            {
                throw new ArgumentException(SR.Format(SR.InvalidIdentifier, value));
            }
        }

        public string CreateValidIdentifier(string name)
        {
            if (CSharpHelpers.IsPrefixTwoUnderscore(name))
            {
                name = "_" + name;
            }

            while (CSharpHelpers.IsKeyword(name))
            {
                name = "_" + name;
            }

            return name;
        }

        public string CreateEscapedIdentifier(string name)
        {
            return CSharpHelpers.CreateEscapedIdentifier(name);
        }

        // returns the type name without any array declaration.
        private string GetBaseTypeOutput(CodeTypeReference typeRef, bool preferBuiltInTypes = true)
        {
            string s = typeRef.BaseType;

            if (s == "System.Nullable`1" && typeRef.TypeArguments.Count > 0)
            {
                return GetBaseTypeOutput(typeRef.TypeArguments[0]) + "?";
            }

            if (preferBuiltInTypes)
            {
                if (typeRef is CodeAnonymousTypeReference)
                {
                    return string.Empty;
                }

                if (typeRef is CodeImplicitlyTypedTypeReference)
                {
                    return "var";
                }

                if (s.Length == 0)
                {
                    return "void";
                }

                string lowerCaseString = s.ToLower(CultureInfo.InvariantCulture).Trim();

                switch (lowerCaseString)
                {
                    case "system.int16":
                        return "short";
                    case "system.int32":
                        return "int";
                    case "system.int64":
                        return "long";
                    case "system.string":
                        return "string";
                    case "system.object":
                        return "object";
                    case "system.boolean":
                        return "bool";
                    case "system.void":
                        return "void";
                    case "system.char":
                        return "char";
                    case "system.byte":
                        return "byte";
                    case "system.uint16":
                        return "ushort";
                    case "system.uint32":
                        return "uint";
                    case "system.uint64":
                        return "ulong";
                    case "system.sbyte":
                        return "sbyte";
                    case "system.single":
                        return "float";
                    case "system.double":
                        return "double";
                    case "system.decimal":
                        return "decimal";
                }
            }

            // replace + with . for nested classes.
            //
            var sb = new StringBuilder(s.Length + 10);
            if ((typeRef.Options & CodeTypeReferenceOptions.GlobalReference) != 0)
            {
                sb.Append("global::");
            }

            string baseType = (typeRef.Options & CodeTypeReferenceOptions.ShortTypeName) != 0
                ? typeRef.BaseType.Split('.').Last().Split('+').Last()
                : typeRef.BaseType;

            int lastIndex = 0;
            int currentTypeArgStart = 0;
            for (int i = 0; i < baseType.Length; i++)
            {
                switch (baseType[i])
                {
                    case '+':
                    case '.':
                        sb.Append(CreateEscapedIdentifier(baseType.Substring(lastIndex, i - lastIndex)));
                        sb.Append('.');
                        i++;
                        lastIndex = i;
                        break;

                    case '`':
                        sb.Append(CreateEscapedIdentifier(baseType.Substring(lastIndex, i - lastIndex)));
                        i++;    // skip the '
                        int numTypeArgs = 0;
                        while (i < baseType.Length && baseType[i] >= '0' && baseType[i] <= '9')
                        {
                            numTypeArgs = numTypeArgs * 10 + (baseType[i] - '0');
                            i++;
                        }

                        GetTypeArgumentsOutput(typeRef.TypeArguments, currentTypeArgStart, numTypeArgs, sb);
                        currentTypeArgStart += numTypeArgs;

                        // Arity can be in the middle of a nested type name, so we might have a . or + after it. 
                        // Skip it if so. 
                        if (i < baseType.Length && (baseType[i] == '+' || baseType[i] == '.'))
                        {
                            sb.Append('.');
                            i++;
                        }

                        lastIndex = i;
                        break;
                }
            }

            if (lastIndex < baseType.Length)
                sb.Append(CreateEscapedIdentifier(baseType.Substring(lastIndex)));

            return sb.ToString();
        }

        private string GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments)
        {
            var sb = new StringBuilder(128);
            GetTypeArgumentsOutput(typeArguments, 0, typeArguments.Count, sb);
            return sb.ToString();
        }

        private void GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments, int start, int length, StringBuilder sb)
        {
            sb.Append('<');
            bool first = true;
            for (int i = start; i < start + length; i++)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }

                // it's possible that we call GetTypeArgumentsOutput with an empty typeArguments collection.  This is the case
                // for open types, so we want to just output the brackets and commas. 
                if (i < typeArguments.Count)
                    sb.Append(GetTypeOutput(typeArguments[i]));
            }
            sb.Append('>');
        }

        public string GetTypeOutput(CodeTypeReference typeRef)
        {
            string s = string.Empty;

            CodeTypeReference baseTypeRef = typeRef;
            while (baseTypeRef.ArrayElementType != null)
            {
                baseTypeRef = baseTypeRef.ArrayElementType;
            }
            s += GetBaseTypeOutput(baseTypeRef);

            while (typeRef != null && typeRef.ArrayRank > 0)
            {
                char[] results = new char[typeRef.ArrayRank + 1];
                results[0] = '[';
                results[typeRef.ArrayRank] = ']';
                for (int i = 1; i < typeRef.ArrayRank; i++)
                {
                    results[i] = ',';
                }
                s += new string(results);
                typeRef = typeRef.ArrayElementType;
            }

            return s;
        }

        private void OutputStartingBrace()
        {
            if (Options.BracingStyle == "C")
            {
                Output.WriteLine();
                Output.WriteLine('{');
            }
            else
            {
                Output.WriteLine(" {");
            }
        }

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
    }
}
