using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DvdNavigatorCrm
{
    public class TraceEventArgs : EventArgs
    {
        public TraceEventArgs(string text)
        {
            this.Text = text;
        }

        public string Text { get; private set; }
    }

    public class LoadedBytesEventArgs : EventArgs
    {
        public LoadedBytesEventArgs(int byteCount)
        {
            this.ByteCount = byteCount;
        }

        public int ByteCount { get; private set; }
    }

    public class TypeEventArgs : EventArgs
    {
        public TypeEventArgs(Type type)
        {
            this.Type = type;
        }

        public Type Type { get; private set; }
    }
}
