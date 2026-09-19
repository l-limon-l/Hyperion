using System;
using System.Collections.Generic;
using System.Linq;

namespace HyperionWPF.Model
{
    /// <summary>
    /// The application catalogue. The data lives in the generated <c>Catalog.Data.cs</c>;
    /// this half assembles it and prepends the "Popular" page.
    /// </summary>
    internal static partial class Catalog
    {
        private static readonly Lazy<IReadOnlyList<Category>> LazyCategories =
            new Lazy<IReadOnlyList<Category>>(Build);

        /// <summary>Every page, "Popular" first.</summary>
        public static IReadOnlyList<Category> Categories => LazyCategories.Value;

        /// <summary>The frequently installed picks shown on the first page.</summary>
        public static Category Popular => Categories[0];

        /// <summary>Total number of installable packages, counting each variant of a group.</summary>
        public static int PackageCount => Categories
            .Skip(1)
            .SelectMany(c => c.Apps)
            .SelectMany(a => a.Packages)
            .Select(p => p.Id)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        private static IReadOnlyList<Category> Build()
        {
            IReadOnlyList<Category> categories = BuildCategories();
            var byKey = new Dictionary<string, AppEntry>(StringComparer.Ordinal);
            foreach (AppEntry app in categories.SelectMany(c => c.Apps))
            {
                byKey[app.Key] = app;
            }

            var popular = new List<AppEntry>();
            foreach (string key in PopularKeys)
            {
                if (byKey.TryGetValue(key, out AppEntry entry))
                {
                    popular.Add(entry);
                }
            }

            foreach ((string Key, string Variant) pick in PopularVariants)
            {
                if (byKey.TryGetValue(pick.Key, out AppEntry group))
                {
                    popular.Add(group.VariantAsEntry(pick.Variant));
                }
            }

            var all = new List<Category>(categories.Count + 1)
            {
                new Category("Popular", "Popular", "Популярное", CategoryGlyph.Star, popular),
            };
            all.AddRange(categories);
            return all;
        }
    }
}
