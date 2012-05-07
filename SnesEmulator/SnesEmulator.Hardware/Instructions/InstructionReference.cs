using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SnesEmulator.Hardware.Instructions
{
    /// <summary>
    /// Instruction décodée.
    /// Une référence vers l'instruction, et les données.
    /// </summary>
    public struct InstructionReference
    {
        /// <summary>
        /// Peut servir pour le debug
        /// </summary>
        public int offset;

        public Instruction instruction;

        public int param1;
        public int param2;

        public string StringRepresentation()
        {
            return String.Format("#{0} \t-- {1}", offset.ToString("X8"), instruction.StringRepresentation(param1, param2));
        }
    }
}
