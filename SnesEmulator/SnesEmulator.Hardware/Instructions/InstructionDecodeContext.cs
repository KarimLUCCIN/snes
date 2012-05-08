using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnesEmulator.Hardware.Memory;

namespace SnesEmulator.Hardware.Instructions
{
    /// <summary>
    /// Contexte de décodage qui permet d'identifier le mode dans lequel des instructions ont été décodées
    /// </summary>
    public struct InstructionDecodeContext
    {
        public bool MFlag;
        public bool XFlag;

        public MemoryBin Source;
    }
}
