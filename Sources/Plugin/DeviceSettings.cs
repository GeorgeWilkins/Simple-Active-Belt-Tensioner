using Microsoft.VisualBasic;
using Newtonsoft.Json;
using SimHub;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using WoteverLocalization;

namespace User.ActiveBeltTensioner
{
    public class DeviceSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private DevicePlugin _plugin;

        private readonly object _profilesLock = new object();

        private bool _isInitialised = false;

        /// <summary>Invoke when the current settings should be serialized to the plugin's JSON configuration file</summary>
        [JsonIgnore]
        public Action Persist { get; set; }

        public string DefaultUpshiftingModifiers = "0ms:100% 150ms:100% 300ms:0%";

        public string PluginVersion = "0.0.0.0";

        public void Initialise(DevicePlugin plugin)
        {
            _plugin = plugin;
            _isInitialised = true;

            ApplyTransition();

            ChangeActiveProfile();
        }
        
        /// <summary>Applies any required settings value transitions, then sets the the stored plugin version for future transitions</summary>
        /// <remarks>This can be used to apply new defaults, merge or migrate properties, change types and scales, etc</remarks>
        public DeviceSettings ApplyTransition()
        {
            string currentVersion = typeof(DevicePlugin).Assembly.GetName().Version.ToString(4) ?? new Version(1, 0).ToString(4);
            string priorVersion = PluginVersion;

            switch (true)
            {
                case bool _ when IsTransitioning(priorVersion, currentVersion, "0.*.*.*", "1.*.*.*"):

                    // TODO: Add any migration logic here for settings values

                    break;

                case bool _ when IsTransitioning(priorVersion, currentVersion, "0.8.*.*", "0.9.*.*"):

                    // TODO: Add any migration logic here for settings values

                    break;
            }

            PluginVersion = currentVersion;

            return this;
        }

        /// <summary>Indicates if these settings are transitioning between the specified plugin versions</summary>
        /// <remarks>Supports wildcard patterns in the version strings like `0.9.*.*` or `1.*.*.*`</remarks>
        private static bool IsTransitioning(string priorVersion, string currentVersion, string priorPattern, string currentPattern)
        {
            return (
                IsVersionMatch(priorVersion, priorPattern) &&
                IsVersionMatch(currentVersion, currentPattern)
            );
        }

        private static bool IsVersionMatch(string version, string wildcardPattern)
        {
            if (string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(wildcardPattern))
            {
                return false;
            }

            string regexPattern = "^" + Regex.Escape(wildcardPattern).Replace("\\*", ".*") + "$";

            return Regex.IsMatch(
                version,
                regexPattern,
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
            );
        }

