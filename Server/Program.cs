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
        const int digits = 5;
        const int step = 1;
        const int sleepMin = 100;
        const int sleepMax = 750;
        const int min = 100000;
        const int max = 200000;
        static bool isStop = false;
        static void Main()
        {
            var server = new UdpClient();
            var ip = IPAddress.Parse(_ip);
            var endPoint = new IPEndPoint(ip, _port);
            var stop = new Thread(new ThreadStart(Stop));
            stop.Start();
            int _max = min + (max - min) / step;
            var rnd = new Random(DateTime.Now.Millisecond);
            while (!isStop) 
            {
                int val = min + (rnd.Next(min, _max) - min) * step;
                server.Send(BitConverter.GetBytes(val), sizeof(int), endPoint);
                Thread.Sleep(rnd.Next(sleepMin,sleepMax));
            }
        }
        static void Stop()
        {
            if (Console.ReadKey().Key==ConsoleKey.Escape)
                isStop = true;
        }
    }
}
