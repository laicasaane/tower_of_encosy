using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Almost everything in this file was copied from Unity's source generators.
// Some minor modifications have been made for convenience
namespace EncosyTower.Modules.SourceGen
{
    public interface IPrintable
    {
        void Print(Printer printer);
    }

    public struct Printer
    {
        public const string NEWLINE = "\n";

        public StringBuilder builder;
        private int _currentIndentIndex;

        public string CurrentIndent => s_tabs[_currentIndentIndex];

        public int IndentDepth => _currentIndentIndex;

        // Array of indent string for each depth level starting with "" and up to 31 tabs.
        private static readonly string[] s_tabs = Enumerable.Range(start: 0, count: 32)
            .Select(i => string.Join("", Enumerable.Repeat(element: "\t", count: i)))
            .ToArray();

        public static Printer Default => new(0);

        public static Printer DefaultLarge => new(0, 1024 * 16);

        /// <summary>
        /// Get a copy of this printer with the same setting but new output.
        /// </summary>
        /// <param name="printer"></param>
        /// <returns></returns>
        public static Printer NewCopy(Printer printer) => new(printer._currentIndentIndex);

        public Printer(int indentCount)
        {
            System.Diagnostics.Debug.Assert(indentCount < s_tabs.Length);
            builder = new StringBuilder();
            _currentIndentIndex = indentCount;
        }
        public Printer(int indentCount, int capacity)
        {
            System.Diagnostics.Debug.Assert(indentCount < s_tabs.Length);
            builder = new StringBuilder(capacity);
            _currentIndentIndex = indentCount;
        }

        public Printer(StringBuilder builder, int indentCount)
        {
            System.Diagnostics.Debug.Assert(indentCount < s_tabs.Length);
            this.builder = builder;
            _currentIndentIndex = indentCount;
        }

        /// <summary>
        /// Allows to continue inline printing using a different printer
        /// </summary>
        /// <param name="printer"></param>
        /// <returns></returns>
        public Printer PrintWith(Printer printer) => printer;

        /// <summary>
        /// Allows to continue inline printing using the same printer from an function call
        /// </summary>
        /// <param name="printer"></param>
        /// <returns></returns>
        public Printer PrintWith(Func<Printer, Printer> func) => func(this);

        /// <summary>
        /// Creates a copy of this printer but with a relative indentCount
        /// </summary>
        /// <returns></returns>
        public Printer WithRelativeIndent(int indentCount) => new(builder, _currentIndentIndex + indentCount);

        public Printer RelativeIndent(int indentCount)
        {
            _currentIndentIndex += indentCount;
            return this;
        }

        /// <summary>
        /// Creates a copy of this printer but with a deeper indent
        /// </summary>
        /// <returns></returns>
        public Printer WithIncreasedIndent() => new(builder, _currentIndentIndex + 1);

        public Printer IncreasedIndent()
        {
            ++_currentIndentIndex;
            return this;
        }

        /// <summary>
        /// Creates a copy of this printer but with a shallower indent
        /// </summary>
        /// <returns></returns>
        public Printer WithDecreasedIndent() => new(builder, _currentIndentIndex - 1);

        public Printer DecreasedIndent()
        {
            --_currentIndentIndex;
            return this;
        }

        /// <summary>
        /// The current output of the printer.
        /// </summary>
        public string Result => builder.Replace("\r\n", NEWLINE).ToString();

        /// <summary>
        /// Clear the output of the printer and reset indent
        /// </summary>
        /// <returns></returns>
        public Printer Clear()
        {
            builder.Clear();
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
            System.Diagnostics.Debug.Assert(indentCount < s_tabs.Length);
            builder.Clear();
            _currentIndentIndex = indentCount;
            return this;
        }

        #region printing

        /// <summary>
        /// Print a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Printer Print(string text)
        {
            //DebugTrace.Write(text);
            builder.Append(text);
            return this;
        }

        /// <summary>
        /// Print indent
        /// </summary>
        /// <returns></returns>
        public Printer PrintBeginLine()
            => Print(CurrentIndent);

        /// <summary>
        /// Print indent and a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Printer PrintBeginLine(string text)
            => Print(CurrentIndent).Print(text);

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
            builder.Append(NEWLINE);
            return this;
        }

