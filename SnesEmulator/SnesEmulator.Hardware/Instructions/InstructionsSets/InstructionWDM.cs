using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware.Instructions.InstructionsSets
{
    public class InstructionWDM : Instruction
    {
        public InstructionWDM(CPU cpu)
            : base(cpu, OpCodes.WDM, AddressingModes.Implied)
        {

        }

        public override bool HaveArgs
        {
            get { return true; }
        }

        public override void Run(int arg1, int arg2)
        {
            if (arg1 != 0)
            {
                var callback = CPU.InstrumentationCallBacks[arg1 - 1];

                if (callback != null)
                    callback(this);
            }
        }

        public override void DecodeArguments(Memory.MemoryBin bin, ref InstructionDecodeContext context, ref int offset, ref InstructionReference instructionReference)
        {
#warning TODO Cheat sur WDM
            /*
             * Normalement, l'instruction WDM est reservée pour une future utilisation (j'en vois pas)
             * et n'est donc pas associée à une instruction du CPU. Du coup j'en fait un cheat qui peut
             * être utilisé pour instrumenter du code généré.
             * 
             * Si l'argument (sur 1 byte) est différent de 0, un callback correspondant sera appelé sur le CPU
             * */
            instructionReference.param1 = DecodeInt1Argument(bin, ref offset);
        }
    }
}
