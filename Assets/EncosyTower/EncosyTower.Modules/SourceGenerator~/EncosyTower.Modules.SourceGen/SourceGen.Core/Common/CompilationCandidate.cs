using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.SourceGen
{
    public struct CompilationCandidate
    {
        public Compilation compilation;
        public References references;
        public bool enableNullable;

        public static CompilationCandidate GetCompilation(Compilation compilation, CancellationToken token)
        {
            var references = new References();

            foreach (var assembly in compilation.ReferencedAssemblyNames)
            {
                if (assembly.Name.StartsWith("Sirenix.OdinInspector.Attributes"))
                {
                    references.odin = true;
                    continue;
                }

                if (assembly.Name.StartsWith("UnityEngine"))
                {
                    references.unity = true;
                    continue;
                }

                if (assembly.Name.StartsWith("Unity.Collections"))
                {
                    references.unityCollections = true;
                    continue;
                }
            }

            return new CompilationCandidate {
                compilation = compilation,
                references = references,
                enableNullable = compilation.Options.NullableContextOptions != NullableContextOptions.Disable,
            };
        }
    }

    public struct References
    {
        public bool odin;
        public bool unity;
        public bool unityCollections;
    }
}
