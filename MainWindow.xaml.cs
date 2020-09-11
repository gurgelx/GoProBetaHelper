using GoPro_Webcam_Beta_helper.GoProConnector;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Media;

namespace GoPro_Webcam_Beta_helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GoProClient GoProClient { get; set; }
        public MainWindow()
        {

            InitializeComponent();
            this.DeveloperPanel.Visibility = Visibility.Collapsed;
            this.GoProClient = new GoProClient();
            this.GoProClient.OnConnect += Connector_OnConnect;
            this.GoProClient.OnDisconnect += Connector_OnDisconnect;
            this.GoProClient.OnData += Connector_OnData;
            this.GoProClient.Connect();
#if DEBUG
            this.DeveloperPanel.Visibility = Visibility.Visible;
#endif
        }

        private void Connector_OnConnect(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.IpLabel.Content = this.GoProClient.GoProIPAddress;
                this.NameLabel.Content = this.GoProClient.Name;
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
                this.SettingsList.ItemsSource = this.GoProClient.RawValues.settings;                
            });
        }
        private void Connector_OnDisconnect(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.IpLabel.Content = this.GoProClient.GoProIPAddress;
                this.NameLabel.Content = this.GoProClient.Name;
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

                if (this.GoProClient.Connected == false)
                {
                    this.NameLabel.Content = "Camera is: Disconnected";
                }
                else
                {

                    this.IpLabel.Content = this.GoProClient.GoProIPAddress;
                    this.NameLabel.Content = "Camera is: " + this.GoProClient.Name;
                    this.ResolutionLabel.Content = "Resolution is: " + this.GoProClient.Resolution;
                    this.BatteryLabel.Content = "Battery Percent is: " + this.GoProClient.BatteryPercent;

                    var lensIs = "Current lens is: ";
                    switch (this.GoProClient.Lens)
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

                    switch (this.GoProClient.Resolution)
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
                    if (this.GoProClient.Started)
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
            this.GoProClient.UpdateStatus();
        }

        private void WideButtonClick(object sender, RoutedEventArgs e)
        {
            this.GoProClient.SetWideMode();
        }

        private void NarrowButtonClick(object sender, RoutedEventArgs e)
        {
            this.GoProClient.SetNarrowMode();
        }

        private void LinearButtonClick(object sender, RoutedEventArgs e)
        {
            this.GoProClient.SetLinearMode();
        }
        private void Res720ButtonClick(object sender, RoutedEventArgs e)
        {
            this.GoProClient.SetRes720();
        }
        private void Res1080ButtonClick(object sender, RoutedEventArgs e)
        {
            this.GoProClient.SetRes1080();
        }
        private void TurnOffButtonClick(object sender, RoutedEventArgs e)
        {
            this.GoProClient.TurnOff();
        }
        private void ReconnectButtonClick(object sender, RoutedEventArgs e)
        {
            this.GoProClient.Connect();
        }
        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            this.GoProClient.Start();
        }
        private async void StopButtonClick(object sender, RoutedEventArgs e)
        {
            await this.GoProClient.Stop();
        }

        private void OnSetManualValue(object sender, RoutedEventArgs e)
        {
            var currentSetting = (KeyValuePair<string,object>)SettingsList.SelectedItem;
            var currentValue = SettingValue.Text;
            this.GoProClient.SetValue(currentSetting.Key, currentValue);
        }

        private void SettingsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(SettingsList.SelectedItem == null)
            {
                return;
            }
            var currentSetting = (KeyValuePair<string, object>)SettingsList.SelectedItem;
            this.SettingValue.Text = currentSetting.Value.ToString();
        }
    }
}