        /// <summary>
        /// Print a string and an end-line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Printer PrintEndLine(string text)
        {
            builder.Append(text).Append(NEWLINE);
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

        #endregion

        #region List

        /// <summary>
        /// Print item in a list with a separator
        /// </summary>
        public struct ListPrinter
        {
            public Printer printer;
            public string  separator;
            public bool    isStarted;

            public ListPrinter(Printer printer, string separator)
            {
                this.printer = printer;
                this.separator = separator;
                isStarted = false;
            }

            /// <summary>
            /// Get the next item printer
            /// </summary>
            /// <returns></returns>
            public Printer NextItemPrinter()
            {
                if (isStarted)
                    printer.Print(separator);
                else
                    isStarted = true;
                return printer;
            }

            /// <summary>
            /// Print all the element in an IEnumerable as list items
            /// </summary>
            /// <param name="elements"></param>
            /// <returns></returns>
            public ListPrinter PrintAll(IEnumerable<string> elements)
            {
                foreach (var e in elements)
                    NextItemPrinter().Print(e);
                return this;
            }

            /// <summary>
            /// Get a multiline list version of this list
            /// </summary>
            public MultilineListPrinter AsMultiline => new(this);

            /// <summary>
            /// Get a multiline list where each item is indented one level deeper
            /// </summary>
            public MultilineListPrinter AsMultilineIndented
            {
                get
                {
                    var printer                 = AsMultiline;
                    printer.listPrinter.printer = printer.listPrinter.printer.WithIncreasedIndent();
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
                if (listPrinter.isStarted)
                {
                    listPrinter.printer.PrintEndLine(listPrinter.separator);
                    listPrinter.printer.PrintBeginLine();
                }
                else
                    listPrinter.isStarted = true;
                return listPrinter.printer;
            }
        }

        /// <summary>
        /// Create a list printer from this printer
        /// </summary>
        /// <param name="separator"></param>
        /// <returns></returns>
        public ListPrinter AsListPrinter(string separator) => new(this, separator);
        #endregion

        #region Scope

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
            //PrintEndLine();
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

        #endregion

        /// <summary>
        /// Print a IPrintable to a string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="printable"></param>
        /// <returns></returns>
        public static string PrintToString<T>(T printable)
            where T : struct, IPrintable
        {
            var printer = Printer.Default;
            printable.Print(printer);
            return printer.builder.ToString();
        }

        /// <summary>
        /// Access debug printing interface
        /// </summary>
        public DebugPrinter Debug => new(this);
    }

    /// <summary>
    /// Printer used for debug purposes only.
    /// Some boxing may happen
    /// </summary>
    public struct DebugPrinter
    {
        public Printer basePrinter;

        public static implicit operator Printer(DebugPrinter dp) => dp.basePrinter;
        public DebugPrinter(Printer basePrinter)
        {
            this.basePrinter = basePrinter;
        }

        public DebugPrinter Print(string text)
        {
            basePrinter.Print(text);
            return this;
        }
        public DebugPrinter Print<T>(T printable)
            where T : struct, IPrintable
        {
            printable.Print(basePrinter);
            return this;
        }

        /// <summary>
        /// Print indent, a string and an end-line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public DebugPrinter PrintLine(string text)
        {
            basePrinter.PrintLine(text);
            return this;
        }

        /// <summary>
        /// Print indent and a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public DebugPrinter PrintBeginLine(string text)
        {
            basePrinter.PrintBeginLine(text);
            return this;
        }

        /// <summary>
        /// Print indent
        /// </summary>
        /// <returns></returns>
        public DebugPrinter PrintBeginLine()
        {
            basePrinter.PrintBeginLine();
            return this;
        }

        /// <summary>
        /// Print a string and an end-line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public DebugPrinter PrintEndLine(string text)
        {
            basePrinter.PrintEndLine(text);
            return this;
        }

        /// <summary>
        /// Print end-line
        /// </summary>
        /// <returns></returns>
        public DebugPrinter PrintEndLine()
        {
            basePrinter.PrintEndLine();
            return this;
        }

        /// <summary>
        /// Print a printable or "<null>" when null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="printable"></param>
        /// <returns></returns>
        public DebugPrinter PrintNullable<T>(T printable)
            where T : class, IPrintable
        {
            if (printable == null)
                Print("<null>");
            else
                printable.Print(basePrinter);
            return this;
        }

        /// <summary>
        /// Print:
        /// kv.Key = kv.Value
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="kv"></param>
        /// <returns></returns>
        public DebugPrinter Print<TKey, TValue>(KeyValuePair<TKey, TValue> kv)
            where TValue : struct, IPrintable
        {
            Print(kv.Key.ToString());
            Print(" = ");
            Print(kv.Value);
            return this;
        }

        /// <summary>
        /// Print: $"{key} = {value}"
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public DebugPrinter PrintKeyValue(string key, string value)
        {
            Print(key);
            Print(" = ");
            Print(value);
            return this;
        }

        /// <summary>
        /// Print $"{key} = {value}" where value is a printable
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public DebugPrinter PrintKeyValue<TValue>(string key, TValue value)
            where TValue : struct, IPrintable
        {
            basePrinter.Print(key);
            basePrinter.Print(" = ");
            Print(value);
            return this;
        }

        /// <summary>
        /// Print $"{key} = {value}" where value is a printable or "<null>" when null
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public DebugPrinter PrintKeyValueNullable<TValue>(string key, TValue value)
            where TValue : class, IPrintable
        {
            basePrinter.Print(key);
            basePrinter.Print(" = ");
            PrintNullable(value);
            return this;
        }

        /// <summary>
        /// Print:
        /// key = { TValue0, TValue1, ..., TValueN }
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="openScope"></param>
        /// <param name="closeScope"></param>
        /// <returns></returns>
        public DebugPrinter PrintKeyList<TValue>(string key, IEnumerable<TValue> value, string openScope = "{", string closeScope = "}")
            where TValue : struct, IPrintable
        {
            if (value == null)
            {
                PrintKeyValue(key, "<null>");
                return this;
            }
            Print(key);
            var scope = basePrinter.Print(" = IEnumerable<>").ScopePrinter(openScope);
            var list  = scope.PrintBeginLine().AsListPrinter(", ").AsMultiline;
            foreach (var v in value)
                new DebugPrinter(list.NextItemPrinter()).Print(v);
            scope.PrintEndLine();
            basePrinter.CloseScope(scope, closeScope);
            Print("}");
            return this;
        }

        /// <summary>
        /// Print:
        /// key = {
        ///     TKey0 = TValue0,
        ///     TKey1 = TValue1,
        ///     ... = ...,
        ///     TKeyN = TValueN
        /// }
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="openScope"></param>
        /// <param name="closeScope"></param>
        /// <returns></returns>
        public DebugPrinter PrintKeyList<TKey, TValue>(string key, IEnumerable<KeyValuePair<TKey, TValue>> value, string openScope = "{", string closeScope = "}")
            where TValue : struct, IPrintable
        {
            Print(key);
            var scope = basePrinter.Print(" = ").ScopePrinter(openScope);
            var list  = scope.PrintBeginLine().AsListPrinter(", ").AsMultiline;
            foreach (var v in value)
                new DebugPrinter(list.NextItemPrinter()).Print(v);
            scope.PrintEndLine();
            basePrinter.CloseScope(scope, closeScope);
            return this;
        }

        /// <summary>
        /// Print a multiline list of item with a given separator
        /// ex:
        ///     item0,
        ///     item1,
        ///     ...,
        ///     itemN,
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="separator"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public DebugPrinter PrintListMultiline<T>(string separator, IEnumerable<T> items)
            where T : struct, IPrintable
        {
            using var itor = items.GetEnumerator();
            if (itor.MoveNext())
                itor.Current.Print(basePrinter);

            while (itor.MoveNext())
            {
                PrintEndLine(separator);
                PrintBeginLine();
                itor.Current.Print(basePrinter);
            }
            return this;
        }
    }

    /// <summary>
    /// Printer that replicate the full scope path declaration (namespace, class and struct declarations) to a given SyntaxNode
    /// </summary>
    public struct SyntaxNodeScopePrinter
    {
        private SyntaxNode _leafNode;
        public Printer     printer;

        public SyntaxNodeScopePrinter(Printer printer, SyntaxNode node)
        {
            this.printer = printer;
            _leafNode = node;
        }

        public SyntaxNodeScopePrinter PrintScope(SyntaxNode node)
        {
            if (node.Parent != null)
                PrintScope(node.Parent);

            switch (node)
            {
                case NamespaceDeclarationSyntax namespaceSyntax:
                    printer.PrintBeginLine();
                    foreach (var m in namespaceSyntax.Modifiers)
                        printer.Print(m.ToString()).Print(" ");
                    printer = printer.Print("namespace ")
                        .PrintEndLine(namespaceSyntax.Name.ToString()).PrintLine("{").WithIncreasedIndent();
                    break;

                case InterfaceDeclarationSyntax interfaceSyntax:
                    printer.PrintBeginLine();
                    foreach (var m in interfaceSyntax.Modifiers)
                        printer.Print(m.ToString()).Print(" ");
                    printer = printer.Print("interface ")
                        .PrintEndLine(interfaceSyntax.Identifier.Text).PrintLine("{").WithIncreasedIndent();
                    break;

                case ClassDeclarationSyntax classSyntax:
                    printer.PrintBeginLine();
                    foreach (var m in classSyntax.Modifiers)
                        printer.Print(m.ToString()).Print(" ");
                    printer = printer.Print("class ")
                        .PrintEndLine(classSyntax.Identifier.Text).PrintLine("{").WithIncreasedIndent();
                    break;

                case StructDeclarationSyntax structSyntax:
                    printer.PrintBeginLine();
                    foreach (var m in structSyntax.Modifiers)
                        printer.Print(m.ToString()).Print(" ");
                    printer = printer.Print("struct ")
                        .PrintEndLine(structSyntax.Identifier.Text).PrintLine("{").WithIncreasedIndent();
                    break;

                case RecordDeclarationSyntax recordSyntax:
                    printer.PrintBeginLine();
                    foreach (var m in recordSyntax.Modifiers)
                        printer.Print(m.ToString()).Print(" ");
                    printer = printer.Print("record ")
                        .PrintIf(recordSyntax.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword), "struct ", "class ")
                        .PrintEndLine(recordSyntax.Identifier.Text).PrintLine("{").WithIncreasedIndent();
                    break;
            }

            return this;
        }

        public SyntaxNodeScopePrinter PrintOpen() => PrintScope(_leafNode);

        public SyntaxNodeScopePrinter PrintClose()
        {
            var parent = _leafNode;
            while (parent != null)
            {
                switch (parent)
                {
                    case NamespaceDeclarationSyntax _:
                    case InterfaceDeclarationSyntax _:
                    case ClassDeclarationSyntax _:
                    case StructDeclarationSyntax _:
                    case RecordDeclarationSyntax _:
                        printer = printer.WithDecreasedIndent().PrintLine("}");
                        break;
                }
                parent = parent.Parent;
            }
            return this;
        }
    }

