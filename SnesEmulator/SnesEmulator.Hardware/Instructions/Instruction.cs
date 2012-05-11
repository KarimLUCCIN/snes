using System;
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
            return (byte)bin.ReadByte(offset-1);
        }

        public short DecodeInt2Argument(MemoryBin bin, ref int offset)
        {
            byte low, high;
            low = (byte)bin.ReadByte(offset);
            high = (byte)bin.ReadByte(offset + 1);

            offset += 2;

            return (short)(low | high << 8);
        }

        public int DecodeInt3Argument(MemoryBin bin, ref int offset)
        {
            byte a, b, c;
            a = (byte)bin.ReadByte(offset);
            b = (byte)bin.ReadByte(offset + 1);
            c = (byte)bin.ReadByte(offset + 2);

            offset += 3;

            return (int)(a | b << 8 | c << 16);
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

        #region Addressing Modes

        public int DirectIndexedIndirect(int arg1)
        {
            int address;
            int X = CPU.XFlag ? CPU.X & 0xFF : CPU.X; // Si x = 1 (donc X sur 8bits) on garde que le low byte
            if (CPU.EFlag)
            {
                int addressPointerLow = CPU.RAM.ReadByte(arg1 + X);
                int addressPointerHigh = CPU.RAM.ReadByte(arg1 + X + 1);
                address = addressPointerLow | addressPointerHigh << 8;
            }
            else
            {
                int addressPointerLow = CPU.RAM.ReadByte(arg1 + CPU.D + X);
                int addressPointerHigh = CPU.RAM.ReadByte(arg1 + CPU.D + X + 1);
                address = addressPointerLow | addressPointerHigh << 8 | CPU.DBR << 16;
            }
            return address;
        }

        public int StackRelative(int arg1)
        {
            // Cet adressage n'existe pas en mode émulation
            return arg1 + CPU.SP;
        }

        public int Direct(int arg1)
        {
            if (CPU.EFlag)
                return arg1;
            else
                return arg1 + CPU.D;
        }

        public int DirectIndirectLong(int arg1)
        {
            // Cet adressage n'existe pas en mode émulation
            int addressPointerLow = CPU.RAM.ReadByte(arg1 + CPU.D);
            int addressPointerHigh = CPU.RAM.ReadByte(arg1 + CPU.D + 1);
            int addressPointerDatabank = CPU.RAM.ReadByte(arg1 + CPU.D + 2);
            return addressPointerLow | addressPointerHigh << 8 | addressPointerDatabank << 16;
        }

        public int Absolute(int arg1)
        {
            if (CPU.EFlag)
                return arg1;
            else
                return arg1 | CPU.DBR << 16;
        }

        public int AbsoluteLong(int arg1)
        {
            // Cet adressage n'existe pas en mode émulation
            return arg1;
        }

        public int DirectIndirectIndexed(int arg1)
        {
            int address;
            int Y = CPU.XFlag ? CPU.Y & 0xFF : CPU.Y; // Si x = 1 (donc Y sur 8bits) on garde que le low byte
            if (CPU.EFlag)
            {
                int addressPointerLow = CPU.RAM.ReadByte(arg1);
                int addressPointerHigh = CPU.RAM.ReadByte(arg1 + 1);
                address = addressPointerLow | addressPointerHigh << 8;
                address += Y;
            }
            else
            {
                int addressPointerLow = CPU.RAM.ReadByte(arg1 + CPU.D);
                int addressPointerHigh = CPU.RAM.ReadByte(arg1 + CPU.D + 1);
                address = addressPointerLow | addressPointerHigh << 8 | CPU.DBR << 16;
                address += Y;
            }
            return address;
        }

        public int DirectIndirect(int arg1)
        {
            int address;
            if (CPU.EFlag)
            {
                int addressPointerLow = CPU.RAM.ReadByte(arg1);
                int addressPointerHigh = CPU.RAM.ReadByte(arg1 + 1);
                address = addressPointerLow | addressPointerHigh << 8;
            }
            else
            {
                int addressPointerLow = CPU.RAM.ReadByte(arg1 + CPU.D);
                int addressPointerHigh = CPU.RAM.ReadByte(arg1 + CPU.D + 1);
                address = addressPointerLow | addressPointerHigh << 8 | CPU.DBR << 16;
            }
            return address;
        }

        public int StackRelativeIndirectIndexed(int arg1)
        {
            // Cet adressage n'existe pas en mode émulation
            int Y = CPU.XFlag ? CPU.Y & 0xFF : CPU.Y; // Si x = 1 (donc Y sur 8bits) on garde que le low byte
            int addressPointerLow = CPU.RAM.ReadByte(arg1 + CPU.SP);
            int addressPointerHigh = CPU.RAM.ReadByte(arg1 + CPU.SP + 1);
            int address = addressPointerLow | addressPointerHigh << 8 | CPU.DBR << 16;
            address += Y;
            return address;
        }

        public int DirectIndexedX(int arg1)
        {
            int X = CPU.XFlag ? CPU.X & 0xFF : CPU.X; // Si x = 1 (donc X sur 8bits) on garde que le low byte
            if (CPU.EFlag)
                return arg1 + X;
            else
                return arg1 + CPU.D + X;
        }

        public int DirectIndirectIndexedLong(int arg1)
        {
            // Cet adressage n'existe pas en mode émulation
            int Y = CPU.XFlag ? CPU.Y & 0xFF : CPU.Y; // Si x = 1 (donc Y sur 8bits) on garde que le low byte
            int addressPointerLow = CPU.RAM.ReadByte(arg1 + CPU.D);
            int addressPointerHigh = CPU.RAM.ReadByte(arg1 + CPU.D + 1);
            int addressPointerDatabank = CPU.RAM.ReadByte(arg1 + CPU.D + 2);
            int address = addressPointerLow | addressPointerHigh << 8 | addressPointerDatabank << 16;
            address += Y;
            return address;
        }

        public int AbsoluteIndexedX(int arg1)
        {
            int X = CPU.XFlag ? CPU.X & 0xFF : CPU.X; // Si x = 1 (donc X sur 8bits) on garde que le low byte
            if (CPU.EFlag)
                return arg1 + CPU.X;
            else
                return (arg1 | CPU.DBR << 16) + CPU.X;
        }

        public int AbsoluteIndexedY(int arg1)
        {
            int Y = CPU.XFlag ? CPU.Y & 0xFF : CPU.Y; // Si x = 1 (donc Y sur 8bits) on garde que le low byte
            if (CPU.EFlag)
                return arg1 + CPU.Y;
            else
                return (arg1 | CPU.DBR << 16) + CPU.Y;
        }

        public int AbsoluteIndexedLong(int arg1)
        {
            int X = CPU.XFlag ? CPU.X & 0xFF : CPU.X; // Si x = 1 (donc X sur 8bits) on garde que le low byte
            return arg1 + X;
        }

        #endregion
    }
}
