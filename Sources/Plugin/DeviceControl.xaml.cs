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

        public Action<string> OnSerialPortSelected;

        //public PlotModel TelemetryGraphModel { get; }

        public DeviceControl(DevicePlugin plugin)
        {
            Logging.Current.Info("SABT: DeviceControl()...");

            _plugin = plugin;

            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            _plugin.Settings.PropertyChanged += OnPropertyChanged;
        }
 
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
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
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Logging.Current.Info("SABT: OnUnloaded()...");
        }

        private void UpdateSerialPorts(object sender, RoutedEventArgs e)
        {
            _plugin.DoWithoutWaiting(
                devicePlugin =>
                {
                    devicePlugin.MotorController.UpdateSerialPorts();
                }
            );
        }

        private void SetLeftMotorIdentifier(object sender, RoutedEventArgs e)
        {
            _plugin.DoWithoutWaiting(
                devicePlugin => {
                    if (!devicePlugin.MotorController.IsBusy)
                    {
                        devicePlugin.MotorController.SetMotorIdentifier(devicePlugin.MotorController.GetLeftMotor());
                    }
                }
            );
        }

        private void SetRightMotorIdentifier(object sender, RoutedEventArgs e)
        {
            _plugin.DoWithoutWaiting(
                devicePlugin =>
                {
                    if (!devicePlugin.MotorController.IsBusy)
                    {
                        devicePlugin.MotorController.SetMotorIdentifier(devicePlugin.MotorController.GetRightMotor());
                    }
                }
            );
        }

        private void TestLeftMotor(object sender, RoutedEventArgs e)
        {
            _plugin.DoWithoutWaiting(
                devicePlugin =>
                {
                    if (!devicePlugin.MotorController.IsBusy)
                    {
                        devicePlugin.MotorController.TestMotor(devicePlugin.MotorController.GetLeftMotor());
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
                        devicePlugin.MotorController.TestMotor(devicePlugin.MotorController.GetRightMotor());
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