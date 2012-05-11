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
                        value = CPU.RAM.ReadByte(address);
                        break;
                    }
                case AddressingModes.StackRelative:
                    {
                        // Cet adressage n'existe pas en mode émulation
                        int address = StackRelative(arg1);
                        value = CPU.RAM.ReadByte(address);
                        break;
                    }
                case AddressingModes.Direct:
                    {
                        int address = Direct(arg1);
                        value = CPU.RAM.ReadByte(address);
                        break;
                    }
                case AddressingModes.DirectIndirectLong:
                    {
                        int address = DirectIndirectLong(arg1);
                        value = CPU.RAM.ReadByte(address);
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
                        value = CPU.RAM.ReadByte(address);
                        break;
                    }
                case AddressingModes.AbsoluteLong:
                    {
                        int address = AbsoluteLong(arg1);
                        value = CPU.RAM.ReadByte(address);
                        break;
                    }
                case AddressingModes.DirectIndirectIndexed:
                    {
                        int address = DirectIndirectIndexed(arg1);
                        value = CPU.RAM.ReadByte(address);
                        break;
                    }
                case AddressingModes.DirectIndirect:
                    {
                        int address = DirectIndirect(arg1);
                        value = CPU.RAM.ReadByte(address);
                        break;
                    }
                case AddressingModes.StackRelativeIndirectIndexed:
                    {
                        int address = StackRelativeIndirectIndexed(arg1);
                        value = CPU.RAM.ReadByte(address);
                        break;
                    }
                case AddressingModes.DirectIndexedX:
                    {
                        int address = DirectIndexedX(arg1);
                        value = CPU.RAM.ReadByte(address);
                        break;
                    }
                case AddressingModes.DirectIndirectIndexedLong:
                    {
                        int address = DirectIndirectIndexedLong(arg1);
                        value = CPU.RAM.ReadByte(address);
                        break;
                    }
                case AddressingModes.AbsoluteIndexedX:
                    {
                        int address = AbsoluteIndexedX(arg1);
                        value = CPU.RAM.ReadByte(address);
                        break;
                    }
                case AddressingModes.AbsoluteIndexedY:
                    {
                        int address = AbsoluteIndexedY(arg1);
                        value = CPU.RAM.ReadByte(address);
                        break;
                    }
                case AddressingModes.AbsoluteIndexedLong:
                    {
                        int address = AbsoluteIndexedLong(arg1);
                        value = CPU.RAM.ReadByte(address);
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException("Addressing mode unknow for this instruction");
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
                    {
                        instructionReference.param1 = DecodeInt2Argument(bin, ref offset);
                        break;
                    }
                case AddressingModes.AbsoluteLong:
                    {
                        instructionReference.param1 = DecodeInt3Argument(bin, ref offset);
                        break;
                    }


                case AddressingModes.DirectIndexedIndirect:
                case AddressingModes.StackRelative:
                case AddressingModes.Direct:
                case AddressingModes.DirectIndirect:
                case AddressingModes.DirectIndirectIndexed:
                case AddressingModes.DirectIndirectLong:
                    {
                        instructionReference.param1 = DecodeInt1Argument(bin, ref offset);
                        break;
                    }

                case AddressingModes.AbsoluteIndexedX:
                case AddressingModes.AbsoluteIndexedY:
                    {
                        instructionReference.param1 = DecodeInt2Argument(bin, ref offset);
                        break;
                    }
                case AddressingModes.AbsoluteIndexedLong:
                    {
                        instructionReference.param1 = DecodeInt3Argument(bin, ref offset);
                        break;
                    }

                default:
                    {
                        instructionReference.param1 = DecodeInt1Argument(bin, ref offset);
                        break;
                    }
            }
        }

        protected void Execute(int value)
        {
            if (CPU.CarryFlag)
                value++;
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
