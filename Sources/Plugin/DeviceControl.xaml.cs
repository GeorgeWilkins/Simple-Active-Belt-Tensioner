using SimHub;
using System;
using System.ComponentModel;
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
        private readonly DispatcherTimer _updateSerialPortsTimer;

        public Action<string> OnSerialPortSelected;

        public DeviceControl(DevicePlugin plugin)
        {
            _plugin = plugin;

            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            _plugin.Settings.PropertyChanged += OnPropertyChanged;

            _updateSerialPortsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _updateSerialPortsTimer.Tick += UpdateSerialPorts;
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
                    devicePlugin.MotorController.UpdateSerialPorts();
                }
            );

            _updateSerialPortsTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
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

        private void DuplicateProfileForCurrentGame(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).Tag is GameTuningProfile profile)
            {
                string game = _plugin.CurrentGame;
                string vehicle = string.Empty;

                if (_plugin.Settings.FindProfile(game, vehicle) != null)
                {
                    MessageBox.Show(
                        SLoc.GetValue("SABT_Message_Profiles_AlreadyExists"),
                        SLoc.GetValue("SABT_Plugin"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation
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
                    MessageBox.Show(
                        SLoc.GetValue("SABT_Message_Profiles_AlreadyExists"),
                        SLoc.GetValue("SABT_Plugin"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation
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

        private void ResetDiagnostics(object sender, RoutedEventArgs e)
        {
            _plugin.DoWithoutWaiting(
                devicePlugin =>
                {
                    devicePlugin.MotorController.ResetDiagnostics();
                }
            );
        }

        private void OpenHyperlink(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }

        private void ResizeTelemetryGraph(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            _plugin.Settings.TelemetryGraphHeight += e.VerticalChange;
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