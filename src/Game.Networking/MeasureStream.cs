using System;
using System.IO;

namespace Game.Networking
{
    /// <summary>
    /// Stream that simply saves the length of the written data.
    /// </summary>
    public class MeasureStream : Stream
    {
        private long _length;

        public MeasureStream()
        {
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => this._length;

        public override long Position { get; set; }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            this._length = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _length += count;
        }
    }
}
