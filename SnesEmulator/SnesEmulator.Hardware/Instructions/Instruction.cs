using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware.Instructions
{
    /// <summary>
    /// Instruction de base
    /// </summary>
    public abstract class Instruction
    {
        private CPU.Opcodes code;

        public CPU.Opcodes Code
        {
            get { return code; }
            set { code = value; }
        }

        private CPU.AddressingModes addrMode;

        public CPU.AddressingModes AddrMode
        {
            get { return addrMode; }
            set { addrMode = value; }
        }
        
        public Instruction(CPU.Opcodes code, CPU.AddressingModes addrMode)
        {

        }

        public void Print()
        {

        }
    }
}
