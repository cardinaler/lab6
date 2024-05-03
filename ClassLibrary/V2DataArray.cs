namespace ClassLibrary
{
    public class V2DataArray : V2Data
    {
        public double[] Net
        { get; set;}
        public double[,] Field_values
        { get; set;}

        public double[] this[int index]
        {
            get
            {
                int size = Field_values.Length / 2;
                double[] tmp = new double[size];
                for (int i = 0; i < size; ++i)
                {
                    tmp[i] = Field_values[index, i];
                }
                return tmp;
            }
        }


        public V2DataArray(string key, DateTime date) : base(key, date)
        {
            Net = new double[0];
            Field_values = new double[0, 0];
        }

        public V2DataArray(string key, DateTime date, double[] x, FValues F) : base(key, date)
        {
            Net = new double[x.Length];
            Field_values = new double[2, x.Length];
            for (int i = 0; i < x.Length; ++i)
            {
                F(x[i], ref Field_values[0, i], ref Field_values[1, i]);
                Net[i] = x[i];
            }
        }
        public V2DataArray(string key, DateTime date, int nX, double xL, double xR, FValues F) : base(key, date)
        {
            Net = new double[nX];
            Field_values = new double[2, nX];
            double step = (xR - xL) / (nX - 1); //nX != 1 
            Net[0] = xL;
            F(Net[0], ref Field_values[0, 0], ref Field_values[1, 0]);
            for (int i = 1; i < nX; ++i)
            {
                Net[i] = Net[i - 1] + step;
                F(Net[i], ref Field_values[0, i], ref Field_values[1, i]);
            }
        }

        public static explicit operator V2DataList(V2DataArray source)
        {
            V2DataList V2 = new V2DataList(source.key, source.date);
            int size = source.Field_values.Length / 2;
            for (int i = 0; i < size; ++i)
            {
                V2.L.Add(new DataItem(source.Net[i], source.Field_values[0, i], source.Field_values[1, i]));
            }
            return V2;
        }

        public override double MinField
        {
            get
            {
                double min = Math.Abs(Field_values[0, 0]);
                int size = Field_values.Length / 2;
                for (int i = 0; i < size; ++i)
                {
                    if (Math.Abs(Field_values[0, i]) < min)
                    {
                        min = Math.Abs(Field_values[0, i]);
                    }
                    if (Math.Abs(Field_values[1, i]) < min)
                    {
                        min = Math.Abs(Field_values[1, i]);
                    }
                }
                return min;
            }
        }

        public override string ToString()
        {
            string s = $"Type name: {this.GetType().ToString()}\n";
            s += $"key = {base.key}, date = {base.date}\n";
            s += $"Net have {Net.Length} elements\n";
            return s;
        }

        public override string ToLongString(string format)
        {
            string s = ToString();
            int size = Field_values.Length / 2;
            for (int i = 0; i < size; ++i)
            {
                s += $"{i}: x = {Net[i].ToString(format)}, y = ({Field_values[0, i].ToString(format)}, {Field_values[1, i].ToString(format)})\n";
            }
            s += '\n';
            return s;
        }

        public override IEnumerator<DataItem> GetEnumerator()
        {
            V2DataList V2L = (V2DataList)this;
            return V2L.GetEnumerator();
        }
        public bool Save(string filename)
        {
            try
            {
                StreamWriter writer = new StreamWriter(filename, false);
                writer.WriteLine(this.key);
                writer.Write(this.date.Year.ToString() + ' ');
                writer.Write(this.date.Month.ToString() + ' ');
                writer.Write(this.date.Day.ToString() + ' ');
                writer.Write(this.date.Hour.ToString() + ' ');
                writer.Write(this.date.Minute.ToString() + ' ');
                writer.Write(this.date.Second.ToString());
                writer.Write('\n');
                for (int i = 0; i < this.Net.Length - 1; ++i)
                {
                    writer.Write(this.Net[i].ToString() + ' ');
                }
                writer.Write(this.Net[this.Net.Length - 1].ToString());

                writer.Write('\n');
                for (int i = 0; i < this.Field_values.Length / 2 - 1; ++i)
                {
                    writer.Write(this.Field_values[0, i].ToString() + ' ');
                }
                writer.Write(this.Field_values[0, this.Field_values.Length / 2 - 1].ToString());

                writer.Write('\n');
                for (int i = 0; i < this.Field_values.Length / 2 - 1; ++i)
                {
                    writer.Write(this.Field_values[1, i].ToString() + ' ');
                }
                writer.Write(this.Field_values[1, this.Field_values.Length / 2 - 1].ToString());

                writer.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception!");
                Console.WriteLine(ex.Message);
                return false;
            }

        }
        public static bool Load(string filename, ref V2DataArray? VA)
        {
            try
            {
                StreamReader reader = new StreamReader(filename);
                string?[] s = new string[5];
                for (int i = 0; i < s.Length; ++i)
                {
                    s[i] = reader.ReadLine();
                    if (s[i] is null)
                        return false;
                }

                VA.key = s[0];
                string[] dat = s[1].Split(' ');
                VA.date = new DateTime(int.Parse(dat[0]), int.Parse(dat[1]), int.Parse(dat[2]), int.Parse(dat[3]), int.Parse(dat[4]), int.Parse(dat[5]));

                string[] Net = s[2].Split(' ');
                VA.Net = new double[Net.Length];
                for (int i = 0; i < Net.Length; ++i)
                {
                    VA.Net[i] = double.Parse(Net[i]);
                }

                string[] FV1 = s[3].Split(' ');
                string[] FV2 = s[4].Split(' ');
                VA.Field_values = new double[2, FV1.Length];
                for (int i = 0; i < FV1.Length; ++i)
                {
                    VA.Field_values[0, i] = double.Parse(FV1[i]);
                    VA.Field_values[1, i] = double.Parse(FV2[i]);
                }

                reader.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception!");
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}