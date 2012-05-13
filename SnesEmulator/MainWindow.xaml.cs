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
using SnesEmulator.Renderer;
using System.ComponentModel;

namespace SnesEmulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public RenderEngine Renderer { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            InitializeGraphics();

            DataContext = this;

#warning TODO A changer lorsqu'il y aura un vrai rendu avec une horloge
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);

            using (var strm = new System.IO.FileStream(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\SnesInitializationROM.smc",
                System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var snes = new SnesPlatform((int)strm.Length, 256);

                var romBin = Loader.LoadInto(strm, 0, 0, snes.Memory, 0);

                using (var decodeOut = new System.IO.FileStream(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\init-decode.txt",
                    System.IO.FileMode.Create))
                {
                    using (var wr = new System.IO.StreamWriter(decodeOut))
                    {
                        snes.Decoder.Decode(romBin, 0).Print(wr);

                        wr.WriteLine();
                        wr.WriteLine();
                        wr.WriteLine();
                        wr.WriteLine("ALTERATION DU DEBUT");
                        wr.WriteLine();
                        wr.WriteLine();

                        int writeOffset = 0;

                        for (int i = 0; i < 8; i++)
                            snes.Encoder.Write(romBin, ref writeOffset, OpCodes.NOP);

                        snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, (i) =>
                        {
                            Debug.WriteLine(snes.CPU.ACC);
                        });
                        snes.Encoder.Write(romBin, ref writeOffset, OpCodes.ADC, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 24);
                        snes.Encoder.Write(romBin, ref writeOffset, OpCodes.ADC, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 26);
                        snes.Encoder.Write(romBin, ref writeOffset, OpCodes.AND, AddressingModes.ImmediateMemoryFlag, ArgumentType.I1, 0);
                        snes.Encoder.WriteCallbackInvoke(romBin, ref writeOffset, (i) =>
                        {
                            Debug.Assert(snes.CPU.ACC == 24 + 26);
                        });
                        snes.Encoder.Write(romBin, ref writeOffset, OpCodes.STP);

                        snes.Decoder.Decode(romBin, 0).Print(wr);

                        snes.Interpreter.Interpret(romBin, 0);

                        wr.Flush();
                        decodeOut.Flush();
                    }
                }
            }

            //using (var strm = new System.IO.FileStream(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Super Mario All-Stars.smc",
            //    System.IO.FileMode.Open, System.IO.FileAccess.Read))
            //{
            //    int romSize, ramSize;

            //    bool hasHeader = true;
            //    var headerPosition = hasHeader ? RomHeaderConstants.HeaderedLoROM : RomHeaderConstants.HeaderLessLoROM;

            //    Loader.GetROMParameters(strm, 0, headerPosition, out romSize, out ramSize);

            //    var snes = new SnesPlatform(romSize, ramSize);

            //    var romBin = Loader.LoadInto(strm, 0, headerPosition, snes.Memory, 0);

            //    using (var decodeOut = new System.IO.FileStream(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\decode.txt",
            //        System.IO.FileMode.Create))
            //    {
            //        using(var wr = new System.IO.StreamWriter(decodeOut))
            //        {
            //            snes.Decoder.Decode(romBin, 0).Print(wr);

            //            wr.Flush();
            //            decodeOut.Flush();
            //        }
            //    }

            //    snes.Interpreter.Interpret(romBin, 0);

            //    //while (strm.Position < strm.Length - 1000)
            //    //    MiniDecode(strm);

            //    MessageBox.Show("");
            //}
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            Renderer.RenderPass();
        }

        private void InitializeGraphics()
        {
            var renderer = new RenderEngine();
            renderer.Initialize(600, 400);

            Renderer = renderer;
            RaisePropertyChanged("Renderer");
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
