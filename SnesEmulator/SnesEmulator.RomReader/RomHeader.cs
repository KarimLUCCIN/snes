using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SnesEmulator.RomReader
{
    public enum RomLayout : byte
    {
        LoRom = 0x20,
        HiRom = 0x21,

        FastLoRom = 0x30,
        FastHiRom = 0x31
    }

    public enum CartrideType : byte
    {
        ROM = 0,
        ROMRAMNoBattery = 0x01,
        SaveRAM = 0x02
    }

    public enum ZoneType
    {
        Invalid,
        NTSC,
        PAL
    }

    public static class RomHeaderConstants
    {
        public const int HeaderLessLoROM = 0x7fc0;
        public const int HeaderLessHiROM = 0xffc0;
        public const int HeaderedLoROM = 0x81c0;
        public const int HeaderedHiROM = 0x101c0;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RomHeader
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=21)]
        public string Name;

        public RomLayout Layout;

        public CartrideType CartrideType;

        private static int ConvertSize(byte val)
        {
            return 1024 * (int)Math.Pow(2, val);
        }

        public byte ROMSizeByte;

        public long TranslatedROMSizeByte
        {
            get { return ConvertSize(ROMSizeByte); }
        }

        public byte RAMSizeByte;

        public long TranslatedRAMSizeByte
        {
            get { return ConvertSize(RAMSizeByte); }
        }

        public byte CountryCode;

        public ZoneType ZoneType
        {
            get
            {
                if (CountryCode < 2)
                    return ZoneType.NTSC;
                else if (CountryCode < 0xd)
                    return ZoneType.PAL;
                else if (CountryCode == 0xd)
                    return ZoneType.NTSC;
                else
                    return ZoneType.Invalid;
            }
        }

        public byte LicenseeCode;

        public bool HaveExtendedHeader
        {
            get { return LicenseeCode == 0x33; }
        }

        public byte Version;

        public ushort Checksum;

        public ushort SNESChecksum;

        public int _Unknown0;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst=6)]
        public ushort[] NativeModeInterupts;

        public int _Unknown1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public ushort[] EmulationModeInterupts;
    }
}
