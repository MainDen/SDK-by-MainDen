// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using MainDen;
using MainDen.Automation;
using MainDen.Collections;
using MainDen.Collections.Generic;
using MainDen.Cyclical;
using MainDen.Enums;

namespace MainDen
{
    namespace Enums
    {
        [DefaultValue(Default)]
        public enum Route : uint
        {
            Empty = 0x0,
            Unit = 0x1,
            Linear = 0x2,
            PingPong = 0x3,
            Default = Linear,
        }
        [Flags]
        [DefaultValue(Default)]
        public enum RouteMode : uint
        {
            Empty = 0x0,
            Loop = 0x1,
            Reverse = 0x2,
            Default = Empty,
        }
        [DefaultValue(Empty)]
        public enum Status : uint
        {
            Empty = 0x0,
            Running = 0x1,
            Completed = 0x2,
            Stopped = 0x3,
            Error = 0x4,
        }
    }
    public interface IConvertible
    {
        XmlElement ToXmlElement(XmlDocument xmlDocument, string name);
    }
    namespace Cyclical
    {
        public interface ICyclical
        {
            int GetHashCode(ref IList<LeftRightPair<object, int>> hashCodes);
        }
        public interface ICyclicalCloneable : ICyclical, ICloneable
        {
            object CyclicalClone(ref IDictionary<object, object> Changes);
        }
        public interface ICyclicalConvertible : ICyclical, IConvertible
        {
            XmlElement ToXmlElement(XmlDocument xmlDocument, ref IList<object> id_soure, string name);
        }
        public static class CyclicalMethods
        {

