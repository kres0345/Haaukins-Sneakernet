using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

#nullable enable
namespace GuestStream
{
    public static class ClipboardController
    {
        private static Process? inputProcess = null;

        public static string GetContent()
        {
            var cmd = "xsel -o";
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }

        public static void SetContent(ArraySegment<byte> content)
        {
            if (!(inputProcess is null))
            {
                inputProcess.StandardInput.Write(Convert.ToBase64String(content));
                inputProcess.StandardInput.Write('\0');
                return;
            }

            var cmd = "xsel -fz";
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            inputProcess = process;
        }
    }

    public class GuestStream : Stream
    {
        private const bool WriteIsEven = false;

        public GuestStream()
        {

        }

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotSupportedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ArraySegment<byte> bufferSegment = new ArraySegment<byte>(buffer, offset, count);

            AwaitParitySecond(WriteIsEven);
            ClipboardController.SetContent(bufferSegment);
        }

        private void AwaitParitySecond(bool waitForEven)
        {
            int targetRemainder = waitForEven ? 0 : 1;
            int currentTime;
            //int startTime = GetTimeUnit();

            do
            {
                currentTime = GetTimeUnit();
                Thread.Sleep(10);
            } while (currentTime % 2 != targetRemainder); //|| startTime == currentTime);
        }

        private int GetTimeUnit() => DateTime.UtcNow.Second;
    }
}
