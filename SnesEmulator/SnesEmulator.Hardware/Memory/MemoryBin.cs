using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SnesEmulator.Hardware.Memory
{
    /// <summary>
    /// Sous section de la mémoire
    /// </summary>
    public class MemoryBin
    {
        protected MemoryContainer container;

        public MemoryContainer Container
        {
            get { return container; }
        }

        protected int start;

        public int Start
        {
            get { return start; }
        }

        protected int length;

        public int Length
        {
            get { return length; }
        }

        public MemoryBin(MemoryContainer container, int start, int length)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            this.container = container;

            if (start < 0 || start >= container.Data.Length)
                throw new ArgumentOutOfRangeException("start");

            this.start = start;

            if (length <= 0 || length + start > container.Data.Length)
                throw new ArgumentOutOfRangeException("length");

            this.length = length;
        }

        public virtual byte ReadInt1(int position)
        {
            if (position < 0 || position >= length)
                throw new InvalidOperationException("Tentative de lecture au delà de la mémoire allouée");
            else
                return container.Data[start + position];
        }

        public virtual void WriteInt1(int position, byte value)
        {
            if (position < 0 || position >= length)
                throw new InvalidOperationException("Tentative d'écriture au delà de la mémoire allouée");
            else
                container.Data[start + position] = value;
        }

        public virtual short ReadInt2(int position)
        {
            if (position < 0 || position + 1 >= length)
                throw new InvalidOperationException("Tentative de lecture au delà de la mémoire allouée");
            else
                return (short)(
                    container.Data[start + position] |
                    (container.Data[start + position + 1] << 8));
        }

        public virtual void WriteInt2(int position, short value)
        {
            if (position < 0 || position + 1 >= length)
                throw new InvalidOperationException("Tentative d'écriture au delà de la mémoire allouée");
            else
            {
                container.Data[start + position] = (byte)(value);
                container.Data[start + position + 1] = (byte)(value >> 8);
            }
        }

        public virtual int ReadInt3(int position)
        {
            if (position < 0 || position + 2 >= length)
                throw new InvalidOperationException("Tentative de lecture au delà de la mémoire allouée");
            else
                return (
                    container.Data[start + position] |
                    (container.Data[start + position + 1] << 8) |
                    (container.Data[start + position + 2] << 16));
        }

        public virtual void WriteInt3(int position, short value)
        {
            if (position < 0 || position + 2 >= length)
                throw new InvalidOperationException("Tentative d'écriture au delà de la mémoire allouée");
            else
            {
                container.Data[start + position] = (byte)(value);
                container.Data[start + position + 1] = (byte)(value >> 8);
                container.Data[start + position + 2] = (byte)(value >> 16);
            }
        }

        public virtual int Read(int position, [In,Out] byte[] data, int offset, int count)
        {
            int readable = Math.Max(0, length - position);
            count = Math.Min(readable, count);

            Buffer.BlockCopy(container.Data, start + position, data, offset, count);

            return count;
        }

        public virtual void Write(int position, [In] byte[] data, int offset, int count)
        {
            int writable = Math.Max(0, length - position);

            if (writable < count)
                throw new InvalidOperationException("Tentative d'écriture au delà de la mémoire allouée");
            else
            {
                Buffer.BlockCopy(data, offset, container.Data, start + position, count);
            }
        }
    }
}
