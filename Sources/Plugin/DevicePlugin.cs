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
using WoteverLocalization;


namespace User.ActiveBeltTensioner
{
    [PluginDescription("A control panel for the 'Simple Active Belt Tensioner'")]
    [PluginAuthor("George Wilkins")]
    [PluginName("Simple Active Belt Tensioner")]
    public class DevicePlugin : IPlugin, IDataPlugin, IWPFSettingsV2
    {
        public DeviceSettings Settings;

        public PluginManager PluginManager { get; set; }

        public ImageSource PictureIcon => this.ToIcon(Properties.Resources.MenuIcon);

        public string LeftMenuTitle => SLoc.GetValue("SABT_Plugin");


        public MotorController MotorController;

        private static string _settingsName = "SimpleActiveBeltTensioner";

        private readonly object _motorControllerLock = new object();

        private readonly object _telemetryLock = new object();
        private TelemetrySnapshot _latestTelemetry;

        private readonly AutoResetEvent _hasTelemetryArrived = new AutoResetEvent(false);
        private Thread _controlThread;
        private volatile bool _runControlLoop = false;
        private volatile bool _hasBeenInactive = true;

        public struct TelemetrySnapshot
        {
            public double? Surge;
            public double? Sway;
            public double? Heave;
            public double? Speed;
            public bool DidUpshift;
            public bool IsActive;
        }

        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            return new DeviceControl(this);
        }

