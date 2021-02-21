using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace Server
{
    class Program
    {
        const int _port = 3130;
        const string _ip = "235.5.5.11";
        static bool isStop = false;
        static void Main()
        {
            var server = new UdpClient();
            var ip = IPAddress.Parse(_ip);
            var endPoint = new IPEndPoint(ip, _port);
            var stop = new Thread(new ThreadStart(Stop));
            stop.Start();
            while (!isStop) 
            {
                x.Write(1.5);
                byte[] data;
                x.BaseStream.Read(new Span<byte> data);
                server.Send(x, sizeof(x), endPoint);
                Console.WriteLine("Send");
                Thread.Sleep(100);
            }

        }
        static void Stop()
        {
            if (Console.ReadKey().Key==ConsoleKey.Escape)
                isStop = true;
        }
    }
}
