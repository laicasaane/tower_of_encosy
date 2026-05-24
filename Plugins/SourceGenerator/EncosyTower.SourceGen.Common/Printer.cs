// com.unity.entities © 2024 Unity Technologies
//
// Licensed under the Unity Companion License for Unity-dependent projects
// (see https://unity3d.com/legal/licenses/unity_companion_license).
//
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS”
// BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.
//
// Please review the license for details on these and other terms and conditions.

using System;
using System.Text;

// Almost everything in this file was copied from Unity's source generators.
// Some minor modifications have been made for convenience
namespace EncosyTower.SourceGen
{
    public delegate void PrinterAction(ref Printer printer);

    public struct Printer
    {
        public const string NEWLINE = "\n";
        public const string INDENT = "    ";

        private readonly StringBuilder _builder;

        private int _currentIndentIndex;

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

        public Printer(StringBuilder builder, int indentCount)
        {
            _builder = builder;
            _currentIndentIndex = indentCount;
        }

        public static Printer Default => new(0);

        public static Printer DefaultLarge => new(0, 1024 * 16);

        public int IndentDepth => _currentIndentIndex;

        /// <summary>
        /// Get a copy of this printer with the same setting but new output.
        /// </summary>
        /// <param name="printer"></param>
        /// <returns></returns>
        public static Printer NewCopy(Printer printer) => new(printer._currentIndentIndex);

        /// <summary>
        /// Allows to continue inline printing using a different printer
        /// </summary>
        /// <param name="printer"></param>
        /// <returns></returns>
        public Printer PrintWith(Printer printer) => printer;

        /// <summary>
        /// Allows to continue inline printing using the same printer from an function call
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public Printer PrintWith(Func<Printer, Printer> func) => func(this);

        /// <summary>
        /// Creates a copy of this printer but with a relative indentCount
        /// </summary>
        /// <returns></returns>
        public Printer WithRelativeIndent(int indentCount) => new(_builder, _currentIndentIndex + indentCount);

        public Printer RelativeIndent(int indentCount)
        {
            _currentIndentIndex += indentCount;
            return this;
        }

        /// <summary>
        /// Creates a copy of this printer but with a deeper indent
        /// </summary>
        /// <returns></returns>
        public Printer WithIncreasedIndent() => new(_builder, _currentIndentIndex + 1);

        public Printer IncreasedIndent()
        {
            ++_currentIndentIndex;
            return this;
        }

        /// <summary>
        /// Creates a copy of this printer but with a shallower indent
        /// </summary>
        /// <returns></returns>
        public Printer WithDecreasedIndent() => new(_builder, _currentIndentIndex - 1);

        public Printer DecreasedIndent()
        {
            --_currentIndentIndex;
            return this;
        }

        /// <summary>
        /// The current output of the printer.
        /// </summary>
        public string Result => _builder.Replace("\r\n", NEWLINE).ToString();

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
        /// Print a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Printer Print(bool value)
        {
            _builder.Append(value ? "true" : "false");
            return this;
        }

