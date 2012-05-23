using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware
{
    public static class Mode8Bits
    {
        public const int UnsignedMax = 255;
        public const int SignedMax = 127;
        public const int SignedMin = -128;
    }

    public static class Mode16Bits
    {
        public const int UnsignedMax = 65535;
        public const int SignedMax = 32767;
        public const int SignedMin = -32768;
    }
}
