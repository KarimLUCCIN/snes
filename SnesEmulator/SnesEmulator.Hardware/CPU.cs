using System;
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

        // Accumulateur B "caché" seulement en mode Emulation
        public int B
        {
            get { return X; }
            set { X = value; }
        }

        public int SP = 0; // Stack pointer
        public int X = 0;
        public int Y = 0;
        //public int M { get; set; } Etre ou ne pas être ? Telle est la question
        
        /// <summary>
        /// Direct Page Register
        /// </summary>
        public short D = 0;

        /// <summary>
        /// Data Bank Register
        /// </summary>
        public byte DBR = 0;

        /// <summary>
        /// Program Bank Register
        /// </summary>
        public byte PBR = 0;

        /// <summary>
        /// Je le met direct, sans propriété, afin de toujours pouvoir le passer en ref sans se prendre la tête
        /// </summary>
        public int PC;


        #endregion

        #region Flags

        /* 
         * Les instructions PHP et PLP push et pull un int qui reprend toutes les valeurs
         * de status du processeur. Du coup, j'en fait un int directement.
         * 
         * 
            Flags stored in P Register

            Mnemonic	Value	Binary Value	Description
            N	#$80	10000000	Negative
            V	#$40	01000000	Overflow
            Z	#$02	00000010	Zero
            C	#$01	00000001	Carry
            D	#$08	00001000	Decimal
            I	#$04	00000100	IRQ disable
            X	#$10	00010000	Index register size (native mode only)
            (0 = 16-bit, 1 = 8-bit)
            M	#$20	00100000	Accumulator register size (native mode only)
            (0 = 16-bit, 1 = 8-bit)
            E			6502 emulation mode
            B	#$10	00010000	Break (emulation mode only)

         */

        /// <summary>
        /// Processor's Flags
        /// </summary>
        public byte P = 0;

        private const int flag_n_offset = (1 << 7);
        private const int flag_v_offset = (1 << 6);
        private const int flag_z_offset = (1 << 1);
        private const int flag_c_offset = (1 << 0);
        private const int flag_d_offset = (1 << 3);
        private const int flag_i_offset = (1 << 2);
        private const int flag_x_offset = (1 << 4);
        private const int flag_m_offset = (1 << 5);

        /*
        * Quand on est en émulation, c'est B, sinon c'est X donc ça tient 
        * la route de le déclarer comme ça
         * */
        private const int flag_b_offset = flag_x_offset;

        public bool NegativeFlag
        {
            get { return (P & flag_n_offset) == flag_n_offset; }
            set
            {
                if (value)
                    P = (byte)(P | flag_n_offset);
                else
                    P = (byte)(P & ~flag_n_offset);
            }
        }

        public bool OverflowFlag
        {
            get { return (P & flag_v_offset) == flag_v_offset; }
            set
            {
                if (value)
                    P = (byte)(P | flag_v_offset);
                else
                    P = (byte)(P & ~flag_v_offset);
            }
        }

        public bool ZeroFlag
        {
            get { return (P & flag_z_offset) == flag_z_offset; }
            set
            {
                if (value)
                    P = (byte)(P | flag_z_offset);
                else
                    P = (byte)(P & ~flag_z_offset);
            }
        }

        public bool CarryFlag
        {
            get { return (P & flag_c_offset) == flag_c_offset; }
            set
            {
                if (value)
                    P = (byte)(P | flag_c_offset);
                else
                    P = (byte)(P & ~flag_c_offset);
            }
        }

        // Pas dans les flags
        public bool EFlag { get; set; }

        public bool DecimalFlag
        {
            get { return (P & flag_d_offset) == flag_d_offset; }
            set
            {
                if (value)
                    P = (byte)(P | flag_d_offset);
                else
                    P = (byte)(P & ~flag_d_offset);
            }
        }

        public bool IRQDisabledFlag
        {
            get { return (P & flag_i_offset) == flag_i_offset; }
            set
            {
                if (value)
                    P = (byte)(P | flag_i_offset);
                else
                    P = (byte)(P & ~flag_i_offset);
            }
        }

        // Emulation mode
        public bool BreakFlag
        {
            get { return XFlag; }
            set { XFlag = value; }
        }

        /// <summary>
        /// Renvoi la taille des registres, soit I1 soit I2, en fonction du mode
        /// actuel du processeur
        /// </summary>
        public ArgumentType CurrentRegisterSize
        {
            get { return MFlag ? ArgumentType.I1 : ArgumentType.I2; }
        }

        // Native mode
        public bool MFlag
        {
            get { return (P & flag_m_offset) == flag_m_offset; }
            set
            {
                if (value)
                    P = (byte)(P | flag_m_offset);
                else
                    P = (byte)(P & ~flag_m_offset);
            }
        }
        
        // Native mode
        public bool XFlag
        {
            get { return (P & flag_x_offset) == flag_x_offset; }
            set
            {
                if (value)
                    P = (byte)(P | flag_x_offset);
                else
                    P = (byte)(P & ~flag_x_offset);
            }
        }
        
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

        public MemoryBin NativeStackBin { get; private set; }

        public MemoryBin EmulationStackBin { get; private set; }

        public MemoryBin CurrentStackBin
        {
            get { return EFlag ? EmulationStackBin : NativeStackBin; }
        }

        public MemoryBin DirectPage { get; set; }

        public SnesPlatform Platform { get; private set; }

        public CPU(SnesPlatform platform)
        {
            if (platform == null)
                throw new ArgumentNullException("platform");

            Platform = platform;

            // TODO vérifier les plages
            DirectPage = new MemoryBin(platform.Memory, 0, 256);

            RAM = new SnesMemoryMappingBin(platform.Memory);

            NativeStackBin = new MemoryBin(platform.Memory, 0x7E0000, 0x00FFFF + 1);
            EmulationStackBin = new MemoryBin(platform.Memory, NativeStackBin.Start + 0x100, 0xFF + 1);

            DecodeTable = new InstructionsDecodeTable(this);

            Reset();
        }

        public void Reset()
        {
            ACC = X = Y = PBR = DBR = 0x00;
            EFlag = true; // Le processeur démarre en mode Emulation
            XFlag = MFlag = true; // ACC, X et Y sur 8 bits
            D = 0x00; // La Direct Page correspond à la Zero Page en mode émulation. Elle pointe donc à l'adresse 0
            //M = 1;
            PC = 0;

            SP = EmulationStackBin.Length-1;
        }

        public void SwitchFromEmulationToNativeMode()
        {
            CarryFlag = EFlag;
            EFlag = false;
            XFlag = MFlag = true; // Les flags X et M sont forcés à 1 pour rester sur 8 bits
            // XFlag et MFlag deviennent disponibles et BreakFlag indisponible

            // TODO Vérifier le comportement du Stack
            SP = 0x100 + (byte)SP;
        }

        public void SwitchFromNativeToEmulationMode()
        {
            CarryFlag = EFlag;
            EFlag = true;
            X = X & 0xFF; // X passe à 8 bits donc perd l'octet fort
            Y = Y & 0xFF; // Pareil pour Y
            B = (ACC >> 8) & 0xFF; // ACC garde son octet fort dans le registre "caché" B ...
            ACC = ACC & 0xFF;
            // La Direct Page passe à 0x00 normalement, mais si on revient en Native mode, elle récupère sa valeur d'avant ou garde 0 ? => A vérifier.
            // XFlag et MFlag ne doivent plus être utilisés en mode Emulation, et BreakFlag devient dispo
            XFlag = MFlag = true;

            // TODO Vérifier le comportement du Stack
            SP = (byte)SP;
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

        #region Stack

        public void StackPush(int value, ArgumentType argType)
        {
            switch (argType)
            {
                case ArgumentType.I1:
                    CurrentStackBin.WriteInt1(SP, (byte)value);
                    SP--;
                    break;
                case ArgumentType.I2:
                    CurrentStackBin.WriteInt1(SP, (byte)value);
                    SP--;

                    CurrentStackBin.WriteInt1(SP, (byte)(value >> 8));
                    SP--;
                    break;
                case ArgumentType.I3:
                case ArgumentType.None:
                default:
                    throw new NotSupportedException(argType.ToString());
            }
        }

        public int StackPull(ArgumentType argType)
        {
            switch (argType)
            {
                case ArgumentType.I1:
                    SP++;
                    return CurrentStackBin.ReadInt1(SP);
                case ArgumentType.I2:
                    SP++;
                    int tmp = CurrentStackBin.ReadInt1(SP) << 8;

                    SP++;
                    tmp |= CurrentStackBin.ReadInt1(SP);

                    return tmp;
                case ArgumentType.I3:
                case ArgumentType.None:
                default:
                    throw new NotSupportedException(argType.ToString());
            }
        }

        public int StackPeek(ArgumentType argType, int offset = 0)
        {
            switch (argType)
            {
                case ArgumentType.I1:
                    return CurrentStackBin.ReadInt1(SP + 1 + offset);
                case ArgumentType.I2:
                    return CurrentStackBin.ReadInt1(SP + 1 + offset) << 8 | CurrentStackBin.ReadInt1(SP + 2 + offset);
                case ArgumentType.I3:
                case ArgumentType.None:
                default:
                    throw new NotSupportedException(argType.ToString());
            }
        }

        #endregion
    }
}
