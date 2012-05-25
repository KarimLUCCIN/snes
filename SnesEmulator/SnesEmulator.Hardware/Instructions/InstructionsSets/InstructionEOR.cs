using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware.Instructions.InstructionsSets
{
    public class InstructionEOR : Instruction
    {
        public InstructionEOR(CPU cpu, AddressingModes addressingMode)
            : base(cpu, OpCodes.EOR, addressingMode)
        {

        }

        public override bool HaveArgs
        {
            get { return true; }
        }

        public override void Run(int arg1, int arg2)
        {
            var value = ResolveArgument(arg1);
            Execute(value);
            SetRegisters();
        }

        public override void DecodeArguments(Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref InstructionReference instructionReference)
        {
            switch (AddrMode)
            {
                case AddressingModes.ImmediateMemoryFlag:
                case AddressingModes.ImmediateIndexFlag:
                case AddressingModes.Immediate8Bit:
                    {
                        instructionReference.param1 = DecodeI1I2ArgumentForMFlag(bin, ref offset, ref context);
                        break;
                    }
                case AddressingModes.Absolute:
                case AddressingModes.AbsoluteIndexedX:
                case AddressingModes.AbsoluteIndexedY:
                    {
                        instructionReference.param1 = DecodeInt2Argument(bin, ref offset);
                        break;
                    }
                case AddressingModes.AbsoluteLong:
                case AddressingModes.AbsoluteIndexedLong:
                    {
                        instructionReference.param1 = DecodeInt3Argument(bin, ref offset);
                        break;
                    }

                case AddressingModes.Direct:
                case AddressingModes.DirectIndirect:
                case AddressingModes.DirectIndirectLong:
                case AddressingModes.DirectIndexedX:
                case AddressingModes.DirectIndexedIndirect:
                case AddressingModes.DirectIndirectIndexed:
                case AddressingModes.DirectIndirectIndexedLong:
                case AddressingModes.StackRelative:
                case AddressingModes.StackRelativeIndirectIndexed:
                    {
                        instructionReference.param1 = DecodeInt1Argument(bin, ref offset);
                        break;
                    }

                default:
                    {
                        throw new InvalidOperationException("Addressing mode unknown for instruction EOR");
                    }
            }
        }

        protected void Execute(int value)
        {
            CPU.ACC ^= value;
        }

        protected void SetRegisters()
        {
            CPU.SetZeroFlag(CPU.ACC);
            CPU.SetNegativeFlag(CPU.ACC);
        }
    }
}
