﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SnesEmulator.Hardware.Memory;

namespace SnesEmulator.Hardware.Instructions
{
    /// <summary>
    /// Instruction de base
    /// </summary>
    public abstract class Instruction
    {
        public abstract bool HaveArgs { get; }

        private OpCodes code;

        public OpCodes Code
        {
            get { return code; }
        }

        private AddressingModes addrMode;

        public AddressingModes AddrMode
        {
            get { return addrMode; }
        }

        private CPU cpu;

        public CPU CPU
        {
            get { return cpu; }
        }

        /// <summary>
        /// Code hexa de l'instruction (spécifié par le DecodeTable)
        /// </summary>
        public byte AssociatedHexCode { get; set; }

        public Instruction(CPU cpu, OpCodes code, AddressingModes addrMode)
        {
            this.cpu = cpu;
            this.code = code;
            this.addrMode = addrMode;
        }

        protected string PrintParameter(string param1, string param2 = null)
        {
            if (!String.IsNullOrEmpty(param1))
                param1 = "$" + param1;

            if (!String.IsNullOrEmpty(param2))
                param2 = "$" + param2;

            switch (addrMode)
            {
                case AddressingModes.Implied:
                    return param1;
                case AddressingModes.ImmediateMemoryFlag:
                case AddressingModes.ImmediateIndexFlag:
                case AddressingModes.Immediate8Bit:
                    return "#" + param1;
                case AddressingModes.Relative:
                case AddressingModes.RelativeLong:
                case AddressingModes.Direct:
                case AddressingModes.DirectIndirect:
                    return "(" + param1 + ")";
                case AddressingModes.DirectIndexedX:
                    return param1 + ", x";
                case AddressingModes.DirectIndexedY:
                    return param1 + ", y";
                case AddressingModes.DirectIndexedIndirect:
                    return "(" + param1 + ", x)";
                case AddressingModes.DirectIndirectIndexed:
                    return "(" + param1 + "), y";
                case AddressingModes.DirectIndirectLong:
                    return "[" + param1 + "]";
                case AddressingModes.DirectIndirectIndexedLong:
                    return "[" + param1 + "], y";
                case AddressingModes.Absolute:
                    return param1;
                case AddressingModes.AbsoluteIndexedX:
                    return param1 + ", x";
                case AddressingModes.AbsoluteIndexedY:
                    return param1 + ", y";
                case AddressingModes.AbsoluteLong:
                    return param1;
                case AddressingModes.AbsoluteIndexedLong:
                    return param1 + ", x";
                case AddressingModes.StackRelative:
                    return param1 + ", s";
                case AddressingModes.StackRelativeIndirectIndexed:
                    return "(" + param1 + ", s), y";
                case AddressingModes.AbsoluteIndirect:
                    return "(" + param1 + ")";
                case AddressingModes.AbsoluteIndirectLong:
                    return "[" + param1 + "]";
                case AddressingModes.AbsoluteIndexedIndirect:
                    return "(" + param1 + ", x)";
                case AddressingModes.ImpliedAccumulator:
                    return param1;
                case AddressingModes.BlockMove:
                    return param1 + ", " + param2;
                default:
                    return param1;
            }
        }

        public virtual string StringRepresentation(int param1 = 0, int param2 = 0)
        {
            return String.Format("{0} {1}", code.ToString(), HaveArgs ? PrintParameter(param1.ToString("X"), param2.ToString("X")) : String.Empty);
        }

        public abstract void Run(int arg1, int arg2);

        public byte DecodeInt1Argument(MemoryBin bin, ref int offset)
        {
            offset++;
            return bin.ReadInt1(offset - 1);
        }

        public short DecodeInt2Argument(MemoryBin bin, ref int offset)
        {
            offset += 2;

            return bin.ReadInt2(offset - 2);
        }

        public int DecodeInt3Argument(MemoryBin bin, ref int offset)
        {
            offset += 3;

            return bin.ReadInt3(offset - 3);
        }

        public int DecodeI1I2ArgumentForMFlag(MemoryBin bin, ref int offset, ref InstructionDecodeContext context)
        {
            if (context.MFlag)
                return DecodeInt1Argument(bin, ref offset);
            else
                return DecodeInt2Argument(bin, ref offset);
        }

        public int DecodeI1I2ArgumentForXFlag(MemoryBin bin, ref int offset, ref InstructionDecodeContext context)
        {
            if (context.XFlag)
                return DecodeInt1Argument(bin, ref offset);
            else
                return DecodeInt2Argument(bin, ref offset);
        }

        /// <summary>
        /// Décode les arguments nécessaires pour l'instruction
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="mode"></param>
        /// <param name="context"></param>
        /// <param name="instructionReference"></param>
        public abstract void DecodeArguments(MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref InstructionReference instructionReference);

        public int ReadAddressedValue(int address)
        {
            int value = 0;

            if (cpu.MFlag)
                value = cpu.RAM.ReadInt1(address);
            else
                value = cpu.RAM.ReadInt2(address);

            return value;
        }

