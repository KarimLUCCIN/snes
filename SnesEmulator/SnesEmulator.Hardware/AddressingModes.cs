using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnesEmulator.Hardware
{
    public enum AddressingModes
    {
        Implied,
        ImmediateMemoryFlag,
        ImmediateIndexFlag,
        Immediate8Bit,
        Relative,
        RelativeLong,
        Direct,
        DirectIndexedX,
        DirectIndexedY,
        DirectIndirect,
        DirectIndexedIndirect,
        DirectIndirectIndexed,
        DirectIndirectLong,
        DirectIndirectIndexedLong,
        Absolute,
        AbsoluteIndexedX,
        AbsoluteIndexedY,
        AbsoluteLong,
        AbsoluteIndexedLong,
        StackRelative,
        StackRelativeIndirectIndexed,
        AbsoluteIndirect,
        AbsoluteIndirectLong,
        AbsoluteIndexedIndirect,
        ImpliedAccumulator,
        BlockMove
    }
}
