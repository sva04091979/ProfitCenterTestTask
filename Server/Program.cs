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
        const int sleepMin = 100;
        const int sleepMax = 750;
        const int min = 100000;
        const int max = 200000;
        const int step = 1;
        const int maxValue = (max - min) / step;
        static bool isStop = false;
        static void Main()
        {
            var server = new UdpClient();
            var ip = IPAddress.Parse(_ip);
            var endPoint = new IPEndPoint(ip, _port);
            var stop = new Thread(new ThreadStart(Stop));
            stop.Start();
            var rnd = new Random(DateTime.Now.Millisecond);
            while (!isStop) 
            {
                int val = rnd.Next(maxValue);
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
