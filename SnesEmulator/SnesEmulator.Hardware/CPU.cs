﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnesEmulator.Hardware.Memory;
using SnesEmulator.Hardware.Instructions;
using System.Diagnostics;

namespace SnesEmulator.Hardware
{
    public class CPU
    {
        #region Debug

        public string PrintStatus()
        {
            string reportStatus = "Registers:\n";
            reportStatus += String.Format("ACC : {0}\n", ACC);
            reportStatus += String.Format("B : {0}\n", B);
            reportStatus += String.Format("SP : {0}\n", SP);
            reportStatus += String.Format("X : {0}\n", X);
            reportStatus += String.Format("Y : {0}\n", Y);
            reportStatus += String.Format("D : {0}\n", D);
            reportStatus += String.Format("DBR : {0}\n", DBR);
            reportStatus += String.Format("PBR : {0}\n", PBR);
            reportStatus += String.Format("PC : {0}\n", PC);
            reportStatus += "\n\n";

            reportStatus += "Flags:\n";
            reportStatus += String.Format("NegativeFlag : {0}\n", NegativeFlag);
            reportStatus += String.Format("ZeroFlag : {0}\n", ZeroFlag);
            reportStatus += String.Format("OverflowFlag : {0}\n", OverflowFlag);
            reportStatus += String.Format("CarryFlag : {0}\n", CarryFlag);
            reportStatus += String.Format("EFlag : {0}\n\n", EFlag);

            reportStatus += String.Format("BreakFlag : {0}\n\n", BreakFlag);

            reportStatus += String.Format("MFlag : {0}\n", MFlag);
            reportStatus += String.Format("XFlag : {0}\n", XFlag);

            Debug.WriteLine(reportStatus);

            return reportStatus;
        }

        private Action<Instruction>[] instrumentationCallBacks = new Action<Instruction>[255];

        /// <summary>
        /// Callback d'instructions qui peuvent être branchés en utilisant l'instruction WDM.
        /// Les plages valides sont de 0 à 254
        /// </summary>
        /// <remarks>ATTENTION : Seulement de 0 à 254</remarks>
        public Action<Instruction>[] InstrumentationCallBacks
        {
            get { return instrumentationCallBacks; }
        }

        /// <summary>
        /// Efface tous les callback
        /// </summary>
        public void InstrumentationCallBacksClear()
        {
            instrumentationCallBacks = new Action<Instruction>[255];
        }

        #endregion
        
        #region Registers

        public int ACC = 0;
        public int B { get; set; } // Accumulateur B "caché" seulement en mode Emulation
        public int SP { get; set; } // Stack pointer
        public int X { get; set; }
        public int Y { get; set; }
        //public int M { get; set; } Etre ou ne pas être ? Telle est la question
        public int D { get; set; } // Direct Page register
        public int DBR { get; set; }
        public int PBR { get; set; }

        /// <summary>
        /// Je le met direct, sans propriété, afin de toujours pouvoir le passer en ref sans se prendre la tête
        /// </summary>
        public int PC;


        #endregion

        #region Flags

        public bool NegativeFlag = false;
        public bool ZeroFlag = true;
        public bool OverflowFlag { get; private set; }
        public bool CarryFlag { get; set; }
        public bool EFlag { get; set; }
        public bool DecimalFlag { get; set; }
        public bool IRQDisabledFlag { get; set; }

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

        public SnesPlatform Platform { get; private set; }

        public CPU(SnesPlatform platform, MemoryBin RAM)
        {
            if (platform == null)
                throw new ArgumentNullException("platform");

            Platform = platform;

            this.RAM = RAM;
            Reset();

            DirectPage = new MemoryBin(RAM.Container, RAM.Start, 256); 

            DecodeTable = new InstructionsDecodeTable(this);
        }

        public void Reset()
        {
            ACC = X = Y = PBR = DBR = 0x00;
            EFlag = true; // Le processeur démarre en mode Emulation
            XFlag = MFlag = true; // ACC, X et Y sur 8 bits
            D = 0x00; // La Direct Page correspond à la Zero Page en mode émulation. Elle pointe donc à l'adresse 0
            SP = 0x100; // Le stack pointer est à 0x100 en mode émulation
            //M = 1;
            PC = 0;
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

        public int BCDConversion(int value)
        {
            double result = 0;
            int i = 1;
            string hexValue = value.ToString();
            foreach (char c in hexValue)
            {
                result += char.GetNumericValue(c) * Math.Pow(16, hexValue.Length - i);
                i++;
            }
            return (int)result;
        }

        public int Decimal16bit(int value)
        {
            int result = 0;
            int mult = 1;
            string hexValue = value.ToString("X");
            IEnumerable<char> rString = hexValue.Reverse();
            foreach (char c in rString)
            {
                int digit = (int)Convert.ToInt32(c.ToString(), 16) * mult;
                result += digit;
                mult = mult * 10;
            }
            return result;
        }
    }
}
