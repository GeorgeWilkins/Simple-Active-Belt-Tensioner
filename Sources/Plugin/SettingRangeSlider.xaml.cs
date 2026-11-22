using Newtonsoft.Json.Linq;
using SimHub;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace User.ActiveBeltTensioner
{
    public partial class SettingRangeSlider : UserControl
    {
        private enum MirroredValueSource
        {
            LeftValue,
            RightValue
        }

        private const int AutomaticMirroringDelay = 20;
        private readonly DispatcherTimer _automaticMirroringTimer;

        private bool _isMirroring;
        private bool _isCoercing;
        private bool _hasLeftValue;
        private bool _hasRightValue;
        private MirroredValueSource _lastChangedValueSource;

        public SettingRangeSlider()
        {
            InitializeComponent();

            _automaticMirroringTimer = new DispatcherTimer();
            _automaticMirroringTimer.Interval = TimeSpan.FromMilliseconds(AutomaticMirroringDelay);
            _automaticMirroringTimer.Tick += OnAutomaticMirroringTimerTick;

            IsEnabledChanged += (s, e) =>
            {
                UpdateSliderOpacity();
                UpdateCenterButtonVisibility();

                if (!IsEnabled)
                {
                    StopAutomaticMirroringTimer();
                }
            };

            Unloaded += (s, e) => StopAutomaticMirroringTimer();

            UpdateSliderOpacity();
            UpdateDisplayValues();
        }

        private void UpdateSliderOpacity()
        {
            RangeSliderControl.Opacity = IsEnabled ? 1.0 : 0.5;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            UpdateSliderOpacity();
            UpdateDisplayValues();
            UpdateCenterButtonVisibility();
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SettingRangeSlider),
            new PropertyMetadata(string.Empty)
        );

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(
            nameof(Unit),
            typeof(string),
            typeof(SettingRangeSlider),
            new PropertyMetadata(string.Empty)
        );

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(SettingRangeSlider),
            new PropertyMetadata(0d, OnRangeChanged)
        );

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(SettingRangeSlider),
            new PropertyMetadata(100d, OnRangeChanged)
        );

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
            nameof(Step),
            typeof(double),
            typeof(SettingRangeSlider),
            new PropertyMetadata(1d, OnStepChanged)
        );

        public double Step
        {
            get { return (double)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }

        public static readonly DependencyProperty LeftValueProperty = DependencyProperty.Register(
            nameof(LeftValue),
            typeof(double),
            typeof(SettingRangeSlider),
            new FrameworkPropertyMetadata(
                0d,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnLeftValueChanged,
                CoerceLeftValue
            )
        );

        public static readonly DependencyProperty RightValueProperty = DependencyProperty.Register(
            nameof(RightValue),
            typeof(double),
            typeof(SettingRangeSlider),
            new FrameworkPropertyMetadata(
                0d,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnRightValueChanged,
                CoerceRightValue
            )
        );

        public double LeftValue
        {
            get { return (double)GetValue(LeftValueProperty); }
            set { SetValue(LeftValueProperty, value); }
        }

        public double RightValue
        {
            get { return (double)GetValue(RightValueProperty); }
            set { SetValue(RightValueProperty, value); }
        }

        public static readonly DependencyProperty ShowCenterProperty = DependencyProperty.Register(
            nameof(ShowCenter),
            typeof(bool),
            typeof(SettingRangeSlider),
            new PropertyMetadata(false, OnShowCenterChanged)
        );

        public bool ShowCenter
        {
            get { return (bool)GetValue(ShowCenterProperty); }
            set { SetValue(ShowCenterProperty, value); }
        }

        public static readonly DependencyProperty ShouldAutomaticallyMirrorProperty = DependencyProperty.Register(
            nameof(ShouldAutomaticallyMirror),
            typeof(bool),
            typeof(SettingRangeSlider),
            new PropertyMetadata(false, OnShouldAutomaticallyMirrorChanged)
        );

        public bool ShouldAutomaticallyMirror
        {
            get { return (bool)GetValue(ShouldAutomaticallyMirrorProperty); }
            set { SetValue(ShouldAutomaticallyMirrorProperty, value); }
        }

        private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;

            control.UpdateDisplayValues();
        }

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;

            control._isCoercing = true;

            try
            {
                control.CoerceValue(LeftValueProperty);
                control.CoerceValue(RightValueProperty);
            }
            finally
            {
                control._isCoercing = false;
            }

            control.UpdateDisplayValues();
        }

        private static void OnLeftValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;

            if (!control._isCoercing)
            {
                control._hasLeftValue = true;
            }

            control.UpdateDisplayValues();
            control.ScheduleAutomaticMirroring(MirroredValueSource.LeftValue);
        }

        private static void OnRightValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;

            if (!control._isCoercing)
            {
                control._hasRightValue = true;
            }

            control.UpdateDisplayValues();
            control.ScheduleAutomaticMirroring(MirroredValueSource.RightValue);
        }

        private static void OnShowCenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;

            control.UpdateCenterButtonVisibility();
        }

        private static void OnShouldAutomaticallyMirrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;

            if (!(bool)e.NewValue)
            {
                control.StopAutomaticMirroringTimer();
                return;
            }

            control.CoerceValue(LeftValueProperty);
            control.CoerceValue(RightValueProperty);
            control.UpdateDisplayValues();
        }

        private void OnCenterClick(object sender, RoutedEventArgs e)
        {
            CenterValues();
        }

        private void CenterValues()
        {
            var midpoint = GetMidpoint();
            var range = RightValue - LeftValue;

            SetCurrentValue(LeftValueProperty, midpoint - (range / 2d));
            SetCurrentValue(RightValueProperty, midpoint + (range / 2d));
        }

        private void ScheduleAutomaticMirroring(MirroredValueSource source)
        {
            if (_isMirroring)
            {
                return;
            }

            _lastChangedValueSource = source;
            _automaticMirroringTimer.Stop();

            if (ShouldAutomaticallyMirror && IsEnabled)
            {
                _automaticMirroringTimer.Start();
            }
        }

        private void StopAutomaticMirroringTimer()
        {
            _automaticMirroringTimer.Stop();
        }

        private void OnAutomaticMirroringTimerTick(object sender, EventArgs e)
        {
            _automaticMirroringTimer.Stop();
            ApplyAutomaticMirroring();
        }

        private void ApplyAutomaticMirroring()
        {
            var midpoint = GetMidpoint();

            _isMirroring = true;

            try
            {
                if (_lastChangedValueSource == MirroredValueSource.LeftValue)
                {
                    SetCurrentValue(RightValueProperty, (midpoint * 2d) - LeftValue);
                }
                else
                {
                    SetCurrentValue(LeftValueProperty, (midpoint * 2d) - RightValue);
                }
            }
            finally
            {
                _isMirroring = false;
            }
        }

        private static object CoerceLeftValue(DependencyObject d, object baseValue)
        {
            var control = (SettingRangeSlider)d;
            var maximum = control._hasRightValue ? control.RightValue : control.Maximum;

            if (control.ShouldAutomaticallyMirror)
            {
                maximum = Math.Min(maximum, control.GetMidpoint());
            }

            return Clamp((double)baseValue, control.Minimum, maximum);
        }

        private static object CoerceRightValue(DependencyObject d, object baseValue)
        {
            var control = (SettingRangeSlider)d;
            var minimum = control._hasLeftValue ? control.LeftValue : control.Minimum;

            if (control.ShouldAutomaticallyMirror)
            {
                minimum = Math.Max(minimum, control.GetMidpoint());
            }

            return Clamp((double)baseValue, minimum, control.Maximum);
        }

        private void UpdateDisplayValues()
        {
            var leftValueTextBlock = FindName("LeftValueTextBlock") as TextBlock;
            if (leftValueTextBlock != null)
            {
                leftValueTextBlock.Text = FormatValueForStep(LeftValue, Step);
            }

            var rightValueTextBlock = FindName("RightValueTextBlock") as TextBlock;
            if (rightValueTextBlock != null)
            {
                rightValueTextBlock.Text = FormatValueForStep(RightValue, Step);
            }
        }

        private void UpdateCenterButtonVisibility()
        {
            var centerButton = FindName("CenterButton") as Button;
            if (centerButton != null)
            {
                centerButton.Visibility = (ShowCenter && IsEnabled) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static string FormatValueForStep(double value, double step)
        {
            var decimals = GetDecimalPlaces(step);
            return value.ToString($"F{decimals}", CultureInfo.CurrentCulture);
        }

        private static int GetDecimalPlaces(double step)
        {
            if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
            {
                return 0;
            }

            var text = step.ToString("0.#############################", CultureInfo.InvariantCulture);
            var decimalIndex = text.IndexOf('.');

            return decimalIndex < 0 ? 0 : text.Length - decimalIndex - 1;
        }

        private double GetMidpoint()
        {
            return Minimum + ((Maximum - Minimum) / 2d);
        }

        private static double Clamp(double value, double minimum, double maximum)
        {
            if (minimum > maximum)
            {
                var temporary = minimum;
                minimum = maximum;
                maximum = temporary;
            }

            return Math.Max(minimum, Math.Min(maximum, value));
        }
    }
}
