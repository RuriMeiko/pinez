using System;
using System.Runtime.InteropServices;

namespace Pinez.Video.Nvdec.H264
{
    internal static class FFmpegNative
    {
        private const string AvCodec = "avcodec";
        private const string AvUtil = "avutil";

        [DllImport(AvCodec, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr avcodec_find_decoder(int id);

        [DllImport(AvCodec, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr avcodec_alloc_context3(IntPtr codec);

        [DllImport(AvCodec, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avcodec_open2(IntPtr ctx, IntPtr codec, IntPtr options);

        [DllImport(AvCodec, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avcodec_send_packet(IntPtr ctx, IntPtr pkt);

        [DllImport(AvCodec, CallingConvention = CallingConvention.Cdecl)]
        public static extern int avcodec_receive_frame(IntPtr ctx, IntPtr frame);

        [DllImport(AvCodec, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr av_packet_alloc();

        [DllImport(AvCodec, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_packet_free(ref IntPtr pkt);

        [DllImport(AvCodec, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_packet_unref(IntPtr pkt);

        [DllImport(AvUtil, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr av_frame_alloc();

        [DllImport(AvUtil, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_frame_free(ref IntPtr frame);

        [DllImport(AvUtil, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_frame_unref(IntPtr frame);
    }
}
