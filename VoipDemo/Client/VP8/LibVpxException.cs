using System;
using System.Runtime.InteropServices;

using Android.Runtime;

namespace Legion.Client.VP8
{
    class LibVpxException : Exception
    {
        public LibVpxException(string msg)
            : base(msg)
        { }
    }
}