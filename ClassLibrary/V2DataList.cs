using System.Collections;

namespace ClassLibrary
{
    public class V2DataList : V2Data
    {
        public List<DataItem> L { get; set; }
        public override double MinField
        {
            get
            {
                int len = L.Count();
                double min = double.MaxValue;
                for (int i = 0; i < len; ++i)
                {
                    if (Math.Abs(L[i].y[0]) < min)
                    {
                        min = Math.Abs(L[i].y[0]);
                    }
                    if (Math.Abs(L[i].y[1]) < min)
                    {
                        min = Math.Abs(L[i].y[1]);
                    }
                }
                return min;
            }
        }
        public V2DataArray VArr
        {
            get
            {
                int size = L.Count;
                V2DataArray V = new V2DataArray(this.key, this.date);
                V.Net = new double[size];
                V.Field_values = new double[2, size];
                for (int i = 0; i < size; ++i)
                {
                    V.Net[i] = L[i].x;
                    V.Field_values[0, i] = L[i].y[0];
                    V.Field_values[1, i] = L[i].y[1];
                }

                return V;
            }

        }

        public V2DataList(string key, DateTime date) : base(key, date)
        {
            L = new List<DataItem>();
        }

        public V2DataList(string key, DateTime date, double[] x, FDI F) : base(key, date)
        {
            L = new List<DataItem>();
            int size = x.Length;
            Array.Sort(x);
            L.Add(F(x[0]));
            for (int i = 1; i < size; ++i)
            {
                if (x[i] == x[i - 1])
                {
                    continue;
                }
                else
                {
                    L.Add(F(x[i]));
                }
            }
        }

        public override string ToString()
        {
            string s = $"Type name: {this.GetType().ToString()}\n";
            s += $"key = {key}, date = {date}\n";
            s += $"List<DataItem> have {L.Count()} elements\n";
            return s;
        }
        public override string ToLongString(string format)
        {
            string s = $"Type name: {this.GetType().ToString()}\n";
            s += $"key = {key}, date = {date}\n";
            s += $"List<DataItem> have {L.Count()} elements\n";
            for (int i = 0; i < L.Count(); ++i)
            {
                s += $"{i}: " + L[i].ToLongString(format);
            }
            s += '\n';
            return s;
        }

        public override IEnumerator<DataItem> GetEnumerator() => new V2DataListEnum(L);


    };

    class V2DataListEnum : IEnumerator<DataItem>
    {
        public List<DataItem> L { get; set; }
        int position = -1;

        public V2DataListEnum(List<DataItem> L)
        {
            this.L = L;
        }
        public DataItem Current
        {
            get
            {
                if (position == -1 || position >= L.Count)
                {
                    throw new ArgumentException();
                }
                else
                {
                    return L[position];
                }
            }
        }
        object IEnumerator.Current => Current;
        public bool MoveNext()
        {
            if (position < L.Count - 1)
            {
                position++;
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Reset() => position = -1;
        public void Dispose() { }
    }
}