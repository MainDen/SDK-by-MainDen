// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System.Collections.Generic;

namespace MainDen.Collections.Generic
{
    public interface IGroupable<T> : IGroupable
    {
        new List<IGroup<T>> Groups { get; }
        bool IncludedIn(IGroup<T> group);
        void IncludeTo(IGroup<T> group);
        void ExcludeFrom(IGroup<T> group);
    }
}