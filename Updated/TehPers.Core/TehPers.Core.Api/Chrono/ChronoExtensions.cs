﻿using TehPers.Core.Api.Conflux.Matching;

namespace TehPers.Core.Api.Chrono
{
    public static class ChronoExtensions
    {
        /// <summary>Converts a <see cref="Season"/> to its lowercase string representation.</summary>
        /// <param name="season">The season to get the name of.</param>
        /// <returns>A lowercase string containing the season's name, or <c>null</c> if it does not represent exactly one season.</returns>
        public static string GetName(this Season season)
        {
            return season.Match<Season, string>()
                .When(Season.Spring, "spring")
                .When(Season.Summer, "summer")
                .When(Season.Fall, "fall")
                .When(Season.Winter, "winter")
                .Else((string)null);
        }

        /// <summary>Tries to convert a string into a <see cref="Season"/>.</summary>
        /// <param name="name">The name of the season.</param>
        /// <returns>The <see cref="Season"/> with the given name, or <c>null</c> if it didn't match.</returns>
        public static Season? GetSeason(this string name)
        {
            return name.ToUpper().Match<string, Season?>()
                .When("SPRING", Season.Spring)
                .When("SUMMER", Season.Summer)
                .When("FALL", Season.Fall)
                .When("WINTER", Season.Winter)
                .Else((Season?)null);
        }

        /// <summary>Tries to convert a string into a <see cref="Season"/>.</summary>
        /// <param name="name">The name of the season.</param>
        /// <param name="season">The resulting <see cref="Season"/>.</param>
        /// <returns>True if the name matched a season, false otherwise.</returns>
        public static bool TryGetSeason(this string name, out Season season)
        {
            Season? parsed = name.GetSeason();
            season = parsed ?? default;
            return parsed.HasValue;
        }
    }
}
