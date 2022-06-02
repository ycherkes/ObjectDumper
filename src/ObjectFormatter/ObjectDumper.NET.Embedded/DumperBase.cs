using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ObjectFormatter.ObjectDumper.NET.Embedded
{
    public abstract class DumperBase : IDisposable
    {
        private readonly IndentedTextWriter _textWriter;
        private readonly Stack<object> _touchedObjects;
        private readonly StringBuilder _stringBuilder;
        private readonly StringWriter _stringWriter;
        private readonly bool _isOwner;

        protected DumperBase(DumpOptions dumpOptions)
        {
            DumpOptions = dumpOptions;
            _stringBuilder = new StringBuilder();
            _stringWriter = new StringWriter(_stringBuilder);
            _textWriter = new IndentedTextWriter(_stringWriter, new string(DumpOptions.IndentChar, DumpOptions.IndentSize));
            _touchedObjects = new Stack<object>();
            Level = 0;
            _isOwner = true;
        }

        protected abstract void FormatValue(object o);

        public int Level
        {
            get => _textWriter.Indent;
            set
            {
                if (value < 0)
                {
                    return;
                    throw new ArgumentException("Level must not be a negative number", nameof(Level));
                }

                _textWriter.Indent = value;
            }
        }

        protected bool IsMaxLevel()
        {
            return Level > DumpOptions.MaxLevel;
        }

        protected DumpOptions DumpOptions { get; }

        /// <summary>
        /// Writes value to underlying <see cref="StringBuilder"/> using <paramref name="indentLevel"/> and <see cref="DumpOptions.IndentChar"/> and <see cref="DumpOptions.IndentSize"/>
        /// to determine the indention chars prepended to <paramref name="value"/>
        /// </summary>
        /// <remarks>
        /// This function needs to keep up with if the last value written included the LineBreakChar at the end
        /// </remarks>
        /// <param name="value">string to be written</param>
        protected void Write(string value)
        {
            _textWriter.Write(value);
        }

        /// <summary>
        /// Writes a line break to underlying <see cref="StringBuilder"/> using <see cref="DumpOptions.LineBreakChar"/>
        /// </summary>
        /// <remarks>
        /// By definition this sets isNewLine to true
        /// </remarks>
        protected void LineBreak()
        {
            _textWriter.WriteLine();
        }

        protected void PushAlreadyTouched(object value)
        {
            _touchedObjects.Push(value);
        }

        protected void PopAlreadyTouched()
        {
            _touchedObjects.Pop();
        }

        protected bool AlreadyTouched(object value)
        {
            return value != null && _touchedObjects.Contains(value);
        }

        /// <summary>
        /// Converts the value of this instance to a <see cref="string"/>
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return _stringBuilder.ToString();
        }

        public void Dispose()
        {
            if (!_isOwner) return;
            _textWriter?.Dispose();
            _stringWriter?.Dispose();
        }
    }
}
