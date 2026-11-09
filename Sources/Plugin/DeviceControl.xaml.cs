using SimHub;
using SimHub.Plugins.Devices;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WoteverLocalization;

namespace User.ActiveBeltTensioner
{
    public partial class DeviceControl : UserControl
    {
        private readonly DevicePlugin _plugin;
        private readonly DispatcherTimer _detectDevicesTimer;

        public Action<string> OnDeviceSelected;

        public DeviceControl(DevicePlugin plugin)
        {
            _plugin = plugin;

            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            _plugin.Settings.PropertyChanged += OnPropertyChanged;

            _detectDevicesTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _detectDevicesTimer.Tick += DetectDevices;
        }
 
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            DataContext = new DeviceViewModel(_plugin);

            _plugin.DoWithoutWaiting(
                devicePlugin =>
                {
                    devicePlugin.MotorController.LoadMotorConfigurations();
                    devicePlugin.MotorController.DetectDevices();
                    devicePlugin.MotorController.DetectMotors();
                }
            );

            _detectDevicesTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _plugin.DoWithoutWaiting(
                devicePlugin =>
                {
                    devicePlugin.MotorController.SaveMotorConfigurations();
                }
            );

            _detectDevicesTimer.Stop();
        }

        private void SetTemperaturesForPLA(object sender, RoutedEventArgs e)
        {
            _plugin.Settings.ReducedOutputTemperature = 0;
            _plugin.Settings.StoppedOutputTemperature = 100;

            _plugin.Settings.ReducedOutputTemperature = 45;
            _plugin.Settings.StoppedOutputTemperature = 55;
        }

        private void SetTemperaturesForABS(object sender, RoutedEventArgs e)
        {
            _plugin.Settings.ReducedOutputTemperature = 0;
            _plugin.Settings.StoppedOutputTemperature = 100;

            _plugin.Settings.ReducedOutputTemperature = 65;
            _plugin.Settings.StoppedOutputTemperature = 75;
        }

        private void SetTemperaturesForPETG(object sender, RoutedEventArgs e)
        {
            _plugin.Settings.ReducedOutputTemperature = 0;
            _plugin.Settings.StoppedOutputTemperature = 100;

            _plugin.Settings.ReducedOutputTemperature = 50;
            _plugin.Settings.StoppedOutputTemperature = 60;
        }

        private void SetTemperaturesForASA(object sender, RoutedEventArgs e)
        {
            _plugin.Settings.ReducedOutputTemperature = 0;
            _plugin.Settings.StoppedOutputTemperature = 100;

            _plugin.Settings.ReducedOutputTemperature = 70;
            _plugin.Settings.StoppedOutputTemperature = 80;
        }

        private void SetTemperaturesForPC(object sender, RoutedEventArgs e)
        {
            _plugin.Settings.ReducedOutputTemperature = 0;
            _plugin.Settings.StoppedOutputTemperature = 100;

            _plugin.Settings.ReducedOutputTemperature = 70;
            _plugin.Settings.StoppedOutputTemperature = 80;
        }

        private void DuplicateProfileForCurrentGame(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).Tag is GameTuningProfile profile)
            {
                string game = _plugin.CurrentGame;
                string vehicle = string.Empty;

                if (_plugin.Settings.FindProfile(game, vehicle) != null)
                {
                    ForegroundPrompt.ShowDialog(
                        SLoc.GetValue("SABT_Message_ProfileAlreadyExists"),
                        "Prompt.Car.png",
                        false
                    );

                    return;
                }

                _plugin.Settings.AddProfile(
                    _plugin.Settings.CloneProfile(
                        profile,
                        game,
                        vehicle
                    )
                );
            }
        }

        private void DuplicateProfileForCurrentGameAndVehicle(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).Tag is GameTuningProfile profile)
            {
                string game = _plugin.CurrentGame;
                string vehicle = _plugin.CurrentVehicle;

                if (_plugin.Settings.FindProfile(game, vehicle) != null)
                {
                    ForegroundPrompt.ShowDialog(
                        SLoc.GetValue("SABT_Message_ProfileAlreadyExists"),
                        "Prompt.Car.png",
                        false
                    );

                    return;
                }

                if (string.IsNullOrEmpty(vehicle))
                {
                    return;
                }

                _plugin.Settings.AddProfile(
                    _plugin.Settings.CloneProfile(
                        profile,
                        game,
                        vehicle
                    )
                );
            }
        }

        private void SelectProfile(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source && FindParent<Button>(source) != null)
            {
                return;
            }

            if (sender is FrameworkElement element && element.DataContext is GameTuningProfile profile)
            {
                if (_plugin.Settings.IsAutomaticallySwitching)
                {
                    _plugin.Settings.IsAutomaticallySwitching = false;
                }

                _plugin.Settings.LoadProfile(profile);
            }
        }

        private void DeleteProfile(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).Tag is GameTuningProfile profile)
            {
                _plugin.Settings.RemoveProfile(profile);
            }
        }

        private void ResetPlugin(object sender, RoutedEventArgs e)
        {
            _plugin.ResetSettings();
            DataContext = new DeviceViewModel(_plugin);
        }

        private void DetectDevices(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                _plugin.DoWithoutWaiting(
                    devicePlugin =>
                    {
                        devicePlugin.MotorController.DetectDevices();
                    }
                );
            }
        }

        private void DetectMotors(object sender, RoutedEventArgs e)
        {
            _plugin.DoWithoutWaiting(
                devicePlugin =>
                {
                    devicePlugin.MotorController.DetectMotors(true);
                }
            );
        }

        private void CopyLogToClipboard(object sender, RoutedEventArgs e)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "SimHub.txt");

                if (File.Exists(logPath))
                {
                    string logText;
                    using (var fileStream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        logText = streamReader.ReadToEnd();
                    }

                    Clipboard.SetText(logText ?? "");

                    Logging.Current.Info($"SABT: Copied log text to clipboard from '{logPath}'");

                    ForegroundPrompt.ShowDialog(
                        SLoc.GetValue("SABT_Message_LogCopied"),
                        "Prompt.Settings.png",
                        false
                    );

                    return;
                }
            }
            catch (Exception exception)
            {
                Logging.Current.Error($"SABT: Could not copy SimHub log text to clipboard ({exception.Message})\nStack: {exception.StackTrace}");
            }

            ForegroundPrompt.ShowDialog(
                SLoc.GetValue("SABT_Message_LogCopyFailure"),
                "Prompt.Settings.png",
                false
            );
        }

        private void ResetUpshiftingModifiers(object sender, RoutedEventArgs e)
        {
            _plugin.Settings.UpshiftingModifiers = _plugin.Settings.DefaultUpshiftingModifiers;
        }

        private void OpenHyperlink(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }

        private void ResizeTelemetryGraph(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            _plugin.Settings.TelemetryGraphHeight += e.VerticalChange;
        }

        private void ToggleCollapsibleBlock(object sender, RoutedEventArgs e)
        {
            if (sender is DependencyObject source && FindParent<Expander>(source) is Expander expander)
            {
                expander.IsExpanded = !expander.IsExpanded;
            }
        }

        private static T FindParent<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}