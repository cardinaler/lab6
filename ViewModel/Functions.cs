using ClassLibrary;

namespace ViewModel
{
    public class Functions
    {
        public static List<FValues> FVFunc { get; set; } = new List<FValues>() { Func1_FV, Func2_FV };

        public static void Func1_FV(double x, ref double y1, ref double y2)
        {
            y1 = x * x * x;
            y2 = x * x * x;
        }
        public static void Func2_FV(double x, ref double y1, ref double y2)
        {
            y1 = x * x;
            y2 = x * x;
        }
        public static double f_Init(double x) => x + 1; //Начальное приближение
    }
}
