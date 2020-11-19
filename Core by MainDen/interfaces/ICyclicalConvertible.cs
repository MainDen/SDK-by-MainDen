// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System.Collections.Generic;
using System.Xml;

namespace MainDen.Cyclical
{
    public interface ICyclicalConvertible : ICyclical, IConvertible
    {
        XmlElement ToXmlElement(XmlDocument xmlDocument, ref IList<object> id_soure, string name);
    }
}