        public void WriteAddressedValue(int address, int value)
        {
            if (cpu.MFlag)
                cpu.RAM.WriteInt1(address, (byte)value);
            else
                cpu.RAM.WriteInt2(address, (byte)value);
        }

        #region Addressing Modes

        protected int DirectIndexedIndirect(int arg1)
        {
            int address;
            int X = CPU.XFlag ? CPU.X & 0xFF : CPU.X; // Si x = 1 (donc X sur 8bits) on garde que le low byte
            if (CPU.EFlag)
            {
                int addressPointerLow = CPU.RAM.ReadInt1(arg1 + X);
                int addressPointerHigh = CPU.RAM.ReadInt1(arg1 + X + 1);
                address = addressPointerLow | addressPointerHigh << 8;
            }
            else
            {
                int addressPointerLow = CPU.RAM.ReadInt1(arg1 + CPU.D + X);
                int addressPointerHigh = CPU.RAM.ReadInt1(arg1 + CPU.D + X + 1);
                address = addressPointerLow | addressPointerHigh << 8 | CPU.DBR << 16;
            }
            return address;
        }

        protected int StackRelative(int arg1)
        {
            // Cet adressage n'existe pas en mode émulation
            return arg1 + CPU.SP;
        }

        protected int Direct(int arg1)
        {
            if (CPU.EFlag)
                return arg1;
            else
                return arg1 + CPU.D;
        }

        protected int DirectIndirectLong(int arg1)
        {
            // Cet adressage n'existe pas en mode émulation
            int addressPointerLow = CPU.RAM.ReadInt1(arg1 + CPU.D);
            int addressPointerHigh = CPU.RAM.ReadInt1(arg1 + CPU.D + 1);
            int addressPointerDatabank = CPU.RAM.ReadInt1(arg1 + CPU.D + 2);
            return addressPointerLow | addressPointerHigh << 8 | addressPointerDatabank << 16;
        }

        protected int Absolute(int arg1)
        {
            if (CPU.EFlag)
                return arg1;
            else
                return arg1 | CPU.DBR << 16;
        }

        protected int AbsoluteLong(int arg1)
        {
            // Cet adressage n'existe pas en mode émulation
            return arg1;
        }

        protected int DirectIndirectIndexed(int arg1)
        {
            int address;
            int Y = CPU.XFlag ? CPU.Y & 0xFF : CPU.Y; // Si x = 1 (donc Y sur 8bits) on garde que le low byte
            if (CPU.EFlag)
            {
                int addressPointerLow = CPU.RAM.ReadInt1(arg1);
                int addressPointerHigh = CPU.RAM.ReadInt1(arg1 + 1);
                address = addressPointerLow | addressPointerHigh << 8;
                address += Y;
            }
            else
            {
                int addressPointerLow = CPU.RAM.ReadInt1(arg1 + CPU.D);
                int addressPointerHigh = CPU.RAM.ReadInt1(arg1 + CPU.D + 1);
                address = addressPointerLow | addressPointerHigh << 8 | CPU.DBR << 16;
                address += Y;
            }
            return address;
        }

        protected int DirectIndirect(int arg1)
        {
            int address;
            if (CPU.EFlag)
            {
                int addressPointerLow = CPU.RAM.ReadInt1(arg1);
                int addressPointerHigh = CPU.RAM.ReadInt1(arg1 + 1);
                address = addressPointerLow | addressPointerHigh << 8;
            }
            else
            {
                int addressPointerLow = CPU.RAM.ReadInt1(arg1 + CPU.D);
                int addressPointerHigh = CPU.RAM.ReadInt1(arg1 + CPU.D + 1);
                address = addressPointerLow | addressPointerHigh << 8 | CPU.DBR << 16;
            }
            return address;
        }

        protected int StackRelativeIndirectIndexed(int arg1)
        {
            // Cet adressage n'existe pas en mode émulation
            int Y = CPU.XFlag ? CPU.Y & 0xFF : CPU.Y; // Si x = 1 (donc Y sur 8bits) on garde que le low byte
            int addressPointerLow = CPU.RAM.ReadInt1(arg1 + CPU.SP);
            int addressPointerHigh = CPU.RAM.ReadInt1(arg1 + CPU.SP + 1);
            int address = addressPointerLow | addressPointerHigh << 8 | CPU.DBR << 16;
            address += Y;
            return address;
        }

        protected int DirectIndexedX(int arg1)
        {
            int X = CPU.XFlag ? CPU.X & 0xFF : CPU.X; // Si x = 1 (donc X sur 8bits) on garde que le low byte
            if (CPU.EFlag)
                return arg1 + X;
            else
                return arg1 + CPU.D + X;
        }

        protected int DirectIndexedY(int arg1)
        {
            int Y = CPU.XFlag ? CPU.Y & 0xFF : CPU.Y; // Si x = 1 (donc X sur 8bits) on garde que le low byte
            if (CPU.EFlag)
                return arg1 + Y;
            else
                return arg1 + CPU.D + Y;
        }

