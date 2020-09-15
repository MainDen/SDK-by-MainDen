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

/// <summary>
/// Main namespace from SDK by MainDen. Contains basic tools.
/// </summary>
namespace MainDen
{
    /// <summary>
    /// Provides a methods for maybe cycled objects.
    /// </summary>
    public interface ICyclical
    {
        /// <summary>
        /// Provides a check on cyclical the object.
        /// </summary>
        /// <returns>True if the given object is cyclical.</returns>
        bool IsCycled();
    }
    /// <summary>
    /// Provides a cloning methods for maybe cycled objects.
    /// </summary>
    public interface ICyclicalCloneable : ICyclical, ICloneable
    {
        /// <summary>
        /// Provides cloning with duplicate objects.
        /// </summary>
        /// <param name="Changes">Associates subobjects with their clones.</param>
        /// <returns>Clone of the object.</returns>
        object CycledClone(ref IDictionary Changes);
    }
    public static class Cloner
    {
        public static object Clone(object source, ref IDictionary contract)
        {
            if (contract == null)
                throw new ArgumentNullException("contract");
            else if (source == null)
                throw new ArgumentNullException("source");
            else if (contract.Contains(source))
                return contract[source];
            else if (source is ICyclicalCloneable cyclicalCloneable)
                cyclicalCloneable.CycledClone(ref contract);
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
    /// <summary>
    /// Expands the capabilities of collections.
    /// </summary>
    namespace Collections
    {
        /// <summary>
        /// Provides methods for objects containing properties.
        /// </summary>
        public interface IObj
        {
            /// <summary>
            /// Properties of the object.
            /// </summary>
            IDictionary<string, object> Properties { get; }
            /// <summary>
            /// Gets the value of the property of the object, if the property exists.
            /// </summary>
            /// <param name="property">Property of the object.</param>
            /// <param name="value">Value of the property.</param>
            /// <returns>True if the given property exist.</returns>
            bool TryGet(string property, out object value);
            /// <summary>
            /// Sets the value for the property of the object.
            /// </summary>
            /// <param name="property">Property of the object.</param>
            /// <param name="value">Value of the property.</param>
            void Set(string property, object value);
            /// <summary>
            /// Removes the property of the object.
            /// </summary>
            /// <param name="property">Property of the object.</param>
            /// <returns>True if the given property was existed.</returns>
            bool Remove(string property);
            /// <summary>
            /// Checks the existence of the property for the object.
            /// </summary>
            /// <param name="property">Property of the object.</param>
            /// <returns>True if the given property exist.</returns>
            bool Has(string property);
            /// <summary>
            /// Determines if the property is missing or does not exist for the object.
            /// </summary>
            /// <param name="property">Property of the object.</param>
            /// <returns>True if the property is missing or does not exist for the object.</returns>
            bool Empty(string property);
            object this[string property] { get; set; }
        }
        /// <summary>
        /// Provides the object with knowledge about the groups in which it is contained.
        /// </summary>
        public interface IGroupable
        {
            /// <summary>
            /// Groups in which object is contained.
            /// </summary>
            ISet<IGroup> Groups { get; }
            /// <summary>
            /// Checks if the object belongs to the group.
            /// </summary>
            /// <param name="group">Checked group.</param>
            /// <returns>True if the object belongs to the group.</returns>
            bool Included(IGroup group);
        }
        /// <summary>
        /// Provides methods for groups.
        /// </summary>
        public interface IGroup
        {
            /// <summary>
            /// Entries to the group.
            /// </summary>
            ISet<object> Entries { get; }
            /// <summary>
            /// Includes the instance to the group.
            /// </summary>
            /// <param name="instance">Inclusive instance.</param>
            void Include(object instance);
            /// <summary>
            /// Excludes the instance from the group.
            /// </summary>
            /// <param name="instance">Exclusive instance.</param>
            void Exclude(object instance);
            /// <summary>
            /// Checks if the group contains the instance.
            /// </summary>
            /// <param name="instance">Contained instance.</param>
            /// <returns></returns>
            bool Contains(object instance);
        }
        /// <summary>
        /// General-purpose class for objects with properties.
        /// </summary>
        public class Obj : IObj, IGroupable, IDisposable, ICyclicalCloneable
        {
            /// <summary>
            /// Binds properties by name.
            /// </summary>
            public IDictionary<string, object> Properties { get; }
            /// <summary>
            /// Contains pointers to the groups to which the object belongs.
            /// </summary>
            public ISet<IGroup> Groups { get; }
            /// <summary>
            /// Gets the value of the property of the object, if the property exists.
            /// </summary>
            /// <param name="property">Property of the object.</param>
            /// <param name="value">Value of the property.</param>
            /// <returns>True if the given property exist.</returns>
            public bool TryGet(string property, out object value)
            {
                return Properties.TryGetValue(property, out value);
            }
            /// <summary>
            /// Sets the property by name.
            /// </summary>
            /// <param name="property">Property name.</param>
            /// <param name="value">Property value.</param>
            public void Set(string property, object value)
            {
                Properties.Remove(property);
                Properties.Add(property, value);
            }
            /// <summary>
            /// Removes the property by name.
            /// </summary>
            /// <param name="property">Property name.</param>
            /// <returns>True if existed.</returns>
            public bool Remove(string property)
            {
                return Properties.Remove(property);
            }
            /// <summary>
            /// Checks the property by name.
            /// </summary>
            /// <param name="property">Property name.</param>
            /// <returns>True if exists.</returns>
            public bool Has(string property)
            {
                return Properties.ContainsKey(property);
            }
            /// <summary>
            /// Checks the property by name.
            /// </summary>
            /// <param name="property">Property name.</param>
            /// <returns>False if not null.</returns>
            public bool Empty(string property)
            {
                return !Properties.ContainsKey(property);
            }
            /// <summary>
            /// Checks if the object belongs to the group.
            /// </summary>
            /// <param name="group">Checked group.</param>
            /// <returns>True if the object belongs to the group.</returns>
            public bool Included(IGroup group)
            {
                return Groups.Contains(group);
            }
            /// <summary>
            /// Excludes the object from its associated groups.
            /// </summary>
            public void Dispose()
            {
                foreach (IGroup group in Groups)
                    group.Exclude(this);
            }
            /// <summary>
            /// Checks if the object belongs to the group.
            /// </summary>
            /// <param name="group">Checked group.</param>
            /// <returns>True if the object belongs to the group.</returns>
            public bool IsCycled()
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// Creates an independent object with identical parameters.
            /// </summary>
            /// <returns>Clone of object.</returns>
            public object Clone()
            {
                IDictionary contract = new Dictionary<object, object>();
                return CycledClone(ref contract);
            }
            /// <summary>
            /// Provides cloning with duplicate objects.
            /// </summary>
            /// <param name="contract">Associates subobjects with their clones.</param>
            /// <returns>Clone of the object.</returns>
            public object CycledClone(ref IDictionary contract)
            {
                IObj clone = new Obj();
                contract.Add(this, clone);
                foreach (string property in Properties.Keys)
                    clone.Set(property, Cloner.Clone(this[property], ref contract));
                return clone;
            }
            /// <summary>
            /// Gets the hash code by properties.
            /// </summary>
            /// <returns>Hash code.</returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 3371;
                    foreach (string property in Properties.Keys)
                        hash = (hash * 3) + Properties[property].ToString().GetHashCode();
                    return hash;
                }
            }
            /// <summary>
            /// Compares the current object with the given object.
            /// </summary>
            /// <param name="obj">Given object.</param>
            /// <returns>True if all properties are the same.</returns>
            public override bool Equals(object obj)
            {
                Obj _obj = obj as Obj;
                if (_obj == null)
                    return false;
                if (_obj.Properties.Count == 0 && Properties.Count == 0)
                    return true;
                if (_obj.Properties.Count != Properties.Count)
                    return false;
                foreach (string property in Properties.Keys)
                    if (!_obj.Has(property))
                        return false;
                if (GetHashCode() != _obj.GetHashCode())
                    return false;
                foreach (string property in Properties.Keys)
                    if (!(_obj[property] == this[property] || _obj[property].Equals(this[property])))
                        return false;
                return true;
            }
            /// <summary>
            /// Makes the sting from the object.
            /// </summary>
            /// <returns>The string representation of the object.</returns>
            public override string ToString()
            {
                return base.ToString();
            }
            /// <summary>
            /// Checks all properties by name.
            /// </summary>
            /// <returns>True if all properties are not empty.</returns>
            public bool Exist()
            {
                foreach (string property in Properties.Keys)
                    if (Properties[property] == null)
                        return false;
                return true;
            }
            /// <summary>
            /// Identifies the property by name.
            /// </summary>
            /// <param name="property">Property name.</param>
            /// <returns>Property instance.</returns>
            public object this[string property]
            {
                get => Properties[property]; set => Properties[property] = value;
            }
            /// <summary>
            /// Initializes a new instance of the class.
            /// </summary>
            public Obj()
            {
                Properties = new Dictionary<string, object>();
                Groups = new HashSet<IGroup>();
            }
        }
        /// <summary>
        /// General-purpose class for groups with properties.
        /// </summary>
        public class Group : Obj, IGroup, IGroupable, IDisposable, ICyclicalCloneable
        {
            public ISet<object> Entries { get; }
            public ISet<object> Members { get => GetMembers(); }
            public ISet<IGroup> Subgroups { get => GetSubgroups(); }
            public void Include(object instance)
            {
                Entries.Add(instance);
                if (instance is IGroupable groupable)
                    groupable.Groups.Add(this);
            }
            public void Exclude(object instance)
            {
                Entries.Remove(instance);
                if (instance is IGroupable groupable)
                    groupable.Groups.Remove(this);
            }
            public bool Contains(object instance)
            {
                return Entries.Contains(instance);
            }
            public new void Dispose()
            {
                while (Entries.Count != 0)
                    Exclude(Entries.GetEnumerator().Current);
                while (Groups.Count != 0)
                    Groups.GetEnumerator().Current.Exclude(this);
            }
            public new bool IsCycled()
            {
                throw new NotImplementedException();
            }
            public new object Clone()
            {
                IDictionary contract = new Dictionary<object, object>();
                return CycledClone(ref contract);
            }
            public new object CycledClone(ref IDictionary contract)
            {
                Group clone = new Group();
                contract.Add(this, clone);
                foreach (string property in Properties.Keys)
                    clone.Set(property, Cloner.Clone(this[property], ref contract));
                foreach (object entry in Entries)
                    clone.Include(Cloner.Clone(entry, ref contract));
                return clone;
            }
            private ISet<object> GetMembers()
            {
                ISet<object> members = new HashSet<object>();
                foreach (object entry in Entries)
                    if (!(entry is IGroup))
                        members.Add(entry);
                return members;
            }
            private ISet<IGroup> GetSubgroups()
            {
                ISet<IGroup> subgroups = new HashSet<IGroup>();
                foreach (object entry in Entries)
                    if (entry is IGroup subgroup)
                        subgroups.Add(subgroup);
                return subgroups;
            }
            public Group() : base()
            {
                Entries = new HashSet<object>();
            }
            public Group(IGroup group) : this()
            {
                foreach (object entry in group.Entries)
                    Include(entry);
            }
        }
    }
}