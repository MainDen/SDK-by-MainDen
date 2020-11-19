// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using MainDen.Collections.Generic;
using MainDen.Cyclical;
using MainDen.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;

namespace MainDen.Automation
{
    public abstract class Worker : IConvertible, ICyclical, ICyclicalConvertible, IMain, IWorker
    {
        public delegate void IWorkerEventHandler(IWorker sender);

        protected string _name;
        
        protected string _description;
        
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentOutOfRangeException(nameof(value), "Must not be null or empty.");
                _name = value;
            }
        }
        
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));
                _description = value;
            }
        }
        
        public virtual string Debug
        {
            get
            {
                string res = GetType().Name + ":";
                res += "\nName: " + _name;
                res += "\nDescription: " + _description;
                res += "\nEnabled: " + Enabled;
                res += "\nStatus: " + Status;
                return res;
            }
        }
        
        public bool Enabled { get; private set; }
        
        public Status Status { get; private set; }
        
        public virtual void Enable()
        {
            Enabled = true;
            Status = Status.Running;
        }
        
        public virtual void Disable()
        {
            Enabled = false;
            Status = Status.Stopped;
        }
        
        public virtual void Complete()
        {
            Enabled = false;
            Status = Status.Completed;
        }
        
        public virtual void Reset()
        {
            Enabled = false;
            Status = Status.Ready;
        }
        
        public virtual void Error()
        {
            Enabled = false;
            Status = Status.Error;
            OnError?.Invoke(this);
        }
        
        public abstract void Work();
        
        public virtual void ChangeStatus(Status status)
        {
            switch (status)
            {
                case Status.Running:
                    Enable();
                    break;
                case Status.Stopped:
                    Disable();
                    break;
                case Status.Completed:
                    Complete();
                    break;
                case Status.Ready:
                    Reset();
                    break;
                case Status.Error:
                    Error();
                    break;
            }
            OnStatusChanged?.Invoke(this);
        }
        
        public virtual int GetHashCode(ref IList<LeftRightPair<object, int>> hashCodes)
        {
            if (hashCodes is null)
                throw new ArgumentNullException(nameof(hashCodes));
            foreach (LeftRightPair<object, int> pair in hashCodes)
                if (pair.Left == this)
                    return pair.Right;
            LeftRightPair<object, int> sourceHash = new LeftRightPair<object, int>(this, 0);
            hashCodes.Add(sourceHash);
            return sourceHash.Right;
        }
        
        public override int GetHashCode()
        {
            IList<LeftRightPair<object, int>> hashCodes = new List<LeftRightPair<object, int>>();
            return GetHashCode(ref hashCodes);
        }
        
        public virtual XmlElement ToXmlElement(XmlDocument xmlDocument, ref IList<object> id_source, string name = "")
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
            }
            return xmlThis;
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
        
        public Worker()
        {
            _name = GetType().Name;
            _description = GetType().Name;
            Enabled = false;
            Status = default;
        }
        
        public event IWorkerEventHandler OnStatusChanged;
        
        public event IWorkerEventHandler OnError;
    }
}