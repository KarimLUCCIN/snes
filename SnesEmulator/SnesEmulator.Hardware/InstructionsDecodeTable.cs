using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnesEmulator.Hardware.Instructions.InstructionsSets;
using SnesEmulator.Hardware.Instructions;

namespace SnesEmulator.Hardware
{
    /// <summary>
    /// Table de toutes les instructions connues ... XD
    /// </summary>
    public class InstructionsDecodeTable
    {
        private CPU cpu;

        public CPU CPU
        {
            get { return cpu; }
        }

        public Instruction[] KnownInstructions { get; private set; }
        
        public InstructionsDecodeTable(CPU cpu)
        {
            if (cpu == null)
                throw new ArgumentNullException("cpu");

            this.cpu = cpu;

            KnownInstructions = new Instruction[256];

            LoadKnownInstructions();

            /* Pour toutes les instructions qu'on ne connait pas ... */
            for (int i = 0; i < 256; i++)
            {
                if (KnownInstructions[i] == null)
                    KnownInstructions[i] = new InstructionInvalid(cpu, i);
            }
        }

        public enum ArgumentType : byte
        {
            None,
            I1,
            I2,
            I3
        }

        public Instruction GenericInstCustom(CPU.Opcodes code, CPU.AddressingModes addrMode, Action<GenericInstruction, int, int> exec, GenericInstruction.DecodeArgumentsFunctionDelegate decodeDelegate = null)
        {
            return new GenericInstruction(cpu, code, addrMode, decodeDelegate != null) { runFunction = exec, decodeArgumentsFunction = decodeDelegate };
        }

        public Instruction GenericInst(CPU.Opcodes code, CPU.AddressingModes addrMode, Action<GenericInstruction, int, int> exec, ArgumentType param1type = ArgumentType.None, ArgumentType param2type = ArgumentType.None)
        {
            var inst = new GenericInstruction(cpu, code, addrMode, param1type != ArgumentType.None) { runFunction = exec };

            if (param1type != ArgumentType.None)
            {
                inst.decodeArgumentsFunction = delegate(GenericInstruction sender, Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref int param1, ref int param2)
                {
                    switch (param1type)
                    {
                        case ArgumentType.None:
                            break;
                        default:
                        case ArgumentType.I1:
                            param1 = inst.DecodeByteArgument(bin, ref offset);
                            break;
                        case ArgumentType.I2:
                            param1 = inst.DecodeInt2Argument(bin, ref offset);
                            break;
                        case ArgumentType.I3:
                            param1 = inst.DecodeInt3Argument(bin, ref offset);
                            break;
                    }

                    switch (param2type)
                    {
                        case ArgumentType.None:
                            break;
                        default:
                        case ArgumentType.I1:
                            param2 = inst.DecodeByteArgument(bin, ref offset);
                            break;
                        case ArgumentType.I2:
                            param2 = inst.DecodeInt2Argument(bin, ref offset);
                            break;
                        case ArgumentType.I3:
                            param2 = inst.DecodeInt3Argument(bin, ref offset);
                            break;
                    }
                };
            }

            return inst;
        }

