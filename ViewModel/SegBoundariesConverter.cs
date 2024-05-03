using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfApp1
{
    public class SegBoundariesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double[] seg = (double[])value;
            return seg[0].ToString() + " " + seg[1].ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string str = ((string)value);
                int len = str.Length;
                double[] edges = new double[2];
                string num = "";
                int id1 = 0, id2 = 0;
                for (int i = 0; i < len; ++i)
                {
                    if (!('0' <= (str[i]) && (str[i]) <= '9') && str[i] != ' ')
                    {
                        throw new Exception("Некорректный ввод");
                    }
                }
                for (int i = 0; i < len; ++i)
                {
                    if (str[i] != ' ')
                    {
                        id1 = i; // Начала первого числа
                        break;
                    }
                }
                for (int i = id1; i < len; ++i)
                {
                    if (str[i] == ' ') // Конец первого числа
                    {
                        id2 = i;
                        break;
                    }
                }
                num = str.Substring(id1, id2 - id1 + 1); // Первое число
                edges[0] = Int32.Parse(num);
                for (int i = id2; i < len; ++i)
                {
                    if (str[i] != ' ')
                    {
                        id2 = i; // Начало второго числа
                        break;
                    }
                }
                num = str.Substring(id2, len - id2); // Второе число
                edges[1] = Int32.Parse(num);

                return edges;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                double[] zero = { 0, 0 };
                return zero;
            }
        }
    }
}
