using OxyPlot;
using SimHub;
using SimHub.Plugins;
using SimHub.Plugins.Devices;
using SimHub.Plugins.Styles;
using System;
using System.ComponentModel;
using System.Drawing.Text;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Xml.Linq;
using WoteverLocalization;

namespace User.ActiveBeltTensioner
{
    public partial class DeviceControl : UserControl
    {
        private readonly DevicePlugin _plugin;
        private readonly DispatcherTimer _updateSerialPortsTimer;

        public Action<string> OnSerialPortSelected;

        public DeviceControl(DevicePlugin plugin)
        {
            Logging.Current.Info("SABT: DeviceControl()...");

            _plugin = plugin;

            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            _plugin.Settings.IsEnabled = (_plugin.Settings.IsEnabled && _plugin.Settings.StartAutomatically);
            _plugin.Settings.PropertyChanged += OnPropertyChanged;

            _updateSerialPortsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _updateSerialPortsTimer.Tick += UpdateSerialPorts;
        }
 
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            /*if (e.PropertyName == nameof(_plugin.Settings.SerialPort))
            {
                _plugin.MotorController.OpenSerialPort();
            }*/
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Logging.Current.Info("SABT: OnLoaded()...");

            DataContext = new DeviceViewModel(
                _plugin.Settings,
                _plugin.MotorController,
                _plugin.TelemetryGraphModel
            );

            _plugin.DoWithoutWaiting(
                devicePlugin =>
                {
                    devicePlugin.MotorController.UpdateSerialPorts();
                }
            );

            _updateSerialPortsTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Logging.Current.Info("SABT: OnUnloaded()...");

            _updateSerialPortsTimer.Stop();
        }

        private void UpdateSerialPorts(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                _plugin.DoWithoutWaiting(
                    devicePlugin =>
                    {
                        devicePlugin.MotorController.UpdateSerialPorts();
                    }
                );
            }
        }

        private void TestLeftMotor(object sender, RoutedEventArgs e)
        {
            _plugin.DoWithoutWaiting(
                devicePlugin =>
                {
                    if (!devicePlugin.MotorController.IsBusy)
                    {
                        devicePlugin.MotorController.GetLeftMotor().Test();
                    }
                }
            );
        }

        private void TestRightMotor(object sender, RoutedEventArgs e)
        {
            _plugin.DoWithoutWaiting(
                devicePlugin =>
                {
                    if (!devicePlugin.MotorController.IsBusy)
                    {
                        devicePlugin.MotorController.GetRightMotor().Test();
                    }
                }
            );
        }

        private void SetupMotors(object sender, RoutedEventArgs e)
        {
            _plugin.DoWithoutWaiting(
                devicePlugin =>
                {
                    if (!devicePlugin.MotorController.IsBusy)
                    {
                        devicePlugin.MotorController.Setup();
                    }
                }
            );
        }

        private void OpenHyperlink(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }
    }
}