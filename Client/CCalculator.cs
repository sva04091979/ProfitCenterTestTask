using System;
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
        static ulong count = 0;
        const int digits = 5;
        const int step = 1;
        const int min = 100000;
        const int max = 200000;
        const int maxValue = (max - min) / step;
        const int maxDataSize = 1000;
        const int dataSplit = maxValue > maxDataSize ? maxValue / maxDataSize + 1 : 1;
        const int dataSize = maxValue % dataSplit == 0 ? maxValue / dataSplit : maxValue / dataSplit + 1;
        static double digitK = Math.Pow(10, digits);
        static ulong[] data = new ulong[dataSize];
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
                    ++count;
                    ++data[i];
                }

            });
        }
        static public void GetInfo()
        {
            var timer = DateTime.Now.Ticks;
            double sma, standartDev,mediana=0,moda=0;
            lock (Program.dataLock)
            {
                sma = sum / count;
                standartDev = Math.Sqrt((squareSum + sma * (count * sma - 2 * sum)) / count);
            }
            timer = DateTime.Now.Ticks - timer;
            Console.WriteLine($"SMA={sma}");
            Console.WriteLine($"Standart deviation={standartDev}");
            Console.WriteLine($"Time: {timer} ticks");
            Console.WriteLine($"Time: {timer / 10000.0} ms");
        }
    };
}