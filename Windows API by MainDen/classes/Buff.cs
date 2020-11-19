// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

using System;

namespace Windows_API_by_MainDen
{
    public sealed class Buff
    {
        public int Length { get; private set; } = 0;
        private byte[] buff;
        public static App app;

        public Buff()
        {
            Length = 0;
            buff = null;
        }
        public Buff(int Length)
        {
            if (Length > 0)
            {
                this.Length = Length;
                buff = new byte[Length];
            }
            else
            {
                this.Length = 0;
                buff = null;
            }
        }
        public Buff(Buff buff)
        {
            Length = buff.Length;
            this.buff = new byte[Length];
            for (int i = 0; i < Length; i++)
                this.buff[i] = buff.buff[i];
        }
        public Buff(Buff buff, int pos)
        {
            int count = buff.Length - pos;
            if (pos < 0 || count <= 0)
            {
                Length = 0;
                this.buff = null;
            }
            else
            {
                Length = count;
                this.buff = new byte[Length];
                for (int i = 0; i < Length; i++)
                    this.buff[i] = buff.buff[i + pos];
            }
        }
        public Buff(Buff buff, int pos, int count)
        {
            if (pos < 0 || count <= 0 || buff.Length < pos + count)
            {
                Length = 0;
                this.buff = null;
            }
            else
            {
                Length = count;
                this.buff = new byte[Length];
                for (int i = 0; i < Length; i++)
                    this.buff[i] = buff.buff[i + pos];
            }
        }
        public Buff(byte[] buff)
        {
            Length = buff.Length;
            this.buff = new byte[Length];
            for (int i = 0; i < Length; i++)
                this.buff[i] = buff[i];
        }
        public Buff(byte[] buff, int pos)
        {
            int count = buff.Length - pos;
            if (pos < 0 || count <= 0)
            {
                Length = 0;
                this.buff = null;
            }
            else
            {
                Length = count;
                this.buff = new byte[Length];
                for (int i = 0; i < Length; i++)
                    this.buff[i] = buff[i + pos];
            }
        }
        public Buff(byte[] buff, int pos, int count)
        {
            if (pos < 0 || count <= 0)
            {
                Length = 0;
                this.buff = null;
            }
            else
            {
                Length = count;
                this.buff = new byte[Length];
                for (int i = 0; i < Length; i++)
                    this.buff[i] = buff[i + pos];
            }
        }
        public bool Equals(Buff buff)
        {
            if (Length == buff.Length)
            {
                for (int i = 0; i < Length; i++)
                    if (this.buff[i] != buff.buff[i])
                        return false;
                return true;
            }
            else
                return false;
        }
        public override bool Equals(object obj)
        {
            if (obj is Buff)
                return Equals((Buff)obj);
            return false;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            if (Length == 0)
                return "null";
            string res = "";
            int i;
            for (i = 0; i < Length - 1; i += 2)
                res = ((buff[i + 1] << 8) + buff[i]).ToString("X4") + res;
            if (i == Length - 1)
                res = buff[i].ToString("X2") + res;
            return res;
        }
        public bool IsNull()
        {
            if (buff == null)
                return true;
            return false;
        }
        public bool IsEmpty()
        {
            if (Length != 0)
                for (int i = 0; i < Length; i++)
                    if (buff[i] != 0)
                        return false;
            return true;
        }
        public static Buff ReadProcessMemory(IntPtr start, int count)
        {
            IntPtr In = IntPtr.Zero;
            Buff memory = new Buff(count);
            if (count > 0)
                if (app.Exist())
                    WinAPI.Proc.ReadProcessMemory(app.ProcHandle(), start, memory.buff, count, In);
            return memory;
        }
        public static Buff ReadProcessMemory(App app, IntPtr start, int count)
        {
            IntPtr In = IntPtr.Zero;
            Buff memory = new Buff(count);
            if (count > 0)
                if (app.Exist())
                    WinAPI.Proc.ReadProcessMemory(app.ProcHandle(), start, memory.buff, count, In);
            return memory;
        }
        public static void WriteProcessMemory(IntPtr start, Buff memory)
        {
            IntPtr Out;
            if (app.Exist() && !memory.IsNull())
                WinAPI.Proc.WriteProcessMemory(app.ProcHandle(), start, memory.buff, memory.Length, out Out);
        }
        public static void WriteProcessMemory(App app, IntPtr start, Buff memory)
        {
            IntPtr Out;
            if (app.Exist() && !memory.IsNull())
                WinAPI.Proc.WriteProcessMemory(app.ProcHandle(), start, memory.buff, memory.Length, out Out);
        }
        public static void WriteProcessMemory(IntPtr start, Buff memory, int count)
        {
            IntPtr Out;
            if (app.Exist() && !memory.IsNull())
                WinAPI.Proc.WriteProcessMemory(app.ProcHandle(), start, memory.buff, count, out Out);
        }
        public static void WriteProcessMemory(App app, IntPtr start, Buff memory, int count)
        {
            IntPtr Out;
            if (app.Exist() && !memory.IsNull())
                WinAPI.Proc.WriteProcessMemory(app.ProcHandle(), start, memory.buff, count, out Out);
        }
        public Buff Reverce()
        {
            for (int i = 0; i < Length / 2; i++)
            {
                byte temp = buff[i];
                buff[i] = buff[Length - 1 - i];
                buff[Length - 1 - i] = temp;
            }
            return this;
        }
        public Buff Copy(int pos, int count)
        {
            int lCount = Length - pos;
            if (pos < 0 || count < 0 || lCount <= 0)
                return new Buff();
            if (lCount > count && count > 0)
                lCount = count;
            return new Buff(this, pos, lCount);
        }
        public Buff Read(IntPtr start)
        {
            IntPtr In = IntPtr.Zero;
            buff = new byte[Length];
            if (app.Exist())
                WinAPI.Proc.ReadProcessMemory(app.ProcHandle(), start, buff, Length, In);
            return this;
        }
        public Buff Read(App app, IntPtr start)
        {
            IntPtr In = IntPtr.Zero;
            buff = new byte[Length];
            if (app.Exist())
                WinAPI.Proc.ReadProcessMemory(app.ProcHandle(), start, buff, Length, In);
            return this;
        }
        public Buff Read(IntPtr start, int count)
        {
            IntPtr In = IntPtr.Zero;
            Length = count;
            buff = new byte[Length];
            if (app.Exist())
                WinAPI.Proc.ReadProcessMemory(app.ProcHandle(), start, buff, count, In);
            return this;
        }
        public Buff Read(App app, IntPtr start, int count)
        {
            IntPtr In = IntPtr.Zero;
            Length = count;
            buff = new byte[Length];
            if (app.Exist())
                WinAPI.Proc.ReadProcessMemory(app.ProcHandle(), start, buff, count, In);
            return this;
        }
        public Buff Write(IntPtr start)
        {
            IntPtr Out = new IntPtr();
            if (app.Exist())
                WinAPI.Proc.WriteProcessMemory(app.ProcHandle(), start, buff, Length, out Out);
            return this;
        }
        public Buff Write(App app, IntPtr start)
        {
            IntPtr Out = new IntPtr();
            if (app.Exist())
                WinAPI.Proc.WriteProcessMemory(app.ProcHandle(), start, buff, Length, out Out);
            return this;
        }
        public Buff Write(IntPtr start, int count)
        {
            IntPtr Out = new IntPtr();
            if (app.Exist())
                WinAPI.Proc.WriteProcessMemory(app.ProcHandle(), start, buff, count, out Out);
            return this;
        }
        public Buff Write(App app, IntPtr start, int count)
        {
            IntPtr Out = new IntPtr();
            if (app.Exist())
                WinAPI.Proc.WriteProcessMemory(app.ProcHandle(), start, buff, count, out Out);
            return this;
        }
        public Buff Next(int count)
        {
            if (Length == 4)
            {
                IntPtr start = this;
                Length = count;
                buff = new byte[Length];
                IntPtr In = System.IntPtr.Zero;
                if (app.Exist())
                    WinAPI.Proc.ReadProcessMemory(app.ProcHandle(), start, buff, count, In);
            }
            else
            {
                Length = 0;
                buff = null;
            }
            return this;
        }
        public Buff Next(App app, int count)
        {
            if (Length == 4)
            {
                IntPtr start = this;
                Length = count;
                buff = new byte[Length];
                IntPtr In = System.IntPtr.Zero;
                if (app.Exist())
                    WinAPI.Proc.ReadProcessMemory(app.ProcHandle(), start, buff, count, In);
            }
            else
            {
                Length = 0;
                buff = null;
            }
            return this;
        }
        public Buff Next(int pos, int count)
        {
            if (Length == 4)
            {
                IntPtr start = this;
                start += pos;
                Length = count;
                buff = new byte[Length];
                IntPtr In = System.IntPtr.Zero;
                if (app.Exist())
                    WinAPI.Proc.ReadProcessMemory(app.ProcHandle(), start, buff, count, In);
            }
            else
            {
                Length = 0;
                buff = null;
            }
            return this;
        }
        public Buff Next(App app, int pos, int count)
        {
            if (Length == 4)
            {
                IntPtr start = this;
                start += pos;
                Length = count;
                buff = new byte[Length];
                IntPtr In = System.IntPtr.Zero;
                if (app.Exist())
                    WinAPI.Proc.ReadProcessMemory(app.ProcHandle(), start, buff, count, In);
            }
            else
            {
                Length = 0;
                buff = null;
            }
            return this;
        }
        public Buff Append(Buff buff)
        {
            if (buff != null)
            {
                byte[] temp = new byte[this.Length];
                for (int i = 0; i < temp.Length; i++)
                    temp[i] = this.buff[i];
                this.Length = temp.Length + buff.Length;
                this.buff = new byte[this.Length];
                for (int i = 0; i < temp.Length; i++)
                    this.buff[i] = temp[i];
                for (int i = 0; i < buff.Length; i++)
                    this.buff[temp.Length + i] = buff.buff[i];
            }
            return this;
        }
        // Implicit convertation from Buff
        public static implicit operator IntPtr(Buff buff)
        {
            return (IntPtr)BitConverter.ToInt32(buff.buff, 0);
        }
        public static implicit operator Int64(Buff buff)
        {
            return BitConverter.ToInt64(buff.buff, 0);
        }
        public static implicit operator Int32(Buff buff)
        {
            return BitConverter.ToInt32(buff.buff, 0);
        }
        public static implicit operator Int16(Buff buff)
        {
            return BitConverter.ToInt16(buff.buff, 0);
        }
        public static implicit operator UInt64(Buff buff)
        {
            return BitConverter.ToUInt64(buff.buff, 0);
        }
        public static implicit operator UInt32(Buff buff)
        {
            return BitConverter.ToUInt32(buff.buff, 0);
        }
        public static implicit operator UInt16(Buff buff)
        {
            return BitConverter.ToUInt16(buff.buff, 0);
        }
        public static implicit operator Double(Buff buff)
        {
            return BitConverter.ToDouble(buff.buff, 0);
        }
        public static implicit operator Single(Buff buff)
        {
            return BitConverter.ToSingle(buff.buff, 0);
        }
        public static implicit operator Byte(Buff buff)
        {
            return buff.buff[0];
        }
        public static implicit operator SByte(Buff buff)
        {
            return (SByte)buff.buff[0];
        }
        public static implicit operator Char(Buff buff)
        {
            return BitConverter.ToChar(buff.buff, 0);
        }
        public static implicit operator Boolean(Buff buff)
        {
            return BitConverter.ToBoolean(buff.buff, 0);
        }
        public static implicit operator Byte[](Buff buff)
        {
            byte[] temp = new byte[buff.Length];
            for (int i = 0; i < buff.Length; i++)
                temp[i] = buff.buff[i];
            return temp;
        }
        // Explicit convertation to Buff
        public static explicit operator Buff(IntPtr setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes((Int32)setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static explicit operator Buff(Int64 setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static explicit operator Buff(Int32 setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static explicit operator Buff(Int16 setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static explicit operator Buff(UInt64 setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static explicit operator Buff(UInt32 setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static explicit operator Buff(UInt16 setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static explicit operator Buff(Double setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static explicit operator Buff(Single setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static explicit operator Buff(Byte setup)
        {
            Buff buff = new Buff();
            buff.buff = new byte[1];
            buff.buff[0] = setup;
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static explicit operator Buff(SByte setup)
        {
            Buff buff = new Buff();
            buff.buff = new byte[1];
            buff.buff[0] = (byte)setup;
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static explicit operator Buff(Char setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static explicit operator Buff(Boolean setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static explicit operator Buff(Byte[] setup)
        {
            Buff buff = new Buff(setup.Length);
            for (int i = 0; i < buff.Length; i++)
                buff.buff[i] = setup[i];
            return buff;
        }
        // Static creation a Buff
        public static Buff SetBuff(Buff setup)
        {
            return new Buff(setup);
        }
        public static Buff SetBuff(IntPtr setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes((Int32)setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static Buff SetBuff(Int64 setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static Buff SetBuff(Int32 setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static Buff SetBuff(Int16 setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static Buff SetBuff(UInt64 setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static Buff SetBuff(UInt32 setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static Buff SetBuff(UInt16 setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static Buff SetBuff(Double setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static Buff SetBuff(Single setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static Buff SetBuff(Byte setup)
        {
            Buff buff = new Buff();
            buff.buff = new byte[1];
            buff.buff[0] = setup;
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static Buff SetBuff(SByte setup)
        {
            Buff buff = new Buff();
            buff.buff = new byte[1];
            buff.buff[0] = (byte)setup;
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static Buff SetBuff(Char setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static Buff SetBuff(Boolean setup)
        {
            Buff buff = new Buff();
            buff.buff = BitConverter.GetBytes(setup);
            buff.Length = buff.buff.Length;
            return buff;
        }
        public static Buff SetBuff(Byte[] setup)
        {
            Buff buff = new Buff(setup.Length);
            for (int i = 0; i < buff.Length; i++)
                buff.buff[i] = setup[i];
            return buff;
        }
        public static Buff SetBuff(String setup, Int32 size = 16)
        {
            int bytes;
            Buff buff = new Buff();
            switch (size)
            {
                case 32:
                case 16:
                case 8:
                    break;
                default:
                    size = 16;
                    break;
            }
            bytes = size / 8;
            buff.Length = setup.Length * bytes;
            buff.buff = new byte[buff.Length];
            for (int i = 0; i < setup.Length; i++)
                for (int j = 0; j < bytes; j++)
                    buff.buff[i * bytes + j] = (byte)((setup[i] >> (8 * (bytes - 1 - j))) & 0xFF);
            return buff;
        }
        public static String String(Buff buff, Int32 size = 16)
        {
            int bytes;
            String res = "";
            int length;
            switch (size)
            {
                case 32:
                case 16:
                case 8:
                    break;
                default:
                    size = 16;
                    break;
            }
            bytes = size / 8;
            length = buff.Length / bytes;
            for (int i = 0; i < length; i++)
            {
                Int64 temp = 0;
                Int64 mask = 0;
                for (int j = 0; j < bytes; j++)
                {
                    temp = (temp << 8) + buff.buff[i * bytes + j];
                    mask = (mask << 8) + 0xFF;
                }
                res += (char)(temp & mask);
            }
            return res;
        }
        // Dinamic setup the Buff
        public Buff Setup(Buff setup)
        {
            Length = setup.Length;
            buff = new byte[Length];
            for (int i = 0; i < Length; i++)
                buff[i] = setup.buff[i];
            return this;
        }
        public Buff Setup(IntPtr setup)
        {
            buff = BitConverter.GetBytes((Int32)setup);
            Length = buff.Length;
            return this;
        }
        public Buff Setup(Int64 setup)
        {
            buff = BitConverter.GetBytes(setup);
            Length = buff.Length;
            return this;
        }
        public Buff Setup(Int32 setup)
        {
            buff = BitConverter.GetBytes(setup);
            Length = buff.Length;
            return this;
        }
        public Buff Setup(Int16 setup)
        {
            buff = BitConverter.GetBytes(setup);
            Length = buff.Length;
            return this;
        }
        public Buff Setup(UInt64 setup)
        {
            buff = BitConverter.GetBytes(setup);
            Length = buff.Length;
            return this;
        }
        public Buff Setup(UInt32 setup)
        {
            buff = BitConverter.GetBytes(setup);
            Length = buff.Length;
            return this;
        }
        public Buff Setup(UInt16 setup)
        {
            buff = BitConverter.GetBytes(setup);
            Length = buff.Length;
            return this;
        }
        public Buff Setup(Double setup)
        {
            buff = BitConverter.GetBytes(setup);
            Length = buff.Length;
            return this;
        }
        public Buff Setup(Single setup)
        {
            buff = BitConverter.GetBytes(setup);
            Length = buff.Length;
            return this;
        }
        public Buff Setup(Byte setup)
        {
            buff = new byte[1];
            buff[0] = setup;
            Length = buff.Length;
            return this;
        }
        public Buff Setup(SByte setup)
        {
            buff = new byte[1];
            buff[0] = (byte)setup;
            Length = buff.Length;
            return this;
        }
        public Buff Setup(Char setup)
        {
            buff = BitConverter.GetBytes(setup);
            Length = buff.Length;
            return this;
        }
        public Buff Setup(Boolean setup)
        {
            buff = BitConverter.GetBytes(setup);
            Length = buff.Length;
            return this;
        }
        public Buff Setup(Byte[] setup)
        {
            Length = setup.Length;
            for (int i = 0; i < Length; i++)
                buff[i] = setup[i];
            return this;
        }
        public Buff Setup(String setup, Int32 size = 16)
        {
            int bytes;
            switch (size)
            {
                case 32:
                case 16:
                case 8:
                    break;
                default:
                    size = 16;
                    break;
            }
            bytes = size / 8;
            Length = setup.Length * bytes;
            buff = new byte[Length];
            for (int i = 0; i < setup.Length; i++)
                for (int j = 0; j < bytes; j++)
                    buff[i * bytes + j] = (byte)((setup[i] >> (8 * (bytes - 1 - j))) & 0xFF);
            return this;
        }
        public String String(Int32 size = 16)
        {
            int bytes;
            String res = "";
            int length;
            switch (size)
            {
                case 32:
                case 16:
                case 8:
                    break;
                default:
                    size = 16;
                    break;
            }
            bytes = size / 8;
            length = Length / bytes;
            for (int i = 0; i < length; i++)
            {
                Int64 temp = 0;
                Int64 mask = 0;
                for (int j = 0; j < bytes; j++)
                {
                    temp = (temp << 8) + buff[i * bytes + j];
                    mask = (mask << 8) + 0xFF;
                }
                res += (char)(temp & mask);
            }
            return res;
        }
    }
}