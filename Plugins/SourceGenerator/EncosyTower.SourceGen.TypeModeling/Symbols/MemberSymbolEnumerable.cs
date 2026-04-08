using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling
{
    public readonly struct FieldSymbolEnumerable
    {
        private readonly ImmutableArray<ISymbol> _members;

        internal FieldSymbolEnumerable(ImmutableArray<ISymbol> members)
        {
            _members = members;
        }

        public Enumerator GetEnumerator() => new(_members);

        public struct Enumerator
        {
            private readonly ImmutableArray<ISymbol> _members;
            private int _index;
            private FieldSymbol _current;

            internal Enumerator(ImmutableArray<ISymbol> members)
            {
                _members = members;
                _index = -1;
                _current = default;
            }

            public FieldSymbol Current => _current;

            public bool MoveNext()
            {
                _index++;

                while (_index < _members.Length)
                {
                    if (_members[_index] is IFieldSymbol field)
                    {
                        _current = new FieldSymbol(field);
                        return true;
                    }

                    _index++;
                }

                return false;
            }
        }
    }

    public readonly struct PropertySymbolEnumerable
    {
        private readonly ImmutableArray<ISymbol> _members;

        internal PropertySymbolEnumerable(ImmutableArray<ISymbol> members)
        {
            _members = members;
        }

        public Enumerator GetEnumerator() => new(_members);

        public struct Enumerator
        {
            private readonly ImmutableArray<ISymbol> _members;
            private int _index;
            private PropertySymbol _current;

            internal Enumerator(ImmutableArray<ISymbol> members)
            {
                _members = members;
                _index = -1;
                _current = default;
            }

            public PropertySymbol Current => _current;

            public bool MoveNext()
            {
                _index++;

                while (_index < _members.Length)
                {
                    if (_members[_index] is IPropertySymbol prop)
                    {
                        _current = new PropertySymbol(prop);
                        return true;
                    }

                    _index++;
                }

                return false;
            }
        }
    }

    public readonly struct MethodSymbolEnumerable
    {
        private readonly ImmutableArray<ISymbol> _members;

        internal MethodSymbolEnumerable(ImmutableArray<ISymbol> members)
        {
            _members = members;
        }

        public Enumerator GetEnumerator() => new(_members);

        public struct Enumerator
        {
            private readonly ImmutableArray<ISymbol> _members;
            private int _index;
            private MethodSymbol _current;

            internal Enumerator(ImmutableArray<ISymbol> members)
            {
                _members = members;
                _index = -1;
                _current = default;
            }

            public MethodSymbol Current => _current;

            public bool MoveNext()
            {
                _index++;

                while (_index < _members.Length)
                {
                    if (_members[_index] is IMethodSymbol method
                        && method.MethodKind == MethodKind.Ordinary)
                    {
                        _current = new MethodSymbol(method);
                        return true;
                    }

                    _index++;
                }

                return false;
            }
        }
    }

    public readonly struct EventSymbolEnumerable
    {
        private readonly ImmutableArray<ISymbol> _members;

        internal EventSymbolEnumerable(ImmutableArray<ISymbol> members)
        {
            _members = members;
        }

        public Enumerator GetEnumerator() => new(_members);

        public struct Enumerator
        {
            private readonly ImmutableArray<ISymbol> _members;
            private int _index;
            private EventSymbol _current;

            internal Enumerator(ImmutableArray<ISymbol> members)
            {
                _members = members;
                _index = -1;
                _current = default;
            }

            public EventSymbol Current => _current;

            public bool MoveNext()
            {
                _index++;

                while (_index < _members.Length)
                {
                    if (_members[_index] is IEventSymbol ev)
                    {
                        _current = new EventSymbol(ev);
                        return true;
                    }

                    _index++;
                }

                return false;
            }
        }
    }

    public readonly struct ConstructorSymbolEnumerable
    {
        private readonly ImmutableArray<ISymbol> _members;

        internal ConstructorSymbolEnumerable(ImmutableArray<ISymbol> members)
        {
            _members = members;
        }

        public Enumerator GetEnumerator() => new(_members);

        public struct Enumerator
        {
            private readonly ImmutableArray<ISymbol> _members;
            private int _index;
            private ConstructorSymbol _current;

            internal Enumerator(ImmutableArray<ISymbol> members)
            {
                _members = members;
                _index = -1;
                _current = default;
            }

            public ConstructorSymbol Current => _current;

            public bool MoveNext()
            {
                _index++;

                while (_index < _members.Length)
                {
                    if (_members[_index] is IMethodSymbol method
                        && method.MethodKind == MethodKind.Constructor)
                    {
                        _current = new ConstructorSymbol(method);
                        return true;
                    }

                    _index++;
                }

                return false;
            }
        }
    }
}
