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

namespace MainDen
{
    public interface ICyclical
    {
    }
    public interface ICyclicalCloneable : ICyclical, ICloneable
    {
        object CyclicalClone(ref IDictionary Changes);
    }
    public static class CyclicalActions
    {
        public static object Clone(object source, ref IDictionary contract)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            else if (contract == null)
                throw new ArgumentNullException("contract");
            else if (contract.Contains(source))
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
        public class Obj : IObj, IGroupable, IDisposable, ICyclicalCloneable
        {
            public IDictionary<string, object> Properties { get; }
            public IList<IGroup> Groups { get; }
            public bool TryGet(string property, out object value)
            {
                if (property == null)
                    throw new ArgumentNullException("property");
                return Properties.TryGetValue(property, out value);
            }
            public void Set(string property, object value)
            {
                if (property == null)
                    throw new ArgumentNullException("property");
                if (value == null)
                    throw new ArgumentNullException("value");
                Properties.Remove(property);
                Properties.Add(property, value);
            }
            public bool Remove(string property)
            {
                if (property == null)
                    throw new ArgumentNullException("property");
                return Properties.Remove(property);
            }
            public bool Has(string property)
            {
                if (property == null)
                    throw new ArgumentNullException("property");
                return Properties.ContainsKey(property);
            }
            public bool Empty(string property)
            {
                if (property == null)
                    throw new ArgumentNullException("property");
                return !Properties.ContainsKey(property);
            }
            public bool IncludedIn(IGroup group)
            {
                if (group == null)
                    throw new ArgumentNullException("group");
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
                if (contract == null)
                    throw new ArgumentNullException("contract");
                IObj clone = new Obj();
                if (!contract.Contains(this))
                    contract.Add(this, clone);
                foreach (string property in Properties.Keys)
                    clone.Set(property, CyclicalActions.Clone(this[property], ref contract));
                return clone;
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
                    if (property == null)
                        throw new ArgumentNullException("property");
                    if (!Properties.ContainsKey(property))
                        throw new ArgumentOutOfRangeException("property");
                    return Properties[property];
                }
                set
                {
                    if (property == null)
                        throw new ArgumentNullException("property");
                    if (value == null)
                        throw new ArgumentNullException("value");
                    if (!Properties.ContainsKey(property))
                        throw new ArgumentOutOfRangeException("property");
                    Properties[property] = value;
                }
            }
            public Obj()
            {
                Properties = new Dictionary<string, object>();
                Groups = new List<IGroup>();
            }
        }
        public class Group : IGroup, IGroupable, IDisposable, ICyclicalCloneable
        {
            public IList<object> Entries { get; }
            public IList<object> Members { get => GetMembers(); }
            public IList<IGroup> Subgroups { get => GetSubgroups(); }
            public IList<IGroup> Groups { get; }
            public void Include(object instance)
            {
                if (instance == null)
                    throw new ArgumentNullException("instance");
                if (!Entries.Contains(instance))
                    Entries.Add(instance);
                if (instance is IGroupable groupable)
                    if (!groupable.Groups.Contains(this))
                        groupable.Groups.Add(this);
            }
            public void Exclude(object instance)
            {
                if (instance == null)
                    throw new ArgumentNullException("instance");
                Entries.Remove(instance);
                if (instance is IGroupable groupable)
                    groupable.Groups.Remove(this);
            }
            public bool Contains(object instance)
            {
                if (instance == null)
                    throw new ArgumentNullException("instance");
                return Entries.Contains(instance);
            }
            public bool IncludedIn(IGroup group)
            {
                if (group == null)
                    throw new ArgumentNullException("group");
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
                if (contract == null)
                    throw new ArgumentNullException("contract");
                Group clone = new Group();
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
            public Group()
            {
                Entries = new List<object>();
                Groups = new List<IGroup>();
            }
            public Group(IGroup group) : this()
            {
                if (group == null)
                    throw new ArgumentNullException("group");
                foreach (object entry in group.Entries)
                    Include(entry);
            }
        }
    }
}
