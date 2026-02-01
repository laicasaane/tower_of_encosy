namespace EncosyTower.Search
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using EncosyTower.Collections.Extensions;
    using EncosyTower.Common;
    using UnityEngine.Pool;

    public interface ISearchValidator<T>
    {
        bool IsSearchable(T item);

        string GetSearchableString(T item);
    }

    public static partial class FuzzySearchAPI
    {
        public static void Search(
              [NotNull] IEnumerable<string> source
            , [NotNull] string searchString
            , [NotNull] ICollection<string> dest
        )
        {
            dest.Clear();

#if !(ENCOSY_RAFFINERT_FUZZYSHARP || NUGET_RAFFINERT_FUZZYSHARP)
            PluginValidator.LogErrorIfFuzzySharpIsNotInstalled();
            dest.AddRange(source);
#else
            if (string.IsNullOrWhiteSpace(searchString))
            {
                dest.AddRange(source);
                return;
            }

            using var _ = ListPool<string>.Get(out var choices);

            if (source.TryGetCountFast(out var count))
            {
                choices.IncreaseCapacityTo(count);
            }

            foreach (var item in source)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                choices.Add(item);
            }

            var searchResults = Raffinert.FuzzySharp.Process.ExtractSorted(searchString, choices);

            dest.TryIncreaseCapacityToFast(choices.Count);

            foreach (var result in searchResults)
            {
                dest.Add(result.Value);
            }
#endif
        }

        public static void Search<T, TValidator>(
              [NotNull] IEnumerable<T> source
            , [NotNull] string searchString
            , [NotNull] ICollection<T> dest
            , [NotNull] TValidator validator
        )
            where TValidator : ISearchValidator<T>
        {
            dest.Clear();

#if !(ENCOSY_RAFFINERT_FUZZYSHARP || NUGET_RAFFINERT_FUZZYSHARP)
            PluginValidator.LogErrorIfFuzzySharpIsNotInstalled();
            dest.AddRange(source);
#else
            if (string.IsNullOrWhiteSpace(searchString))
            {
                dest.AddRange(source);
                return;
            }

            using var _ = ListPool<Choice<T>>.Get(out var choices);

            if (source.TryGetCountFast(out var count))
            {
                choices.IncreaseCapacityTo(count);
            }

            foreach (var item in source)
            {
                if (validator.IsSearchable(item) == false)
                {
                    continue;
                }

                var searchableString = validator.GetSearchableString(item) ?? string.Empty;

                if (string.IsNullOrEmpty(searchableString))
                {
                    continue;
                }

                choices.Add(new(item, searchableString));
            }

            var searchResults = Raffinert.FuzzySharp.Process.ExtractSorted(
                  new Choice<T>(Option.None, searchString)
                , choices
                , static x => x.SearchableString
            );

            dest.TryIncreaseCapacityToFast(choices.Count);

            foreach (var result in searchResults)
            {
                if (result.Value.Item.TryGetValue(out var item))
                {
                    dest.Add(item);
                }
            }
#endif
        }

        private readonly record struct Choice<T>(Option<T> Item, string SearchableString);
    }
}

#if !(ENCOSY_RAFFINERT_FUZZYSHARP || NUGET_RAFFINERT_FUZZYSHARP)

namespace EncosyTower.Search
{
    using System.Diagnostics;
    using EncosyTower.Logging;
    using UnityEngine;

    partial class FuzzySearchAPI
    {
        private static class PluginValidator
        {
#if DEVELOPMENT_BUILD
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#elif UNITY_EDITOR
            [UnityEditor.InitializeOnLoadMethod]
#endif
            [HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
            public static void LogErrorIfFuzzySharpIsNotInstalled()
            {
                StaticDevLogger.LogError(
                    "Please install Raffinert.FuzzySharp plugin via one of these methods:\n" +
                    "1. OpenUPM: `openupm add org.nuget.raffinert.fuzzysharp`. " +
                    "Read <a href=\"https://openupm.com/nuget/#using-uplinked-unitynuget\">more</a> about this method.\n" +
                    "2. Add this scripting symbol to the project: ENCOSY_RAFFINERT_FUZZYSHARP. " +
                    "Read <a href=\"https://docs.unity3d.com/Manual/custom-scripting-symbols.html\">more</a> about this method."
                );
            }
        }
    }
}

#endif
