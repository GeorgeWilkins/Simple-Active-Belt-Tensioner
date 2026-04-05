using SimHub;
using SimHub.Plugins;
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
using OxyPlot;

namespace User.ActiveBeltTensioner
{
    public partial class DeviceControl : UserControl
    {
        private readonly DevicePlugin _plugin;
        private readonly DispatcherTimer __serialRefreshTimer = new DispatcherTimer();

        public Action<string> OnSerialPortSelected;

        public PlotModel TelemetryGraphModel { get; }

        public DeviceControl(DevicePlugin plugin)
        {
            Logging.Current.Info("SABT: DeviceControl()...");

            _plugin = plugin;

            InitializeComponent();

            DataContext = _plugin.Settings;

            TelemetryGraph.Model = plugin.TelemetryGraphModel;

            if (TelemetryGraphModel != null)
            {
                Logging.Current.Info("GRAPH EXISTS");
            }
            else {
                Logging.Current.Warn("GRAPH IS NULL");
            }

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            _plugin.Settings.PropertyChanged += OnPropertyChanged;

            __serialRefreshTimer.Interval = TimeSpan.FromSeconds(5);
            __serialRefreshTimer.Tick += (s, e) => RefreshPorts();
        }
 
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            /*
            if (
                e.PropertyName == "SerialPort" ||
                e.PropertyName == "BaudRate"
            )
            {
                _plugin.ConfigureMotorController();
            }
            */
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Logging.Current.Info("SABT: OnLoaded()...");

            RefreshPorts();

            __serialRefreshTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Logging.Current.Info("SABT: OnUnloaded()...");

            __serialRefreshTimer.Stop();
        }

        private void RefreshPorts()
        {
            DeviceSettings settings = DataContext as DeviceSettings;

            if (settings == null || !settings.IsEnabled)
            {
                return;
            }

            Logging.Current.Info("SABT: RefreshPorts()...");

            string[] ports = DeviceEnumerator.FindMatchingDevices()
                .Select(device => device.SerialPort)
                .Where(port => !string.IsNullOrWhiteSpace(port))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(port => port, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            bool changed =
                (settings.SerialPorts == null) ||
                (settings.SerialPorts.Length != ports.Length) ||
                !settings.SerialPorts.SequenceEqual(ports, StringComparer.OrdinalIgnoreCase);

            if (!changed)
            {
                return;
            }

            settings.SerialPorts = ports;

            if (!string.IsNullOrWhiteSpace(settings.SerialPort) &&
                ports.Contains(settings.SerialPort, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            settings.SerialPort = ports.Length > 0 ? ports[0] : null;
        }

        private async void ConnectToMotorController(object sender, RoutedEventArgs e)
        {
            await _plugin.ConfigureMotorController();
        }

        private async void SetLeftMotorIdentifier(object sender, RoutedEventArgs e)
        {
            await _plugin.OnMotorController(
                controller => {
                    if (!controller.IsBusy)
                    {
                        controller.SetMotorIdentifier(controller.GetLeftMotor());
                    }
                }
            );
        }

        private async void SetRightMotorIdentifier(object sender, RoutedEventArgs e)
        {
            await _plugin.OnMotorController(
                controller =>
                {
                    if (!controller.IsBusy)
                    {
                        controller.SetMotorIdentifier(controller.GetRightMotor());
                    }
                }
            );
        }

        private async void TestLeftMotor(object sender, RoutedEventArgs e)
        {
            await _plugin.OnMotorController(
                controller =>
                {
                    if (!controller.IsBusy)
                    {
                        controller.TestMotor(controller.GetLeftMotor());
                    }
                }
            );
        }

        private async void TestRightMotor(object sender, RoutedEventArgs e)
        {
            await _plugin.OnMotorController(
                controller =>
                {
                    if (!controller.IsBusy)
                    {
                        controller.TestMotor(controller.GetRightMotor());
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