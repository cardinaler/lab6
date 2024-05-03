using ClassLibrary;
using Xunit;
using FluentAssertions;
namespace ClassLibrary.Tests
{
    //public delegate void FValues(double x, ref double y1, ref double y2);

    public class TestClassLibrary
    {
        public double xL = 0;
        public double xR = 1;
        public int nX = 10;
        public int SDNodesNum = 10;
        public int SDAddonGridNodesNum = 100;
        public int MaxItersNum = 1000;
        public static void FuncCube_FV(double x, ref double y1, ref double y2)
        {
            y1 = x * x * x;
            y2 = x * x * x;
        }
        public static void FuncSquare_FV(double x, ref double y1, ref double y2)
        {
            y1 = x * x;
            y2 = x * x;
        }
        public static void FuncLinear_FV(double x, ref double y1, ref double y2)
        {
            y1 = x;
            y2 = x;
        }

        public static void FuncSin_FV(double x, ref double y1, ref double y2)
        {
            y1 = x * Math.Sin(x);
            y2 = x;
        }



        public static double f_Init(double x) => x + 1;
        
        public (List<SplineDataItem>, List<DataItemS>) InitSplineAndReturnRes(
            int nX,
            double xL,
            double xR,
            FValues Func,
            int SDNodesNum,
            int SDAddonGridNodesNum,
            int MaxItersNum)
        {
            var VA = new V2DataArray("key", new DateTime(), nX, xL, xR, Func);
            var SD = new SplineData(VA, SDNodesNum, MaxItersNum, SDAddonGridNodesNum);
            SD.DebugMod = false; // Режим отладки
            SD.SplineMklCall(f_Init); // Вычисление сплайна
   
            return (SD.ApproximationRes,   // Результат аппроксимации на основной сетке
                    SD.ResultOnAddonGrid); // Результат аппроксимации на дополнительной сетке
        }
        [Fact]
        public void CubeTest()
        {
            (var SplineApprRes, var SplineOnAddonGridRes) = InitSplineAndReturnRes(nX, xL, xR, FuncCube_FV, SDNodesNum, SDAddonGridNodesNum, MaxItersNum);

            SplineApprRes
                .Zip(SplineApprRes, (first, second) => Math.Abs(first.NodeVal - second.SplineNodeVal))
                .Should()
                .OnlyContain(dy => dy < 10e-7); // Проверка погрешности аппроксимации на основной сетке
            SplineOnAddonGridRes
                .Zip(SplineOnAddonGridRes, (first, second) => Math.Abs(first.Node * first.Node * first.Node - second.SplineValue))
                .Should()
                .OnlyContain(dy => dy < 10e-3); // Проверка погрешности аппроксимации на дополнительной сетке


            SDNodesNum = 5; // Число излов сетки для сплайн аппроксимации
            SDAddonGridNodesNum = 1000;
            (SplineApprRes, SplineOnAddonGridRes) = InitSplineAndReturnRes(nX, xL, xR, FuncCube_FV, SDNodesNum, SDAddonGridNodesNum, MaxItersNum);

            SplineApprRes
                .Zip(SplineApprRes, (first, second) => Math.Abs(first.NodeVal - second.SplineNodeVal))
                .Should()
                .OnlyContain(dy => dy < 10e-3); // Проверка погрешности аппроксимации на основной сетке
            SplineOnAddonGridRes
                .Zip(SplineOnAddonGridRes, (first, second) => Math.Abs(first.Node * first.Node * first.Node - second.SplineValue))
                .Should()
                .OnlyContain(dy => dy < 10e-3); // Проверка погрешности аппроксимации на дополнительной сетке
        }

