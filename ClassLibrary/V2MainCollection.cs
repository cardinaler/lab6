namespace ClassLibrary
{
    public class V2MainCollection : System.Collections.ObjectModel.ObservableCollection<V2Data>
    {
        public List<V2Data> V;
        public bool Contains(string key)
        {
            for (int i = 0; i < V.Count(); ++i)
            {
                if (V[i].key == key)
                {
                    return true;
                }
            }
            return false;
        }

        public new bool Add(V2Data v2Data)
        {
            for (int i = 0; i < V.Count(); ++i)
            {
                if (V[i].key == v2Data.key)
                {
                    return false;
                }
            }
            V.Add(v2Data);
            return true;
        }

        public static void Func1_FV(double x, ref double y1, ref double y2)
        {
            y1 = 0;
            y2 = 0;
        }

        public static DataItem Func1_FDI(double x)
        {
            return new DataItem(x, x - 10, x + 10);
        }

        public V2MainCollection(int nV2DataArray, int nV2DataList)
        {
            FValues F1 = Func1_FV;
            FDI F2 = Func1_FDI;
            V = new List<V2Data>();

            const int N = 3;
            double[] d = new double[N];
            d[0] = 0.0;
            for (int i = 1; i < N; ++i)
            {
                d[i] = d[i - 1] + 0.5;
            }
            for (int i = 0; i < nV2DataList; ++i)
            {
                V.Add(new V2DataList($"VL_elem{i}", new DateTime(), d, F2));
            }
            for (int i = 0; i < nV2DataArray; ++i)
            {
                V.Add(new V2DataArray($"VA_elem{i}", new DateTime(), 5, 0.0, 1.0, F1));
            }
        }

        public string ToLongString(string format)
        {
            string s = "";
            for (int i = 0; i < V.Count(); ++i)
            {
                s = s + V[i].ToLongString(format);
            }
            return s;
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < V.Count(); ++i)
            {
                s = s + V[i].ToString();
            }
            return s;
        }


        public int get_max_num_zero_elem
        {
            get
            {
                if (V.Count == 0)
                {
                    return -1;
                }
                var ans = V.Select(p => p.Where(b => b.y[0] == 0 && b.y[1] == 0).Count());

                return ans.Max();
            }
        }
        public DataItem? get_max_DataItem
        {
            get
            {
                if (V.Count == 0)
                {
                    return null;
                }
                var first = from p in V from b in p select b;
                var ans = first.OrderBy(item => item.y[0] * item.y[0] + item.y[1] * item.y[1]).Last();
                return ans;
            }
        }

        public IEnumerable<double>? get_raw_x
        {
            get
            {
                if (V.Count == 0)
                {
                    return null;
                }
                var x_raw = from p in V from b in p select b.x;

                var ans = x_raw.GroupBy(b => b).Where(g => g.Count() == 1).Select(g => g.First());
                return ans;
            }
        }
    }

}