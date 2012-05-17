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

        Instruction[] knownInstructions = null;

        public Instruction[] KnownInstructions
        {
            get { return knownInstructions; }
        }

        public InstructionsDecoder(CPU cpu)
        {
            if (cpu == null)
                throw new ArgumentNullException("cpu");

            this.cpu = cpu;

            knownInstructions = cpu.DecodeTable.KnownInstructions;
        }

        public InstructionsBin Decode(MemoryBin bin, int offset)
        {
            return Decode(bin, offset, CPU.BuildCurrentContext());
        }

        /// <summary>
        /// Effectue le décodage des instructions
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="offset"></param>
        /// <param name="context">Contexte de décodage. Par défaut, prend la valeur du mode actuel du CPU</param>
        /// <returns></returns>
        public InstructionsBin Decode(MemoryBin bin, int offset, InstructionDecodeContext context)
        {
            context.Source = bin;

            var decodeResult = new List<InstructionReference>();
            var length = bin.Length;

            int originalOffset = 0;

            while (offset < length)
            {
                var code = bin.ReadInt1(originalOffset = offset);
                offset++;

                var match = knownInstructions[code];

                var instructionReference = new InstructionReference();
                instructionReference.instruction = match;
                instructionReference.offset = originalOffset;

                match.DecodeArguments(bin, ref context, ref offset, ref instructionReference);

                decodeResult.Add(instructionReference);
            }

            var result = new InstructionsBin() { DecodeContext = context, Instructions = decodeResult.ToArray() };

            return result;
        }

        /// <summary>
        /// Decode une unique instruction à partir de l'offset spécifié dans le bin spécifié à l'aide du contexte courrant, et la stoque dans decodedInstruction
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="offset"></param>
        /// <param name="context"></param>
        /// <param name="decodedInstruction"></param>
        public void DecodeOnce(MemoryBin bin, ref int offset, ref InstructionDecodeContext context, ref InstructionReference decodedInstruction)
        {
            var code = bin.ReadInt1(offset);
            offset++;

            var match = knownInstructions[code];
            decodedInstruction.instruction = match;
            decodedInstruction.offset = offset - 1;

            match.DecodeArguments(bin, ref context, ref offset, ref decodedInstruction);
        }
    }
}
