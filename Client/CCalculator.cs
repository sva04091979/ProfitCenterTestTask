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
        const int step = 1;
        const int min = 100000;
        const int max = 200000;
        const int _max = min + (max - min) / step;
        const int dataSize = 1000;
        const int dataSplit = _max - min > dataSize?(_max-min)/dataSize+1:1;
        static double digitK = 100000.0;
        static ulong[] data = new ulong[(_max-min)/dataSplit];
        static async public void Add(int value)
        {
            await Task.Run(() =>
            {
                int _it = min+(value-min) / step;
                double it = value / digitK;
                double squareIt = it * it;
                int i = (_it - min) / dataSplit;
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
                Calculate(ref mediana, ref moda);
            }
            timer = DateTime.Now.Ticks - timer;
            Console.WriteLine($"SMA={sma}");
            Console.WriteLine($"Standart deviation={standartDev}");
            Console.WriteLine($"Time: {timer} ticks");
            Console.WriteLine($"Time: {timer / 10000.0} ms");
        }
        static void Calculate(ref double mediana,ref double moda)
        {
            int[] medianaShift=new int[2];
            List<int> modaShift = new List<int>();
            GetShifts(ref medianaShift, ref modaShift);
            if (dataSplit == 1)
            {
                mediana=data
            }
        }
    };
}