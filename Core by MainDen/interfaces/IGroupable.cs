// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System.Collections.Generic;

namespace MainDen.Collections
{
    public interface IGroupable
    {
        List<IGroup> Groups { get; }
        bool IncludedIn(IGroup group);
        void IncludeTo(IGroup group);
        void ExcludeFrom(IGroup group);
    }
}