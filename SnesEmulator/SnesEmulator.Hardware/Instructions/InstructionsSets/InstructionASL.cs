using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SnesEmulator.Hardware.Instructions.InstructionsSets
{
    class InstructionASL : Instruction
    {
        public InstructionASL(CPU cpu, AddressingModes addressingMode)
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
                case AddressingModes.ImpliedAccumulator:
                    {
                        if (CPU.MFlag)
                        {
                            CPU.CarryFlag = Convert.ToBoolean((CPU.ACC >> 7) & 1);
                            CPU.ACC = (CPU.ACC << 1) & 0xFF;
                        }
                        else
                        {
                            CPU.CarryFlag = Convert.ToBoolean((CPU.ACC >> 15) & 1);
                            CPU.ACC = (CPU.ACC << 1) & 0xFFFF;
                        }
                        value = CPU.ACC;
                        break;
                    }

                case AddressingModes.Direct:
                    {
                        int address = Direct(arg1);
                        value = ReadAddressedValue(address);
                        Execute(value, address);
                        break;
                    }

                case AddressingModes.Absolute:
                    {
                        int address = Absolute(arg1);
                        value = ReadAddressedValue(address);
                        Execute(value, address);
                        break;
                    }
                case AddressingModes.DirectIndexedX:
                    {
                        int address = DirectIndexedX(arg1);
                        value = ReadAddressedValue(address);
                        Execute(value, address);
                        break;
                    }
                case AddressingModes.AbsoluteIndexedX:
                    {
                        int address = AbsoluteIndexedX(arg1);
                        value = ReadAddressedValue(address);
                        Execute(value, address);
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException("Addressing mode unknown for instruction ASL");
                    }
            }
            SetRegisters(value);
        }

        public override void DecodeArguments(Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref InstructionReference instructionReference)
        {
            switch (AddrMode)
            {
                case AddressingModes.ImpliedAccumulator:
                    {
                        break;
                    }
                case AddressingModes.Absolute:
                case AddressingModes.AbsoluteIndexedX:
                    {
                        instructionReference.param1 = DecodeInt2Argument(bin, ref offset);
                        break;
                    }

                case AddressingModes.Direct:
                case AddressingModes.DirectIndexedX:
                    {
                        instructionReference.param1 = DecodeInt1Argument(bin, ref offset);
                        break;
                    }

                default:
                    {
                        throw new InvalidOperationException("Addressing mode unknown for instruction ASL");
                    }
            }
        }

        protected void Execute(int value, int address)
        {
            if (CPU.MFlag)
            {
                value = (value << 1) & 0xFF;
                CPU.RAM.WriteByte(address, Convert.ToByte(value));
            }
            else
            {
                byte[] data = new byte[2];
                value = (value << 1) & 0xFFFF;
                data[0] = Convert.ToByte(value & 0xFF);
                data[1] = Convert.ToByte((value >> 8) & 0xFF);
                CPU.RAM.Write(address, data, 0, 2);
            }
        }

        protected void SetRegisters(int value)
        {
            CPU.SetZeroFlag(value);
            CPU.SetNegativeFlag(value);
        }
    }
}
