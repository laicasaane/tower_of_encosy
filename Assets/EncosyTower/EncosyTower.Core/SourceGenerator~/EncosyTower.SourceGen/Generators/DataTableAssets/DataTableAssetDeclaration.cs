using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.DataTableAssets
{
    public partial class DataTableAssetDeclaration
    {
        public DataTableAssetRef TypeRef { get; }

        public bool GetIdMethodIsImplemented { get; }

        public bool InitializeMethodIsImplemented { get; }

        public bool ConvertIdMethodIsImplemented { get; }

        public DataTableAssetDeclaration(DataTableAssetRef candidate)
        {
            TypeRef = candidate;

            var members = candidate.Symbol.GetMembers();
            var comparer = SymbolEqualityComparer.Default;

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method)
                {
                    continue;
                }

                var name = method.Name;
                var parameters = method.Parameters;
                var parametersLength = parameters.Length;

                if (name == "GetId"
                    && parametersLength == 1
                    && parameters[0].RefKind == RefKind.In
                    && comparer.Equals(parameters[0].Type, TypeRef.DataType)
                )
                {
                    GetIdMethodIsImplemented = true;
                    continue;
                }

                if (name == "Initialize"
                    && parametersLength == 1
                    && parameters[0].RefKind == RefKind.Ref
                    && comparer.Equals(parameters[0].Type, TypeRef.DataType)
                )
                {
                    InitializeMethodIsImplemented = true;
                    continue;
                }

                if (name == "ConvertId"
                    && parametersLength == 1
                    && parameters[0].RefKind == RefKind.None
                    && comparer.Equals(parameters[0].Type, TypeRef.IdType)
                )
                {
                    ConvertIdMethodIsImplemented = true;
                    continue;
                }
            }
        }
    }
}
