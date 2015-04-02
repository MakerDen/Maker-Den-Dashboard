using ProcessMQTT.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProcessMQTT.WPF {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            this.DataContext = new MainVM();
        }

        private void PostCommandClicked(object sender, RoutedEventArgs e)
        {
            (DataContext as MainVM).PostCommand();
        }

        private void ExitClicked(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void DefaultsClicked(object sender, RoutedEventArgs e)
        {
            (DataContext as MainVM).SetDefaults();
        }
    }
}
