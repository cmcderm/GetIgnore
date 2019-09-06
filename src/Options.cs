using System;

namespace GetIgnore
{
    // Option name MUST match the long form option name (e.g. --verbose)
    [Flags]
    public enum Options : byte{
        None = 0,
        Verbose = 1 << 0,
        Preview = 1 << 1,
    }
}