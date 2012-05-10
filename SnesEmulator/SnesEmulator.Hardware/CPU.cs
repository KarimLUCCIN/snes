using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnesEmulator.Hardware.Memory;
using SnesEmulator.Hardware.Instructions;

namespace SnesEmulator.Hardware
{
    public class CPU
    {
        
        #region Registers

        public int ACC { get; set; }
        public int B { get; set; } // Accumulateur B "caché" seulement en mode Emulation
        public int SP { get; set; } // Stack pointer
        public int X { get; set; }
        public int Y { get; set; }
        //public int M { get; set; } Etre ou ne pas être ? Telle est la question
        public int D { get; set; } // Direct Page register
        public int DBR { get; set; }
        public int PBR { get; set; }


        #endregion

        #region Flags

        /// <summary>
        /// Je le met direct, sans propriété, afin de toujours pouvoir le passer en ref sans se prendre la tête
        /// </summary>
        public int PC;

        public bool NegativeFlag { get; private set; }
        public bool ZeroFlag { get; private set; }
        public bool OverflowFlag { get; private set; }
        public bool CarryFlag { get; private set; }
        public bool EFlag { get; set; }

        public bool BreakFlag { get; set; } // Emulation mode

        public bool MFlag { get; private set; } // Native mode
        public bool XFlag { get; private set; } // Native mode

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

        public MemoryBin DirectPage { get; set; }

        public CPU(MemoryBin RAM)
        {
            this.RAM = RAM;
            ACC = X = Y = PBR = DBR = 0x00;
            EFlag = true; // Le processeur démarre en mode Emulation
            D = 0x00; // La Direct Page correspond à la Zero Page en mode émulation. Elle pointe donc à l'adresse 0
            SP = 0x100; // Le stack pointer est à 0x100 en mode émulation
            //M = 1;
            PC = 0;
            DirectPage = new MemoryBin(RAM.Container, RAM.Start, 256); 

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
            SP = 0x100; // Stack remise à la page 1 du Data Bank 0
            // La Direct Page passe à 0x00 normalement, mais si on revient en Native mode, elle récupère sa valeur d'avant ou garde 0 ? => A vérifier.
            // XFlag et MFlag ne doivent plus être utilisés en mode Emulation, et BreakFlag devient dispo
            XFlag = MFlag = true;
        }

        /// <summary>
        /// Retourne une structure décrivant le contexte actuel du processeur
        /// </summary>
        /// <returns></returns>
        public InstructionDecodeContext BuildCurrentContext()
        {
            var result = new InstructionDecodeContext();
            UpdateCurrentContext(ref result);

            return result;
        }

        /// <summary>
        /// Met à jour un contexte en fonction de l'état du CPU
        /// </summary>
        /// <param name="context"></param>
        public void UpdateCurrentContext(ref InstructionDecodeContext context)
        {
            context.MFlag = MFlag;
            context.XFlag = XFlag;
        }
    }
}
