using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SnesEmulator.RomReader;
using System.Diagnostics;
using System.Reflection;
using SnesEmulator.Hardware;

namespace SnesEmulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            using (var strm = new System.IO.FileStream(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Super Mario All-Stars.smc",
                System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                int romSize, ramSize;

                bool hasHeader = true;
                var headerPosition = hasHeader ? RomHeaderConstants.HeaderedLoROM : RomHeaderConstants.HeaderLessLoROM;

                Loader.GetROMParameters(strm, 0, headerPosition, out romSize, out ramSize);

                var snes = new SnesPlatform(romSize, ramSize);

                var romBin = Loader.LoadInto(strm, 0, headerPosition, snes.Memory, 0);

                using (var decodeOut = new System.IO.FileStream(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\decode.txt",
                    System.IO.FileMode.Create))
                {
                    using(var wr = new System.IO.StreamWriter(decodeOut))
                    {
                        snes.Decoder.Decode(romBin, 0).Print(wr);

                        wr.Flush();
                        decodeOut.Flush();
                    }
                }

                snes.Interpreter.Interpret(romBin, 0);

                //while (strm.Position < strm.Length - 1000)
                //    MiniDecode(strm);

                MessageBox.Show("");
            }
        }

        //Marche pas, car on doit connaître la valeur des registres pour certaines instructions
        //private void MiniDecode(System.IO.FileStream strm)
        //{
        //    int b = strm.ReadByte();

        //    switch (b)
        //    {
        //        case 0: { Debug.WriteLine("BRK"); strm.Position += 1; break; }
        //        case 0x12: { Debug.WriteLine("ORA(dp)"); strm.Position += 1; break; }
        //        case 0x05: { Debug.WriteLine("ORA dp"); strm.Position += 1; break; }
        //        case 0x01: { Debug.WriteLine("ORA (dp,X)"); strm.Position += 1; break; }
        //        case 0x1f: { Debug.WriteLine("ORA long,X"); strm.Position += 2; break; }
        //        case 0xc5: { Debug.WriteLine("CMP dp"); strm.Position += 1; break; }
        //        case 0x32: { Debug.WriteLine("AND (dp)"); strm.Position += 1; break; }
        //        case 0x85: { Debug.WriteLine("STA dp"); strm.Position += 1; break; }
        //        case 0x81: { Debug.WriteLine("STA (dp,X)"); strm.Position += 1; break; }
        //        case 0x87: { Debug.WriteLine("STA [dp]"); strm.Position += 1; break; }
        //        case 0x80: { Debug.WriteLine("BRA nearlabel"); strm.Position += 1; break; }
        //        case 0xc4: { Debug.WriteLine("CPY dp"); strm.Position += 1; break; }
        //        case 0x84: { Debug.WriteLine("STY dp"); strm.Position += 1; break; }
        //        case 0x86: { Debug.WriteLine("STX dp"); strm.Position += 1; break; }
        //        case 0x83: { Debug.WriteLine("STA sr,S"); strm.Position += 1; break; }
        //        case 0xbb: { Debug.WriteLine("TYX"); strm.Position += 0; break; }
        //        case 0x22: { Debug.WriteLine("JSR long"); strm.Position += 3; break; }
        //        case 0xfc: { Debug.WriteLine("JSR (addr,X)"); strm.Position += 2; break; }
        //        case 0x7c: { Debug.WriteLine("JMP (addr,X)"); strm.Position += 2; break; }
        //        case 0x45: { Debug.WriteLine("EOR dp"); strm.Position += 3; break; }
        //        case 0x47: { Debug.WriteLine("EOR [dp]"); strm.Position += 1; break; }
        //        case 0x38: { Debug.WriteLine("SEC"); strm.Position += 0; break; }
        //        case 0x3b: { Debug.WriteLine("TSC"); strm.Position += 0; break; }
        //        case 0x0c: { Debug.WriteLine("TSB addr"); strm.Position += 2; break; }
        //        case 0x7b: { Debug.WriteLine("TDC"); strm.Position += 0; break; }
        //        case 0x78: { Debug.WriteLine("SEI"); strm.Position += 0; break; }
        //        case 0x79: { Debug.WriteLine("ADC addr,Y"); strm.Position += 2; break; }
        //        case 0x7f: { Debug.WriteLine("ADC long,X"); strm.Position += 2; break; }
        //        case 0xff: { Debug.WriteLine("SBC long,X"); strm.Position += 2; break; }
        //        case 0x40: { Debug.WriteLine("RTI"); strm.Position += 0; break; }
        //        default:
        //            {
        //                Debug.WriteLine(String.Format("Unknown : {0}", b.ToString("x")));
        //                break;
        //            }
        //    }
        //}
    }
}