        /// <summary>Called by SimHub to initialise the plugin</summary>
        public void Init(PluginManager pluginManager)
        {
            Logging.Current.Info("SABT: Initialising...");

            Settings = this.ReadCommonSettings<DeviceSettings>(_settingsName, () => new DeviceSettings());
            Settings.PropertyChanged += OnSettingsChanged;

            MotorController = new MotorController(this);
            if (Settings.IsEnabled && Settings.IsSerialPortValid)
            {
                DoWithoutWaiting(devicePlugin =>
                {
                    devicePlugin.MotorController.Connect();
                });
            }

            InitialiseTelemetryGraph();
            UpdateTelemetryGraphThresholds(Settings);
            UpdateTelemetryGraph(0, 0, 0);

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
                e.PropertyName == nameof(Settings.SerialPort) ||
                e.PropertyName == nameof(Settings.IsEnabled)
            )
            {
                if (Settings.IsEnabled)
                {
                    if (Settings.IsSerialPortValid)
                    {
                        DoWithoutWaiting(devicePlugin =>
                        {
                            devicePlugin.MotorController.Connect();
                        });
                    }
                }
                else
                {
                    _hasBeenInactive = true;

                    DoWithoutWaiting(devicePlugin =>
                    {
                        devicePlugin.MotorController.Disconnect();
                    });
                }
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
            }
        }

        /// <summary>Called by SimHub when new telemetry data is available</summary>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            if (!Settings.IsEnabled) { return; }

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

            _hasTelemetryArrived.Set();

            if (telemetrySnapshot.IsActive) {

                UpdateTelemetryGraph(
                    telemetrySnapshot.Surge ?? 0,
                    telemetrySnapshot.Sway ?? 0,
                    telemetrySnapshot.Heave ?? 0
                );

            }
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

        /// <summary>Evalulates the <see cref="TelemetrySnapshot"/> propeties and calculates the appropriate effects to apply</summary>
        /// <remarks>Runs as a separate thread to keep effects processing and motor commands out of the <see cref="DataUpdate"/> calls</remarks>
        private void ControlLoop()
        {
            while (_runControlLoop)
            {
                if (!_runControlLoop)
                {
                    break;
                }

                _hasTelemetryArrived.WaitOne();

                if (!Settings.IsEnabled)
                {
                    _hasBeenInactive = true;

                    continue;
                }

                TelemetrySnapshot telemetrySnapshot;
                lock (_telemetryLock)
                {
                    telemetrySnapshot = _latestTelemetry;
                }
                
                MotorController motorController;
                lock (_motorControllerLock)
                {
                    motorController = MotorController;
                }

                if (_hasBeenInactive)
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
                        Settings.IsEnabled = false;

                        continue;
                    }

                    _hasBeenInactive = false;
                }

                try
                {
                    // Preferences
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

                    // Tuning
                    int minimumSurge = Settings.MinimumSurge;
                    int maximumSurge = Settings.MaximumSurge;
                    int minimumSway = Settings.MinimumSway;
                    int maximumSway = Settings.MaximumSway;
                    int minimumHeave = Settings.MinimumHeave;
                    int maximumHeave = Settings.MaximumHeave;

                    // Telemetry
                    bool isMoving = telemetrySnapshot.Speed > 0.2;
                    bool didUpshift = telemetrySnapshot.DidUpshift;
                    double surge = telemetrySnapshot.Surge ?? 0.0;
                    double sway = (ConvertToFractionOfRange(telemetrySnapshot.Sway ?? 0.0, minimumSway, maximumSway) * 2.0) - 1.0;
                    double heave = telemetrySnapshot.Heave ?? 0.0;
                    double speed = telemetrySnapshot.Speed ?? 0.0;

                    double braking = ConvertToFractionOfRange(surge, 0, maximumSurge);
                    double acceleration = 1.0 - ConvertToFractionOfRange(surge, minimumSurge, 0);
                    double landing = ConvertToFractionOfRange(heave, 0, maximumHeave);
                    double jumping = 1.0 - ConvertToFractionOfRange(heave, minimumHeave, 0);

                    // Effects
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

                    if (didUpshift && shiftingStrength > 0.0)
                    {
                        Logging.Current.Info("SABT: Upshift detected (@" + speed + ")");

                        // @TODO: A very crude and temporary proof-of-concept (replace with time-controlled muliplier of underlying negative surge force)
                        if (!motorController.IsBusy)
                        {
                            motorController.SetTorques(0.0, 0.0);
                            Thread.Sleep((int)(shiftingStrength * 1000));
                        }
                    }

                    // Combinator
                    double totalModifierLeft = increasingModifierLeft - decreasingModifierLeft;
                    double totalModifierRight = increasingModifierRight - decreasingModifierRight;

                    if (totalModifierLeft < 0.0)
                    {
                        leftTarget = minimumTension + (totalModifierLeft * minimumTension);
                    }
                    else {
                        leftTarget = minimumTension + totalModifierLeft;
                    }
                    if (totalModifierRight < 0.0)
                    {
                        rightTarget = minimumTension + (totalModifierRight * minimumTension);
                    }
                    else
                    {
                        rightTarget = minimumTension + totalModifierRight;
                    }

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

                    // Send To Motors
                    if (!motorController.IsBusy)
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

                            Settings.IsEnabled = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Current.Error("SABT: " + ex.Message);
                }
            }
        }

        /// <summary>A task wrapper for the <see cref="ActiveBeltTensioner.DevicePlugin" /> instance, allowing logic in this and other classes to asynchronously execute actions</summary>
        public async Task DoWithoutWaiting(Action<DevicePlugin> taskToPerform)
        {
            await Task.Run(() => taskToPerform(this));
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




        public PlotModel TelemetryGraphModel { get; private set; }

        private LineSeries _surgeSeries;
        private LineSeries _swaySeries;
        private LineSeries _heaveSeries;

        private LineAnnotation _surgeMinimumAnnotation;
        private LineAnnotation _surgeMaximumAnnotation;
        private LineAnnotation _swayMinimumAnnotation;
        private LineAnnotation _swayMaximumAnnotation;
        private LineAnnotation _heaveMinimumAnnotation;
        private LineAnnotation _heaveMaximumAnnotation;

        private int _plotPointIndex = 0;
        private const int MaxPlotPoints = 200;

        private DateTime _lastPlotRefresh = DateTime.MinValue;
        private static readonly TimeSpan PlotRefreshInterval = TimeSpan.FromMilliseconds(33);

        /// <summary>Initialises the telemetry graph instance and configures its styling and legends</summary>
        private void InitialiseTelemetryGraph()
        {
            OxyColor blue = OxyColor.Parse("#119eda");
            OxyColor grey = OxyColor.Parse("#454545");

            TelemetryGraphModel = new PlotModel {
                Title = " ",
                TextColor = OxyColors.White,
                LegendTextColor = OxyColors.White,
                PlotAreaBorderColor = OxyColors.Transparent,
                PlotType = PlotType.XY
            };

            TelemetryGraphModel.Axes.Add(
                new LinearAxis {
                    Title = "m/s²",
                    Position = AxisPosition.Left,
                    MajorGridlineStyle = LineStyle.Solid,
                    MajorGridlineColor = grey,
                    MinorGridlineStyle = LineStyle.Dot,
                    MinorGridlineColor = grey,
                    TicklineColor = OxyColors.Transparent,
                    Minimum = -50,
                    Maximum = 100,
                    IsPanEnabled = false,
                    IsZoomEnabled = false
                }
            );

            TelemetryGraphModel.Axes.Add(
                new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    IsAxisVisible = false,
                    IsPanEnabled = false,
                    IsZoomEnabled = false
                }
            );

            _surgeSeries = AddTelemetryLine(SLoc.GetValue("SABT_Legend_Surge"), OxyColors.Red);
            _surgeMinimumAnnotation = AddThresholdLine(OxyColors.Red);
            _surgeMaximumAnnotation = AddThresholdLine(OxyColors.Red);

            _swaySeries = AddTelemetryLine(SLoc.GetValue("SABT_Legend_Sway"), OxyColors.Green);
            _swayMinimumAnnotation = AddThresholdLine(OxyColors.Green);
            _swayMaximumAnnotation = AddThresholdLine(OxyColors.Green);

            _heaveSeries = AddTelemetryLine(SLoc.GetValue("SABT_Legend_Heave"), OxyColors.Blue);
            _heaveMinimumAnnotation = AddThresholdLine(OxyColors.Blue);
            _heaveMaximumAnnotation = AddThresholdLine(OxyColors.Blue);
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

        /// <summary>Applies the given telemetry data to the telemetry graph and requests (but does not guarantee) a redraw</summary>
        private void UpdateTelemetryGraph(double surge, double sway, double heave)
        {
            double x = _plotPointIndex++;

            _surgeSeries.Points.Add(new DataPoint(x, surge));
            _swaySeries.Points.Add(new DataPoint(x, sway));
            _heaveSeries.Points.Add(new DataPoint(x, heave));

            if (_surgeSeries.Points.Count > MaxPlotPoints)
            {
                _surgeSeries.Points.RemoveAt(0);
                _swaySeries.Points.RemoveAt(0);
                _heaveSeries.Points.RemoveAt(0);
            }

            RedrawGraph();
        }

        /// <summary>Applies the given telemetry thresholds to the telemetry graph and requests (but does not guarantee) a redraw</summary>
        private void UpdateTelemetryGraphThresholds(DeviceSettings settings)
        {
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
                StrokeThickness = 1,
                LineStyle = LineStyle.Dot,
                Y = 0,
            };

            TelemetryGraphModel.Annotations.Add(annotation);

            return annotation;
        }

        /// <summary>Adds and returns a new telemetry line of the given title and color to the telemetry graph</summary>
        private LineSeries AddTelemetryLine(string title, OxyColor color)
        {
            LineSeries series = new LineSeries
            {
                Title = title,
                Color = color,
                StrokeThickness = 1
            };

            series.Points.Capacity = MaxPlotPoints;

            TelemetryGraphModel.Series.Add(series);

            return series;
        }
    }
}