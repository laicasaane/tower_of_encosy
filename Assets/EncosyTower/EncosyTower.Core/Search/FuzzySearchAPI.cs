using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EncosyTower.Collections;
using EncosyTower.Common;
using UnityEngine.Pool;

namespace EncosyTower.Search
{
    public interface ISearchValidator<T>
    {
        bool IsSearchable(T item);

        string GetSearchableString(T item);
    }

    public static class FuzzySearchAPI
    {
        public static void Search(
              [NotNull] IEnumerable<string> source
            , [NotNull] string searchString
            , [NotNull] ICollection<string> dest
        )
        {
            dest.Clear();

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
        }

        private readonly record struct Choice<T>(Option<T> Item, string SearchableString);
    }
}
