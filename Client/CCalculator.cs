using System;
using System.Collections.Generic;
using System.IO;

namespace Client
{
    static class CCalculator
    {
        const int maxDataSize = 1000;
        static double sum = 0.0;
        static double squareSum = 0.0;
        static ulong dataPass = 0;
        static ulong lastData = 0;
        static public ulong Count { get; set; } = 0;
        static int digits;
        static int step;
        static int min;
        public static int maxValue;
        public static int dataSplit;
        static int dataSize;
        static public double digitK;
        static public ulong[] arr;
        static IStatistic stat;

        static long session;
        static public Message NewMessage = Mess;
        static Message Adder = FirstAdd;
        static object locker = new object();
        static public void Init()
        {
            if (dataSplit == 1)
                stat = new SimpleStatistic();
            else
                stat = new RangeStatistic();
        }
        static void InitValues(ref BinaryReader data)
        {
            data.BaseStream.Seek(0, SeekOrigin.Begin);
            session = data.ReadInt64();
            min = data.ReadInt32();
            maxValue = data.ReadInt32();
            step= data.ReadInt32();
            digits = data.ReadInt32();
            dataSplit = maxValue > maxDataSize ? maxValue / maxDataSize + 1 : 1;
            dataSize = maxValue % dataSplit == 0 ? maxValue / dataSplit : maxValue / dataSplit + 1;
            digitK = Math.Pow(10, digits);
            arr = new ulong[dataSize];
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
            data.BaseStream.Seek(Program.heard, SeekOrigin.Begin);
            ulong _count = data.ReadUInt64();
            if (++lastData < _count)
            {
                dataPass += _count - lastData;
                lastData = _count;
            }
            int value = data.ReadInt32();
            double it = (min + value * step) / digitK;
            double squareIt = it * it;
            int i = value / dataSplit;
            sum += it;
            squareSum += squareIt;
            ++Count;
            stat.Add(i);
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
                sma = sum / Count;
                standartDev = Math.Sqrt((squareSum + sma * (Count * sma - 2 * sum)) / Count);
                stat.Stat(ref mediana,ref moda);
            }
            timer = DateTime.Now.Ticks - timer;
            Console.WriteLine($"SMA={sma}");
            Console.WriteLine($"Standart deviation={standartDev}");
            Console.WriteLine($"Mediana={mediana+min/digitK}");
            foreach (var it in moda)
                Console.WriteLine($"Moda={it+min/digitK}");
            Console.WriteLine($"{dataPass} datagrams passed");
            Console.WriteLine($"Time: {timer} ticks");
            Console.WriteLine($"Time: {timer / 10000.0} ms");
        }
    };
}