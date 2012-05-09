using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnesEmulator.Hardware.Memory;

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
            XBA,     // Exchanges low and high byte of the A register
            
            WDM     //Reserved for future use
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
        
        #region Registers

        public int ACC { get; set; }
        public int B { get; set; } // Accumulateur B "caché" seulement en mode Emulation
        public int SP { get; set; } // Stack pointer
        public int X { get; set; }
        public int Y { get; set; }
        public int M { get; set; }
        public int D { get; set; } // Direct Page register
        public int DBR { get; set; }
        public int PBR { get; set; }


        #endregion

        #region Flags

        public bool NegativeFlag { get; private set; }
        public bool ZeroFlag { get; private set; }
        public bool OverflowFlag { get; private set; }
        public bool CarryFlag { get; private set; }
        public bool EFlag { get; set; }

        public bool BreakFlag { get; set; } // Emulation mode

        public bool MFlag { get { return M > 0; } private set { ; } } // Native mode
        public bool XFlag { get { return X > 0; } private set { ; } } // Native mode

        public void SetNegativeFlag(int value)
        {
            if (MFlag)
                NegativeFlag = value > 127 ? true : false;
            else
                NegativeFlag = value > 32767 ? true : false;
        }

        public void SetZeroFlag(int value)
        {
            ZeroFlag = value == 0 ? true : false;
        }

        public void SetOverflowFlag(int value)
        {
            if (MFlag)
                OverflowFlag = value > Mode8Bits.Max ? true : false;
            else
                OverflowFlag = value > Mode16Bits.Max ? true : false;
        }

        public void SetCarryFlag(int value)
        {
            if (MFlag)
                CarryFlag = value > Mode8Bits.Max ? true : false;
            else
                CarryFlag = value > Mode16Bits.Max ? true : false;
        }

        #endregion

        public InstructionsDecodeTable DecodeTable { get; private set; }

        public MemoryBin RAM { get; private set; }

        public CPU(MemoryBin RAM)
        {
            this.RAM = RAM;
            ACC = X = Y = PBR = DBR = 0x00;
            EFlag = true; // Le processeur démarre en mode Emulation
            D = 0x00; // La Direct Page correspond à la Zero Page en mode émulation. Elle pointe donc à l'adresse 0
            SP = 0x100; // Le stack pointer est à 0x100 en mode émulation
            M = 1;

            DecodeTable = new InstructionsDecodeTable(this);
        }

        public void SwitchFromEmulationToNativeMode()
        {
            CarryFlag = EFlag;
            EFlag = false;
            XFlag = MFlag = true; // Les flags X et M sont forcés à 1 pour rester sur 8 bits
            // ... Modifier le SP ?
            // XFlag et MFlag deviennent disponibles et BreakFlag indisponible
        }

        public void SwitchFromNativeToEmulationMode()
        {
            CarryFlag = EFlag;
            EFlag = true;
            X = X & 0xFF; // X passe à 8 bits donc perd l'octet fort
            Y = Y & 0xFF; // Pareil pour Y
            B = (ACC >> 8) & 0xFF; // ACC garde son octet fort dans le registre "caché" B ...
            ACC = ACC & 0xFF;
            // ... Modifier le SP ?
            // XFlag et MFlag ne doivent plus être utilisés en mode Emulation, et BreakFlag devient dispo
        }
    }
}
