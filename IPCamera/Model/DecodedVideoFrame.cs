using System;

namespace SimpleRtspPlayer.RawFramesDecoding.DecodedFrames
{
    public class DecodedVideoFrame
    {
        private readonly Action<IntPtr, int> _transformAction;

        public DecodedVideoFrame(Action<IntPtr, int> transformAction)
        {
            _transformAction = transformAction;
        }

        public void TransformTo(IntPtr buffer, int bufferStride)
        {
            _transformAction(buffer, bufferStride);
        }
    }
}