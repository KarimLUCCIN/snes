using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SnesEmulator.Hardware.Instructions
{
    public class InstructionsBin
    {
        private CPUMode decodeMode;

        public CPUMode DecodeMode
        {
            get { return decodeMode; }
            set { decodeMode = value; }
        }

        private InstructionReference[] instructions = new InstructionReference[0];

        public InstructionReference[] Instructions
        {
            get { return instructions; }
            set { instructions = value; }
        }

        public void Print(TextWriter wr)
        {
            if (instructions != null)
            {
                for (int i = 0; i < instructions.Length; i++)
                {
                    wr.WriteLine(instructions[i].StringRepresentation());
                }

                wr.WriteLine("; END");
            }
        }
    }
}
