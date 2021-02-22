using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    abstract class IMediana
    {
        public delegate void Checker(int value);
        protected int i;
        ulong ii = 0;
        public Checker Check;
        public IMediana()
        {
            Check = Start;
        }
        abstract public double Compute();
        void _Check(int value)
        {
            if (CCalculator.Count % 2 == 0)
            {
                if (value < i)
                {
                    if (ii == 0) ShiftLeft();
                    else --ii;
                }
            }
            else
            {
                if (value >= i)
                {
                    if (++ii == CCalculator.data[i]) ShiftRight();
                }
            }
        }
        void ShiftLeft()
        {
            while (CCalculator.data[--i] == 0) ;
            ii = CCalculator.data[i] - 1;
        }
        void ShiftRight()
        {
            while (CCalculator.data[++i] == 0) ;
            ii = 0;
        }
        void Start(int value)
        {
            i = value;
            Check -= Start;
            Check = _Check;
        }
        protected int Right()
        {
            if (ii == CCalculator.data[i] - 1)
            {
                int iii = i;
                while (CCalculator.data[++iii]==0);
                return iii;
            }
            else return i;
        }
    }
    class SimpleMediana:IMediana
    {
        public override double Compute()
        {
            double ret=i;
            if (CCalculator.Count % 2 == 0)
            {
                ret = (ret + Right()) / 2.0;
            }
            return ret / CCalculator.digitK;
        }
    }
    class RangeMediana : IMediana
    {
        public override double Compute()
        {
            int x0 = i * CCalculator.dataSplit;
            ulong delta = i == CCalculator.data.Length - 1 ? CCalculator.maxValue - CCalculator.data[i] : CCalculator.dataSplit;
            ulong prevSum = PrevSum();
            Console.WriteLine($"x0={x0}");
            Console.WriteLine($"delta={delta}");
            Console.WriteLine($"prevSum={prevSum}");
            return (x0 + delta * ((CCalculator.Count/2.0)-prevSum) / CCalculator.data[i])/CCalculator.digitK;
        }
        ulong PrevSum()
        {
            ulong ret = 0;
            for (int _i = 0; _i < i; ret += CCalculator.data[_i++]) ;
            return ret;
        }
    }
    abstract class IModa
    {
        protected List<int> moda = new List<int>();
        ulong max = 0;
        abstract public void Compute(ref List<double> moda);
        public void Check(int value)
        {
            if (max < CCalculator.data[value])
            {
                moda.Clear();
                moda.Add(value);
                max = CCalculator.data[value];
            }
            else if (max == CCalculator.data[value])
                moda.Add(value);
        }
    }
    class SimpleModa : IModa
    {
        override public void Compute(ref List<double> list)
        {
            foreach(var it in moda)
            {
                list.Add(it/CCalculator.digitK);
            }
        }
    }
    class RangeModa : IModa
    {
        override public void Compute(ref List<double> list)
        {
            foreach (var i in moda)
            {
                int x0 = i * CCalculator.dataSplit;
                int delta = i == CCalculator.data.Length - 1 ? CCalculator.maxValue - x0 : CCalculator.dataSplit;
                ulong prevF = i == 0 ? 0 : CCalculator.data[i - 1];
                ulong nextF = i == CCalculator.data.Length - 1 ? 0 : CCalculator.data[i + 1];
                double deltaPrev = CCalculator.data[i] - prevF;
                double deltaNext = CCalculator.data[i] - nextF;
                list.Add((x0+delta*(deltaPrev/(deltaPrev+deltaNext)))/CCalculator.digitK);
            }
        }
    }
    abstract class IStatistic
    {
        protected IMediana mediana;
        protected IModa moda;
        public void Add(int value)
        {
            ++CCalculator.data[value];
            mediana.Check(value);
            moda.Check(value);
        }
        public void Stat(ref double _mediana,ref List<double> _moda)
        {
            _mediana = mediana.Compute();
            moda.Compute(ref _moda);
        }
    }
    class SimpleStatistic : IStatistic
    {
        public SimpleStatistic()
        {
            mediana = new SimpleMediana();
            moda = new SimpleModa();
        }
    }
    class RangeStatistic : IStatistic
    {
        public RangeStatistic()
        {
            mediana = new RangeMediana();
            moda = new RangeModa();
        }
    }
}
