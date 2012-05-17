using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnesEmulator.Hardware.Instructions;
using SnesEmulator.Hardware.Memory;

namespace SnesEmulator.Hardware
{
    /// <summary>
    /// Une SNES ... \o/
    /// </summary>
    public class SnesPlatform
    {
        private CPU cpu;

        /// <summary>
        /// CPU
        /// </summary>
        public CPU CPU
        {
            get { return cpu; }
        }

        private InstructionsDecoder decoder;

        /// <summary>
        /// Décodeur d'instructions
        /// </summary>
        public InstructionsDecoder Decoder
        {
            get { return decoder; }
        }

        private InstructionsEncoder encoder;

        /// <summary>
        /// Petit écriveur d'instructions
        /// </summary>
        public InstructionsEncoder Encoder
        {
            get { return encoder; }
        }

        private LiveInstructionsDecoder interpreter;

        /// <summary>
        /// Interpréteur d'instructions
        /// </summary>
        public LiveInstructionsDecoder Interpreter
        {
            get { return interpreter; }
        }
        
        private MemoryContainer memory;

        public MemoryContainer Memory
        {
            get { return memory; }
        }

        /// <summary>
        /// Crée une nouvelle instance de SNES
        /// </summary>
        public SnesPlatform()
        {
            //Taille maximale addressable par la SNES
            const int mappedMemorySize = 0x7FFFFF;

            memory = new MemoryContainer(mappedMemorySize);

            cpu = new CPU(this);

            decoder = new InstructionsDecoder(cpu);
            encoder = new InstructionsEncoder(cpu);
            interpreter = new LiveInstructionsDecoder(this);
        }
    }
}
