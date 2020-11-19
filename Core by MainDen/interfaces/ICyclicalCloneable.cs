// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System;
using System.Collections.Generic;

namespace MainDen.Cyclical
{
    public interface ICyclicalCloneable : ICyclical, ICloneable
    {
        object CyclicalClone(ref IDictionary<object, object> Changes);
    }
}