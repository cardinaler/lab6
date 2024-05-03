using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ViewModel;

namespace WpfApp1
{
    public partial class MainWindow : Window, IUIServices
    {
        public ViewData VD;
        public MainWindow()
        {
            VD = new ViewData(this);
            InitializeComponent();
            DataContext = VD;
            BindConnections(VD); // Установка привязки с элементами управления
        }
        public void BindConnections(ViewData VD)
        {
            SegBoundariesConverter SD_Converter = new SegBoundariesConverter();

            Binding Binding_DASegBoundaries = new Binding(); // Ввод границ равномерной сетки
            Binding_DASegBoundaries.Source = VD;
            Binding_DASegBoundaries.Path = new PropertyPath("DA_SegBoundaries");
            Binding_DASegBoundaries.Converter = SD_Converter;
            Binding_DASegBoundaries.ValidatesOnDataErrors = true;
            DASegBoundariesBox.SetBinding(TextBox.TextProperty, Binding_DASegBoundaries);
        }
        public void ReportError(string message)
        {
            MessageBox.Show($"Error:\n" + message);
        }
    }
}
