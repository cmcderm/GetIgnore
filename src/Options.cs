using System;

namespace GetIgnore
{
    [Flags]
    public enum Options : byte{
        None = 0,
        Verbose = 1 << 0,
        Preview = 1 << 1,
    }
}