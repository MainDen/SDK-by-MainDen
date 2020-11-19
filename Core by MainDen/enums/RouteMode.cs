// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System;

namespace MainDen.Enums
{
    [Flags]
    public enum RouteMode : uint
    {
        Default = 0x0,
        Loop = 0x1,
        Reverse = 0x2
    }
}