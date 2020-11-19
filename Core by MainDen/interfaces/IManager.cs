// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using MainDen.Collections.Generic;

namespace MainDen.Automation
{
    public interface IManager : IWorker
    {
        RoutedList<IWorker> Workers { get; }
        int WorkersCount { get; }
        void AddWorker(IWorker worker);
        void RemoveWorker(int index);
        void InsertWorker(int index, IWorker worker);
        void EnableAll();
        void DisableAll();
        void ResetAll();
        void WorkAll();
        void StatusChanged(IWorker worker);
        void Error(IWorker worker);
        IWorker this[int index] { get; set; }
    }
}