        protected int DirectIndirectIndexedLong(int arg1)
        {
            // Cet adressage n'existe pas en mode émulation
            int Y = CPU.XFlag ? CPU.Y & 0xFF : CPU.Y; // Si x = 1 (donc Y sur 8bits) on garde que le low byte
            int addressPointerLow = CPU.RAM.ReadInt1(arg1 + CPU.D);
            int addressPointerHigh = CPU.RAM.ReadInt1(arg1 + CPU.D + 1);
            int addressPointerDatabank = CPU.RAM.ReadInt1(arg1 + CPU.D + 2);
            int address = addressPointerLow | addressPointerHigh << 8 | addressPointerDatabank << 16;
            address += Y;
            return address;
        }

        protected int AbsoluteIndexedX(int arg1)
        {
            int X = CPU.XFlag ? CPU.X & 0xFF : CPU.X; // Si x = 1 (donc X sur 8bits) on garde que le low byte
            if (CPU.EFlag)
                return arg1 + CPU.X;
            else
                return (arg1 | CPU.DBR << 16) + CPU.X;
        }

        protected int AbsoluteIndexedY(int arg1)
        {
            int Y = CPU.XFlag ? CPU.Y & 0xFF : CPU.Y; // Si x = 1 (donc Y sur 8bits) on garde que le low byte
            if (CPU.EFlag)
                return arg1 + CPU.Y;
            else
                return (arg1 | CPU.DBR << 16) + CPU.Y;
        }

        protected int AbsoluteIndexedLong(int arg1)
        {
            int X = CPU.XFlag ? CPU.X & 0xFF : CPU.X; // Si x = 1 (donc X sur 8bits) on garde que le low byte
            return arg1 + X;
        }

        /// <summary>
        /// Obtient une addresse à partir de l'argument spécifié en évaluant le mode
        /// </summary>
        /// <param name="param"></param>
        /// <param name="p_mode"></param>
        /// <returns></returns>
        public int ResolveAddress(int param, AddressingModes? p_mode = null)
        {
            var mode = p_mode == null ? addrMode : p_mode.Value;

            switch (mode)
            {
                case AddressingModes.DirectIndexedIndirect:
                    {
                        return DirectIndexedIndirect(param);
                    }
                case AddressingModes.StackRelative:
                    {
                        // Cet adressage n'existe pas en mode émulation
                        return StackRelative(param);
                    }
                case AddressingModes.Direct:
                    {
                        return Direct(param);
                    }
                case AddressingModes.DirectIndirectLong:
                    {
                        return DirectIndirectLong(param);
                    }
                case AddressingModes.ImmediateMemoryFlag:
                case AddressingModes.ImmediateIndexFlag:
                case AddressingModes.Immediate8Bit:
                    {
                        throw new NotSupportedException("Immediate Mode doesn't correspond to an address");
                    }
                case AddressingModes.Absolute:
                    {
                        return Absolute(param);
                    }
                case AddressingModes.AbsoluteLong:
                    {
                        return AbsoluteLong(param);
                    }
                case AddressingModes.DirectIndirectIndexed:
                    {
                        return DirectIndirectIndexed(param);
                    }
                case AddressingModes.DirectIndirect:
                    {
                        return DirectIndirect(param);
                    }
                case AddressingModes.StackRelativeIndirectIndexed:
                    {
                        return StackRelativeIndirectIndexed(param);
                    }
                case AddressingModes.DirectIndexedX:
                    {
                        return DirectIndexedX(param);
                    }
                case AddressingModes.DirectIndexedY:
                    {
                        return DirectIndexedY(param);
                    }
                case AddressingModes.DirectIndirectIndexedLong:
                    {
                        return DirectIndirectIndexedLong(param);
                    }
                case AddressingModes.AbsoluteIndexedX:
                    {
                        return AbsoluteIndexedX(param);
                    }
                case AddressingModes.AbsoluteIndexedY:
                    {
                        return AbsoluteIndexedY(param);
                    }
                case AddressingModes.AbsoluteIndexedLong:
                    {
                        return AbsoluteIndexedLong(param);
                    }
                default:
                    { throw new NotSupportedException(mode.ToString()); }
            }
        }

        /// <summary>
        /// Obtient un argument à partir de la mémoire en fonction du mode sélectionné.
        /// </summary>
        /// <remarks>Avoir un mode "BlockMove" fera une exception, ce mode est spécial</remarks>
        /// <param name="param"></param>
        /// <param name="p_mode"></param>
        /// <returns></returns>
        public int ResolveArgument(int param, AddressingModes? p_mode = null)
        {
            var mode = p_mode == null ? addrMode : p_mode.Value;

            switch (mode)
            {
                case AddressingModes.Immediate8Bit:
                case AddressingModes.ImmediateIndexFlag:
                case AddressingModes.ImmediateMemoryFlag:
                    {
                        return param;
                    }
                case AddressingModes.BlockMove:
                    {
#warning TO CHECK
                        throw new NotSupportedException("BlockMove doesn't resolve like this");
                    }
                default:
                    {
                        return ReadAddressedValue(ResolveAddress(param, mode));
                    }
            }
        }

        #endregion
    }
}
