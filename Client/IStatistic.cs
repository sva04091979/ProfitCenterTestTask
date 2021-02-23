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
            if (CCalculator.stat.count % 2 == 0)
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
                    if (++ii == CCalculator.arr[i]) ShiftRight();
                }
            }
        }
        void ShiftLeft()
        {
            while (CCalculator.arr[--i] == 0) ;
            ii = CCalculator.arr[i] - 1;
        }
        void ShiftRight()
        {
            while (CCalculator.arr[++i] == 0) ;
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
            if (ii == CCalculator.arr[i] - 1)
            {
                int iii = i;
                while (CCalculator.arr[++iii]==0);
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
            if (CCalculator.stat.count % 2 == 0)
            {
                ret = (ret + Right()) / 2.0;
            }
            return ret / CCalculator.set.digitK;
        }
    }
    class RangeMediana : IMediana
    {
        public override double Compute()
        {
            int x0 = i * CCalculator.set.dataSplit;
            long delta = i == CCalculator.arr.Length - 1 ? CCalculator.set.maxValue - x0 : CCalculator.set.dataSplit;
            ulong prevSum = PrevSum();
            return (x0 + delta * ((CCalculator.stat.count/2.0)-prevSum) / CCalculator.arr[i])/CCalculator.set.digitK;
        }
        ulong PrevSum()
        {
            ulong ret = 0;
            for (int _i = 0; _i < i; ret += CCalculator.arr[_i++]) ;
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
            if (max < CCalculator.arr[value])
            {
                moda.Clear();
                moda.Add(value);
                max = CCalculator.arr[value];
            }
            else if (max == CCalculator.arr[value])
                moda.Add(value);
        }
    }
    class SimpleModa : IModa
    {
        override public void Compute(ref List<double> list)
        {
            foreach(var it in moda)
            {
                list.Add(it/CCalculator.set.digitK);
            }
        }
    }
    class RangeModa : IModa
    {
        override public void Compute(ref List<double> list)
        {
            foreach (var i in moda)
            {
                int x0 = i * CCalculator.set.dataSplit;
                int delta = i == CCalculator.arr.Length - 1 ? CCalculator.set.maxValue - x0 : CCalculator.set.dataSplit;
                ulong prevF = i == 0 ? 0 : CCalculator.arr[i - 1];
                ulong nextF = i == CCalculator.arr.Length - 1 ? 0 : CCalculator.arr[i + 1];
                double deltaPrev = CCalculator.arr[i] - prevF;
                double deltaNext = CCalculator.arr[i] - nextF;
                list.Add((x0+delta*(deltaPrev/(deltaPrev+deltaNext)))/CCalculator.set.digitK);
            }
        }
    }
    abstract class IStatistic
    {
        protected IMediana mediana;
        protected IModa moda;
        public void Add(int value)
        {
            ++CCalculator.arr[value];
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
