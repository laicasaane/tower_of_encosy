using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct AuthoringCompilationInfo : System.IEquatable<AuthoringCompilationInfo>
    {
        public bool databaseAuthoring;
        public bool bakingSheet;
        public CompilationCandidateSlim compilation;

        public readonly bool IsValid => compilation.isValid;

        public static AuthoringCompilationInfo GetCompilation(Compilation compilation, CancellationToken _)
        {
            if (compilation == null)
            {
                return default;
            }

            var compilationSlim = CompilationCandidateSlim.GetCompilation(
                  compilation
                , Helpers.DATABASES_NAMESPACE
                , Helpers.SKIP_ATTRIBUTE
            );

            var databaseAuthoring = false;
            var bakingSheet = false;

            foreach (var assembly in compilation.ReferencedAssemblyNames)
            {
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

            return new AuthoringCompilationInfo {
                databaseAuthoring = databaseAuthoring,
                bakingSheet       = bakingSheet,
                compilation       = compilationSlim,
            };
        }

        public readonly bool Equals(AuthoringCompilationInfo other)
            => databaseAuthoring == other.databaseAuthoring
            && bakingSheet == other.bakingSheet
            && compilation.Equals(other.compilation)
            ;

        public override readonly bool Equals(object obj)
            => obj is AuthoringCompilationInfo other && Equals(other);

        public override readonly int GetHashCode()
            => HashValue.Combine(
                  databaseAuthoring
                , bakingSheet
                , compilation
            );
    }
}
