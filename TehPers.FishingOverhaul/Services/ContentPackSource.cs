﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;
using TehPers.Core.Api.Content;
using TehPers.Core.Api.Json;
using TehPers.FishingOverhaul.Api.Content;
using TehPers.FishingOverhaul.Config.ContentPacks;

namespace TehPers.FishingOverhaul.Services
{
    internal sealed class ContentPackSource : IFishingContentSource
    {
        private readonly IModHelper helper;
        private readonly IMonitor monitor;
        private readonly IJsonProvider jsonProvider;

        public ContentPackSource(IModHelper helper, IMonitor monitor, IJsonProvider jsonProvider)
        {
            this.helper = helper ?? throw new ArgumentNullException(nameof(helper));
            this.monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            this.jsonProvider =
                jsonProvider ?? throw new ArgumentNullException(nameof(jsonProvider));
        }

        public IEnumerable<FishingContent> Reload()
        {
            // Load content packs
            foreach (var pack in this.helper.ContentPacks.GetOwned())
            {
                // Create content source
                var content = new FishingContent(pack.Manifest);
                var isNewPack = false;
                var warnedObsolete = false;

                // Content
                if (this.TryRead<FishingContentPack>(pack, "content.json", out var contentPack))
                {
                    content = contentPack.AddTo(content, string.Empty, pack, this.jsonProvider, this.monitor);
                    isNewPack = true;
                }

                // Fish traits
                // TODO: Remove this when compatibility with the old content pack system is no longer needed
                if (!isNewPack && this.TryRead<FishTraitsPack>(pack, "fishTraits.json", out var fishTraits))
                {
                    content = fishTraits.AddTo(content);
                    this.WarnObsolete(ref warnedObsolete, pack.Manifest.UniqueID);
                }

                // Fish
                // TODO: Remove this when compatibility with the old content pack system is no longer needed
                if (!isNewPack && this.TryRead<FishPack>(pack, "fish.json", out var fish))
                {
                    content = fish.AddTo(content);
                    this.WarnObsolete(ref warnedObsolete, pack.Manifest.UniqueID);
                }

                // Trash
                // TODO: Remove this when compatibility with the old content pack system is no longer needed
                if (!isNewPack && this.TryRead<TrashPack>(pack, "trash.json", out var trash))
                {
                    content = trash.AddTo(content);
                    this.WarnObsolete(ref warnedObsolete, pack.Manifest.UniqueID);
                }

                // Treasure
                // TODO: Remove this when compatibility with the old content pack system is no longer needed
                if (!isNewPack && this.TryRead<TreasurePack>(pack, "treasure.json", out var treasure))
                {
                    content = treasure.AddTo(content);
                    this.WarnObsolete(ref warnedObsolete, pack.Manifest.UniqueID);
                }

                // Yield the content
                yield return content;
            }
        }

        private void WarnObsolete(ref bool warned, string uniqueId)
        {
            if (warned)
            {
                return;
            }

            warned = true;
            this.monitor.Log($"Content pack {uniqueId} uses an obsolete format that will eventually be removed.", LogLevel.Warn);
        }

        private bool TryRead<T>(IContentPack pack, string path, [NotNullWhen(true)] out T? result)
            where T : class
        {
            try
            {
                // Try to read the file
                result = this.jsonProvider.ReadJson<T>(
                    path,
                    new ContentPackAssetProvider(pack),
                    null
                );
                return result is not null;
            }
            catch (Exception ex)
            {
                this.monitor.Log(
                    $"Error loading content pack '{pack.Manifest.UniqueID}'. The file {path} is invalid.\n{ex}",
                    LogLevel.Error
                );
                result = default;
                return false;
            }
        }
    }
}