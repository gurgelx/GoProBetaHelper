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

namespace GoPro_Webcam_Beta_helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GoProConnector Connector { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            this.Connector = new GoProConnector();
            this.Connector.OnConnect += Connector_OnConnect;
            this.Connector.Connect();            
        }

        private void Connector_OnConnect(object sender, EventArgs e)
        {
            this.IpLabel.Content = this.Connector.GoProIPAddress;
        }

        private void WideButtonClick(object sender, RoutedEventArgs e)
        {
            this.Connector.SetWideMode();
        }

        private void NarrowButtonClick(object sender, RoutedEventArgs e)
        {
            this.Connector.SetNarrowMode();
        }

        private void LinearButtonClick(object sender, RoutedEventArgs e)
        {
            this.Connector.SetLinearMode();
        }
    }
}
