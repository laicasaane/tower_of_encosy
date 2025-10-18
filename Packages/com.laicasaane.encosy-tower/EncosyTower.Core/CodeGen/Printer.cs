using System;
using System.Collections.Generic;
using System.Text;

namespace EncosyTower.CodeGen
{
    public struct Printer
    {
        public const char NEWLINE = '\n';
        public const string INDENT = "    ";

        private readonly StringBuilder _builder;

        private int _currentIndentIndex;

        public Printer()
        {
            _builder = new StringBuilder();
            _currentIndentIndex = 0;
        }

        public Printer(int indentCount)
        {
            _builder = new StringBuilder();
            _currentIndentIndex = indentCount;
        }

        public Printer(int indentCount, int capacity)
        {
            _builder = new StringBuilder(capacity);
            _currentIndentIndex = indentCount;
        }

        public Printer(StringBuilder builder)
        {
            _builder = builder;
            _currentIndentIndex = 0;
        }

        public Printer(StringBuilder builder, int indentCount)
        {
            _builder = builder;
            _currentIndentIndex = indentCount;
        }

        public static Printer Default => new(0);

        public static Printer DefaultLarge => new(0, 1024 * 16);

        public readonly bool IsCreated => _builder != null;

        public readonly int IndentDepth => _currentIndentIndex;

        /// <summary>
        /// Allows to continue inline printing using a different printer
        /// </summary>
        /// <param name="printer"></param>
        /// <returns></returns>
        public readonly Printer PrintWith(Printer printer) => printer;

        /// <summary>
        /// Allows to continue inline printing using the same printer from an function call
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public readonly Printer PrintWith(Func<Printer, Printer> func)
        {
            return func?.Invoke(this) ?? this;
        }

        /// <summary>
        /// Creates a copy of this printer but with a relative indentCount
        /// </summary>
        /// <returns></returns>
        public readonly Printer WithRelativeIndent(int indentCount) => new(_builder, _currentIndentIndex + indentCount);

        public Printer RelativeIndent(int indentCount)
        {
            _currentIndentIndex += indentCount;
            return this;
        }

        /// <summary>
        /// Creates a copy of this printer but with a deeper indent
        /// </summary>
        /// <returns></returns>
        public readonly Printer WithIncreasedIndent() => new(_builder, _currentIndentIndex + 1);

        public Printer IncreasedIndent()
        {
            ++_currentIndentIndex;
            return this;
        }

        /// <summary>
        /// Creates a copy of this printer but with a shallower indent
        /// </summary>
        /// <returns></returns>
        public readonly Printer WithDecreasedIndent() => new(_builder, _currentIndentIndex - 1);

        public Printer DecreasedIndent()
        {
            --_currentIndentIndex;
            return this;
        }

        /// <summary>
        /// The current output of the printer.
        /// </summary>
        public readonly string Result
            => _builder.Replace('\r', NEWLINE).ToString();

        /// <summary>
        /// Clear the output of the printer and reset indent
        /// </summary>
        /// <returns></returns>
        public Printer Clear()
        {
            _builder.Clear();
            _currentIndentIndex = 0;
            return this;
        }

        /// <summary>
        /// Clear the output and set the current indent
        /// </summary>
        /// <param name="indentCount"></param>
        /// <returns></returns>
        public Printer ClearAndIndent(int indentCount)
        {
            _builder.Clear();
            _currentIndentIndex = indentCount;
            return this;
        }

        /// <summary>
        /// Print a character
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public readonly Printer Print(char character)
        {
            _builder.Append(character);
            return this;
        }

        /// <summary>
        /// Print a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public readonly Printer Print(ReadOnlySpan<char> text)
        {
            _builder.Append(text);
            return this;
        }

        /// <summary>
        /// Print a string from another printer
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public readonly Printer Print(Printer text)
        {
            _builder.Append(text._builder);
            return this;
        }

        /// <summary>
        /// Print a character multiple times
        /// </summary>
        /// <param name="character"></param>
        /// <param name="repeatCount"></param>
        /// <returns></returns>
        public readonly Printer Print(char character, int repeatCount)
        {
            _builder.Append(character, repeatCount);
            return this;
        }

        /// <summary>
        /// Print a character if condition is true
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        public readonly Printer Print(char character, bool condition)
        {
            if (condition)
                Print(character);

            return this;
        }

        /// <summary>
        /// Print a string if condition is true
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public readonly Printer Print(ReadOnlySpan<char> text, bool condition)
        {
            if (condition)
                Print(text);

            return this;
        }

        /// <summary>
        /// Print a character
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="trueCharacter"></param>
        /// <param name="falseCharacter"></param>
        /// <returns></returns>
        public readonly Printer PrintSelect(char trueCharacter, char falseCharacter, bool condition)
        {
            Print(condition ? trueCharacter : falseCharacter);
            return this;
        }

