// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System.Xml;

namespace MainDen
{
    public interface IConvertible
    {
        XmlElement ToXmlElement(XmlDocument xmlDocument, string name);
    }
}