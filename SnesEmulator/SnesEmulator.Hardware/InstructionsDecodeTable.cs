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

            // ASL
            KnownInstructions[0x06] = GenericInst(Hardware.CPU.Opcodes.ASL, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x0A] = GenericInst(Hardware.CPU.Opcodes.ASL, Hardware.CPU.AddressingModes.ImpliedAccumulator, (sender, p1, p2) => { });
            KnownInstructions[0x0E] = GenericInst(Hardware.CPU.Opcodes.ASL, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x16] = GenericInst(Hardware.CPU.Opcodes.ASL, Hardware.CPU.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x1E] = GenericInst(Hardware.CPU.Opcodes.ASL, Hardware.CPU.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2);

            // BRANCH (1)
            KnownInstructions[0x90] = GenericInst(Hardware.CPU.Opcodes.BCC, Hardware.CPU.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xB0] = GenericInst(Hardware.CPU.Opcodes.BCS, Hardware.CPU.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xF0] = GenericInst(Hardware.CPU.Opcodes.BEQ, Hardware.CPU.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1);

            // TESTS
            KnownInstructions[0x24] = GenericInst(Hardware.CPU.Opcodes.BIT, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x2C] = GenericInst(Hardware.CPU.Opcodes.BIT, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x34] = GenericInst(Hardware.CPU.Opcodes.BIT, Hardware.CPU.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x3C] = GenericInst(Hardware.CPU.Opcodes.BIT, Hardware.CPU.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x89] = GenericInstCustom(CPU.Opcodes.BIT, CPU.AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref int p1, ref int p2)
                {
                    if (mode == MFlagMode.Mode16Bits)
                        p1 = sender.DecodeInt3Argument(bin, ref offset);
                    else
                        p1 = sender.DecodeInt2Argument(bin, ref offset);
                });

            // BRANCH (2)
            KnownInstructions[0x30] = GenericInst(Hardware.CPU.Opcodes.BMI, Hardware.CPU.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xD0] = GenericInst(Hardware.CPU.Opcodes.BNE, Hardware.CPU.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x10] = GenericInst(Hardware.CPU.Opcodes.BPL, Hardware.CPU.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x80] = GenericInst(Hardware.CPU.Opcodes.BRA, Hardware.CPU.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1);

            // BRK
            KnownInstructions[0x00] = new InstructionBRK(cpu);

            // BRANCH (3)
            KnownInstructions[0x82] = GenericInst(Hardware.CPU.Opcodes.BRL, Hardware.CPU.AddressingModes.RelativeLong, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x50] = GenericInst(Hardware.CPU.Opcodes.BVC, Hardware.CPU.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x70] = GenericInst(Hardware.CPU.Opcodes.BVS, Hardware.CPU.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1);
          
            // CLEAR
            KnownInstructions[0x18] = GenericInst(Hardware.CPU.Opcodes.CLC, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });
            KnownInstructions[0xD8] = GenericInst(Hardware.CPU.Opcodes.CLD, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });
            KnownInstructions[0x58] = GenericInst(Hardware.CPU.Opcodes.CLI, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });
            KnownInstructions[0xB8] = GenericInst(Hardware.CPU.Opcodes.CLV, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // CMP
            KnownInstructions[0xC1] = GenericInst(Hardware.CPU.Opcodes.CMP, Hardware.CPU.AddressingModes.DirectIndexedIndirect, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xC3] = GenericInst(Hardware.CPU.Opcodes.CMP, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xC5] = GenericInst(Hardware.CPU.Opcodes.CMP, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xC7] = GenericInst(Hardware.CPU.Opcodes.CMP, Hardware.CPU.AddressingModes.DirectIndirectLong, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xC9] = GenericInstCustom(CPU.Opcodes.CMP, CPU.AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref int p1, ref int p2)
                {
                    if (mode == MFlagMode.Mode16Bits)
                        p1 = sender.DecodeInt3Argument(bin, ref offset);
                    else
                        p1 = sender.DecodeInt2Argument(bin, ref offset);
                });
            KnownInstructions[0xCD] = GenericInst(Hardware.CPU.Opcodes.CMP, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0xCF] = GenericInst(Hardware.CPU.Opcodes.CMP, Hardware.CPU.AddressingModes.AbsoluteLong, (sender, p1, p2) => { }, ArgumentType.I3);
            KnownInstructions[0xD1] = GenericInst(Hardware.CPU.Opcodes.CMP, Hardware.CPU.AddressingModes.DirectIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xD2] = GenericInst(Hardware.CPU.Opcodes.CMP, Hardware.CPU.AddressingModes.DirectIndirect, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xD3] = GenericInst(Hardware.CPU.Opcodes.CMP, Hardware.CPU.AddressingModes.StackRelativeIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xD5] = GenericInst(Hardware.CPU.Opcodes.CMP, Hardware.CPU.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xD7] = GenericInst(Hardware.CPU.Opcodes.CMP, Hardware.CPU.AddressingModes.DirectIndirectIndexedLong, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xD9] = GenericInst(Hardware.CPU.Opcodes.CMP, Hardware.CPU.AddressingModes.AbsoluteIndexedY, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0xDD] = GenericInst(Hardware.CPU.Opcodes.CMP, Hardware.CPU.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0xDF] = GenericInst(Hardware.CPU.Opcodes.CMP, Hardware.CPU.AddressingModes.AbsoluteIndexedLong, (sender, p1, p2) => { }, ArgumentType.I3);

            // COP
            KnownInstructions[0x02] = GenericInstCustom(Hardware.CPU.Opcodes.COP, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref int p1, ref int p2)
                {
                    /* on skip le byte d'après */
                    offset++;

                    p1 = sender.DecodeByteArgument(bin, ref offset);
                });

            // ---- MISSING CPX and CPY

            // DEC
            KnownInstructions[0x3A] = GenericInst(Hardware.CPU.Opcodes.DEC, Hardware.CPU.AddressingModes.ImpliedAccumulator, (sender, p1, p2) => { });
            KnownInstructions[0xC6] = GenericInst(Hardware.CPU.Opcodes.DEC, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xCE] = GenericInst(Hardware.CPU.Opcodes.DEC, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0xD6] = GenericInst(Hardware.CPU.Opcodes.DEC, Hardware.CPU.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xDE] = GenericInst(Hardware.CPU.Opcodes.DEC, Hardware.CPU.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2);

            // DEX & DEY
            KnownInstructions[0xCA] = GenericInst(Hardware.CPU.Opcodes.DEX, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });
            KnownInstructions[0x88] = GenericInst(Hardware.CPU.Opcodes.DEY, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // EOR
            KnownInstructions[0x41] = GenericInst(Hardware.CPU.Opcodes.EOR, Hardware.CPU.AddressingModes.DirectIndexedIndirect, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x43] = GenericInst(Hardware.CPU.Opcodes.EOR, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x45] = GenericInst(Hardware.CPU.Opcodes.EOR, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x47] = GenericInst(Hardware.CPU.Opcodes.EOR, Hardware.CPU.AddressingModes.DirectIndirectLong, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x49] = GenericInstCustom(CPU.Opcodes.EOR, CPU.AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref int p1, ref int p2)
                {
                    if (mode == MFlagMode.Mode16Bits)
                        p1 = sender.DecodeInt3Argument(bin, ref offset);
                    else
                        p1 = sender.DecodeInt2Argument(bin, ref offset);
                });
            KnownInstructions[0x4D] = GenericInst(Hardware.CPU.Opcodes.EOR, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x4F] = GenericInst(Hardware.CPU.Opcodes.EOR, Hardware.CPU.AddressingModes.AbsoluteLong, (sender, p1, p2) => { }, ArgumentType.I3);
            KnownInstructions[0x51] = GenericInst(Hardware.CPU.Opcodes.EOR, Hardware.CPU.AddressingModes.DirectIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x52] = GenericInst(Hardware.CPU.Opcodes.EOR, Hardware.CPU.AddressingModes.DirectIndirect, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x53] = GenericInst(Hardware.CPU.Opcodes.EOR, Hardware.CPU.AddressingModes.StackRelativeIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x55] = GenericInst(Hardware.CPU.Opcodes.EOR, Hardware.CPU.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x57] = GenericInst(Hardware.CPU.Opcodes.EOR, Hardware.CPU.AddressingModes.DirectIndirectIndexedLong, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x59] = GenericInst(Hardware.CPU.Opcodes.EOR, Hardware.CPU.AddressingModes.AbsoluteIndexedY, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x5D] = GenericInst(Hardware.CPU.Opcodes.EOR, Hardware.CPU.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x5F] = GenericInst(Hardware.CPU.Opcodes.EOR, Hardware.CPU.AddressingModes.AbsoluteIndexedLong, (sender, p1, p2) => { }, ArgumentType.I3);

            // INC
            KnownInstructions[0x1A] = GenericInst(Hardware.CPU.Opcodes.INC, Hardware.CPU.AddressingModes.ImpliedAccumulator, (sender, p1, p2) => { });
            KnownInstructions[0xE6] = GenericInst(Hardware.CPU.Opcodes.INC, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xEE] = GenericInst(Hardware.CPU.Opcodes.INC, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0xF6] = GenericInst(Hardware.CPU.Opcodes.INC, Hardware.CPU.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xFE] = GenericInst(Hardware.CPU.Opcodes.INC, Hardware.CPU.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2);

            // INX & INY
            KnownInstructions[0xE8] = GenericInst(Hardware.CPU.Opcodes.INX, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });
            KnownInstructions[0xC8] = GenericInst(Hardware.CPU.Opcodes.INY, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // JMP
            KnownInstructions[0x4c] = new InstructionJMP(cpu, Hardware.CPU.AddressingModes.Absolute);
            KnownInstructions[0x5c] = new InstructionJMP(cpu, Hardware.CPU.AddressingModes.AbsoluteLong);
            KnownInstructions[0x6c] = new InstructionJMP(cpu, Hardware.CPU.AddressingModes.AbsoluteIndirect);
            KnownInstructions[0x7c] = new InstructionJMP(cpu, Hardware.CPU.AddressingModes.AbsoluteIndexedIndirect);
            KnownInstructions[0xDc] = new InstructionJMP(cpu, Hardware.CPU.AddressingModes.AbsoluteIndirectLong);

            // JSR
            KnownInstructions[0x20] = GenericInst(Hardware.CPU.Opcodes.JSR, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x22] = GenericInst(Hardware.CPU.Opcodes.JSR, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I3);
            KnownInstructions[0xFC] = GenericInst(Hardware.CPU.Opcodes.JSR, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);

            // LDA
            KnownInstructions[0xA1] = GenericInst(Hardware.CPU.Opcodes.LDA, Hardware.CPU.AddressingModes.DirectIndexedIndirect, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xA3] = GenericInst(Hardware.CPU.Opcodes.LDA, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xA5] = GenericInst(Hardware.CPU.Opcodes.LDA, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xA7] = GenericInst(Hardware.CPU.Opcodes.LDA, Hardware.CPU.AddressingModes.DirectIndirectLong, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xA9] = GenericInstCustom(CPU.Opcodes.LDA, CPU.AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref int p1, ref int p2)
                {
                    if (mode == MFlagMode.Mode16Bits)
                        p1 = sender.DecodeInt3Argument(bin, ref offset);
                    else
                        p1 = sender.DecodeInt2Argument(bin, ref offset);
                });
            KnownInstructions[0xAD] = GenericInst(Hardware.CPU.Opcodes.LDA, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0xAF] = GenericInst(Hardware.CPU.Opcodes.LDA, Hardware.CPU.AddressingModes.AbsoluteLong, (sender, p1, p2) => { }, ArgumentType.I3);
            KnownInstructions[0xB1] = GenericInst(Hardware.CPU.Opcodes.LDA, Hardware.CPU.AddressingModes.DirectIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xB2] = GenericInst(Hardware.CPU.Opcodes.LDA, Hardware.CPU.AddressingModes.DirectIndirect, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xB3] = GenericInst(Hardware.CPU.Opcodes.LDA, Hardware.CPU.AddressingModes.StackRelativeIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xB5] = GenericInst(Hardware.CPU.Opcodes.LDA, Hardware.CPU.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xB7] = GenericInst(Hardware.CPU.Opcodes.LDA, Hardware.CPU.AddressingModes.DirectIndirectIndexedLong, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xB9] = GenericInst(Hardware.CPU.Opcodes.LDA, Hardware.CPU.AddressingModes.AbsoluteIndexedY, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0xBD] = GenericInst(Hardware.CPU.Opcodes.LDA, Hardware.CPU.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0xBF] = GenericInst(Hardware.CPU.Opcodes.LDA, Hardware.CPU.AddressingModes.AbsoluteIndexedLong, (sender, p1, p2) => { }, ArgumentType.I3);

            // ---- MISSING LDX and LDY

            // LSR
            KnownInstructions[0x46] = GenericInst(Hardware.CPU.Opcodes.LSR, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x4A] = GenericInst(Hardware.CPU.Opcodes.LSR, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { });
            KnownInstructions[0x4E] = GenericInst(Hardware.CPU.Opcodes.LSR, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x56] = GenericInst(Hardware.CPU.Opcodes.LSR, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x5E] = GenericInst(Hardware.CPU.Opcodes.LSR, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I2);

            // BLOCK MOVE
            KnownInstructions[0x54] = GenericInst(Hardware.CPU.Opcodes.MVN, Hardware.CPU.AddressingModes.BlockMove, (sender, p1, p2) => { }, ArgumentType.I1, ArgumentType.I1);
            KnownInstructions[0x44] = GenericInst(Hardware.CPU.Opcodes.MVP, Hardware.CPU.AddressingModes.BlockMove, (sender, p1, p2) => { }, ArgumentType.I1, ArgumentType.I1);

            // NOP
            KnownInstructions[0xEA] = new InstructionNOP(cpu);

            // ORA
            KnownInstructions[0x01] = GenericInst(Hardware.CPU.Opcodes.ORA, Hardware.CPU.AddressingModes.DirectIndexedIndirect, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x03] = GenericInst(Hardware.CPU.Opcodes.ORA, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x05] = GenericInst(Hardware.CPU.Opcodes.ORA, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x07] = GenericInst(Hardware.CPU.Opcodes.ORA, Hardware.CPU.AddressingModes.DirectIndirectLong, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x09] = GenericInstCustom(CPU.Opcodes.ORA, CPU.AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref int p1, ref int p2)
                {
                    if (mode == MFlagMode.Mode16Bits)
                        p1 = sender.DecodeInt3Argument(bin, ref offset);
                    else
                        p1 = sender.DecodeInt2Argument(bin, ref offset);
                });
            KnownInstructions[0x0D] = GenericInst(Hardware.CPU.Opcodes.ORA, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x1F] = GenericInst(Hardware.CPU.Opcodes.ORA, Hardware.CPU.AddressingModes.AbsoluteLong, (sender, p1, p2) => { }, ArgumentType.I3);
            KnownInstructions[0x11] = GenericInst(Hardware.CPU.Opcodes.ORA, Hardware.CPU.AddressingModes.DirectIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x12] = GenericInst(Hardware.CPU.Opcodes.ORA, Hardware.CPU.AddressingModes.DirectIndirect, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x13] = GenericInst(Hardware.CPU.Opcodes.ORA, Hardware.CPU.AddressingModes.StackRelativeIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x15] = GenericInst(Hardware.CPU.Opcodes.ORA, Hardware.CPU.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x17] = GenericInst(Hardware.CPU.Opcodes.ORA, Hardware.CPU.AddressingModes.DirectIndirectIndexedLong, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x19] = GenericInst(Hardware.CPU.Opcodes.ORA, Hardware.CPU.AddressingModes.AbsoluteIndexedY, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x1D] = GenericInst(Hardware.CPU.Opcodes.ORA, Hardware.CPU.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x1F] = GenericInst(Hardware.CPU.Opcodes.ORA, Hardware.CPU.AddressingModes.AbsoluteIndexedLong, (sender, p1, p2) => { }, ArgumentType.I3);

            // PUSH
            KnownInstructions[0xF4] = GenericInst(Hardware.CPU.Opcodes.PEA, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0xD4] = GenericInst(Hardware.CPU.Opcodes.PEI, Hardware.CPU.AddressingModes.DirectIndirect, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x62] = GenericInst(Hardware.CPU.Opcodes.PER, Hardware.CPU.AddressingModes.RelativeLong, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x48] = GenericInst(Hardware.CPU.Opcodes.PHA, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });
            KnownInstructions[0x8B] = GenericInst(Hardware.CPU.Opcodes.PHB, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });
            KnownInstructions[0x0B] = GenericInst(Hardware.CPU.Opcodes.PHD, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });
            KnownInstructions[0x4B] = GenericInst(Hardware.CPU.Opcodes.PHK, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });
            KnownInstructions[0x08] = GenericInst(Hardware.CPU.Opcodes.PHP, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });
            KnownInstructions[0xDA] = GenericInst(Hardware.CPU.Opcodes.PHX, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });
            KnownInstructions[0x5A] = GenericInst(Hardware.CPU.Opcodes.PHY, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });

            // PULL
            KnownInstructions[0x68] = GenericInst(Hardware.CPU.Opcodes.PLA, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });
            KnownInstructions[0xAB] = GenericInst(Hardware.CPU.Opcodes.PLB, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });
            KnownInstructions[0x2B] = GenericInst(Hardware.CPU.Opcodes.PLD, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });
            KnownInstructions[0x28] = GenericInst(Hardware.CPU.Opcodes.PLP, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });
            KnownInstructions[0xFA] = GenericInst(Hardware.CPU.Opcodes.PLX, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });
            KnownInstructions[0x7A] = GenericInst(Hardware.CPU.Opcodes.PLY, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });

            // REP
            KnownInstructions[0xC2] = GenericInst(Hardware.CPU.Opcodes.REP, Hardware.CPU.AddressingModes.ImmediateMemoryFlag, (sender, p1, p2) => { });

            // ROL
            KnownInstructions[0x26] = GenericInst(Hardware.CPU.Opcodes.ROL, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x2A] = GenericInst(Hardware.CPU.Opcodes.ROL, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { });
            KnownInstructions[0x2E] = GenericInst(Hardware.CPU.Opcodes.ROL, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x36] = GenericInst(Hardware.CPU.Opcodes.ROL, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x3E] = GenericInst(Hardware.CPU.Opcodes.ROL, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I2);
        
            // ROR
            KnownInstructions[0x66] = GenericInst(Hardware.CPU.Opcodes.ROR, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x6A] = GenericInst(Hardware.CPU.Opcodes.ROR, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { });
            KnownInstructions[0x6E] = GenericInst(Hardware.CPU.Opcodes.ROR, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x76] = GenericInst(Hardware.CPU.Opcodes.ROR, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x7E] = GenericInst(Hardware.CPU.Opcodes.ROR, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I2);

            // RETURNS
            KnownInstructions[0x40] = GenericInst(Hardware.CPU.Opcodes.RTI, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });
            KnownInstructions[0x6B] = GenericInst(Hardware.CPU.Opcodes.RTL, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });
            KnownInstructions[0x60] = GenericInst(Hardware.CPU.Opcodes.RTS, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { });

            // SBC
            KnownInstructions[0xE1] = GenericInst(Hardware.CPU.Opcodes.SBC, Hardware.CPU.AddressingModes.DirectIndexedIndirect, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xE3] = GenericInst(Hardware.CPU.Opcodes.SBC, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xE5] = GenericInst(Hardware.CPU.Opcodes.SBC, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xE7] = GenericInst(Hardware.CPU.Opcodes.SBC, Hardware.CPU.AddressingModes.DirectIndirectLong, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xE9] = GenericInstCustom(CPU.Opcodes.SBC, CPU.AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref int p1, ref int p2)
                {
                    if (mode == MFlagMode.Mode16Bits)
                        p1 = sender.DecodeInt3Argument(bin, ref offset);
                    else
                        p1 = sender.DecodeInt2Argument(bin, ref offset);
                });
            KnownInstructions[0xED] = GenericInst(Hardware.CPU.Opcodes.SBC, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0xEF] = GenericInst(Hardware.CPU.Opcodes.SBC, Hardware.CPU.AddressingModes.AbsoluteLong, (sender, p1, p2) => { }, ArgumentType.I3);
            KnownInstructions[0xF1] = GenericInst(Hardware.CPU.Opcodes.SBC, Hardware.CPU.AddressingModes.DirectIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xF2] = GenericInst(Hardware.CPU.Opcodes.SBC, Hardware.CPU.AddressingModes.DirectIndirect, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xF3] = GenericInst(Hardware.CPU.Opcodes.SBC, Hardware.CPU.AddressingModes.StackRelativeIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xF5] = GenericInst(Hardware.CPU.Opcodes.SBC, Hardware.CPU.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xF7] = GenericInst(Hardware.CPU.Opcodes.SBC, Hardware.CPU.AddressingModes.DirectIndirectIndexedLong, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0xF9] = GenericInst(Hardware.CPU.Opcodes.SBC, Hardware.CPU.AddressingModes.AbsoluteIndexedY, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0xFD] = GenericInst(Hardware.CPU.Opcodes.SBC, Hardware.CPU.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0xFF] = GenericInst(Hardware.CPU.Opcodes.SBC, Hardware.CPU.AddressingModes.AbsoluteIndexedLong, (sender, p1, p2) => { }, ArgumentType.I3);
            
            // SEC
            KnownInstructions[0x38] = GenericInst(Hardware.CPU.Opcodes.SEC, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // SED
            KnownInstructions[0xF8] = GenericInst(Hardware.CPU.Opcodes.SED, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // SEI
            KnownInstructions[0x78] = GenericInst(Hardware.CPU.Opcodes.SEI, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // SEP
            KnownInstructions[0xE2] = GenericInst(Hardware.CPU.Opcodes.SEP, Hardware.CPU.AddressingModes.ImmediateMemoryFlag, (sender, p1, p2) => { }, ArgumentType.I1);

            // STA
            KnownInstructions[0x81] = GenericInst(Hardware.CPU.Opcodes.STA, Hardware.CPU.AddressingModes.DirectIndexedIndirect, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x83] = GenericInst(Hardware.CPU.Opcodes.STA, Hardware.CPU.AddressingModes.StackRelative, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x85] = GenericInst(Hardware.CPU.Opcodes.STA, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x87] = GenericInst(Hardware.CPU.Opcodes.STA, Hardware.CPU.AddressingModes.DirectIndirectLong, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x89] = GenericInstCustom(CPU.Opcodes.STA, CPU.AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref int p1, ref int p2)
                {
                    if (mode == MFlagMode.Mode16Bits)
                        p1 = sender.DecodeInt3Argument(bin, ref offset);
                    else
                        p1 = sender.DecodeInt2Argument(bin, ref offset);
                });
            KnownInstructions[0x8D] = GenericInst(Hardware.CPU.Opcodes.STA, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x8F] = GenericInst(Hardware.CPU.Opcodes.STA, Hardware.CPU.AddressingModes.AbsoluteLong, (sender, p1, p2) => { }, ArgumentType.I3);
            KnownInstructions[0x91] = GenericInst(Hardware.CPU.Opcodes.STA, Hardware.CPU.AddressingModes.DirectIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x92] = GenericInst(Hardware.CPU.Opcodes.STA, Hardware.CPU.AddressingModes.DirectIndirect, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x93] = GenericInst(Hardware.CPU.Opcodes.STA, Hardware.CPU.AddressingModes.StackRelativeIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x95] = GenericInst(Hardware.CPU.Opcodes.STA, Hardware.CPU.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x97] = GenericInst(Hardware.CPU.Opcodes.STA, Hardware.CPU.AddressingModes.DirectIndirectIndexedLong, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x99] = GenericInst(Hardware.CPU.Opcodes.STA, Hardware.CPU.AddressingModes.AbsoluteIndexedY, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x9D] = GenericInst(Hardware.CPU.Opcodes.STA, Hardware.CPU.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x9F] = GenericInst(Hardware.CPU.Opcodes.STA, Hardware.CPU.AddressingModes.AbsoluteIndexedLong, (sender, p1, p2) => { }, ArgumentType.I3);

            // STP
            KnownInstructions[0xDB] = GenericInst(Hardware.CPU.Opcodes.STP, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // STX
            KnownInstructions[0x86] = GenericInst(Hardware.CPU.Opcodes.STX, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x8E] = GenericInst(Hardware.CPU.Opcodes.STX, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x96] = GenericInst(Hardware.CPU.Opcodes.STX, Hardware.CPU.AddressingModes.DirectIndexedY, (sender, p1, p2) => { }, ArgumentType.I1);

            // STY
            KnownInstructions[0x84] = GenericInst(Hardware.CPU.Opcodes.STY, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x8C] = GenericInst(Hardware.CPU.Opcodes.STY, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x94] = GenericInst(Hardware.CPU.Opcodes.STY, Hardware.CPU.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1);

            // STZ
            KnownInstructions[0x64] = GenericInst(Hardware.CPU.Opcodes.STZ, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x74] = GenericInst(Hardware.CPU.Opcodes.STZ, Hardware.CPU.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x9C] = GenericInst(Hardware.CPU.Opcodes.STZ, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);
            KnownInstructions[0x9E] = GenericInst(Hardware.CPU.Opcodes.STZ, Hardware.CPU.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2);

            // TAX
            KnownInstructions[0xAA] = GenericInst(Hardware.CPU.Opcodes.TAX, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // TAY
            KnownInstructions[0xA8] = GenericInst(Hardware.CPU.Opcodes.TAY, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // TCD
            KnownInstructions[0x5B] = GenericInst(Hardware.CPU.Opcodes.TCD, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });
           
            // TCS
            KnownInstructions[0x1B] = GenericInst(Hardware.CPU.Opcodes.TCS, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });
           
            // TDC
            KnownInstructions[0x7B] = GenericInst(Hardware.CPU.Opcodes.TDC, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // TRB
            KnownInstructions[0x14] = GenericInst(Hardware.CPU.Opcodes.TRB, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x1C] = GenericInst(Hardware.CPU.Opcodes.TRB, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);

            // TSB
            KnownInstructions[0x04] = GenericInst(Hardware.CPU.Opcodes.TSB, Hardware.CPU.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1);
            KnownInstructions[0x0C] = GenericInst(Hardware.CPU.Opcodes.TSB, Hardware.CPU.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2);

            // TSC
            KnownInstructions[0x3B] = GenericInst(Hardware.CPU.Opcodes.TSC, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // TSX
            KnownInstructions[0xBA] = GenericInst(Hardware.CPU.Opcodes.TSX, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

            // TXA
            KnownInstructions[0x8A] = GenericInst(Hardware.CPU.Opcodes.TXA, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });
                      
            // TXS
            KnownInstructions[0x9A] = GenericInst(Hardware.CPU.Opcodes.TXS, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });
           
            // TXY
            KnownInstructions[0x9B] = GenericInst(Hardware.CPU.Opcodes.TXY, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });
            
            // TYA
            KnownInstructions[0x98] = GenericInst(Hardware.CPU.Opcodes.TYA, Hardware.CPU.AddressingModes.Implied, (sender, p1, p2) => { });

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
