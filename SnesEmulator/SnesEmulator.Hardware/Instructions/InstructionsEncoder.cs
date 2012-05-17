using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnesEmulator.Hardware.Memory;

namespace SnesEmulator.Hardware.Instructions
{
    /// <summary>
    /// Vu que pour l'instant je n'ai pas d'assembleur sous la main, permet d'émettre des instructions dans un bin.
    /// N'est pas fait pour la performance mais pour le debug
    /// </summary>
    public class InstructionsEncoder
    {
        private CPU cpu;

        public CPU CPU
        {
            get { return cpu; }
        }
        
        public InstructionsEncoder(CPU cpu)
        {
            if (cpu == null)
                throw new ArgumentNullException("cpu");

            this.cpu = cpu;
        }

        /// <summary>
        /// Efface les callback associés au processeur
        /// </summary>
        public void ClearCallbackInvokes()
        {
            cpu.InstrumentationCallBacksClear();
        }

        /// <summary>
        /// Utilise une instruction spéciale qui va appeler un code à un moment clef de l'exécution
        /// </summary>
        /// <remarks>ATTENTION : Il n'y a que 254 slots disponibles en l'état. Pour effacer les associations, utiliser ClearCallbackInvokes</remarks>
        /// <param name="bin"></param>
        /// <param name="offset"></param>
        /// <param name="callback"></param>
        public void WriteCallbackInvoke(MemoryBin bin, ref int offset, Action<Instruction> callback)
        {
            if (callback == null)
                return;

            int slot = 0;

            for(slot = 0; slot <= 254;slot++)
            {
                if (cpu.InstrumentationCallBacks[slot] == null)
                    break;
            }

            if (slot > 254)
                throw new InvalidOperationException("Tous les slots de callback sont utilisés. Impossible d'encoder cette instruction");
            else
            {
                cpu.InstrumentationCallBacks[slot] = callback;
                Write(bin, ref offset, OpCodes.WDM, param1Type: ArgumentType.I1, param1: slot + 1);
            }
        }

        /// <summary>
        /// Ecrit une instruction. C'est un peut chiant car il faut spécifier à la fois l'instruction, l'addrMode, le type des args et les args mais bon,
        /// pour l'instant ça devrait faire l'affaire
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="offset"></param>
        /// <param name="opCode"></param>
        /// <param name="addrMode"></param>
        /// <param name="param1Type"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        public void Write(MemoryBin bin, ref int offset, OpCodes opCode, AddressingModes addrMode = AddressingModes.Direct, ArgumentType param1Type = ArgumentType.I1, int param1 = 0, int param2 = 0)
        {
            /* d'abord, on cherche l'instruction */
            Instruction match_instruction = null;

            var match_code = from instruction in cpu.DecodeTable.KnownInstructions where instruction.Code == opCode select instruction;
            switch(match_code.Count())
            {
                case 0: throw new InvalidOperationException(String.Format("Instruction not found : {0}", opCode));
                case 1: {
                    /* on se prend pas la tête sur le mode d'addr */
                    match_instruction = match_code.First();
                    break;
                }
                default:
                    {
                        /* on cherche le bon mode */
                        match_instruction = (from code_item in match_code where code_item.AddrMode == addrMode select code_item).FirstOrDefault();

                        if(match_instruction== null)
                            throw new InvalidOperationException(String.Format("Instruction {0} was found, but the addrMode {1} has no binding", opCode, addrMode));

                        break;
                    }
            }
            
            /* on écrit le code */
            bin.WriteInt1(offset, match_instruction.AssociatedHexCode);
            offset++;

            /* s'il y a des arguments, on les écrit */
            if (match_instruction.HaveArgs)
            {
                WriteParameter(bin, ref offset, param1Type, param1);

                if (addrMode == AddressingModes.BlockMove)
                {
                    /* c'est le seul mode qui a deux arguments, et il a la même
                     * taille que le premier argument
                     * */
                    WriteParameter(bin, ref offset, param1Type, param2);
                }
            }
        }

        private void WriteParameter(MemoryBin bin, ref int offset, ArgumentType paramType, int param)
        {
            switch (paramType)
            {
                default:
                case ArgumentType.None:
                    break;
                case ArgumentType.I1:
                    bin.WriteInt1(offset, (byte)param);
                    offset++;
                    break;
                case ArgumentType.I2:
                    bin.WriteInt1(offset, (byte)param);
                    bin.WriteInt1(offset + 1, (byte)(param >> 8));
                    offset += 2;
                    break;
                case ArgumentType.I3:
                    bin.WriteInt1(offset, (byte)param);
                    bin.WriteInt1(offset + 1, (byte)(param >> 8));
                    bin.WriteInt1(offset + 2, (byte)(param >> 16));
                    offset += 3;
                    break;
            }
        }
    }
}
