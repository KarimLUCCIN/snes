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

        private CPU.Opcodes code;

        public CPU.Opcodes Code
        {
            get { return code; }
        }

        private CPU.AddressingModes addrMode;

        public CPU.AddressingModes AddrMode
        {
            get { return addrMode; }
        }

        private CPU cpu;

        public CPU CPU
        {
            get { return cpu; }
        }

        public Instruction(CPU cpu, CPU.Opcodes code, CPU.AddressingModes addrMode)
        {
            this.cpu = cpu;
            this.code = code;
            this.addrMode = addrMode;
        }

        protected virtual string PrintParameter(string param1, string param2 = null)
        {
            switch (addrMode)
            {
                case CPU.AddressingModes.Implied:
                    return param1;
                case CPU.AddressingModes.ImmediateMemoryFlag:
                case CPU.AddressingModes.ImmediateIndexFlag:
                case CPU.AddressingModes.Immediate8Bit:
                    return "#" + param1;
                case CPU.AddressingModes.Relative:
                case CPU.AddressingModes.RelativeLong:
                case CPU.AddressingModes.Direct:
                case CPU.AddressingModes.DirectIndirect:
                    return "(" + param1 + ")";
                case CPU.AddressingModes.DirectIndexedX:
                    return param1 + ", x";
                case CPU.AddressingModes.DirectIndexedY:
                    return param1 + ", y";
                case CPU.AddressingModes.DirectIndexedIndirect:
                    return "(" + param1 + ", x)";
                case CPU.AddressingModes.DirectIndirectIndexed:
                    return "(" + param1 + "), y";
                case CPU.AddressingModes.DirectIndirectLong:
                    return "[" + param1 + "]";
                case CPU.AddressingModes.DirectIndirectIndexedLong:
                    return "[" + param1 + "], y";
                case CPU.AddressingModes.Absolute:
                    return param1;
                case CPU.AddressingModes.AbsoluteIndexedX:
                    return param1 + ", x";
                case CPU.AddressingModes.AbsoluteIndexedY:
                    return param1 + ", y";
                case CPU.AddressingModes.AbsoluteLong:
                    return param1;
                case CPU.AddressingModes.AbsoluteIndexedLong:
                    return param1 + ", x";
                case CPU.AddressingModes.StackRelative:
                    return param1 + ", s";
                case CPU.AddressingModes.StackRelativeIndirectIndexed:
                    return "(" + param1 + ", s), y";
                case CPU.AddressingModes.AbsoluteIndirect:
                    return "(" + param1 + ")";
                case CPU.AddressingModes.AbsoluteIndirectLong:
                    return "[" + param1 + "]";
                case CPU.AddressingModes.AbsoluteIndexedIndirect:
                    return "(" + param1 + ", x)";
                case CPU.AddressingModes.ImpliedAccumulator:
                    return param1;
                case CPU.AddressingModes.BlockMove:
                    return param1 + ", " + param2;
                default:
                    return param1;
            }
        }

        public virtual string StringRepresentation(int param1 = 0, int param2 = 0)
        {
            return String.Format("{0} {1}", code.ToString(), HaveArgs ? PrintParameter(param1.ToString(), param2.ToString()) : String.Empty);
        }

        public abstract void Run(int arg1, int arg2);

        public byte DecodeByteArgument(MemoryBin bin, ref int offset)
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

        /// <summary>
        /// Décode les arguments nécessaires pour l'instruction
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="mode"></param>
        /// <param name="offset"></param>
        /// <param name="instructionReference"></param>
        public abstract void DecodeArguments(MemoryBin bin, MFlagMode mode, ref int offset, ref InstructionReference instructionReference);
    }
}
