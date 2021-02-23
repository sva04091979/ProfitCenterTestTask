using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml;

namespace Server
{
    static class Net
    {
        static public int Port { get; set; }
        static public IPAddress IP { get; set; }
        static public int Delay { get; set; }
    }
    static class Data
    {
        public const int heard = 4 * sizeof(int);
        public const int prefix = heard + sizeof(long);
        public const int totalSize = prefix + sizeof(ulong) + sizeof(int);
        static public MemoryStream data = new MemoryStream(totalSize);
    }
    static class Set
    {
        static public int min;
        static public int max;
        static public int maxValue;
        static public int step;
        static public int digits;
    }
    class Program
    {
        static bool isStop = false;
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
            try
            {
                var set = new XmlDocument();
                set.Load("ServerConfig.xml");
                var _min = set["config"]["global"]["min"].InnerText;
                var _max = set["config"]["global"]["max"].InnerText;
                var _tickSize = set["config"]["global"]["tick_size"].InnerText;
                Net.Port = int.Parse(set["config"]["global"]["port"].InnerText);
                Net.IP = IPAddress.Parse(set["config"]["global"]["ip"].InnerText);
                Net.Delay = int.Parse(set["config"]["server"]["delay"].InnerText);
                var posTickSize = _tickSize.IndexOf(".");
                var posMin = _min.IndexOf(".");
                var posMax = _max.IndexOf(".");
                var digitsTickSize = posTickSize < 0 ? 0 : _tickSize.Length - posTickSize - 1;
                var digitsMin = posMin < 0 ? 0 : _min.Length - posMin - 1;
                var digitsMax = posMax < 0 ? 0 : _max.Length - posMin - 1;
                Set.digits = Math.Max(digitsTickSize, Math.Max(digitsMin, digitsMax));
                if (posTickSize != -1) _tickSize = _tickSize.Remove(posTickSize, 1);
                if (posMin != -1) _min = _min.Remove(posMin, 1);
                if (posMax != -1) _max = _max.Remove(posMax, 1);
                if (Set.digits > digitsMin) _min = _min.PadRight(_min.Length + Set.digits - digitsMin, '0');
                if (Set.digits > digitsMax) _max = _max.PadRight(_max.Length + Set.digits - digitsMax, '0');
                if (Set.digits > digitsTickSize) _tickSize = _tickSize.PadRight(_tickSize.Length + Set.digits - digitsTickSize, '0');
                Set.min = int.Parse(_min);
                Set.max = int.Parse(_max);
                Set.step = int.Parse(_tickSize);
                Set.maxValue = (Set.max - Set.min) / Set.step;
                Data.data.Seek(0, SeekOrigin.Begin);
                Data.data.Write(BitConverter.GetBytes(Set.min));
                Data.data.Write(BitConverter.GetBytes(Set.maxValue));
                Data.data.Write(BitConverter.GetBytes(Set.step));
                Data.data.Write(BitConverter.GetBytes(Set.digits));
            }
            catch
            {
                Console.WriteLine("Config file error pr not found");
                isStop = true;
            }
        }
        static void Sender()
        {
            bool isSocket = false;
            try
            {
                var server = new UdpClient();
                var endPoint = new IPEndPoint(Net.IP, Net.Port);
                var rnd = new Random(DateTime.Now.Millisecond);
                ulong count = 0;
                Data.data.Seek(Data.heard, SeekOrigin.Begin);
                Data.data.Write(BitConverter.GetBytes(DateTime.Now.ToBinary()));
                Console.WriteLine("Server start.");
                Console.WriteLine("Press escape for exit.");
                while (!isStop)
                {
                    Data.data.Seek(Data.prefix, SeekOrigin.Begin);
                    Data.data.Write(BitConverter.GetBytes(++count));
                    Data.data.Write(BitConverter.GetBytes(rnd.Next(Set.maxValue)));
                    server.Send(Data.data.GetBuffer(), (int)Data.data.Length, endPoint);
                    Thread.Sleep(rnd.Next(Net.Delay));
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
