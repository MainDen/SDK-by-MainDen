// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using MainDen;
using MainDen.Collections;
using MainDen.Cyclical;

namespace MainDen
{
    public interface IConvertible
    {
        XmlElement ToXmlElement(XmlDocument xmlDocument, string name);
    }
    namespace Cyclical
    {
        public interface ICyclical
        {
            int GetHashCode(ref IDictionary<object, int> hashCodes);
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
            public static int GetHashCode(object source, ref IDictionary<object, int> hashCodes)
            {
                if (source is null)
                    throw new ArgumentNullException(nameof(source));
                if (hashCodes is null)
                    throw new ArgumentNullException(nameof(hashCodes));
                if (hashCodes.ContainsKey(source))
                    return hashCodes[source];
                else if (source is ICyclical cyclical)
                    return cyclical.GetHashCode(ref hashCodes);
                else if (source is Array array)
                {
                    int hash = 0;
                    hashCodes.Add(source, hash);
                    unchecked
                    {
                        for (int i = 0; i < array.GetLength(0); ++i)
                        {
                            hash *= 2;
                            hash += GetHashCode(array.GetValue(i), ref hashCodes);
                        }
                    }
                    return hash;
                }
                else
                    return source.GetHashCode();
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
                        else if (XmlElementConverters.ContainsKey(source_t))
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
                        if (source_object == null)
                            throw new MissingMethodException($"There is no suitable method for converting a XML node to an object of type \"{source_t}\".");
                        if (source.HasAttribute("id"))
                            id_source.Add(source.GetAttribute("id"), source_object);
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
        public class Obj : IObj, IGroupable, IDisposable, ICyclicalCloneable, ICloneable, ICyclicalConvertible, IConvertible, ICyclical
        {
            private Dictionary<string, object> _properties;
            private List<IGroup> _groups;
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
                XmlNodeList xmlPropertiesList = source.GetElementsByTagName("Properties");
                if (xmlPropertiesList.Count != 1)
                    throw new XmlException($"XML representation of object of type \"{typeof(Obj).FullName}\" must contain one node \"Properties\".");
                XmlElement xmlProperties = (XmlElement)xmlPropertiesList[0];
                foreach (XmlElement xmlProperty in xmlProperties.GetElementsByTagName("Property"))
                    if (xmlProperty.HasAttribute("key"))
                        if (obj.ContainsProperty(xmlProperty.GetAttribute("key")))
                            throw new XmlException($"Property with key=\"{xmlProperty.GetAttribute("key")}\" has already been created.");
                        else
                            obj.SetProperty(xmlProperty.GetAttribute("key"), CyclicalMethods.ToObject(xmlProperty, ref id_source));
                    else
                        throw new XmlException("XML node \"Property\" must contain the attribute \"key\".");
                return obj;
            }
            public int GetHashCode(ref IDictionary<object, int> hashCodes)
            {
                unchecked
                {
                    int hash = 3371;
                    foreach (string property in _properties.Keys)
                    {
                        hash *= 2;
                        hash += CyclicalMethods.GetHashCode(_properties[property], ref hashCodes);
                    }
                    return hash;
                }
            }
            public override int GetHashCode()
            {
                IDictionary<object, int> hashCodes = new Dictionary<object, int>();
                return GetHashCode(ref hashCodes);
            }
            public Obj()
            {
                _properties = new Dictionary<string, object>();
                _groups = new List<IGroup>();
            }
        }
        public class Group : IGroup, IGroupable, IDisposable, ICyclicalCloneable, ICloneable, ICyclicalConvertible, IConvertible, ICyclical
        {
            private List<object> _entries;
            private List<IGroup> _groups;
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
                XmlNodeList xmlEntriesList = source.GetElementsByTagName("Entries");
                if (xmlEntriesList.Count != 1)
                    throw new XmlException($"XML representation of object of type \"{typeof(Group).FullName}\" must contain one node \"Entries\".");
                XmlElement xmlEntries = (XmlElement)xmlEntriesList[0];
                foreach (XmlElement xmlEntry in xmlEntries.GetElementsByTagName("Entry"))
                    group.Include(CyclicalMethods.ToObject(xmlEntry, ref id_source));
                return group;
            }
            public int GetHashCode(ref IDictionary<object, int> hashCodes)
            {
                unchecked
                {
                    int hash = 1733;
                    foreach (object entry in Entries)
                    {
                        hash *= 2;
                        hash += CyclicalMethods.GetHashCode(entry, ref hashCodes);
                    }
                    return hash;
                }
            }
            public override int GetHashCode()
            {
                IDictionary<object, int> hashCodes = new Dictionary<object, int>();
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
        }
    }
    namespace Automation
    {
        public class Manager : IDisposable
        {
            public delegate void ManagerErrorEventHandler(Manager manager, Worker worker);
            internal List<Worker> _workers;
            public List<Worker> Workers
            {
                get
                {
                    List<Worker> workers = new List<Worker>();
                    foreach (Worker worker in _workers)
                        workers.Add(worker);
                    return workers;
                }
            }
            public void AddWorker(Worker worker)
            {
                if (worker is null)
                    throw new ArgumentNullException(nameof(worker));
                _workers.Add(worker);
                worker._managers.Add(this);
                worker.OnError += Error;
            }
            public void DisableAll()
            {
                foreach (Worker worker in Workers)
                    worker.Disable();
            }
            public void EnableAll()
            {
                foreach (Worker worker in Workers)
                    worker.Enable();
            }
            public void ResetAll()
            {
                foreach (Worker worker in Workers)
                    worker.Reset();
            }
            public void WorkAll()
            {
                foreach (Worker worker in Workers)
                    worker.Work();
            }
            public void Dispose()
            {
                foreach (Worker worker in Workers)
                {
                    worker._managers.Remove(this);
                    worker.OnError -= Error;
                }
                _workers.Clear();
            }
            public void Error(Worker worker)
            {
                OnError?.Invoke(this, worker);
            }
            public Manager()
            {
                _workers = new List<Worker>();
            }
            public Manager(IList<Worker> workers)
            {
                foreach (Worker worker in workers)
                    _workers.Add(worker);
            }
            public event ManagerErrorEventHandler OnError;
        }
        public abstract class Worker : IDisposable
        {
            public delegate void WorkerErrorEventHandler(Worker sender);
            internal List<Manager> _managers;
            public List<Manager> Managers
            {
                get
                {
                    List<Manager> managers = new List<Manager>();
                    foreach (Manager manager in _managers)
                        managers.Add(manager);
                    return managers;
                }
            }
            public bool Enabled { get; protected set; }
            public virtual void Disable()
            {
                Enabled = false;
            }
            public virtual void Enable()
            {
                Enabled = true;
            }
            public abstract void Reset();
            public abstract void Work();
            public virtual void Dispose()
            {
                foreach (Manager manager in Managers)
                {
                    _managers.Remove(manager);
                    manager._workers.Remove(this);
                    if (!_managers.Contains(manager))
                        OnError -= manager.Error;
                }
            }
            public virtual void Error()
            {
                OnError?.Invoke(this);
            }
            public Worker()
            {
                _managers = new List<Manager>();
                Enabled = false;
            }
            public event WorkerErrorEventHandler OnError;
        }
    }
}
