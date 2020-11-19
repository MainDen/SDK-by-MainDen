// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using MainDen.Enums;

namespace MainDen.Automation
{
    public interface IWorker
    {
        bool Enabled { get; }
        Status Status { get; }
        void Enable();
        void Disable();
        void Complete();
        void Reset();
        void Error();
        void Work();
        void ChangeStatus(Status status);
        event Worker.IWorkerEventHandler OnStatusChanged;
        event Worker.IWorkerEventHandler OnError;
    }
}