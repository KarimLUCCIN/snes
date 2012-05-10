﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnesEmulator.Hardware.Instructions.InstructionsSets;
using SnesEmulator.Hardware.Instructions;
using System.Diagnostics;

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
                {
                    KnownInstructions[i] = new InstructionInvalid(cpu, i);
                    Debug.WriteLine(String.Format("No OpCode for : {0}", i.ToString("X")));
                }
            }
        }

        public enum ArgumentType : byte
        {
            None,
            I1,
            I2,
            I3
        }

        public Instruction GenericInstCustom(OpCodes code, AddressingModes addrMode, Action<GenericInstruction, int, int> exec, GenericInstruction.DecodeArgumentsFunctionDelegate decodeDelegate = null)
        {
            return new GenericInstruction(cpu, code, addrMode, decodeDelegate != null) { runFunction = exec, decodeArgumentsFunction = decodeDelegate };
        }

        public Instruction GenericInst(OpCodes code, AddressingModes addrMode, Action<GenericInstruction, int, int> exec, ArgumentType param1type = ArgumentType.None, ArgumentType param2type = ArgumentType.None)
        {
            var inst = new GenericInstruction(cpu, code, addrMode, param1type != ArgumentType.None) { runFunction = exec };

            if (param1type != ArgumentType.None)
            {
                inst.decodeArgumentsFunction = delegate(GenericInstruction sender, Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref int param1, ref int param2)
                {
                    switch (param1type)
                    {
                        case ArgumentType.None:
                            break;
                        default:
                        case ArgumentType.I1:
                            param1 = inst.DecodeInt1Argument(bin, ref offset);
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
                            param2 = inst.DecodeInt1Argument(bin, ref offset);
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

        private void RegisterKnownInstruction(int code, Instruction instruction)
        {
            if (instruction == null)
                throw new ArgumentNullException("instruction");
            else if (KnownInstructions[code] != null)
                throw new InvalidOperationException(String.Format("Une instruction avec le code {0} existe déjà", code.ToString("X")));
            else
                KnownInstructions[code] = instruction;
        }

        private void LoadKnownInstructions()
        {
            // ADC
            RegisterKnownInstruction(0x61, new InstructionADC(cpu, Hardware.AddressingModes.DirectIndexedIndirect));
            RegisterKnownInstruction(0x63, new InstructionADC(cpu, Hardware.AddressingModes.StackRelative));
            RegisterKnownInstruction(0x65, new InstructionADC(cpu, Hardware.AddressingModes.Direct));
            RegisterKnownInstruction(0x67, new InstructionADC(cpu, Hardware.AddressingModes.DirectIndirectLong));
            RegisterKnownInstruction(0x69, new InstructionADC(cpu, Hardware.AddressingModes.ImmediateMemoryFlag));
            RegisterKnownInstruction(0x6D, new InstructionADC(cpu, Hardware.AddressingModes.Absolute));
            RegisterKnownInstruction(0x6F, new InstructionADC(cpu, Hardware.AddressingModes.AbsoluteLong));
            RegisterKnownInstruction(0x71, new InstructionADC(cpu, Hardware.AddressingModes.DirectIndirectIndexed));
            RegisterKnownInstruction(0x72, new InstructionADC(cpu, Hardware.AddressingModes.DirectIndirect));
            RegisterKnownInstruction(0x73, new InstructionADC(cpu, Hardware.AddressingModes.StackRelativeIndirectIndexed));
            RegisterKnownInstruction(0x75, new InstructionADC(cpu, Hardware.AddressingModes.DirectIndexedX));
            RegisterKnownInstruction(0x77, new InstructionADC(cpu, Hardware.AddressingModes.DirectIndirectIndexedLong));
            RegisterKnownInstruction(0x79, new InstructionADC(cpu, Hardware.AddressingModes.AbsoluteIndexedY));
            RegisterKnownInstruction(0x7D, new InstructionADC(cpu, Hardware.AddressingModes.AbsoluteIndexedX));
            RegisterKnownInstruction(0x7F, new InstructionADC(cpu, Hardware.AddressingModes.AbsoluteIndexedLong));

            // AND
            RegisterKnownInstruction(0x21, GenericInst(OpCodes.AND, AddressingModes.DirectIndexedIndirect, (sender, a, b) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x23, GenericInst(OpCodes.AND, AddressingModes.StackRelative, (sender, a, b) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x25, GenericInst(OpCodes.AND, AddressingModes.Direct, (sender, a, b) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x27, GenericInst(OpCodes.AND, AddressingModes.DirectIndirectLong, (sender, a, b) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x29, GenericInstCustom(OpCodes.AND, AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref int p1, ref int p2)
            {
                p1 = sender.DecodeI1I2ArgumentForMFlag(bin, ref offset, ref context);
            }));
            RegisterKnownInstruction(0x2D, GenericInst(OpCodes.AND, AddressingModes.Absolute, (sender, a, b) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x2F, GenericInst(OpCodes.AND, AddressingModes.AbsoluteLong, (sender, a, b) => { }, ArgumentType.I3));
            RegisterKnownInstruction(0x31, GenericInst(OpCodes.AND, AddressingModes.DirectIndirectIndexed, (sender, a, b) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x32, GenericInst(OpCodes.AND, AddressingModes.DirectIndirect, (sender, a, b) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x33, GenericInst(OpCodes.AND, AddressingModes.StackRelativeIndirectIndexed, (sender, a, b) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x35, GenericInst(OpCodes.AND, AddressingModes.DirectIndexedX, (sender, a, b) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x37, GenericInst(OpCodes.AND, AddressingModes.DirectIndirectIndexedLong, (sender, a, b) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x39, GenericInst(OpCodes.AND, AddressingModes.AbsoluteIndexedY, (sender, a, b) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x3D, GenericInst(OpCodes.AND, AddressingModes.AbsoluteIndexedX, (sender, a, b) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x3F, GenericInst(OpCodes.AND, AddressingModes.AbsoluteIndexedLong, (sender, a, b) => { }, ArgumentType.I3));

            // ASL
            RegisterKnownInstruction(0x06, GenericInst(Hardware.OpCodes.ASL, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x0A, GenericInst(Hardware.OpCodes.ASL, Hardware.AddressingModes.ImpliedAccumulator, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x0E, GenericInst(Hardware.OpCodes.ASL, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x16, GenericInst(Hardware.OpCodes.ASL, Hardware.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x1E, GenericInst(Hardware.OpCodes.ASL, Hardware.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2));

            // BRANCH (1)
            RegisterKnownInstruction(0x90, GenericInst(Hardware.OpCodes.BCC, Hardware.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xB0, GenericInst(Hardware.OpCodes.BCS, Hardware.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xF0, GenericInst(Hardware.OpCodes.BEQ, Hardware.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1));

            // TESTS
            RegisterKnownInstruction(0x24, GenericInst(Hardware.OpCodes.BIT, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x2C, GenericInst(Hardware.OpCodes.BIT, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x34, GenericInst(Hardware.OpCodes.BIT, Hardware.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x3C, GenericInst(Hardware.OpCodes.BIT, Hardware.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x89, GenericInstCustom(OpCodes.BIT, AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref int p1, ref int p2)
                {
                    p1 = sender.DecodeI1I2ArgumentForMFlag(bin, ref offset, ref context);
                }));

            // BRANCH (2)
            RegisterKnownInstruction(0x30, GenericInst(Hardware.OpCodes.BMI, Hardware.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xD0, GenericInst(Hardware.OpCodes.BNE, Hardware.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x10, GenericInst(Hardware.OpCodes.BPL, Hardware.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x80, GenericInst(Hardware.OpCodes.BRA, Hardware.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1));

            // BRK
            RegisterKnownInstruction(0x00, new InstructionBRK(cpu));

            // BRANCH (3)
            RegisterKnownInstruction(0x82, GenericInst(Hardware.OpCodes.BRL, Hardware.AddressingModes.RelativeLong, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x50, GenericInst(Hardware.OpCodes.BVC, Hardware.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x70, GenericInst(Hardware.OpCodes.BVS, Hardware.AddressingModes.Relative, (sender, p1, p2) => { }, ArgumentType.I1));
          
            // CLEAR
            RegisterKnownInstruction(0x18, GenericInst(Hardware.OpCodes.CLC, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0xD8, GenericInst(Hardware.OpCodes.CLD, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x58, GenericInst(Hardware.OpCodes.CLI, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0xB8, GenericInst(Hardware.OpCodes.CLV, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // CMP
            RegisterKnownInstruction(0xC1, GenericInst(Hardware.OpCodes.CMP, Hardware.AddressingModes.DirectIndexedIndirect, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xC3, GenericInst(Hardware.OpCodes.CMP, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xC5, GenericInst(Hardware.OpCodes.CMP, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xC7, GenericInst(Hardware.OpCodes.CMP, Hardware.AddressingModes.DirectIndirectLong, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xC9, GenericInstCustom(OpCodes.CMP, AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref int p1, ref int p2)
                {
                    p1 = sender.DecodeI1I2ArgumentForMFlag(bin, ref offset, ref context);
                }));
            RegisterKnownInstruction(0xCD, GenericInst(Hardware.OpCodes.CMP, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0xCF, GenericInst(Hardware.OpCodes.CMP, Hardware.AddressingModes.AbsoluteLong, (sender, p1, p2) => { }, ArgumentType.I3));
            RegisterKnownInstruction(0xD1, GenericInst(Hardware.OpCodes.CMP, Hardware.AddressingModes.DirectIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xD2, GenericInst(Hardware.OpCodes.CMP, Hardware.AddressingModes.DirectIndirect, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xD3, GenericInst(Hardware.OpCodes.CMP, Hardware.AddressingModes.StackRelativeIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xD5, GenericInst(Hardware.OpCodes.CMP, Hardware.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xD7, GenericInst(Hardware.OpCodes.CMP, Hardware.AddressingModes.DirectIndirectIndexedLong, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xD9, GenericInst(Hardware.OpCodes.CMP, Hardware.AddressingModes.AbsoluteIndexedY, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0xDD, GenericInst(Hardware.OpCodes.CMP, Hardware.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0xDF, GenericInst(Hardware.OpCodes.CMP, Hardware.AddressingModes.AbsoluteIndexedLong, (sender, p1, p2) => { }, ArgumentType.I3));

            // COP
            RegisterKnownInstruction(0x02, GenericInstCustom(Hardware.OpCodes.COP, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref int p1, ref int p2)
                {
                    /* on skip le byte d'après */
                    offset++;

                    p1 = sender.DecodeInt1Argument(bin, ref offset);
                }));

            // CPX
            RegisterKnownInstruction(0xE0, GenericInstCustom(Hardware.OpCodes.CPX, Hardware.AddressingModes.ImmediateMemoryFlag, (sender, p1, p2) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref int p1, ref int p2)
                {
                    p1 = sender.DecodeI1I2ArgumentForXFlag(bin, ref offset, ref context);
                }));
            RegisterKnownInstruction(0xE4, GenericInst(Hardware.OpCodes.CPX, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xEC, GenericInst(Hardware.OpCodes.CPX, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
           
            // CPY
            RegisterKnownInstruction(0xC0, GenericInstCustom(Hardware.OpCodes.CPY, Hardware.AddressingModes.ImmediateMemoryFlag, (sender, p1, p2) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref int p1, ref int p2)
                {
                    p1 = sender.DecodeI1I2ArgumentForXFlag(bin, ref offset, ref context);
                }));
            RegisterKnownInstruction(0xC4, GenericInst(Hardware.OpCodes.CPY, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xCC, GenericInst(Hardware.OpCodes.CPY, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));

            // DEC
            RegisterKnownInstruction(0x3A, GenericInst(Hardware.OpCodes.DEC, Hardware.AddressingModes.ImpliedAccumulator, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0xC6, GenericInst(Hardware.OpCodes.DEC, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xCE, GenericInst(Hardware.OpCodes.DEC, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0xD6, GenericInst(Hardware.OpCodes.DEC, Hardware.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xDE, GenericInst(Hardware.OpCodes.DEC, Hardware.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2));

            // DEX & DEY
            RegisterKnownInstruction(0xCA, GenericInst(Hardware.OpCodes.DEX, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x88, GenericInst(Hardware.OpCodes.DEY, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // EOR
            RegisterKnownInstruction(0x41, GenericInst(Hardware.OpCodes.EOR, Hardware.AddressingModes.DirectIndexedIndirect, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x43, GenericInst(Hardware.OpCodes.EOR, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x45, GenericInst(Hardware.OpCodes.EOR, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x47, GenericInst(Hardware.OpCodes.EOR, Hardware.AddressingModes.DirectIndirectLong, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x49, GenericInstCustom(OpCodes.EOR, AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref int p1, ref int p2)
                {
                    p1 = sender.DecodeI1I2ArgumentForMFlag(bin, ref offset, ref context);
                }));
            RegisterKnownInstruction(0x4D, GenericInst(Hardware.OpCodes.EOR, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x4F, GenericInst(Hardware.OpCodes.EOR, Hardware.AddressingModes.AbsoluteLong, (sender, p1, p2) => { }, ArgumentType.I3));
            RegisterKnownInstruction(0x51, GenericInst(Hardware.OpCodes.EOR, Hardware.AddressingModes.DirectIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x52, GenericInst(Hardware.OpCodes.EOR, Hardware.AddressingModes.DirectIndirect, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x53, GenericInst(Hardware.OpCodes.EOR, Hardware.AddressingModes.StackRelativeIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x55, GenericInst(Hardware.OpCodes.EOR, Hardware.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x57, GenericInst(Hardware.OpCodes.EOR, Hardware.AddressingModes.DirectIndirectIndexedLong, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x59, GenericInst(Hardware.OpCodes.EOR, Hardware.AddressingModes.AbsoluteIndexedY, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x5D, GenericInst(Hardware.OpCodes.EOR, Hardware.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x5F, GenericInst(Hardware.OpCodes.EOR, Hardware.AddressingModes.AbsoluteIndexedLong, (sender, p1, p2) => { }, ArgumentType.I3));

            // INC
            RegisterKnownInstruction(0x1A, GenericInst(Hardware.OpCodes.INC, Hardware.AddressingModes.ImpliedAccumulator, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0xE6, GenericInst(Hardware.OpCodes.INC, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xEE, GenericInst(Hardware.OpCodes.INC, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0xF6, GenericInst(Hardware.OpCodes.INC, Hardware.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xFE, GenericInst(Hardware.OpCodes.INC, Hardware.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2));

            // INX & INY
            RegisterKnownInstruction(0xE8, GenericInst(Hardware.OpCodes.INX, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0xC8, GenericInst(Hardware.OpCodes.INY, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // JMP
            RegisterKnownInstruction(0x4c, new InstructionJMP(cpu, Hardware.AddressingModes.Absolute));
            RegisterKnownInstruction(0x5c, new InstructionJMP(cpu, Hardware.AddressingModes.AbsoluteLong));
            RegisterKnownInstruction(0x6c, new InstructionJMP(cpu, Hardware.AddressingModes.AbsoluteIndirect));
            RegisterKnownInstruction(0x7c, new InstructionJMP(cpu, Hardware.AddressingModes.AbsoluteIndexedIndirect));
            RegisterKnownInstruction(0xDc, new InstructionJMP(cpu, Hardware.AddressingModes.AbsoluteIndirectLong));

            // JSR
            RegisterKnownInstruction(0x20, GenericInst(Hardware.OpCodes.JSR, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x22, GenericInst(Hardware.OpCodes.JSR, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I3));
            RegisterKnownInstruction(0xFC, GenericInst(Hardware.OpCodes.JSR, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));

            // LDA
            RegisterKnownInstruction(0xA1, GenericInst(Hardware.OpCodes.LDA, Hardware.AddressingModes.DirectIndexedIndirect, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xA3, GenericInst(Hardware.OpCodes.LDA, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xA5, GenericInst(Hardware.OpCodes.LDA, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xA7, GenericInst(Hardware.OpCodes.LDA, Hardware.AddressingModes.DirectIndirectLong, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xA9, GenericInstCustom(OpCodes.LDA, AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref int p1, ref int p2)
                {
                    p1 = sender.DecodeI1I2ArgumentForMFlag(bin, ref offset, ref context);
                }));
            RegisterKnownInstruction(0xAD, GenericInst(Hardware.OpCodes.LDA, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0xAF, GenericInst(Hardware.OpCodes.LDA, Hardware.AddressingModes.AbsoluteLong, (sender, p1, p2) => { }, ArgumentType.I3));
            RegisterKnownInstruction(0xB1, GenericInst(Hardware.OpCodes.LDA, Hardware.AddressingModes.DirectIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xB2, GenericInst(Hardware.OpCodes.LDA, Hardware.AddressingModes.DirectIndirect, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xB3, GenericInst(Hardware.OpCodes.LDA, Hardware.AddressingModes.StackRelativeIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xB5, GenericInst(Hardware.OpCodes.LDA, Hardware.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xB7, GenericInst(Hardware.OpCodes.LDA, Hardware.AddressingModes.DirectIndirectIndexedLong, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xB9, GenericInst(Hardware.OpCodes.LDA, Hardware.AddressingModes.AbsoluteIndexedY, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0xBD, GenericInst(Hardware.OpCodes.LDA, Hardware.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0xBF, GenericInst(Hardware.OpCodes.LDA, Hardware.AddressingModes.AbsoluteIndexedLong, (sender, p1, p2) => { }, ArgumentType.I3));

            // LDX
            RegisterKnownInstruction(0xA2, GenericInstCustom(Hardware.OpCodes.LDX, Hardware.AddressingModes.ImmediateMemoryFlag, (sender, p1, p2) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref int p1, ref int p2)
                {
                    p1 = sender.DecodeI1I2ArgumentForXFlag(bin, ref offset, ref context);
                }));
            RegisterKnownInstruction(0xA6, GenericInst(Hardware.OpCodes.LDX, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xAE, GenericInst(Hardware.OpCodes.LDX, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0xB6, GenericInst(Hardware.OpCodes.LDX, Hardware.AddressingModes.DirectIndexedY, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xBE, GenericInst(Hardware.OpCodes.LDX, Hardware.AddressingModes.AbsoluteIndexedY, (sender, p1, p2) => { }, ArgumentType.I2));
            
            // LDY
            RegisterKnownInstruction(0xA0, GenericInstCustom(Hardware.OpCodes.LDY, Hardware.AddressingModes.ImmediateMemoryFlag, (sender, p1, p2) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref int p1, ref int p2)
                {
                    p1 = sender.DecodeI1I2ArgumentForXFlag(bin, ref offset, ref context);
                }));
            RegisterKnownInstruction(0xA4, GenericInst(Hardware.OpCodes.LDY, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xAC, GenericInst(Hardware.OpCodes.LDY, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0xB4, GenericInst(Hardware.OpCodes.LDY, Hardware.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xBC, GenericInst(Hardware.OpCodes.LDY, Hardware.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2));

            // LSR
            RegisterKnownInstruction(0x46, GenericInst(Hardware.OpCodes.LSR, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x4A, GenericInst(Hardware.OpCodes.LSR, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x4E, GenericInst(Hardware.OpCodes.LSR, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x56, GenericInst(Hardware.OpCodes.LSR, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x5E, GenericInst(Hardware.OpCodes.LSR, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I2));

            // BLOCK MOVE
            RegisterKnownInstruction(0x54, GenericInst(Hardware.OpCodes.MVN, Hardware.AddressingModes.BlockMove, (sender, p1, p2) => { }, ArgumentType.I1, ArgumentType.I1));
            RegisterKnownInstruction(0x44, GenericInst(Hardware.OpCodes.MVP, Hardware.AddressingModes.BlockMove, (sender, p1, p2) => { }, ArgumentType.I1, ArgumentType.I1));

            // NOP
            RegisterKnownInstruction(0xEA, new InstructionNOP(cpu));

            // ORA
            RegisterKnownInstruction(0x01, GenericInst(Hardware.OpCodes.ORA, Hardware.AddressingModes.DirectIndexedIndirect, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x03, GenericInst(Hardware.OpCodes.ORA, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x05, GenericInst(Hardware.OpCodes.ORA, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x07, GenericInst(Hardware.OpCodes.ORA, Hardware.AddressingModes.DirectIndirectLong, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x09, GenericInstCustom(OpCodes.ORA, AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref int p1, ref int p2)
                {
                    p1 = sender.DecodeI1I2ArgumentForMFlag(bin, ref offset, ref context);
                }));
            RegisterKnownInstruction(0x0D, GenericInst(Hardware.OpCodes.ORA, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x0F, GenericInst(Hardware.OpCodes.ORA, Hardware.AddressingModes.AbsoluteLong, (sender, p1, p2) => { }, ArgumentType.I3));
            RegisterKnownInstruction(0x11, GenericInst(Hardware.OpCodes.ORA, Hardware.AddressingModes.DirectIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x12, GenericInst(Hardware.OpCodes.ORA, Hardware.AddressingModes.DirectIndirect, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x13, GenericInst(Hardware.OpCodes.ORA, Hardware.AddressingModes.StackRelativeIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x15, GenericInst(Hardware.OpCodes.ORA, Hardware.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x17, GenericInst(Hardware.OpCodes.ORA, Hardware.AddressingModes.DirectIndirectIndexedLong, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x19, GenericInst(Hardware.OpCodes.ORA, Hardware.AddressingModes.AbsoluteIndexedY, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x1D, GenericInst(Hardware.OpCodes.ORA, Hardware.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x1F, GenericInst(Hardware.OpCodes.ORA, Hardware.AddressingModes.AbsoluteIndexedLong, (sender, p1, p2) => { }, ArgumentType.I3));

            // PUSH
            RegisterKnownInstruction(0xF4, GenericInst(Hardware.OpCodes.PEA, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0xD4, GenericInst(Hardware.OpCodes.PEI, Hardware.AddressingModes.DirectIndirect, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x62, GenericInst(Hardware.OpCodes.PER, Hardware.AddressingModes.RelativeLong, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x48, GenericInst(Hardware.OpCodes.PHA, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x8B, GenericInst(Hardware.OpCodes.PHB, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x0B, GenericInst(Hardware.OpCodes.PHD, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x4B, GenericInst(Hardware.OpCodes.PHK, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x08, GenericInst(Hardware.OpCodes.PHP, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0xDA, GenericInst(Hardware.OpCodes.PHX, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x5A, GenericInst(Hardware.OpCodes.PHY, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));

            // PULL
            RegisterKnownInstruction(0x68, GenericInst(Hardware.OpCodes.PLA, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0xAB, GenericInst(Hardware.OpCodes.PLB, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x2B, GenericInst(Hardware.OpCodes.PLD, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x28, GenericInst(Hardware.OpCodes.PLP, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0xFA, GenericInst(Hardware.OpCodes.PLX, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x7A, GenericInst(Hardware.OpCodes.PLY, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));

            // REP
            RegisterKnownInstruction(0xC2, GenericInst(Hardware.OpCodes.REP, Hardware.AddressingModes.ImmediateMemoryFlag, (sender, p1, p2) => { }));

            // ROL
            RegisterKnownInstruction(0x26, GenericInst(Hardware.OpCodes.ROL, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x2A, GenericInst(Hardware.OpCodes.ROL, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x2E, GenericInst(Hardware.OpCodes.ROL, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x36, GenericInst(Hardware.OpCodes.ROL, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x3E, GenericInst(Hardware.OpCodes.ROL, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I2));
        
            // ROR
            RegisterKnownInstruction(0x66, GenericInst(Hardware.OpCodes.ROR, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x6A, GenericInst(Hardware.OpCodes.ROR, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x6E, GenericInst(Hardware.OpCodes.ROR, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x76, GenericInst(Hardware.OpCodes.ROR, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x7E, GenericInst(Hardware.OpCodes.ROR, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I2));

            // RETURNS
            RegisterKnownInstruction(0x40, GenericInst(Hardware.OpCodes.RTI, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x6B, GenericInst(Hardware.OpCodes.RTL, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));
            RegisterKnownInstruction(0x60, GenericInst(Hardware.OpCodes.RTS, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }));

            // SBC
            RegisterKnownInstruction(0xE1, GenericInst(Hardware.OpCodes.SBC, Hardware.AddressingModes.DirectIndexedIndirect, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xE3, GenericInst(Hardware.OpCodes.SBC, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xE5, GenericInst(Hardware.OpCodes.SBC, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xE7, GenericInst(Hardware.OpCodes.SBC, Hardware.AddressingModes.DirectIndirectLong, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xE9, GenericInstCustom(OpCodes.SBC, AddressingModes.ImmediateMemoryFlag, (sender, a, b) => { },
                (GenericInstruction.DecodeArgumentsFunctionDelegate)delegate(GenericInstruction sender, Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref int p1, ref int p2)
                {
                    p1 = sender.DecodeI1I2ArgumentForMFlag(bin, ref offset, ref context);
                }));
            RegisterKnownInstruction(0xED, GenericInst(Hardware.OpCodes.SBC, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0xEF, GenericInst(Hardware.OpCodes.SBC, Hardware.AddressingModes.AbsoluteLong, (sender, p1, p2) => { }, ArgumentType.I3));
            RegisterKnownInstruction(0xF1, GenericInst(Hardware.OpCodes.SBC, Hardware.AddressingModes.DirectIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xF2, GenericInst(Hardware.OpCodes.SBC, Hardware.AddressingModes.DirectIndirect, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xF3, GenericInst(Hardware.OpCodes.SBC, Hardware.AddressingModes.StackRelativeIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xF5, GenericInst(Hardware.OpCodes.SBC, Hardware.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xF7, GenericInst(Hardware.OpCodes.SBC, Hardware.AddressingModes.DirectIndirectIndexedLong, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0xF9, GenericInst(Hardware.OpCodes.SBC, Hardware.AddressingModes.AbsoluteIndexedY, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0xFD, GenericInst(Hardware.OpCodes.SBC, Hardware.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0xFF, GenericInst(Hardware.OpCodes.SBC, Hardware.AddressingModes.AbsoluteIndexedLong, (sender, p1, p2) => { }, ArgumentType.I3));
            
            // SEC
            RegisterKnownInstruction(0x38, GenericInst(Hardware.OpCodes.SEC, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // SED
            RegisterKnownInstruction(0xF8, GenericInst(Hardware.OpCodes.SED, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // SEI
            RegisterKnownInstruction(0x78, GenericInst(Hardware.OpCodes.SEI, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // SEP
            RegisterKnownInstruction(0xE2, GenericInst(Hardware.OpCodes.SEP, Hardware.AddressingModes.ImmediateMemoryFlag, (sender, p1, p2) => { }, ArgumentType.I1));

            // STA
            RegisterKnownInstruction(0x81, GenericInst(Hardware.OpCodes.STA, Hardware.AddressingModes.DirectIndexedIndirect, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x83, GenericInst(Hardware.OpCodes.STA, Hardware.AddressingModes.StackRelative, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x85, GenericInst(Hardware.OpCodes.STA, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x87, GenericInst(Hardware.OpCodes.STA, Hardware.AddressingModes.DirectIndirectLong, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x8D, GenericInst(Hardware.OpCodes.STA, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x8F, GenericInst(Hardware.OpCodes.STA, Hardware.AddressingModes.AbsoluteLong, (sender, p1, p2) => { }, ArgumentType.I3));
            RegisterKnownInstruction(0x91, GenericInst(Hardware.OpCodes.STA, Hardware.AddressingModes.DirectIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x92, GenericInst(Hardware.OpCodes.STA, Hardware.AddressingModes.DirectIndirect, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x93, GenericInst(Hardware.OpCodes.STA, Hardware.AddressingModes.StackRelativeIndirectIndexed, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x95, GenericInst(Hardware.OpCodes.STA, Hardware.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x97, GenericInst(Hardware.OpCodes.STA, Hardware.AddressingModes.DirectIndirectIndexedLong, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x99, GenericInst(Hardware.OpCodes.STA, Hardware.AddressingModes.AbsoluteIndexedY, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x9D, GenericInst(Hardware.OpCodes.STA, Hardware.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x9F, GenericInst(Hardware.OpCodes.STA, Hardware.AddressingModes.AbsoluteIndexedLong, (sender, p1, p2) => { }, ArgumentType.I3));

            // STP
            RegisterKnownInstruction(0xDB, GenericInst(Hardware.OpCodes.STP, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // STX
            RegisterKnownInstruction(0x86, GenericInst(Hardware.OpCodes.STX, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x8E, GenericInst(Hardware.OpCodes.STX, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x96, GenericInst(Hardware.OpCodes.STX, Hardware.AddressingModes.DirectIndexedY, (sender, p1, p2) => { }, ArgumentType.I1));

            // STY
            RegisterKnownInstruction(0x84, GenericInst(Hardware.OpCodes.STY, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x8C, GenericInst(Hardware.OpCodes.STY, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x94, GenericInst(Hardware.OpCodes.STY, Hardware.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1));

            // STZ
            RegisterKnownInstruction(0x64, GenericInst(Hardware.OpCodes.STZ, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x74, GenericInst(Hardware.OpCodes.STZ, Hardware.AddressingModes.DirectIndexedX, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x9C, GenericInst(Hardware.OpCodes.STZ, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));
            RegisterKnownInstruction(0x9E, GenericInst(Hardware.OpCodes.STZ, Hardware.AddressingModes.AbsoluteIndexedX, (sender, p1, p2) => { }, ArgumentType.I2));

            // TAX
            RegisterKnownInstruction(0xAA, GenericInst(Hardware.OpCodes.TAX, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // TAY
            RegisterKnownInstruction(0xA8, GenericInst(Hardware.OpCodes.TAY, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // TCD
            RegisterKnownInstruction(0x5B, GenericInst(Hardware.OpCodes.TCD, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));
           
            // TCS
            RegisterKnownInstruction(0x1B, GenericInst(Hardware.OpCodes.TCS, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));
           
            // TDC
            RegisterKnownInstruction(0x7B, GenericInst(Hardware.OpCodes.TDC, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // TRB
            RegisterKnownInstruction(0x14, GenericInst(Hardware.OpCodes.TRB, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x1C, GenericInst(Hardware.OpCodes.TRB, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));

            // TSB
            RegisterKnownInstruction(0x04, GenericInst(Hardware.OpCodes.TSB, Hardware.AddressingModes.Direct, (sender, p1, p2) => { }, ArgumentType.I1));
            RegisterKnownInstruction(0x0C, GenericInst(Hardware.OpCodes.TSB, Hardware.AddressingModes.Absolute, (sender, p1, p2) => { }, ArgumentType.I2));

            // TSC
            RegisterKnownInstruction(0x3B, GenericInst(Hardware.OpCodes.TSC, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // TSX
            RegisterKnownInstruction(0xBA, GenericInst(Hardware.OpCodes.TSX, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // TXA
            RegisterKnownInstruction(0x8A, GenericInst(Hardware.OpCodes.TXA, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));
                      
            // TXS
            RegisterKnownInstruction(0x9A, GenericInst(Hardware.OpCodes.TXS, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));
           
            // TXY
            RegisterKnownInstruction(0x9B, GenericInst(Hardware.OpCodes.TXY, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));
            
            // TYA
            RegisterKnownInstruction(0x98, GenericInst(Hardware.OpCodes.TYA, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // TYX
            RegisterKnownInstruction(0xBB, GenericInst(Hardware.OpCodes.TYX, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // WAI
            RegisterKnownInstruction(0xCB, GenericInst(Hardware.OpCodes.WAI, Hardware.AddressingModes.Implied, (sender, p1, p2) => { }));

            // WDM (not used)
            RegisterKnownInstruction(0x42, new InstructionWDM(cpu));

            // XBA
            RegisterKnownInstruction(0xeb, new InstructionXBA(cpu));

            // XCE
            RegisterKnownInstruction(0xfb, new InstructionXCE(cpu));
        }
    }
}