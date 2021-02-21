using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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
                var data = Encoding.Unicode.GetBytes("Hi!");
                server.Send(data, data.Length, endPoint);
                Console.WriteLine("Send");
                Thread.Sleep(2000);
            }

        }
        static void Stop()
        {
            Console.ReadKey();
            isStop = true;
        }
    }
}
