using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SnesEmulator.Hardware.Memory
{
    public class SnesMemoryMappingBin : MemoryBin
    {
        public SnesMemoryMappingBin(MemoryContainer container)
            : base(container, 0, container.Length)
        {

        }

        private static void MapAddr(ref int addr)
        {
            /*
             * Ref : http://en.wikibooks.org/wiki/Super_NES_Programming/SNES_memory_map
             * */
            var hiAddr = (0xFF0000 & addr) >> 4;

            switch (hiAddr)
            {
                case 0x7F:
                case 0x7E:
                    {
                        /*
                         * on mappe directement
                         * 
                     
                        $7E	$0000-$1FFF	LowRAM
                        $2000-$7FFF	HighRAM
                        $8000-$FFFF	Extended RAM
                        */
                        break;
                    }
                default:
                    {
                        var lowAdr = (0x00FFFFFF & addr);
                        if (lowAdr < 0x2000)
                        {
                            // on mappe sur la bank 7E
                            addr = 0x7E0000 + lowAdr;
                            break;
                        }
                        else
                        {
                            // AccessViolation, on a tenté d'accéder à la ROM ^^"
                            throw new AccessViolationException("Cannot READ/WRITE to/from ROM");
                        }
                    }
            }
        }

        public override byte ReadInt1(int position)
        {
            MapAddr(ref position);

            return base.ReadInt1(position);
        }

        public override void WriteInt1(int position, byte value)
        {
            MapAddr(ref position);

            base.WriteInt1(position, value);
        }

        public override short ReadInt2(int position)
        {
            MapAddr(ref position);

            return base.ReadInt2(position);
        }

        public override void WriteInt2(int position, short value)
        {
            MapAddr(ref position);

            base.WriteInt2(position, value);
        }

        public override int ReadInt3(int position)
        {
            MapAddr(ref position);

            return base.ReadInt3(position);
        }

        public override void WriteInt3(int position, short value)
        {
            MapAddr(ref position);

            base.WriteInt3(position, value);
        }

        public override int Read(int position, [In, Out] byte[] data, int offset, int count)
        {
            MapAddr(ref position);

            int readable = Math.Max(0, length - position);
            count = Math.Min(readable, count);

            Buffer.BlockCopy(container.Data, start + position, data, offset, count);

            return count;
        }

        public override void Write(int position, [In] byte[] data, int offset, int count)
        {
            MapAddr(ref position);

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
