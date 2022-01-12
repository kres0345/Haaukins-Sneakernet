using System;
using System.IO;
using System.Threading;

namespace HaaukinStream
{
    public class SemiDuplexTimeDependantStream : Stream
    {
        private readonly Stream wrappedStream;

        public SemiDuplexTimeDependantStream(Stream stream)
        {
            wrappedStream = stream;
        }

        /// <summary>
        /// Writes on even digits of units of time,
        /// reads on odd.
        /// </summary>
        protected bool WriteIsEven = true;

        public override bool CanRead => wrappedStream.CanRead;

        public override bool CanSeek => wrappedStream.CanSeek;

        public override bool CanWrite => wrappedStream.CanWrite;

        public override long Length => wrappedStream.Length;

        public override long Position { get => wrappedStream.Position; set => wrappedStream.Position = value; }

        public override void Flush()
        {
            // Gives 1 second to do reading.
            AwaitParitySecond(WriteIsEven);

            wrappedStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Gives 1 second to do reading.
            AwaitParitySecond(!WriteIsEven);

            return wrappedStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return wrappedStream.Seek(offset, origin);
        }

        public override void SetLength(long value) => wrappedStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
        {
            AwaitParitySecond(WriteIsEven);

            wrappedStream.Write(buffer, offset, count);
        }

        private int GetTimeUnit() => DateTime.UtcNow.Second;

        private void AwaitParitySecond(bool waitForEven)
        {
            int targetRemainder = waitForEven ? 0 : 1;
            int currentTime;
            int startTime = GetTimeUnit();

            do
            {
                currentTime = GetTimeUnit();
                Thread.Sleep(0);
            } while (currentTime % 2 != targetRemainder || startTime == currentTime);
        }
    }
}
