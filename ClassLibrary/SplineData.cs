using System.Runtime.InteropServices;


namespace ClassLibrary
{
    public class SplineData
    {
        public int AddonUniformGridNodesNum { get; set; } // Более мелкая равномерная сетка для вычисления на ней значений сплайна
        public List<double[]> CoordAndSplineValue { get; set; } // Координата узла и значение сплайна на дополнительной сетке
        public List<DataItemS> ResultOnAddonGrid { get; set; } // Результат апроксимации на добавочной сетке

        public int m;                         //Число узлов равномерной сетки == m (для построения)
        public int MaxItersNum;               //Максимальное число итераций 
        public int StopReason;                //Причина остановки
        public int ItersNum;                  //Число итераций
        public double[]? SplineValues = null; //Значения на сетке (для вычисления)
        public double ResidualMin;

        public bool DebugMod = false;

        public V2DataArray? V2DataLink = null;//Ссылка на V2DataArray
        public List<SplineDataItem>? ApproximationRes { get; set; }  // Класс для значений на узлах сетки

        void InitAddonGrid(int AddonUniformGridNodesNum)
        {
            CoordAndSplineValue = new List<double[]>();
            CoordAndSplineValue.Add(new double[AddonUniformGridNodesNum]); // Координаты узлов (равномерная сетка)
            CoordAndSplineValue.Add(new double[AddonUniformGridNodesNum]); // Значения сплайна в узлах

            double xL = this.V2DataLink.Net[0];                            // Левая граница отрезка
            double xR = this.V2DataLink.Net[V2DataLink.Net.Length - 1];    // Правая граница отрезка
            double hS = (xR - xL) / (AddonUniformGridNodesNum - 1);        // Шаг сетки
            CoordAndSplineValue[0][0] = xL;
            for (int j = 0; j < AddonUniformGridNodesNum; ++j)
            {
                CoordAndSplineValue[0][j] = CoordAndSplineValue[0][0] + hS * j; //Определение равномерной сетки
            }
        }
        public SplineData(V2DataArray V2A, int NodesNum, int MaxItersNum, int AddonUniformGridNodesNum = 0)
        {
            this.V2DataLink = V2A;
            this.m = NodesNum;
            this.MaxItersNum = MaxItersNum;
            this.SplineValues = new double[V2A.Net.Length];
            this.ApproximationRes = new List<SplineDataItem>();
            this.AddonUniformGridNodesNum = AddonUniformGridNodesNum;
            if (AddonUniformGridNodesNum > 1)
            {
                InitAddonGrid(AddonUniformGridNodesNum);
            }
        }

