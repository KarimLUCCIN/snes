using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware.Instructions.InstructionsSets
{
    public class InstructionADC : Instruction
    {
        public InstructionADC(CPU cpu, AddressingModes addressingMode)
            : base(cpu, OpCodes.ADC, addressingMode)
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
                        throw new InvalidOperationException("Addressing mode unknown for instruction ADC");
                    }
            }
        }

        protected void Execute(int value)
        {
            if (CPU.CarryFlag)
                value++;
            CPU.ACC += value;
            if (CPU.DecimalFlag)
            {
                CPU.ACC = BCDAdjust(CPU.ACC);
                if (CPU.MFlag)
                    CPU.ACC &= 0xFF;
                else
                    CPU.ACC &= 0xFFFF;
            }
        }

        private int BCDAdjust(int val) 
        {
            int nibbleCount = 4;
            if (CPU.MFlag) 
                nibbleCount = 2;
 	               
            // For each 4-bit digit...
            for (int k = 0; k < nibbleCount; k++) 
            {
                // Are those 4-bits larger than 9?
                int digit = ((val & (0xF << (k * 4))) >> (k * 4));
                if (digit > 9) 
                {
                    // If so, add 6 to the digit
                    val += (6 << (k * 4));
                }
            }
            return val;
        }

        protected void SetRegisters()
        {
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
        }
    }
}
