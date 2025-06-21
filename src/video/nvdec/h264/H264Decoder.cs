using System;
using System.Runtime.InteropServices;

namespace Pinez.Video.Nvdec.H264
{
    public class DecodedFrame
    {
        public byte[] YPlane { get; }
        public byte[] UPlane { get; }
        public byte[] VPlane { get; }
        public int Width { get; }
        public int Height { get; }

        public DecodedFrame(int width, int height, byte[] y, byte[] u, byte[] v)
        {
            Width = width;
            Height = height;
            YPlane = y;
            UPlane = u;
            VPlane = v;
        }
    }

    public sealed unsafe class H264Decoder : IDisposable
    {
        private readonly IntPtr _codec;
        private readonly IntPtr _context;
        private readonly IntPtr _packet;
        private readonly IntPtr _frame;

        public H264Decoder()
        {
            const int AV_CODEC_ID_H264 = 27;
            _codec = FFmpegNative.avcodec_find_decoder(AV_CODEC_ID_H264);
            if (_codec == IntPtr.Zero)
            {
                throw new InvalidOperationException("H264 codec not found in FFmpeg");
            }

            _context = FFmpegNative.avcodec_alloc_context3(_codec);
            if (_context == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to allocate codec context");
            }

            if (FFmpegNative.avcodec_open2(_context, _codec, IntPtr.Zero) < 0)
            {
                throw new InvalidOperationException("Could not open codec");
            }

            _packet = FFmpegNative.av_packet_alloc();
            _frame = FFmpegNative.av_frame_alloc();
        }

        public DecodedFrame Decode(ReadOnlySpan<byte> bitstream)
        {
            AVPacket* pkt = (AVPacket*)_packet;
            AVFrame* frame = (AVFrame*)_frame;

            fixed (byte* ptr = bitstream)
            {
                pkt->data = ptr;
                pkt->size = bitstream.Length;

                if (FFmpegNative.avcodec_send_packet(_context, _packet) < 0)
                {
                    FFmpegNative.av_packet_unref(_packet);
                    return null;
                }
            }

            int ret = FFmpegNative.avcodec_receive_frame(_context, _frame);
            FFmpegNative.av_packet_unref(_packet);
            if (ret < 0)
            {
                return null;
            }

            int width = frame->width;
            int height = frame->height;
            IntPtr data0 = frame->data0;
            IntPtr data1 = frame->data1;
            IntPtr data2 = frame->data2;
            int stride0 = frame->linesize0;
            int stride1 = frame->linesize1;

            byte[] y = new byte[stride0 * height];
            byte[] u = new byte[stride1 * ((height + 1) / 2)];
            byte[] v = new byte[stride1 * ((height + 1) / 2)];

            Marshal.Copy(data0, y, 0, y.Length);
            Marshal.Copy(data1, u, 0, u.Length);
            Marshal.Copy(data2, v, 0, v.Length);

            FFmpegNative.av_frame_unref(_frame);

            return new DecodedFrame(width, height, y, u, v);
        }

        public void Dispose()
        {
            IntPtr frame = _frame;
            FFmpegNative.av_frame_free(ref frame);
            IntPtr pkt = _packet;
            FFmpegNative.av_packet_free(ref pkt);
        }
    }
}
