using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;

#nullable enable
namespace HostStream
{
    // Hvis den overskrides, splitter Guacamole den op i flere blobs.
    //private const int MaxBlockSize = 8064;

    internal class BrowserGuacamoleMirror : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            HostStream.websocketInstance = this;
            Debug.WriteLine("Connected");
        }

        public void UpdateClipboard(string encodedData)
        {
            /* 9.clipboard,1.0,10.text/plain;
             * 4.blob,1.0,12.YWJjY2FiYw==;
             * 3.end,1.0;
             */
            // 4.blob,1.0,4.YWJj;

            StringBuilder request = new StringBuilder();
            request.Append("9.clipboard,1.0,10.text/plain;");
            request.Append("4.blob,1.0,");

            request.Append(encodedData.Length);
            request.Append('.');
            request.Append(encodedData);
            request.Append(';');

            request.Append("3.end,1.0;");

            Send(request.ToString());
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            /* 9.clipboard,1.0,10.text/plain;
             * 3.img,1.3,2.14,1.0,9.image/png,3.538,3.283;
             * 4.blob,1.0,4.YWJj;
             * 3.end,1.0;
             * 4.blob,1.3,760.iVBORw0KGgoAAAANSUhEUgAAAMIAAAEMCAMAAAChqplzAAAAZlBMVEUQEBAwMDAgMEgYLEg4bMBIhOhAdNAoSHhAfOAQFBgYJDggPGgoTIA4YKhIiPAwWJgQHCA4cMhAcMggPGAYLEAwXKAQGCAYKEA4cMAoQGgwUIg4aLgQFBAYJDAgNFBAeNAoRHAgJChkiMrUAAABjklEQVR4nO3VyVVDMRAAQQH+bAYDZjM75J8kPhEAF0q87gimRtLTGD8dTNo43A9/tFqOT+YlnJ6N82W1vricl7A5HFfL9dQXabMeN9u538LmdmxnJ9yN++VhasLxGI/L7un55fWvR/lt422///fd8vE57yn8g68tAlAEofE1fRGEIghFEIogFEEoglAEoQhCEYQiCEUQiiAUQSiCUAShCEIRhCIIRRCKIBRBKIJQBKEIQhGEIghFEIogFEEoglAEoQhCEYQiCEUQiiAUQSiCUAShCEIRhCIIRRCKIBRBKIJQBKEIQhGEIghFEIogFEEoglAEoQhCEYQiCEUQiiAUQSiCUAShCEIRhCIIRRCKIBRBKIJQBKEIQhGEIghFEIogFEEoglAEoQhCEYQiCEUQiiAUQSiCUAShCEIRhCIIRRCKIBRBKIJQBKEIQhGEIghFEIogFEEoglAEoQhCEYQiCEUQiiAUQSiCUAShCEIRhCIIRRCKIBRBKIJQBKEIQhGEIghFEIogFEEogtA/IHwDCDzeGmgJ/PoAAAAASUVORK5CYII=;
             * 3.end,1.3;
             * 4.sync,10.9063784212;
             */
            //Console.WriteLine($"msg: {e.Data}");

            string[] commands = e.Data.Split(';', StringSplitOptions.RemoveEmptyEntries);
            string? argumentDirective = null;
            StringBuilder blobStream = new StringBuilder();

            foreach (var command in commands)
            {
                string[] arguments = command.Split(',');
                Debug.WriteLine($"Command: {arguments[0]}");

                if (arguments.Length > 1 && arguments[0] != "4.sync" && arguments[0] != "4.size")
                {
                    if (arguments[0] == "3.end")
                    {
                        Console.WriteLine($"Directive end: {arguments[1]}");
                    }
                    else
                    {
                        Console.WriteLine($"Directive: {arguments[1]} - {arguments[0]}");
                    }
                }

                if (arguments[0] != "9.clipboard" && argumentDirective is null)
                {
                    continue;
                }

                if (!(argumentDirective is null) && argumentDirective != arguments[1])
                {
                    // Not associated with clipboard.
                    continue;
                }

                switch (arguments[0])
                {
                    case "9.clipboard":
                        argumentDirective = arguments[1];
                        blobStream = new StringBuilder();
                        break;
                    case "4.blob":
                        string[] subArguments = arguments[2].Split('.');
                        int blobSize = int.Parse(subArguments[0]);

                        blobStream.Append(subArguments[1]);
                        break;
                    case "3.end":
                        argumentDirective = null;

                        string encoded = blobStream.ToString();
                        HostStream.ReceivedClipboard(encoded);

                        string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                        Console.WriteLine($"Clipboard content: {decoded}");
                        break;
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// A stream for transfering data between Guacamole host and guest using clipboard.
    /// </summary>
    public class HostStream : Stream
    {
        private const bool WriteIsEven = true;
        internal static BrowserGuacamoleMirror? websocketInstance;
        private readonly WebSocketSharp.Server.WebSocketServer wssv;

        /// <summary>
        /// Contains serialized data.
        /// </summary>
        private static readonly Queue<byte> receivedData = new Queue<byte>();

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => false;

        public override bool CanWrite => !(websocketInstance is null);

        public override long Length => receivedData.Count;

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public HostStream(string hostAddress="ws://127.0.0.1:8080")
        {
            wssv = new WebSocketSharp.Server.WebSocketServer(hostAddress);
            wssv.AddWebSocketService<BrowserGuacamoleMirror>("/");
            wssv.Start();
        }

        internal static void ReceivedClipboard(string data)
        {
            byte[] convertedData = Convert.FromBase64String(data);

            for (int i = 0; i < convertedData.Length; i++)
            {
                receivedData.Enqueue(convertedData[i]);
            }
        }

        public override void Close()
        {
            wssv.Stop();
            base.Close();
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

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int i = offset;
            int readBytes = 0;
            while (readBytes < count)
            {
                if (receivedData.Count == 0)
                {
                    // No more data received, waiting for more.
                    Thread.Sleep(10);
                    continue;
                }

                readBytes++;
                buffer[i++] = receivedData.Dequeue();
            }
            
            // should always be the same amount.
            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // TODO: maybe implement some kind of division into chunks, if Guacamoles' isn't sufficient.
            ArraySegment<byte> bufferSpan = new ArraySegment<byte>(buffer, offset, count);
            string encoded = Convert.ToBase64String(bufferSpan);
            
            AwaitParitySecond(WriteIsEven);

            if (websocketInstance is null)
            {
                Console.WriteLine("Not connected, can't write");
            }
            else
            {
                websocketInstance.UpdateClipboard(encoded);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

    }
}
