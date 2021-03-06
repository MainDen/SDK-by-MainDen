﻿// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using MainDen.Collections.Generic;
using System.Collections.Generic;

namespace MainDen.Cyclical
{
    public interface ICyclical
    {
        int GetHashCode(ref IList<LeftRightPair<object, int>> hashCodes);
    }
}