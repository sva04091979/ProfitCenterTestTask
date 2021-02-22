using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml;

namespace Server
{
    class Program
    {
        const int _port = 3130;
        const string _ip = "235.5.5.11";
        const int sleepMin = 100;
        const int sleepMax = 750;
        static int min;
        static int max;
        static int step = 1;
        static bool isStop = false;
        static int digits;
        static void Main()
        {
            Init();
            int maxValue = (max - min) / step;
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
        static void Init()
        {
            var set = new XmlDocument();
            set.Load("ServerConfig.xml");
            var _min=set["config"]["global"]["min"].InnerText;
            var _max= set["config"]["global"]["max"].InnerText;
            var _tickSize= set["config"]["global"]["tick_size"].InnerText;
            var posTickSize = _tickSize.IndexOf(".");
            var posMin = _min.IndexOf(".");
            var posMax = _max.IndexOf(".");
            var digitsTickSize = posTickSize<0?0:_tickSize.Length - posTickSize - 1;
            var digitsMin = posMin<0?0:_min.Length - posMin - 1;
            var digitsMax = posMax<0?0:_max.Length - posMin - 1;
            var digits = Math.Max(digitsTickSize, Math.Max(digitsMin, digitsMax));
            if (posTickSize != -1) _tickSize = _tickSize.Remove(posTickSize, 1);
            if (posMin != -1) _min = _min.Remove(posMin, 1);
            if (posMax != -1) _max = _max.Remove(posMax, 1);
            if (digits > digitsMin) _min = _min.PadRight(_min.Length+digits - digitsMin, '0');
            if (digits > digitsMax) _max = _max.PadRight(_max.Length+digits - digitsMax, '0');
            if (digits> digitsTickSize) _tickSize = _tickSize.PadRight(_tickSize.Length+digits - digitsTickSize, '0');
            min = int.Parse(_min);
            max = int.Parse(_max);
            step = int.Parse(_tickSize);
            Console.WriteLine(digitsTickSize);
            Console.WriteLine(digitsMin);
            Console.WriteLine(digitsMax);
            Console.WriteLine(min);
            Console.WriteLine(max);
            Console.WriteLine(step);
        }
    }
}
