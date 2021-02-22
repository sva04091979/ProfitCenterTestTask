using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace Client
{
    delegate void Simple();
    delegate void Message(ref BinaryReader data);
    class Program
    {
        public const int heard = sizeof(long) + 4 * sizeof(int);
        static int _port;
        static string _ip;
        static int delay;
        static bool IsStop { set; get; } = false;
        static event Message NewMessageEvent = CCalculator.NewMessage;
        static event Simple GetInfoEvent = CCalculator.GetInfo;
        static event Simple StopEvent = () => { IsStop = true; };
        static void Main()
        {
            Init();
            CCalculator.Init();
            var keyReader = new Thread(new ThreadStart(KeyReader));
            keyReader.Start();
            while (!IsStop)
            {
                var listener = new Thread(new ThreadStart(Listener));
                listener.Start();
                listener.Join();
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
        static void Listener()
        {
            bool isSocket = false;
            try
            {
                var client = new UdpClient(_port);
                var ip = IPAddress.Parse(_ip);
                client.JoinMulticastGroup(ip, 20);
                IPEndPoint endPoint = null;
                Console.WriteLine("Client start.");
                Console.WriteLine("Press enter for statistic show.");
                Console.WriteLine("Press escape for exit.");
                var rnd = new Random(DateTime.Now.Millisecond);
                while (!IsStop)
                {
                    byte[] mess = client.Receive(ref endPoint);
                    MemoryStream xxdata = new MemoryStream(heard + sizeof(ulong) + sizeof(int));
                    xxdata.Write(mess);
                    BinaryReader _xxdata = new BinaryReader(xxdata);
                    _xxdata.BaseStream.Seek(heard, SeekOrigin.Begin);
                    Console.WriteLine(_xxdata.ReadUInt64());
                    Task write = new Task(() =>
                    {
                        MemoryStream data = new MemoryStream(heard + sizeof(ulong) + sizeof(int));
                        data.Write(mess);
                        Task.Run(() =>
                        {
                            BinaryReader _data = new BinaryReader(data);
                            NewMessageEvent(ref _data);
                        });
                    });
                    write.Start();
                    write.Wait();
                    Thread.Sleep(rnd.Next(delay));
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
                IsStop = true;
            }
            finally
            {
                Console.WriteLine("Client stop");
            }
        }
        static void Init()
        {
            var set = new XmlDocument();
            set.Load("ClientConfig.xml");
            _port = int.Parse(set["config"]["global"]["port"].InnerText);
            _ip = set["config"]["global"]["ip"].InnerText;
            delay = int.Parse(set["config"]["client"]["delay"].InnerText);
        }

    }
}
