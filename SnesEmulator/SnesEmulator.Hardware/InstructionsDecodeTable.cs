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
            KnownInstructions[0xea] = new InstructionNOP(cpu);

            // WDM (not used)
            KnownInstructions[0x42] = new InstructionWDM(cpu);

            // XBA
            KnownInstructions[0xeb] = new InstructionXBA(cpu);

            // XCE
            KnownInstructions[0xfb] = new InstructionXCE(cpu);
        }
    }
}
