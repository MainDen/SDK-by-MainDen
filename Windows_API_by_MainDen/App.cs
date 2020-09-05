// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System;

namespace Windows_API_by_MainDen
{
    public sealed class App
    {
        private IntPtr windHandle = IntPtr.Zero;
        private IntPtr procHandle = IntPtr.Zero;
        private uint procId = 0;
        private uint thrdId = 0;
        private string appName = "";
        private string description = "";
        public App()
        {
            windHandle = IntPtr.Zero;
            procHandle = IntPtr.Zero;
        }
        public App(string appName)
        {
            this.appName = appName;
            if (this.appName != "")
            {
                windHandle = WinAPI.Wind.FindWindow(null, this.appName);
                if (windHandle != IntPtr.Zero)
                {
                    thrdId = WinAPI.Wind.GetWindowThreadProcessId(windHandle, out procId);
                    procHandle = WinAPI.Proc.OpenProcess(WinAPI.Proc.ProcessAccessFlags.All, false, procId);
                    if (procHandle == IntPtr.Zero)
                        windHandle = IntPtr.Zero;
                }
            }
            else
                windHandle = IntPtr.Zero;
        }
        public App(string appName, string description)
        {
            this.appName = appName;
            this.description = description;
            if (this.appName != "")
            {
                windHandle = WinAPI.Wind.FindWindow(null, this.appName);
                if (windHandle != IntPtr.Zero)
                {
                    thrdId = WinAPI.Wind.GetWindowThreadProcessId(windHandle, out procId);
                    procHandle = WinAPI.Proc.OpenProcess(WinAPI.Proc.ProcessAccessFlags.All, false, procId);
                    if (procHandle == IntPtr.Zero)
                        windHandle = IntPtr.Zero;
                }
            }
            else
                windHandle = IntPtr.Zero;
        }
        public App Reset()
        {
            if (this.appName != "")
            {
                windHandle = WinAPI.Wind.FindWindow(null, this.appName);
                if (windHandle != IntPtr.Zero)
                {
                    thrdId = WinAPI.Wind.GetWindowThreadProcessId(windHandle, out procId);
                    procHandle = WinAPI.Proc.OpenProcess(WinAPI.Proc.ProcessAccessFlags.All, false, procId);
                    if (procHandle == IntPtr.Zero)
                        windHandle = IntPtr.Zero;
                }
            }
            else
                windHandle = IntPtr.Zero;
            return this;
        }
        public App Reset(string appName)
        {
            this.appName = appName;
            if (this.appName != "")
            {
                windHandle = WinAPI.Wind.FindWindow(null, this.appName);
                if (windHandle != IntPtr.Zero)
                {
                    thrdId = WinAPI.Wind.GetWindowThreadProcessId(windHandle, out procId);
                    procHandle = WinAPI.Proc.OpenProcess(WinAPI.Proc.ProcessAccessFlags.All, false, procId);
                    if (procHandle == IntPtr.Zero)
                        windHandle = IntPtr.Zero;
                }
            }
            else
                windHandle = IntPtr.Zero;
            return this;
        }
        public App Reset(string appName, string description)
        {
            this.appName = appName;
            this.description = description;
            if (this.appName != "")
            {
                windHandle = WinAPI.Wind.FindWindow(null, this.appName);
                if (windHandle != IntPtr.Zero)
                {
                    thrdId = WinAPI.Wind.GetWindowThreadProcessId(windHandle, out procId);
                    procHandle = WinAPI.Proc.OpenProcess(WinAPI.Proc.ProcessAccessFlags.All, false, procId);
                    if (procHandle == IntPtr.Zero)
                        windHandle = IntPtr.Zero;
                }
            }
            else
                windHandle = IntPtr.Zero;
            return this;
        }
        public bool Exist()
        {
            return windHandle != IntPtr.Zero && windHandle == WinAPI.Wind.FindWindow(null, appName);
        }
        public bool Active()
        {
            return windHandle != IntPtr.Zero && windHandle == WinAPI.Wind.GetForegroundWindow();
        }
        public string Name()
        {
            return appName;
        }
        public App Name(string appName)
        {
            if (Exist())
                if (WinAPI.Wind.SetWindowText(windHandle, appName))
                    this.appName = appName;
            return this;
        }
        public string Description()
        {
            return description;
        }
        public App Description(string newVal)
        {
            description = newVal;
            return this;
        }
        public IntPtr WindHandle()
        {
            return windHandle;
        }
        public IntPtr ProcHandle()
        {
            return procHandle;
        }
        public uint ProcId()
        {
            return procId;
        }
        public uint ThrdId()
        {
            return thrdId;
        }
    }
}
