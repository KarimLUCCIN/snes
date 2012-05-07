using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware.Instructions.InstructionsSets
{
    public class InstructionInvalid : Instruction
    {
        private int hexa;

        public int Hexa
        {
            get { return hexa; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cpu"></param>
        /// <param name="hexa">Le code brut de l'instruction non reconnue</param>
        public InstructionInvalid(CPU cpu, int hexa)
            :base(cpu, CPU.Opcodes.INVALID, CPU.AddressingModes.Absolute)
        {
            this.hexa = hexa;
        }

        public override bool HaveArgs
        {
            get { return false; }
        }

        public override string StringRepresentation(int param1 = 0, int param2 = 0)
        {
            return String.Format("INV {0}", hexa.ToString("x"));
        }

        public override void Run(int arg1, int arg2)
        {
            throw new InvalidProgramException(String.Format("Instruction non reconnue : {0}", hexa.ToString("x")));
        }

        public override void DecodeArguments(Memory.MemoryBin bin, MFlagMode mode, ref int offset, ref InstructionReference instructionReference)
        {
            /* No Args */
        }
    }
}
