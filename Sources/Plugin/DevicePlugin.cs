using GameReaderCommon;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using SimHub;
using SimHub.Plugins;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using WoteverLocalization;


namespace User.ActiveBeltTensioner
{
    [PluginDescription("A control panel for the 'Simple Active Belt Tensioner'")]
    [PluginAuthor("George Wilkins")]
    [PluginName("Simple Active Belt Tensioner")]
    public class DevicePlugin : IPlugin, IDataPlugin, IWPFSettingsV2, IReusable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DeviceSettings Settings;

        public PluginManager PluginManager { get; set; }

        public ImageSource PictureIcon => this.ToIcon(Properties.Resources.MenuIcon);

        public string LeftMenuTitle => SLoc.GetValue("SABT_Plugin");


        public MotorController MotorController;

        public int SelectedTabIndex { get; set; } = 0;

        private static string _settingsName = "SimpleActiveBeltTensioner";

        private readonly object _motorControllerLock = new object();
        private readonly object _telemetryLock = new object();
        private TelemetrySnapshot _latestTelemetry;

        private readonly AutoResetEvent _hasTelemetryArrived = new AutoResetEvent(false);
        private Thread _controlThread;
        private volatile bool _runControlLoop = false;
        private volatile bool _hasBeenInactive = true;
        private volatile bool _hasBypassedActivationWarning = false;

        public struct TelemetrySnapshot
        {
            public double? Surge;
            public double? Sway;
            public double? Heave;
            public double? Speed;
            public bool DidUpshift;
            public bool IsActive;
        }

