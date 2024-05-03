using ViewModel;
using FluentAssertions;
using Xunit;
using Moq;
namespace ViewModel.Test
{
    public class TestViewModel
    {
        public ViewData VD;

        public double[] DA_SegBoundaries = [0, 1]; // Границы отрезка с узлами сетки
        public int DA_NodesNum = 10;        // Число узлов сетки
        public bool DA_IsGridUniform = true;  // Сетка равномерна/неравномерная
        public int DA_FunctionID = 0;  // Функция для инициализации

        public int SD_NodesNum = 10;         // Число узлов сглаживающего сплайна (для построения)
        public int SD_UniformNodesNum = 100; // Число узлов равномерной сетки, на которой вычисляются значения сплайна
        public int SD_MaxItersNum = 1000; // Масимальное число итераций   
        public double SD_BreakConditionNorma = 1E-3;
        public void InitVD()
        {
            VD.DA_SegBoundaries = DA_SegBoundaries;
            VD.DA_NodesNum = DA_NodesNum;
            VD.DA_IsGridUniform = DA_IsGridUniform;
            VD.DA_FunctionID = DA_FunctionID;

            VD.SD_NodesNum = SD_NodesNum;
            VD.SD_UniformNodesNum = SD_UniformNodesNum;
            VD.SD_MaxItersNum = SD_MaxItersNum;
            VD.SD_BreakConditionNorma = SD_BreakConditionNorma;
        }
        [Fact]
        public void SegmentBoundariesErrorTest()
        {
            var Model = new Mock<IUIServices>();
            VD = new ViewData(Model.Object, false);

            DA_SegBoundaries = [1, 1];
            InitVD();
            VD.RunCommand.CanExecute(null).Should().BeFalse();

            DA_SegBoundaries = [1, 0];
            InitVD();
            VD.RunCommand.CanExecute(null).Should().BeFalse();

            DA_SegBoundaries = [0, 1];
            InitVD();
            VD.RunCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void DA_SD_NodesNumErrorTest()
        {
            var Model = new Mock<IUIServices>();
            VD = new ViewData(Model.Object, false);

            DA_NodesNum = -1;
            SD_NodesNum = -1;
            InitVD();
            VD.RunCommand.CanExecute(null).Should().BeFalse();

            DA_NodesNum = 10;
            SD_NodesNum = 11;
            InitVD();
            VD.RunCommand.CanExecute(null).Should().BeFalse();

            DA_NodesNum = 5;
            SD_NodesNum = -1;
            InitVD();
            VD.RunCommand.CanExecute(null).Should().BeFalse();

            DA_NodesNum = 5;
            SD_NodesNum = 1;
            InitVD();
            VD.RunCommand.CanExecute(null).Should().BeFalse();

            DA_NodesNum = 10;
            SD_NodesNum = 10;
            InitVD();
            VD.RunCommand.CanExecute(null).Should().BeTrue();

            DA_NodesNum = 10;
            SD_NodesNum = 10;
            SD_UniformNodesNum = -4;
            InitVD();
            VD.RunCommand.CanExecute(null).Should().BeFalse();

            DA_NodesNum = 10;
            SD_NodesNum = 10;
            SD_UniformNodesNum = 1;
            InitVD();
            VD.RunCommand.CanExecute(null).Should().BeFalse();

            DA_NodesNum = 10;
            SD_NodesNum = 10;
            SD_UniformNodesNum = 2;
            InitVD();
            VD.RunCommand.CanExecute(null).Should().BeFalse();

            DA_NodesNum = 10;
            SD_NodesNum = 10;
            SD_UniformNodesNum = 3;
            InitVD();
            VD.RunCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void SplineTest()
        {
            var Model = new Mock<IUIServices>();
            VD = new ViewData(Model.Object, false);

            InitVD();
            VD.RunCommand.Execute(null);
            var SplineApprRes = VD.SD_Link.ApproximationRes;
            var SplineOnAddonGridRes = VD.SD_Link.ResultOnAddonGrid;

            SplineApprRes
                .Zip(SplineApprRes, (first, second) => Math.Abs(first.NodeVal - second.SplineNodeVal))
                .Should()
                .OnlyContain(dy => dy < 10e-7); // Проверка погрешности аппроксимации на основной сетке
            SplineOnAddonGridRes
                .Zip(SplineOnAddonGridRes, (first, second) => Math.Abs(first.Node * first.Node * first.Node - second.SplineValue))
                .Should()
                .OnlyContain(dy => dy < 10e-3); // Проверка погрешности аппроксимации на дополнительной сетке
        }
    }
}
