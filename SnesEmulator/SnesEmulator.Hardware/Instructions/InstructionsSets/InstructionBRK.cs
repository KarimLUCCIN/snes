using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware.Instructions.InstructionsSets
{
    public class InstructionBRK: Instruction
    {
        public InstructionBRK(CPU cpu)
            :base(cpu, CPU.Opcodes.BRK, CPU.AddressingModes.StackRelative)
        {

        }

        public override bool HaveArgs
        {
            get { return false; }
        }

        public override void Run(int arg1, int arg2)
        {
            throw new NotImplementedException();
        }

        public override void DecodeArguments(Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref InstructionReference instructionReference)
        {
            /* on skip le byte suivant, qui est censé être une signature */
            offset++;
        }
    }
}
