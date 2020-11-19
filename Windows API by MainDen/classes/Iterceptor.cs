// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Windows_API_by_MainDen
{
    public static class Interceptor
    {
        public delegate void KeyboardHookerEventHandler(object sender, KeyState state);
        public class KeyState : ICloneable
        {
            private static bool _Simple = false;
            public static bool Simple { get => _Simple; set => _Simple = value; }
            private WinAPI.Keyboard.VirtualKeyStates _Key;
            private bool _Pressed;
            private bool _Hold;
            private bool _LWin;
            private bool _RWin;
            private bool _LShiftKey;
            private bool _RShiftKey;
            private bool _LControlKey;
            private bool _RControlKey;
            private bool _LMenu;
            private bool _RMenu;
            public WinAPI.Keyboard.VirtualKeyStates Key { get => _Key; }
            public bool Pressed { get => _Pressed; }
            public bool Hold { get => _Hold; }
            public bool LWin { get => _LWin; }
            public bool RWin { get => _RWin; }
            public bool Win { get => _LWin || _RWin; }
            public bool LShiftKey { get => _LShiftKey; }
            public bool RShiftKey { get => _RShiftKey; }
            public bool ShiftKey { get => _LShiftKey || _RShiftKey; }
            public bool LControlKey { get => _LControlKey; }
            public bool RControlKey { get => _RControlKey; }
            public bool ControlKey { get => _LControlKey || _RControlKey; }
            public bool LMenu { get => _LMenu; }
            public bool RMenu { get => _RMenu; }
            public bool Menu { get => _LMenu || _RMenu; }
            public static KeyState Empty
            {
                get
                {
                    return new KeyState(WinAPI.Keyboard.VirtualKeyStates.None, true);
                }
            }
            private void UpdateHold()
            {
                if (_Pressed)
                    _Hold = (WinAPI.Keyboard.GetAsyncKeyState(_Key) & 0x8000) != 0;
            }
            private void UpdateModifiers()
            {
                if (_Key != WinAPI.Keyboard.VirtualKeyStates.LWin)
                    _LWin = (WinAPI.Keyboard.GetAsyncKeyState(WinAPI.Keyboard.VirtualKeyStates.LWin) & 0x8000) != 0;
                if (_Key != WinAPI.Keyboard.VirtualKeyStates.RWin)
                    _RWin = (WinAPI.Keyboard.GetAsyncKeyState(WinAPI.Keyboard.VirtualKeyStates.RWin) & 0x8000) != 0;
                if (_Key != WinAPI.Keyboard.VirtualKeyStates.LShiftKey)
                    _LShiftKey = (WinAPI.Keyboard.GetAsyncKeyState(WinAPI.Keyboard.VirtualKeyStates.LShiftKey) & 0x8000) != 0;
                if (_Key != WinAPI.Keyboard.VirtualKeyStates.RShiftKey)
                    _RShiftKey = (WinAPI.Keyboard.GetAsyncKeyState(WinAPI.Keyboard.VirtualKeyStates.RShiftKey) & 0x8000) != 0;
                if (_Key != WinAPI.Keyboard.VirtualKeyStates.LControlKey)
                    _LControlKey = (WinAPI.Keyboard.GetAsyncKeyState(WinAPI.Keyboard.VirtualKeyStates.LControlKey) & 0x8000) != 0;
                if (_Key != WinAPI.Keyboard.VirtualKeyStates.RControlKey)
                    _RControlKey = (WinAPI.Keyboard.GetAsyncKeyState(WinAPI.Keyboard.VirtualKeyStates.RControlKey) & 0x8000) != 0;
                if (_Key != WinAPI.Keyboard.VirtualKeyStates.LMenu)
                    _LMenu = (WinAPI.Keyboard.GetAsyncKeyState(WinAPI.Keyboard.VirtualKeyStates.LMenu) & 0x8000) != 0;
                if (_Key != WinAPI.Keyboard.VirtualKeyStates.RMenu)
                    _RMenu = (WinAPI.Keyboard.GetAsyncKeyState(WinAPI.Keyboard.VirtualKeyStates.RMenu) & 0x8000) != 0;
            }
            public void Update()
            {
                UpdateHold();
                UpdateModifiers();
            }
            public void Set(KeyState state)
            {
                if (state is null)
                    throw new ArgumentNullException(nameof(state));
                _Key = state._Key;
                _Pressed = state._Pressed;
                _Hold = state._Hold;
                _LWin = state._LWin;
                _RWin = state._RWin;
                _LShiftKey = state._LShiftKey;
                _RShiftKey = state._RShiftKey;
                _LControlKey = state._LControlKey;
                _RControlKey = state._RControlKey;
                _LMenu = state._LMenu;
                _RMenu = state._RMenu;
            }
            public object Clone()
            {
                return new KeyState(this);
            }
            public override bool Equals(object o)
            {
                if (this == o)
                    return true;
                KeyState state = o as KeyState;
                return Equals(state);
            }
            public bool Equals(KeyState state)
            {
                if (this == state)
                    return true;
                if (state is null || _Key != state._Key || _Pressed != state._Pressed || _Hold != state._Hold)
                    return false;
                if (_Simple && (Win != state.Win || ShiftKey != state.ShiftKey || ControlKey != state.ControlKey || Menu != state.Menu))
                    return false;
                else if (!_Simple && (_LWin != state._LWin || _RWin != state._RWin || _LShiftKey != state._LShiftKey || _RShiftKey != state._RShiftKey ||
                    _LControlKey != state._LControlKey || _RControlKey != state._RControlKey || _LMenu != state._LMenu || _RMenu != state._RMenu))
                    return false;
                return true;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            public override string ToString()
            {
                string res = "";
                if (_Simple)
                {
                    if (_Key != WinAPI.Keyboard.VirtualKeyStates.LWin && _Key != WinAPI.Keyboard.VirtualKeyStates.RWin && Win)
                        res += "Win + ";
                    if (_Key != WinAPI.Keyboard.VirtualKeyStates.LShiftKey && _Key != WinAPI.Keyboard.VirtualKeyStates.RShiftKey && ShiftKey)
                        res += "ShiftKey + ";
                    if (_Key != WinAPI.Keyboard.VirtualKeyStates.LControlKey && _Key != WinAPI.Keyboard.VirtualKeyStates.RControlKey && ControlKey)
                        res += "ControlKey + ";
                    if (_Key != WinAPI.Keyboard.VirtualKeyStates.LMenu && _Key != WinAPI.Keyboard.VirtualKeyStates.RMenu && Menu)
                        res += "Menu + ";
                }
                else
                {
                    if (_Key != WinAPI.Keyboard.VirtualKeyStates.LWin && _LWin)
                        res += "LWin + ";
                    if (_Key != WinAPI.Keyboard.VirtualKeyStates.RWin && _RWin)
                        res += "RWin + ";
                    if (_Key != WinAPI.Keyboard.VirtualKeyStates.LShiftKey && _LShiftKey)
                        res += "LShiftKey + ";
                    if (_Key != WinAPI.Keyboard.VirtualKeyStates.RShiftKey && _RShiftKey)
                        res += "RShiftKey + ";
                    if (_Key != WinAPI.Keyboard.VirtualKeyStates.LControlKey && _LControlKey)
                        res += "LControlKey + ";
                    if (_Key != WinAPI.Keyboard.VirtualKeyStates.RControlKey && _RControlKey)
                        res += "RControlKey + ";
                    if (_Key != WinAPI.Keyboard.VirtualKeyStates.LMenu && _LMenu)
                        res += "LMenu + ";
                    if (_Key != WinAPI.Keyboard.VirtualKeyStates.RMenu && _RMenu)
                        res += "RMenu + ";
                }
                res += _Key.ToString();
                if (_Hold)
                    res += " [Hold]";
                else if (_Pressed)
                    res += " [Down]";
                else
                    res += " [Up]";
                return res;
            }
            public string ToString(string format)
            {
                if (format is null)
                    throw new ArgumentNullException(nameof(format));
                string FORMAT = format.ToUpper();
                switch (FORMAT)
                {
                    case "KEY":
                    case "KEY []":
                    case "MOD + KEY":
                    case "MOD + KEY []":
                        break;
                    default:
                        return ToString();
                }
                string res = "";
                if (FORMAT == "MOD + KEY" || FORMAT == "MOD + KEY []")
                    if (_Simple)
                    {
                        if (_Key != WinAPI.Keyboard.VirtualKeyStates.LWin && _Key != WinAPI.Keyboard.VirtualKeyStates.RWin && Win)
                            res += "Win + ";
                        if (_Key != WinAPI.Keyboard.VirtualKeyStates.LShiftKey && _Key != WinAPI.Keyboard.VirtualKeyStates.RShiftKey && ShiftKey)
                            res += "ShiftKey + ";
                        if (_Key != WinAPI.Keyboard.VirtualKeyStates.LControlKey && _Key != WinAPI.Keyboard.VirtualKeyStates.RControlKey && ControlKey)
                            res += "ControlKey + ";
                        if (_Key != WinAPI.Keyboard.VirtualKeyStates.LMenu && _Key != WinAPI.Keyboard.VirtualKeyStates.RMenu && Menu)
                            res += "Menu + ";
                    }
                    else
                    {
                        if (_Key != WinAPI.Keyboard.VirtualKeyStates.LWin && _LWin)
                            res += "LWin + ";
                        if (_Key != WinAPI.Keyboard.VirtualKeyStates.RWin && _RWin)
                            res += "RWin + ";
                        if (_Key != WinAPI.Keyboard.VirtualKeyStates.LShiftKey && _LShiftKey)
                            res += "LShiftKey + ";
                        if (_Key != WinAPI.Keyboard.VirtualKeyStates.RShiftKey && _RShiftKey)
                            res += "RShiftKey + ";
                        if (_Key != WinAPI.Keyboard.VirtualKeyStates.LControlKey && _LControlKey)
                            res += "LControlKey + ";
                        if (_Key != WinAPI.Keyboard.VirtualKeyStates.RControlKey && _RControlKey)
                            res += "RControlKey + ";
                        if (_Key != WinAPI.Keyboard.VirtualKeyStates.LMenu && _LMenu)
                            res += "LMenu + ";
                        if (_Key != WinAPI.Keyboard.VirtualKeyStates.RMenu && _RMenu)
                            res += "RMenu + ";
                    }
                res += _Key.ToString();
                if (FORMAT == "MOD + KEY []" || FORMAT == "KEY []")
                    if (_Hold)
                        res += " [Hold]";
                    else if (_Pressed)
                        res += " [Down]";
                    else
                        res += " [Up]";
                return res;
            }
            public KeyState(WinAPI.Keyboard.VirtualKeyStates key, bool pressed)
            {
                _Key = key;
                _Pressed = pressed;
                _Hold = false;
                _LWin = false;
                _RWin = false;
                _LShiftKey = false;
                _RShiftKey = false;
                _LControlKey = false;
                _RControlKey = false;
                _LMenu = false;
                _RMenu = false;
            }
            public KeyState(KeyState state)
            {
                if (state is null)
                    throw new ArgumentNullException(nameof(state));
                _Key = state._Key;
                _Pressed = state._Pressed;
                _Hold = state._Hold;
                _LWin = state._LWin;
                _RWin = state._RWin;
                _LShiftKey = state._LShiftKey;
                _RShiftKey = state._RShiftKey;
                _LControlKey = state._LControlKey;
                _RControlKey = state._RControlKey;
                _LMenu = state._LMenu;
                _RMenu = state._RMenu;
            }
            public static KeyState GetKeyState(string str)
            {
                if (str is null)
                    throw new ArgumentNullException(nameof(str));
                string[] strs = str.Split(new[] { ' ', '+' }, StringSplitOptions.RemoveEmptyEntries);
                KeyState k = Empty;
                if (strs.Length < 2)
                    return k;
                for (int i = 0; i < strs.Length - 2; i++)
                {
                    switch (strs[i])
                    {
                        case "LWin":
                            k._LWin = true;
                            break;
                        case "RWin":
                            k._RWin = true;
                            break;
                        case "LShiftKey":
                            k._LShiftKey = true;
                            break;
                        case "RShiftKey":
                            k._RShiftKey = true;
                            break;
                        case "LControlKey":
                            k._LControlKey = true;
                            break;
                        case "RControlKey":
                            k._RControlKey = true;
                            break;
                        case "LMenu":
                            k._LMenu = true;
                            break;
                        case "RMenu":
                            k._RMenu = true;
                            break;
                    }
                }
                if (!Enum.TryParse(strs[strs.Length - 2], out k._Key))
                    k._Key = WinAPI.Keyboard.VirtualKeyStates.None;
                switch (strs[strs.Length - 1])
                {
                    case "[Hold]":
                        k._Hold = true;
                        k._Pressed = true;
                        break;
                    case "[Up]":
                        k._Hold = false;
                        k._Pressed = false;
                        break;
                    default:
                        k._Hold = false;
                        k._Pressed = true;
                        break;
                }
                return k;
            }
        }
        public class KeyboardHooker : IDisposable
        {
            private int _key;
            public event KeyboardHookerEventHandler KeyDown;
            public event KeyboardHookerEventHandler KeyUp;
            public event KeyboardHookerEventHandler Key;
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
                {
                    KeyState ks = new KeyState((WinAPI.Keyboard.VirtualKeyStates)key, true);
                    ks.Update();
                    KeyDown?.Invoke(this, ks);
                    Key?.Invoke(this, ks);
                }
                if (code >= 0 && (wParam == (IntPtr)WinAPI.Message.WindowsMessage.KEYUP || wParam == (IntPtr)WinAPI.Message.WindowsMessage.SYSKEYUP) && key == _key)
                {
                    KeyState ks = new KeyState((WinAPI.Keyboard.VirtualKeyStates)key, false);
                    ks.Update();
                    KeyUp?.Invoke(this, ks);
                    Key?.Invoke(this, ks);
                }
                return WinAPI.Hook.CallNextHookEx(_keyHHook, code, wParam, lParam);
            }
            private IntPtr AllKeyHookProc(int code, IntPtr wParam, IntPtr lParam)
            {
                int key = Marshal.ReadInt32(lParam);
                if (code >= 0 && (wParam == (IntPtr)WinAPI.Message.WindowsMessage.KEYDOWN || wParam == (IntPtr)WinAPI.Message.WindowsMessage.SYSKEYDOWN))
                {
                    KeyState ks = new KeyState((WinAPI.Keyboard.VirtualKeyStates)key, true);
                    ks.Update();
                    KeyDown?.Invoke(this, ks);
                    Key?.Invoke(this, ks);
                }
                if (code >= 0 && (wParam == (IntPtr)WinAPI.Message.WindowsMessage.KEYUP || wParam == (IntPtr)WinAPI.Message.WindowsMessage.SYSKEYUP))
                {
                    KeyState ks = new KeyState((WinAPI.Keyboard.VirtualKeyStates)key, false);
                    ks.Update();
                    KeyUp?.Invoke(this, ks);
                    Key?.Invoke(this, ks);
                }
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