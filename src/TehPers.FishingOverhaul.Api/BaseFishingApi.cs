﻿using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TehPers.Core.Api.Extensions;
using TehPers.Core.Api.Items;
using TehPers.FishingOverhaul.Api.Content;
using TehPers.FishingOverhaul.Api.Events;
using TehPers.FishingOverhaul.Api.Weighted;

namespace TehPers.FishingOverhaul.Api
{
    /// <summary>
    /// Base API for working with fishing. This implements many of the API members
    /// </summary>
    public abstract class BaseFishingApi : IFishingApi
    {
        /// <inheritdoc/>
        public event EventHandler<CaughtItemEventArgs>? CaughtItem;

        /// <inheritdoc/>
        public event EventHandler<OpeningChestEventArgs>? OpeningChest;

        /// <inheritdoc/>
        public event EventHandler<CustomEventArgs>? CustomEvent;

        /// <inheritdoc/>
        public event EventHandler<CreatedDefaultFishingInfoEventArgs>? CreatedDefaultFishingInfo;

        /// <inheritdoc/>
        public event EventHandler<PreparedFishEventArgs>? PreparedFishChances;

        /// <inheritdoc/>
        public event EventHandler<PreparedTrashEventArgs>? PreparedTrashChances;

        /// <inheritdoc/>
        public event EventHandler<PreparedTreasureEventArgs>? PreparedTreasureChances;

        /// <inheritdoc/>
        public event EventHandler<CalculatedFishChanceEventArgs>? CalculatedFishChance;

        /// <inheritdoc />
        public event EventHandler<CalculatedFishChanceEventArgs>? CalculatedMinFishChance;

        /// <inheritdoc />
        public event EventHandler<CalculatedFishChanceEventArgs>? CalculatedMaxFishChance;

        /// <inheritdoc/>
        public event EventHandler<CalculatedTreasureChanceEventArgs>? CalculatedTreasureChance;

        /// <inheritdoc />
        public event EventHandler<CalculatedTreasureChanceEventArgs>? CalculatedMinTreasureChance;

        /// <inheritdoc />
        public event EventHandler<CalculatedTreasureChanceEventArgs>? CalculatedMaxTreasureChance;

        /// <inheritdoc/>
        public abstract int GetStreak(Farmer farmer);

        /// <inheritdoc/>
        public abstract void SetStreak(Farmer farmer, int streak);

        /// <inheritdoc/>
        public abstract FishingInfo CreateDefaultFishingInfo(Farmer farmer);

        /// <inheritdoc/>
        public abstract IEnumerable<IWeightedValue<FishEntry>> GetFishChances(
            FishingInfo fishingInfo
        );

        /// <inheritdoc/>
        public IEnumerable<string> GetCatchableFish(Farmer farmer, int depth)
        {
            return this
                .GetFishChances(this.CreateDefaultFishingInfo(farmer) with {BobberDepth = depth})
                .Select(weightedValue => weightedValue.Value.FishKey.Key);
        }

