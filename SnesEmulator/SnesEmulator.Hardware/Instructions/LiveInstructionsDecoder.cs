using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnesEmulator.Hardware.Memory;
using System.Threading;
using System.Diagnostics;

namespace SnesEmulator.Hardware.Instructions
{
    public class LiveInstructionsDecoder
    {
        private SnesPlatform platform;

        public SnesPlatform Platform
        {
            get { return platform; }
        }

        public LiveInstructionsDecoder(SnesPlatform platform)
        {
            if (platform == null)
                throw new ArgumentNullException("platform");

            this.platform = platform;
        }

        /// <summary>
        /// Exécute une seule instruction en partant de l'addresse spécifiée
        /// </summary>
        /// <param name="bin">Mémoire à utiliser pour la référence</param>
        /// <param name="startAddress">Adresse, ou rien pour utiliser le PC actuel</param>
        public void RunOnce(MemoryBin bin, int startAddress = -1)
        {
            if (bin == null)
                throw new ArgumentNullException("bin");

            if (startAddress >= 0)
                platform.CPU.PC = startAddress;

            var refInstruction = new InstructionReference();
            var context = platform.CPU.BuildCurrentContext();

            platform.Decoder.DecodeOnce(bin, ref platform.CPU.PC, ref context, ref refInstruction);

            refInstruction.instruction.Run(refInstruction.param1, refInstruction.param2);
        }

        Thread interpretThread = null;

        /// <summary>
        /// Quand le thread d'interprétation est lancé, ce dernier
        /// </summary>
        public Thread InterpretThread
        {
            get { return interpretThread; }
        }

        private bool stop = true;

        /// <summary>
        /// Indique si l'exécution doit être arretée, en cas d'interprétation en cours
        /// </summary>
        public bool Stop
        {
            get { return stop; }
            set { stop = value; }
        }

        private bool interpreting;

        /// <summary>
        /// Indique si une interprétation est en cours
        /// </summary>
        public bool Interpreting
        {
            get { return interpreting; }
        }

        private MemoryBin interpreterMemory;

        /// <summary>
        /// Mémoire qui contient le code exécuté par l'intérpréteur
        /// </summary>
        public MemoryBin InterpreterMemory
        {
            get { return interpreterMemory; }
        }

        private Exception lastInterpretException;

        public Exception LastInterpretException
        {
            get { return lastInterpretException; }
            set { lastInterpretException = value; }
        }

        private bool rethrowExecutionExceptions = false;

        public bool RethrowExecutionExceptions
        {
            get { return rethrowExecutionExceptions; }
            set { rethrowExecutionExceptions = value; }
        }

        private bool trace = false;

        public bool Trace
        {
            get { return trace; }
            set { trace = value; }
        }
                
        private void InterpretThreadFunction()
        {
            try
            {
                try
                {
                    var currentInstruction = new InstructionReference();
                    var currentContext = platform.CPU.BuildCurrentContext();

                    var cpu = platform.CPU;
                    var decoder = platform.Decoder;

                    while (!stop)
                    {
                        // On pourrait peut être le mettre en cache et je màj uniquement
                        // après une instruction qui le modifie
                        cpu.UpdateCurrentContext(ref currentContext);

                        decoder.DecodeOnce(interpreterMemory, ref cpu.PC, ref currentContext, ref currentInstruction);

                        if (trace)
                        {
                            Debug.WriteLine(currentInstruction.StringRepresentation());
                        }

                        currentInstruction.instruction.Run(currentInstruction.param1, currentInstruction.param2);
                    }
                }
                finally
                {
                    interpreting = false;
                }
            }
            catch (Exception ex)
            {
                if (rethrowExecutionExceptions)
                    throw;

                lastInterpretException = ex;
                Debug.WriteLine("Interpret Failure");
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Lance un thread d'exécution qui process toutes les instructions à partir de startAddress
        /// </summary>
        /// <param name="bin">Mémoire à utiliser pour la référence</param>
        /// <param name="startAddress">Adresse, ou rien pour utiliser le PC actuel</param>
        /// <param name="separateThread">true pour lancer dans un thread séparé, false sinon</param>
        public void Interpret(MemoryBin bin, int startAddress = -1, bool separateThread = true)
        {
            if (bin == null)
                throw new ArgumentNullException("bin");
            else if (interpreting)
                throw new InvalidOperationException("Une seule interprétation possible à la fois");
            else
            {
                // Il y a clairement une RunCondition ici mais bon ...
                interpreting = true;
                stop = false;

                if (startAddress >= 0)
                    platform.CPU.PC = startAddress;

                interpreterMemory = bin;

                if (separateThread)
                {
                    interpretThread = new Thread(new ThreadStart(InterpretThreadFunction));
                    interpretThread.Name = String.Format("SNES Interpreter - {0}", DateTime.Now.ToLongTimeString());

                    // Si on kill le programme, ça va killer aussi le thread ... c'est bien ça
                    interpretThread.IsBackground = true;

                    interpretThread.Start();
                }
                else
                {
                    InterpretThreadFunction();
                }
            }
        }
    }
}