    /// <summary>
    /// Printer that replicate the full scope path declaration (namespace, class and struct declarations) to a given ISymbol
    /// </summary>
    public struct SymbolScopePrinter
    {
        private ISymbol _symbol;
        public Printer     printer;

        public SymbolScopePrinter(Printer printer, ISymbol symbol)
        {
            this.printer = printer;
            _symbol = symbol;
        }

        public SymbolScopePrinter PrintScope(ISymbol symbol)
        {
            PrintScopeContainingNamespace(symbol.ContainingNamespace);
            PrintScopeContainingType(symbol.ContainingType);
            return this;
        }

        private SymbolScopePrinter PrintScopeContainingNamespace(INamespaceSymbol symbol)
        {
            if (symbol != null && string.IsNullOrWhiteSpace(symbol.Name) == false)
            {
                printer.PrintBeginLine().Print("namespace ");
                PrintNamespacePart(ref printer, symbol, symbol);
                printer = printer.PrintEndLine().PrintLine("{").WithIncreasedIndent();
            }

            return this;

            static void PrintNamespacePart(ref Printer p, INamespaceSymbol ns, INamespaceSymbol root)
            {
                if (ns != null && string.IsNullOrWhiteSpace(ns.Name) == false)
                {
                    if (ns.ContainingNamespace != null)
                    {
                        PrintNamespacePart(ref p, ns.ContainingNamespace, root);
                    }

                    p.Print(ns.Name);

                    if (ns.Name != root.Name)
                    {
                        p.Print(".");
                    }
                }
            }
        }

        private SymbolScopePrinter PrintScopeContainingType(INamedTypeSymbol symbol)
        {
            if (symbol != null && string.IsNullOrWhiteSpace(symbol.Name) == false)
            {
                if (symbol.ContainingType != null)
                {
                    PrintScopeContainingType(symbol.ContainingType);
                }

                printer.PrintBeginLine().Print("partial ");

                if (symbol.TypeKind == TypeKind.Struct)
                    printer.Print("struct ");
                else
                    printer.Print("class ");

                printer = printer.PrintEndLine(symbol.Name).PrintLine("{").WithIncreasedIndent();
            }

            return this;
        }

        public SymbolScopePrinter PrintOpen() => PrintScope(_symbol);

        public SymbolScopePrinter PrintClose()
        {
            ISymbol parent = _symbol.ContainingType;

            while (parent != null && string.IsNullOrEmpty(parent.Name) == false)
            {
                printer = printer.WithDecreasedIndent().PrintLine("}");
                parent = parent.ContainingType;
            }

            parent = _symbol.ContainingNamespace;

            if (parent != null && string.IsNullOrEmpty(parent.Name) == false)
            {
                printer = printer.WithDecreasedIndent().PrintLine("}");
            }

            return this;
        }
    }
}

