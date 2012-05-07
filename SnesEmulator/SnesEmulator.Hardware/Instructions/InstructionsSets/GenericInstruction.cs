using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware.Instructions.InstructionsSets
{
    public class GenericInstruction : Instruction
    {
        public GenericInstruction(CPU cpu, CPU.Opcodes opCode, CPU.AddressingModes addrMode, bool haveArguments)
            :base(cpu, opCode, addrMode)
        {
            this.haveArguments = haveArguments;
        }

        private bool haveArguments = false;

        public override bool HaveArgs
        {
            get { return haveArguments; }
        }

        public Action<GenericInstruction, int, int> runFunction = null;

        public delegate void DecodeArgumentsFunctionDelegate(GenericInstruction sender, Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref int param1, ref int param2);
        public DecodeArgumentsFunctionDelegate decodeArgumentsFunction;

        public override void Run(int arg1, int arg2)
        {
            if (runFunction != null)
                runFunction(this, arg1, arg1);
        }

        public override void DecodeArguments(Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref InstructionReference instructionReference)
        {
            if (decodeArgumentsFunction != null)
            {
                decodeArgumentsFunction(this, bin, mode, ref offset, ref instructionReference.param1, ref instructionReference.param2);
            }
        }
    }
}
