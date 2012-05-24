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
            snes = new SnesPlatform();
            romBin = new MemoryBin(snes.Memory, 0, snes.Memory.Length);

            snes.Interpreter.RethrowExecutionExceptions = true;
            snes.Interpreter.Trace = true;

            snes.Encoder.EnableInstructionValidation = true;

            writeOffset = 0;
        }

        [TestMethod]
        public void TestAllDecoded()
        {
            /* On vérifie juste que tout soit bien implémenté */

            SnesPlatform snes;
            MemoryBin romBin;
            int writeOffset;

            InitTestContext(out snes, out romBin, out writeOffset);

            for(int i = 0;i<256;i++)
            {
                Assert.AreNotEqual(null, snes.CPU.DecodeTable.KnownInstructions[i]);
            }
        }

        [TestMethod]
        public void TestAllImplemented()
        {
            /* On vérifie juste que tout soit bien implémenté */

            SnesPlatform snes;
            MemoryBin romBin;
            int writeOffset;

            InitTestContext(out snes, out romBin, out writeOffset);

            string notImplemented = String.Empty;

            int validCount = 0;

            for (int i = 0; i < 256; i++)
            {
                Assert.AreNotEqual(null, snes.CPU.DecodeTable.KnownInstructions[i]);

                try
                {
                    snes.CPU.Reset();
                    snes.CPU.StackPush(0, ArgumentType.I2);

                    snes.CPU.DecodeTable.KnownInstructions[i].Run(0, 0);

                    validCount++;
                }
                catch (NotImplementedException)
                {
                    notImplemented += String.Format("{0} : {1} ({2})\r\n",snes.CPU.DecodeTable.KnownInstructions[i].AssociatedHexCode.ToString("X2"), snes.CPU.DecodeTable.KnownInstructions[i].Code, snes.CPU.DecodeTable.KnownInstructions[i].AddrMode);
                }
            }

            Assert.AreEqual(String.Empty, notImplemented, String.Format("{0}/{1} Implemented ({2} %)", validCount, 256, (int)(100 * (validCount / 256.0))));
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

        [TestMethod]
        public void TestBIT()
        {
            /*  On test ça :
             
               >emul<
             
               LDA #$[bit: 11000000]
               STA $01
               LDA #$0
               BIT $01
               [Check(n = 1, v = 1, z = 1)]
               STZ $01
               BIT $01
               [Check(n = 0, v = 0, z = 1)]
               LDA #$[bit: 01000000]
               STA $04
               BIT $04
               [Check(n = 0, v = 1, z = 0)]
               STP
            * */

            SnesPlatform snes;
            MemoryBin romBin;
            int writeOffset;

            InitTestContext(out snes, out romBin, out writeOffset);

            snes.Encoder.WriteNativeToEmulationMode(romBin, ref writeOffset);

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.LDA, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, (1 << 7) | (1 << 6));
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.STA, AddressingModes.AbsoluteLong, ArgumentType.I3, 1);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.LDA, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 0);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.BIT, AddressingModes.Absolute, ArgumentType.I2, 1);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, delegate
            {
                Assert.AreEqual(true, snes.CPU.ZeroFlag);
                Assert.AreEqual(true, snes.CPU.OverflowFlag);
                Assert.AreEqual(true, snes.CPU.NegativeFlag);
            });

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.STZ, AddressingModes.Absolute, ArgumentType.I2, 1);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.BIT, AddressingModes.Absolute, ArgumentType.I2, 1);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, delegate
            {
                Assert.AreEqual(true, snes.CPU.ZeroFlag);
                Assert.AreEqual(false, snes.CPU.OverflowFlag);
                Assert.AreEqual(false, snes.CPU.NegativeFlag);
            });

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.LDA, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, (1 << 6));
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.STA, AddressingModes.AbsoluteLong, ArgumentType.I3, 4);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.BIT, AddressingModes.Absolute, ArgumentType.I2, 4);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, delegate
            {
                Assert.AreEqual(false, snes.CPU.ZeroFlag);
                Assert.AreEqual(true, snes.CPU.OverflowFlag);
                Assert.AreEqual(false, snes.CPU.NegativeFlag);
            });

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.STP);
            snes.Interpreter.Interpret(romBin, 0, false);
        }

        [TestMethod]
        public void TestTransfers()
        {
            /*  On test ça :
               LDA #$03 
               TAY
               TYX
               [Check(X == 3)]
               TSX
               TXY
               TYA
               [Check(ACC == SP)]
               STP
            * */
            SnesPlatform snes;
            MemoryBin romBin;
            int writeOffset;

            InitTestContext(out snes, out romBin, out writeOffset);

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.LDA, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 3);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.TAY);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.TYX);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, delegate
            {
                Assert.AreEqual(3, snes.CPU.X);
            });

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.TSX);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.TXY);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.TYA);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, delegate
            {
                Assert.AreEqual(snes.CPU.SP, snes.CPU.ACC);
            });

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.STP);
            snes.Interpreter.Interpret(romBin, 0, false);
        }

        [TestMethod]
        public void TestSTA()
        {
            /*  On test ça :
                LDA #$03 
                STA $7E0001
                LDA #0
                ADC $7E0001
                ADC $7E0001
                [Check(A == 6)]
                LDA $7E0001
                [Check(A == 3)]
                STZ $7E0001
                LDA $7E0001
                [Check(A == 0)]
                STP
             * */

            SnesPlatform snes;
            MemoryBin romBin;
            int writeOffset;

            InitTestContext(out snes, out romBin, out writeOffset);

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.LDA, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 3);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.STA, AddressingModes.AbsoluteLong, ArgumentType.I3, 1);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.LDA, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 0);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.ADC, AddressingModes.AbsoluteLong, ArgumentType.I3, 1);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.ADC, AddressingModes.AbsoluteLong, ArgumentType.I3, 1);

            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, delegate
            {
                Assert.AreEqual(6, snes.CPU.ACC);
            });

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.LDA, AddressingModes.AbsoluteLong, ArgumentType.I3, 1);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, delegate
            {
                Assert.AreEqual(3, snes.CPU.ACC);
            });

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.STZ, AddressingModes.Absolute, ArgumentType.I2, 1);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.LDA, AddressingModes.AbsoluteLong, ArgumentType.I3, 1);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, delegate
            {
                Assert.AreEqual(0, snes.CPU.ACC);
            });

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.STP);
            snes.Interpreter.Interpret(romBin, 0, false);
        }

        [TestMethod]
        public void TestStackBasicOnCPU()
        {
            /*
             * On test
             * 
             * PUSH 10,20,30,0xFF7,40
             * LDA $1337
             * PHA
             * 
             * PLA
             * [Check(ACC == 1337)]
             * foreach(nb in 10,20,30,0xFF7,40)
             * {
             *   PLX
             *   [Check(X == nb)]
             * }
             * 
             * PER 0x10 [lb]
             * PLA
             * 
             * [Check(ACC == lb + 10)]
             * */
            SnesPlatform snes;
            MemoryBin romBin;
            int writeOffset;

            InitTestContext(out snes, out romBin, out writeOffset);

            snes.CPU.SwitchFromEmulationToNativeMode();
            snes.CPU.MFlag = false;

            var testValues = new[] { 10, 20, 30, 0xFF7, 40 };

            foreach (var c_nb in testValues)
            {
                snes.Encoder.Write(romBin, ref writeOffset, OpCodes.PEA, AddressingModes.ImmediateMemoryFlag, ArgumentType.I2, c_nb);
            }

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.LDA, AddressingModes.ImmediateMemoryFlag, ArgumentType.I2, 0x1337);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.PHA, AddressingModes.StackRelative);

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.PLA);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, delegate
            {
                Assert.AreEqual(0x1337, snes.CPU.ACC);
            });

            for (int i = testValues.Length - 1; i >= 0; i--)
            {
                var current_param = testValues[i];

                snes.Encoder.Write(romBin, ref writeOffset, OpCodes.PLX);
                snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, delegate
                {
                    Assert.AreEqual(current_param, snes.CPU.X);
                });
            }

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.PER, AddressingModes.RelativeLong, ArgumentType.I2, 0x10);
            var lb = writeOffset;

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.PLA);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, delegate
            {
                Assert.AreEqual(lb + 0x10, snes.CPU.ACC);
            });
            

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.STP, AddressingModes.Implied);
            snes.Interpreter.Interpret(romBin, 0, false);
        }

        [TestMethod]
        public void TestStackBasicOnCPUWithCPUModeSwitch()
        {
            /*
             * On test
             * 
             * // emul to native
             * clc
             * xce
             * 
             * PUSH 10,20,30,0xFF7,40
             * LDA $13
             * PHA
             * 
             * // native to emul
             * sec
             * xce
             * 
             * PLA
             * [Check(ACC == 13)]
             * foreach(nb in 10,20,30,0xFF7,40)
             * {
             *   PLX
             *   [Check(X == nb)]
             * }
             * 
             * PER 0x10 [lb]
             * PLA
             * 
             * [Check(ACC == lb + 10)]
             * 
             * 
             * 
             * Note : certaines instructions sont doublées car en natif le stack fait 2 bytes alors qu'en emulation il n'en fait qu'1.
             * Il faut donc lire le byte 0 qui se trouve devant (si j'ai bien compris la logique ...)
             * */
            SnesPlatform snes;
            MemoryBin romBin;
            int writeOffset;

            InitTestContext(out snes, out romBin, out writeOffset);

            snes.Encoder.WriteEmulationToNativeMode(romBin, ref writeOffset);

            var testValues = new[] { 10, 20, 30, 0xF7, 40 };

            foreach (var c_nb in testValues)
            {
                snes.Encoder.Write(romBin, ref writeOffset, OpCodes.PEA, AddressingModes.ImmediateMemoryFlag, ArgumentType.I2, c_nb);
            }

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.LDA, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 0x13);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.PHA, AddressingModes.StackRelative);


            snes.Encoder.WriteNativeToEmulationMode(romBin, ref writeOffset);

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.PLA);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, delegate
            {
                Assert.AreEqual(0x13, snes.CPU.ACC);
            });

            for (int i = testValues.Length - 1; i >= 0; i--)
            {
                var current_param = testValues[i];

                snes.Encoder.Write(romBin, ref writeOffset, OpCodes.PLX);
                snes.Encoder.Write(romBin, ref writeOffset, OpCodes.PLX);
                snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, delegate
                {
                    Assert.AreEqual(current_param, snes.CPU.X);
                });
            }

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.PER, AddressingModes.RelativeLong, ArgumentType.I2, 0x10);
            var lb = writeOffset;

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.PLA);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.PLA);
            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, delegate
            {
                Assert.AreEqual(lb + 0x10, snes.CPU.ACC);
            });


            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.STP, AddressingModes.Implied);
            snes.Interpreter.Interpret(romBin, 0, false);
        }

        [TestMethod]
        public void TestStackLiteral()
        {
            /* Teste le stack, litéralement, sans instructions du CPU */
            SnesPlatform snes;
            MemoryBin romBin;
            int writeOffset;

            InitTestContext(out snes, out romBin, out writeOffset);

            var cpu = snes.CPU;

            cpu.StackPush(10, ArgumentType.I1);
            cpu.StackPush(20, ArgumentType.I1);
            cpu.StackPush(30, ArgumentType.I1);
            cpu.StackPush(0xFF7, ArgumentType.I2);
            cpu.StackPush(40, ArgumentType.I1);

            Assert.AreEqual(40, cpu.StackPeek(ArgumentType.I1));
            Assert.AreEqual(40, cpu.StackPull(ArgumentType.I1));

            Assert.AreEqual(0xFF7, cpu.StackPeek(ArgumentType.I2));
            Assert.AreEqual(0xFF7, cpu.StackPull(ArgumentType.I2));

            Assert.AreEqual(30, cpu.StackPeek(ArgumentType.I1));
            Assert.AreEqual(30, cpu.StackPull(ArgumentType.I1));

            Assert.AreEqual(20, cpu.StackPeek(ArgumentType.I1));
            Assert.AreEqual(20, cpu.StackPull(ArgumentType.I1));

            Assert.AreEqual(10, cpu.StackPeek(ArgumentType.I1));
            Assert.AreEqual(10, cpu.StackPull(ArgumentType.I1));
        }

        [TestMethod]
        public void TestStackLiteralWithCPUModeSwitch()
        {
            /* Teste le stack, litéralement, sans instructions du CPU */
            SnesPlatform snes;
            MemoryBin romBin;
            int writeOffset;

            InitTestContext(out snes, out romBin, out writeOffset);

            var cpu = snes.CPU;
            cpu.SwitchFromEmulationToNativeMode();

            cpu.StackPush(10, ArgumentType.I1);
            cpu.StackPush(20, ArgumentType.I1);
            cpu.StackPush(30, ArgumentType.I1);
            cpu.StackPush(0xFF7, ArgumentType.I2);
            cpu.StackPush(40, ArgumentType.I1);

            cpu.SwitchFromNativeToEmulationMode();

            Assert.AreEqual(40, cpu.StackPeek(ArgumentType.I1));
            Assert.AreEqual(40, cpu.StackPull(ArgumentType.I1));

            Assert.AreEqual(0xFF7, cpu.StackPeek(ArgumentType.I2));
            Assert.AreEqual(0xFF7, cpu.StackPull(ArgumentType.I2));

            Assert.AreEqual(30, cpu.StackPeek(ArgumentType.I1));
            Assert.AreEqual(30, cpu.StackPull(ArgumentType.I1));

            Assert.AreEqual(20, cpu.StackPeek(ArgumentType.I1));
            Assert.AreEqual(20, cpu.StackPull(ArgumentType.I1));

            Assert.AreEqual(10, cpu.StackPeek(ArgumentType.I1));
            Assert.AreEqual(10, cpu.StackPull(ArgumentType.I1));
        }

        [TestMethod]
        public void TestCMP()
        {
            /*  On test ça :
                
                LDA #$10	;A = 0x10
                CLC		;prepare for compare
                CMP #$10    ; ZeroFlag = 1
             * */

            SnesPlatform snes;
            MemoryBin romBin;
            int writeOffset;

            InitTestContext(out snes, out romBin, out writeOffset);

            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.LDA, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 0x10);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.CLC);
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.CMP, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 0x10);

            snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, (i) =>
            {
                Assert.AreEqual(true, snes.CPU.ZeroFlag);
            });
            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.STP);
            snes.Interpreter.Interpret(romBin, 0, false);
        }
    }
}
