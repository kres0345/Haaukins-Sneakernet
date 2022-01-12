using System;
using System.Threading;

namespace HostStreamTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("!");
            Random rng = new Random();

            HostStream.HostStream server = new HostStream.HostStream();
            while (true)
            {
                if (!server.CanWrite)
                {
                    Thread.Sleep(100);
                    continue;
                }

                byte[] randomData = new byte[1024];
                rng.NextBytes(randomData);
                server.Write(randomData, 0, 1024);
                Console.WriteLine($"Wrote data: {randomData.Length}");

                //byte readByte = (byte)server.ReadByte();
                //Console.WriteLine("byte read: " + server.ReadByte());
                //server.WriteByte((byte)(readByte + 1));
                Thread.Sleep(500);
            }

            Console.ReadKey();
        }
    }
}
