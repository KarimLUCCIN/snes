using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware
{
    public class CPU
    {
        public enum Opcodes
        {
            // Invalid instruction (debug)
            INVALID,

            // Loading instructions
            LDA,    // Load into accumulator from memory
            LDX, 	// Load into X from memory
            LDY, 	//Load into Y from memory

            //Compare instructions
            CMP,    // Compare Accumulator with Memory
            CPY,    // Compare Y with Memory
            CPX,    // Compare X with Memory

            // Storage instructions
            STA,    // Store Accumulator to Memory
            STX,    // Store X to Memory
            STY,    // Store Y to Memory
            STZ,    // Store Zero to Memory

            // Branching instructions
            BCC,    // Branch if Carry Clear
            BCS,    // Branch if Carry Set
            BPL,    // Branch if Plus value
            BMI,    // Branch if Minus value
            BNE,    // Branch if not Equal/Branch if not zero
            BEQ,    // Branch if Equal/Branch if zero
            BVC,    // Branch if Overflow Clear
            BVS,    // Branch if Overflow Set
            BRL,    // Branch Always Long
            BRA,    // Branch Always

            // Mathematical instructions
            ADC,    // Add with carry
            SBC,    // Subtract with Carry
            INC,    // Increment Accumulator or Memory
            INX,    // Increment X
            INY,    // Increment Y
            DEC,    // Decrement Accumulator or Memory
            DEX,    // Decrement X
            DEY,    // Decrement Y

            // Processor flag instructions
            SEP,    // Set Processor Status Flag
            REP,    // Reset Processor Status Flag
            SEC,    // Set Carry Flag
            SED,    // Set Decimal Flag
            SEI,    // Set Interrupt Flag
            CLC,    // Clear Carry Flag
            CLD,    // Clear Decimal Flag
            CLI,    // Clear Interrupt Flag
            CLV,    // Clear Overflow Flag
            XCE,    // Exchange Carry and Emulation (swaps bits of emulation flag and carry flag, toggling emulation mode on/off)

            // Stack instructions
            PEA,    // Push Effective Address (Simply push a 16-bit absolute value on the stack)
            PEI,    // Push Effective Indirect Address
            PER,    // Push Program Counter Relative
            PHA,    // Push Accumulator
            PHB,    // Push Data Bank Register
            PHD,    // Push Direct Page Register
            PHK,    // Push Program Bank Register
            PHP,    // Push Processor Status Flags
            PHX,    // Push X
            PHY,    // Push Y
            PLA,    // Pull Accumulator
            PLB,    // Pull Data Bank Register
            PLD,    // Pull Direct Page Register
            PLP,    // Pull Processor Status Flags
            PLX,    // Pull X
            PLY,    // Pull Y
                
            // Bitwise instructions
            AND,    // AND Accumulator with Memory
            ASL,    // Left Shift Accumulator or Memory
            BIT,    // Bit Test
            EOR,    // Exclusive OR Accumulator with Memory
            LSR,    // Shift Right Accumulator or Memory
            ORA,    // OR Accumulator with Memory
            ROL,    // Rotate Left Accumulator or Memory
            ROR,    // Rotate Right Accumulator or Memory
            TRB,    // Test and Reset Bit
            TSB,    // Test and Set Bit

            // Transfer instructions
            TAX,    // Transfer Accumulator to X
            TAY,    // Transfer Accumulator to Y
            TCD,    // Transfer Accumulator to Direct Page
            TCS,    // Transfer Accumulator to Stack pointer
            TDC,    // Transfer Direct Page to Accumulator
            TSC,    // Transfer Stack Pointer to Accumulator
            TSX,    // Transfer Stack Pointer to X
            TXA,    // Transfer X to Accumulator
            TXS,    // Transfer X to Stack Pointer
            TXY,    // Transfer X to Y
            TYA,    // Transfer Y to Accumulator
            TYX,    // Transfer Y to X
            MVN,    // Block Move Negative
            MVP,    // Block Move Positive

            // Program flow instructions
            JML,    // Jump Long
            JMP,    // Jump
            JSL,    // Jump to Subroutine Long
            JSR,    // Jump to Subroutine
            RTI,    // Return from Interrupt
            RTL,    // Return from Subroutine Long
            RTS,    // Return from Subroutine

            // Other instructions
            BRK,    // Software Break (Sets the B flag in emulation mode, interrupt in native)
            COP,    // Coprocessor Empowerment (interrupt)
            NOP,    // No operation (does absolutely nothing except waste a cycle of processing time)
            STP,    // Stop the Clock (freezes the SNES's processor)
            WAI,    // Wait for Interrupt
            XBA     // Exchanges low and high byte of the A register
        }

        public enum AddressingModes
        {
            Implied,
            ImmediateMemoryFlag,
            ImmediateIndexFlag,
            Immediate8Bit,
            Relative,
            RelativeLong,
            Direct,
            DirectIndexedX,
            DirectIndexedY,
            DirectIndirect,
            DirectIndexedIndirect,
            DirectIndirectIndexed,
            DirectIndirectLong,
            DirectIndirectIndexedLong,
            Absolute,
            AbsoluteIndexedX,
            AbsoluteIndexedY,
            AbsoluteLong,
            AbsoluteIndexedLong,
            StackRelative,
            StackRelativeIndirectIndexed,
            AbsoluteIndirect,
            AbsoluteIndirectLong,
            AbsoluteIndexedIndirect,
            ImpliedAccumulator,
            BlockMove
        };

        enum Flags
        {
            Negative,
            Overflow,
            Zero,
            Carry,
            Decimal,
            IRQDisable,
            IndexRegisterSize, // Native mode only
            AccumulatorRegisterSize, // Native mode only
            EmulationMode,
            Break // Emulation mode only
        };

        public byte[][] Instructions { get; private set; }

        public InstructionsDecodeTable DecodeTable { get; private set; }

        public CPU()
        {
            DecodeTable = new InstructionsDecodeTable(this);

            Instructions = new byte[][]
            {
                //           IMP,  IMF,  IIF,  I8B,  REL,  RELL, DIR,  DIX,  DIY,  DI,   INX,  INY,  DIL,  DIIL, ABS,  AIX,  AIY,  ABSL, AIL,  SR,   SRII, ABSI, AIL,  AII,  IACC, BM
      /* LDA */ new byte[] { 0x00, 0xA9, 0x00, 0x00, 0x00, 0x00, 0xA5, 0xB5, 0x00, 0xB2, 0xA1, 0xB1, 0xA7, 0xB7, 0xAD, 0xBD, 0xB9, 0xAF, 0xBF, 0xA3, 0xB3, 0x00, 0x00, 0x00, 0x00, 0x00 },
      /* LDX */ new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                // A continuer avec tous les opcodes...
            };

        }

        private void Push(byte value)
        {
        }

        private byte Pop()
        {
            return 0;
        }

        private void Fetch()
        {
        }

        private void Decode()
        {
        }

        private void Execute()
        {
        }
    }
}
