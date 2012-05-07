using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware.Instructions.InstructionsSets
{
    public class InstructionPush : Instruction
    {
        public enum PushType
        {
            PEA,
            PEI,
            PER,
            PHA,
            PHB,
            PHD,
            PHK,
            PHP,
            PHX,
            PHY
        }

        private PushType pushType;

        public PushType CurrentPushType
        {
            get { return pushType; }
        }

        public InstructionPush(CPU cpu, PushType pushType)
            : base(cpu, ParsePushOpCode(pushType), ParsePushAdrType(pushType))
        {
            this.pushType = pushType;
        }

        private static Hardware.CPU.AddressingModes ParsePushAdrType(PushType pushType)
        {
            switch (pushType)
            {
                default:
                case PushType.PEA:
                    return CPU.AddressingModes.Absolute;
                case PushType.PEI:
                    return CPU.AddressingModes.DirectIndirect;
                case PushType.PER:
                    return CPU.AddressingModes.RelativeLong;
                case PushType.PHA:
                case PushType.PHB:
                case PushType.PHD:
                case PushType.PHK:
                case PushType.PHP:
                case PushType.PHX:
                case PushType.PHY:
                    return CPU.AddressingModes.StackRelative;
            }
        }

        private static Hardware.CPU.Opcodes ParsePushOpCode(PushType pushType)
        {
            switch (pushType)
            {
                default:
                case PushType.PEA:
                    return CPU.Opcodes.PEA;
                case PushType.PEI:
                    return CPU.Opcodes.PEI;
                case PushType.PER:
                    return CPU.Opcodes.PER;
                case PushType.PHA:
                    return CPU.Opcodes.PHA;
                case PushType.PHB:
                    return CPU.Opcodes.PHB;
                case PushType.PHD:
                    return CPU.Opcodes.PHD;
                case PushType.PHK:
                    return CPU.Opcodes.PHK;
                case PushType.PHP:
                    return CPU.Opcodes.PHP;
                case PushType.PHX:
                    return CPU.Opcodes.PHX;
                case PushType.PHY:
                    return CPU.Opcodes.PHY;
            }
        }

        public override bool HaveArgs
        {
            get
            {
                switch (pushType)
                {
                    case PushType.PEA:
                    case PushType.PEI:
                    case PushType.PER:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public override void Run(int arg1, int arg2)
        {
            throw new NotImplementedException();
        }

        public override void DecodeArguments(Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref InstructionReference instructionReference)
        {
            switch (pushType)
            {
                case PushType.PEA:
                    {
                        instructionReference.param1 = DecodeInt2Argument(bin, ref offset);
                        break;
                    }
                case PushType.PEI:
                    {
                        instructionReference.param1 = DecodeByteArgument(bin, ref offset);
                        break;
                    }
                case PushType.PER:
                    {
                        instructionReference.param1 = DecodeInt2Argument(bin, ref offset);
                        break;
                    }
            }
        }
    }
}
