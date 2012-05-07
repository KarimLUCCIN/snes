using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnesEmulator.Hardware.Memory;
using SnesEmulator.Hardware.Instructions.InstructionsSets;

namespace SnesEmulator.Hardware.Instructions
{
    /// <summary>
    /// Décodeur d'instructions
    /// </summary>
    public class InstructionsDecoder
    {
        private CPU cpu;

        public CPU CPU
        {
            get { return cpu; }
        }
        
        public InstructionsDecoder(CPU cpu)
        {
            if (cpu == null)
                throw new ArgumentNullException("cpu");

            this.cpu = cpu;
        }

        /// <summary>
        /// Effectue le décodage des instructions
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="offset"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public InstructionsBin Decode(MemoryBin bin, int offset, CPUMode mode)
        {
            var decodeResult = new List<InstructionReference>();
            var length = bin.Length;

            var insTable = CPU.DecodeTable.KnownInstructions;

            int originalOffset = 0;

            while (offset < length)
            {
                var code = bin.ReadByte(originalOffset = offset);
                offset++;

                var match = insTable[code];

                var instructionReference = match.Decode(bin, mode, ref offset);
                instructionReference.offset = originalOffset;

                decodeResult.Add(instructionReference);
            }

            var result = new InstructionsBin() { DecodeMode = mode, Instructions = decodeResult.ToArray() };

            return result;
        }
    }
}
