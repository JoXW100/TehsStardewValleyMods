﻿using System;
using StardewModdingAPI;
using TehPers.CoreMod.Api;
using TehPers.CoreMod.Api.ContentPacks.Tokens;

namespace TehPers.CoreMod {
    internal class CoreApiInitializer : ICoreApiInitializer {
        public IMod Owner => this.CoreApi.Owner;
        public ICoreApi CoreApi { get; }

        public CoreApiInitializer(ICoreApi coreApi) {
            this.CoreApi = coreApi;
        }

        public void RegisterToken(string name, IToken token) {
            throw new NotImplementedException();
        }
    }
}