        /// <summary>
        /// Print a string
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="trueText"></param>
        /// <param name="falseText"></param>
        /// <returns></returns>
        public readonly Printer PrintSelect(ReadOnlySpan<char> trueText, ReadOnlySpan<char> falseText, bool condition)
        {
            Print(condition ? trueText : falseText);
            return this;
        }

        /// <summary>
        /// Print a character multiple times if condition is true
        /// </summary>
        /// <param name="character"></param>
        /// <param name="repeatCount"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public readonly Printer Print(char character, int repeatCount, bool condition)
        {
            if (condition)
                Print(character, repeatCount);

            return this;
        }

        /// <summary>
        /// Print indent
        /// </summary>
        /// <returns></returns>
        public readonly Printer PrintBeginLine()
        {
            var depth = IndentDepth;

            for (var i = 0; i < depth; i++)
            {
                Print(INDENT);
            }

            return this;
        }

        /// <summary>
        /// Print indent and a character
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public readonly Printer PrintBeginLine(char character)
            => PrintBeginLine().Print(character);

        /// <summary>
        /// Print indent and a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public readonly Printer PrintBeginLine(ReadOnlySpan<char> text)
            => PrintBeginLine().Print(text);

        /// <summary>
        /// Print indent and a string if condition is true
        /// </summary>
        /// <param name="text"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public readonly Printer PrintBeginLine(ReadOnlySpan<char> text, bool condition)
        {
            if (condition)
                PrintBeginLine(text);

            return this;
        }

        /// <summary>
        /// Print indent and a string
        /// </summary>
        /// <param name="trueText"></param>
        /// <param name="falseText"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public readonly Printer PrintBeginLineSelect(ReadOnlySpan<char> trueText, ReadOnlySpan<char> falseText, bool condition)
        {
            PrintBeginLine(condition ? trueText : falseText);
            return this;
        }

        /// <summary>
        /// Print indent and a string
        /// </summary>
        /// <param name="character"></param>
        /// <param name="repeatCount"></param>
        /// <returns></returns>
        public readonly Printer PrintBeginLine(char character, int repeatCount)
            => PrintBeginLine().Print(character, repeatCount);

        /// <summary>
        /// Print indent and a string from another printer
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public readonly Printer PrintBeginLine(Printer text)
        {
            PrintBeginLine().Print(text);
            return this;
        }

        /// <summary>
        /// Print end-line
        /// </summary>
        /// <returns></returns>
        public readonly Printer PrintEndLine()
        {
            _builder.Append(NEWLINE);
            return this;
        }

        /// <summary>
        /// Print a string and an end-line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public readonly Printer PrintEndLine(ReadOnlySpan<char> text)
        {
            _builder.Append(text).Append(NEWLINE);
            return this;
        }

        /// <summary>
        /// Print a character multiple times, and an end-line
        /// </summary>
        /// <param name="character"></param>
        /// <param name="repeatCount"></param>
        /// <returns></returns>
        public readonly Printer PrintEndLine(char character, int repeatCount)
        {
            _builder.Append(character, repeatCount).Append(NEWLINE);
            return this;
        }

        /// <summary>
        /// Print a string from another printer, and an end-line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public readonly Printer PrintEndLine(Printer text)
        {
            _builder.Append(text._builder).Append(NEWLINE);
            return this;
        }

        /// <summary>
        /// Print indent, a string and an end-line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public readonly Printer PrintLine(ReadOnlySpan<char> text)
            => PrintBeginLine().PrintEndLine(text);

        /// <summary>
        /// Print indent, a character multiple times, and an end-line
        /// </summary>
        /// <param name="character"></param>
        /// <param name="repeatCount"></param>
        /// <returns></returns>
        public readonly Printer PrintLine(char character, int repeatCount)
            => PrintBeginLine().PrintEndLine(character, repeatCount);

        /// <summary>
        /// Print indent, a string and an end-line if a condition is true
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public readonly Printer PrintLine(ReadOnlySpan<char> text, bool condition)
        {
            if (condition)
                PrintLine(text);

            return this;
        }

        /// <summary>
        /// Print indent, a string from another printer, and an end-line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public readonly Printer PrintLine(Printer text)
            => PrintBeginLine().PrintEndLine(text);

        /// <summary>
        /// Print indent, a string, and an end-line
        /// </summary>
        /// <param name="trueText"></param>
        /// <param name="falseText"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public readonly Printer PrintLineSelect(ReadOnlySpan<char> trueText, ReadOnlySpan<char> falseText, bool condition)
        {
            PrintLine(condition ? trueText : falseText);
            return this;
        }

