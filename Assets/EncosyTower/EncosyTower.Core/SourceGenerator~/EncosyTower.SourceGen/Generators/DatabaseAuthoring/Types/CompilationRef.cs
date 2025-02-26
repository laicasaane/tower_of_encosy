using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public struct CompilationRef
    {
        public Compilation compilation;
        public bool databaseAuthoring;
        public bool bakingSheet;

        public static CompilationRef GetCompilation(Compilation compilation, CancellationToken _)
        {
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

            return new CompilationRef {
                compilation = compilation,
                databaseAuthoring = databaseAuthoring,
                bakingSheet = bakingSheet,
            };
        }
    }
}