        private void InvokePropertyChange([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

            GameTuningProfile profile = GetActiveProfile();

            if (profile is GameTuningProfile)
            {
                profile.SetTuningProperty(
                    name,
                    this.GetType().GetProperty(name)?.GetValue(this)
                );
            }
        }

        private string _deviceIdentifier = null;
        public string DeviceIdentifier
        {
            get { return _deviceIdentifier; }
            set
            {
                if (_deviceIdentifier != value)
                {
                    _deviceIdentifier = value;
                    InvokePropertyChange(nameof(DeviceIdentifier));
                }
            }
        }

        private bool _startAutomatically = false;
        public bool StartAutomatically
        {
            get { return _startAutomatically; }
            set
            {
                if (_startAutomatically != value)
                {
                    _startAutomatically = value;
                    InvokePropertyChange(nameof(StartAutomatically));
                }
            }
        }

        private bool _isAutomaticallySwitching = true;
        public bool IsAutomaticallySwitching
        {
            get { return _isAutomaticallySwitching; }
            set
            {
                if (_isAutomaticallySwitching != value)
                {
                    _isAutomaticallySwitching = value;
                    InvokePropertyChange(nameof(IsAutomaticallySwitching));

                    if (_isAutomaticallySwitching)
                    {
                        ChangeActiveProfile();
                    }
                }
            }
        }

        private int _reducedOutputTemperature = 50;
        public int ReducedOutputTemperature
        {
            get { return _reducedOutputTemperature; }
            set
            {
                if (_reducedOutputTemperature != value)
                {
                    _reducedOutputTemperature = value;
                    InvokePropertyChange(nameof(ReducedOutputTemperature));
                }
            }
        }

        private int _stoppedOutputTemperature = 60;
        public int StoppedOutputTemperature
        {
            get { return _stoppedOutputTemperature; }
            set
            {
                if (_stoppedOutputTemperature != value)
                {
                    _stoppedOutputTemperature = value;
                    InvokePropertyChange(nameof(StoppedOutputTemperature));
                }
            }
        }

        private double _idleTension = 0.15;
        public double IdleTension
        {
            get { return _idleTension; }
            set
            {
                if (Math.Abs(_idleTension - value) > double.Epsilon)
                {
                    _idleTension = value;
                    InvokePropertyChange(nameof(IdleTension));
                }
            }
        }

        private double _minimumTension = 20.0;
        public double MinimumTension
        {
            get { return _minimumTension; }
            set
            {
                value = Math.Min(
                    Math.Max(value, 0.0),
                    _maximumTension - _tensionStep
                );

                if (Math.Abs(_minimumTension - value) > double.Epsilon)
                {
                    _minimumTension = value;
                    InvokePropertyChange(nameof(MinimumTension));
                }
            }
        }

        private double _maximumTension = 100.0;
        public double MaximumTension
        {
            get { return _maximumTension; }
            set
            {
                value = Math.Min(
                    Math.Max(value, _minimumTension + _tensionStep),
                    100.0
                );

                if (Math.Abs(_maximumTension - value) > double.Epsilon)
                {
                    _maximumTension = value;
                    InvokePropertyChange(nameof(MaximumTension));
                }
            }
        }

        private double _tensionStep = 0.1;
        public double TensionStep
        {
            get { return _tensionStep; }
            set
            {
                value = Math.Min(
                    Math.Max(value, 0.1),
                    20.0
                );

                if (Math.Abs(_tensionStep - value) > double.Epsilon)
                {
                    _tensionStep = value;
                    InvokePropertyChange(nameof(TensionStep));
                }
            }
        }


        private double _driverMass = 1.0;
        public double DriverMass
        {
            get { return _driverMass; }
            set
            {
                double clampedValue = Math.Max(0.1, Math.Min(3.0, value));

                if (_driverMass != clampedValue)
                {
                    _driverMass = clampedValue;
                    InvokePropertyChange(nameof(DriverMass));
                }
            }
        }

        private double _driverDamping = 500;
        public double DriverDamping
        {
            get { return _driverDamping; }
            set
            {
                double clampedValue = Math.Max(0.0, Math.Min(1000.0, value));

                if (_driverDamping != clampedValue)
                {
                    _driverDamping = clampedValue;
                    InvokePropertyChange(nameof(DriverDamping));
                }
            }
        }

        private double _seatDegreesFromVertical = 20.0;
        public double SeatDegreesFromVertical
        {
            get { return _seatDegreesFromVertical; }
            set
            {
                double clampedValue = Math.Max(-60.0, Math.Min(60.0, value));

                if (_seatDegreesFromVertical != clampedValue)
                {
                    _seatDegreesFromVertical = clampedValue;
                    InvokePropertyChange(nameof(SeatDegreesFromVertical));
                }
            }
        }

        private double _leftShoulderBeltDegreesFromVertical = -30.0;
        public double LeftShoulderBeltDegreesFromVertical
        {
            get { return _leftShoulderBeltDegreesFromVertical; }
            set
            {
                double clampedValue = Math.Max(-90.0, Math.Min(90.0, value));

                if (_leftShoulderBeltDegreesFromVertical != clampedValue)
                {
                    _leftShoulderBeltDegreesFromVertical = clampedValue;
                    InvokePropertyChange(nameof(LeftShoulderBeltDegreesFromVertical));
                }
            }
        }

        private double _rightShoulderBeltDegreesFromVertical = 30.0;
        public double RightShoulderBeltDegreesFromVertical
        {
            get { return _rightShoulderBeltDegreesFromVertical; }
            set
            {
                double clampedValue = Math.Max(-90.0, Math.Min(90.0, value));

                if (_rightShoulderBeltDegreesFromVertical != clampedValue)
                {
                    _rightShoulderBeltDegreesFromVertical = clampedValue;
                    InvokePropertyChange(nameof(RightShoulderBeltDegreesFromVertical));
                }
            }
        }

        private double _leftWaistBeltDegreesFromVertical = -45.0;
        public double LeftWaistBeltDegreesFromVertical
        {
            get { return _leftWaistBeltDegreesFromVertical; }
            set
            {
                double clampedValue = Math.Max(-90.0, Math.Min(90.0, value));

                if (_leftWaistBeltDegreesFromVertical != clampedValue)
                {
                    _leftWaistBeltDegreesFromVertical = clampedValue;
                    InvokePropertyChange(nameof(LeftWaistBeltDegreesFromVertical));
                }
            }
        }

        private double _rightWaistBeltDegreesFromVertical = 45.0;
        public double RightWaistBeltDegreesFromVertical
        {
            get { return _rightWaistBeltDegreesFromVertical; }
            set
            {
                double clampedValue = Math.Max(-90.0, Math.Min(90.0, value));

                if (_rightWaistBeltDegreesFromVertical != clampedValue)
                {
                    _rightWaistBeltDegreesFromVertical = clampedValue;
                    InvokePropertyChange(nameof(RightWaistBeltDegreesFromVertical));
                }
            }
        }

        private double _leftShoulderBeltDistance = 0.3;
        public double LeftShoulderBeltDistance
        {
            get { return _leftShoulderBeltDistance; }
            set
            {
                double clampedValue = Math.Max(0.0, Math.Min(1.0, value));

                if (_leftShoulderBeltDistance != clampedValue)
                {
                    _leftShoulderBeltDistance = clampedValue;
                    InvokePropertyChange(nameof(LeftShoulderBeltDistance));
                }
            }
        }

        private double _rightShoulderBeltDistance = 0.3;
        public double RightShoulderBeltDistance
        {
            get { return _rightShoulderBeltDistance; }
            set
            {
                double clampedValue = Math.Max(0.0, Math.Min(1.0, value));

                if (_rightShoulderBeltDistance != clampedValue)
                {
                    _rightShoulderBeltDistance = clampedValue;
                    InvokePropertyChange(nameof(RightShoulderBeltDistance));
                }
            }
        }

        private double _leftWaistBeltDistance = 0.7;
        public double LeftWaistBeltDistance
        {
            get { return _leftWaistBeltDistance; }
            set
            {
                double clampedValue = Math.Max(0.0, Math.Min(1.0, value));

                if (_leftWaistBeltDistance != clampedValue)
                {
                    _leftWaistBeltDistance = clampedValue;
                    InvokePropertyChange(nameof(LeftWaistBeltDistance));
                }
            }
        }

        private double _rightWaistBeltDistance = 0.7;
        public double RightWaistBeltDistance
        {
            get { return _rightWaistBeltDistance; }
            set
            {
                double clampedValue = Math.Max(0.0, Math.Min(1.0, value));

                if (_rightWaistBeltDistance != clampedValue)
                {
                    _rightWaistBeltDistance = clampedValue;
                    InvokePropertyChange(nameof(RightWaistBeltDistance));
                }
            }
        }

        private double _minimumSurge = -8.0;
        public double MinimumSurge
        {
            get { return _minimumSurge; }
            set
            {
                if (Math.Abs(_minimumSurge - value) > double.Epsilon)
                {
                    _minimumSurge = Math.Min(value, _maximumSurge);
                    InvokePropertyChange(nameof(MinimumSurge));
                }
            }
        }

        private double _maximumSurge = 25.0;
        public double MaximumSurge
        {
            get { return _maximumSurge; }
            set
            {
                if (Math.Abs(_maximumSurge - value) > double.Epsilon)
                {
                    _maximumSurge = Math.Max(value, _minimumSurge);
                    InvokePropertyChange(nameof(MaximumSurge));
                }
            }
        }

        private double _minimumSway = -25.0;
        public double MinimumSway
        {
            get { return _minimumSway; }
            set
            {
                if (Math.Abs(_minimumSway - value) > double.Epsilon)
                {
                    _minimumSway = Math.Min(value, _maximumSway);
                    InvokePropertyChange(nameof(MinimumSway));
                }
            }
        }

        private double _maximumSway = 25.0;
        public double MaximumSway
        {
            get { return _maximumSway; }
            set
            {
                if (Math.Abs(_maximumSway - value) > double.Epsilon)
                {
                    _maximumSway = Math.Max(value, _minimumSway);
                    InvokePropertyChange(nameof(MaximumSway));
                }
            }
        }

        private double _minimumHeave = -25.0;
        public double MinimumHeave
        {
            get { return _minimumHeave; }
            set
            {
                value = Math.Min(value, _maximumHeave);

                if (Math.Abs(_minimumHeave - value) > double.Epsilon)
                {
                    _minimumHeave = value;
                    InvokePropertyChange(nameof(MinimumHeave));
                }
            }
        }

        private double _maximumHeave = 75.0;
        public double MaximumHeave
        {
            get { return _maximumHeave; }
            set
            {
                value = Math.Max(value, _minimumHeave);

                if (Math.Abs(_maximumHeave - value) > double.Epsilon)
                {
                    _maximumHeave = value;
                    InvokePropertyChange(nameof(MaximumHeave));
                }
            }
        }

        private double _horizontalBias = 0.0;
        public double HorizontalBias
        {
            get { return _horizontalBias; }
            set {
                if (Math.Abs(_horizontalBias - value) > double.Epsilon)
                {
                    _horizontalBias = value;
                    InvokePropertyChange(nameof(HorizontalBias));
                }
            }
        }

        private double _smoothingFactor = 30.0;
        public double SmoothingFactor
        {
            get { return _smoothingFactor; }
            set
            {
                if (Math.Abs(_smoothingFactor - value) > double.Epsilon)
                {
                    _smoothingFactor = value;
                    InvokePropertyChange(nameof(SmoothingFactor));
                }
            }
        }

        private double _engineStrength = 0.0;
        public double EngineStrength
        {
            get { return _engineStrength; }
            set
            {
                if (Math.Abs(_engineStrength - value) > double.Epsilon)
                {
                    _engineStrength = value;
                    InvokePropertyChange(nameof(EngineStrength));
                }
            }
        }

        private double _upshiftingStrength = 0.0;
        public double UpshiftingStrength
        {
            get { return _upshiftingStrength; }
            set
            {
                if (Math.Abs(_upshiftingStrength - value) > double.Epsilon)
                {
                    _upshiftingStrength = value;
                    InvokePropertyChange(nameof(UpshiftingStrength));
                }
            }
        }

        private string _upshiftingModifiers = null;
        public string UpshiftingModifiers
        {
            get {
                _upshiftingModifiers = _upshiftingModifiers ?? DefaultUpshiftingModifiers;

                return _upshiftingModifiers;
            }
            set
            {
                if (_upshiftingModifiers != value)
                {
                    _upshiftingModifiers = value;
                    InvokePropertyChange(nameof(UpshiftingModifiers));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AreUpshiftingModifiersValid)));
                }
            }
        }

        [JsonIgnore]
        public bool AreUpshiftingModifiersValid => DevicePlugin.ValidateUpshiftingModifiers(_upshiftingModifiers);

        private bool _showSurgePlot = true;
        public bool ShowSurgePlot
        {
            get { return _showSurgePlot; }
            set
            {
                if (_showSurgePlot != value)
                {
                    _showSurgePlot = value;
                    InvokePropertyChange(nameof(ShowSurgePlot));
                }
            }
        }

        private bool _showSwayPlot = true;
        public bool ShowSwayPlot
        {
            get { return _showSwayPlot; }
            set
            {
                if (_showSwayPlot != value)
                {
                    _showSwayPlot = value;
                    InvokePropertyChange(nameof(ShowSwayPlot));
                }
            }
        }

        private bool _showHeavePlot = true;
        public bool ShowHeavePlot
        {
            get { return _showHeavePlot; }
            set
            {
                if (_showHeavePlot != value)
                {
                    _showHeavePlot = value;
                    InvokePropertyChange(nameof(ShowHeavePlot));
                }
            }
        }

        private bool _showTorquePlot = true;
        public bool ShowTorquePlot
        {
            get { return _showTorquePlot; }
            set
            {
                if (_showTorquePlot != value)
                {
                    _showTorquePlot = value;
                    InvokePropertyChange(nameof(ShowTorquePlot));
                }
            }
        }

        private double _telemetryGraphHeight = 500.0;
        public double TelemetryGraphHeight
        {
            get { return _telemetryGraphHeight; }
            set
            {
                double clampedValue = Math.Max(200.0, Math.Min(1200.0, value));

                if (_telemetryGraphHeight != clampedValue)
                {
                    _telemetryGraphHeight = clampedValue;
                    InvokePropertyChange(nameof(TelemetryGraphHeight));
                }
            }
        }

        public string ActiveProfileKey
        {
            get
            {
                GameTuningProfile active = GetActiveProfile();
                return active != null ? active.GetKey() : null;
            }
        }

        public ObservableCollection<MotorConfiguration> MotorConfigurations { get; set; } = new ObservableCollection<MotorConfiguration>();

        public ObservableCollection<GameTuningProfile> Profiles { get; set; } = new ObservableCollection<GameTuningProfile>();

        /// <summary>Adds the given <see cref="GameTuningProfile" /> instance to our collection of profiles</summary>
        public void AddProfile(GameTuningProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            if (profile.Game == String.Empty)
            {
                return;
            }

            lock (_profilesLock)
            {
                Profiles.Add(profile);
            }

            ChangeActiveProfile(profile);
        }

        /// <summary>Removes the given <see cref="GameTuningProfile" /> instance from our collection of profiles</summary>
        public void RemoveProfile(GameTuningProfile profile)
        {
            lock (_profilesLock)
            {
                Profiles.Remove(profile);
            }

            ChangeActiveProfile();
        }

        /// <summary>Creates a <see cref="GameTuningProfile" /> instance for the given game and vehicle</summary>
        public GameTuningProfile CreateProfile(string game, string vehicle)
        {
            return GameTuningProfile.Make(this, game, vehicle);
        }

        /// <summary>Clones the given <see cref="GameTuningProfile" /> instance, overwriting its game and vehicle with those given</summary>
        public GameTuningProfile CloneProfile(GameTuningProfile profile, string game, string vehicle)
        {
            return profile == null ? null : profile.Clone(game, vehicle);
        }

        /// <summary>Applies the given <see cref="GameTuningProfile" /> instance properties to the current settings properties and marks it as active</summary>
        public void LoadProfile(GameTuningProfile profile)
        {
            Logging.Current.Info($"SABT: Loading profile '{profile.GetKey()}'...");

            for (int i = 0; i < Profiles.Count; i++)
            {
                Profiles[i].IsActive = false;
            }

            Persist?.Invoke();

            MinimumSurge = profile.MinimumSurge;
            MaximumSurge = profile.MaximumSurge;
            MinimumSway = profile.MinimumSway;
            MaximumSway = profile.MaximumSway;
            MinimumHeave = profile.MinimumHeave;
            MaximumHeave = profile.MaximumHeave;
            SmoothingFactor = profile.SmoothingFactor;
            EngineStrength = profile.EngineStrength;
            UpshiftingStrength = profile.UpshiftingStrength;
            UpshiftingModifiers = profile.UpshiftingModifiers ?? DefaultUpshiftingModifiers;

            profile.IsActive = true;

            InvokePropertyChange(nameof(ActiveProfileKey));
        }

        /// <summary>Returns the <see cref="GameTuningProfile" /> instance that is currently marked as active</summary>
        public GameTuningProfile GetActiveProfile()
        {
            lock (_profilesLock)
            {
                for (int i = Profiles.Count - 1; i >= 0; i--)
                {
                    if (Profiles[i].IsActive)
                    {
                        return Profiles[i];
                    }
                }
            }

            return null;
        }

        /// <summary>Returns the <see cref="GameTuningProfile" /> that matches the given game and vehicle; optionally returning the default if none are found</summary>
        public GameTuningProfile FindProfile(string game, string vehicle, bool useDefault = false)
        {
            lock (_profilesLock)
            {
                for (int i = Profiles.Count - 1; i >= 0; i--)
                {
                    if (Profiles[i].Matches(game, vehicle))
                    {
                        return Profiles[i];
                    }
                }

                if (useDefault && Profiles.Count > 0)
                {
                    for (int i = Profiles.Count - 1; i >= 0; i--)
                    {
                        if (Profiles[i].Matches(string.Empty, string.Empty))
                        {
                            return Profiles[i];
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>Refreshes the active profile, loading whichever matches the current (or given) game and vehicle</summary>
        public GameTuningProfile ChangeActiveProfile(GameTuningProfile profile = null)
        {
            if (!_isInitialised || _plugin == null)
            {
                return null;
            }

            lock (_profilesLock)
            {
                CleanProfiles();

                // Use Given Profile
                if (profile != null)
                {
                    for (int i = 0; i < Profiles.Count; i++)
                    {
                        if (Profiles[i] == profile)
                        {
                            LoadProfile(profile);

                            return profile;
                        }
                    }
                }

                // Use Game & Vehicle Profile
                profile = FindProfile(_plugin.CurrentGame, _plugin.CurrentVehicle);

                if (profile is GameTuningProfile)
                {
                    LoadProfile(profile);

                    return profile;
                }

                // Use Game Profile (Or Default)
                profile = FindProfile(_plugin.CurrentGame, string.Empty, true);

                if (profile is GameTuningProfile)
                {
                    LoadProfile(profile);

                    return profile;
                }

                // Create Default Profile
                profile = CreateProfile(string.Empty, string.Empty);

                Profiles.Insert(0, profile);

                return profile;
            }
        }

        /// <summary>Sorts and deduplicates the profiles collection</summary>
        private void CleanProfiles()
        {
            // Identify & Remove Duplicate Profiles
            HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);

            for (int i = Profiles.Count - 1; i >= 0; i--)
            {
                if (!keys.Add(Profiles[i].GetKey()))
                {
                    Profiles.RemoveAt(i);
                }
            }

            // Sort By Game Label, Then Vehicle Label
            var sorted = Profiles
                .OrderBy(p => !p.IsDefault)
                .ThenBy(p => p.GameLabel, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.VehicleLabel, StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                int p = Profiles.IndexOf(sorted[i]);
                if (p != i)
                {
                    Profiles.Move(p, i);
                }
            }
        }
    }

    /// <summary>A representation of the tuning parameters making up a game (and optionnaly vehicle) profile</summary>
    public class GameTuningProfile : INotifyPropertyChanged
    {
        private const string _wildcardSymbol = "✱";

        public event PropertyChangedEventHandler PropertyChanged;

        private void InvokePropertyChange([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool IsDefault { get; }

        public bool IsNotDefault {
            get { return !IsDefault; }
        }
        
        public string Game { get; }
        public string GameLabel { get; set; }
        
        public string Vehicle { get; }
        public string VehicleLabel { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    InvokePropertyChange(nameof(IsActive));
                }
            }
        }

        public double MinimumSurge { get; set; }
        public double MaximumSurge { get; set; }
        public double MinimumSway { get; set; }
        public double MaximumSway { get; set; }
        public double MinimumHeave { get; set; }
        public double MaximumHeave { get; set; }
        public double SmoothingFactor { get; set; }
        public double EngineStrength { get; set; }
        public double UpshiftingStrength { get; set; }
        public string UpshiftingModifiers { get; set; }

        public GameTuningProfile(string game, string vehicle, bool promptForLabels = false)
        {
            Game = game;
            Vehicle = vehicle;
            IsDefault = (game == string.Empty && vehicle == string.Empty);

            if (promptForLabels)
            {
                GameLabel = (game == string.Empty) ? _wildcardSymbol : Interaction.InputBox(
                    Prompt: SLoc.GetValue("SABT_Message_AlterGameName"),
                    Title: SLoc.GetValue("SABT_Title_AlterGameName"),
                    DefaultResponse: PrettifyLabelPart(game)
                );

                if (string.IsNullOrEmpty(GameLabel))
                {
                    GameLabel = PrettifyLabelPart(game);
                }

                VehicleLabel = (vehicle == string.Empty) ? _wildcardSymbol : Interaction.InputBox(
                    Prompt: SLoc.GetValue("SABT_Message_AlterVehicleName"),
                    Title: SLoc.GetValue("SABT_Title_AlterVehicleName"),
                    DefaultResponse: PrettifyLabelPart(vehicle)
                );

                if (string.IsNullOrEmpty(VehicleLabel))
                {
                    VehicleLabel = PrettifyLabelPart(vehicle);
                }
            }
        }

        /// <summary>Returns a new <see cref="GameTuningProfile" /> instance for the given game and vehicle, applying the current global settings as its base values</summary>
        public static GameTuningProfile Make(DeviceSettings settings, string game, string vehicle)
        {
            return new GameTuningProfile(game, vehicle, true)
            {
                IsActive = false,

                MinimumSurge = settings.MinimumSurge,
                MaximumSurge = settings.MaximumSurge,
                MinimumSway = settings.MinimumSway,
                MaximumSway = settings.MaximumSway,
                MinimumHeave = settings.MinimumHeave,
                MaximumHeave = settings.MaximumHeave,
                SmoothingFactor = settings.SmoothingFactor,
                EngineStrength = settings.EngineStrength,
                UpshiftingStrength = settings.UpshiftingStrength,
                UpshiftingModifiers = settings.UpshiftingModifiers
            };
        }

        /// <summary>Returns a new <see cref="GameTuningProfile" /> instance for the given game and vehicle, applying the current instance properties as its base values</summary>
        public GameTuningProfile Clone(string game, string vehicle)
        {
            return new GameTuningProfile(game, vehicle, true)
            {
                IsActive = false,

                MinimumSurge = this.MinimumSurge,
                MaximumSurge = this.MaximumSurge,
                MinimumSway = this.MinimumSway,
                MaximumSway = this.MaximumSway,
                MinimumHeave = this.MinimumHeave,
                MaximumHeave = this.MaximumHeave,
                SmoothingFactor = this.SmoothingFactor,
                EngineStrength = this.EngineStrength,
                UpshiftingStrength = this.UpshiftingStrength,
                UpshiftingModifiers = this.UpshiftingModifiers
            };
        }

        /// <summary>Indicates if the given game and vehicle match the current instance</summary>
        public bool Matches(string game, string vehicle)
        {
            return (
                string.Equals(SimplifyKeyPart(Game), SimplifyKeyPart(game), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(SimplifyKeyPart(Vehicle), SimplifyKeyPart(vehicle), StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <summary>Returns a string key representing this instance's game and vehicle associations</summary>
        public string GetKey()
        {
            return $"{SimplifyKeyPart(Game)}|{SimplifyKeyPart(Vehicle)}";
        }

        /// <summary>Sets the property matching the given name to the given value, if it exists and is writable</summary>
        public void SetTuningProperty(string name, object value)
        {
            var property = this.GetType().GetProperty(name);

            if (property != null && property.CanWrite)
            {
                property.SetValue(this, value);
            }
        }

        /// <summary>Returns a cleaned-up version of the given (highly variable) game or vehicle name string reported by SimHub</summary>
        private static string PrettifyLabelPart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return _wildcardSymbol;
            }

            value = value.Trim();

            StringBuilder words = new StringBuilder(value.Length * 2);

            bool hasPreviousAlphaNumeric = false;
            bool previousWasLower = false;
            bool previousWasUpper = false;
            bool previousWasDigit = false;

            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];

                if (!char.IsLetterOrDigit(current))
                {
                    if (words.Length > 0 && words[words.Length - 1] != ' ')
                    {
                        words.Append(' ');
                    }

                    hasPreviousAlphaNumeric = false;
                    previousWasLower = false;
                    previousWasUpper = false;
                    previousWasDigit = false;
                    continue;
                }

                bool currentIsLetter = char.IsLetter(current);
                bool currentIsLower = currentIsLetter && char.IsLower(current);
                bool currentIsUpper = currentIsLetter && char.IsUpper(current);
                bool currentIsDigit = char.IsDigit(current);

                if (hasPreviousAlphaNumeric)
                {
                    bool splitBeforeCurrent = (
                        (previousWasLower && currentIsUpper) ||
                        (previousWasDigit && !currentIsDigit) ||
                        (!previousWasDigit && currentIsDigit)
                    );

                    if (splitBeforeCurrent && words.Length > 0 && words[words.Length - 1] != ' ')
                    {
                        words.Append(' ');
                    }
                }

                words.Append(current);

                hasPreviousAlphaNumeric = true;
                previousWasLower = currentIsLower;
                previousWasUpper = currentIsUpper;
                previousWasDigit = currentIsDigit;
            }

            string separated = words.ToString().Trim();

            if (string.IsNullOrWhiteSpace(separated))
            {
                return "*";
            }

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                separated.ToLowerInvariant()
            );
        }

        /// <summary>Returns a simplified version of the given (highly variable) game or vehicle name string reported by SimHub</summary>
        private static string SimplifyKeyPart(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "*";
            }

            value = value.Trim();

            StringBuilder simplified = new StringBuilder(value.Length);

            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];

                if (char.IsLetterOrDigit(current))
                {
                    simplified.Append(char.ToLowerInvariant(current));
                }
            }

            return simplified.ToString();
        }
    }

    /// <summary>A wrapper for the <see cref="Motor" /> configuration, allowing it to be stored and restored as part of the plugin settings</summary>
    public class MotorConfiguration : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void InvokePropertyChange([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public byte Identifier { get; set; }

        private byte _mapping = Motor.MotorMapping.Unused.Key;
        public byte Mapping
        {
            get { return _mapping; }
            set
            {
                if (_mapping != value)
                {
                    _mapping = value;
                    InvokePropertyChange(nameof(Mapping));
                }
            }
        }

        private byte _direction = Motor.MotorDirection.Clockwise.Key;
        public byte Direction
        {
            get { return _direction; }
            set
            {
                if (_direction != value)
                {
                    _direction = value;
                    InvokePropertyChange(nameof(Direction));
                }
            }
        }
    }
}
