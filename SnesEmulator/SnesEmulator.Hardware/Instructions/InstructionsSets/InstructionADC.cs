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
            switch (AddrMode)
            {
                case CPU.AddressingModes.DirectIndexedIndirect:
                    {
                        int address = CPU.RAM.ReadByte(arg1 + CPU.X);
                        int value = CPU.RAM.ReadByte(address);
                        if (CPU.CarryFlag)
                            value++;
                        CPU.ACC += value;
                        CPU.SetCarryFlag(CPU.ACC);
                        CPU.SetOverflowFlag(CPU.ACC);
                        if (CPU.MFlag)
                        {
                            if (CPU.ACC > Mode8Bits.Max)
                                CPU.ACC = CPU.ACC - (Mode16Bits.Max + 1);
                        }
                        else
                        {
                            if (CPU.ACC > Mode16Bits.Max)
                                CPU.ACC = CPU.ACC - (Mode16Bits.Max + 1);
                        }
                        CPU.SetZeroFlag(CPU.ACC);
                        CPU.SetNegativeFlag(CPU.ACC);
                        break;
                    }

                default:
                    {
                        break;
                    }
            }
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
