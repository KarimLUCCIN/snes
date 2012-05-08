using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnesEmulator.Hardware.Instructions;
using SnesEmulator.Hardware.Memory;

namespace SnesEmulator.Hardware
{
    /// <summary>
    /// Une SNES ... \o/
    /// </summary>
    public class SnesPlatform
    {
        private CPU cpu;

        /// <summary>
        /// CPU
        /// </summary>
        public CPU CPU
        {
            get { return cpu; }
        }

        private InstructionsDecoder decoder;

        /// <summary>
        /// Décodeur d'instructions
        /// </summary>
        public InstructionsDecoder Decoder
        {
            get { return decoder; }
        }

        private MemoryContainer memory;

        public MemoryContainer Memory
        {
            get { return memory; }
        }

        /// <summary>
        /// Crée une nouvelle instance de SNES
        /// </summary>
        public SnesPlatform(int romSize, int ramSize)
        {
            int memorySize = romSize + ramSize;
            //on ne peut pas addresser plus de 64 Mo toute façon ..
            if (memorySize < 0 || memorySize > 64 * 1024 * 1024)
                throw new ArgumentOutOfRangeException("memorySize");

            memory = new MemoryContainer(memorySize);

            cpu = new CPU(new MemoryBin(memory, romSize, ramSize));
            decoder = new InstructionsDecoder(cpu);
        }
    }
}
