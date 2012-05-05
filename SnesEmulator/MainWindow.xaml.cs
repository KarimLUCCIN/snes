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

            using (var strm = new System.IO.FileStream(@"C:\LKSoft\Applications\MS.Net\SnesEmulator\Games\Super Mario All-Stars + Super Mario World (E) [!].smc",
                System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                strm.Position = RomHeaderConstants.HeaderLessLoROM;

                var header = (RomHeader)SnesEmulator.RomReader.StructGetter.ReadBlock(strm, typeof(RomHeader));

                MessageBox.Show("");
            }
        }
    }
}
