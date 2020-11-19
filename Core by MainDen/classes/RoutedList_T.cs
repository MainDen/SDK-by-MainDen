// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using MainDen.Enums;
using MainDen.Cyclical;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace MainDen.Collections.Generic
{
    /// <summary>
    /// Represents a list of items extended with the methods of a custom directed enumerator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RoutedList<T> : List<T>, ICollection<T>, IConvertible, ICyclical, ICyclicalConvertible, IEnumerable<T>, IList, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, IRouted
    {
        private bool _increase;

        private List<T> _remainder;

        /// <summary>
        /// Defines the updated value for the "<c>_increase</c>" for the current state.
        /// </summary>
        private bool Increase
        {
            get
            {
                switch (RouteType)
                {
                    case RouteType.Empty:
                    case RouteType.PingPong:
                    case RouteType.Unit:
                        return _increase;
                    default:
                        return !RouteMode.HasFlag(RouteMode.Reverse);
                }
            }
        }

        /// <summary>
        /// Defines the default value for the "<c>CurrentIndex</c>" for the current state.
        /// </summary>
        private int? CInd
        {
            get
            {
                if (RouteType == RouteType.Empty || Count == 0)
                    return null;
                if (RouteMode.HasFlag(RouteMode.Reverse))
                    return Count - 1;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Defines the default value for the "<c>BeginIndex</c>" for the current state.
        /// </summary>
        private int? BInd
        {
            get
            {
                if (Count == 0 || RouteType == RouteType.Empty || RouteMode.HasFlag(RouteMode.Loop))
                    return null;
                if (RouteType == RouteType.Unit)
                    return CurrentIndex;
                if (RouteMode.HasFlag(RouteMode.Reverse))
                    switch (RouteType)
                    {
                        case RouteType.Linear:
                        case RouteType.PingPong:
                            return Count - 1;
                    }
                else
                    switch (RouteType)
                    {
                        case RouteType.Linear:
                        case RouteType.PingPong:
                            return 0;
                    }
                return null;
            }
        }

        /// <summary>
        /// Defines the default value for the "<c>EndIndex</c>" for the current state.
        /// </summary>
        private int? EInd
        {
            get
            {
                if (RouteType == RouteType.Empty || RouteMode.HasFlag(RouteMode.Loop) || Count == 0)
                    return null;
                if (RouteType == RouteType.Unit)
                    return CurrentIndex;
                if (RouteMode.HasFlag(RouteMode.Reverse))
                    switch (RouteType)
                    {
                        case RouteType.Linear:
                            return 0;
                        case RouteType.PingPong:
                            return Count - 1;
                    }
                else
                    switch (RouteType)
                    {
                        case RouteType.Linear:
                            return Count - 1;
                        case RouteType.PingPong:
                            return 0;
                    }
                return null;
            }
        }

        /// <summary>
        /// Defines the updated value for the "<c>NextIndex</c>" for the current state.
        /// </summary>
        private int? NInd
        {
            get
            {
                if (RouteType == RouteType.Empty || CurrentIndex == null || Count == 0)
                    return null;
                if (RouteType == RouteType.Unit)
                    if (RouteMode.HasFlag(RouteMode.Loop))
                        return CurrentIndex;
                    else
                        return null;
                if (CurrentIndex == EndIndex && !RouteMode.HasFlag(RouteMode.Loop) &&
                    (RouteType == RouteType.Linear || RouteType == RouteType.PingPong && (RouteMode.HasFlag(RouteMode.Reverse) == _increase)))
                    return null;
                int _next = (int)CurrentIndex;
                if (_increase)
                    _next++;
                else
                    _next--;
                if (_next == Count)
                {
                    if (RouteType == RouteType.Linear)
                        _next = 0;
                    if (RouteType == RouteType.PingPong)
                        _next = Count - 2;
                }
                else if (_next == -1)
                {
                    if (RouteType == RouteType.Linear)
                        _next = Count - 1;
                    if (RouteType == RouteType.PingPong)
                        _next = 1;
                }
                if (_next >= Count)
                    _next = Count - 1;
                if (_next < 0)
                    _next = 0;
                return _next;
            }
        }

        /// <summary>
        /// Defines the updated value for the "<c>_remainder</c>" for the current state.
        /// </summary>
        private List<T> Rmndr
        {
            get
            {
                List<T> remainder = new List<T>();
                if (RouteType == RouteType.Empty || CurrentIndex == null || Count == 0)
                    return remainder;
                int c = (int)CurrentIndex;
                if (RouteType == RouteType.Unit)
                {
                    remainder.Add(this[c]);
                    return remainder;
                }
                if (RouteType == RouteType.Linear)
                {
                    int i = c;
                    int step = _increase ? 1 : -1;
                    for (; i >= 0 && i < Count; i += step)
                        remainder.Add(this[i]);
                    if (i < 0)
                        i = Count - 1;
                    if (i >= Count)
                        i = 0;
                    if (RouteMode.HasFlag(RouteMode.Loop))
                        for (; i != c; i += step)
                            remainder.Add(this[i]);
                    return remainder;
                }
                if (RouteType == RouteType.PingPong)
                {
                    int i = c;
                    int last = Count - 1;
                    int step = _increase ? 1 : -1;
                    for (; i >= 0 && i <= last; i += step)
                        remainder.Add(this[i]);
                    if (i < 0)
                        i = 0;
                    if (i > last)
                        i = last;
                    if (i == EndIndex && !RouteMode.HasFlag(RouteMode.Loop) && _increase == RouteMode.HasFlag(RouteMode.Reverse))
                        return remainder;
                    step = -step;
                    i += step;
                    if (i < 0)
                        i = 0;
                    if (i > last)
                        i = last;
                    for (; i > 0 && i < last; i += step)
                        remainder.Add(this[i]);
                    if (i != c || !RouteMode.HasFlag(RouteMode.Loop))
                        remainder.Add(this[i]);
                    if (i == c || i == EndIndex && !RouteMode.HasFlag(RouteMode.Loop))
                        return remainder;
                    step = -step;
                    i += step;
                    for (; i != c; i += step)
                        remainder.Add(this[i]);
                    return remainder;
                }
                return remainder;
            }
        }

        /// <summary>
        /// List of items in the remainder. Items can be repeated.
        /// </summary>
        public List<T> Remainder
        {
            get
            {
                if (_remainder == null)
                    _remainder = Rmndr;
                List<T> list = new List<T>(_remainder.Count);
                for (int i = 0; i < _remainder.Count; i++)
                    list.Add(_remainder[i]);
                return list;
            }
        }

        /// <inheritdoc/>
        public RouteType RouteType { get; private set; }

        /// <inheritdoc/>
        public RouteMode RouteMode { get; private set; }

        /// <inheritdoc/>
        public int? CurrentIndex { get; private set; }

        /// <inheritdoc/>
        public int? BeginIndex { get; private set; }

        /// <inheritdoc/>
        public int? EndIndex { get; private set; }

        /// <inheritdoc/>
        public int? NextIndex { get; private set; }

        /// <inheritdoc/>
        public object Current
        {
            get
            {
                if (CurrentIndex != null && CurrentIndex >= 0 && CurrentIndex < Count)
                    return this[(int)CurrentIndex];
                return null;
            }
        }

        /// <inheritdoc/>
        public object Begin
        {
            get
            {
                if (BeginIndex != null && BeginIndex >= 0 && BeginIndex < Count)
                    return this[(int)BeginIndex];
                return null;
            }
        }

        /// <inheritdoc/>
        public object End
        {
            get
            {
                if (EndIndex != null && EndIndex >= 0 && EndIndex < Count)
                    return this[(int)EndIndex];
                return null;
            }
        }

        /// <inheritdoc/>
        public object Next
        {
            get
            {
                if (NextIndex != null && NextIndex >= 0 && NextIndex < Count)
                    return this[(int)NextIndex];
                return null;
            }
        }

        /// <inheritdoc/>
        public int RemainderCount
        {
            get
            {
                if (RouteType == RouteType.Empty || CurrentIndex == null || Count == 0)
                    return 0;
                if (RouteMode.HasFlag(RouteMode.Loop))
                    return -1;
                if (RouteType == RouteType.Unit)
                    return 1;
                int c = (int)CurrentIndex;
                if (RouteType == RouteType.Linear)
                    if (RouteMode.HasFlag(RouteMode.Reverse))
                        return c + 1;
                    else
                        return Count - c;
                if (RouteType == RouteType.PingPong)
                    if (RouteMode.HasFlag(RouteMode.Reverse))
                        if (_increase)
                            return Count - c;
                        else
                            return Count + c;
                    else if (_increase)
                        return 2 * Count - 1 - c;
                    else
                        return c + 1;
                return 0;
            }
        }

        /// <inheritdoc/>
        public void MoveNext()
        {
            if (CurrentIndex is null)
                return;
            _remainder = null;
            if (RouteType == RouteType.PingPong && CurrentIndex != null && NextIndex != null && CurrentIndex < NextIndex != _increase)
                _increase = !_increase;
            CurrentIndex = NextIndex;
            NextIndex = NInd;
        }

        /// <inheritdoc/>
        public void ReverseDirection()
        {
            switch (RouteType)
            {
                case RouteType.Empty:
                case RouteType.PingPong:
                case RouteType.Unit:
                    _increase = !_increase;
                    return;
            }
        }

        /// <inheritdoc/>
        public void SetRouteType(RouteType routeType)
        {
            if (RouteType != routeType)
                _remainder = null;
            else
                return;
            RouteType = routeType;
            _increase = Increase;
            BeginIndex = BInd;
            EndIndex = EInd;
            NextIndex = NInd;
        }

        /// <inheritdoc/>
        public void SetRouteMode(RouteMode routeMode)
        {
            if (RouteMode != routeMode)
                _remainder = null;
            else
                return;
            if (RouteMode.HasFlag(RouteMode.Reverse) != routeMode.HasFlag(RouteMode.Reverse))
                _increase = !_increase;
            RouteMode = routeMode;
            BeginIndex = BInd;
            EndIndex = EInd;
            NextIndex = NInd;
        }

        /// <inheritdoc/>
        public void SetCurrentIndex(int? index)
        {
            if (CurrentIndex != index)
                _remainder = null;
            else
                return;
            CurrentIndex = index;
            if (CurrentIndex != null && (CurrentIndex < 0 || CurrentIndex >= Count))
                CurrentIndex = null;
            NextIndex = NInd;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _remainder = null;
            _increase = !RouteMode.HasFlag(RouteMode.Reverse);
            CurrentIndex = CInd;
            NextIndex = NInd;
        }

        /// <inheritdoc/>
        public void Update()
        {
            _remainder = null;
            if (CurrentIndex != null && (CurrentIndex < 0 || CurrentIndex >= Count))
                CurrentIndex = null;
            BeginIndex = BInd;
            EndIndex = EInd;
            NextIndex = NInd;
        }

        /// <inheritdoc/>
        public void Update(int? current)
        {
            _remainder = null;
            CurrentIndex = current;
            if (CurrentIndex != null && (CurrentIndex < 0 || CurrentIndex >= Count))
                CurrentIndex = null;
            BeginIndex = BInd;
            EndIndex = EInd;
            NextIndex = NInd;
        }

        /// <inheritdoc/>
        public void Update(int index, int offset)
        {
            _remainder = null;
            if (CurrentIndex != null)
            {
                if (offset > 0 && index <= CurrentIndex)
                    CurrentIndex += offset;
                else if (offset < 0)
                    if (index <= CurrentIndex + offset)
                        CurrentIndex += offset;
                    else if (index <= CurrentIndex)
                        CurrentIndex = null;
                if (CurrentIndex < 0 || CurrentIndex >= Count)
                    CurrentIndex = null;
            }
            BeginIndex = BInd;
            EndIndex = EInd;
            NextIndex = NInd;
        }

        /// <inheritdoc/>
        public new void Add(T item)
        {
            base.Add(item);
            Update();
        }

        /// <inheritdoc/>
        public new void AddRange(IEnumerable<T> collection)
        {
            if (collection is null)
                throw new ArgumentNullException(nameof(collection));
            base.AddRange(collection);
            Update();
        }

        /// <inheritdoc/>
        public new void Clear()
        {
            base.Clear();
            Update(null);
        }

        /// <inheritdoc/>
        public new void Insert(int index, T item)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            base.Insert(index, item);
            Update(index, 1);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public new void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            base.RemoveAt(index);
            Update(index, -1);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public new void Reverse()
        {
            base.Reverse();
            if (Count > 1)
                if (CurrentIndex != null)
                    Update(Count - 1 - CurrentIndex);
                else
                    Update();
        }

        /// <inheritdoc/>
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
                if (CurrentIndex != null && index <= CurrentIndex && index + count > CurrentIndex)
                    Update(2 * index + count - 1 - CurrentIndex);
                else
                    Update();
        }

        /// <inheritdoc/>
        public new void Sort(Comparison<T> comparison)
        {
            if (comparison is null)
                throw new ArgumentNullException(nameof(comparison));
            bool notSorted = true;
            int index = -1;
            if (CurrentIndex != null)
                index = (int)CurrentIndex;
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
            if (index != -1 && index != CurrentIndex)
                Update(index);
            else
                Update();
        }

        /// <inheritdoc/>
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
            if (CurrentIndex != null)
                ind = (int)CurrentIndex;
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
            if (ind != -1 && ind != CurrentIndex)
                Update(ind);
            else
                Update();
        }

        /// <inheritdoc/>
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
            if (CurrentIndex != null)
                ind = (int)CurrentIndex;
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
            if (ind != -1 && ind != CurrentIndex)
                Update(ind);
            else
                Update();
        }

        /// <inheritdoc/>
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
            if (CurrentIndex != null)
                ind = (int)CurrentIndex;
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
            if (ind != -1 && ind != CurrentIndex)
                Update(ind);
            else
                Update();
        }

        /// <inheritdoc/>
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
                XmlElement xmlRoute = xmlDocument.CreateElement("RouteType");
                xmlThis.AppendChild(xmlRoute);
                xmlRoute.InnerText = RouteType.ToString();
                XmlElement xmlRouteMode = xmlDocument.CreateElement("RouteMode");
                xmlThis.AppendChild(xmlRouteMode);
                xmlRouteMode.InnerText = RouteMode.ToString();
                XmlElement xmlIncrease = xmlDocument.CreateElement("Increase");
                xmlThis.AppendChild(xmlIncrease);
                xmlIncrease.InnerText = _increase.ToString();
                XmlElement xmlCurrentIndex = xmlDocument.CreateElement("CurrentIndex");
                xmlThis.AppendChild(xmlCurrentIndex);
                xmlCurrentIndex.InnerText = CurrentIndex is null ? "null" : CurrentIndex.ToString();
                XmlElement xmlEntries = xmlDocument.CreateElement("Entries");
                xmlThis.AppendChild(xmlEntries);
                foreach (T entry in this)
                {
                    XmlElement xmlEntry = CyclicalMethods.ToXmlElement(entry, xmlDocument, ref id_source, "Entry");
                    xmlEntries.AppendChild(xmlEntry);
                }
            }
            return xmlThis;
        }
        
        /// <inheritdoc/>
        public XmlElement ToXmlElement(XmlDocument xmlDocument, string name = "")
        {
            if (xmlDocument is null)
                throw new ArgumentNullException(nameof(xmlDocument));
            if (name is null)
                throw new ArgumentNullException(nameof(name));
            IList<object> is_source = new List<object>();
            return ToXmlElement(xmlDocument, ref is_source, name);
        }

        /// <inheritdoc/>
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
                foreach (T t in this)
                {
                    hashCode *= 2;
                    hashCode += CyclicalMethods.GetHashCode(t, ref hashCodes);
                }
                sourceHash.Right = hashCode;
                return hashCode;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            IList<LeftRightPair<object, int>> hashCodes = new List<LeftRightPair<object, int>>();
            return GetHashCode(ref hashCodes);
        }

        /// <inheritdoc/>
        int IList.Add(object item)
        {
            if (item is T)
            {
                int result = ((IList)this).Add(item);
                Update();
                return result;
            }
            throw new ArgumentException($"{nameof(item)} is of a type that cannot be assigned to IList.");
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedList{T}"/> class that is empty and has the default initial capacity, route type and route mode.
        /// </summary>
        public RoutedList() : base()
        {
            _remainder = null;
            RouteType = RouteType.Linear;
            RouteMode = RouteMode.Default;
            _increase = !RouteMode.HasFlag(RouteMode.Reverse);
            CurrentIndex = CInd;
            BeginIndex = BInd;
            EndIndex = EInd;
            NextIndex = NInd;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedList{T}"/> class that is empty and has the default initial capacity and the specified initial route type and route mode.
        /// </summary>
        /// <param name="routeType"></param>
        /// <param name="routeMode"></param>
        public RoutedList(RouteType routeType, RouteMode routeMode) : base()
        {
            _remainder = null;
            RouteType = routeType;
            RouteMode = routeMode;
            _increase = !RouteMode.HasFlag(RouteMode.Reverse);
            CurrentIndex = CInd;
            BeginIndex = BInd;
            EndIndex = EInd;
            NextIndex = NInd;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedList{T}"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied and has the default initial route type and route mode.
        /// </summary>
        /// <param name="collection"></param>
        public RoutedList(IEnumerable<T> collection) : base(collection)
        {
            _remainder = null;
            RouteType = RouteType.Linear;
            RouteMode = RouteMode.Default;
            _increase = !RouteMode.HasFlag(RouteMode.Reverse);
            CurrentIndex = CInd;
            BeginIndex = BInd;
            EndIndex = EInd;
            NextIndex = NInd;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedList{T}"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied and the specified initial route type and route mode.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="routeType"></param>
        /// <param name="routeMode"></param>
        public RoutedList(IEnumerable<T> collection, RouteType routeType, RouteMode routeMode) : base(collection)
        {
            _remainder = null;
            RouteType = routeType;
            RouteMode = routeMode;
            _increase = !RouteMode.HasFlag(RouteMode.Reverse);
            CurrentIndex = CInd;
            BeginIndex = BInd;
            EndIndex = EInd;
            NextIndex = NInd;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedList{T}"/> class that is empty and has the specified initial capacity and the default initial route type and route mode.
        /// </summary>
        /// <param name="capacity"></param>
        public RoutedList(int capacity) : base(capacity)
        {
            _remainder = null;
            RouteType = RouteType.Linear;
            RouteMode = RouteMode.Default;
            _increase = !RouteMode.HasFlag(RouteMode.Reverse);
            CurrentIndex = CInd;
            BeginIndex = BInd;
            EndIndex = EInd;
            NextIndex = NInd;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedList{T}"/> class that is empty and has the specified initial capacity, route type and route mode.
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="routeType"></param>
        /// <param name="routeMode"></param>
        public RoutedList(int capacity, RouteType routeType, RouteMode routeMode) : base(capacity)
        {
            _remainder = null;
            RouteType = routeType;
            RouteMode = routeMode;
            _increase = !RouteMode.HasFlag(RouteMode.Reverse);
            CurrentIndex = CInd;
            BeginIndex = BInd;
            EndIndex = EInd;
            NextIndex = NInd;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedList{T}"/> class that contains elements, route type, route mode, direction and current index copied from the source and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="source"></param>
        public RoutedList(RoutedList<T> source) : base(source)
        {
            _remainder = source._remainder;
            RouteType = source.RouteType;
            RouteMode = source.RouteMode;
            _increase = source._increase;
            CurrentIndex = source.CurrentIndex;
            BeginIndex = source.BeginIndex;
            EndIndex = source.EndIndex;
            NextIndex = source.NextIndex;
        }

        /// <summary>
        /// Creates an object from its <see cref="XmlElement"/> representation.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id_source"></param>
        /// <returns></returns>
        public static RoutedList<T> ToObject(XmlElement source, ref IDictionary<string, object> id_source)
        {
            Type type = typeof(RoutedList<T>);
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (id_source is null)
                throw new ArgumentNullException(nameof(id_source));
            if (source.HasAttribute("id"))
                if (id_source.ContainsKey(source.GetAttribute("id")))
                    throw new XmlException($"The object with id=\"{source.GetAttribute("id")}\" has already been created.");
            if (source.HasAttribute("ref"))
                if (id_source.ContainsKey(source.GetAttribute("ref")))
                    if (id_source[source.GetAttribute("ref")] is RoutedList<T> _source)
                        return _source;
                    else
                        throw new XmlException($"The object with id=\"{source.GetAttribute("id")}\" must be \"{type.FullName}\".");
                else
                    throw new XmlException($"The object with id=\"{source.GetAttribute("id")}\" must have been created earlier.");
            RoutedList<T> routedList = new RoutedList<T>();
            if (source.HasAttribute("id"))
                id_source.Add(source.GetAttribute("id"), routedList);
            XmlNodeList xmlEntriesList = source.ChildNodes;
            foreach (XmlElement xmlElement in xmlEntriesList)
                switch (xmlElement.Name)
                {
                    case "RouteType":
                        RouteType routeType;
                        if (Enum.TryParse(xmlElement.InnerText, out routeType))
                            routedList.RouteType = routeType;
                        else
                            throw new XmlException($"Invalid data in the node \"{xmlElement.Name}\".");
                        break;
                    case "RouteMode":
                        RouteMode routeMode;
                        if (Enum.TryParse(xmlElement.InnerText, out routeMode))
                            routedList.RouteMode = routeMode;
                        else
                            throw new XmlException($"Invalid data in the node \"{xmlElement.Name}\".");
                        break;
                    case "Increase":
                        if (xmlElement.InnerText == bool.TrueString)
                            routedList._increase = true;
                        else if (xmlElement.InnerText == bool.FalseString)
                            routedList._increase = false;
                        else
                            throw new XmlException($"Invalid data in the node \"{xmlElement.Name}\".");
                        break;
                    case "CurrentIndex":
                        if (xmlElement.InnerText == "null")
                            routedList.CurrentIndex = null;
                        else if (int.TryParse(xmlElement.InnerText, out int currentIndex))
                            routedList.CurrentIndex = currentIndex;
                        else
                            throw new XmlException($"Invalid data in the node \"{xmlElement.Name}\".");
                        break;
                    case "Entries":
                        List<T> list = routedList as List<T>;
                        foreach (XmlElement xmlEntry in xmlElement.ChildNodes)
                            if (xmlEntry.Name != "Entry")
                                throw new XmlException("The \"Entries\" node can only contain \"Entry\" nodes.");
                            else if (CyclicalMethods.ToObject(xmlEntry, ref id_source) is T t)
                                list.Add(t);
                            else
                                throw new XmlException($"Invalid data in the node \"{xmlElement.Name}\".");
                        break;
                }
            routedList._increase = routedList.Increase;
            routedList.Update();
            return routedList;
        }

        /// <summary>
        /// Creates an object from its <see cref="XmlElement"/> representation.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static RoutedList<T> ToObject(XmlElement source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            IDictionary<string, object> id_source = new Dictionary<string, object>();
            return ToObject(source, ref id_source);
        }

        /// <summary>
        /// Initializes <see cref="ToObject(XmlElement, ref IDictionary{string, object})"/> and <see cref="ToObject(XmlElement)"/> converters.
        /// </summary>
        public static void InitializeToObjectConverters()
        {
            string typeFullName = typeof(RoutedList<T>).FullName;
            if (!CyclicalMethods.XmlElementConverters.ContainsKey(typeFullName))
                CyclicalMethods.XmlElementConverters.Add(typeFullName, ToObject);
            if (!CyclicalMethods.XmlElementIdConverters.ContainsKey(typeFullName))
                CyclicalMethods.XmlElementIdConverters.Add(typeFullName, ToObject);
        }
    }
}