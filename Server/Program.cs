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
        const int heard = sizeof(long) + 4 * sizeof(int);
        static int _port;
        static string _ip;
        static int sleepMax;
        static int min;
        static int max;
        static int maxValue;
        static int step;
        static int digits;
        static bool isStop = false;
        static MemoryStream data = new MemoryStream(heard + sizeof(ulong) + sizeof(int));
        static void Main()
        {
            Init();
            var stop = new Thread(new ThreadStart(Stop));
            stop.Start();
            while (!isStop)
            {
                var sender = new Thread(new ThreadStart(Sender));
                sender.Start();
                sender.Join();
            }
            stop.Join();
        }
        static void Stop()
        {
            while (!isStop)
            {
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                    isStop = true;
            }
        }
        static void Init()
        {
            var set = new XmlDocument();
            set.Load("ServerConfig.xml");
            var _min=set["config"]["global"]["min"].InnerText;
            var _max= set["config"]["global"]["max"].InnerText;
            var _tickSize= set["config"]["global"]["tick_size"].InnerText;
            _port= int.Parse(set["config"]["global"]["port"].InnerText);
            _ip = set["config"]["global"]["ip"].InnerText;
            sleepMax = int.Parse(set["config"]["server"]["delay"].InnerText);
            var posTickSize = _tickSize.IndexOf(".");
            var posMin = _min.IndexOf(".");
            var posMax = _max.IndexOf(".");
            var digitsTickSize = posTickSize<0?0:_tickSize.Length - posTickSize - 1;
            var digitsMin = posMin<0?0:_min.Length - posMin - 1;
            var digitsMax = posMax<0?0:_max.Length - posMin - 1;
            digits = Math.Max(digitsTickSize, Math.Max(digitsMin, digitsMax));
            if (posTickSize != -1) _tickSize = _tickSize.Remove(posTickSize, 1);
            if (posMin != -1) _min = _min.Remove(posMin, 1);
            if (posMax != -1) _max = _max.Remove(posMax, 1);
            if (digits > digitsMin) _min = _min.PadRight(_min.Length+digits - digitsMin, '0');
            if (digits > digitsMax) _max = _max.PadRight(_max.Length+digits - digitsMax, '0');
            if (digits> digitsTickSize) _tickSize = _tickSize.PadRight(_tickSize.Length+digits - digitsTickSize, '0');
            min = int.Parse(_min);
            max = int.Parse(_max);
            step = int.Parse(_tickSize);
            maxValue = (max - min) / step;
            data.Seek(sizeof(long), SeekOrigin.Begin);
            data.Write(BitConverter.GetBytes(min));
            data.Write(BitConverter.GetBytes(maxValue));
            data.Write(BitConverter.GetBytes(step));
            data.Write(BitConverter.GetBytes(digits));
        }
        static void Sender()
        {
            bool isSocket = false;
            try
            {
                var server = new UdpClient();
                var ip = IPAddress.Parse(_ip);
                var endPoint = new IPEndPoint(ip, _port);
                var rnd = new Random(DateTime.Now.Millisecond);
                ulong count = 0;
                data.Seek(0, SeekOrigin.Begin);
                data.Write(BitConverter.GetBytes(DateTime.Now.ToBinary()));
                Console.WriteLine("Server start.");
                Console.WriteLine("Press escape for exit.");
                while (!isStop)
                {
                    data.Seek(heard, SeekOrigin.Begin);
                    data.Write(BitConverter.GetBytes(++count));
                    data.Write(BitConverter.GetBytes(rnd.Next(maxValue)));
                    server.Send(data.GetBuffer(), (int)data.Length, endPoint);
                    Console.WriteLine(count);
                    Thread.Sleep(rnd.Next(sleepMax));
                }
            }
            catch (SocketException)
            {
                if (!isSocket)
                {
                    isSocket = true;
                    Console.WriteLine("Net error");
                }
                Thread.Sleep(100);
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Wrong ip or port");
                Console.WriteLine("Press any key for exit");
                isStop = true;
            }
            finally
            {
                Console.WriteLine("Server stop");
            }
        }
    }
}
