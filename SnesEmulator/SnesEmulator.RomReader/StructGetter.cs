using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace SnesEmulator.RomReader
{
    public static class StructGetter
    {
        public static void WriteBlock(Stream Output, object structure)
        {
            byte[] buf = new byte[SizeOf(structure)];
            WriteBlock(Output, structure, buf);
        }

        public static void WriteBlock(Stream Output, object structure, byte[] buf)
        {
            GetBytes(structure, buf, 0, buf.Length);
            Output.Write(buf, 0, SizeOf(structure));
        }

        public static object ReadBlock(Stream Input, Type structure_type)
        {
            byte[] buf = new byte[Marshal.SizeOf(structure_type)];
            return ReadBlock(Input, structure_type, buf);
        }

        public static object ReadBlock(Stream Input, Type structure_type, byte[] buf)
        {
            if (Input.Read(buf, 0, Marshal.SizeOf(structure_type)) < Marshal.SizeOf(structure_type))
                throw new InvalidDataException("Stream too short");

            return GetStruct(buf, 0, Marshal.SizeOf(structure_type), structure_type);
        }

        public static int SizeOf(object structure)
        {
            return Marshal.SizeOf(structure);
        }

        public static void GetBytes(object val,
                                      [In, Out] byte[] buffer,
                                     int offset,
                                    int bufsize)
        {
            if (val == null)
                throw new ArgumentNullException("val");

            int size = SizeOf(val);
            IntPtr p = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.StructureToPtr(val, p, true);
                Marshal.Copy(p, buffer, offset, bufsize);
            }
            finally
            {
                Marshal.FreeHGlobal(p);
            }
        }

        public static object GetStruct(byte[] buffer,
                                       int offset,
                                       int count,
                                       Type struct_type)
        {
            IntPtr p = Marshal.AllocHGlobal(count);
            object result = null;

            try
            {
                Marshal.Copy(buffer, offset, p, count);
                result = Marshal.PtrToStructure(p, struct_type);
            }
            finally
            {
                Marshal.FreeHGlobal(p);
            }

            return result;
        }
    }
}
