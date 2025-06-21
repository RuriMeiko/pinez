using System;
using System.Runtime.InteropServices;

namespace Pinez.Video.Nvdec.H264
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct AVPacket
    {
        public IntPtr buf;
        public long pts;
        public long dts;
        public byte* data;
        public int size;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AVRational
    {
        public int num;
        public int den;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AVFrame
    {
        public IntPtr data0;
        public IntPtr data1;
        public IntPtr data2;
        public IntPtr data3;
        public IntPtr data4;
        public IntPtr data5;
        public IntPtr data6;
        public IntPtr data7;

        public int linesize0;
        public int linesize1;
        public int linesize2;
        public int linesize3;
        public int linesize4;
        public int linesize5;
        public int linesize6;
        public int linesize7;

        public IntPtr extended_data;
        public int width;
        public int height;
        public int nb_samples;
        public int format;
    }
}
