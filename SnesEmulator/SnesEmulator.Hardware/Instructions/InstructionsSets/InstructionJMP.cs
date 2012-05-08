using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware.Instructions.InstructionsSets
{
    public class InstructionJMP : Instruction
    {
        public InstructionJMP(CPU cpu, CPU.AddressingModes addressingMode)
            : base(cpu, CPU.Opcodes.ADC, addressingMode)
        {

        }

        public override bool HaveArgs
        {
            get { return true; }
        }

        public override void Run(int arg1, int arg2)
        {
            throw new NotImplementedException();
        }

        public override void DecodeArguments(Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref InstructionReference instructionReference)
        {
            switch (AddrMode)
            {
                case CPU.AddressingModes.AbsoluteLong:
                    {
                        instructionReference.param1 = DecodeInt3Argument(bin, ref offset);
                        break;
                    }
                default:
                    {
                        instructionReference.param1 = DecodeInt2Argument(bin, ref offset);
                        break;
                    }
            }
        }
    }
}