        /// <summary>
        /// Print a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Printer Print(sbyte value)
        {
            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Print a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Printer Print(byte value)
        {
            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Print a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Printer Print(char value)
        {
            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Print a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Printer Print(short value)
        {
            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Print a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Printer Print(ushort value)
        {
            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Print a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Printer Print(int value)
        {
            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Print a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Printer Print(uint value)
        {
            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Print a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Printer Print(float value)
        {
            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Print a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Printer Print(double value)
        {
            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Print a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Printer Print(long value)
        {
            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Print a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Printer Print(ulong value)
        {
            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Print a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Printer Print(string text)
        {
            if (string.IsNullOrEmpty(text) == false)
            {
                _builder.Append(text);
            }

            return this;
        }

        /// <summary>
        /// Print a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Printer Print(StringBuilder text)
        {
            if (text != null)
            {
                _builder.Append(text);
            }

            return this;
        }

        /// <summary>
        /// Print a string
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public Printer PrintRepeat(char ch, int repeatCount)
        {
            if (repeatCount > 0)
            {
                _builder.Append(ch, repeatCount);
            }

            return this;
        }

        /// <summary>
        /// Print indent
        /// </summary>
        /// <returns></returns>
        public Printer PrintBeginLine()
        {
            var depth = IndentDepth;

            for (var i = 0; i < depth; i++)
            {
                Print(INDENT);
            }

            return this;
        }

        /// <summary>
        /// Print indent and a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Printer PrintBeginLine(string text)
            => PrintBeginLine().Print(text);

        public Printer PrintBeginLineIf(bool condition, string trueText)
        {
            if (condition) PrintBeginLine(trueText);
            return this;
        }

        public Printer PrintBeginLineIf(bool condition, string trueText, string falseText)
        {
            if (condition) PrintBeginLine(trueText);
            else PrintBeginLine(falseText);
            return this;
        }

        /// <summary>
        /// Print end-line
        /// </summary>
        /// <returns></returns>
        public Printer PrintEndLine()
        {
            _builder.Append(NEWLINE);
            return this;
        }

        /// <summary>
        /// Print a string and an end-line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Printer PrintEndLine(string text)
        {
            if (string.IsNullOrEmpty(text) == false)
            {
                _builder.Append(text);
            }

            _builder.Append(NEWLINE);
            return this;
        }

        public Printer PrintEndLineIf(bool condition, string trueText)
        {
            if (condition) PrintEndLine(trueText);
            return this;
        }

        public Printer PrintEndLineIf(bool condition, string trueText, string falseText)
        {
            if (condition) PrintEndLine(trueText);
            else PrintEndLine(falseText);
            return this;
        }

        /// <summary>
        /// Print indent, a string and an end-line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Printer PrintLine(string text)
            => PrintBeginLine().PrintEndLine(text);

        public Printer PrintLine(string text, object arg1)
            => PrintBeginLine().PrintEndLine(string.Format(text, arg1));

        public Printer PrintLine(string text, object arg1, object arg2)
            => PrintBeginLine().PrintEndLine(string.Format(text, arg1, arg2));

        /// <summary>
        /// Print a string if condition is truw
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="trueText"></param>
        /// <returns></returns>
        public Printer PrintIf(bool condition, string trueText)
        {
            if (condition) Print(trueText);
            return this;
        }

        public Printer PrintIf(bool condition, string trueText, string falseText)
        {
            if (condition) Print(trueText);
            else Print(falseText);
            return this;
        }

        /// <summary>
        /// Print indent, a string and an end-line if a condition is true
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="trueText"></param>
        /// <returns></returns>
        public Printer PrintLineIf(bool condition, string trueText)
        {
            if (condition) PrintLine(trueText);
            return this;
        }

        public Printer PrintLineIf(bool condition, string trueText, string falseText)
        {
            if (condition) PrintLine(trueText);
            else PrintLine(falseText);
            return this;
        }

        public Printer PrintLineIf(bool condition, string trueText, object trueArg1)
        {
            if (condition) PrintLine(trueText, trueArg1);
            return this;
        }

        public Printer PrintLineIf(bool condition, string trueText, object trueArg1, object trueArg2)
        {
            if (condition) PrintLine(trueText, trueArg1, trueArg2);
            return this;
        }

        public Printer PrintLineIf(bool condition, string trueText, object trueArg1, string falseText, object falseArg1)
        {
            if (condition) PrintLine(trueText, trueArg1);
            else PrintLine(falseText, falseArg1);
            return this;
        }

        public Printer PrintLineIf(bool condition, string trueText, object trueArg1, object trueArg2, string falseText, object falseArg1, object falseArg2)
        {
            if (condition) PrintLine(trueText, trueArg1, trueArg2);
            else PrintLine(falseText, falseArg1, falseArg2);
            return this;
        }

        public Printer PrintCallerDebug(
#if DEBUG
              [System.Runtime.CompilerServices.CallerLineNumber] int l = default
            , [System.Runtime.CompilerServices.CallerMemberName] string m = default
            , [System.Runtime.CompilerServices.CallerFilePath] string f = default
#endif
        )
        {
#if DEBUG
            PrintLine($"// [{l}] :: {m} :: {f}");
#endif

            return this;
        }

        /// <summary>
        /// print the scope open string and return a new printer with deeper indent
        /// The returned printer must be terminated with CloseScope.
        /// </summary>
        /// <param name="scopeOpen"></param>
        /// <returns>a copy of this printer with a deeper indent</returns>
        public Printer ScopePrinter(string scopeOpen = "{")
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
        public Printer CloseScope(Printer scopedPrinter, string scopeClose = "{")
        {
            PrintBeginLine(scopeClose);
            return scopedPrinter.WithDecreasedIndent();
        }

        /// <summary>
        /// print the scope open string and increase indent
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
    }
}

