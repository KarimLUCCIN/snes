using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnesEmulator.Hardware.Memory;
using System.IO;

namespace SnesEmulator.RomReader
{
    public static class Loader
    {
        /// <summary>
        /// Charge une ROM en mémoire.
        /// </summary>
        /// <param name="romStream"></param>
        /// <param name="baseOffset">Début du stream à considérer</param>
        /// <param name="headerOffset">A partir du début du stream, endroit où se trouve le header</param>
        /// <param name="container"></param>
        /// <param name="containerOffset"></param>
        /// <returns></returns>
        public static MemoryBin LoadInto(Stream romStream, int baseOffset, int headerOffset, MemoryContainer container, int containerOffset)
        {
            int romSize, ramSize;

            GetROMParameters(romStream, baseOffset, headerOffset, out romSize, out ramSize);


            if (romSize > container.Length - containerOffset)
                throw new OutOfMemoryException(String.Format("Container est trop petit. \nDisponible : {0}.\nRequis : {1}",
                    container.Length - containerOffset,
                    romSize));

            romStream.Position = baseOffset;
            romStream.Read(container.Data, containerOffset, romSize);

            return new MemoryBin(container, containerOffset, romSize);
        }

        public static void GetROMParameters(Stream romStream, int baseOffset, int headerOffset, out int romSize, out int ramSize)
        {
            romStream.Position = baseOffset + headerOffset;

            var header = (RomHeader)StructGetter.ReadBlock(romStream, typeof(RomHeader));

            romSize = header.TranslatedROMSizeByte;
            ramSize = header.TranslatedRAMSizeByte;
        }
    }
}