        /// <inheritdoc />
        public NamespacedKey? GetFishPondFish(
            Farmer farmer,
            Vector2 bobberTile,
            bool takeFish = false
        )
        {
            // Get the fish in that fish pond, if any
            return farmer.currentLocation.buildings.OfType<FishPond>()
                .Where(pond => pond.isTileFishable(bobberTile))
                .Select(
                    pond =>
                    {
                        int? parentSheetIndex = takeFish switch
                        {
                            true when pond.CatchFish() is {ParentSheetIndex: var id} => id,
                            false when pond.currentOccupants.Value > 0 => pond.GetFishObject().ParentSheetIndex,
                            _ => null,
                        };
                        return parentSheetIndex.Select(NamespacedKey.SdvObject);
                    }
                )
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public abstract IEnumerable<IWeightedValue<TrashEntry>> GetTrashChances(
            FishingInfo fishingInfo
        );

        /// <inheritdoc />
        public IEnumerable<string> GetCatchableTrash(Farmer farmer)
        {
            return this.GetTrashChances(this.CreateDefaultFishingInfo(farmer))
                .Select(weightedValue => weightedValue.Value.ItemKey.Key);
        }

        /// <inheritdoc/>
        public abstract IEnumerable<IWeightedValue<TreasureEntry>> GetTreasureChances(
            FishingInfo fishingInfo
        );

        /// <inheritdoc />
        public IEnumerable<string> GetCatchableTreasure(Farmer farmer)
        {
            return this.GetTreasureChances(this.CreateDefaultFishingInfo(farmer))
                .SelectMany(weightedValue => weightedValue.Value.ItemKeys)
                .Select(key => key.Key)
                .Distinct();
        }

        /// <inheritdoc/>
        public abstract double GetChanceForFish(FishingInfo fishingInfo);

        /// <inheritdoc />
        public double GetChanceForFish(Farmer farmer)
        {
            var fishingInfo = this.CreateDefaultFishingInfo(farmer);
            return this.GetChanceForFish(fishingInfo);
        }

        /// <inheritdoc/>
        public abstract double GetChanceForTreasure(FishingInfo fishingInfo);

        /// <inheritdoc />
        public double GetChanceForTreasure(Farmer farmer)
        {
            var fishingInfo = this.CreateDefaultFishingInfo(farmer);
            return this.GetChanceForTreasure(fishingInfo);
        }

        /// <inheritdoc />
        public void ModifyChanceForFish(Func<Farmer, double, double> chanceModifier)
        {
            _ = chanceModifier ?? throw new ArgumentNullException(nameof(chanceModifier));

            this.CalculatedFishChance += (_, e) =>
                e.Chance = chanceModifier(e.FishingInfo.User, e.Chance);
        }

        /// <inheritdoc />
        public void ModifyChanceForTreasure(Func<Farmer, double, double> chanceModifier)
        {
            _ = chanceModifier ?? throw new ArgumentNullException(nameof(chanceModifier));

            this.CalculatedTreasureChance += (_, e) =>
                e.Chance = chanceModifier(e.FishingInfo.User, e.Chance);
        }

        /// <inheritdoc/>
        public abstract bool TryGetFishTraits(
            NamespacedKey fishKey,
            [NotNullWhen(true)] out FishTraits? traits
        );

        /// <inheritdoc/>
        public abstract bool IsLegendary(NamespacedKey fishKey);

        /// <inheritdoc />
        public bool IsLegendary(string fishKey)
        {
            return NamespacedKey.TryParse(fishKey, out var key) && this.IsLegendary(key);
        }

        /// <inheritdoc/>
        public abstract PossibleCatch GetPossibleCatch(FishingInfo fishingInfo);

        /// <inheritdoc />
        public string GetPossibleCatch(Farmer farmer, int bobberDepth, out bool isFish)
        {
            var possibleCatch = this.GetPossibleCatch(
                this.CreateDefaultFishingInfo(farmer) with {BobberDepth = bobberDepth}
            );
            switch (possibleCatch)
            {
                case PossibleCatch.Fish(var entry):
                    {
                        isFish = true;
                        return entry.FishKey.Key;
                    }
                case PossibleCatch.Trash(var entry):
                    {
                        isFish = false;
                        return entry.ItemKey.Key;
                    }
                default:
                    {
                        throw new InvalidOperationException(
                            $"Unknown possible catch type: {possibleCatch}"
                        );
                    }
            }
        }

        /// <inheritdoc/>
        public abstract IEnumerable<TreasureEntry> GetPossibleTreasure(
            CatchInfo.FishCatch catchInfo
        );

        /// <inheritdoc />
        public IEnumerable<TreasureEntry> GetPossibleTreasure(FishingInfo fishingInfo)
        {
            var catchInfo = new CatchInfo.FishCatch(
                fishingInfo,
                new(NamespacedKey.SdvObject(0), new(0.0)),
                new StardewValley.Object("0", 1),
                0,
                false,
                0,
                0,
                new(false, TreasureState.None),
                false
            );
            return this.GetPossibleTreasure(catchInfo);
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetPossibleTreasure(Farmer farmer)
        {
            var fishingInfo = this.CreateDefaultFishingInfo(farmer);
            var catchInfo = new CatchInfo.FishCatch(
                fishingInfo,
                new(NamespacedKey.SdvObject(0), new(0.0)),
                new StardewValley.Object("0", 1),
                0,
                false,
                0,
                0,
                new(false, TreasureState.None),
                false
            );
            return this.GetPossibleTreasure(catchInfo)
                .SelectMany(entry => entry.ItemKeys)
                .Select(key => key.Key);
        }

        /// <inheritdoc/>
        public void RaiseCustomEvent(CustomEventArgs customEventArgs)
        {
            this.CustomEvent?.Invoke(this, customEventArgs);
        }

        /// <inheritdoc/>
        public abstract void RequestReload();

        /// <summary>
        /// Invokes the <see cref="CaughtItem"/> event.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected void OnCaughtItem(CaughtItemEventArgs e)
        {
            this.CaughtItem?.Invoke(this, e);
        }

        /// <summary>
        /// Invokes the <see cref="OpeningChest"/> event.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected void OnOpeningChest(OpeningChestEventArgs e)
        {
            this.OpeningChest?.Invoke(this, e);
        }

        /// <summary>
        /// Invokes the <see cref="CreatedDefaultFishingInfo"/> event.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected void OnCreatedDefaultFishingInfo(CreatedDefaultFishingInfoEventArgs e)
        {
            this.CreatedDefaultFishingInfo?.Invoke(this, e);
        }

        /// <summary>
        /// Invokes the <see cref="PreparedFishChances"/> event.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected void OnPreparedFishChances(PreparedFishEventArgs e)
        {
            this.PreparedFishChances?.Invoke(this, e);
        }

        /// <summary>
        /// Invokes the <see cref="PreparedTrashChances"/> event.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected void OnPreparedTrashChances(PreparedTrashEventArgs e)
        {
            this.PreparedTrashChances?.Invoke(this, e);
        }

        /// <summary>
        /// Invokes the <see cref="PreparedTreasureChances"/> event.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected void OnPreparedTreasureChances(PreparedTreasureEventArgs e)
        {
            this.PreparedTreasureChances?.Invoke(this, e);
        }

        /// <summary>
        /// Invokes the <see cref="CalculatedFishChance"/> event.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected void OnCalculatedFishChance(CalculatedFishChanceEventArgs e)
        {
            this.CalculatedFishChance?.Invoke(this, e);
        }

        /// <summary>
        /// Invokes the <see cref="CalculatedTreasureChance"/> event.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected void OnCalculatedTreasureChance(CalculatedTreasureChanceEventArgs e)
        {
            this.CalculatedTreasureChance?.Invoke(this, e);
        }

        /// <summary>
        /// Invokes the <see cref="CalculatedMinFishChance"/> event.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnCalculatedMinFishChance(CalculatedFishChanceEventArgs e)
        {
            this.CalculatedMinFishChance?.Invoke(this, e);
        }

        /// <summary>
        /// Invokes the <see cref="CalculatedMaxFishChance"/> event.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnCalculatedMaxFishChance(CalculatedFishChanceEventArgs e)
        {
            this.CalculatedMaxFishChance?.Invoke(this, e);
        }

        /// <summary>
        /// Invokes the <see cref="CalculatedMinTreasureChance"/> event.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnCalculatedMinTreasureChance(CalculatedTreasureChanceEventArgs e)
        {
            this.CalculatedMinTreasureChance?.Invoke(this, e);
        }

        /// <summary>
        /// Invokes the <see cref="CalculatedMaxTreasureChance"/> event.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnCalculatedMaxTreasureChance(CalculatedTreasureChanceEventArgs e)
        {
            this.CalculatedMaxTreasureChance?.Invoke(this, e);
        }
    }
}
