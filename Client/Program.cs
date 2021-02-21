using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace Client
{
    class Program
    {
        const int _port = 3130;
        const string _ip = "235.5.5.11";
        static bool isStop = false;
        delegate void Message(double tick);
        static event Message NewMessage;
        static object dataLock=new object();
        static void Main()
        {
            var client = new UdpClient(_port);
            var ip = IPAddress.Parse(_ip);
            var stop = new Thread(new ThreadStart(Stop));
            client.JoinMulticastGroup(ip, 20);
            IPEndPoint endPoint = null;
            while (!isStop)
            {
                var data = client.Receive(ref endPoint);
                var reader = new BinaryReader(new MemoryStream(data));
                NewMessage?.Invoke(reader.ReadDouble());
                Console.WriteLine(Encoding.Unicode.GetString(data));
            }
        }
        static void Stop()
        {
            Console.ReadKey();
            isStop = true;
        }
        static void Calculator()
        {
            double sum = 0;
            double squareSum = 0;
            ulong count = 0;
            Message add = (double it) =>
              {
                  double squareIt = it * it;
                  lock (dataLock)
                  {
                      sum += it;
                      squareSum += squareIt;
                      ++count;
                  }
              };
            NewMessage += add;
            while (!isStop)
            {
                var key=Console.ReadKey();
                if (key.Key == ConsoleKey.Enter)
                {
                    double _sum;
                    double _squareSum;
                    ulong _count;
                    lock (dataLock)
                    {
                        _sum = sum;
                        _squareSum = squareSum;
                        _count = count;
                    }
                    GetInfo(_sum, _squareSum,_count);
                }
            }
            NewMessage -= add;
        }
        static void GetInfo(double sum,double squareSum,ulong count)
        {
            double sma = sum / count;
            double standartDev = squareSum + sma * (2 * sum + count * sma);
            Console.WriteLine($"SMA={sma}");
            Console.WriteLine($"Standart deviation={standartDev}");
        }
    }
}
