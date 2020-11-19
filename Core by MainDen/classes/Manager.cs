// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using MainDen.Collections;
using MainDen.Collections.Generic;
using MainDen.Cyclical;
using MainDen.Enums;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MainDen.Automation
{
    public class Manager : Worker, IConvertible, ICyclicalConvertible, IMain, IManager, IRouted, IWorker
    {
        protected readonly RoutedList<IWorker> _workers;
        
        public override string Debug
        {
            get
            {
                string res = base.Debug;
                res += $"\nRouteType: {RouteType}";
                res += $"\nRouteMode: {RouteMode}";
                res += $"\nWorkersCount: {WorkersCount}";
                res += $"\nRemainderCount: {RemainderCount}";
                res += $"\nCurrentIndex: ";
                if (CurrentIndex is null)
                    res += "null";
                else
                    res += CurrentIndex.ToString();
                IWorker worker = Current as IWorker;
                if (worker != null)
                    if (worker is IMain main)
                    {
                        res += "\n  Current.Debug:";
                        res += $"\n{main.Debug}";
                    }
                    else
                    {
                        res += "\n  Current.Debug:";
                        res += $"\nEnabled: {worker.Enabled}";
                        res += $"\nStatus: {worker.Status}";
                    }
                return res;
            }
        }
        
        public RoutedList<IWorker> Workers
        {
            get
            {
                return new RoutedList<IWorker>(_workers);
            }
        }
        
        public int WorkersCount => _workers.Count;
        
        public RouteType RouteType => _workers.RouteType;
        
        public RouteMode RouteMode => _workers.RouteMode;
        
        public int? CurrentIndex => _workers.CurrentIndex;
        
        public int? BeginIndex => _workers.CurrentIndex;
        
        public int? EndIndex => _workers.CurrentIndex;
        
        public int? NextIndex => _workers.CurrentIndex;
        
        public object Current => _workers.Current;
        
        public object Begin => _workers.Begin;
        
        public object End => _workers.End;
        
        public object Next => _workers.Next;
        
        public int RemainderCount => _workers.RemainderCount;
        
        public void MoveNext() => _workers.MoveNext();

        public void ReverseDirection() => _workers.ReverseDirection();

        public void SetRouteType(RouteType routeType) => _workers.SetRouteType(routeType);

        public void SetRouteMode(RouteMode routeMode) => _workers.SetRouteMode(routeMode);

        public void SetCurrentIndex(int? index) => _workers.SetCurrentIndex(index);

        public void Update() => _workers.Update();
        
        public void Update(int? current) => _workers.Update(current);
        
        public void Update(int index, int offset) => _workers.Update(index, offset);
        
        public override void Reset()
        {
            base.Reset();
            _workers.Reset();
        }
        
        public override void Work()
        {
            if (Enabled && Status == Status.Running)
            {
                IWorker worker = (IWorker)_workers.Current;
                if (worker == null)
                    Complete();
                else
                {
                    switch (worker.Status)
                    {
                        case Status.Ready:
                            worker.Enable();
                            break;
                        case Status.Completed:
                            _workers.MoveNext();
                            break;
                        case Status.Running:
                            worker.Work();
                            break;
                    }
                }
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
        
        public virtual void StatusChanged(IWorker worker)
        {
            if (worker is null)
                throw new ArgumentNullException(nameof(worker));
            if (Enabled && Status == Status.Running)
                switch (worker.Status)
                {
                    case Status.Completed:
                        if (_workers.Current == worker)
                            _workers.MoveNext();
                        break;
                }
        }
        
        public virtual void Error(IWorker worker)
        {
            if (worker is null)
                throw new ArgumentNullException(nameof(worker));
            if (Enabled && Status == Status.Running && Current == worker)
                Error();
        }
        
        public override XmlElement ToXmlElement(XmlDocument xmlDocument, ref IList<object> id_source, string name = "")
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
                xmlThis.SetAttribute("type", GetType().FullName);
                XmlElement xmlName = xmlDocument.CreateElement("Name");
                xmlThis.AppendChild(xmlName);
                xmlName.InnerText = _name;
                XmlElement xmlDescription = xmlDocument.CreateElement("Description");
                xmlThis.AppendChild(xmlDescription);
                xmlDescription.InnerText = _description;
                XmlElement xmlRouteType = xmlDocument.CreateElement("RouteType");
                xmlThis.AppendChild(xmlRouteType);
                xmlRouteType.InnerText = RouteType.ToString();
                XmlElement xmlRouteMode = xmlDocument.CreateElement("RouteMode");
                xmlThis.AppendChild(xmlRouteMode);
                xmlRouteMode.InnerText = RouteMode.ToString();
                XmlElement xmlWorkers = xmlDocument.CreateElement("Workers");
                xmlThis.AppendChild(xmlWorkers);
                foreach (object worker in _workers)
                {
                    XmlElement xmlWorker = CyclicalMethods.ToXmlElement(worker, xmlDocument, ref id_source, "Worker");
                    xmlWorkers.AppendChild(xmlWorker);
                }
            }
            return xmlThis;
        }
        
        public IWorker this[int index]
        {
            get
            {
                if (index < 0 || index >= _workers.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _workers[index];
            }
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));
                if (index < 0 || index >= _workers.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                _workers[index] = value;
            }
        }
        
        public Manager() : base()
        {
            _workers = new RoutedList<IWorker>();
        }
        
        public Manager(RouteType routeType, RouteMode routeMode) : base()
        {
            _workers = new RoutedList<IWorker>(routeType, routeMode);
        }
        
        public Manager(IEnumerable<IWorker> workers) : base()
        {
            _workers = new RoutedList<IWorker>(workers);
        }
        
        public Manager(IEnumerable<IWorker> workers, RouteType routeType, RouteMode routeMode) : base()
        {
            _workers = new RoutedList<IWorker>(workers, routeType, routeMode);
        }

        /// <summary>
        /// Creates an object from its <see cref="XmlElement"/> representation.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="id_source"></param>
        /// <returns></returns>
        public static Manager ToObject(XmlElement source, ref IDictionary<string, object> id_source)
        {
            Type type = typeof(Manager);
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (id_source is null)
                throw new ArgumentNullException(nameof(id_source));
            if (source.HasAttribute("id"))
                if (id_source.ContainsKey(source.GetAttribute("id")))
                    throw new XmlException($"The object with id=\"{source.GetAttribute("id")}\" has already been created.");
            if (source.HasAttribute("ref"))
                if (id_source.ContainsKey(source.GetAttribute("ref")))
                    if (id_source[source.GetAttribute("ref")] is Manager _source)
                        return _source;
                    else
                        throw new XmlException($"The object with id=\"{source.GetAttribute("id")}\" must be \"{type.FullName}\".");
                else
                    throw new XmlException($"The object with id=\"{source.GetAttribute("id")}\" must have been created earlier.");
            Manager manager = new Manager();
            if (source.HasAttribute("id"))
                id_source.Add(source.GetAttribute("id"), manager);
            XmlNodeList xmlEntriesList = source.ChildNodes;
            foreach (XmlElement xmlElement in xmlEntriesList)
                switch (xmlElement.Name)
                {
                    case "Name":
                        if (!string.IsNullOrEmpty(xmlElement.InnerText))
                            manager.Name = xmlElement.InnerXml;
                        else
                            throw new XmlException($"Invalid data in the node \"{xmlElement.Name}\".");
                        break;
                    case "Description":
                        if (xmlElement.InnerText != null)
                            manager.Description = xmlElement.InnerText;
                        else
                            throw new XmlException($"Invalid data in the node \"{xmlElement.Name}\".");
                        break;
                    case "RouteType":
                        RouteType routeType;
                        if (Enum.TryParse(xmlElement.InnerText, out routeType))
                            manager.SetRouteType(routeType);
                        else
                            throw new XmlException($"Invalid data in the node \"{xmlElement.Name}\".");
                        break;
                    case "RouteMode":
                        RouteMode routeMode;
                        if (Enum.TryParse(xmlElement.InnerText, out routeMode))
                            manager.SetRouteMode(routeMode);
                        else
                            throw new XmlException($"Invalid data in the node \"{xmlElement.Name}\".");
                        break;
                    case "Workers":
                        List<IWorker> list = manager._workers;
                        foreach (XmlElement xmlEntry in xmlElement.ChildNodes)
                            if (xmlEntry.Name != "Worker")
                                throw new XmlException("The \"Entries\" node can only contain \"Worker\" nodes.");
                            else if (CyclicalMethods.ToObject(xmlEntry, ref id_source) is IWorker worker)
                                list.Add(worker);
                            else
                                throw new XmlException($"Invalid data in the node \"{xmlElement.Name}\".");
                        break;
                }
            manager.Reset();
            return manager;
        }

        /// <summary>
        /// Creates an object from its <see cref="XmlElement"/> representation.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Manager ToObject(XmlElement source)
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
            string typeFullName = typeof(Manager).FullName;
            if (!CyclicalMethods.XmlElementConverters.ContainsKey(typeFullName))
                CyclicalMethods.XmlElementConverters.Add(typeFullName, ToObject);
            if (!CyclicalMethods.XmlElementIdConverters.ContainsKey(typeFullName))
                CyclicalMethods.XmlElementIdConverters.Add(typeFullName, ToObject);
        }
    }
}