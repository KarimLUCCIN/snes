﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware.Instructions.InstructionsSets
{
    public class InstructionXBA : Instruction
    {
        public InstructionXBA(CPU cpu)
            :base(cpu, CPU.Opcodes.XBA, CPU.AddressingModes.Implied)
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
            /* No Args */
        }
    }
}
