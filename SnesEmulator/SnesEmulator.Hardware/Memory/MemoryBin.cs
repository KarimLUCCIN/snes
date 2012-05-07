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
        private MemoryContainer container;

        public MemoryContainer Container
        {
            get { return container; }
        }

        private int start;

        public int Start
        {
            get { return start; }
        }

        private int length;

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

        public int ReadByte(int position)
        {
            if (position < 0 || position >= length)
                return -1;
            else
                return container.Data[start + position];
        }

        public void WriteByte(int position, byte value)
        {
            if (position < 0 || position >= length)
                throw new InvalidOperationException("Tentative d'écriture au delà de la mémoire allouée");
            else
                container.Data[start + position] = value;
        }

        public int Read(int position, [In,Out] byte[] data, int offset, int count)
        {
            int readable = Math.Max(0, length - position);
            count = Math.Min(readable, count);

            Buffer.BlockCopy(container.Data, start + position, data, offset, count);

            return count;
        }

        public void Write(int position, [In] byte[] data, int offset, int count)
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
