using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    static class CCalculator
    {
        static double sum=0.0;
        static double squareSum=0.0;
        static ulong count = 0;
        static public void Add(double value)
        {
            double squareIt = value * value;
            lock (Program.dataLock)
            {
                sum += value;
                squareSum += squareIt;
                ++count;
            }
        }
        static public void GetInfo()
        {
            double _sum;
            double _squareSum;
            ulong _count;
            lock (Program.dataLock)
            {
                _sum = sum;
                _squareSum = squareSum;
                _count = count;
            }
            double sma = _sum / _count;
            double standartDev = _squareSum + sma * (2 * _sum + _count * sma);
            Console.WriteLine($"SMA={sma}");
            Console.WriteLine($"Standart deviation={standartDev}");
        }
    };
}