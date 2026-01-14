using System;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators
{
    public struct CompilationCandidate : IEquatable<CompilationCandidate>
    {
        public Compilation compilation;
        public string assemblyName;
        public References references;
        public bool enableNullable;
        public bool isValid;

        public static CompilationCandidate GetCompilation(Compilation compilation, CancellationToken _)
        {
            return new CompilationCandidate {
                compilation = compilation,
                assemblyName = compilation.Assembly.Name,
                references = References.Create(compilation),
                enableNullable = compilation.Options.NullableContextOptions != NullableContextOptions.Disable,
            };
        }

        public readonly override bool Equals(object obj)
            => obj is CompilationCandidate other && Equals(other);

        public readonly bool Equals(CompilationCandidate other)
            => string.Equals(assemblyName, other.assemblyName, StringComparison.Ordinal)
            && references.Equals(other.references)
            && enableNullable == other.enableNullable
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(assemblyName, references, enableNullable);
    }

    public struct CompilationCandidateSlim : IEquatable<CompilationCandidateSlim>
    {
        public string assemblyName;
        public References references;
        public bool enableNullable;
        public bool isValid;

        /// <summary>
        /// Do not store <paramref name="compilation"/>.
        /// </summary>
        public static CompilationCandidateSlim GetCompilation(
              Compilation compilation
            , string generatorNamespace
            , string skipAttribute
        )
        {
            return new CompilationCandidateSlim {
                assemblyName = compilation.Assembly.Name,
                references = References.Create(compilation),
                enableNullable = compilation.Options.NullableContextOptions != NullableContextOptions.Disable,
                isValid = compilation.IsValidCompilation(generatorNamespace, skipAttribute),
            };
        }

        public readonly override bool Equals(object obj)
            => obj is CompilationCandidateSlim other && Equals(other);

        public readonly bool Equals(CompilationCandidateSlim other)
            => string.Equals(assemblyName, other.assemblyName, StringComparison.Ordinal)
            && references.Equals(other.references)
            && enableNullable == other.enableNullable
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(assemblyName, references, enableNullable);
    }

    public struct References : IEquatable<References>
    {
        public bool odin;
        public bool unity;
        public bool unityCollections;
        public bool unityEntities;
        public bool unitask;
        public bool latiosCore;

        public static References Create(Compilation compilation)
        {
            var result = new References();

            foreach (var assembly in compilation.ReferencedAssemblyNames)
            {
                if (assembly.Name is "Sirenix.OdinInspector.Attributes")
                {
                    result.odin = true;
                    continue;
                }

                if (assembly.Name.StartsWith("UnityEngine"))
                {
                    result.unity = true;
                    continue;
                }

                if (assembly.Name is "Unity.Collections")
                {
                    result.unityCollections = true;
                    continue;
                }

                if (assembly.Name is "Unity.Entities")
                {
                    result.unityEntities = true;
                    continue;
                }

                if (assembly.Name is "UniTask")
                {
                    result.unitask = true;
                    continue;
                }

                if (assembly.Name is "Latios.Core")
                {
                    result.latiosCore = true;
                    continue;
                }
            }

            return result;
        }

        public readonly override bool Equals(object obj)
            => obj is References other && Equals(other);

        public readonly bool Equals(References other)
            => odin == other.odin
            && unity == other.unity
            && unityCollections == other.unityCollections
            && unityEntities == other.unityEntities
            && latiosCore == other.latiosCore
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(odin, unity, unityCollections, unityEntities, latiosCore);
    }
}
