// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System.Collections.Generic;

namespace MainDen.Collections
{
    public interface IObj
    {
        Dictionary<string, object> Properties { get; }
        bool TryGetProperty(string property, out object value);
        void SetProperty(string property, object value);
        bool RemoveProperty(string property);
        bool ContainsProperty(string property);
        object this[string property] { get; set; }
    }
}