﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    static class CCalculator
    {
        static double sum = 0.0;
        static double squareSum = 0.0;
        static public ulong Count { get; set; } = 0;
        const int digits = 5;
        const int step = 1;
        const int min = 100000;
        const int max = 200000;
        public const int maxValue = (max - min) / step;
        const int maxDataSize = 1000;
        public const int dataSplit = maxValue > maxDataSize ? maxValue / maxDataSize + 1 : 1;
        const int dataSize = maxValue % dataSplit == 0 ? maxValue / dataSplit : maxValue / dataSplit + 1;
        static public double digitK = Math.Pow(10, digits);
        static public ulong[] data = new ulong[dataSize];
        static IStatistic stat;
        static public void Init()
        {
            if (dataSplit == 1)
                stat = new SimpleStatistic();
            else
                stat = new RangeStatistic();
        }
        static async public void Add(int value)
        {
            await Task.Run(() =>
            {
                double it = (min+value*step) / digitK;
                double squareIt = it * it;
                int i = value / dataSplit;
                lock(Program.dataLock)
                {
                    sum += it;
                    squareSum += squareIt;
                    ++Count;
                    stat.Add(i);
                }

            });
        }
        static public void GetInfo()
        {
            var timer = DateTime.Now.Ticks;
            double sma, standartDev;
            double mediana=0;
            List<double> moda=new List<double>();
            lock (Program.dataLock)
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
            Console.WriteLine($"Time: {timer} ticks");
            Console.WriteLine($"Time: {timer / 10000.0} ms");
        }
    };
}