using System;
using System.Collections.Generic;
using System.IO;

namespace Client
{
    struct Stat
    {
        public double sum;
        public double squareSum;
        public ulong dataPass;
        public ulong count;
        public IStatistic stat;
    }
    struct Set
    {
        const int maxDataSize = 1000;
        public int digits;
        public int step;
        public int min;
        public int maxValue;
        public  int dataSplit;
        public int dataSize;
        public double digitK;
        public void Init(ref BinaryReader data)
        {
            data.BaseStream.Seek(0, SeekOrigin.Begin);
            min = data.ReadInt32();
            maxValue = data.ReadInt32();
            step = data.ReadInt32();
            digits = data.ReadInt32();
            dataSplit = maxValue > maxDataSize ? maxValue / maxDataSize + 1 : 1;
            dataSize = maxValue % dataSplit == 0 ? maxValue / dataSplit : maxValue / dataSplit + 1;
            digitK = Math.Pow(10, digits);
        }
    }
    static class CCalculator
    {
        static public Stat stat = new Stat { sum = 0.0, squareSum = 0.0, dataPass = 0, count=0, stat=null };
        static public Set set;
        static ulong lastData = 0;
        static long session=0;
        static public ulong[] arr;

        static public Message NewMessage = Mess;
        static Message Adder = FirstAdd;
        static object locker = new object();
        static public void Init()
        {
            if (set.dataSplit == 1)
                stat.stat = new SimpleStatistic();
            else
                stat.stat = new RangeStatistic();
        }
        static void InitValues(ref BinaryReader data)
        {
            set.Init(ref data);
            arr = new ulong[set.dataSize];
        }
        static void Mess(ref BinaryReader data)
        {
            lock (locker)
            {
                Adder(ref data);
            }
        }
        static public void FirstAdd(ref BinaryReader data)
        {
            InitValues(ref data);
            Adder -= FirstAdd;
            Adder += Add;
            Adder(ref data);
        }
        static public void Add(ref BinaryReader data)
        {
            data.BaseStream.Seek(DataSize.heard, SeekOrigin.Begin);
            long _session = data.ReadInt64();
            if (session == _session) CheckPass(data.ReadUInt64());
            else StartSession(_session,data.ReadUInt64());
            int value = data.ReadInt32();
            double it = (set.min + value * set.step) / set.digitK;
            double squareIt = it * it;
            int i = value / set.dataSplit;
            stat.sum += it;
            stat.squareSum += squareIt;
            ++stat.count;
            stat.stat.Add(i);
        }
        static void CheckPass(ulong count)
        {
            if (++lastData < count)
            {
                stat.dataPass += count - lastData;
                lastData = count;
            }
        }
        static void StartSession(long _session,ulong count)
        {
            if (session != 0)
            {
                stat.dataPass = count - 1;
            }
            session = _session;
            lastData = count;
            Console.WriteLine("Connect with server.");
        }
        static public void GetInfo()
        {
            if (arr == null)
            {
                Console.WriteLine("No history");
                return;
            }
            var timer = DateTime.Now.Ticks;
            double sma, standartDev;
            double mediana=0;
            List<double> moda=new List<double>();
            lock (locker)
            {   
                sma = stat.sum / stat.count;
                standartDev = Math.Sqrt((stat.squareSum + sma * (stat.count * sma - 2 * stat.sum)) / stat.count);
                stat.stat.Stat(ref mediana,ref moda);
            }
            timer = DateTime.Now.Ticks - timer;
            Console.WriteLine($"SMA={sma}");
            Console.WriteLine($"Standart deviation={standartDev}");
            Console.WriteLine($"Mediana={mediana+set.min/set.digitK}");
            foreach (var it in moda)
                Console.WriteLine($"Moda={it+set.min/set.digitK}");
            Console.WriteLine($"{stat.dataPass} datagrams passed");
            Console.WriteLine($"Time: {timer} ticks");
            Console.WriteLine($"Time: {timer / 10000.0} ms");
        }
    };
}