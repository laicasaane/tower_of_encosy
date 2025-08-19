#if UNITY_EDITOR

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EncosyTower.Collections;
using EncosyTower.Common;
using UnityEngine.Pool;

namespace EncosyTower.Editor
{
    public interface ISearchValidator<T>
    {
        bool IsSearchable(T item);

        string GetSearchableString(T item);
    }

    public static class FuzzySearchAPI
    {
        private static readonly char[] s_splitChars = new[] {
            ' ', '.', ',', ':', ';', '\t', '_', '-', '\\', '/', '~',
            '<', '>', '(', ')', '[', ']', '{', '}', '=', '+', '*',
            '?', '&', '^', '%', '$'
        };

        public static void Search<T, TValidator>(
              [NotNull] IReadOnlyCollection<T> source
            , [NotNull] string searchString
            , [NotNull] ICollection<T> dest
            , [NotNull] TValidator validator
        )
            where TValidator : ISearchValidator<T>
        {
            dest.Clear();

            if (source.Count < 1)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(searchString))
            {
                dest.AddRange(source);
                return;
            }

            using var _ = HashSetPool<string>.Get(out var patterns);

            GetPatterns(searchString, patterns);

            if (patterns.Count < 1)
            {
                dest.AddRange(source);
                return;
            }

            using var __ = ListPool<SearchEntry<T>>.Get(out var searchEntries);
            searchEntries.IncreaseCapacityTo(source.Count);

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

                var totalScore = 0L;

                foreach (var pattern in patterns)
                {
                    var score = 0L;

                    if (UnityEditor.Search.FuzzySearch.FuzzyMatch(pattern, searchableString, ref score))
                    {
                        totalScore += score;
                    }
                }

                if (totalScore > 0L)
                {
                    searchEntries.Add(new(item, totalScore));
                }
            }

            searchEntries.Sort(SearchEntry<T>.Comparer);
            dest.AddRange(searchEntries.Select(SearchEntry<T>.Selector));
        }

        private static void GetPatterns(string searchString, HashSet<string> patterns)
        {
            var searchSpan = searchString.AsSpan();

            foreach (var range in searchSpan.SplitAny(s_splitChars))
            {
                var (offset, length) = range.GetOffsetAndLength(searchSpan.Length);

                if (length < 1)
                {
                    continue;
                }

                var patternSpan = searchSpan.Slice(offset, length);

                if (patternSpan.IsEmptyOrWhiteSpace())
                {
                    continue;
                }

                patterns.Add(patternSpan.ToString());
            }
        }

        private readonly record struct SearchEntry<T>(T Item, long Score)
        {
            public static readonly Comparison<SearchEntry<T>> Comparer = Compare;
            public static readonly Func<SearchEntry<T>, T> Selector = Select;

            private static int Compare(SearchEntry<T> x, SearchEntry<T> y)
                => y.Score.CompareTo(x.Score);

            private static T Select(SearchEntry<T> x)
                => x.Item;
        }
    }
}

#endif
