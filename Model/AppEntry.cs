using System;
using System.Collections.Generic;
using System.Linq;

namespace HyperionWPF.Model
{
    /// <summary>Glyph bucket for a category, resolved to a Segoe MDL2 code point by the UI.</summary>
    internal enum CategoryGlyph
    {
        Star,
        Browser,
        Chat,
        Media,
        Image,
        Doc,
        Game,
        Cloud,
        Tools,
        Archive,
        Shield,
        Code,
        Runtime,
    }

    /// <summary>
    /// The identifiers one package is known by across the three supported package managers.
    /// Any of them may be null when a manager does not carry the package.
    /// </summary>
    internal sealed class PackageRef
    {
        public PackageRef(string wingetId, string scoopId, string chocoId)
        {
            WingetId = wingetId;
            ScoopId = scoopId;
            ChocoId = chocoId;
        }

        public string WingetId { get; }

        public string ScoopId { get; }

        public string ChocoId { get; }

        /// <summary>Stable identity used to de-duplicate a package selected on two pages.</summary>
        public string Id => WingetId ?? ChocoId ?? ScoopId;

        /// <summary>What the log shows for this package.</summary>
        public string DisplayId => WingetId ?? ChocoId ?? ScoopId ?? "?";
    }

    /// <summary>One selectable option inside a grouped entry, such as ".NET Desktop Runtime 9 (x64)".</summary>
    internal sealed class AppVariant
    {
        public AppVariant(string label, PackageRef package)
        {
            Label = label;
            Package = package;
        }

        public string Label { get; }

        public PackageRef Package { get; }
    }

    /// <summary>A catalogue row: either a single package or a group of versioned variants.</summary>
    internal sealed class AppEntry
    {
        private static readonly AppVariant[] NoVariants = new AppVariant[0];

        /// <summary>Single-package entry.</summary>
        public AppEntry(string key, string title, string iconKey, string descriptionEn, string descriptionRu, PackageRef package)
            : this(key, title, iconKey, descriptionEn, descriptionRu, package, NoVariants)
        {
        }

        /// <summary>Grouped entry with one toggle per variant.</summary>
        public AppEntry(string key, string title, string iconKey, string descriptionEn, string descriptionRu, AppVariant[] variants)
            : this(key, title, iconKey, descriptionEn, descriptionRu, null, variants)
        {
        }

        private AppEntry(string key, string title, string iconKey, string descriptionEn, string descriptionRu,
            PackageRef package, AppVariant[] variants)
        {
            Key = key;
            Title = title;
            IconKey = iconKey;
            DescriptionEn = descriptionEn;
            DescriptionRu = descriptionRu;
            Package = package;
            Variants = variants ?? NoVariants;
        }

        public string Key { get; }

        public string Title { get; }

        /// <summary>Name of the PNG in Icons/, or null to fall back to a generated monogram.</summary>
        public string IconKey { get; }

        public string DescriptionEn { get; }

        public string DescriptionRu { get; }

        /// <summary>Non-null for single-package entries, null for groups.</summary>
        public PackageRef Package { get; }

        public IReadOnlyList<AppVariant> Variants { get; }

        public bool IsGroup => Variants.Count > 0;

        public string Description(bool russian) => russian ? DescriptionRu : DescriptionEn;

        /// <summary>Every package this entry can install, whether it is a group or not.</summary>
        public IEnumerable<PackageRef> Packages =>
            IsGroup ? Variants.Select(v => v.Package) : new[] { Package };

        /// <summary>Projects one variant of a group into a standalone entry, for the Popular page.</summary>
        public AppEntry VariantAsEntry(string variantLabel)
        {
            AppVariant variant = Variants.FirstOrDefault(v =>
                string.Equals(v.Label, variantLabel, StringComparison.Ordinal));
            if (variant == null)
            {
                throw new ArgumentException("Unknown variant '" + variantLabel + "' of entry '" + Key + "'.", nameof(variantLabel));
            }

            return new AppEntry(
                Key + "/" + variant.Label,
                Title + " " + variant.Label,
                IconKey,
                DescriptionEn,
                DescriptionRu,
                variant.Package);
        }
    }

    /// <summary>A navigation page: a named list of catalogue entries.</summary>
    internal sealed class Category
    {
        public Category(string key, string nameEn, string nameRu, CategoryGlyph glyph, IReadOnlyList<AppEntry> apps)
        {
            Key = key;
            NameEn = nameEn;
            NameRu = nameRu;
            Glyph = glyph;
            Apps = apps;
        }

        public string Key { get; }

        public string NameEn { get; }

        public string NameRu { get; }

        public CategoryGlyph Glyph { get; }

        public IReadOnlyList<AppEntry> Apps { get; }

        public string Name(bool russian) => russian ? NameRu : NameEn;
    }
}
