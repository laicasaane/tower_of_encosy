using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct CompilationSpec : System.IEquatable<CompilationSpec>
    {
        public bool databaseAuthoring;
        public bool bakingSheet;
        public CompilationInfo compilation;

        public readonly bool IsValid => compilation.isValid;

        public static CompilationSpec GetCompilation(Compilation compilation, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (compilation == null)
            {
                return default;
            }

            var compilationSlim = CompilationInfo.GetCompilation(
                  compilation
                , token
                , Helpers.DATABASES_NAMESPACE
                , Helpers.SKIP_ATTRIBUTE
            );

            var databaseAuthoring = false;
            var bakingSheet = false;

            foreach (var assembly in compilation.ReferencedAssemblyNames)
            {
                token.ThrowIfCancellationRequested();

                if (assembly.Name is Helpers.DATABASES_AUTHORING_NAMESPACE)
                {
                    databaseAuthoring = true;
                    continue;
                }

                if (assembly.Name is "BakingSheet")
                {
                    bakingSheet = true;
                    continue;
                }
            }

            return new CompilationSpec {
                databaseAuthoring = databaseAuthoring,
                bakingSheet = bakingSheet,
                compilation = compilationSlim,
            };
        }

        public readonly bool Equals(CompilationSpec other)
            => databaseAuthoring == other.databaseAuthoring
            && bakingSheet == other.bakingSheet
            && compilation.Equals(other.compilation)
            ;

        public override readonly bool Equals(object obj)
            => obj is CompilationSpec other && Equals(other);

        public override readonly int GetHashCode()
            => HashValue.Combine(
                  databaseAuthoring
                , bakingSheet
                , compilation
            );
    }
}
