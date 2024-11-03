using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.Data.SourceGen
{
    public partial class DataTableAssetDeclaration
    {
        public DataTableAssetRef TypeRef { get; }

        public bool GetIdMethodIsImplemented { get; }

        public DataTableAssetDeclaration(DataTableAssetRef candidate)
        {
            TypeRef = candidate;

            var members = candidate.Symbol.GetMembers();

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method)
                {
                    continue;
                }

                if (method.Name == "GetId"
                    && method.Parameters.Length == 1
                    && method.Parameters[0].RefKind == RefKind.In
                    && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, TypeRef.DataType)
                )
                {
                    GetIdMethodIsImplemented = true;
                    break;
                }
            }
        }
    }
}
