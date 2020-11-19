// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System.Collections.Generic;

namespace MainDen.Collections.Generic
{
    public interface IGroup<T> : IGroup
    {
        new List<object> Entries { get; }
        new List<T> Members { get; }
        new List<IGroup<T>> Subgroups { get; }
        void Include(T instance);
        void Include(IGroup<T> instance);
        void Exclude(T instance);
        void Exclude(IGroup<T> instance);
        bool Contains(T instance);
        bool Contains(IGroup<T> instance);
    }
}