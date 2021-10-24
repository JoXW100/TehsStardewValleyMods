using System;
using System.Collections.Immutable;
using System.IO;
using StardewModdingAPI;
using TehPers.Core.Api.Content;
using TehPers.Core.Api.Items;
using TehPers.Core.Api.Json;
using TehPers.FishingOverhaul.Api.Content;

namespace TehPers.FishingOverhaul.Config.ContentPacks
{
    /// <summary>
    /// Fishing content pack.
    /// </summary>
    public record FishingContentPack : JsonConfigRoot
    {
        /// <summary>
        /// Fish traits to set.
        /// </summary>
        public ImmutableDictionary<NamespacedKey, FishTraits> SetFishTraits { get; init; } = ImmutableDictionary<NamespacedKey, FishTraits>.Empty;

        /// <summary>
        /// Fish traits to remove.
        /// </summary>
        public ImmutableArray<NamespacedKey> RemoveFishTraits { get; init; } = ImmutableArray<NamespacedKey>.Empty;

        /// <summary>
        /// Fish entries to add.
        /// </summary>
        public ImmutableArray<FishEntry> AddFish { get; init; } = ImmutableArray<FishEntry>.Empty;

        /// <summary>
        /// Fish entries to remove.
        /// </summary>
        public ImmutableArray<FishEntryFilter> RemoveFish { get; init; } = ImmutableArray<FishEntryFilter>.Empty;

        /// <summary>
        /// Trash entries to remove.
        /// </summary>
        public ImmutableArray<TrashEntry> AddTrash { get; init; } = ImmutableArray<TrashEntry>.Empty;

        /// <summary>
        /// Trash entries to remove.
        /// </summary>
        public ImmutableArray<TrashEntryFilter> RemoveTrash { get; init; } = ImmutableArray<TrashEntryFilter>.Empty;

        /// <summary>
        /// Treasure entries to add.
        /// </summary>
        public ImmutableArray<TreasureEntry> AddTreasure { get; init; } = ImmutableArray<TreasureEntry>.Empty;

        /// <summary>
        /// Treasure entries to remove.
        /// </summary>
        public ImmutableArray<TreasureEntryFilter> RemoveTreasure { get; init; } = ImmutableArray<TreasureEntryFilter>.Empty;

        /// <summary>
        /// The additional content files to include.
        /// </summary>
        public ImmutableArray<string> Include { get; init; } = ImmutableArray<string>.Empty;

        /// <summary>
        /// Merges all the content into a single content object.
        /// </summary>
        /// <param name="content">The content to merge into.</param>
        /// <param name="baseDir">The base directory to load included content from.</param>
        /// <param name="contentPack">The content pack.</param>
        /// <param name="jsonProvider">The JSON provider.</param>
        /// <param name="monitor">The monitor to log errors to.</param>
        public FishingContent AddTo(FishingContent content, string baseDir, IContentPack contentPack, IJsonProvider jsonProvider, IMonitor monitor)
        {
            // Add base content
            content = content with
            {
                SetFishTraits = content.SetFishTraits.AddRange(this.SetFishTraits),
                RemoveFishTraits = content.RemoveFishTraits.AddRange(this.RemoveFishTraits),
                AddFish = content.AddFish.AddRange(this.AddFish),
                RemoveFish = content.RemoveFish.AddRange(this.RemoveFish),
                AddTrash = content.AddTrash.AddRange(this.AddTrash),
                RemoveTrash = content.RemoveTrash.AddRange(this.RemoveTrash),
                AddTreasure = content.AddTreasure.AddRange(this.AddTreasure),
                RemoveTreasure = content.RemoveTreasure.AddRange(this.RemoveTreasure),
            };

            // Add included content
            foreach (string relativePath in this.Include)
            {
                // Get the full path to the included file
                var path = Path.Combine(baseDir, relativePath);

                // Load the included file
                FishingContentPack? included;
                try
                {
                    included = jsonProvider.ReadJson<FishingContentPack>(path, new ContentPackAssetProvider(contentPack), null);
                }
                catch (Exception ex)
                {
                    monitor.Log($"Failed to load included content pack '{path}'", LogLevel.Error);
                    monitor.Log(ex.ToString(), LogLevel.Error);
                    continue;
                }

                // Merge the included content
                if (included is not null)
                {
                    var contentBaseDir = Path.GetDirectoryName(path) ?? string.Empty;
                    content = included.AddTo(content, contentBaseDir, contentPack, jsonProvider, monitor);
                }
                else
                {
                    monitor.Log($"Content file is empty: {relativePath}", LogLevel.Error);
                }
            }

            return content;
        }
    }
}