        [Fact]
        public void SquareTest()
        {
            (var SplineApprRes, var SplineOnAddonGridRes) = InitSplineAndReturnRes(nX, xL, xR, FuncSquare_FV, SDNodesNum, SDAddonGridNodesNum, MaxItersNum);

            SplineApprRes
                .Zip(SplineApprRes, (first, second) => Math.Abs(first.NodeVal - second.SplineNodeVal))
                .Should()
                .OnlyContain(dy => dy < 10e-7); // Проверка погрешности аппроксимации на основной сетке
            SplineOnAddonGridRes
                .Zip(SplineOnAddonGridRes, (first, second) => Math.Abs(first.Node * first.Node - second.SplineValue))
                .Should()
                .OnlyContain(dy => dy < 10e-3); // Проверка погрешности аппроксимации на дополнительной сетке


            SDNodesNum = 5; // Число излов сетки для сплайн аппроксимации
            SDAddonGridNodesNum = 1000;
            (SplineApprRes, SplineOnAddonGridRes) = InitSplineAndReturnRes(nX, xL, xR, FuncSquare_FV, SDNodesNum, SDAddonGridNodesNum, MaxItersNum);

            SplineApprRes
                .Zip(SplineApprRes, (first, second) => Math.Abs(first.NodeVal - second.SplineNodeVal))
                .Should()
                .OnlyContain(dy => dy < 10e-3); // Проверка погрешности аппроксимации на основной сетке
            SplineOnAddonGridRes
                .Zip(SplineOnAddonGridRes, (first, second) => Math.Abs(first.Node * first.Node - second.SplineValue))
                .Should()
                .OnlyContain(dy => dy < 10e-3); // Проверка погрешности аппроксимации на дополнительной сетке
        }

        [Fact]
        public void LinearTest()
        {
            (var SplineApprRes, var SplineOnAddonGridRes) = InitSplineAndReturnRes(nX, xL, xR, FuncLinear_FV, SDNodesNum, SDAddonGridNodesNum, MaxItersNum);

            SplineApprRes
                .Zip(SplineApprRes, (first, second) => Math.Abs(first.NodeVal - second.SplineNodeVal))
                .Should()
                .OnlyContain(dy => dy < 10e-7); // Проверка погрешности аппроксимации на основной сетке
            SplineOnAddonGridRes
                .Zip(SplineOnAddonGridRes, (first, second) => Math.Abs(first.Node - second.SplineValue))
                .Should()
                .OnlyContain(dy => dy < 10e-3); // Проверка погрешности аппроксимации на дополнительной сетке


            SDNodesNum = 5; // Число излов сетки для сплайн аппроксимации
            SDAddonGridNodesNum = 1000;
            (SplineApprRes, SplineOnAddonGridRes) = InitSplineAndReturnRes(nX, xL, xR, FuncLinear_FV, SDNodesNum, SDAddonGridNodesNum, MaxItersNum);

            SplineApprRes
                .Zip(SplineApprRes, (first, second) => Math.Abs(first.NodeVal - second.SplineNodeVal))
                .Should()
                .OnlyContain(dy => dy < 10e-3); // Проверка погрешности аппроксимации на основной сетке
            SplineOnAddonGridRes
                .Zip(SplineOnAddonGridRes, (first, second) => Math.Abs(first.Node - second.SplineValue))
                .Should()
                .OnlyContain(dy => dy < 10e-3); // Проверка погрешности аппроксимации на дополнительной сетке
        }

        [Fact]
        public void SinTest()
        {
            nX = 50;
            SDNodesNum = 50;
            (var SplineApprRes, var SplineOnAddonGridRes) = InitSplineAndReturnRes(nX, xL, xR, FuncSin_FV, SDNodesNum, SDAddonGridNodesNum, MaxItersNum);

            SplineApprRes
                .Zip(SplineApprRes, (first, second) => Math.Abs(first.NodeVal - second.SplineNodeVal))
                .Should()
                .OnlyContain(dy => dy < 10e-7); // Проверка погрешности аппроксимации на основной сетке
            SplineOnAddonGridRes
                .Zip(SplineOnAddonGridRes, (first, second) => Math.Abs(first.Node - second.SplineValue))
                .Should()
                .OnlyContain(dy => dy < 10e-1); // Проверка погрешности аппроксимации на дополнительной сетке


            SDNodesNum = 50; // Число излов сетки для сплайн аппроксимации
            SDAddonGridNodesNum = 10000;
            (SplineApprRes, SplineOnAddonGridRes) = InitSplineAndReturnRes(nX, xL, xR, FuncSin_FV, SDNodesNum, SDAddonGridNodesNum, MaxItersNum);

            SplineApprRes
                .Zip(SplineApprRes, (first, second) => Math.Abs(Math.Sin(first.NodeVal) - second.SplineNodeVal))
                .Should()
                .OnlyContain(dy => dy < 10e-2); // Проверка погрешности аппроксимации на основной сетке
            SplineOnAddonGridRes
                .Zip(SplineOnAddonGridRes, (first, second) => Math.Abs(Math.Sin(first.Node) - second.SplineValue))
                .Should()
                .OnlyContain(dy => dy < 0.3); // Проверка погрешности аппроксимации на дополнительной сетке
        }
    }
}

