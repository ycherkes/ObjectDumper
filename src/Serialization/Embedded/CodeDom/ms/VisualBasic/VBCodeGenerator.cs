// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.Microsoft.Common;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.CodeDom.Compiler;
using YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.System.Collections.Specialized;

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms.CodeDom.Microsoft.VisualBasic
{
    internal sealed class VBCodeGenerator : CodeGenerator
    {
        private const int MaxLineLength = int.MaxValue;
        private int _statementDepth = 0;

        // This is the keyword list. To minimize search time and startup time, this is stored by length
        // and then alphabetically for use by FixedStringLookup.Contains.
        private static readonly string[][] s_keywords = {
            null,           // 1 character
            new[] {  // 2 characters            
                "as",
                "do",
                "if",
                "in",
                "is",
                "me",
                "of",
                "on",
                "or",
                "to",
            },
            new[] {  // 3 characters
                "and",
                "dim",
                "end",
                "for",
                "get",
                "let",
                "lib",
                "mod",
                "new",
                "not",
                "rem",
                "set",
                "sub",
                "try",
                "xor",
            },
            new[] {  // 4 characters
                "ansi",
                "auto",
                "byte",
                "call",
                "case",
                "cdbl",
                "cdec",
                "char",
                "cint",
                "clng",
                "cobj",
                "csng",
                "cstr",
                "date",
                "each",
                "else",
                "enum",
                "exit",
                "goto",
                "like",
                "long",
                "loop",
                "next",
                "step",
                "stop",
                "then",
                "true",
                "wend",
                "when",
                "with",
            },
            new[] {  // 5 characters  
                "alias",
                "byref",
                "byval",
                "catch",
                "cbool",
                "cbyte",
                "cchar",
                "cdate",
                "class",
                "const",
                "ctype",
                "cuint",
                "culng",
                "endif",
                "erase",
                "error",
                "event",
                "false",
                "gosub",
                "isnot",
                "redim",
                "sbyte",
                "short",
                "throw",
                "ulong",
                "until",
                "using",
                "while",
             },
            new[] {  // 6 characters
                "csbyte",
                "cshort",
                "double",
                "elseif",
                "friend",
                "global",
                "module",
                "mybase",
                "object",
                "option",
                "orelse",
                "public",
                "resume",
                "return",
                "select",
                "shared",
                "single",
                "static",
                "string",
                "typeof",
                "ushort",
            },
            new[] { // 7 characters
                "andalso",
                "boolean",
                "cushort",
                "decimal",
                "declare",
                "default",
                "finally",
                "gettype",
                "handles",
                "imports",
                "integer",
                "myclass",
                "nothing",
                "partial",
                "private",
                "shadows",
                "trycast",
                "unicode",
                "variant",
            },
            new[] {  // 8 characters
                "assembly",
                "continue",
                "delegate",
                "function",
                "inherits",
                "operator",
                "optional",
                "preserve",
                "property",
                "readonly",
                "synclock",
                "uinteger",
                "widening"
            },
            new[] { // 9 characters
                "addressof",
                "interface",
                "namespace",
                "narrowing",
                "overloads",
                "overrides",
                "protected",
                "structure",
                "writeonly",
            },
            new[] { // 10 characters
                "addhandler",
                "directcast",
                "implements",
                "paramarray",
                "raiseevent",
                "withevents",
            },
            new[] {  // 11 characters
                "mustinherit",
                "overridable",
            },
            new[] { // 12 characters
                "mustoverride",
            },
            new[] { // 13 characters
                "removehandler",
            },
            // class_finalize and class_initialize are not keywords anymore,
            // but it will be nice to escape them to avoid warning
            new[] { // 14 characters
                "class_finalize",
                "notinheritable",
                "notoverridable",
            },
            null,           // 15 characters
            new[] {
                "class_initialize",
            }
        };

        /// <summary>Tells whether or not the current class should be generated as a module</summary>

        protected override string NullToken => "Nothing";

        private static void EnsureInDoubleQuotes(ref bool fInDoubleQuotes, StringBuilder b)
        {
            if (fInDoubleQuotes) return;
            b.Append("&\"");
            fInDoubleQuotes = true;
        }

        private static void EnsureNotInDoubleQuotes(ref bool fInDoubleQuotes, StringBuilder b)
        {
            if (!fInDoubleQuotes) return;
            b.Append('\"');
            fInDoubleQuotes = false;
        }

        protected override string QuoteSnippetString(string value)
        {
            StringBuilder b = new StringBuilder(value.Length + 5);

            bool fInDoubleQuotes = true;
            Indentation indentObj = new Indentation((ExposedTabStringIndentedTextWriter)Output, Indent + 1);

            b.Append('\"');

            int i = 0;
            while (i < value.Length)
            {
                char ch = value[i];
                switch (ch)
                {
                    case '\"':
                    // These are the inward sloping quotes used by default in some cultures like CHS. 
                    // VBC.EXE does a mapping ANSI that results in it treating these as syntactically equivalent to a
                    // regular double quote.
                    case '\u201C':
                    case '\u201D':
                    case '\uFF02':
                        EnsureInDoubleQuotes(ref fInDoubleQuotes, b);
                        b.Append(ch);
                        b.Append(ch);
                        break;
                    case '\r':
                        EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                        if (i < value.Length - 1 && value[i + 1] == '\n')
                        {
                            b.Append("&Global.Microsoft.VisualBasic.ChrW(13)&Global.Microsoft.VisualBasic.ChrW(10)");
                            i++;
                        }
                        else
                        {
                            b.Append("&Global.Microsoft.VisualBasic.ChrW(13)");
                        }
                        break;
                    case '\t':
                        EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                        b.Append("&Global.Microsoft.VisualBasic.ChrW(9)");
                        break;
                    case '\0':
                        EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                        b.Append("&Global.Microsoft.VisualBasic.ChrW(0)");
                        break;
                    case '\n':
                        EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                        b.Append("&Global.Microsoft.VisualBasic.ChrW(10)");
                        break;
                    case '\u2028':
                    case '\u2029':
                        EnsureNotInDoubleQuotes(ref fInDoubleQuotes, b);
                        AppendEscapedChar(b, ch);
                        break;
                    default:
                        EnsureInDoubleQuotes(ref fInDoubleQuotes, b);
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
                    if (char.IsHighSurrogate(value[i])
                        && i < value.Length - 1
                        && char.IsLowSurrogate(value[i + 1]))
                    {
                        b.Append(value[++i]);
                    }

                    if (fInDoubleQuotes)
                        b.Append('\"');
                    fInDoubleQuotes = true;

                    b.Append("& _ ");
                    b.Append(Environment.NewLine);
                    b.Append(indentObj.IndentationString);
                    b.Append('\"');
                }
                ++i;
            }

            if (fInDoubleQuotes)
                b.Append('\"');

            return b.ToString();
        }

        private static void AppendEscapedChar(StringBuilder b, char value)
        {
            b.Append("&Global.Microsoft.VisualBasic.ChrW(");
            b.Append(((int)value).ToString(CultureInfo.InvariantCulture));
            b.Append(")");
        }

        protected override void GenerateNamedArgumentExpression(CodeNamedArgumentExpression e)
        {
            Output.Write(e.Name);
            Output.Write(":=");
            GenerateExpression(e.Value);
        }

        protected override void GenerateCodeAssignExpression(CodeAssignExpression e)
        {
            GenerateExpression(e.Left);
            Output.Write(" = ");
            GenerateExpression(e.Right);
        }

        protected override void GenerateDefaultValueExpression(CodeDefaultValueExpression e)
        {
            Output.Write("CType(Nothing, " + GetTypeOutput(e.Type) + ")");
        }

        protected override void OutputOperator(CodeBinaryOperatorType op)
        {
            switch (op)
            {
                case CodeBinaryOperatorType.IdentityInequality:
                    Output.Write("<>");
                    break;
                case CodeBinaryOperatorType.IdentityEquality:
                    Output.Write("Is");
                    break;
                case CodeBinaryOperatorType.BooleanOr:
                    Output.Write("OrElse");
                    break;
                case CodeBinaryOperatorType.BooleanAnd:
                    Output.Write("AndAlso");
                    break;
                case CodeBinaryOperatorType.ValueEquality:
                    Output.Write('=');
                    break;
                case CodeBinaryOperatorType.Modulus:
                    Output.Write("Mod");
                    break;
                case CodeBinaryOperatorType.BitwiseOr:
                    Output.Write("Or");
                    break;
                case CodeBinaryOperatorType.BitwiseAnd:
                    Output.Write("And");
                    break;
                default:
                    base.OutputOperator(op);
                    break;
            }
        }

        private void GenerateNotIsNullExpression(CodeExpression e)
        {
            Output.Write("(Not (");
            GenerateExpression(e);
            Output.Write(") Is ");
            Output.Write(NullToken);
            Output.Write(')');
        }

        protected override void GenerateBinaryOperatorExpression(CodeBinaryOperatorExpression e)
        {
            if (e.Operator != CodeBinaryOperatorType.IdentityInequality)
            {
                base.GenerateBinaryOperatorExpression(e);
                return;
            }

            // "o <> nothing" should be "not o is nothing"
            if (e.Right is CodePrimitiveExpression && ((CodePrimitiveExpression)e.Right).Value == null)
            {
                GenerateNotIsNullExpression(e.Left);
                return;
            }
            if (e.Left is CodePrimitiveExpression && ((CodePrimitiveExpression)e.Left).Value == null)
            {
                GenerateNotIsNullExpression(e.Right);
                return;
            }

            base.GenerateBinaryOperatorExpression(e);
        }

        protected override void OutputIdentifier(string ident)
        {
            Output.Write(CreateEscapedIdentifier(ident));
        }

        protected override void OutputType(CodeTypeReference typeRef)
        {
            Output.Write(GetTypeOutputWithoutArrayPostFix(typeRef));
        }


        protected void OutputTypeNamePair(CodeTypeReference typeRef, string name)
        {
            if (string.IsNullOrEmpty(name))
                name = "__exception";

            OutputIdentifier(name);
            OutputArrayPostfix(typeRef);
            if (!(typeRef is CodeEmptyTypeReference))
            {
                Output.Write(" As ");
                OutputType(typeRef);
            }
        }

        private string GetArrayPostfix(CodeTypeReference typeRef)
        {
            string s = "";
            if (typeRef.ArrayElementType != null)
            {
                // Recurse up
                s = GetArrayPostfix(typeRef.ArrayElementType);
            }

            if (typeRef.ArrayRank > 0)
            {
                char[] results = new char[typeRef.ArrayRank + 1];
                results[0] = '(';
                results[typeRef.ArrayRank] = ')';
                for (int i = 1; i < typeRef.ArrayRank; i++)
                {
                    results[i] = ',';
                }
                s = new string(results) + s;
            }

            return s;
        }

        private void OutputArrayPostfix(CodeTypeReference typeRef)
        {
            if (typeRef.ArrayRank > 0)
            {
                Output.Write(GetArrayPostfix(typeRef));
            }
        }

        protected override void GeneratePrimitiveExpression(CodePrimitiveExpression e)
        {
            if (e.Value is char)
            {
                Output.Write("Global.Microsoft.VisualBasic.ChrW(" + ((IConvertible)e.Value).ToInt32(CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) + ")");
            }
            else if (e.Value is sbyte)
            {
                Output.Write("CSByte(");
                Output.Write(((sbyte)e.Value).ToString(CultureInfo.InvariantCulture));
                Output.Write(')');
            }
            else if (e.Value is ushort)
            {
                Output.Write(((ushort)e.Value).ToString(CultureInfo.InvariantCulture));
                Output.Write("US");
            }
            else if (e.Value is uint)
            {
                Output.Write(((uint)e.Value).ToString(CultureInfo.InvariantCulture));
                Output.Write("UI");
            }
            else if (e.Value is ulong)
            {
                Output.Write(((ulong)e.Value).ToString(CultureInfo.InvariantCulture));
                Output.Write("UL");
            }
            else
            {
                base.GeneratePrimitiveExpression(e);
            }
        }

        protected override void GenerateArrayCreateExpression(CodeArrayCreateExpression e)
        {
            if (!(e.CreateType is CodeEmptyTypeReference))
            {
                Output.Write("New ");
            }

            CodeExpressionCollection init = e.Initializers;
            if (init.Count > 0)
            {
                if (!(e.CreateType is CodeEmptyTypeReference))
                {
                    string typeName = GetTypeOutput(e.CreateType);
                    Output.Write(typeName);
                    Output.Write("()");
                }

                //Output.WriteLine("");
                Output.Write("{");
                Output.WriteLine("");
                //Indent++;
                OutputExpressionList(init, newlineBetweenItems: true, newLineContinuation: false);
                //Indent--;
                Output.WriteLine("");
                Output.Write('}');
            }
            else
            {
                string typeName = GetTypeOutput(e.CreateType);

                int index = typeName.IndexOf('(');
                if (index == -1)
                {
                    Output.Write(typeName);
                    Output.Write('(');
                }
                else
                {
                    Output.Write(typeName.Substring(0, index + 1));
                }

                // The tricky thing is we need to declare the size - 1
                if (e.SizeExpression != null)
                {
                    Output.Write('(');
                    GenerateExpression(e.SizeExpression);
                    Output.Write(") - 1");
                }
                else
                {
                    Output.Write(e.Size - 1);
                }

                if (index == -1)
                {
                    Output.Write(')');
                }
                else
                {
                    Output.Write(typeName.Substring(index + 1));
                }

                Output.Write(" {}");
            }
        }

        protected override void GenerateCastExpression(CodeCastExpression e)
        {
            Output.Write("CType(");
            GenerateExpression(e.Expression);
            Output.Write(", ");
            OutputType(e.TargetType);
            OutputArrayPostfix(e.TargetType);
            Output.Write(')');
        }

        protected override void GenerateFieldReferenceExpression(CodeFieldReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                GenerateExpression(e.TargetObject);
                Output.Write('.');
            }

            OutputIdentifier(e.FieldName);
        }

        protected override void GenerateSingleFloatValue(float s)
        {
            if (float.IsNaN(s))
            {
                Output.Write("Single.NaN");
            }
            else if (float.IsNegativeInfinity(s))
            {
                Output.Write("Single.NegativeInfinity");
            }
            else if (float.IsPositiveInfinity(s))
            {
                Output.Write("Single.PositiveInfinity");
            }
            else
            {
                Output.Write(s.ToString(CultureInfo.InvariantCulture));
                Output.Write('!');
            }
        }

        protected override void GenerateDoubleValue(double d)
        {
            if (double.IsNaN(d))
            {
                Output.Write("Double.NaN");
            }
            else if (double.IsNegativeInfinity(d))
            {
                Output.Write("Double.NegativeInfinity");
            }
            else if (double.IsPositiveInfinity(d))
            {
                Output.Write("Double.PositiveInfinity");
            }
            else
            {
                Output.Write(d.ToString("R", CultureInfo.InvariantCulture));
                // always mark a double as being a double in case we have no decimal portion (e.g write 1D instead of 1 which is an int)
                Output.Write('R');
            }
        }

        protected override void GenerateDecimalValue(decimal d)
        {
            Output.Write(d.ToString(CultureInfo.InvariantCulture));
            Output.Write('D');
        }

        protected override void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e)
        {
            OutputIdentifier(e.VariableName);
        }

        protected override void GenerateSnippetExpression(CodeSnippetExpression e)
        {
            Output.Write(e.Value);
        }

        protected override void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e)
        {
            GenerateMethodReferenceExpression(e.Method);
            CodeExpressionCollection parameters = e.Parameters;
            if (parameters.Count > 0)
            {
                Output.Write('(');
                OutputExpressionList(e.Parameters);
                Output.Write(')');
            }
            else
            {
                Output.Write("()");
            }
        }

        protected override void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                GenerateExpression(e.TargetObject);
                Output.Write('.');
                Output.Write(e.MethodName);
            }
            else
            {
                OutputIdentifier(e.MethodName);
            }

            if (e.TypeArguments.Count > 0)
            {
                Output.Write(GetTypeArgumentsOutput(e.TypeArguments));
            }
        }

        protected override void GenerateObjectCreateAndInitializeExpression(CodeObjectCreateAndInitializeExpression e)
        {
            Output.Write("New ");
            OutputType(e.CreateType);
            if (e.Parameters.Count > 0 || e.InitializeExpressions.Count == 0)
            {
                // always write out the () to disambiguate cases like "New System.Random().Next(x,y)"
                Output.Write('(');
                OutputExpressionList(e.Parameters);
                Output.Write(')');
            }

            if (e.InitializeExpressions.Count <= 0) return;

            Output.Write(e.CreateType switch
            {
                CodeEmptyTypeReference => "With ",
                CodeCollectionTypeReference => " From ",
                _ => " With "
            });
            Output.WriteLine('{');
            OutputExpressionList(e.InitializeExpressions, newlineBetweenItems: true, newLineContinuation: false);
            Output.WriteLine();
            Output.Write("}");
        }

        protected override void GenerateValueTupleCreateExpression(CodeValueTupleCreateExpression e)
        {
            Output.Write('(');
            OutputExpressionList(e.Parameters);
            Output.Write(')');
        }

        protected override void GenerateCodeImplicitKeyValuePairCreateExpression(CodeImplicitKeyValuePairCreateExpression e)
        {
            Output.WriteLine('{');
            OutputExpressionList(new CodeExpressionCollection(new[] { e.Key, e.Value }), true, false);
            Output.WriteLine();
            Output.Write('}');
        }

        protected override void GenerateStatementExpression(CodeStatementExpression e)
        {
            GenerateStatement(e.Statement);
        }

        protected override void GenerateSeparatedExpressionCollection(CodeSeparatedExpressionCollection e)
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

        protected override void GenerateObjectCreateExpression(CodeObjectCreateExpression e)
        {
            Output.Write("New ");
            OutputType(e.CreateType);
            // always write out the () to disambiguate cases like "New System.Random().Next(x,y)"
            Output.Write('(');
            OutputExpressionList(e.Parameters);
            Output.Write(')');
        }

        protected override void GenerateLambdaExpression(CodeLambdaExpression codeLambdaExpression)
        {
            Output.Write("Function (");
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

            Output.Write(')');
            Output.Write(" ");
            GenerateExpression(codeLambdaExpression.LambdaExpression);
        }

        protected override void GenerateExpressionStatement(CodeExpressionStatement e)
        {
            GenerateExpression(e.Expression);
            Output.WriteLine();
        }

        private bool IsDocComment(CodeCommentStatement comment)
        {
            return ((comment != null) && (comment.Comment != null) && comment.Comment.DocComment);
        }

        protected override void GenerateCommentStatements(CodeCommentStatementCollection e)
        {
            // since the compiler emits a warning if XML DocComment blocks appear before
            //  normal comments, we need to output non-DocComments first, followed by
            //  DocComments.
            //            
            foreach (CodeCommentStatement comment in e)
            {
                if (!IsDocComment(comment))
                {
                    GenerateCommentStatement(comment);
                }
            }

            foreach (CodeCommentStatement comment in e)
            {
                if (IsDocComment(comment))
                {
                    GenerateCommentStatement(comment);
                }
            }
        }

        protected override void GenerateComment(CodeComment e)
        {
            string commentLineStart = e.DocComment ? "'''" : "'";
            Output.Write(commentLineStart);
            string value = e.Text;
            for (int i = 0; i < value.Length; i++)
            {
                Output.Write(value[i]);

                if (value[i] == '\r')
                {
                    if (i < value.Length - 1 && value[i + 1] == '\n')
                    { // if next char is '\n', skip it
                        Output.Write('\n');
                        i++;
                    }
                    ((ExposedTabStringIndentedTextWriter)Output).InternalOutputTabs();
                    Output.Write(commentLineStart);
                }
                else if (value[i] == '\n')
                {
                    ((ExposedTabStringIndentedTextWriter)Output).InternalOutputTabs();
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

        protected override void GenerateConditionStatement(CodeConditionStatement e)
        {
            Output.Write("If ");
            GenerateExpression(e.Condition);
            Output.WriteLine(" Then");
            Indent++;
            GenerateVBStatements(e.TrueStatements);
            Indent--;

            CodeStatementCollection falseStatemetns = e.FalseStatements;
            if (falseStatemetns.Count > 0)
            {
                Output.Write("Else");
                Output.WriteLine();
                Indent++;
                GenerateVBStatements(e.FalseStatements);
                Indent--;
            }
            Output.WriteLine("End If");
        }

        protected override void GenerateAssignStatement(CodeAssignStatement e)
        {
            GenerateExpression(e.Left);
            Output.Write(" = ");
            GenerateExpression(e.Right);
            Output.WriteLine();
        }

        protected override void GenerateSnippetStatement(CodeSnippetStatement e)
        {
            Output.WriteLine(e.Value);
        }

        protected override void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e)
        {
            bool doInit = true;

            Output.Write("Dim ");

            CodeTypeReference typeRef = e.Type;
            if (typeRef.ArrayRank == 1 && e.InitExpression != null)
            {
                CodeArrayCreateExpression eAsArrayCreate = e.InitExpression as CodeArrayCreateExpression;
                if (eAsArrayCreate != null && eAsArrayCreate.Initializers.Count == 0)
                {
                    doInit = false;
                    OutputIdentifier(e.Name);
                    Output.Write('(');

                    if (eAsArrayCreate.SizeExpression != null)
                    {
                        Output.Write('(');
                        GenerateExpression(eAsArrayCreate.SizeExpression);
                        Output.Write(") - 1");
                    }
                    else
                    {
                        Output.Write(eAsArrayCreate.Size - 1);
                    }

                    Output.Write(')');

                    if (typeRef.ArrayElementType != null)
                        OutputArrayPostfix(typeRef.ArrayElementType);

                    Output.Write(" As ");
                    OutputType(typeRef);
                }
                else
                    OutputTypeNamePair(e.Type, e.Name);
            }
            else
                OutputTypeNamePair(e.Type, e.Name);

            if (doInit && e.InitExpression != null)
            {
                Output.Write(" = ");
                GenerateExpression(e.InitExpression);
            }

            Output.WriteLine();
        }

        protected override void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                GenerateExpression(e.TargetObject);
                Output.Write('.');
                Output.Write(e.PropertyName);
            }
            else
            {
                Output.Write('.');
                OutputIdentifier(e.PropertyName);
            }
        }

        protected override void GenerateTypeOfExpression(CodeTypeOfExpression e)
        {
            Output.Write("GetType(");
            Output.Write(GetTypeOutput(e.Type));
            Output.Write(')');
        }

        public static bool IsKeyword(string value)
        {
            return FixedStringLookup.Contains(s_keywords, value, true);
        }

        protected override bool IsValidIdentifier(string value)
        {
            // identifiers must be 1 char or longer
            //
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (value.Length > 1023)
                return false;

            // identifiers cannot be a keyword unless surrounded by []'s
            //
            if (value[0] != '[' || value[value.Length - 1] != ']')
            {
                if (IsKeyword(value))
                {
                    return false;
                }
            }
            else
            {
                value = value.Substring(1, value.Length - 2);
            }

            // just _ as an identifier is not valid. 
            if (value.Length == 1 && value[0] == '_')
                return false;

            return CodeGenerator.IsValidLanguageIndependentIdentifier(value);
        }

        protected override string CreateValidIdentifier(string name)
        {
            if (IsKeyword(name))
            {
                return "_" + name;
            }
            return name;
        }

        protected override string CreateEscapedIdentifier(string name)
        {
            if (IsKeyword(name))
            {
                return "[" + name + "]";
            }
            return name;
        }

        private string GetBaseTypeOutput(CodeTypeReference typeRef, bool preferBuiltInTypes = true)
        {
            string s = typeRef.BaseType;

            if (s == "System.Nullable`1" && typeRef.TypeArguments.Count > 0)
            {
                return GetBaseTypeOutput(typeRef.TypeArguments[0]) + "?";
            }

            if (preferBuiltInTypes)
            {
                if (typeRef is CodeEmptyTypeReference)
                {
                    return string.Empty;
                }

                if (s.Length == 0)
                {
                    return "Void";
                }

                string lowerCaseString = s.ToLowerInvariant();

                switch (lowerCaseString)
                {
                    case "system.byte":
                        return "Byte";
                    case "system.sbyte":
                        return "SByte";
                    case "system.int16":
                        return "Short";
                    case "system.int32":
                        return "Integer";
                    case "system.int64":
                        return "Long";
                    case "system.uint16":
                        return "UShort";
                    case "system.uint32":
                        return "UInteger";
                    case "system.uint64":
                        return "ULong";
                    case "system.string":
                        return "String";
                    case "system.datetime":
                        return "Date";
                    case "system.decimal":
                        return "Decimal";
                    case "system.single":
                        return "Single";
                    case "system.double":
                        return "Double";
                    case "system.boolean":
                        return "Boolean";
                    case "system.char":
                        return "Char";
                    case "system.object":
                        return "Object";
                }
            }

            var sb = new StringBuilder(s.Length + 10);
            if ((typeRef.Options & CodeTypeReferenceOptions.GlobalReference) != 0)
            {
                sb.Append("Global.");
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
            {
                sb.Append(CreateEscapedIdentifier(baseType.Substring(lastIndex)));
            }

            return sb.ToString();
        }

        private string GetTypeOutputWithoutArrayPostFix(CodeTypeReference typeRef)
        {
            StringBuilder sb = new StringBuilder();

            while (typeRef.ArrayElementType != null)
            {
                typeRef = typeRef.ArrayElementType;
            }

            sb.Append(GetBaseTypeOutput(typeRef));
            return sb.ToString();
        }

        private string GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments)
        {
            StringBuilder sb = new StringBuilder(128);
            GetTypeArgumentsOutput(typeArguments, 0, typeArguments.Count, sb);
            return sb.ToString();
        }


        private void GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments, int start, int length, StringBuilder sb)
        {
            sb.Append("(Of ");
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
            sb.Append(')');
        }

        protected override string GetTypeOutput(CodeTypeReference typeRef)
        {
            string s = string.Empty;
            s += GetTypeOutputWithoutArrayPostFix(typeRef);

            if (typeRef.ArrayRank > 0)
            {
                s += GetArrayPostfix(typeRef);
            }
            return s;
        }

        protected override void ContinueOnNewLine(string st, bool newLineContinuation = true)
        {
            Output.Write(st);
            Output.WriteLine(newLineContinuation ? " _" : "");
        }

        private bool IsGeneratingStatements()
        {
            Debug.Assert(_statementDepth >= 0, "statementDepth >= 0");
            return (_statementDepth > 0);
        }

        private void GenerateVBStatements(CodeStatementCollection stms)
        {
            _statementDepth++;
            try
            {
                GenerateStatements(stms);
            }
            finally
            {
                _statementDepth--;
            }
        }
    }
}
