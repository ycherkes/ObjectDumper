using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectFormatter.ObjectDumper.NET.Embedded
{
    public abstract class DumperBase
    {
        private readonly List<int> hashListOfFoundElements;
        private readonly StringBuilder stringBuilder;
        private bool isNewLine;
        private int level;

        protected DumperBase(DumpOptions dumpOptions)
        {
            DumpOptions = dumpOptions;
            Level = 0;
            stringBuilder = new StringBuilder();
            hashListOfFoundElements = new List<int>();
            isNewLine = true;
        }

        protected abstract void FormatValue(object o, int intentLevel);

        public int Level
        {
            get => level;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Level must not be a negative number", nameof(Level));
                }

                level = value;
            }
        }

        public bool IsMaxLevel()
        {
            return Level > DumpOptions.MaxLevel;
        }

        protected DumpOptions DumpOptions { get; }

        /// <summary>
        /// Calls <see cref="Write(string, int)"/> using the current Level as indentLevel if the current
        /// position is at a the beginning of a new line or 0 otherwise. 
        /// </summary>
        /// <param name="value">string to be written</param>
        protected void Write(string value)
        {
            if (isNewLine)
            {
                Write(value, Level);
            }
            else
            {
                Write(value, 0);
            }
        }

        /// <summary>
        /// Writes value to underlying <see cref="StringBuilder"/> using <paramref name="indentLevel"/> and <see cref="DumpOptions.IndentChar"/> and <see cref="DumpOptions.IndentSize"/>
        /// to determin the indention chars prepended to <paramref name="value"/>
        /// </summary>
        /// <remarks>
        /// This function needs to keep up with if the last value written included the LineBreakChar at the end
        /// </remarks>
        /// <param name="value">string to be written</param>
        /// <param name="indentLevel">number of indentions to prepend default 0</param>
        protected void Write(string value, int indentLevel = 0)
        {
            stringBuilder.Append(DumpOptions.IndentChar, indentLevel * DumpOptions.IndentSize);
            stringBuilder.Append(value);
            if (value.EndsWith(DumpOptions.LineBreakChar))
            {
                isNewLine = true;
            }
            else
            {
                isNewLine = false;
            }
        }

        /// <summary>
        /// Writes a line break to underlying <see cref="StringBuilder"/> using <see cref="DumpOptions.LineBreakChar"/>
        /// </summary>
        /// <remarks>
        /// By definition this sets isNewLine to true
        /// </remarks>
        protected void LineBreak()
        {
            stringBuilder.Append(DumpOptions.LineBreakChar);
            isNewLine = true;
        }

        protected void AddAlreadyTouched(object value)
        {
            var hashCode = GenerateHashCode(value);
            hashListOfFoundElements.Add(hashCode);
        }

        protected bool AlreadyTouched(object value)
        {
            if (value == null)
            {
                return false;
            }

            var hashCode = GenerateHashCode(value);
            for (var i = 0; i < hashListOfFoundElements.Count; i++)
            {
                if (hashListOfFoundElements[i] == hashCode)
                {
                    return true;
                }
            }

            return false;
        }

        private static int GenerateHashCode(object value)
        {
            return Combine(value.GetHashCode(), value.GetType().GetHashCode());
        }

        public static int Combine(int h1, int h2)
        {
            // RyuJIT optimizes this to use the ROL instruction
            // Related GitHub pull request: dotnet/coreclr#1830
            uint rol5 = (uint)h1 << 5 | (uint)h1 >> 27;
            return (int)rol5 + h1 ^ h2;
        }


        /// <summary>
        /// Converts the value of this instance to a <see cref="string"/>
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return stringBuilder.ToString();
        }
    }
}
