// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System.Collections.Generic;

namespace MainDen.Collections
{
    public interface IGroup
    {
        List<object> Entries { get; }
        List<object> Members { get; }
        List<IGroup> Subgroups { get; }
        void Include(object instance);
        void Exclude(object instance);
        bool Contains(object instance);
    }
}