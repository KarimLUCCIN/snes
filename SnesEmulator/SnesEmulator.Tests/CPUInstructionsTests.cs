using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnesEmulator.Hardware;
using System.Diagnostics;
using SnesEmulator.Hardware.Memory;

namespace SnesEmulator.Tests
{
    [TestClass]
    public class CPUInstructionsTests
    {
        private static void InitTestContext(out SnesPlatform snes, out MemoryBin romBin, out int writeOffset)
        {
            snes = new SnesPlatform(1024, 512);
            romBin = new MemoryBin(snes.Memory, 0, snes.Memory.Length);

            snes.Interpreter.RethrowExecutionExceptions = true;
            snes.Interpreter.Trace = true;

            writeOffset = 0;
        }

        [TestMethod]
        public void TestAllDecodedAndImplemented()
        {
            /* On vérifie juste que tout soit bien implémenté */

            SnesPlatform snes;
            MemoryBin romBin;
            int writeOffset;

            InitTestContext(out snes, out romBin, out writeOffset);

            for(int i = 0;i<256;i++)
            {
                snes.CPU.Reset();
                snes.CPU.DecodeTable.KnownInstructions[i].Run(0, 0);
            }
        }

        [TestMethod]
        public void BasicTestInterpreter()
        {
            SnesPlatform snes;
            MemoryBin romBin;
            int writeOffset;

            InitTestContext(out snes, out romBin, out writeOffset);

            for (int i = 0; i < 8; i++)
                snes.Encoder.Write(romBin, ref writeOffset, OpCodes.NOP);

            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, (i) =>
            {
                Assert.AreEqual(0, snes.CPU.ACC, "ACC doit être égal à 0 à la base");
            });
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.ADC, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 24);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.ADC, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 26);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.ADC, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 0);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, (i) =>
            {
                Assert.AreEqual(24 + 26, snes.CPU.ACC);
            });
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.STP);

            snes.Interpreter.Interpret(romBin, 0, false);
        }

        [TestMethod]
        public void TestADCDecimal()
        {
            /*  On test ça :
                SED		;Set decimal mode.
                LDA #$00	;A = 00
                CLC		;prepare for addition
                ADC #$02	;00 + 02 = 02
                ADC #$0C	;#$0C = 12. 02 + 12 = 14. NOT #$0E
                ADC #$09	;14 + 9 = 23
                ADC #$12	;23 + 12 = 35
                ADC #$80	;35 + 80 = 0115. But since A = 8-bit, the result is 15.
                CLD		;Clear decimal mode.
             * */

            SnesPlatform snes;
            MemoryBin romBin;
            int writeOffset;

            InitTestContext(out snes, out romBin, out writeOffset);

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.SED);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, (i) =>
            {
                Assert.AreEqual(true, snes.CPU.DecimalFlag);
            });
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.LDA, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 0);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, (i) =>
            {
                Assert.AreEqual(0, snes.CPU.ACC);
            });
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.CLC);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.ADC, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 0x02);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.ADC, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 0x0C);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.ADC, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 0x09);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.ADC, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 0x12);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.ADC, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 0x80);

            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, (i) =>
            {
                Assert.AreEqual(0x15, snes.CPU.ACC);
            });
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.CLD);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, (i) =>
            {
                Assert.AreEqual(false, snes.CPU.DecimalFlag);
            });
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.STP);
            snes.Interpreter.Interpret(romBin, 0, false);
        }
    }
}
