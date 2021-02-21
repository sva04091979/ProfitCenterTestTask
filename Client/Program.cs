using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace Client
{
    delegate void Message(int tick);
    delegate void Simple();
    class Program
    {
        const int _port = 3130;
        const string _ip = "235.5.5.11";
        static bool IsStop { set; get; } = false;
        static public object dataLock = new object();
        static event Message NewMessageEvent = CCalculator.Add;
        static event Simple GetInfoEvent = CCalculator.GetInfo;
        static event Simple StopEvent = () => { IsStop = true; };
        static void Main()
        {
            var client = new UdpClient(_port);
            var ip = IPAddress.Parse(_ip);
            var keyReader = new Thread(new ThreadStart(KeyReader));
            keyReader.Start();
            client.JoinMulticastGroup(ip, 20);
            IPEndPoint endPoint = null;
            while (!IsStop)
            {
                var data = client.Receive(ref endPoint);
                var reader = new BinaryReader(new MemoryStream(data));
                NewMessageEvent?.Invoke(reader.ReadInt32());
                //               Console.WriteLine(Encoding.Unicode.GetString(data));
            }
            keyReader.Join();
        }
        static void KeyReader()
        {
            while (!IsStop)
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Escape:
                        StopEvent();
                        break;
                    case ConsoleKey.Enter:
                        GetInfoEvent?.Invoke();
                        break;
                }
        }
    }
}