        public void SplineMklCall(Func<double, double> F_Init)
        {

            // F_Init - функция, чтобы получить начальное приближение на равномерной сетке

            int nS = V2DataLink.Net.Length;                             //Число неравномерной узлов сетки
            double[] grid = this.V2DataLink.Net;                        //Неравномерная сетка

            double xL = this.V2DataLink.Net[0];
            double xR = this.V2DataLink.Net[V2DataLink.Net.Length - 1];
            double[] X = { xL, xR };                                    //Концы отрезка для равномерной сетки
            double[] Uniform_grid = new double[m];                      //Равномерная сетка
            double hS = (xR - xL) / (m - 1);                            // шаг сетки
            Uniform_grid[0] = xL;
            for (int j = 0; j < m; ++j)
            {
                Uniform_grid[j] = Uniform_grid[0] + hS * j;             //Определение равномерной сетки
            }
            double[] ApprStartVals = new double[m];
            for (int i = 0; i < m; ++i)
            {
                ApprStartVals[i] = F_Init(Uniform_grid[i]);             //Начальное приближение
            }

            int nY = 1;                                                 //Размерность векторной функции
            double[] y_true = new double[nS];
            for (int i = 0; i < nS; ++i)
            {
                y_true[i] = this.V2DataLink.Field_values[0, i]; //Истинные значения на неравномерной сетке
            }

            SplineValues = new double[nS];
            try
            {
                OptimSplineInterpolation(
                    m,
                    X,
                    nY,
                    nS,
                    grid,
                    y_true,
                    ApprStartVals,
                    SplineValues,
                    ref StopReason,
                    ref ResidualMin,
                    MaxItersNum,
                    ref ItersNum,
                    DebugMod);

                ResidualMin = 0;
                for (int i = 0; i < nS; ++i)
                {
                    ResidualMin += (y_true[i] - SplineValues[i]) * (y_true[i] - SplineValues[i]);
                    ApproximationRes.Add(new SplineDataItem(grid[i], y_true[i], SplineValues[i]));
                }
                ResidualMin = Math.Sqrt(ResidualMin);
                if (AddonUniformGridNodesNum > 1)
                {
                    ResultOnAddonGrid = new List<DataItemS>();
                    double[] F = new double[AddonUniformGridNodesNum];
                    int nS_ = AddonUniformGridNodesNum;
                    CalcSplineOnAddonGrid(
                        ref nS_,
                        ref m,
                        ApprStartVals,
                        F,
                        X,
                        nY,
                        CoordAndSplineValue[0],
                        CoordAndSplineValue[1],
                        y_true,
                        false);
                    for (int i = 0; i <= nS_; ++i)
                    {
                        ResultOnAddonGrid.Add(new DataItemS(CoordAndSplineValue[0][i], CoordAndSplineValue[1][i]));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сплайн-интерполяции\n{ex}");
            }
        }


        [DllImport("Dll_for_lab3.dll",
        CallingConvention = CallingConvention.Cdecl)]
        public static extern
        void OptimSplineInterpolation(
        int m,                   // число узлов сплайна на равномерной сетке == m (для построения)
        double[] X,              // массив узлов сплайна на равномерной сетке (для построения)
        int nY,                  // размерность векторной функции (для построения)
        int nS,                  // число узлов сетки, на которой вычисляются значения сплайна (для расчета)
        double[] grid,           // Сетка, на которой происходит вычисление значений сплайна (для расчета)
        double[] true_y,         // Истинные значения функции на сетке grid 
        double[] ApprStartVals,  // Начальное приближение
        double[] SplineValues,   // Значения сплайна на сетке grid (искомое)
        ref int StpReason,       // Причина остановки
        ref double MinResVal,    // Минимальное значение невязки
        int MaxIters,            // Максимальное число итераций
        ref int Iters,           // Сделаное число итераций
        bool DebugMod = false);

        [DllImport("Dll_for_lab3.dll",
        CallingConvention = CallingConvention.Cdecl)]
        public static extern
        void CalcSplineOnAddonGrid(
        ref Int32 nS,            // число узлов сетки, на которой вычисляются значения сплайна (для расчета) nS
        ref Int32 m,             // число узлов сплайна == m (для построения) nX 
        double[] Y,              // массив заданных значений векторной функции (для построения) (Этот вход и нужно сделать оптимальным)
        double[] F,              // Расчет невязки (стремимся к ее минимуму) F
        double[] X,              // массив узлов сплайна (для построения)
        int nY,                  // размерность векторной функции (для построения)
        double[] grid,           // сетка (для расчета)
        double[] splineValues,   // массив вычисленных значений сплайна на сетке grid
        double[] y_true,         // Истинные значения функции на сетке grid
        bool DebugMode = false);



        public string ToLongString(string format)
        {
            string ans = "";
            ans += V2DataLink.ToLongString(format);
            for (int i = 0; i < ApproximationRes.Count(); ++i)
            {
                ans += ApproximationRes[i].ToString(format);
            }
            ans += $"ResidualMin = {ResidualMin.ToString()}\n";
            ans += $"StopReason = {StopReason.ToString()}\n";
            ans += $"ItersNum = {ItersNum}\n";

            return ans;
        }

        public void Save(string filename, string format)
        {
            try
            {
                StreamWriter writer = new StreamWriter(filename, false);
                writer.Write(ToLongString(format));
                writer.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception!");
                Console.WriteLine(ex.Message);
            }
        }


    }

}