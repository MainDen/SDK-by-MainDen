// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Windows_API_by_MainDen
{
    namespace Interceptor
    {
        public class KeyboardHooker : IDisposable
        {
            private int _key;
            public event KeyEventHandler KeyDown;
            public event KeyEventHandler KeyUp;
            private WinAPI.Hook.HookProc _keyProc;
            private IntPtr _keyHHook = IntPtr.Zero;
            public KeyboardHooker()
            {
                _key = 0;
                _keyProc = AllKeyHookProc;
            }
            public KeyboardHooker(int keyCode)
            {
                _key = keyCode;
                _keyProc = KeyHookProc;
            }
            public void SetHook()
            {
                var hInstance = WinAPI.Proc.LoadLibrary("User32");
                _keyHHook = WinAPI.Hook.SetWindowsHookEx(WinAPI.Hook.HookType.WH_KEYBOARD_LL, _keyProc, hInstance, 0);
            }
            public void Dispose()
            {
                UnHook();
            }
            public void UnHook()
            {
                WinAPI.Hook.UnhookWindowsHookEx(_keyHHook);
            }
            private IntPtr KeyHookProc(int code, IntPtr wParam, IntPtr lParam)
            {
                int key = Marshal.ReadInt32(lParam);
                if (code >= 0 && (wParam == (IntPtr)WinAPI.Message.WindowsMessage.KEYDOWN || wParam == (IntPtr)WinAPI.Message.WindowsMessage.SYSKEYDOWN) && key == _key)
                    KeyDown?.Invoke(this, new KeyEventArgs((Keys)key));
                if (code >= 0 && (wParam == (IntPtr)WinAPI.Message.WindowsMessage.KEYUP || wParam == (IntPtr)WinAPI.Message.WindowsMessage.SYSKEYUP) && key == _key)
                    KeyUp?.Invoke(this, new KeyEventArgs((Keys)key));
                return WinAPI.Hook.CallNextHookEx(_keyHHook, code, wParam, lParam);
            }
            private IntPtr AllKeyHookProc(int code, IntPtr wParam, IntPtr lParam)
            {
                int key = Marshal.ReadInt32(lParam);
                if (code >= 0 && (wParam == (IntPtr)WinAPI.Message.WindowsMessage.KEYDOWN || wParam == (IntPtr)WinAPI.Message.WindowsMessage.SYSKEYDOWN))
                    KeyDown?.Invoke(this, new KeyEventArgs((Keys)key));
                if (code >= 0 && (wParam == (IntPtr)WinAPI.Message.WindowsMessage.KEYUP || wParam == (IntPtr)WinAPI.Message.WindowsMessage.SYSKEYUP))
                    KeyUp?.Invoke(this, new KeyEventArgs((Keys)key));
                return WinAPI.Hook.CallNextHookEx(_keyHHook, code, wParam, lParam);
            }
        }
        public class MouseHooker : IDisposable
        {
            public struct tagMSLLHOOKSTRUCT
            {
                public tagMSLLHOOKSTRUCT(int x, int y, uint mouseData)
                {
                    this.x = x;
                    this.y = y;
                    this.mouseData = mouseData;
                    flags = 0;
                    time = 0;
                    dwExtraInfo = UIntPtr.Zero;
                }
                public int x;
                public int y;
                public uint mouseData;
                public uint flags;
                public uint time;
                public UIntPtr dwExtraInfo;
            }
            private WinAPI.Message.WindowsMessage _mb;
            public event MouseEventHandler MouseDown;
            public event MouseEventHandler MouseUp;
            public event MouseEventHandler MouseWheel;
            public event MouseEventHandler MouseHWheel;
            public event MouseEventHandler MouseMove;
            private WinAPI.Hook.HookProc _mbProc;
            private IntPtr _mbHHook = IntPtr.Zero;
            public MouseHooker()
            {
                _mb = 0;
                _mbProc = AllMBHookProc;
            }
            public MouseHooker(int keyCode, WinAPI.Message.WindowsMessage mbCode)
            {
                _mb = mbCode;
                _mbProc = MBHookProc;
            }
            public void SetHook()
            {
                var hInstance = WinAPI.Proc.LoadLibrary("User32");
                _mbHHook = WinAPI.Hook.SetWindowsHookEx(WinAPI.Hook.HookType.WH_MOUSE_LL, _mbProc, hInstance, 0);
            }
            public void Dispose()
            {
                UnHook();
            }
            public void UnHook()
            {
                WinAPI.Hook.UnhookWindowsHookEx(_mbHHook);
            }
            private IntPtr MBHookProc(int code, IntPtr wParam, IntPtr lParam)
            {
                tagMSLLHOOKSTRUCT param = new tagMSLLHOOKSTRUCT();
                param = (tagMSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(tagMSLLHOOKSTRUCT));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.LBUTTONDOWN && wParam == (IntPtr)_mb)
                    MouseDown?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.RBUTTONDOWN && wParam == (IntPtr)_mb)
                    MouseDown?.Invoke(this, new MouseEventArgs(MouseButtons.Right, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.MBUTTONDOWN && wParam == (IntPtr)_mb)
                    MouseDown?.Invoke(this, new MouseEventArgs(MouseButtons.Middle, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.XBUTTONDOWN && wParam == (IntPtr)_mb)
                    MouseDown?.Invoke(this, new MouseEventArgs((param.mouseData & 0x10000) != 0 ? MouseButtons.XButton1 : MouseButtons.XButton2, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.LBUTTONUP && wParam == (IntPtr)_mb)
                    MouseUp?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.RBUTTONUP && wParam == (IntPtr)_mb)
                    MouseUp?.Invoke(this, new MouseEventArgs(MouseButtons.Right, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.MBUTTONUP && wParam == (IntPtr)_mb)
                    MouseUp?.Invoke(this, new MouseEventArgs(MouseButtons.Middle, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.XBUTTONUP && wParam == (IntPtr)_mb)
                    MouseUp?.Invoke(this, new MouseEventArgs((param.mouseData & 0x10000) != 0 ? MouseButtons.XButton1 : MouseButtons.XButton2, 0, param.x, param.y, 0));
                return WinAPI.Hook.CallNextHookEx(_mbHHook, code, wParam, lParam);
            }
            private IntPtr AllMBHookProc(int code, IntPtr wParam, IntPtr lParam)
            {
                tagMSLLHOOKSTRUCT param = new tagMSLLHOOKSTRUCT();
                param = (tagMSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(tagMSLLHOOKSTRUCT));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.LBUTTONDOWN)
                    MouseDown?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.RBUTTONDOWN)
                    MouseDown?.Invoke(this, new MouseEventArgs(MouseButtons.Right, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.MBUTTONDOWN)
                    MouseDown?.Invoke(this, new MouseEventArgs(MouseButtons.Middle, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.XBUTTONDOWN)
                    MouseDown?.Invoke(this, new MouseEventArgs((param.mouseData & 0x10000) != 0 ? MouseButtons.XButton1 : MouseButtons.XButton2, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.LBUTTONUP)
                    MouseUp?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.RBUTTONUP)
                    MouseUp?.Invoke(this, new MouseEventArgs(MouseButtons.Right, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.MBUTTONUP)
                    MouseUp?.Invoke(this, new MouseEventArgs(MouseButtons.Middle, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.XBUTTONUP)
                    MouseUp?.Invoke(this, new MouseEventArgs(param.mouseData == 1 ? MouseButtons.XButton1 : MouseButtons.XButton2, 0, param.x, param.y, 0));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.MOUSEWHEEL)
                    MouseWheel?.Invoke(this, new MouseEventArgs(MouseButtons.None, 0, param.x, param.y, (int)(param.mouseData)));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.MOUSEHWHEEL)
                    MouseHWheel?.Invoke(this, new MouseEventArgs(MouseButtons.None, 0, param.x, param.y, (int)(param.mouseData)));
                if (code >= 0 && wParam == (IntPtr)WinAPI.Message.WindowsMessage.MOUSEMOVE)
                    MouseMove?.Invoke(this, new MouseEventArgs(MouseButtons.None, 0, param.x, param.y, (int)(param.mouseData)));
                return WinAPI.Hook.CallNextHookEx(_mbHHook, code, wParam, lParam);
            }
        }
        public class Injector
        {
            public static void Inject(uint pid, string dllPath)
            {
                IntPtr openedProcess = WinAPI.Proc.OpenProcess(WinAPI.Proc.ProcessAccessFlags.All, false, pid);
                IntPtr kernelModule = WinAPI.Proc.GetModuleHandle("kernel32.dll");
                IntPtr loadLibratyAddr = WinAPI.Proc.GetProcAddress(kernelModule, "LoadLibraryA");

                int len = dllPath.Length;

                IntPtr argLoadLibrary = WinAPI.Proc.VirtualAllocEx(openedProcess, IntPtr.Zero, len, WinAPI.Proc.AllocationType.Reserve | WinAPI.Proc.AllocationType.Commit, WinAPI.Proc.MemoryProtection.ReadWrite);

                IntPtr writedBytesCount;

                Boolean writed = WinAPI.Proc.WriteProcessMemory(openedProcess, argLoadLibrary, System.Text.Encoding.ASCII.GetBytes(dllPath), len, out writedBytesCount);

                uint threadIdOut;
                IntPtr threadId = WinAPI.Proc.CreateRemoteThread(openedProcess, IntPtr.Zero, 0, loadLibratyAddr, argLoadLibrary, 0, out threadIdOut);

                WinAPI.Proc.CloseHandle(threadId);
            }
        }
    }
}
