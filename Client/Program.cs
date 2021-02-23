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
    static class DataSize
    {
        public const int heard = 4 * sizeof(int);
        public const int prefix = heard + sizeof(long);
        public const int totalSize = prefix + sizeof(ulong) + sizeof(int);
    }
    static class Net
    {
        public static int port;
        public static IPAddress ip;
        public static int delay;
        static public void Init(ref XmlDocument set)
        {
            port = int.Parse(set["config"]["global"]["port"].InnerText);
            ip = IPAddress.Parse(set["config"]["global"]["ip"].InnerText);
            delay = int.Parse(set["config"]["client"]["delay"].InnerText);
        }
    }
    delegate void Simple();
    delegate void Message(ref BinaryReader data);
    class Program
    {
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
                var client = new UdpClient(Net.port);
                client.JoinMulticastGroup(Net.ip, 20);
                IPEndPoint endPoint = null;
                Console.WriteLine("Client start.");
                Console.WriteLine("Press enter for statistic show.");
                Console.WriteLine("Press escape for exit.");
                var rnd = new Random(DateTime.Now.Millisecond);
                while (!IsStop)
                {
                    byte[] mess = client.Receive(ref endPoint);
                    Task write = new Task(() =>
                    {
                        MemoryStream data = new MemoryStream(DataSize.totalSize);
                        data.Write(mess);
                        Task.Run(() =>
                        {
                            BinaryReader _data = new BinaryReader(data);
                            NewMessageEvent(ref _data);
                        });
                    });
                    write.Start();
                    write.Wait();
                    Thread.Sleep(rnd.Next(Net.delay));
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
            try
            {
                set.Load("ClientConfig.xml");
                Net.Init(ref set);
            }
            catch (XmlException)
            {
                Console.WriteLine("Config file error");
                IsStop = true;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Config file not found");
                IsStop = true;
            }
            catch
            {
                Console.WriteLine("Config data error (port,ip or delay");
                IsStop = true;
            }
        }

    }
}
