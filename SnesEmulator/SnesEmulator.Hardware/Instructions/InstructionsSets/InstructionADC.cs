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
            int value = 0;
            switch (AddrMode)
            {
                case AddressingModes.DirectIndexedIndirect:
                    {
                        int address = DirectIndexedIndirect(arg1);
                        value = ReadAddressedValue(address);
                        break;
                    }
                case AddressingModes.StackRelative:
                    {
                        // Cet adressage n'existe pas en mode émulation
                        int address = StackRelative(arg1);
                        value = ReadAddressedValue(address);
                        break;
                    }
                case AddressingModes.Direct:
                    {
                        int address = Direct(arg1);
                        value = ReadAddressedValue(address);
                        break;
                    }
                case AddressingModes.DirectIndirectLong:
                    {
                        int address = DirectIndirectLong(arg1);
                        value = ReadAddressedValue(address);
                        break;
                    }
                case AddressingModes.ImmediateMemoryFlag:
                case AddressingModes.ImmediateIndexFlag:
                case AddressingModes.Immediate8Bit:
                    {
                        value = arg1;
                        break;
                    }
                case AddressingModes.Absolute:
                    {
                        int address = Absolute(arg1);
                        value = ReadAddressedValue(address);
                        break;
                    }
                case AddressingModes.AbsoluteLong:
                    {
                        int address = AbsoluteLong(arg1);
                        value = ReadAddressedValue(address);
                        break;
                    }
                case AddressingModes.DirectIndirectIndexed:
                    {
                        int address = DirectIndirectIndexed(arg1);
                        value = ReadAddressedValue(address);
                        break;
                    }
                case AddressingModes.DirectIndirect:
                    {
                        int address = DirectIndirect(arg1);
                        value = ReadAddressedValue(address);
                        break;
                    }
                case AddressingModes.StackRelativeIndirectIndexed:
                    {
                        int address = StackRelativeIndirectIndexed(arg1);
                        value = ReadAddressedValue(address);
                        break;
                    }
                case AddressingModes.DirectIndexedX:
                    {
                        int address = DirectIndexedX(arg1);
                        value = ReadAddressedValue(address);
                        break;
                    }
                case AddressingModes.DirectIndirectIndexedLong:
                    {
                        int address = DirectIndirectIndexedLong(arg1);
                        value = ReadAddressedValue(address);
                        break;
                    }
                case AddressingModes.AbsoluteIndexedX:
                    {
                        int address = AbsoluteIndexedX(arg1);
                        value = ReadAddressedValue(address);
                        break;
                    }
                case AddressingModes.AbsoluteIndexedY:
                    {
                        int address = AbsoluteIndexedY(arg1);
                        value = ReadAddressedValue(address);
                        break;
                    }
                case AddressingModes.AbsoluteIndexedLong:
                    {
                        int address = AbsoluteIndexedLong(arg1);
                        value = ReadAddressedValue(address);
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException("Addressing mode unknown for instruction ADC");
                    }
            }
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
            if (CPU.DecimalFlag)
            {
                if (CPU.MFlag)
                {
                    int tmp = CPU.ACC;
                    if (tmp > 0x10)
                        tmp = Convert.ToInt32(tmp.ToString("X"), 10);
                    CPU.ACC = CPU.BCDConversion(tmp + value);
                    if (CPU.ACC >= 0x100)
                        CPU.ACC = CPU.ACC - 0x100;
                }
                else
                {
                    int tmp = CPU.ACC;
                    if (tmp > 0x10)
                        tmp = Convert.ToInt32(tmp.ToString("X"), 10);
                    int sum = CPU.Decimal16bit(value) + tmp;
                    CPU.ACC = CPU.BCDConversion(sum);
                    if (CPU.ACC >= 0x9999)
                    {
                        CPU.ACC &= 0xFFFF;
                        CPU.CarryFlag = true;
                    }
                }
            }
            else
                CPU.ACC += value;
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
