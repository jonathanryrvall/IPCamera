using System;
using System.Collections.Generic;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Video;
using SimpleRtspPlayer.RawFramesDecoding.DecodedFrames;
using SimpleRtspPlayer.RawFramesDecoding.FFmpeg;
using SimpleRtspPlayer.RawFramesReceiving;

namespace SimpleRtspPlayer.GUI
{
    public class RealtimeVideoSource
    {
        FFmpegVideoDecoder decoder;


        public event EventHandler<DecodedVideoFrame> FrameReceived;

        public void SetRawFramesSource(RawFramesSource rawFramesSource)
        {
             rawFramesSource.FrameReceived += OnFrameReceived;
        }

        
       

        private void OnFrameReceived(object sender, RawFrame rawFrame)
        {
            if (!(rawFrame is RawVideoFrame rawVideoFrame))
                return;

            FFmpegVideoDecoder decoder = GetDecoderForFrame(rawVideoFrame);

            DecodedVideoFrame decodedFrame = decoder.TryDecode(rawVideoFrame);

            if (decodedFrame != null)
                FrameReceived?.Invoke(this, decodedFrame);
        }

        private FFmpegVideoDecoder GetDecoderForFrame(RawVideoFrame videoFrame)
        {
            FFmpegVideoCodecId codecId = DetectCodecId(videoFrame);
            if (decoder == null)
            {
                decoder = FFmpegVideoDecoder.CreateDecoder(codecId);
            }
         


            return decoder;
        }

        private FFmpegVideoCodecId DetectCodecId(RawVideoFrame videoFrame)
        {
            if (videoFrame is RawJpegFrame)
                return FFmpegVideoCodecId.MJPEG;
            if (videoFrame is RawH264Frame)
                return FFmpegVideoCodecId.H264;

            throw new ArgumentOutOfRangeException(nameof(videoFrame));
        }
    }
}