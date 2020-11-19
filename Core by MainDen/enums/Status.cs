// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System.ComponentModel;

namespace MainDen.Enums
{
    [DefaultValue(Ready)]
    public enum Status : uint
    {
        Ready = 0x0,
        Running = 0x1,
        Completed = 0x2,
        Stopped = 0x3,
        Error = 0x4
    }
}