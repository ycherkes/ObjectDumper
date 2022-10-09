// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Globalization;

namespace YellowFlavor.Serialization.Embedded.CodeDom.ms
{
    /// <devdoc>
    ///    <para>Provides a base class for code generators.</para>
    /// </devdoc>
    internal abstract class CSharpHelpers
    {
        private static readonly Dictionary<string, object> s_fixedStringLookup;

        static CSharpHelpers()
        {
            s_fixedStringLookup = new Dictionary<string, object>();
            for (int i = 0; i < s_keywords.Length; i++)
            {
                string[] values = s_keywords[i];
                if (values != null)
                {
                    for (int j = 0; j < values.Length; j++)
                    {
                        s_fixedStringLookup.Add(values[j], null);
                    }
                }
            }
        }

        public static string CreateEscapedIdentifier(string name)
        {
            // Any identifier started with two consecutive underscores are 
            // reserved by CSharp.
            if (IsKeyword(name) || IsPrefixTwoUnderscore(name))
            {
                return "@" + name;
            }
            return name;
        }

        private static readonly string[][] s_keywords = {
            null,           // 1 character
            new[] {  // 2 characters
                "as",
                "do",
                "if",
                "in",
                "is",
            },
            new[] {  // 3 characters
                "for",
                "int",
                "new",
                "out",
                "ref",
                "try",
            },
            new[] {  // 4 characters
                "base",
                "bool",
                "byte",
                "case",
                "char",
                "else",
                "enum",
                "goto",
                "lock",
                "long",
                "null",
                "this",
                "true",
                "uint",
                "void",
            },
            new[] {  // 5 characters
                "break",
                "catch",
                "class",
                "const",
                "event",
                "false",
                "fixed",
                "float",
                "sbyte",
                "short",
                "throw",
                "ulong",
                "using",
                "where",
                "while",
                "yield",
            },
            new[] {  // 6 characters
                "double",
                "extern",
                "object",
                "params",
                "public",
                "return",
                "sealed",
                "sizeof",
                "static",
                "string",
                "struct",
                "switch",
                "typeof",
                "unsafe",
                "ushort",
            },
            new[] {  // 7 characters
                "checked",
                "decimal",
                "default",
                "finally",
                "foreach",
                "partial",
                "private",
                "virtual",
            },
            new[] {  // 8 characters
                "abstract",
                "continue",
                "delegate",
                "explicit",
                "implicit",
                "internal",
                "operator",
                "override",
                "readonly",
                "volatile",
            },
            new[] {  // 9 characters
                "__arglist",
                "__makeref",
                "__reftype",
                "interface",
                "namespace",
                "protected",
                "unchecked",
            },
            new[] {  // 10 characters
                "__refvalue",
                "stackalloc",
            },
        };

        internal static bool IsKeyword(string value)
        {
            return s_fixedStringLookup.ContainsKey(value);
        }

        internal static bool IsPrefixTwoUnderscore(string value)
        {
            if (value.Length < 3)
            {
                return false;
            }
            else
            {
                return value[0] == '_' && value[1] == '_' && value[2] != '_';
            }
        }

        internal static bool IsValidTypeNameOrIdentifier(string value, bool isTypeName)
        {
            bool nextMustBeStartChar = true;

            if (value.Length == 0)
                return false;

            // each char must be Lu, Ll, Lt, Lm, Lo, Nd, Mn, Mc, Pc
            //
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                switch (uc)
                {
                    case UnicodeCategory.UppercaseLetter:        // Lu
                    case UnicodeCategory.LowercaseLetter:        // Ll
                    case UnicodeCategory.TitlecaseLetter:        // Lt
                    case UnicodeCategory.ModifierLetter:         // Lm
                    case UnicodeCategory.LetterNumber:           // Lm
                    case UnicodeCategory.OtherLetter:            // Lo
                        nextMustBeStartChar = false;
                        break;

                    case UnicodeCategory.NonSpacingMark:         // Mn
                    case UnicodeCategory.SpacingCombiningMark:   // Mc
                    case UnicodeCategory.ConnectorPunctuation:   // Pc
                    case UnicodeCategory.DecimalDigitNumber:     // Nd
                        // Underscore is a valid starting character, even though it is a ConnectorPunctuation.
                        if (nextMustBeStartChar && ch != '_')
                            return false;

                        nextMustBeStartChar = false;
                        break;
                    default:
                        // We only check the special Type chars for type names.
                        if (isTypeName && IsSpecialTypeChar(ch, ref nextMustBeStartChar))
                        {
                            break;
                        }

                        return false;
                }
            }

            return true;
        }

        // This can be a special character like a separator that shows up in a type name
        // This is an odd set of characters.  Some come from characters that are allowed by C++, like < and >.
        // Others are characters that are specified in the type and assembly name grammar.
        private static bool IsSpecialTypeChar(char ch, ref bool nextMustBeStartChar)
        {
            switch (ch)
            {
                case ':':
                case '.':
                case '$':
                case '+':
                case '<':
                case '>':
                case '-':
                case '[':
                case ']':
                case ',':
                case '&':
                case '*':
                    nextMustBeStartChar = true;
                    return true;

                case '`':
                    return true;
            }
            return false;
        }
    }
}