        private volatile bool _isEnabled = false;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));

                    if (_isEnabled)
                    {
                        DoWithoutWaiting(devicePlugin =>
                        {
                            devicePlugin.MotorController?.Connect();
                        });
                    }
                    else
                    {
                        _hasBeenInactive = true;

                        DoWithoutWaiting(devicePlugin =>
                        {
                            devicePlugin.MotorController?.Disconnect();
                        });
                    }
                }
            }
        }

        private string _currentGame = string.Empty;
        public string CurrentGame
        {
            get { return _currentGame; }
            set
            {
                string newValue = value ?? string.Empty;
                if (_currentGame != newValue)
                {
                    _currentGame = newValue;
                    OnPropertyChanged(nameof(CurrentGame));

                    if (Settings != null && Settings.IsAutomaticallySwitching)
                    {
                        Settings.ChangeActiveProfile();
                    }
                }
            }
        }

        private string _currentVehicle = string.Empty;
        public string CurrentVehicle
        {
            get { return _currentVehicle; }
            set
            {
                string newValue = value ?? string.Empty;
                if (_currentVehicle != newValue)
                {
                    _currentVehicle = newValue;
                    OnPropertyChanged(nameof(CurrentVehicle));
                    OnPropertyChanged(nameof(HasCurrentVehicle));

                    if (Settings != null && Settings.IsAutomaticallySwitching)
                    {
                        Settings.ChangeActiveProfile();
                    }
                }
            }
        }

        public bool HasCurrentVehicle
        {
            get { return _currentVehicle != string.Empty; }
        }

        private bool _isAutomaticallyTuning = false;
        public bool IsAutomaticallyTuning
        {
            get { return _isAutomaticallyTuning; }
            set
            {
                if (_isAutomaticallyTuning != value)
                {
                    _isAutomaticallyTuning = value;
                    OnPropertyChanged(nameof(IsAutomaticallyTuning));

                    if (_isAutomaticallyTuning)
                    {
                        Settings.IsAutomaticallySwitching = false;

                        if (!_latestTelemetry.IsActive)
                        {
                            MessageBox.Show(
                                SLoc.GetValue("SABT_Message_AutomaticTuningRequiresTelemetry"),
                                SLoc.GetValue("SABT_Plugin"),
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning
                            );

                            IsAutomaticallyTuning = false;

                            return;
                        }

                        MessageBoxResult result = MessageBoxResult.No;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            result = MessageBox.Show(
                                SLoc.GetValue("SABT_Message_AutomaticTuningReset"),
                                SLoc.GetValue("SABT_Plugin"),
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning
                            );
                        });

                        if (result == MessageBoxResult.Yes)
                        {
                            Settings.MinimumSurge = -1;
                            Settings.MaximumSurge = 1;
                            Settings.MinimumSway = -1;
                            Settings.MaximumSway = 1;
                            Settings.MinimumHeave = -20;
                            Settings.MaximumHeave = 40;

                            IsEnabled = false;
                        }
                        else
                        {
                            IsAutomaticallyTuning = false;
                        }
                    }
                }
            }
        }
        public bool IsNotAutomaticallyTuning
        {
            get { return !_isAutomaticallyTuning; }
        }



        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            return new DeviceControl(this);
        }

        /// <summary>Called by SimHub to initialise the plugin</summary>
        public void Init(PluginManager pluginManager)
        {
            Logging.Current.Info("SABT: Initialising...");

            // Obtain Game & Vehicle
            CurrentGame = (
                pluginManager.GetPropertyValue("DataCorePlugin.CurrentGame")?.ToString() ??
                string.Empty
            );
            CurrentVehicle = (
                pluginManager.GetPropertyValue("DataCorePlugin.NewData.CarClass")?.ToString() ??
                pluginManager.GetPropertyValue("DataCorePlugin.NewData.CarModel")?.ToString() ??
                string.Empty
            );

            // Load Serialised Settings
            Settings = this.ReadCommonSettings<DeviceSettings>(_settingsName, () => new DeviceSettings());
            Settings.Persist = () => this.SaveCommonSettings(_settingsName, Settings);
            Settings.PropertyChanged += OnSettingsChanged;
            Settings.Initialise(this);

            IsEnabled = IsEnabled || Settings.StartAutomatically;

            // Register Actions (For External Control)
            pluginManager.AddAction(
                actionName: "SABT.ToggleMotors",
                actionStart: (PluginManager manager, string input) => {
                    DoOnMainThread(devicePlugin =>
                    {
                        Logging.Current.Info("SABT: Toggling motors from external input");

                        devicePlugin._hasBypassedActivationWarning = false;
                        devicePlugin.IsEnabled = !devicePlugin.IsEnabled;
                    });
                }
            );
            pluginManager.AddAction(
                actionName: "SABT.ToggleMotorsWithoutWarning",
                actionStart: (PluginManager manager, string input) => {
                    DoOnMainThread(devicePlugin =>
                    {
                        Logging.Current.Info("SABT: Toggling motors from external input (without warning)");

                        devicePlugin._hasBypassedActivationWarning = devicePlugin.IsEnabled ? false : true;
                        devicePlugin.IsEnabled = !devicePlugin.IsEnabled;
                    });
                }
            );

            pluginManager.AddAction(
                actionName: "SABT.IncreaseIdleTension",
                actionStart: (PluginManager manager, string input) => {
                    DoOnMainThread(devicePlugin =>
                    {
                        Logging.Current.Info("SABT: Increasing idle tension from external input");

                        devicePlugin.Settings.IdleTension += devicePlugin.Settings.TensionStep;
                    });
                }
            );
            pluginManager.AddAction(
                actionName: "SABT.DecreaseIdleTension",
                actionStart: (PluginManager manager, string input) => {
                    DoOnMainThread(devicePlugin =>
                    {
                        Logging.Current.Info("SABT: Decreasing idle tension from external input");

                        devicePlugin.Settings.IdleTension -= devicePlugin.Settings.TensionStep;
                    });
                }
            );

            pluginManager.AddAction(
                actionName: "SABT.IncreaseMinimumTension",
                actionStart: (PluginManager manager, string input) => {
                    DoOnMainThread(devicePlugin =>
                    {
                        Logging.Current.Info("SABT: Increasing minimum tension from external input");

                        devicePlugin.Settings.MinimumTension += devicePlugin.Settings.TensionStep;
                    });
                }
            );
            pluginManager.AddAction(
                actionName: "SABT.DecreaseMinimumTension",
                actionStart: (PluginManager manager, string input) => {
                    DoOnMainThread(devicePlugin =>
                    {
                        Logging.Current.Info("SABT: Decreasing minimum tension from external input");

                        devicePlugin.Settings.MinimumTension -= devicePlugin.Settings.TensionStep;
                    });
                }
            );

            pluginManager.AddAction(
                actionName: "SABT.IncreaseMaximumTension",
                actionStart: (PluginManager manager, string input) => {
                    DoOnMainThread(devicePlugin =>
                    {
                        Logging.Current.Info("SABT: Increasing maximum tension from external input");
                        devicePlugin.Settings.MaximumTension += devicePlugin.Settings.TensionStep;
                    });
                }
            );
            pluginManager.AddAction(
                actionName: "SABT.DecreaseMaximumTension",
                actionStart: (PluginManager manager, string input) => {
                    DoOnMainThread(devicePlugin =>
                    {
                        Logging.Current.Info("SABT: Decreasing maximum tension from external input");

                        devicePlugin.Settings.MaximumTension -= devicePlugin.Settings.TensionStep;
                    });
                }
            );

            // Expose Properties
            pluginManager.AttachDelegate("SABT.IsEnabled", typeof(DevicePlugin), () => IsEnabled);
            pluginManager.AttachDelegate("SABT.IdleTension", typeof(DevicePlugin), () => Settings.IdleTension / 10.0);
            pluginManager.AttachDelegate("SABT.MinimumTension", typeof(DevicePlugin), () => Settings.MinimumTension / 10.0);
            pluginManager.AttachDelegate("SABT.MaximumTension", typeof(DevicePlugin), () => Settings.MaximumTension / 10.0);

            // Initialise Motor Controller
            MotorController = new MotorController(this);
            if (IsEnabled && Settings.IsSerialPortValid)
            {
                DoWithoutWaiting(devicePlugin =>
                {
                    devicePlugin.MotorController.Connect();
                });
            }

            // Initialise Telemetry Graph
            InitialiseTelemetryGraph();
            UpdateTelemetryGraphThresholds(Settings);
            UpdateTelemetryGraph(0, 0, 0, 0, 0);

            // Start Control Loop
            _runControlLoop = true;
            _controlThread = new Thread(ControlLoop)
            {
                IsBackground = true,
                Name = "SABT.ControlLoop"
            };
            _controlThread.Start();
        }

        /// <summary>Selectively initiates side effects for settings property changes</summary>
        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (
                e.PropertyName == nameof(Settings.SerialPort)
            )
            {
                IsEnabled = false;

                return;
            }

            if (
                e.PropertyName == nameof(Settings.MinimumSurge) ||
                e.PropertyName == nameof(Settings.MaximumSurge) ||
                e.PropertyName == nameof(Settings.MinimumSway) ||
                e.PropertyName == nameof(Settings.MaximumSway) ||
                e.PropertyName == nameof(Settings.MinimumHeave) ||
                e.PropertyName == nameof(Settings.MaximumHeave)
            )
            {
                UpdateTelemetryGraphThresholds(Settings);

                return;
            }

            if (
                e.PropertyName == nameof(Settings.ShowSurgePlot) ||
                e.PropertyName == nameof(Settings.ShowSwayPlot) ||
                e.PropertyName == nameof(Settings.ShowHeavePlot) ||
                e.PropertyName == nameof(Settings.ShowTorquePlot)
            )
            {
                UpdateTelemetryGraphFilters();

                return;
            }

            if (
                e.PropertyName == nameof(Settings.ActiveProfileKey)
            )
            {
                IsAutomaticallyTuning = false;
            }
        }

        /// <summary>Called by SimHub when new telemetry data is available</summary>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            short oldGear = 0;
            short newGear = 0;
            bool inGear = Int16.TryParse(data.OldData?.Gear, out oldGear) && Int16.TryParse(data.NewData?.Gear, out newGear);

            TelemetrySnapshot telemetrySnapshot = new TelemetrySnapshot
            {
                Surge = data.NewData?.AccelerationSurge,
                Sway = data.NewData?.AccelerationSway,
                Heave = data.NewData?.AccelerationHeave,
                Speed = data.NewData?.SpeedKmh,
                DidUpshift = inGear && (oldGear < newGear),
                IsActive = (data.GameRunning && !data.GameInMenu) || data.GameReplay
            };

            lock (_telemetryLock)
            {
                _latestTelemetry = telemetrySnapshot;
            }

            CurrentGame = data.GameName ?? String.Empty;
            CurrentVehicle = data.NewData?.CarClass ?? data.NewData?.CarModel ?? String.Empty;

            _hasTelemetryArrived.Set();
        }

        /// <summary>Called by SimHub when the plugin is unloaded, allowing the graceful release of connections and resources</summary>
        public void End(PluginManager pluginManager)
        {
            this.SaveCommonSettings(_settingsName, Settings);

            _runControlLoop = false;
            _hasTelemetryArrived.Set();

            if (_controlThread != null)
            {
                _controlThread.Join(500);
                _controlThread = null;
            }

            MotorController.Disconnect();
        }

        public void FinalizePlugin()
        {
            Logging.Current.Info("SABT: Finalizing plugin");
        }

        /// <summary>Evaluates the <see cref="TelemetrySnapshot"/> properties, then performs auto-tuning and calculates the appropriate effects to apply (sending commands to the motors if enabled)</summary>
        /// <remarks>Runs as a separate thread to keep effects processing and motor commands out of the <see cref="DataUpdate"/> calls</remarks>
        private void ControlLoop()
        {
            // Initialise Telemetry Buffers (For Auto-Tuning)
            const int telemetryBufferSize = 20;
            double[] telemetrySurgeBuffer = new double[telemetryBufferSize];
            double[] telemetrySwayBuffer = new double[telemetryBufferSize];
            double[] telemetryHeaveBuffer = new double[telemetryBufferSize];
            int telemetryBufferIndex = 0;

            while (_runControlLoop)
            {
                if (!_runControlLoop)
                {
                    break;
                }

                _hasTelemetryArrived.WaitOne();

                TelemetrySnapshot telemetrySnapshot;
                lock (_telemetryLock)
                {
                    telemetrySnapshot = _latestTelemetry;
                }

                try
                {
                    // Parse Preferences
                    double idleTension = ConvertToFraction(Settings.IdleTension);
                    double minimumTension = ConvertToFraction(Settings.MinimumTension);
                    double maximumTension = ConvertToFraction(Settings.MaximumTension);
                    double sideBias = ConvertToFraction(Settings.SideBias);
                    double smoothingFactor = ConvertToFraction(Settings.SmoothingFactor);
                    double corneringStrength = ConvertToFraction(Settings.CorneringStrength);
                    double accelerationStrength = ConvertToFraction(Settings.AccelerationStrength);
                    double brakingStrength = ConvertToFraction(Settings.BrakingStrength);
                    double jumpingStrength = ConvertToFraction(Settings.JumpingStrength);
                    double landingStrength = ConvertToFraction(Settings.LandingStrength);
                    double shiftingStrength = ConvertToFraction(Settings.ShiftingStrength);

                    // Handle Tuning & Telemetry
                    int minimumSurge = Settings.MinimumSurge;
                    int maximumSurge = Settings.MaximumSurge;
                    int minimumSway = Settings.MinimumSway;
                    int maximumSway = Settings.MaximumSway;
                    int minimumHeave = Settings.MinimumHeave;
                    int maximumHeave = Settings.MaximumHeave;

                    bool isMoving = telemetrySnapshot.Speed > 0.2;
                    bool didUpshift = telemetrySnapshot.DidUpshift;
                    double surge = telemetrySnapshot.Surge ?? 0.0;
                    double sway = (ConvertToFractionOfRange(telemetrySnapshot.Sway ?? 0.0, minimumSway, maximumSway) * 2.0) - 1.0;
                    double heave = telemetrySnapshot.Heave ?? 0.0;
                    double speed = telemetrySnapshot.Speed ?? 0.0;

                    // Calculate Effects
                    double braking = ConvertToFractionOfRange(surge, 0, maximumSurge);
                    double acceleration = 1.0 - ConvertToFractionOfRange(surge, minimumSurge, 0);
                    double landing = ConvertToFractionOfRange(heave, 0, maximumHeave);
                    double jumping = 1.0 - ConvertToFractionOfRange(heave, minimumHeave, 0);

                    double increasingModifierLeft = 0.0;
                    double increasingModifierRight = 0.0;
                    double decreasingModifierLeft = 0.0;
                    double decreasingModifierRight = 0.0;

                    double leftTarget = 0.0;
                    double rightTarget = 0.0;

                    increasingModifierLeft = Math.Max(increasingModifierLeft, (braking * brakingStrength));
                    increasingModifierRight = Math.Max(increasingModifierRight, (braking * brakingStrength));
                    decreasingModifierLeft = Math.Max(decreasingModifierLeft, (acceleration * accelerationStrength));
                    decreasingModifierRight = Math.Max(decreasingModifierRight, (acceleration * accelerationStrength));
                    decreasingModifierLeft = Math.Max(decreasingModifierLeft, (jumping * jumpingStrength));
                    decreasingModifierRight = Math.Max(decreasingModifierRight, (jumping * jumpingStrength));
                    increasingModifierLeft = Math.Max(increasingModifierLeft, (landing * landingStrength));
                    increasingModifierRight = Math.Max(increasingModifierRight, (landing * landingStrength));
                    increasingModifierLeft = Math.Max(increasingModifierLeft, (sway <= 0.0) ? (Math.Abs(sway * corneringStrength)) : 0.0);
                    increasingModifierRight = Math.Max(increasingModifierRight, (sway > 0.0) ? (Math.Abs(sway * corneringStrength)) : 0.0);

                    // Combinator
                    double totalModifierLeft = increasingModifierLeft - decreasingModifierLeft;
                    double totalModifierRight = increasingModifierRight - decreasingModifierRight;

                    if (totalModifierLeft < 0.0)
                    {
                        leftTarget = minimumTension + (totalModifierLeft * minimumTension);
                    }
                    else {
                        leftTarget = minimumTension + (totalModifierLeft * (maximumTension - minimumTension));
                    }

                    if (totalModifierRight < 0.0)
                    {
                        rightTarget = minimumTension + (totalModifierRight * minimumTension);
                    }
                    else
                    {
                        rightTarget = minimumTension + (totalModifierRight * (maximumTension - minimumTension));
                    }

                    // Map To Range (Minimum ~ Maximum Tension)
                    leftTarget = ClampTo(leftTarget, 0.0, maximumTension);
                    rightTarget = ClampTo(rightTarget, 0.0, maximumTension);

                    // Idle Tension
                    if (!isMoving)
                    {
                        leftTarget = idleTension;
                        rightTarget = idleTension;
                    }

                    // Side Bias
                    if (sideBias < 0.0)
                    {
                        rightTarget *= (1.0 - Math.Abs(sideBias));
                    }
                    else if (sideBias > 0.0)
                    {
                        leftTarget *= (1.0 - sideBias);
                    }

                    // Update Telemetry Graph
                    if (telemetrySnapshot.IsActive && SelectedTabIndex == 3)
                    {
                        UpdateTelemetryGraph(
                            telemetrySnapshot.Surge ?? 0,
                            telemetrySnapshot.Sway ?? 0,
                            telemetrySnapshot.Heave ?? 0,
                            leftTarget,
                            rightTarget
                        );
                    }

                    // Apply Automatic Tuning
                    if (IsAutomaticallyTuning)
                    {
                        int averagedSurge = GetAveragedTelemetryValue(telemetrySurgeBuffer, telemetrySnapshot.Surge ?? 0, telemetryBufferIndex);
                        int averagedSway = GetAveragedTelemetryValue(telemetrySwayBuffer, Math.Abs(telemetrySnapshot.Sway ?? 0), telemetryBufferIndex);
                        int averagedHeave = GetAveragedTelemetryValue(telemetryHeaveBuffer, telemetrySnapshot.Heave ?? 0, telemetryBufferIndex);

                        Settings.MaximumSurge = Math.Max(Settings.MaximumSurge, averagedSurge);
                        Settings.MinimumSurge = Math.Min(Settings.MinimumSurge, averagedSurge);
                        Settings.MaximumSway = Math.Max(Settings.MaximumSway, averagedSway);
                        Settings.MinimumSway = Settings.MaximumSway * -1;
                        Settings.MaximumHeave = Math.Max(Settings.MaximumHeave, averagedHeave);
                        Settings.MinimumHeave = Math.Min(Settings.MinimumHeave, averagedHeave);

                        telemetryBufferIndex = (telemetryBufferIndex + 1) % telemetryBufferSize;

                        continue;
                    }

                    // Check State
                    if (!IsEnabled)
                    {
                        _hasBeenInactive = true;

                        continue;
                    }

                    MotorController motorController;
                    lock (_motorControllerLock)
                    {
                        motorController = MotorController;
                    }

                    if (_hasBeenInactive)
                    {
                        if (!_hasBypassedActivationWarning)
                        {
                            MessageBoxResult result = MessageBoxResult.No;

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                result = MessageBox.Show(
                                    SLoc.GetValue("SABT_Message_ActivationWarning"),
                                    SLoc.GetValue("SABT_Plugin"),
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning
                                );
                            });

                            if (result != MessageBoxResult.Yes)
                            {
                                IsEnabled = false;

                                continue;
                            }

                            motorController.Connect();
                        }

                        _hasBeenInactive = false;
                    }

                    _hasBypassedActivationWarning = false;

                    // Send To Motors
                    if (!motorController.IsBusy && motorController.HasSerial)
                    {
                        if (!motorController.SetTorques(leftTarget, rightTarget, smoothingFactor))
                        {
                            Logging.Current.Warn("SABT: Exceeded motor communication failure limit (disabling plugin)");

                            MessageBox.Show(
                                SLoc.GetValue("SABT_Message_DeviceFailure"),
                                SLoc.GetValue("SABT_Plugin"),
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning
                            );

                            IsEnabled = false;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logging.Current.Error("SABT: " + exception.Message);
                }
            }
        }

        /// <summary>An action wrapper for executing logic asynchronously</summary>
        public async Task DoWithoutWaiting(Action<DevicePlugin> actionToPerform)
        {
            await Task.Run(() => actionToPerform(this));
        }

        /// <summary>An action wrapper for executing logic on the main thread (where triggering logic may not be)</summary>
        private void DoOnMainThread(Action<DevicePlugin> actionToPerform)
        {
            Dispatcher dispatcher = Application.Current?.Dispatcher;

            if (dispatcher == null || dispatcher.CheckAccess())
            {
                actionToPerform(this);

                return;
            }

            dispatcher.BeginInvoke(
                new Action(() => actionToPerform(this)),
                DispatcherPriority.Send
            );
        }

        /// <summary>A utility method for converting the 10x/100x/1000x integers used in the settings sliders with decimal values</summary>
        private static double ConvertToFraction(double value, uint resolution = 1000)
        {
            value /= resolution;
            if (value < -1.0) { return -1.0; }
            if (value > 1.0) { return 1.0; }
            return value;
        }

        /// <summary>A utility method for converting the integers used in the settings sliders with decimal values (relative to the given range)</summary>
        private static double ConvertToFractionOfRange(double value, double min, double max)
        {
            value = ClampTo(value, min, max);

            return (value - min) / (max - min);
        }

        /// <summary>A utility method for clamping the given value to a given range</summary>
        private static double ClampTo(double value, double min, double max)
        {
            if (value < min) { return min; }
            if (value > max) { return max; }
            return value;
        }

        /// <summary>Updates the telemetry value buffer and returns the averaged value</summary>
        private int GetAveragedTelemetryValue(double[] buffer, double newValue, int bufferIndex)
        {
            buffer[bufferIndex] = newValue;
            
            double sum = 0.0;

            foreach (double value in buffer)
            {
                sum += value;
            }
            
            return (int) (sum / buffer.Length);
        }




        public PlotModel TelemetryGraphModel { get; private set; }

        private LineSeries _surgeSeries;
        private LineSeries _swaySeries;
        private LineSeries _heaveSeries;
        private LineSeries _leftTorqueSeries;
        private LineSeries _rightTorqueSeries;

        private LineAnnotation _surgeMinimumAnnotation;
        private LineAnnotation _surgeMaximumAnnotation;
        private LineAnnotation _swayMinimumAnnotation;
        private LineAnnotation _swayMaximumAnnotation;
        private LineAnnotation _heaveMinimumAnnotation;
        private LineAnnotation _heaveMaximumAnnotation;

        private LineAnnotation _targetDividerAnnotation;
        private RectangleAnnotation _leftTargetBarAnnotation;
        private RectangleAnnotation _rightTargetBarAnnotation;

        private LinearAxis _accelerationAxis;
        private LinearAxis _timeAxis;
        private LinearAxis _torqueAxis;
        private LinearAxis _targetAxis;

        private int _plotPointIndex = 0;
        private const int _maximumPlotPoints = 150;
        private const string _accelerationAxisKey = "accelerationAxis";
        private const string _torqueAxisKey = "torqueAxis";
        private const string _targetAxisKey = "targetAxis";

        private DateTime _lastPlotRefresh = DateTime.MinValue;
        private static readonly TimeSpan PlotRefreshInterval = TimeSpan.FromMilliseconds(33);

        /// <summary>Initialises the telemetry graph instance and configures its styling and legends</summary>
        private void InitialiseTelemetryGraph()
        {
            OxyColor lighterBlue = OxyColor.Parse("#119eda");
            OxyColor darkerBlue = OxyColor.Parse("#0b668d");
            OxyColor grey = OxyColor.Parse("#454545");
            OxyColor red = OxyColor.Parse("#f44336");
            OxyColor green = OxyColor.Parse("#357c38");
            OxyColor yellow = OxyColor.Parse("#ffd03a");

            TelemetryGraphModel = new PlotModel {
                Title = " ",
                TextColor = OxyColors.White,
                LegendTextColor = OxyColors.White,
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopLeft,
                PlotAreaBorderColor = OxyColors.Transparent,
                PlotType = PlotType.XY
            };

            _accelerationAxis = new LinearAxis {
                Key = _accelerationAxisKey,
                Title = "m/s²",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = grey,
                MinorGridlineStyle = LineStyle.Dot,
                MinorGridlineColor = grey,
                TicklineColor = OxyColors.Transparent,
                Minimum = -50,
                Maximum = 70,
                IsPanEnabled = false,
                IsZoomEnabled = false
            };

            _torqueAxis = new LinearAxis
            {
                Key = _torqueAxisKey,
                Title = "%",
                Position = AxisPosition.Right,
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                TicklineColor = OxyColors.Transparent,
                Minimum = 0,
                Maximum = 100,
                IsPanEnabled = false,
                IsZoomEnabled = false
            };

            _targetAxis = new LinearAxis
            {
                Key = _targetAxisKey,
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 1,
                IsAxisVisible = false,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                StartPosition = 0.90,
                EndPosition = 1.0
            };

            _timeAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                IsAxisVisible = false,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                StartPosition = 0.0,
                EndPosition = 0.9
            };

            TelemetryGraphModel.Axes.Add(_accelerationAxis);
            TelemetryGraphModel.Axes.Add(_torqueAxis);
            TelemetryGraphModel.Axes.Add(_timeAxis);
            TelemetryGraphModel.Axes.Add(_targetAxis);

            _surgeSeries = AddTelemetryLine(_accelerationAxisKey, SLoc.GetValue("SABT_Legend_Surge"), red);
            _surgeMinimumAnnotation = AddThresholdLine(red);
            _surgeMaximumAnnotation = AddThresholdLine(red);

            _swaySeries = AddTelemetryLine(_accelerationAxisKey, SLoc.GetValue("SABT_Legend_Sway"), green);
            _swayMinimumAnnotation = AddThresholdLine(green);
            _swayMaximumAnnotation = AddThresholdLine(green);

            _heaveSeries = AddTelemetryLine(_accelerationAxisKey, SLoc.GetValue("SABT_Legend_Heave"), yellow);
            _heaveMinimumAnnotation = AddThresholdLine(yellow);
            _heaveMaximumAnnotation = AddThresholdLine(yellow);

            _leftTorqueSeries = AddTelemetryLine(_torqueAxisKey, SLoc.GetValue("SABT_Legend_Torque") + " (L)", lighterBlue);
            _rightTorqueSeries = AddTelemetryLine(_torqueAxisKey, SLoc.GetValue("SABT_Legend_Torque") + " (R)", darkerBlue);

            _targetDividerAnnotation = new LineAnnotation
            {
                XAxisKey = _targetAxisKey,
                Type = LineAnnotationType.Vertical,
                Color = grey,
                StrokeThickness = 2,
                LineStyle = LineStyle.Dot,
                X = 0
            };

            _leftTargetBarAnnotation = new RectangleAnnotation
            {
                XAxisKey = _targetAxisKey,
                Fill = lighterBlue,
                Layer = AnnotationLayer.AboveSeries,
                MinimumX = 0.1,
                MaximumX = 0.45,
                MinimumY = _accelerationAxis.Minimum,
                MaximumY = _accelerationAxis.Minimum
            };

            _rightTargetBarAnnotation = new RectangleAnnotation
            {
                XAxisKey = _targetAxisKey,
                Fill = darkerBlue,
                Layer = AnnotationLayer.AboveSeries,
                MinimumX = 0.55,
                MaximumX = 0.9,
                MinimumY = _accelerationAxis.Minimum,
                MaximumY = _accelerationAxis.Minimum
            };

            TelemetryGraphModel.Annotations.Add(_targetDividerAnnotation);
            TelemetryGraphModel.Annotations.Add(_leftTargetBarAnnotation);
            TelemetryGraphModel.Annotations.Add(_rightTargetBarAnnotation);

            UpdateTelemetryGraphFilters();
        }

        /// <summary>Redraws the telemetry graph, providing enough time has passed since the last redraw to achieve the desired refresh rate</summary>
        private void RedrawGraph()
        {
            DateTime now = DateTime.UtcNow;
            if (now - _lastPlotRefresh >= PlotRefreshInterval)
            {
                TelemetryGraphModel.InvalidatePlot(true);
                _lastPlotRefresh = now;
            }
        }

        // <summary>Updates the visibility of the various plots</summary>
        private void UpdateTelemetryGraphFilters()
        {
            if (_surgeSeries != null)
            {
                _surgeSeries.IsVisible = Settings.ShowSurgePlot;
                ToggleThresholdlLine(_surgeMinimumAnnotation, Settings.ShowSurgePlot);
                ToggleThresholdlLine(_surgeMaximumAnnotation, Settings.ShowSurgePlot);
            }
            if (_swaySeries != null)
            {
                _swaySeries.IsVisible = Settings.ShowSwayPlot;
                ToggleThresholdlLine(_swayMinimumAnnotation, Settings.ShowSwayPlot);
                ToggleThresholdlLine(_swayMaximumAnnotation, Settings.ShowSwayPlot);
            }
            if (_heaveSeries != null)
            {
                _heaveSeries.IsVisible = Settings.ShowHeavePlot;
                ToggleThresholdlLine(_heaveMinimumAnnotation, Settings.ShowHeavePlot);
                ToggleThresholdlLine(_heaveMaximumAnnotation, Settings.ShowHeavePlot);
            }
            if (_leftTorqueSeries != null)
            {
                _leftTorqueSeries.IsVisible = Settings.ShowTorquePlot;
            }
            if (_rightTorqueSeries != null)
            {
                _rightTorqueSeries.IsVisible = Settings.ShowTorquePlot;
            }

            TelemetryGraphModel.InvalidatePlot(true);

            RedrawGraph();
        }

        /// <summary>Applies the given telemetry data to the telemetry graph and requests (but does not guarantee) a redraw</summary>
        private void UpdateTelemetryGraph(double surge, double sway, double heave, double leftTorque, double rightTorque)
        {
            double x = _plotPointIndex++;

            _surgeSeries.Points.Add(new DataPoint(x, surge));
            _swaySeries.Points.Add(new DataPoint(x, sway));
            _heaveSeries.Points.Add(new DataPoint(x, heave));
            _leftTorqueSeries.Points.Add(new DataPoint(x, leftTorque * 100));
            _rightTorqueSeries.Points.Add(new DataPoint(x, rightTorque * 100));

            if (_surgeSeries.Points.Count > _maximumPlotPoints)
            {
                _surgeSeries.Points.RemoveAt(0);
                _swaySeries.Points.RemoveAt(0);
                _heaveSeries.Points.RemoveAt(0);
                _leftTorqueSeries.Points.RemoveAt(0);
                _rightTorqueSeries.Points.RemoveAt(0);
            }

            double leftTorqueClamped = ClampTo(leftTorque, 0.0, 1.0);
            double rightTorqueClamped = ClampTo(rightTorque, 0.0, 1.0);
            double graphHeight = _accelerationAxis.Maximum - _accelerationAxis.Minimum;

            _leftTargetBarAnnotation.MinimumY = _accelerationAxis.Minimum;
            _leftTargetBarAnnotation.MaximumY = _accelerationAxis.Minimum + (leftTorqueClamped * graphHeight);

            _rightTargetBarAnnotation.MinimumY = _accelerationAxis.Minimum;
            _rightTargetBarAnnotation.MaximumY = _accelerationAxis.Minimum + (rightTorqueClamped * graphHeight);

            RedrawGraph();
        }

        /// <summary>Applies the given telemetry thresholds to the telemetry graph and requests (but does not guarantee) a redraw</summary>
        private void UpdateTelemetryGraphThresholds(DeviceSettings settings)
        {
            if (TelemetryGraphModel == null)
            {
                return;
            }

            _surgeMinimumAnnotation.Y = settings.MinimumSurge;
            _surgeMaximumAnnotation.Y = settings.MaximumSurge;
            _swayMinimumAnnotation.Y = settings.MinimumSway;
            _swayMaximumAnnotation.Y = settings.MaximumSway;
            _heaveMinimumAnnotation.Y = settings.MinimumHeave;
            _heaveMaximumAnnotation.Y = settings.MaximumHeave;

            TelemetryGraphModel.InvalidatePlot(true);

            RedrawGraph();
        }

        /// <summary>Adds and returns a new threshold line of the given color to the telemetry graph</summary>
        private LineAnnotation AddThresholdLine(OxyColor color)
        {
            LineAnnotation annotation = new LineAnnotation
            {
                Type = LineAnnotationType.Horizontal,
                Color = color,
                StrokeThickness = 2,
                LineStyle = LineStyle.Dot,
                Y = 0
            };

            TelemetryGraphModel.Annotations.Add(annotation);

            return annotation;
        }

        /// <summary>Toggles the removal/addition of an existing threshold line</summary>
        private void ToggleThresholdlLine(Annotation annotation, bool shouldShow)
        {
            if (annotation == null || TelemetryGraphModel == null)
            {
                return;
            }

            if (TelemetryGraphModel.Annotations.Contains(annotation))
            {
                if (!shouldShow)
                {
                    TelemetryGraphModel.Annotations.Remove(annotation);
                }
            }
            else
            {
                if (shouldShow)
                {
                    TelemetryGraphModel.Annotations.Add(annotation);
                }
            }
        }

        /// <summary>Adds and returns a new telemetry line of the given title, color and style to the telemetry graph</summary>
        private LineSeries AddTelemetryLine(string axisKey, string title, OxyColor color, LineStyle style = LineStyle.Solid)
        {
            LineSeries series = new LineSeries
            {
                YAxisKey = axisKey,
                Title = title,
                Color = color,
                StrokeThickness = 2,
                LineStyle = style
            };

            series.Points.Capacity = _maximumPlotPoints;

            TelemetryGraphModel.Series.Add(series);

            return series;
        }
    }
}
