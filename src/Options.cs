using System;

namespace GetIgnore
{
    // Option name MUST match the long form option name (e.g. --verbose)
    [Flags]
    public enum Options : byte{
        None = 0,           //0x00000000
        Verbose = 1 << 0,   //0x00000001
        Preview = 1 << 1,   //0x00000010
        Append = 1 << 2,    //0x00000100
        No = 1 << 3,        //0x00001000
        Force = 1 << 4,     //0x00010000
        Nocache = 1 << 5,   //0x00100000
    }
}