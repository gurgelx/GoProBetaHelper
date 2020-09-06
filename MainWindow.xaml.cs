using System;
using System.Windows;
using System.Windows.Media;

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
            this.Connector.OnDisconnect += Connector_OnDisconnect;
            this.Connector.OnData += Connector_OnData;
            this.Connector.Connect();            
        }

        private void Connector_OnConnect(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.IpLabel.Content = this.Connector.GoProIPAddress;
                this.NameLabel.Content = this.Connector.Name;
                this.OffButton.IsEnabled = true;
                this.WideButton.IsEnabled = true;
                this.WideButton.IsEnabled = true;
                this.LinearButton.IsEnabled = true;
                this.NarrowButton.IsEnabled = true;
                this.Res720Button.IsEnabled = true;
                this.Res1080Button.IsEnabled = true;
                this.StopButton.IsEnabled = true;
                this.StartButton.IsEnabled = true;
                this.GetStatus.IsEnabled = true;
            });
        }
        private void Connector_OnDisconnect(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.IpLabel.Content = this.Connector.GoProIPAddress;
                this.NameLabel.Content = this.Connector.Name;
                this.OffButton.IsEnabled = false;
                this.WideButton.IsEnabled = false;
                this.WideButton.IsEnabled = false;
                this.LinearButton.IsEnabled = false;
                this.NarrowButton.IsEnabled = false;
                this.Res720Button.IsEnabled = false;
                this.Res1080Button.IsEnabled = false;
                this.StopButton.IsEnabled = false;
                this.StartButton.IsEnabled = false;
                this.GetStatus.IsEnabled = false;
            });
        }

        private void Connector_OnData(Object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {

                if (this.Connector.Connected == false)
                {
                    this.NameLabel.Content = "Camera is: Disconnected";
                }
                else
                {

                    this.IpLabel.Content = this.Connector.GoProIPAddress;
                    this.NameLabel.Content = "Camera is: " + this.Connector.Name;
                    this.ResolutionLabel.Content = "Resolution is: " + this.Connector.Resolution;
                    this.BatteryLabel.Content = "Battery Percent is: " + this.Connector.BatteryPercent;

                    var lensIs = "Current lens is: ";
                    switch (this.Connector.Lens)
                    {
                        case 0:
                            lensIs += "Wide";
                            this.WideButton.Background = new SolidColorBrush(Color.FromRgb(121, 134, 203));
                            this.LinearButton.Background = new SolidColorBrush(Color.FromRgb(43, 43, 43));
                            this.NarrowButton.Background = new SolidColorBrush(Color.FromRgb(43, 43, 43));
                            break;
                        case 4:
                            lensIs += "Linear";
                            this.LinearButton.Background = new SolidColorBrush(Color.FromRgb(121, 134, 203));
                            this.WideButton.Background = new SolidColorBrush(Color.FromRgb(43, 43, 43));
                            this.NarrowButton.Background = new SolidColorBrush(Color.FromRgb(43, 43, 43));
                            break;
                        case 6:
                            lensIs += "Narrow";
                            this.NarrowButton.Background = new SolidColorBrush(Color.FromRgb(121, 134, 203));
                            this.WideButton.Background = new SolidColorBrush(Color.FromRgb(43, 43, 43));
                            this.LinearButton.Background = new SolidColorBrush(Color.FromRgb(43, 43, 43));
                            break;
                        default:
                            lensIs += "N/A";
                            break;
                    }

                    switch (this.Connector.Resolution)
                    {
                        case "720":
                            this.Res720Button.Background = new SolidColorBrush(Color.FromRgb(121, 134, 203));
                            this.Res1080Button.Background = new SolidColorBrush(Color.FromRgb(43, 43, 43));
                            break;
                        case "1080":
                            this.Res1080Button.Background = new SolidColorBrush(Color.FromRgb(121, 134, 203));
                            this.Res720Button.Background = new SolidColorBrush(Color.FromRgb(43, 43, 43));
                            break;
                        default:
                            this.Res720Button.Background = new SolidColorBrush(Color.FromRgb(43, 43, 43));
                            this.Res1080Button.Background = new SolidColorBrush(Color.FromRgb(43, 43, 43));
                            break;
                    }
                    if (this.Connector.Started)
                    {
                        this.StartButton.Background = new SolidColorBrush(Color.FromRgb(121, 134, 203));
                        this.StopButton.Background = new SolidColorBrush(Color.FromRgb(43, 43, 43));

                    }
                    else
                    {
                        this.StartButton.Background = new SolidColorBrush(Color.FromRgb(43, 43, 43));
                        this.StopButton.Background = new SolidColorBrush(Color.FromRgb(121, 134, 203));
                    }
                }
            });
        }
        private void UpdateStatus(object sender, RoutedEventArgs e)
        {
            this.Connector.UpdateStatus();
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
        private void Res720ButtonClick(object sender, RoutedEventArgs e)
        {
            this.Connector.SetRes720();
        }
        private void Res1080ButtonClick(object sender, RoutedEventArgs e)
        {
            this.Connector.SetRes1080();
        }
        private void TurnOffButtonClick(object sender, RoutedEventArgs e)
        {
            this.Connector.TurnOff();
        }
        private void ReconnectButtonClick(object sender, RoutedEventArgs e)
        {
            this.Connector.Connect();
        }
        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            this.Connector.Start();
        }
        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            this.Connector.Stop();
        }
    }
}


