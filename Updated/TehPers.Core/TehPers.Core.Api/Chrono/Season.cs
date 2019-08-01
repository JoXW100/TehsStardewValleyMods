﻿using System;

namespace TehPers.Core.Api.Chrono {
    [Flags]
    public enum Season {
        Spring = 1,
        Summer = 2,
        Fall = 4,
        Winter = 8,

        None = 0,
        Any = Season.Spring | Season.Summer | Season.Fall | Season.Winter
    }
}