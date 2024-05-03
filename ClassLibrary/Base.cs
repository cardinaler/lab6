using System.Collections;

namespace ClassLibrary
{
    public delegate void FValues(double x, ref double y1, ref double y2);

    public delegate DataItem FDI(double x);
    public struct DataItem
    {
        public double x { get; set; }
        public double[] y;
        public DataItem(double x, double y1, double y2)
        {
            this.x = x;
            y = new double[2] { y1, y2 };
        }

        public string ToLongString(string format)
        {
            return $"x = {x.ToString(format)},  y = ({y[0].ToString(format)},{y[1].ToString(format)})\n";
        }

        public override string ToString()
        {
            return $"x = {x}, y = ({y[0]}, {y[1]})\n";
        }
    }

    public struct DataItemS
    {
        public double Node { get; set; }
        public double SplineValue { get; set; }

        public DataItemS(double Node, double SplineValue)
        {
            this.Node = Node;
            this.SplineValue = SplineValue;
        }
    }
    abstract public class V2Data : IEnumerable<DataItem>
    {
        public string key { get; set; }
        public DateTime date { get; set; }
        public abstract double MinField { get; }
        public abstract string ToLongString(string format);
        public V2Data(string key, DateTime date)
        {
            this.key = key;
            this.date = date;
        }
        public override string ToString()
        {
            return "";
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;
        }

        public abstract IEnumerator<DataItem> GetEnumerator();

    }


}