            public delegate object TypeToObject<T>(T source);
            public delegate object TypeToIdObject<T>(T source, ref IDictionary<string, object> id_source);
            public static Dictionary<string, TypeToObject<XmlElement>> XmlElementConverters = new Dictionary<string, TypeToObject<XmlElement>>();
            public static Dictionary<string, TypeToIdObject<XmlElement>> XmlElementIdConverters = new Dictionary<string, TypeToIdObject<XmlElement>>();
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
                else if (source is Array array)
                {
                    object[] objects = new object[array.GetLength(0)];
                    contract.Add(source, objects);
                    for (int i = 0; i < array.GetLength(0); ++i)
                        objects[i] = Clone(array.GetValue(i), ref contract);
                }
                else
                    contract.Add(source, source.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(source, null));
                return contract[source];
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
                            #region Simple types
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
        }
    }
    namespace Collections
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
        public interface IGroupable
        {
            List<IGroup> Groups { get; }
            bool IncludedIn(IGroup group);
            void IncludeTo(IGroup group);
            void ExcludeFrom(IGroup group);
        }
        public interface IGroup
        {
            List<object> Entries { get; }
            List<object> Members { get; }
            List<IGroup> Subgroups { get; }
            void Include(object instance);
            void Exclude(object instance);
            bool Contains(object instance);
        }
        public interface IRouted
        {
            Route Route { get; set; }
            RouteMode RouteMode { get; set; }
            int? CurrentIndex { get; set; }
            int? BeginIndex { get; }
            int? EndIndex { get; }
            int? NextIndex { get; }
            object Current { get; }
            object Begin { get; }
            object End { get; }
            object Next { get; }
        }
        public class Obj : IObj, IGroupable, IDisposable, ICyclicalCloneable, ICloneable, ICyclicalConvertible, IConvertible, ICyclical
        {
            private readonly Dictionary<string, object> _properties;
            private readonly List<IGroup> _groups;
            public Dictionary<string, object> Properties
            {
                get
                {
                    Dictionary<string, object> properties = new Dictionary<string, object>();
                    foreach (string key in _properties.Keys)
                        properties.Add(key, _properties[key]);
                    return properties;
                }
            }
            public List<IGroup> Groups
            {
                get
                {
                    List<IGroup> groups = new List<IGroup>();
                    foreach (IGroup group in _groups)
                        groups.Add(group);
                    return groups;
                }
            }
            public bool TryGetProperty(string property, out object value)
            {
                if (property is null)
                    throw new ArgumentNullException(nameof(property));
                return _properties.TryGetValue(property, out value);
            }
            public void SetProperty(string property, object value)
            {
                if (property is null)
                    throw new ArgumentNullException(nameof(property));
                if (value is null)
                    throw new ArgumentNullException(nameof(value));
                _properties.Remove(property);
                _properties.Add(property, value);
            }
            public bool RemoveProperty(string property)
            {
                if (property is null)
                    throw new ArgumentNullException(nameof(property));
                return _properties.Remove(property);
            }
            public bool ContainsProperty(string property)
            {
                if (property is null)
                    throw new ArgumentNullException(nameof(property));
                return _properties.ContainsKey(property);
            }
            public object this[string property]
            {
                get
                {
                    if (property is null)
                        throw new ArgumentNullException(nameof(property));
                    if (!_properties.ContainsKey(property))
                        throw new ArgumentOutOfRangeException(nameof(property));
                    return _properties[property];
                }
                set
                {
                    if (property is null)
                        throw new ArgumentNullException(nameof(property));
                    if (value is null)
                        throw new ArgumentNullException(nameof(value));
                    if (!_properties.ContainsKey(property))
                        throw new ArgumentOutOfRangeException(nameof(property));
                    _properties[property] = value;
                }
            }
            public bool IncludedIn(IGroup group)
            {
                if (group is null)
                    throw new ArgumentNullException(nameof(group));
                return _groups.Contains(group);
            }
            void IGroupable.IncludeTo(IGroup group)
            {
                if (group is null)
                    throw new ArgumentNullException(nameof(group));
                if (!_groups.Contains(group))
                    _groups.Add(group);
            }
            void IGroupable.ExcludeFrom(IGroup group)
            {
                if (group is null)
                    throw new ArgumentNullException(nameof(group));
                _groups.Remove(group);
            }
            public void Dispose()
            {
                while (_groups.Count != 0)
                    _groups[0].Exclude(this);
            }
            public object Clone()
            {
                IDictionary<object, object> contract = new Dictionary<object, object>();
                return CyclicalClone(ref contract);
            }
            public object CyclicalClone(ref IDictionary<object, object> contract)
            {
                if (contract is null)
                    throw new ArgumentNullException(nameof(contract));
                Obj clone = new Obj();
                if (!contract.ContainsKey(this))
                    contract.Add(this, clone);
                foreach (string property in _properties.Keys)
                    clone.SetProperty(property, CyclicalMethods.Clone(this[property], ref contract));
                return clone;
            }
            public XmlElement ToXmlElement(XmlDocument xmlDocument, string name = "")
            {
                if (xmlDocument is null)
                    throw new ArgumentNullException(nameof(xmlDocument));
                if (name is null)
                    throw new ArgumentNullException(nameof(name));
                IList<object> is_source = new List<object>();
                return ToXmlElement(xmlDocument, ref is_source, name);
            }
            public XmlElement ToXmlElement(XmlDocument xmlDocument, ref IList<object> id_source, string name = "")
            {
                if (xmlDocument is null)
                    throw new ArgumentNullException(nameof(xmlDocument));
                if (id_source is null)
                    throw new ArgumentNullException(nameof(id_source));
                if (name is null)
                    throw new ArgumentNullException(nameof(name));
                if (name == "")
                    name = GetType().Name;
                XmlElement xmlThis = xmlDocument.CreateElement(name);
                if (id_source.Contains(this))
                    xmlThis.SetAttribute("ref", id_source.IndexOf(this).ToString("x16"));
                else
                {
                    xmlThis.SetAttribute("id", id_source.Count.ToString("x16"));
                    id_source.Add(this);
                    xmlThis.SetAttribute("type", GetType().FullName);
                    XmlElement xmlProperties = xmlDocument.CreateElement("Properties");
                    xmlThis.AppendChild(xmlProperties);
                    foreach (string property in _properties.Keys)
                    {
                        XmlElement xmlProperty = CyclicalMethods.ToXmlElement(this[property], xmlDocument, ref id_source, "Property");
                        xmlProperty.SetAttribute("key", property);
                        xmlProperties.AppendChild(xmlProperty);
                    }
                }
                return xmlThis;
            }
            public static Obj ToObject(XmlElement source)
            {
                if (source is null)
                    throw new ArgumentNullException(nameof(source));
                IDictionary<string, object> id_source = new Dictionary<string, object>();
                if (!CyclicalMethods.XmlElementConverters.ContainsKey(typeof(Obj).FullName))
                    CyclicalMethods.XmlElementConverters.Add(typeof(Obj).FullName, ToObject);
                if (!CyclicalMethods.XmlElementIdConverters.ContainsKey(typeof(Obj).FullName))
                    CyclicalMethods.XmlElementIdConverters.Add(typeof(Obj).FullName, ToObject);
                return ToObject(source, ref id_source);
            }
            public static Obj ToObject(XmlElement source, ref IDictionary<string, object> id_source)
            {
                if (source is null)
                    throw new ArgumentNullException(nameof(source));
                if (id_source is null)
                    throw new ArgumentNullException(nameof(id_source));
                if (!CyclicalMethods.XmlElementConverters.ContainsKey(typeof(Obj).FullName))
                    CyclicalMethods.XmlElementConverters.Add(typeof(Obj).FullName, ToObject);
                if (!CyclicalMethods.XmlElementIdConverters.ContainsKey(typeof(Obj).FullName))
                    CyclicalMethods.XmlElementIdConverters.Add(typeof(Obj).FullName, ToObject);
                if (source.HasAttribute("id"))
                    if (id_source.ContainsKey(source.GetAttribute("id")))
                        throw new XmlException($"Object with id=\"{source.GetAttribute("id")}\" has already been created.");
                Obj obj = new Obj();
                if (source.HasAttribute("id"))
                    id_source.Add(source.GetAttribute("id"), obj);
                XmlNodeList xmlPropertiesList = source.ChildNodes;
                int k = -1;
                for (int i = 0; i < xmlPropertiesList.Count; i++)
                    if (xmlPropertiesList[i].Name == "Properties")
                        if (k == -1)
                            k = i;
                        else
                            throw new XmlException($"XML representation of object of type \"{typeof(Obj).FullName}\" must contain one node \"Properties\".");
                if (k == -1)
                    throw new XmlException($"XML representation of object of type \"{typeof(Obj).FullName}\" must contain one node \"Properties\".");
                XmlElement xmlProperties = (XmlElement)xmlPropertiesList[k];
                foreach (XmlElement xmlProperty in xmlProperties.ChildNodes)
                    if (xmlProperty.Name != "Property")
                        throw new XmlException("The XML node \"Properties\" can only contain \"Property\" nodes.");
                    else if (xmlProperty.HasAttribute("key"))
                        if (obj.ContainsProperty(xmlProperty.GetAttribute("key")))
                            throw new XmlException($"Property with key=\"{xmlProperty.GetAttribute("key")}\" has already been created.");
                        else
                            obj.SetProperty(xmlProperty.GetAttribute("key"), CyclicalMethods.ToObject(xmlProperty, ref id_source));
                    else
                        throw new XmlException("XML node \"Property\" must contain the attribute \"key\".");
                return obj;
            }
            public int GetHashCode(ref IList<LeftRightPair<object, int>> hashCodes)
            {
                if (hashCodes is null)
                    throw new ArgumentNullException(nameof(hashCodes));
                foreach (LeftRightPair<object, int> pair in hashCodes)
                    if (pair.Left == this)
                        return pair.Right;
                LeftRightPair<object, int> sourceHash = new LeftRightPair<object, int>(this, 0);
                hashCodes.Add(sourceHash);
                unchecked
                {
                    int hashCode = 3371;
                    foreach (string property in _properties.Keys)
                    {
                        hashCode *= 2;
                        hashCode += CyclicalMethods.GetHashCode(_properties[property], ref hashCodes);
                    }
                    sourceHash.Right = hashCode;
                    return hashCode;
                }
            }
            public override int GetHashCode()
            {
                IList<LeftRightPair<object, int>> hashCodes = new List<LeftRightPair<object, int>>();
                return GetHashCode(ref hashCodes);
            }
            public override bool Equals(object obj)
            {
                return this == obj;
            }
            public Obj()
            {
                _properties = new Dictionary<string, object>();
                _groups = new List<IGroup>();
            }
        }
        public class Group : IGroup, IGroupable, IDisposable, ICyclicalCloneable, ICloneable, ICyclicalConvertible, IConvertible, ICyclical
        {
            private readonly List<object> _entries;
            private readonly List<IGroup> _groups;
            public List<object> Entries
            {
                get
                {
                    List<object> entries = new List<object>();
                    foreach (object entry in _entries)
                        entries.Add(entry);
                    return entries;
                }
            }
            public List<object> Members
            {
                get
                {
                    List<object> members = new List<object>();
                    foreach (object entry in _entries)
                        if (!(entry is IGroup))
                            members.Add(entry);
                    return members;
                }
            }
            public List<IGroup> Subgroups
            {
                get
                {
                    List<IGroup> subgroups = new List<IGroup>();
                    foreach (object entry in _entries)
                        if (entry is IGroup subgroup)
                            subgroups.Add(subgroup);
                    return subgroups;
                }
            }
            public List<IGroup> Groups
            {
                get
                {
                    List<IGroup> groups = new List<IGroup>();
                    foreach (IGroup group in _groups)
                        groups.Add(group);
                    return groups;
                }
            }
            public void Include(object instance)
            {
                if (instance is null)
                    throw new ArgumentNullException(nameof(instance));
                if (!_entries.Contains(instance))
                    _entries.Add(instance);
                if (instance is IGroupable groupable)
                    groupable.IncludeTo(this);
            }
            public void Exclude(object instance)
            {
                if (instance is null)
                    throw new ArgumentNullException(nameof(instance));
                _entries.Remove(instance);
                if (instance is IGroupable groupable)
                    groupable.ExcludeFrom(this);
            }
            public bool Contains(object instance)
            {
                if (instance is null)
                    throw new ArgumentNullException(nameof(instance));
                return Entries.Contains(instance);
            }
            public bool IncludedIn(IGroup group)
            {
                if (group is null)
                    throw new ArgumentNullException(nameof(group));
                return Groups.Contains(group);
            }
            void IGroupable.IncludeTo(IGroup group)
            {
                if (group is null)
                    throw new ArgumentNullException(nameof(group));
                if (!_groups.Contains(group))
                    _groups.Add(group);
            }
            void IGroupable.ExcludeFrom(IGroup group)
            {
                if (group is null)
                    throw new ArgumentNullException(nameof(group));
                _groups.Remove(group);
            }
            public void Dispose()
            {
                while (_entries.Count != 0)
                    Exclude(_entries[0]);
                while (_groups.Count != 0)
                    _groups[0].Exclude(this);
            }
            public object Clone()
            {
                IDictionary<object, object> contract = new Dictionary<object, object>();
                return CyclicalClone(ref contract);
            }
            public object CyclicalClone(ref IDictionary<object, object> contract)
            {
                if (contract is null)
                    throw new ArgumentNullException(nameof(contract));
                IGroup clone = new Group();
                if (!contract.ContainsKey(this))
                    contract.Add(this, clone);
                foreach (object entry in _entries)
                    clone.Include(CyclicalMethods.Clone(entry, ref contract));
                return clone;
            }
            public XmlElement ToXmlElement(XmlDocument xmlDocument, string name = "")
            {
                if (xmlDocument is null)
                    throw new ArgumentNullException(nameof(xmlDocument));
                if (name is null)
                    throw new ArgumentNullException(nameof(name));
                IList<object> is_source = new List<object>();
                return ToXmlElement(xmlDocument, ref is_source, name);
            }
            public XmlElement ToXmlElement(XmlDocument xmlDocument, ref IList<object> id_source, string name = "")
            {
                if (xmlDocument is null)
                    throw new ArgumentNullException(nameof(xmlDocument));
                if (id_source is null)
                    throw new ArgumentNullException(nameof(id_source));
                if (name is null)
                    throw new ArgumentNullException(nameof(name));
                if (name == "")
                    name = GetType().Name;
                XmlElement xmlThis = xmlDocument.CreateElement(name);
                if (id_source.Contains(this))
                    xmlThis.SetAttribute("ref", id_source.IndexOf(this).ToString("x16"));
                else
                {
                    xmlThis.SetAttribute("id", id_source.Count.ToString("x16"));
                    id_source.Add(this);
                    xmlThis.SetAttribute("type", GetType().FullName);
                    XmlElement xmlEntries = xmlDocument.CreateElement("Entries");
                    xmlThis.AppendChild(xmlEntries);
                    foreach (object entry in Entries)
                    {
                        XmlElement xmlEntry = CyclicalMethods.ToXmlElement(entry, xmlDocument, ref id_source, "Entry");
                        xmlEntries.AppendChild(xmlEntry);
                    }
                }
                return xmlThis;
            }
            public static Group ToObject(XmlElement source)
            {
                if (source is null)
                    throw new ArgumentNullException(nameof(source));
                if (!CyclicalMethods.XmlElementConverters.ContainsKey(typeof(Group).FullName))
                    CyclicalMethods.XmlElementConverters.Add(typeof(Group).FullName, ToObject);
                if (!CyclicalMethods.XmlElementIdConverters.ContainsKey(typeof(Group).FullName))
                    CyclicalMethods.XmlElementIdConverters.Add(typeof(Group).FullName, ToObject);
                IDictionary<string, object> id_source = new Dictionary<string, object>();
                return ToObject(source, ref id_source);
            }
            public static Group ToObject(XmlElement source, ref IDictionary<string, object> id_source)
            {
                if (source is null)
                    throw new ArgumentNullException(nameof(source));
                if (id_source is null)
                    throw new ArgumentNullException(nameof(id_source));
                if (!CyclicalMethods.XmlElementConverters.ContainsKey(typeof(Group).FullName))
                    CyclicalMethods.XmlElementConverters.Add(typeof(Group).FullName, ToObject);
                if (!CyclicalMethods.XmlElementIdConverters.ContainsKey(typeof(Group).FullName))
                    CyclicalMethods.XmlElementIdConverters.Add(typeof(Group).FullName, ToObject);
                if (source.HasAttribute("id"))
                    if (id_source.ContainsKey(source.GetAttribute("id")))
                        throw new XmlException($"Object with id=\"{source.GetAttribute("id")}\" has already been created.");
                Group group = new Group();
                if (source.HasAttribute("id"))
                    id_source.Add(source.GetAttribute("id"), group);
                XmlNodeList xmlEntriesList = source.ChildNodes;
                int k = -1;
                for (int i = 0; i < xmlEntriesList.Count; i++)
                    if (xmlEntriesList[i].Name == "Entries")
                        if (k == -1)
                            k = i;
                        else
                            throw new XmlException($"XML representation of object of type \"{typeof(Group).FullName}\" must contain one node \"Entries\".");
                if (k == -1)
                    throw new XmlException($"XML representation of object of type \"{typeof(Group).FullName}\" must contain one node \"Entries\".");
                XmlElement xmlEntries = (XmlElement)xmlEntriesList[k];
                foreach (XmlElement xmlEntry in xmlEntries.ChildNodes)
                    if (xmlEntry.Name != "Entry")
                        throw new XmlException("The XML node \"Entries\" can only contain \"Entry\" nodes.");
                    else
                        group.Include(CyclicalMethods.ToObject(xmlEntry, ref id_source));
                return group;
            }
            public int GetHashCode(ref IList<LeftRightPair<object, int>> hashCodes)
            {
                if (hashCodes is null)
                    throw new ArgumentNullException(nameof(hashCodes));
                foreach (LeftRightPair<object, int> pair in hashCodes)
                    if (pair.Left == this)
                        return pair.Right;
                LeftRightPair<object, int> sourceHash = new LeftRightPair<object, int>(this, 0);
                hashCodes.Add(sourceHash);
                unchecked
                {
                    int hashCode = 1733;
                    foreach (object entry in Entries)
                    {
                        hashCode *= 2;
                        hashCode += CyclicalMethods.GetHashCode(entry, ref hashCodes);
                    }
                    sourceHash.Right = hashCode;
                    return hashCode;
                }
            }
            public override int GetHashCode()
            {
                IList<LeftRightPair<object, int>> hashCodes = new List<LeftRightPair<object, int>>();
                return GetHashCode(ref hashCodes);
            }
            public Group()
            {
                _entries = new List<object>();
                _groups = new List<IGroup>();
            }
            public Group(IGroup group) : this()
            {
                if (group is null)
                    throw new ArgumentNullException(nameof(group));
                foreach (object entry in group.Entries)
                    Include(entry);
            }
        }
        namespace Generic
        {
            public interface IGroupable<T> : IGroupable
            {
                new List<IGroup<T>> Groups { get; }
                bool IncludedIn(IGroup<T> group);
                void IncludeTo(IGroup<T> group);
                void ExcludeFrom(IGroup<T> group);
            }
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
            public class Group<T> : IGroup<T>, IGroupable<T>, IGroup, IGroupable, IDisposable, ICyclicalCloneable, ICloneable, ICyclicalConvertible, IConvertible, ICyclical
            {
                private readonly List<object> _entries;
                private readonly List<IGroup<T>> _groups;
                public List<object> Entries
                {
                    get
                    {
                        List<object> entries = new List<object>();
                        foreach (object entry in _entries)
                            entries.Add(entry);
                        return entries;
                    }
                }
                List<object> IGroup.Members
                {
                    get
                    {
                        List<object> members = new List<object>();
                        foreach (object entry in _entries)
                            if (!(entry is IGroup<T>))
                                members.Add(entry);
                        return members;
                    }
                }
                public List<T> Members
                {
                    get
                    {
                        List<T> members = new List<T>();
                        foreach (object entry in _entries)
                            if (!(entry is IGroup<T>))
                                members.Add((T)entry);
                        return members;
                    }
                }
                List<IGroup> IGroup.Subgroups
                {
                    get
                    {
                        List<IGroup> subgroups = new List<IGroup>();
                        foreach (object entry in _entries)
                            if (entry is IGroup<T> subgroup)
                                subgroups.Add(subgroup);
                        return subgroups;
                    }
                }
                public List<IGroup<T>> Subgroups
                {
                    get
                    {
                        List<IGroup<T>> subgroups = new List<IGroup<T>>();
                        foreach (object entry in _entries)
                            if (entry is IGroup<T> subgroup)
                                subgroups.Add(subgroup);
                        return subgroups;
                    }
                }
                List<IGroup> IGroupable.Groups
                {
                    get
                    {
                        List<IGroup> groups = new List<IGroup>();
                        foreach (IGroup<T> group in _groups)
                            groups.Add(group);
                        return groups;
                    }
                }
                public List<IGroup<T>> Groups
                {
                    get
                    {
                        List<IGroup<T>> groups = new List<IGroup<T>>();
                        foreach (IGroup<T> group in _groups)
                            groups.Add(group);
                        return groups;
                    }
                }
                public void Include(T instance)
                {
                    if (instance == null)
                        throw new ArgumentNullException(nameof(instance));
                    if (!_entries.Contains(instance))
                        _entries.Add(instance);
                    if (instance is IGroupable<T> groupable)
                        groupable.IncludeTo(this);
                }
                public void Include(IGroup<T> instance)
                {
                    if (instance is null)
                        throw new ArgumentNullException(nameof(instance));
                    if (!_entries.Contains(instance))
                        _entries.Add(instance);
                    if (instance is IGroupable<T> groupable)
                        groupable.IncludeTo(this);
                }
                public void Include(object instance)
                {
                    if (instance is null)
                        throw new ArgumentNullException(nameof(instance));
                    if (!(instance is T || instance is IGroup<T>))
                        throw new ArgumentException($"{nameof(instance)} is of a type that cannot be assigned to IGroup.");
                    if (!_entries.Contains(instance))
                        _entries.Add(instance);
                    if (instance is IGroupable<T> groupable)
                        groupable.IncludeTo(this);
                }
                public void Exclude(T instance)
                {
                    if (instance == null)
                        throw new ArgumentNullException(nameof(instance));
                    _entries.Remove(instance);
                    if (instance is IGroupable<T> groupable)
                        groupable.ExcludeFrom(this);
                }
                public void Exclude(IGroup<T> instance)
                {
                    if (instance is null)
                        throw new ArgumentNullException(nameof(instance));
                    _entries.Remove(instance);
                    if (instance is IGroupable<T> groupable)
                        groupable.ExcludeFrom(this);
                }
                public void Exclude(object instance)
                {
                    if (instance is null)
                        throw new ArgumentNullException(nameof(instance));
                    if (!(instance is T || instance is IGroup<T>))
                        throw new ArgumentException($"{nameof(instance)} is of a type that cannot be assigned to IGroup.");
                    _entries.Remove(instance);
                    if (instance is IGroupable<T> groupable)
                        groupable.ExcludeFrom(this);
                }
                public bool Contains(T instance)
                {
                    if (instance == null)
                        throw new ArgumentNullException(nameof(instance));
                    return Entries.Contains(instance);
                }
                public bool Contains(IGroup<T> instance)
                {
                    if (instance is null)
                        throw new ArgumentNullException(nameof(instance));
                    return Entries.Contains(instance);
                }
                public bool Contains(object instance)
                {
                    if (instance is null)
                        throw new ArgumentNullException(nameof(instance));
                    if (!(instance is T || instance is IGroup<T>))
                        throw new ArgumentException($"{nameof(instance)} is of a type that cannot be assigned to IGroup.");
                    return Entries.Contains(instance);
                }
                public bool IncludedIn(IGroup<T> group)
                {
                    if (group is null)
                        throw new ArgumentNullException(nameof(group));
                    return Groups.Contains(group);
                }
                bool IGroupable.IncludedIn(IGroup group)
                {
                    if (group is null)
                        throw new ArgumentNullException(nameof(group));
                    if (!(group is IGroup<T>))
                        throw new ArgumentException($"{nameof(group)} is of a type that cannot be assigned to IGroup.");
                    return Groups.Contains((IGroup<T>)group);
                }
                void IGroupable<T>.IncludeTo(IGroup<T> group)
                {
                    if (group is null)
                        throw new ArgumentNullException(nameof(group));
                    if (!_groups.Contains(group))
                        _groups.Add(group);
                }
                void IGroupable.IncludeTo(IGroup group)
                {
                    if (group is null)
                        throw new ArgumentNullException(nameof(group));
                    if (!(group is IGroup<T>))
                        throw new ArgumentException($"{nameof(group)} is of a type that cannot be assigned to IGroup.");
                    if (!_groups.Contains((IGroup<T>)group))
                        _groups.Add((IGroup<T>)group);
                }
                void IGroupable<T>.ExcludeFrom(IGroup<T> group)
                {
                    if (group is null)
                        throw new ArgumentNullException(nameof(group));
                    _groups.Remove(group);
                }
                void IGroupable.ExcludeFrom(IGroup group)
                {
                    if (group is null)
                        throw new ArgumentNullException(nameof(group));
                    if (!(group is IGroup<T>))
                        throw new ArgumentException($"{nameof(group)} is of a type that cannot be assigned to IGroup.");
                    _groups.Remove((IGroup<T>)group);
                }
                public void Dispose()
                {
                    while (_entries.Count != 0)
                        Exclude(_entries[0]);
                    while (_groups.Count != 0)
                        _groups[0].Exclude(this);
                }
                public object Clone()
                {
                    IDictionary<object, object> contract = new Dictionary<object, object>();
                    return CyclicalClone(ref contract);
                }
                public object CyclicalClone(ref IDictionary<object, object> contract)
                {
                    if (contract is null)
                        throw new ArgumentNullException(nameof(contract));
                    IGroup<T> clone = new Group<T>();
                    if (!contract.ContainsKey(this))
                        contract.Add(this, clone);
                    foreach (object entry in _entries)
                        clone.Include(CyclicalMethods.Clone(entry, ref contract));
                    return clone;
                }
                public XmlElement ToXmlElement(XmlDocument xmlDocument, string name = "")
                {
                    if (xmlDocument is null)
                        throw new ArgumentNullException(nameof(xmlDocument));
                    if (name is null)
                        throw new ArgumentNullException(nameof(name));
                    IList<object> is_source = new List<object>();
                    return ToXmlElement(xmlDocument, ref is_source, name);
                }
                public XmlElement ToXmlElement(XmlDocument xmlDocument, ref IList<object> id_source, string name = "")
                {
                    if (xmlDocument is null)
                        throw new ArgumentNullException(nameof(xmlDocument));
                    if (id_source is null)
                        throw new ArgumentNullException(nameof(id_source));
                    if (name is null)
                        throw new ArgumentNullException(nameof(name));
                    if (name == "")
                        name = GetType().Name;
                    XmlElement xmlThis = xmlDocument.CreateElement(name);
                    if (id_source.Contains(this))
                        xmlThis.SetAttribute("ref", id_source.IndexOf(this).ToString("x16"));
                    else
                    {
                        xmlThis.SetAttribute("id", id_source.Count.ToString("x16"));
                        id_source.Add(this);
                        xmlThis.SetAttribute("type", GetType().FullName);
                        XmlElement xmlEntries = xmlDocument.CreateElement("Entries");
                        xmlThis.AppendChild(xmlEntries);
                        foreach (object entry in Entries)
                        {
                            XmlElement xmlEntry = CyclicalMethods.ToXmlElement(entry, xmlDocument, ref id_source, "Entry");
                            xmlEntries.AppendChild(xmlEntry);
                        }
                    }
                    return xmlThis;
                }
                public static Group<T> ToObject(XmlElement source)
                {
                    if (source is null)
                        throw new ArgumentNullException(nameof(source));
                    if (!CyclicalMethods.XmlElementConverters.ContainsKey(typeof(Group<T>).FullName))
                        CyclicalMethods.XmlElementConverters.Add(typeof(Group<T>).FullName, ToObject);
                    if (!CyclicalMethods.XmlElementIdConverters.ContainsKey(typeof(Group<T>).FullName))
                        CyclicalMethods.XmlElementIdConverters.Add(typeof(Group<T>).FullName, ToObject);
                    IDictionary<string, object> id_source = new Dictionary<string, object>();
                    return ToObject(source, ref id_source);
                }
                public static Group<T> ToObject(XmlElement source, ref IDictionary<string, object> id_source)
                {
                    if (source is null)
                        throw new ArgumentNullException(nameof(source));
                    if (id_source is null)
                        throw new ArgumentNullException(nameof(id_source));
                    if (!CyclicalMethods.XmlElementConverters.ContainsKey(typeof(Group<T>).FullName))
                        CyclicalMethods.XmlElementConverters.Add(typeof(Group<T>).FullName, ToObject);
                    if (!CyclicalMethods.XmlElementIdConverters.ContainsKey(typeof(Group<T>).FullName))
                        CyclicalMethods.XmlElementIdConverters.Add(typeof(Group<T>).FullName, ToObject);
                    if (source.HasAttribute("id"))
                        if (id_source.ContainsKey(source.GetAttribute("id")))
                            throw new XmlException($"Object with id=\"{source.GetAttribute("id")}\" has already been created.");
                    Group<T> group = new Group<T>();
                    if (source.HasAttribute("id"))
                        id_source.Add(source.GetAttribute("id"), group);
                    XmlNodeList xmlEntriesList = source.ChildNodes;
                    int k = -1;
                    for (int i = 0; i < xmlEntriesList.Count; i++)
                        if (xmlEntriesList[i].Name == "Entries")
                            if (k == -1)
                                k = i;
                            else
                                throw new XmlException($"XML representation of object of type \"{typeof(Group<T>).FullName}\" must contain one node \"Entries\".");
                    if (k == -1)
                        throw new XmlException($"XML representation of object of type \"{typeof(Group<T>).FullName}\" must contain one node \"Entries\".");
                    XmlElement xmlEntries = (XmlElement)xmlEntriesList[k];
                    foreach (XmlElement xmlEntry in xmlEntries.ChildNodes)
                        if (xmlEntry.Name != "Entry")
                            throw new XmlException("The XML node \"Entries\" can only contain \"Entry\" nodes.");
                        else
                            group.Include(CyclicalMethods.ToObject(xmlEntry, ref id_source));
                    return group;
                }
                public int GetHashCode(ref IList<LeftRightPair<object, int>> hashCodes)
                {
                    if (hashCodes is null)
                        throw new ArgumentNullException(nameof(hashCodes));
                    foreach (LeftRightPair<object, int> pair in hashCodes)
                        if (pair.Left == this)
                            return pair.Right;
                    LeftRightPair<object, int> sourceHash = new LeftRightPair<object, int>(this, 0);
                    hashCodes.Add(sourceHash);
                    unchecked
                    {
                        int hashCode = 1733;
                        foreach (object entry in Entries)
                        {
                            hashCode *= 2;
                            hashCode += CyclicalMethods.GetHashCode(entry, ref hashCodes);
                        }
                        sourceHash.Right = hashCode;
                        return hashCode;
                    }
                }
                public override int GetHashCode()
                {
                    IList<LeftRightPair<object, int>> hashCodes = new List<LeftRightPair<object, int>>();
                    return GetHashCode(ref hashCodes);
                }
                public Group()
                {
                    _entries = new List<object>();
                    _groups = new List<IGroup<T>>();
                }
                public Group(IGroup<T> group) : this()
                {
                    if (group is null)
                        throw new ArgumentNullException(nameof(group));
                    foreach (object entry in group.Entries)
                        Include(entry);
                }
            }
            public class LeftRightPair<TLeft, TRight>
            {
                TLeft left;
                TRight right;
                public TLeft Left
                {
                    get
                    {
                        return left;
                    }
                    set
                    {
                        left = value;
                    }
                }
                public TRight Right
                {
                    get
                    {
                        return right;
                    }
                    set
                    {
                        right = value;
                    }
                }
                public LeftRightPair(TLeft left, TRight right)
                {
                    this.left = left;
                    this.right = right;
                }
            }
            public class RoutedList<T> : List<T>, ICollection<T>, IEnumerable<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, IList, IRouted
            {
                private Route _route;
                private RouteMode _routeMode;
                private bool _increase;
                private int? _current;
                private int? _begin;
                private int? _end;
                private int? _next;
                private List<T> _list;
                private int? CInd
                {
                    get
                    {
                        if (Count == 0)
                            return null;
                        if (_route == Route.Empty)
                            return null;
                        if (_routeMode.HasFlag(RouteMode.Reverse))
                            return Count - 1;
                        else
                            return 0;
                    }
                }
                private int? BInd
                {
                    get
                    {
                        if (Count == 0)
                            return null;
                        if (_route == Route.Empty)
                            return null;
                        if (_routeMode.HasFlag(RouteMode.Loop))
                            return null;
                        if (_routeMode.HasFlag(RouteMode.Reverse))
                        {
                            if (_route == Route.Unit)
                                return _current;
                            if (_route == Route.Linear)
                                return Count - 1;
                            if (_route == Route.PingPong)
                                return Count - 1;
                        }
                        else
                        {
                            if (_route == Route.Unit)
                                return _current;
                            if (_route == Route.Linear)
                                return 0;
                            if (_route == Route.PingPong)
                                return 0;
                        }
                        return null;
                    }
                }
                private int? EInd
                {
                    get
                    {
                        if (Count == 0)
                            return null;
                        if (_route == Route.Empty)
                            return null;
                        if (_routeMode.HasFlag(RouteMode.Loop))
                            return null;
                        if (_routeMode.HasFlag(RouteMode.Reverse))
                        {
                            if (_route == Route.Unit)
                                return _current;
                            if (_route == Route.Linear)
                                return 0;
                            if (_route == Route.PingPong)
                                return Count - 1;
                        }
                        else
                        {
                            if (_route == Route.Unit)
                                return _current;
                            if (_route == Route.Linear)
                                return Count - 1;
                            if (_route == Route.PingPong)
                                return 0;
                        }
                        return null;
                    }
                }
                private int? NInd
                {
                    get
                    {
                        if (Count == 0)
                            return null;
                        if (_route == Route.Empty)
                            return null;
                        if (_route == Route.Unit)
                            if (_routeMode.HasFlag(RouteMode.Loop))
                                return _current;
                            else
                                return null;
                        if (_current == null)
                            return null;
                        if (!_routeMode.HasFlag(RouteMode.Loop) && _current == _end)
                        {
                            if (_route == Route.Linear)
                                return null;
                            if (_route == Route.PingPong && (_routeMode.HasFlag(RouteMode.Reverse) == _increase))
                                return null;
                        }
                        int _next = (int)_current;
                        if (_increase)
                            _next++;
                        else
                            _next--;
                        if (_next == Count)
                        {
                            if (_route == Route.Linear)
                                _next = 0;
                            if (_route == Route.PingPong)
                                _next = Count - 2;
                        }
                        else if (_next == -1)
                        {
                            if (_route == Route.Linear)
                                _next = Count - 1;
                            if (_route == Route.PingPong)
                                _next = 1;
                        }
                        if (_next >= Count)
                            _next = Count - 1;
                        if (_next < 0)
                            _next = 0;
                        return _next;
                    }
                }
                private List<T> Lst
                {
                    get
                    {
                        List<T> list = new List<T>();
                        if (_route == Route.Empty || Count == 0 || _current == null)
                            return list;
                        int c = (int)_current;
                        if (_route == Route.Unit)
                        {
                            list.Add(this[c]);
                            return list;
                        }
                        if (_route == Route.Linear)
                        {
                            int i = c;
                            int step = _increase ? 1 : -1;
                            for (; i >= 0 && i < Count; i += step)
                                list.Add(this[i]);
                            if (i < 0)
                                i = Count - 1;
                            if (i >= Count)
                                i = 0;
                            if (_routeMode.HasFlag(RouteMode.Loop))
                                for (; i != c; i += step)
                                    list.Add(this[i]);
                            return list;
                        }
                        if (_route == Route.PingPong)
                        {
                            int i = c;
                            int last = Count - 1;
                            int step = _increase ? 1 : -1;
                            for (; i >= 0 && i <= last; i += step)
                                list.Add(this[i]);
                            if (i < 0)
                                i = 0;
                            if (i > last)
                                i = last;
                            if (i == _end && _increase == _routeMode.HasFlag(RouteMode.Reverse) && !_routeMode.HasFlag(RouteMode.Loop))
                                return list;
                            step = -step;
                            i += step;
                            if (i < 0)
                                i = 0;
                            if (i > last)
                                i = last;
                            for (; i > 0 && i < last; i += step)
                                list.Add(this[i]);
                            if (i != c || !_routeMode.HasFlag(RouteMode.Loop))
                                list.Add(this[i]);
                            if (i == _end && !_routeMode.HasFlag(RouteMode.Loop) || i == c)
                                return list;
                            step = -step;
                            i += step;
                            for (; i != c; i += step)
                                list.Add(this[i]);
                            return list;
                        }
                        return list;
                    }
                }
                public RoutedList() : base()
                {
                    _route = Route.Default;
                    _routeMode = RouteMode.Default;
                    _increase = !_routeMode.HasFlag(RouteMode.Reverse);
                    _current = CInd;
                    _begin = BInd;
                    _end = EInd;
                    _next = NInd;
                    _list = null;
                }
                public RoutedList(Route route, RouteMode routeMode) : base()
                {
                    _route = route;
                    _routeMode = routeMode;
                    _increase = !_routeMode.HasFlag(RouteMode.Reverse);
                    _current = CInd;
                    _begin = BInd;
                    _end = EInd;
                    _next = NInd;
                    _list = null;
                }
                public RoutedList(IEnumerable<T> collection) : base(collection)
                {
                    _route = Route.Default;
                    _routeMode = RouteMode.Default;
                    _increase = !_routeMode.HasFlag(RouteMode.Reverse);
                    _current = CInd;
                    _begin = BInd;
                    _end = EInd;
                    _next = NInd;
                    _list = null;
                }
                public RoutedList(IEnumerable<T> collection, Route route, RouteMode routeMode) : base(collection)
                {
                    _route = route;
                    _routeMode = routeMode;
                    _increase = !_routeMode.HasFlag(RouteMode.Reverse);
                    _current = CInd;
                    _begin = BInd;
                    _end = EInd;
                    _next = NInd;
                    _list = null;
                }
                public RoutedList(int capacity) : base(capacity)
                {
                    _route = Route.Default;
                    _routeMode = RouteMode.Default;
                    _increase = !_routeMode.HasFlag(RouteMode.Reverse);
                    _current = CInd;
                    _begin = BInd;
                    _end = EInd;
                    _next = NInd;
                    _list = null;
                }
                public RoutedList(int capacity, Route route, RouteMode routeMode) : base(capacity)
                {
                    _route = route;
                    _routeMode = routeMode;
                    _increase = !_routeMode.HasFlag(RouteMode.Reverse);
                    _current = CInd;
                    _begin = BInd;
                    _end = EInd;
                    _next = NInd;
                    _list = null;
                }
                public RoutedList(RoutedList<T> source) : base(source)
                {
                    _route = source._route;
                    _routeMode = source._routeMode;
                    _increase = source._increase;
                    _current = source._current;
                    _begin = source._begin;
                    _end = source._end;
                    _next = source._next;
                    _list = source._list;
                }
                public int? CurrentIndex
                {
                    get
                    {
                        return _current;
                    }
                    set
                    {
                        _current = value;
                        if (_current != null && (_current < 0 || _current >= Count))
                            _current = null;
                        _next = NInd;
                    }
                }
                public int? BeginIndex
                {
                    get
                    {
                        return _begin;
                    }
                }
                public int? EndIndex
                {
                    get
                    {
                        return _end;
                    }
                }
                public int? NextIndex
                {
                    get
                    {
                        return _next;
                    }
                }
                public object Current
                {
                    get
                    {
                        if (_current != null && _current >= 0 && _current < Count)
                            return this[(int)_current];
                        return null;
                    }
                }
                public object Begin
                {
                    get
                    {
                        if (_begin != null && _begin >= 0 && _begin < Count)
                            return this[(int)_begin];
                        return null;
                    }
                }
                public object End
                {
                    get
                    {
                        if (_end != null && _end >= 0 && _end < Count)
                            return this[(int)_end];
                        return null;
                    }
                }
                public object Next
                {
                    get
                    {
                        if (_next != null && _next >= 0 && _next < Count)
                            return this[(int)_next];
                        return null;
                    }
                }
                public List<T> List
                {
                    get
                    {
                        if (_list == null)
                            _list = Lst;
                        List<T> list = new List<T>(_list.Count);
                        for (int i = 0; i < _list.Count; i++)
                            list.Add(_list[i]);
                        return list;
                    }
                }
                public Route Route
                {
                    get
                    {
                        return _route;
                    }
                    set
                    {
                        bool chPingPong = _route == Route.PingPong && value != Route.PingPong;
                        if (chPingPong)
                            _increase = !_routeMode.HasFlag(RouteMode.Reverse);
                        _route = value;
                        _begin = BInd;
                        _end = EInd;
                        _next = NInd;
                        _list = null;
                    }
                }
                public RouteMode RouteMode
                {
                    get
                    {
                        return _routeMode;
                    }
                    set
                    {
                        bool chReverse = _routeMode.HasFlag(RouteMode.Reverse) != value.HasFlag(RouteMode.Reverse);
                        if (chReverse)
                            _increase = !_increase;
                        _routeMode = value;
                        _begin = BInd;
                        _end = EInd;
                        _next = NInd;
                        _list = null;
                    }
                }
                public void MoveNext()
                {
                    if (_current is null)
                        return;
                    if (_route == Route.PingPong && _current != null && _next != null && _current < _next != _increase)
                        _increase = !_increase;
                    _current = _next;
                    _next = NInd;
                    _list = null;
                }
                public void Reset()
                {
                    _increase = !_routeMode.HasFlag(RouteMode.Reverse);
                    _current = CInd;
                    _next = NInd;
                    _list = null;
                }
                public void Update()
                {
                    if (_current != null && (_current < 0 || _current >= Count))
                        _current = null;
                    _begin = BInd;
                    _end = EInd;
                    _next = NInd;
                    _list = null;
                }
                public void Update(int? current)
                {
                    _current = current;
                    if (_current != null && (_current < 0 || _current >= Count))
                        _current = null;
                    _begin = BInd;
                    _end = EInd;
                    _next = NInd;
                    _list = null;
                }
                public void Update(int index, int offset)
                {
                    if (_current != null)
                    {
                        if (offset > 0 && index <= _current)
                            _current += offset;
                        else if (offset < 0)
                            if (index <= _current + offset)
                                _current += offset;
                            else if (index <= _current)
                                _current = null;
                        if (_current < 0 || _current >= Count)
                            _current = null;
                    }
                    _begin = BInd;
                    _end = EInd;
                    _next = NInd;
                    _list = null;
                }
                // Use Update
                public new void Add(T item)
                {
                    base.Add(item);
                    Update();
                }
                public new void AddRange(IEnumerable<T> collection)
                {
                    if (collection is null)
                        throw new ArgumentNullException(nameof(collection));
                    base.AddRange(collection);
                    Update();
                }
                public new void Clear()
                {
                    base.Clear();
                    Update(null);
                }
                public new void Insert(int index, T item)
                {
                    if (index < 0 || index > Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    base.Insert(index, item);
                    Update(index, 1);
                }
                public new void InsertRange(int index, IEnumerable<T> collection)
                {
                    if (index < 0 || index > Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    if (collection is null)
                        throw new ArgumentNullException(nameof(collection));
                    base.InsertRange(index, collection);
                    int count = 0;
                    foreach (T item in collection)
                        count++;
                    Update(index, count);
                }
                public new bool Remove(T item)
                {
                    int index = IndexOf(item);
                    if (index != -1)
                    {
                        base.RemoveAt(index);
                        Update(index, -1);
                        return true;
                    }
                    return false;
                }
                public new int RemoveAll(Predicate<T> match)
                {
                    if (match is null)
                        throw new ArgumentNullException(nameof(match));
                    int index = 0;
                    int count = 0;
                    while (index < Count)
                    {
                        if (match(this[index]))
                        {
                            base.RemoveAt(index);
                            Update(index, -1);
                            count++;
                        }
                        else
                            index++;
                    }
                    return count;
                }
                public new void RemoveAt(int index)
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    base.RemoveAt(index);
                    Update(index, -1);
                }
                public new void RemoveRange(int index, int count)
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    if (count < 0)
                        throw new ArgumentOutOfRangeException(nameof(count));
                    if (index + count > Count)
                        throw new ArgumentException($"Sum of \"{nameof(index)}\"={index} and \"{nameof(count)}\"={count} must not be more then \"{nameof(Count)}\"={Count}.");
                    base.RemoveRange(index, count);
                    Update(index, -count);
                }
                public new void Reverse()
                {
                    base.Reverse();
                    if (Count > 1)
                        if (_current != null)
                            Update(Count - 1 - _current);
                        else
                            Update();
                }
                public new void Reverse(int index, int count)
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    if (count < 0)
                        throw new ArgumentOutOfRangeException(nameof(count));
                    if (index + count > Count)
                        throw new ArgumentException($"Sum of \"{nameof(index)}\"={index} and \"{nameof(count)}\"={count} must not be more then \"{nameof(Count)}\"={Count}.");
                    base.Reverse(index, count);
                    if (count > 1)
                        if (_current != null && index <= _current && index + count > _current)
                            Update(2 * index + count - 1 - _current);
                        else
                            Update();
                }
                public new void Sort(Comparison<T> comparison)
                {
                    if (comparison is null)
                        throw new ArgumentNullException(nameof(comparison));
                    bool notSorted = true;
                    int index = -1;
                    if (_current != null)
                        index = (int)_current;
                    while (notSorted)
                    {
                        notSorted = false;
                        for (int i = 0; i < Count - 1; i++)
                            if (comparison(this[i], this[i + 1]) > 0)
                            {
                                notSorted = true;
                                T t = this[i];
                                this[i] = this[i + 1];
                                this[i + 1] = t;
                                if (index == i)
                                    index = i + 1;
                                else if (index == i + 1)
                                    index = i;
                            }
                    }
                    if (index != -1 && index != _current)
                        Update(index);
                    else
                        Update();
                }
                public new void Sort(int index, int count, IComparer<T> comparer = null)
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    if (count < 0)
                        throw new ArgumentOutOfRangeException(nameof(count));
                    if (index + count > Count)
                        throw new ArgumentException($"Sum of \"{nameof(index)}\"={index} and \"{nameof(count)}\"={count} must not be more then \"{nameof(Count)}\"={Count}.");
                    if (comparer is null)
                    {
                        if (Comparer<T>.Default is IComparer<T> comparerD)
                            comparer = comparerD;
                        else
                            throw new InvalidOperationException("The default comparison function Default cannot find an implementation of the generic interface IComparable<T> or the IComparable interface for type T.");
                    }
                    if (comparer.Compare(this[index], this[index]) != 0)
                        throw new ArgumentException("\"comparer\" cannot return 0 when comparing an element to itself.");
                    bool notSorted = true;
                    int ind = -1;
                    if (_current != null)
                        ind = (int)_current;
                    while (notSorted)
                    {
                        notSorted = false;
                        for (int i = index; i < index + count - 1; i++)
                            if (comparer.Compare(this[i], this[i + 1]) > 0)
                            {
                                notSorted = true;
                                T t = this[i];
                                this[i] = this[i + 1];
                                this[i + 1] = t;
                                if (ind == i)
                                    ind = i + 1;
                                else if (ind == i + 1)
                                    ind = i;
                            }
                    }
                    if (ind != -1 && ind != _current)
                        Update(ind);
                    else
                        Update();
                }
                public new void Sort()
                {
                    IComparer<T> comparer = null;
                    if (comparer is null)
                    {
                        if (Comparer<T>.Default is IComparer<T> comparerD)
                            comparer = comparerD;
                        else
                            throw new InvalidOperationException("The default comparison function Default cannot find an implementation of the generic interface IComparable<T> or the IComparable interface for type T.");
                    }
                    if (Count > 0 && comparer.Compare(this[0], this[0]) != 0)
                        throw new ArgumentException("\"comparer\" cannot return 0 when comparing an element to itself.");
                    bool notSorted = true;
                    int ind = -1;
                    if (_current != null)
                        ind = (int)_current;
                    while (notSorted)
                    {
                        notSorted = false;
                        for (int i = 0; i < Count - 1; i++)
                            if (comparer.Compare(this[i], this[i + 1]) > 0)
                            {
                                notSorted = true;
                                T t = this[i];
                                this[i] = this[i + 1];
                                this[i + 1] = t;
                                if (ind == i)
                                    ind = i + 1;
                                else if (ind == i + 1)
                                    ind = i;
                            }
                    }
                    if (ind != -1 && ind != _current)
                        Update(ind);
                    else
                        Update();
                }
                public new void Sort(IComparer<T> comparer = null)
                {
                    if (comparer is null)
                    {
                        if (Comparer<T>.Default is IComparer<T> comparerD)
                            comparer = comparerD;
                        else
                            throw new InvalidOperationException("The default comparison function Default cannot find an implementation of the generic interface IComparable<T> or the IComparable interface for type T.");
                    }
                    if (Count > 0 && comparer.Compare(this[0], this[0]) != 0)
                        throw new ArgumentException("\"comparer\" cannot return 0 when comparing an element to itself.");
                    bool notSorted = true;
                    int ind = -1;
                    if (_current != null)
                        ind = (int)_current;
                    while (notSorted)
                    {
                        notSorted = false;
                        for (int i = 0; i < Count - 1; i++)
                            if (comparer.Compare(this[i], this[i + 1]) > 0)
                            {
                                notSorted = true;
                                T t = this[i];
                                this[i] = this[i + 1];
                                this[i + 1] = t;
                                if (ind == i)
                                    ind = i + 1;
                                else if (ind == i + 1)
                                    ind = i;
                            }
                    }
                    if (ind != -1 && ind != _current)
                        Update(ind);
                    else
                        Update();
                }
                int IList.Add(object item)
                {
                    if (item is T)
                    {
                        int index = Count;
                        ((IList)this).Add(item);
                        Update();
                        return index;
                    }
                    throw new ArgumentException($"{nameof(item)} is of a type that cannot be assigned to IList.");
                }
                void IList.Insert(int index, object item)
                {
                    if (index < 0 || index > Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    if (item is T)
                    {
                        ((IList)this).Insert(index, item);
                        Update(index, 1);
                        return;
                    }
                    throw new ArgumentException($"{nameof(item)} is of a type that cannot be assigned to IList.");
                }
                void IList.Remove(object item)
                {
                    if (item is T t)
                    {
                        int index = IndexOf(t);
                        if (index != -1)
                        {
                            base.RemoveAt(index);
                            Update(index, -1);
                        }
                        return;
                    }
                    throw new ArgumentException($"{nameof(item)} is of a type that cannot be assigned to IList.");
                }
            }
        }
    }
    namespace Automation
    {
        public interface IWorker
        {
            Status Status { get; }
            bool Enabled { get; set; }
            void Enable();
            void Disable();
            void Reset();
            void Error();
            void Complete();
            void Work();
            void ChangeStatus(Status status);
            event Worker.IWorkerEventHandler OnError;
            event Worker.IWorkerEventHandler OnStatusChanged;
        }
        public interface IManager : IWorker
        {
            RoutedList<IWorker> Workers { get; }
            void AddWorker(IWorker worker);
            void RemoveWorker(int index);
            void InsertWorker(int index, IWorker worker);
            void EnableAll();
            void DisableAll();
            void ResetAll();
            void WorkAll();
            void Error(IWorker worker);
            void StatusChanged(IWorker worker);
        }
        public abstract class Worker : IWorker
        {
            public delegate void IWorkerEventHandler(IWorker sender);
            protected Status _status;
            protected bool _enabled;
            public Status Status { get; }
            public bool Enabled
            {
                get
                {
                    return _enabled;
                }
            }
            bool IWorker.Enabled
            {
                get
                {
                    return _enabled;
                }
                set
                {
                    _enabled = value;
                }
            }
            public void Enable()
            {
                _enabled = true;
                _status = Status.Running;
            }
            public void Disable()
            {
                _enabled = false;
                _status = Status.Stopped;
            }
            public void Reset()
            {
                _enabled = false;
                _status = Status.Empty;
            }
            public void Error()
            {
                _status = Status.Error;
                OnError?.Invoke(this);
            }
            protected void Complete()
            {
                _enabled = false;
                _status = Status.Completed;
                OnStatusChanged?.Invoke(this);
            }
            void IWorker.Complete()
            {
                _enabled = false;
                _status = Status.Completed;
                OnStatusChanged?.Invoke(this);
            }
            public abstract void Work();
            protected void ChangeStatus(Status status)
            {
                _status = status;
                OnStatusChanged?.Invoke(this);
            }
            void IWorker.ChangeStatus(Status status)
            {
                _status = status;
                OnStatusChanged?.Invoke(this);
            }
            public Worker()
            {
                _enabled = false;
                _status = Status.Empty;
            }
            public event IWorkerEventHandler OnError;
            public event IWorkerEventHandler OnStatusChanged;
        }
        public class Manager : Worker, IManager, IWorker
        {
            protected readonly RoutedList<IWorker> _workers;
            public RoutedList<IWorker> Workers
            {
                get
                {
                    return new RoutedList<IWorker>(_workers);
                }
            }
            public override void Work()
            {
                if (Enabled && Status == Status.Running && _workers.Current != null)
                {
                    IWorker worker = (IWorker)_workers.Current;
                    if (worker.Status == Status.Completed)
                        _workers.MoveNext();
                    else
                        worker.Work();
                }
            }
            public void AddWorker(IWorker worker)
            {
                if (worker is null)
                    throw new ArgumentNullException(nameof(worker));
                worker.OnError += Error;
                worker.OnStatusChanged += StatusChanged;
                _workers.Add(worker);
            }
            public void RemoveWorker(int index)
            {
                if (index < 0 || index >= _workers.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                IWorker worker = _workers[index];
                worker.OnError -= Error;
                worker.OnStatusChanged -= StatusChanged;
                _workers.RemoveAt(index);
            }
            public void InsertWorker(int index, IWorker worker)
            {
                if (index < 0 || index >= _workers.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                if (worker is null)
                    throw new ArgumentNullException(nameof(worker));
                worker.OnError += Error;
                worker.OnStatusChanged += StatusChanged;
                _workers.Insert(index, worker);
            }
            public void EnableAll()
            {
                foreach (IWorker worker in _workers)
                    worker.Enable();
            }
            public void DisableAll()
            {
                foreach (IWorker worker in _workers)
                    worker.Disable();
            }
            public void ResetAll()
            {
                foreach (IWorker worker in _workers)
                    worker.Reset();
            }
            public void WorkAll()
            {
                foreach (IWorker worker in _workers)
                    worker.Work();
            }
            private void Error(IWorker worker)
            {
                if (worker is null)
                    throw new ArgumentNullException(nameof(worker));
                if (Enabled && Status == Status.Running)
                    Error();
            }
            void IManager.Error(IWorker worker)
            {
                if (worker is null)
                    throw new ArgumentNullException(nameof(worker));
                if (Enabled && Status == Status.Running)
                    Error();
            }
            private void StatusChanged(IWorker worker)
            {
                if (worker is null)
                    throw new ArgumentNullException(nameof(worker));
                if (worker.Status == Status.Completed)
                {
                    if (_workers.Current == worker)
                        _workers.MoveNext();
                    if (Enabled && Status == Status.Running && _workers.Current != null)
                    {
                        IWorker next = (IWorker)_workers.Current;
                        if (next.Status == Status.Empty)
                            next.Enable();
                    }
                }
            }
            void IManager.StatusChanged(IWorker worker)
            {
                if (worker is null)
                    throw new ArgumentNullException(nameof(worker));
                if (worker.Status == Status.Completed)
                {
                    if (_workers.Current == worker)
                        _workers.MoveNext();
                    if (Enabled && Status == Status.Running && _workers.Current != null)
                    {
                        IWorker next = (IWorker)_workers.Current;
                        if (next.Status == Status.Empty)
                            next.Enable();
                    }
                }
            }
            public Manager()
            {
                _enabled = false;
                _status = Status.Empty;
                _workers = new RoutedList<IWorker>();
            }
            public Manager(Route route, RouteMode routeMode)
            {
                _enabled = false;
                _status = Status.Empty;
                _workers = new RoutedList<IWorker>(route, routeMode);
            }
            public Manager(IEnumerable<IWorker> workers)
            {
                _enabled = false;
                _status = Status.Empty;
                _workers = new RoutedList<IWorker>(workers);
            }
            public Manager(IEnumerable<IWorker> workers, Route route, RouteMode routeMode)
            {
                _enabled = false;
                _status = Status.Empty;
                _workers = new RoutedList<IWorker>(workers, route, routeMode);
            }
        }
    }
}
