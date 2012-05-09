using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware.Instructions.InstructionsSets
{
    public class InstructionWDM : Instruction
    {
        public InstructionWDM(CPU cpu)
            : base(cpu, OpCodes.WDM, AddressingModes.Implied)
        {

        }

        public override bool HaveArgs
        {
            get { return false; }
        }

        public override void Run(int arg1, int arg2)
        {

        }

        public override void DecodeArguments(Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref InstructionReference instructionReference)
        {
            /* No Args */
        }
    }

    public class InstructionNOP : Instruction
    {
        public InstructionNOP(CPU cpu)
            : base(cpu, OpCodes.NOP, AddressingModes.Implied)
        {

        }

        public override bool HaveArgs
        {
            get { return false; }
        }

        public override void Run(int arg1, int arg2)
        {

        }

        public override void DecodeArguments(Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref InstructionReference instructionReference)
        {
            /* No Args */
        }
    }
}
