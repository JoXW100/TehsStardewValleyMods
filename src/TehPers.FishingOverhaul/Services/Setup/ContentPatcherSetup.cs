﻿using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Tools;
using TehPers.Core.Api.Items;
using TehPers.Core.Api.Setup;
using TehPers.FishingOverhaul.Services.Tokens;

namespace TehPers.FishingOverhaul.Services.Setup
{
    internal sealed class ContentPatcherSetup : ISetup
    {
        private readonly IManifest manifest;
        private readonly IContentPatcherAPI contentPatcherApi;
        private readonly MissingSecretNotesToken missingSecretNotesToken;
        private readonly MissingJournalScrapsToken missingJournalScrapsToken;

        public ContentPatcherSetup(
            IManifest manifest,
            IContentPatcherAPI contentPatcherApi,
            MissingSecretNotesToken missingSecretNotesToken,
            MissingJournalScrapsToken missingJournalScrapsToken
        )
        {
            this.manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
            this.contentPatcherApi = contentPatcherApi
                ?? throw new ArgumentNullException(nameof(contentPatcherApi));
            this.missingSecretNotesToken = missingSecretNotesToken
                ?? throw new ArgumentNullException(nameof(missingSecretNotesToken));
            this.missingJournalScrapsToken = missingJournalScrapsToken
                ?? throw new ArgumentNullException(nameof(missingJournalScrapsToken));
        }

        public void Setup()
        {
            this.contentPatcherApi.RegisterToken(
                this.manifest,
                "BooksFound",
                new BooksFoundToken()
            );
            this.contentPatcherApi.RegisterToken(this.manifest, "HasItem", new HasItemToken());
            this.contentPatcherApi.RegisterToken(
                this.manifest,
                "SpecialOrderRuleActive",
                ContentPatcherSetup.GetSpecialOrderRuleActive
            );
            this.contentPatcherApi.RegisterToken(
                this.manifest,
                "MissingSecretNotes",
                this.missingSecretNotesToken
            );
            this.contentPatcherApi.RegisterToken(
                this.manifest,
                "MissingJournalScraps",
                this.missingJournalScrapsToken
            );
            this.contentPatcherApi.RegisterToken(
                this.manifest,
                "RandomGoldenWalnuts",
                ContentPatcherSetup.GetRandomGoldenWalnuts
            );
            this.contentPatcherApi.RegisterToken(
                this.manifest,
                "TidePoolGoldenWalnut",
                ContentPatcherSetup.GetTidePoolGoldenWalnut
            );
            this.contentPatcherApi.RegisterToken(
                this.manifest,
                "ActiveBait",
                ContentPatcherSetup.GetActiveBait
            );
            this.contentPatcherApi.RegisterToken(
                this.manifest,
                "ActiveTackle",
                ContentPatcherSetup.GetActiveTackle
            );
            this.contentPatcherApi.RegisterToken(
                this.manifest,
                "IslandNorthBridgeFixed",
                ContentPatcherSetup.GetIslandNorthBridgeFixed
            );
        }

        private static IEnumerable<string>? GetSpecialOrderRuleActive()
        {
            if (Game1.player is not {team: {specialOrders: { } specialOrders}})
            {
                return null;
            }

            return specialOrders.SelectMany(
                    specialOrder =>
                    {
                        if (specialOrder.questState.Value is not SpecialOrder.QuestState.InProgress)
                        {
                            return Enumerable.Empty<string>();
                        }

                        if (specialOrder.specialRule.Value is not { } specialRule)
                        {
                            return Enumerable.Empty<string>();
                        }

                        return specialRule.Split(
                            ',',
                            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                        );
                    }
                )
                .OrderBy(val => val, StringComparer.OrdinalIgnoreCase);
        }

        private static IEnumerable<string>? GetRandomGoldenWalnuts()
        {
            if (Game1.player is not {team: {limitedNutDrops: { } limitedNutDrops}})
            {
                return null;
            }

            return limitedNutDrops.TryGetValue("IslandFishing", out var fishingNuts)
                ? new[] {fishingNuts.ToString("G")}
                : new[] {"0"};
        }

        private static IEnumerable<string>? GetTidePoolGoldenWalnut()
        {
            if (Game1.player is not {team: { } team})
            {
                return null;
            }

            return team.collectedNutTracker.TryGetValue("StardropPool", out var gotNut) && gotNut
                ? new[] {"true"}
                : new[] {"false"};
        }

        private static IEnumerable<string>? GetActiveBait()
        {
            if (Game1.player is not {CurrentItem: FishingRod rod})
            {
                return null;
            }

            var index = rod.getBaitAttachmentIndex();
            if (index < 0)
            {
                return null;
            }

            return new[] {NamespacedKey.SdvObject(index).ToString()};
        }

        private static IEnumerable<string>? GetActiveTackle()
        {
            if (Game1.player is not {CurrentItem: FishingRod rod})
            {
                return null;
            }

            var index = rod.getBobberAttachmentIndex();
            if (index < 0)
            {
                return null;
            }

            return new[] {NamespacedKey.SdvObject(index).ToString()};
        }

        private static IEnumerable<string>? GetIslandNorthBridgeFixed()
        {
            if (Game1.getLocationFromName("IslandNorth") is not IslandNorth islandNorth)
            {
                return null;
            }

            return new[] {islandNorth.bridgeFixed.Value ? "true" : "false"};
        }
    }
}
