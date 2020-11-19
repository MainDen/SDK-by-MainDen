// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using MainDen.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace MainDen.Cyclical
{
    public static class CyclicalMethods
    {
        public delegate object TypeToObject<T>(T source);
        public delegate object TypeToIdObject<T>(T source, ref IDictionary<string, object> id_source);
        public static Dictionary<string, TypeToObject<XmlElement>> XmlElementConverters = new Dictionary<string, TypeToObject<XmlElement>>();
        public static Dictionary<string, TypeToIdObject<XmlElement>> XmlElementIdConverters = new Dictionary<string, TypeToIdObject<XmlElement>>();
        public static object Clone(object source, ref IDictionary<object, object> contract)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (contract is null)
                throw new ArgumentNullException(nameof(contract));
            if (contract.ContainsKey(source))
                return contract[source];
            else if (source is ICyclicalCloneable cyclicalCloneable)
                cyclicalCloneable.CyclicalClone(ref contract);
            else if (source is ICloneable cloneable)
                contract.Add(source, cloneable.Clone());
            else
                contract.Add(source, source.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(source, null));
            return contract[source];
        }
        public static int GetHashCode(object source, ref IList<LeftRightPair<object, int>> hashCodes)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (hashCodes is null)
                throw new ArgumentNullException(nameof(hashCodes));
            foreach (LeftRightPair<object, int> pair in hashCodes)
                if (pair.Left == source)
                    return pair.Right;
            int hash = 0;
            if (source is ICyclical cyclical)
                return cyclical.GetHashCode(ref hashCodes);
            else if (source is Array array)
            {
                LeftRightPair<object, int> pair = new LeftRightPair<object, int>(source, hash);
                hashCodes.Add(pair);
                unchecked
                {
                    for (int i = 0; i < array.GetLength(0); ++i)
                    {
                        hash *= 2;
                        hash += GetHashCode(array.GetValue(i), ref hashCodes);
                    }
                }
                pair.Right = hash;
                return hash;
            }
            else
            {
                LeftRightPair<object, int> pair = new LeftRightPair<object, int>(source, source.GetHashCode());
                hashCodes.Add(pair);
                return pair.Right;
            }
        }
        public static object ToObject(XmlElement source, ref IDictionary<string, object> id_source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (id_source is null)
                throw new ArgumentNullException(nameof(id_source));
            if (source.HasAttribute("ref"))
            {
                if (id_source.ContainsKey(source.GetAttribute("ref")))
                    return id_source[source.GetAttribute("ref")];
                else
                    throw new XmlException($"The object reference must be after the object instance.\nref=\"{source.GetAttribute("ref")}\"");
            }
            else
            {
                if (source.HasAttribute("id"))
                    if (id_source.ContainsKey(source.GetAttribute("id")))
                        throw new XmlException($"Object with id=\"{source.GetAttribute("id")}\" has already been created.");
                if (source.HasAttribute("type"))
                {
                    string source_t = source.GetAttribute("type");
                    object source_object = null;
                    if (XmlElementIdConverters.ContainsKey(source_t))
                        source_object = XmlElementIdConverters[source_t](source, ref id_source);
                    else
                    {
                        if (XmlElementConverters.ContainsKey(source_t))
                            source_object = XmlElementConverters[source_t](source);
                        #region System types
                        else if (source_t == typeof(Boolean).FullName)
                            source_object = Convert.ToBoolean(source.InnerText);
                        else if (source_t == typeof(Byte).FullName)
                            source_object = Convert.ToByte(source.InnerText);
                        else if (source_t == typeof(Char).FullName)
                            source_object = Convert.ToChar(source.InnerText);
                        else if (source_t == typeof(DateTime).FullName)
                            source_object = Convert.ToDateTime(source.InnerText);
                        else if (source_t == typeof(Decimal).FullName)
                            source_object = Convert.ToDecimal(source.InnerText);
                        else if (source_t == typeof(Double).FullName)
                            source_object = Convert.ToDouble(source.InnerText);
                        else if (source_t == typeof(Int16).FullName)
                            source_object = Convert.ToInt16(source.InnerText);
                        else if (source_t == typeof(Int32).FullName)
                            source_object = Convert.ToInt32(source.InnerText);
                        else if (source_t == typeof(Int64).FullName)
                            source_object = Convert.ToInt64(source.InnerText);
                        else if (source_t == typeof(SByte).FullName)
                            source_object = Convert.ToSByte(source.InnerText);
                        else if (source_t == typeof(Single).FullName)
                            source_object = Convert.ToSingle(source.InnerText);
                        else if (source_t == typeof(String).FullName)
                            source_object = Convert.ToString(source.InnerText);
                        else if (source_t == typeof(UInt16).FullName)
                            source_object = Convert.ToUInt16(source.InnerText);
                        else if (source_t == typeof(UInt32).FullName)
                            source_object = Convert.ToUInt32(source.InnerText);
                        else if (source_t == typeof(UInt64).FullName)
                            source_object = Convert.ToUInt64(source.InnerText);
                        #endregion
                        if (source.HasAttribute("id"))
                            id_source.Add(source.GetAttribute("id"), source_object);
                    }
                    if (source_object == null)
                        throw new MissingMethodException($"There is no suitable method for converting a XML node to an object of type \"{source_t}\".");
                    return source_object;
                }
                else if (source.ChildNodes.Count == 1 && source.FirstChild is XmlText xmlText)
                {
                    if (source.HasAttribute("id"))
                        id_source.Add(source.GetAttribute("id"), xmlText.Value);
                    return xmlText.Value;
                }
                else if (source.ChildNodes.Count > 0)
                {
                    object[] source_o = new object[source.ChildNodes.Count];
                    if (source.HasAttribute("id"))
                        id_source.Add(source.GetAttribute("id"), source_o);
                    int i = 0;
                    foreach (XmlElement xmlElement in source)
                        source_o[i++] = ToObject(xmlElement, ref id_source);
                    return source_o;
                }
                else
                {
                    object[] source_a = new object[0];
                    if (source.HasAttribute("id"))
                        id_source.Add(source.GetAttribute("id"), source_a);
                    return source_a;
                }
            }
        }
        public static XmlElement ToXmlElement(object source, XmlDocument xmlDocument, ref IList<object> id_source, string name = "")
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (xmlDocument is null)
                throw new ArgumentNullException(nameof(xmlDocument));
            if (id_source is null)
                throw new ArgumentNullException(nameof(id_source));
            if (name is null)
                throw new ArgumentNullException(nameof(name));
            if (name == "")
                name = source.GetType().Name;
            if (id_source.Contains(source))
            {
                XmlElement xml_element = xmlDocument.CreateElement(name);
                xml_element.SetAttribute("ref", id_source.IndexOf(source).ToString("x16"));
                return xml_element;
            }
            else if (source is ICyclicalConvertible cyclicalConvertible)
                return cyclicalConvertible.ToXmlElement(xmlDocument, ref id_source, name);
            else if (source is IConvertible convertible)
            {
                XmlElement xml_element = convertible.ToXmlElement(xmlDocument, name);
                xml_element.SetAttribute("id", id_source.Count.ToString("x16"));
                id_source.Add(convertible);
                return xml_element;
            }
            else if (source is Array array)
            {
                XmlElement xml_element = xmlDocument.CreateElement(name);
                xml_element.SetAttribute("id", id_source.Count.ToString("x16"));
                id_source.Add(array);
                xml_element.SetAttribute("type", source.GetType().FullName);
                foreach (object obj in array)
                    xml_element.AppendChild(ToXmlElement(obj, xmlDocument, ref id_source, ""));
                return xml_element;
            }
            else
            {
                XmlElement xml_element = xmlDocument.CreateElement(name);
                xml_element.SetAttribute("type", source.GetType().FullName);
                xml_element.AppendChild(xmlDocument.CreateTextNode(source.ToString()));
                return xml_element;
            }
        }
    }
}