        private void LoadKnownInstructions()
        {
            // ADC
            KnownInstructions[0x61] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.DirectIndexedIndirect);
            KnownInstructions[0x63] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.StackRelative);
            KnownInstructions[0x65] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.Direct);
            KnownInstructions[0x67] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.DirectIndirectLong);
            KnownInstructions[0x69] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.ImmediateMemoryFlag);
            KnownInstructions[0x6D] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.Absolute);
            KnownInstructions[0x6F] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.AbsoluteLong);
            KnownInstructions[0x71] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.DirectIndirectIndexed);
            KnownInstructions[0x72] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.DirectIndirect);
            KnownInstructions[0x73] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.StackRelativeIndirectIndexed);
            KnownInstructions[0x75] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.DirectIndexedX);
            KnownInstructions[0x77] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.DirectIndirectIndexedLong);
            KnownInstructions[0x79] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.AbsoluteIndexedY);
            KnownInstructions[0x7D] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.AbsoluteIndexedX);
            KnownInstructions[0x7F] = new InstructionADC(cpu, Hardware.CPU.AddressingModes.AbsoluteIndexedLong);

            // AND
            KnownInstructions[0x21] = GenericInst(CPU.Opcodes.AND, CPU.AddressingModes.DirectIndexedIndirect, (sender, a, b) => { }, ArgumentType.I1);
            KnownInstructions[0x23] = GenericInst(CPU.Opcodes.AND, CPU.AddressingModes.StackRelative, (sender, a, b) => { }, ArgumentType.I1);
            KnownInstructions[0x25] = GenericInst(CPU.Opcodes.AND, CPU.AddressingModes.Direct, (sender, a, b) => { }, ArgumentType.I1);
            KnownInstructions[0x27] = GenericInst(CPU.Opcodes.AND, CPU.AddressingModes.DirectIndirectLong, (sender, a, b) => { }, ArgumentType.I1);
            KnownInstructions[0x29] = GenericInstCustom(CPU.Opcodes.AND, CPU.AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref int p1, ref int p2)
            {
                if (mode == MFlagMode.Mode16Bits)
                    p1 = sender.DecodeInt3Argument(bin, ref offset);
                else
                    p1 = sender.DecodeInt2Argument(bin, ref offset);
            });
            KnownInstructions[0x2D] = GenericInst(CPU.Opcodes.AND, CPU.AddressingModes.Absolute, (sender, a, b) => { }, ArgumentType.I2);
            KnownInstructions[0x2F] = GenericInst(CPU.Opcodes.AND, CPU.AddressingModes.AbsoluteLong, (sender, a, b) => { }, ArgumentType.I3);
            KnownInstructions[0x31] = GenericInst(CPU.Opcodes.AND, CPU.AddressingModes.DirectIndirectIndexed, (sender, a, b) => { }, ArgumentType.I1);
            KnownInstructions[0x32] = GenericInst(CPU.Opcodes.AND, CPU.AddressingModes.DirectIndirect, (sender, a, b) => { }, ArgumentType.I1);
            KnownInstructions[0x33] = GenericInst(CPU.Opcodes.AND, CPU.AddressingModes.StackRelativeIndirectIndexed, (sender, a, b) => { }, ArgumentType.I1);
            KnownInstructions[0x35] = GenericInst(CPU.Opcodes.AND, CPU.AddressingModes.DirectIndexedX, (sender, a, b) => { }, ArgumentType.I1);
            KnownInstructions[0x37] = GenericInst(CPU.Opcodes.AND, CPU.AddressingModes.DirectIndirectIndexedLong, (sender, a, b) => { }, ArgumentType.I1);
            KnownInstructions[0x39] = GenericInst(CPU.Opcodes.AND, CPU.AddressingModes.AbsoluteIndexedY, (sender, a, b) => { }, ArgumentType.I2);
            KnownInstructions[0x3D] = GenericInst(CPU.Opcodes.AND, CPU.AddressingModes.AbsoluteIndexedX, (sender, a, b) => { }, ArgumentType.I2);
            KnownInstructions[0x3F] = GenericInst(CPU.Opcodes.AND, CPU.AddressingModes.AbsoluteIndexedLong, (sender, a, b) => { }, ArgumentType.I3);

            // JMP
            KnownInstructions[0x4c] = new InstructionJMP(cpu, Hardware.CPU.AddressingModes.Absolute);
            KnownInstructions[0x5c] = new InstructionJMP(cpu, Hardware.CPU.AddressingModes.AbsoluteLong);
            KnownInstructions[0x6c] = new InstructionJMP(cpu, Hardware.CPU.AddressingModes.AbsoluteIndirect);
            KnownInstructions[0x7c] = new InstructionJMP(cpu, Hardware.CPU.AddressingModes.AbsoluteIndexedIndirect);
            KnownInstructions[0xDc] = new InstructionJMP(cpu, Hardware.CPU.AddressingModes.AbsoluteIndirectLong);

            // PUSH
            KnownInstructions[0xF4] = new InstructionPush(cpu, InstructionPush.PushType.PEA);
            KnownInstructions[0xD4] = new InstructionPush(cpu, InstructionPush.PushType.PEI);
            KnownInstructions[0x62] = new InstructionPush(cpu, InstructionPush.PushType.PER);
            KnownInstructions[0x48] = new InstructionPush(cpu, InstructionPush.PushType.PHA);
            KnownInstructions[0x8B] = new InstructionPush(cpu, InstructionPush.PushType.PHB);
            KnownInstructions[0x0B] = new InstructionPush(cpu, InstructionPush.PushType.PHD);
            KnownInstructions[0x4B] = new InstructionPush(cpu, InstructionPush.PushType.PHK);
            KnownInstructions[0x08] = new InstructionPush(cpu, InstructionPush.PushType.PHP);
            KnownInstructions[0xDA] = new InstructionPush(cpu, InstructionPush.PushType.PHX);
            KnownInstructions[0x5A] = new InstructionPush(cpu, InstructionPush.PushType.PHY);

            // BRK
            KnownInstructions[0x00] = new InstructionBRK(cpu);

            // NOP
            KnownInstructions[0xEA] = new InstructionNOP(cpu);

            // SEI
            KnownInstructions[0x78] = GenericInst(Hardware.CPU.Opcodes.SEI, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // TYX
            KnownInstructions[0xBB] = GenericInst(Hardware.CPU.Opcodes.TYX, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // WAI
            KnownInstructions[0xCB] = GenericInst(Hardware.CPU.Opcodes.WAI, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // WDM (not used)
            KnownInstructions[0x42] = new InstructionWDM(cpu);

            // XBA
            KnownInstructions[0xeb] = new InstructionXBA(cpu);

            // XCE
            KnownInstructions[0xfb] = new InstructionXCE(cpu);
        }
    }
}
