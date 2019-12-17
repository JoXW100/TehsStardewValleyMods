﻿using System;

namespace TehPers.Core.Api.Harmony
{
    /// <summary>A service designed to manage one or more Harmony patches.</summary>
    public interface IPatchingService : IDisposable
    {
        /// <summary>Applies the Harmony patches.</summary>
        void ApplyPatches();

        /// <summary>Removes the Harmony patches applied by this <see cref="IPatchingService"/>.</summary>
        void RemovePatches();
    }
}
