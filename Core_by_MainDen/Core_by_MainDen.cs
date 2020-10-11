// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace MainDen
{
    public interface ICyclical { }
    public interface ICyclicalCloneable : ICyclical, ICloneable
    {
        object CyclicalClone(ref IDictionary Changes);
    }
    public interface ICyclicalConvertible
    {
        XmlElement ToXmlElement(XmlDocument xmlDocument, ref IList<object> id_soure, string name);
        void FromXmlElement(XmlElement xmlElement, ref IDictionary<string, object> id_source);
    }
    public static class CyclicalActions
    {
        public static object Clone(object source, ref IDictionary contract)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (contract is null)
                throw new ArgumentNullException(nameof(contract));
            if (contract.Contains(source))
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
                    xml_element.AppendChild(ToXmlElement(obj, xmlDocument, ref id_source, name));
                return xml_element;
            }
            else
            {
                XmlElement xml_element = xmlDocument.CreateElement(name);
                xml_element.SetAttribute("id", id_source.Count.ToString("x16"));
                id_source.Add(source);
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
                    throw new ArgumentOutOfRangeException(nameof(source), $"The object reference must be after the object instance.\nref=\"{source.GetAttribute("ref")}\"");
            }
            else if (source.HasAttribute("type"))
            {
                string str_source_t = source.GetAttribute("type");
                object source_o = null;
                Type source_t = Type.GetType(str_source_t);
                List<Type> interfaces = new List<Type>(source_t.GetInterfaces());
                if (interfaces.Contains(typeof(ICyclicalConvertible)))
                {
                    ICyclicalConvertible cyclicalConvertible = source_t.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null).Invoke(null) as ICyclicalConvertible;
                    if (cyclicalConvertible != null)
                        cyclicalConvertible.FromXmlElement(source, ref id_source);
                    return cyclicalConvertible;
                }
                else if (source_t.IsArray)
                {
                    object[] source_a = new object[source.ChildNodes.Count];
                    if (source.HasAttribute("id"))
                        id_source.Add(source.GetAttribute("id"), source_a);
                    int i = 0;
                    foreach (XmlElement xmlElement in source)
                        source_a[i++] = ToObject(xmlElement, ref id_source);
                    return source_a;
                }
                #region Simple types
                else if (source_t == typeof(Boolean))
                    source_o = Convert.ToBoolean(source.InnerText);
                else if (source_t == typeof(Byte))
                    source_o = Convert.ToByte(source.InnerText);
                else if (source_t == typeof(Char))
                    source_o = Convert.ToChar(source.InnerText);
                else if (source_t == typeof(DateTime))
                    source_o = Convert.ToDateTime(source.InnerText);
                else if (source_t == typeof(Decimal))
                    source_o = Convert.ToDecimal(source.InnerText);
                else if (source_t == typeof(Double))
                    source_o = Convert.ToDouble(source.InnerText);
                else if (source_t == typeof(Single))
                    source_o = Convert.ToSingle(source.InnerText);
                else if (source_t == typeof(Int32))
                    source_o = Convert.ToInt32(source.InnerText);
                else if (source_t == typeof(Int64))
                    source_o = Convert.ToInt64(source.InnerText);
                else if (source_t == typeof(SByte))
                    source_o = Convert.ToSByte(source.InnerText);
                else if (source_t == typeof(Int16))
                    source_o = Convert.ToInt16(source.InnerText);
                else if (source_t == typeof(String))
                    source_o = Convert.ToString(source.InnerText);
                else if (source_t == typeof(UInt32))
                    source_o = Convert.ToUInt32(source.InnerText);
                else if (source_t == typeof(UInt64))
                    source_o = Convert.ToUInt64(source.InnerText);
                else if (source_t == typeof(UInt16))
                    source_o = Convert.ToUInt16(source.InnerText);
                #endregion
                if (source_o == null)
                    throw new ArgumentOutOfRangeException(nameof(source), "There is no method to handle node named \"" + source.Name + "\".");
                if (source.HasAttribute("id"))
                    id_source.Add(source.GetAttribute("id"), source_o);
                return source_o;
            }
            else if (source.ChildNodes.Count == 1 && source.FirstChild is XmlText xmlText)
            {
                return xmlText.Value;
            }
            else if (source.ChildNodes.Count > 0)
            {
                object[] source_a = new object[source.ChildNodes.Count];
                if (source.HasAttribute("id"))
                    id_source.Add(source.GetAttribute("id"), source_a);
                int i = 0;
                foreach (XmlElement xmlElement in source)
                    source_a[i++] = ToObject(xmlElement, ref id_source);
                return source_a;
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
    namespace Collections
    {
        public interface IObj
        {
            IDictionary<string, object> Properties { get; }
            bool TryGet(string property, out object value);
            void Set(string property, object value);
            bool Remove(string property);
            bool Has(string property);
            bool Empty(string property);
            object this[string property] { get; set; }
        }
        public interface IGroupable
        {
            IList<IGroup> Groups { get; }
            bool IncludedIn(IGroup group);
        }
        public interface IGroup
        {
            IList<object> Entries { get; }
            void Include(object instance);
            void Exclude(object instance);
            bool Contains(object instance);
        }
        public class Obj : IObj, IGroupable, IDisposable, ICyclicalCloneable, ICyclicalConvertible
        {
            public IDictionary<string, object> Properties { get; }
            public IList<IGroup> Groups { get; }
            public bool TryGet(string property, out object value)
            {
                if (property is null)
                    throw new ArgumentNullException(nameof(property));
                return Properties.TryGetValue(property, out value);
            }
            public void Set(string property, object value)
            {
                if (property is null)
                    throw new ArgumentNullException(nameof(property));
                if (value is null)
                    throw new ArgumentNullException(nameof(value));
                Properties.Remove(property);
                Properties.Add(property, value);
            }
            public bool Remove(string property)
            {
                if (property is null)
                    throw new ArgumentNullException(nameof(property));
                return Properties.Remove(property);
            }
            public bool Has(string property)
            {
                if (property is null)
                    throw new ArgumentNullException(nameof(property));
                return Properties.ContainsKey(property);
            }
            public bool Empty(string property)
            {
                if (property is null)
                    throw new ArgumentNullException(nameof(property));
                return !Properties.ContainsKey(property);
            }
            public bool IncludedIn(IGroup group)
            {
                if (group is null)
                    throw new ArgumentNullException(nameof(group));
                return Groups.Contains(group);
            }
            public void Dispose()
            {
                while (Groups.Count != 0)
                    Groups[0].Exclude(this);
            }
            public object Clone()
            {
                IDictionary contract = new Dictionary<object, object>();
                return CyclicalClone(ref contract);
            }
            public object CyclicalClone(ref IDictionary contract)
            {
                if (contract is null)
                    throw new ArgumentNullException(nameof(contract));
                IObj clone = new Obj();
                if (!contract.Contains(this))
                    contract.Add(this, clone);
                foreach (string property in Properties.Keys)
                    clone.Set(property, CyclicalActions.Clone(this[property], ref contract));
                return clone;
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
                xmlThis.SetAttribute("id", id_source.Count.ToString("x16"));
                id_source.Add(this);
                xmlThis.SetAttribute("type", GetType().FullName);
                XmlElement xmlProperties = xmlDocument.CreateElement("Properties");
                xmlThis.AppendChild(xmlProperties);
                foreach (string property in Properties.Keys)
                {
                    XmlElement xmlProperty = CyclicalActions.ToXmlElement(this[property], xmlDocument, ref id_source, "Property");
                    xmlProperty.SetAttribute("key", property);
                    xmlProperties.AppendChild(xmlProperty);
                }
                return xmlThis;
            }
            public void FromXmlElement(XmlElement source, ref IDictionary<string, object> id_source)
            {
                if (source is null)
                    throw new ArgumentNullException(nameof(source));
                if (id_source is null)
                    throw new ArgumentNullException(nameof(id_source));
                if (source.HasAttribute("id"))
                    if (id_source.ContainsKey(source.GetAttribute("id")))
                        id_source[source.GetAttribute("id")] = this;
                    else
                        id_source.Add(source.GetAttribute("id"), this);
                XmlNodeList xmlPropertiesList = source.GetElementsByTagName("Properties");
                if (xmlPropertiesList.Count != 1)
                    throw new XmlException($"XML representation of object of type \"{GetType().FullName}\" must contain one node \"Properties\".");
                XmlElement xmlProperties = (XmlElement)xmlPropertiesList[0];
                foreach (XmlElement xmlProperty in xmlProperties.GetElementsByTagName("Property"))
                    if (xmlProperty.Attributes.GetNamedItem("key") != null)
                        Set(xmlProperty.Attributes.GetNamedItem("key").Value, CyclicalActions.ToObject(xmlProperty, ref id_source));
                    else
                        throw new XmlException("XML node \"Property\" must contain the attribute \"key\".");
            }
            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 3371;
                    foreach (string property in Properties.Keys)
                    {
                        hash *= 2;
                        if (Properties[property] is ICyclical)
                            hash += 1;
                        else
                            hash += Properties[property].GetHashCode();
                    }
                    return hash;
                }
            }
            public object this[string property]
            {
                get
                {
                    if (property is null)
                        throw new ArgumentNullException(nameof(property));
                    if (!Properties.ContainsKey(property))
                        throw new ArgumentOutOfRangeException(nameof(property));
                    return Properties[property];
                }
                set
                {
                    if (property is null)
                        throw new ArgumentNullException(nameof(property));
                    if (value is null)
                        throw new ArgumentNullException(nameof(value));
                    if (!Properties.ContainsKey(property))
                        throw new ArgumentOutOfRangeException(nameof(property));
                    Properties[property] = value;
                }
            }
            public Obj()
            {
                Properties = new Dictionary<string, object>();
                Groups = new List<IGroup>();
            }
        }
        public class Group : IGroup, IGroupable, IDisposable, ICyclicalCloneable, ICyclicalConvertible
        {
            public IList<object> Entries { get; }
            public IList<object> Members { get => GetMembers(); }
            public IList<IGroup> Subgroups { get => GetSubgroups(); }
            public IList<IGroup> Groups { get; }
            public void Include(object instance)
            {
                if (instance is null)
                    throw new ArgumentNullException(nameof(instance));
                if (!Entries.Contains(instance))
                    Entries.Add(instance);
                if (instance is IGroupable groupable)
                    if (!groupable.Groups.Contains(this))
                        groupable.Groups.Add(this);
            }
            public void Exclude(object instance)
            {
                if (instance is null)
                    throw new ArgumentNullException(nameof(instance));
                Entries.Remove(instance);
                if (instance is IGroupable groupable)
                    groupable.Groups.Remove(this);
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
            public void Dispose()
            {
                while (Entries.Count != 0)
                    Exclude(Entries[0]);
                while (Groups.Count != 0)
                    Groups[0].Exclude(this);
            }
            public object Clone()
            {
                IDictionary contract = new Dictionary<object, object>();
                return CyclicalClone(ref contract);
            }
            public object CyclicalClone(ref IDictionary contract)
            {
                if (contract is null)
                    throw new ArgumentNullException(nameof(contract));
                IGroup clone = new Group();
                if (!contract.Contains(this))
                    contract.Add(this, clone);
                foreach (object entry in Entries)
                    clone.Include(CyclicalActions.Clone(entry, ref contract));
                return clone;
            }
            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 1733;
                    foreach (object entry in Entries)
                    {
                        hash *= 2;
                        if (entry is ICyclical)
                            hash += 1;
                        else
                            hash += entry.GetHashCode();
                    }
                    return hash;
                }
            }
            private IList<object> GetMembers()
            {
                IList<object> members = new List<object>();
                foreach (object entry in Entries)
                    if (!(entry is IGroup))
                        members.Add(entry);
                return members;
            }
            private IList<IGroup> GetSubgroups()
            {
                IList<IGroup> subgroups = new List<IGroup>();
                foreach (object entry in Entries)
                    if (entry is IGroup subgroup)
                        subgroups.Add(subgroup);
                return subgroups;
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
                xmlThis.SetAttribute("id", id_source.Count.ToString("x16"));
                id_source.Add(this);
                xmlThis.SetAttribute("type", GetType().FullName);
                XmlElement xmlEntries = xmlDocument.CreateElement("Entries");
                xmlThis.AppendChild(xmlEntries);
                foreach (object entry in Entries)
                {
                    XmlElement xmlEntry = CyclicalActions.ToXmlElement(entry, xmlDocument, ref id_source, "Entry");
                    xmlEntries.AppendChild(xmlEntry);
                }
                return xmlThis;
            }
            public void FromXmlElement(XmlElement source, ref IDictionary<string, object> id_source)
            {
                if (source is null)
                    throw new ArgumentNullException(nameof(source));
                if (id_source is null)
                    throw new ArgumentNullException(nameof(id_source));
                if (source.HasAttribute("id"))
                    if (id_source.ContainsKey(source.GetAttribute("id")))
                        id_source[source.GetAttribute("id")] = this;
                    else
                        id_source.Add(source.GetAttribute("id"), this);
                XmlNodeList xmlEntriesList = source.GetElementsByTagName("Entries");
                if (xmlEntriesList.Count != 1)
                    throw new XmlException($"XML representation of object of type \"{GetType().FullName}\" must contain one node \"Entries\".");
                XmlElement xmlEntries = (XmlElement)xmlEntriesList[0];
                foreach (XmlElement xmlEntry in xmlEntries.GetElementsByTagName("Entry"))
                    Include(CyclicalActions.ToObject(xmlEntry, ref id_source));
            }
            public Group()
            {
                Entries = new List<object>();
                Groups = new List<IGroup>();
            }
            public Group(IGroup group) : this()
            {
                if (group is null)
                    throw new ArgumentNullException(nameof(group));
                foreach (object entry in group.Entries)
                    Include(entry);
            }
        }
    }
}
