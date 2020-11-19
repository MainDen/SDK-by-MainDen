// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using MainDen.Cyclical;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MainDen.Collections.Generic
{
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
            IDictionary<string, object> id_source = new Dictionary<string, object>();
            return ToObject(source, ref id_source);
        }
        public static Group<T> ToObject(XmlElement source, ref IDictionary<string, object> id_source)
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
                    if (id_source[source.GetAttribute("ref")] is Group<T> _group)
                        return _group;
                    else
                        throw new XmlException($"Object with id=\"{source.GetAttribute("id")}\" must be \"{typeof(Group<T>).FullName}\".");
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
        public static void Initialize()
        {
            if (!CyclicalMethods.XmlElementConverters.ContainsKey(typeof(Group<T>).FullName))
                CyclicalMethods.XmlElementConverters.Add(typeof(Group<T>).FullName, ToObject);
            if (!CyclicalMethods.XmlElementIdConverters.ContainsKey(typeof(Group<T>).FullName))
                CyclicalMethods.XmlElementIdConverters.Add(typeof(Group<T>).FullName, ToObject);
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
}