        /// <summary>
        /// Print indent, a character multiple times, and an end-line if a condition is true
        /// </summary>
        /// <param name="character"></param>
        /// <param name="repeatCount"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public readonly Printer PrintLine(char character, int repeatCount, bool condition)
        {
            if (condition)
                PrintLine(character, repeatCount);

            return this;
        }

        /// <summary>
        /// Print a string and an end-line if the condition is true
        /// </summary>
        /// <param name="text"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public readonly Printer PrintEndLine(ReadOnlySpan<char> text, bool condition)
        {
            if (condition)
                PrintEndLine(text);

            return this;
        }

        /// <summary>
        /// Print a string, and an end-line
        /// </summary>
        /// <param name="trueText"></param>
        /// <param name="falseText"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public readonly Printer PrintEndLineSelect(ReadOnlySpan<char> trueText, ReadOnlySpan<char> falseText, bool condition)
        {
            PrintEndLine(condition ? trueText : falseText);
            return this;
        }

        /// <summary>
        /// Create a list printer from this printer
        /// </summary>
        /// <param name="separator"></param>
        /// <returns></returns>
        public readonly ListPrinter AsListPrinter(string separator)
            => new(this, separator);

        /// <summary>
        /// Print the scope open string and return a new printer with deeper indent.
        /// The returned printer must be terminated with CloseScope.
        /// </summary>
        /// <param name="scopeOpen"></param>
        /// <returns>a copy of this printer with a deeper indent</returns>
        public readonly Printer ScopePrinter(string scopeOpen = "{")
        {
            PrintEndLine(scopeOpen);
            return WithIncreasedIndent();
        }

        /// <summary>
        /// Close a scope printer and print a string.
        /// </summary>
        /// <param name="scopedPrinter">the scope printer to close</param>
        /// <param name="scopeClose"></param>
        /// <returns>a copy of the closed scope printer with a shallower indent</returns>
        public readonly Printer CloseScope(Printer scopedPrinter, string scopeClose = "{")
        {
            PrintBeginLine(scopeClose);
            return scopedPrinter.WithDecreasedIndent();
        }

        /// <summary>
        /// Print the scope open string and increase indent.
        /// </summary>
        /// <param name="scopeOpen"></param>
        /// <returns>this</returns>
        public Printer OpenScope(string scopeOpen = "{")
        {
            PrintLine(scopeOpen);
            IncreasedIndent();
            return this;
        }

        /// <summary>
        /// Decrease indent and print the scope close string.
        /// </summary>
        /// <param name="scopeClose"></param>
        /// <returns>this</returns>
        public Printer CloseScope(string scopeClose = "}")
        {
            DecreasedIndent();
            PrintLine(scopeClose);
            return this;
        }

        /// <summary>
        /// Print item in a list with a separator
        /// </summary>
        public struct ListPrinter
        {
            internal Printer _printer;
            internal readonly string  _separator;
            internal bool _started;

            public ListPrinter(Printer printer, string separator)
            {
                _printer = printer;
                _separator = separator;
                _started = false;
            }

            /// <summary>
            /// Get the next item printer
            /// </summary>
            /// <returns></returns>
            public Printer NextItemPrinter()
            {
                if (_started)
                    _printer.Print(_separator);
                else
                    _started = true;

                return _printer;
            }

            /// <summary>
            /// Print all the element in an IEnumerable as list items
            /// </summary>
            /// <param name="elements"></param>
            /// <returns></returns>
            public ListPrinter PrintAll(IEnumerable<string> elements)
            {
                if (elements == null)
                {
                    return this;
                }

                foreach (var e in elements)
                {
                    NextItemPrinter().Print(e);
                }

                return this;
            }

            /// <summary>
            /// Get a multiline list version of this list
            /// </summary>
            public readonly MultilineListPrinter AsMultiline => new(this);

            /// <summary>
            /// Get a multiline list where each item is indented one level deeper
            /// </summary>
            public readonly MultilineListPrinter AsMultilineIndented
            {
                get
                {
                    var printer                  = AsMultiline;
                    printer.listPrinter._printer = printer.listPrinter._printer.WithIncreasedIndent();
                    return printer;
                }
            }
        }

        /// <summary>
        /// Print item in a multi-line list such as:
        /// "item0, item1, ..., itemN"
        /// </summary>
        public struct MultilineListPrinter
        {
            public ListPrinter listPrinter;

            public MultilineListPrinter(ListPrinter listPrinter)
            {
                this.listPrinter = listPrinter;
            }

            /// <summary>
            /// Get the next item printer
            /// </summary>
            /// <returns></returns>
            public Printer NextItemPrinter()
            {
                if (listPrinter._started)
                {
                    listPrinter._printer.PrintEndLine(listPrinter._separator);
                    listPrinter._printer.PrintBeginLine();
                }
                else
                {
                    listPrinter._started = true;
                }

                return listPrinter._printer;
            }
        }
    }
}
