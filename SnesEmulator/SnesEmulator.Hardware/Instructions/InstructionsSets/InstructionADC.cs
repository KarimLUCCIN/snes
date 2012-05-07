using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware.Instructions.InstructionsSets
{
    public class InstructionADC : Instruction
    {
        public InstructionADC(CPU cpu, CPU.AddressingModes addressingMode)
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

        public override void DecodeArguments(Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref InstructionReference instructionReference)
        {
            switch (AddrMode)
            {
                case CPU.AddressingModes.ImmediateMemoryFlag:
                case CPU.AddressingModes.ImmediateIndexFlag:
                case CPU.AddressingModes.Immediate8Bit:
                    {
                        if (mode == MFlagMode.Mode8Bits)
                            instructionReference.param1 = DecodeByteArgument(bin, ref offset);
                        else
                            instructionReference.param1 = DecodeInt2Argument(bin, ref offset);
                        break;
                    }
                case CPU.AddressingModes.Absolute:
                    {
                        instructionReference.param1 = DecodeInt2Argument(bin, ref offset);
                        break;
                    }
                case CPU.AddressingModes.AbsoluteLong:
                    {
                        instructionReference.param1 = DecodeInt3Argument(bin, ref offset);
                        break;
                    }


                case CPU.AddressingModes.DirectIndexedIndirect:
                case CPU.AddressingModes.StackRelative:
                case CPU.AddressingModes.Direct:
                case CPU.AddressingModes.DirectIndirect:
                case CPU.AddressingModes.DirectIndirectIndexed:
                case CPU.AddressingModes.DirectIndirectLong:
                    {
                        instructionReference.param1 = DecodeByteArgument(bin, ref offset);
                        break;
                    }

                case CPU.AddressingModes.AbsoluteIndexedX:
                case CPU.AddressingModes.AbsoluteIndexedY:
                    {
                        instructionReference.param1 = DecodeInt2Argument(bin, ref offset);
                        break;
                    }
                case CPU.AddressingModes.AbsoluteIndexedLong:
                    {
                        instructionReference.param1 = DecodeInt3Argument(bin, ref offset);
                        break;
                    }

                default:
                    {
                        instructionReference.param1 = DecodeByteArgument(bin, ref offset);
                        break;
                    }
            }
        }
    }
}
