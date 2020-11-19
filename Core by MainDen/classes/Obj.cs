// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using MainDen.Collections.Generic;
using MainDen.Cyclical;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MainDen.Collections
{
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
            return ToObject(source, ref id_source);
        }
        public static Obj ToObject(XmlElement source, ref IDictionary<string, object> id_source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (id_source is null)
                throw new ArgumentNullException(nameof(id_source));
            if (source.HasAttribute("id"))
                if (id_source.ContainsKey(source.GetAttribute("id")))
                    throw new XmlException($"Object with id=\"{source.GetAttribute("id")}\" has already been created.");
            if (source.HasAttribute("ref"))
                if (id_source.ContainsKey(source.GetAttribute("ref")))
                    if (id_source[source.GetAttribute("ref")] is Obj _obj)
                        return _obj;
                    else
                        throw new XmlException($"Object with id=\"{source.GetAttribute("id")}\" must be \"{typeof(Obj).FullName}\".");
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
        public static void Initialize()
        {
            if (!CyclicalMethods.XmlElementConverters.ContainsKey(typeof(Obj).FullName))
                CyclicalMethods.XmlElementConverters.Add(typeof(Obj).FullName, ToObject);
            if (!CyclicalMethods.XmlElementIdConverters.ContainsKey(typeof(Obj).FullName))
                CyclicalMethods.XmlElementIdConverters.Add(typeof(Obj).FullName, ToObject);
        }
        public Obj()
        {
            _properties = new Dictionary<string, object>();
            _groups = new List<IGroup>();
        }
    }
}