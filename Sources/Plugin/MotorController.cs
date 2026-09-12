using SimHub;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using WoteverCommon.Extensions;
using WoteverLocalization;
using static User.ActiveBeltTensioner.MotorController;

namespace User.ActiveBeltTensioner
{
    /// <summary>A representation of the motor control system, which is technically one serial port shared by multiple <see cref="Motor" /> objects</summary>
    public class MotorController : INotifyPropertyChanged, IDisposable
    {
        public static class MotorGraphic
        {
            public const string Disconnected = "/User.ActiveBeltTensioner;component/Motor, Disconnected.png";
            public const string Connect = "/User.ActiveBeltTensioner;component/Motor, Connect.png";
            public const string Communicating = "/User.ActiveBeltTensioner;component/Motor, Communicating.png";
            public const string Connected = "/User.ActiveBeltTensioner;component/Motor, Connected.png";
            public const string Error = "/User.ActiveBeltTensioner;component/Motor, Error.png";
            public const string Overheating = "/User.ActiveBeltTensioner;component/Motor, Overheating.png";
            public const string Overheated = "/User.ActiveBeltTensioner;component/Motor, Overheated.png";
        }

        public struct MotorMapping
        {
            public string Label { get; }
            public string Graphic { get; }
            public MotorMapping(string label, string graphic)
            {
                Label = label;
                Graphic = graphic;
            }

            public static MotorMapping Unused = new MotorMapping("Unused", "/User.ActiveBeltTensioner;component/Mapping.Unused.png");
            public static MotorMapping LeftShoulder = new MotorMapping("Left Shoulder", "/User.ActiveBeltTensioner;component/Mapping.LeftShoulder.png");
            public static MotorMapping RightShoulder = new MotorMapping("Right Shoulder", "/User.ActiveBeltTensioner;component/Mapping.RightShoulder.png");
            public static MotorMapping LeftWaist = new MotorMapping("Left Waist", "/User.ActiveBeltTensioner;component/Mapping.LeftWaist.png");
            public static MotorMapping RightWaist = new MotorMapping("Right Waist", "/User.ActiveBeltTensioner;component/Mapping.RightWaist.png");

            public static MotorMapping[] Mappings = {
                Unused,
                LeftShoulder,
                RightShoulder,
                LeftWaist,
                RightWaist,
            };
        }

        public struct MotorDirection
        {
            public string Label { get; }
            public sbyte Multiplier { get; }
            public string Graphic { get; }
            public MotorDirection(string label, sbyte multiplier, string graphic)
            {
                Label = label;
                Multiplier = multiplier;
                Graphic = graphic;
            }

            public static MotorDirection Clockwise = new MotorDirection("Clockwise", 1, "/User.ActiveBeltTensioner;component/Direction.Clockwise.png");
            public static MotorDirection AntiClockwise = new MotorDirection("Anti-Clockwise", -1, "/User.ActiveBeltTensioner;component/Direction.AntiClockwise.png");

            public static MotorDirection[] Directions = {
                Clockwise,
                AntiClockwise,
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InvokePropertyChange([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>A representation of a single motor, which receives and responds to commands via the shared serial port of the parent <see cref="MotorController" /></summary>
        public class Motor : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private void InvokePropertyChange([CallerMemberName] string name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

            public byte Identifier { get; set; } = 0;

            private MotorMapping _mapping;
            public MotorMapping Mapping
            {
                get { return _mapping; }
                set
                {
                    if (_mapping.Label != value.Label)
                    {
                        _mapping = value;
                        InvokePropertyChange();
                    }
                }
            }

            private MotorDirection _direction = MotorDirection.Clockwise;
            public MotorDirection Direction
            {
                get { return _direction; }
                set
                {
                    if (_direction.Multiplier != value.Multiplier)
                    {
                        _direction = value;
                        InvokePropertyChange();
                    }
                }
            }

            private bool _isConnected = false;
            public bool IsConnected
            {
                get { return _isConnected; }
                set
                {
                    if (_isConnected != value)
                    {
                        _isConnected = value;
                        InvokePropertyChange();
                    }
                }
            }

            private string _status = SLoc.GetValue("SABT_Status_Disconnected");
            public string Status
            {
                get { return _status; }
                set
                {
                    if (_status != value)
                    {
                        _status = value;
                        InvokePropertyChange();
                    }
                }
            }

            private string _graphic = MotorGraphic.Disconnected;
            public string Graphic
            {
                get { return _graphic; }
                set
                {
                    if (_graphic != value)
                    {
                        _graphic = value;
                        InvokePropertyChange();
                    }
                }
            }

            private byte _angle = 0;
            public byte Angle
            {
                get { return _angle; }
                set
                {
                    if (_angle != value)
                    {
                        _angle = value;
                        InvokePropertyChange();
                    }
                }
            }

            private byte _temperature = 0;
            public byte Temperature
            {
                get { return _temperature; }
                set
                {
                    if (_temperature != value)
                    {
                        _temperature = value;
                        InvokePropertyChange();
                    }

                    if (value > 0)
                    {
                        if (LowestTemperature == null || value < LowestTemperature)
                        {
                            LowestTemperature = value;
                        }

                        if (HighestTemperature == null || value > HighestTemperature)
                        {
                            HighestTemperature = value;
                        }
                    }
                }
            }

            private byte? _lowestTemperature = null;
            public byte? LowestTemperature
            {
                get { return _lowestTemperature; }
                private set
                {
                    if (_lowestTemperature != value)
                    {
                        _lowestTemperature = value;
                        InvokePropertyChange();
                    }
                }
            }

            private byte? _highestTemperature = null;
            public byte? HighestTemperature
            {
                get { return _highestTemperature; }
                private set
                {
                    if (_highestTemperature != value)
                    {
                        _highestTemperature = value;
                        InvokePropertyChange();
                    }
                }
            }

            private int _faults = 0;
            public int Faults
            {
                get { return _faults; }
                private set
                {
                    if (_faults != value)
                    {
                        _faults = value;
                        InvokePropertyChange();
                    }
                }
            }

            private byte _error = 0;
            public byte Error
            {
                get { return _error; }
                set
                {
                    if (_error != value)
                    {
                        _error = value;
                        InvokePropertyChange();
                    }
                }
            }

            private const short _maximumConsecutiveFaults = 100;
            private const byte _torqueMode = 0x01;
            private const short _torqueLimit = 12000;
            private MotorController _controller;

            private int _commandFailures = 0;
            private double _smoothedTorque = 0.0;

            public ICommand TriggerTest { get; }
            public ICommand TriggerAssign { get; }

            public Motor(MotorController controller, byte identifier)
            {
                _controller = controller;

                Identifier = identifier;

                TriggerTest = new RelayCommand(
                    execute: _ => Test()
                );

                TriggerAssign = new RelayCommand(
                    execute: _ => Assign()
                );
            }

            /// <summary>Resets the session diagnostic data for the motor</summary>
            public void ResetDiagnostics()
            {
                LowestTemperature = null;
                HighestTemperature = null;
                Faults = 0;
            }

            /// <summary>Assigns the identifier to the motor via a series of guided prompts</summary>
            /// <returns>Whether the process succeeded</returns>
            public bool Assign()
            {
                Status = SLoc.GetValue("SABT_Status_AwaitingConnection");
                Graphic = MotorGraphic.Connect;

                if (
                    MessageBox.Show(
                        SLoc.GetValue("SABT_Message_PlugInMotor"),
                        SLoc.GetValue("SABT_Plugin"),
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Information
                    ) == MessageBoxResult.Yes
                )
                {
                    if (!SetIdentifier())
                    {
                        MessageBox.Show(
                            SLoc.GetValue("SABT_Message_FailedToAssignMotor"),
                            SLoc.GetValue("SABT_Plugin"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );

                        return false;
                    }

                    MessageBox.Show(
                        SLoc.GetValue("SABT_Message_AssignedMotor"),
                        SLoc.GetValue("SABT_Plugin"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    return true;
                }

                Status = SLoc.GetValue("SABT_Status_Disconnected");
                Graphic = MotorGraphic.Disconnected;

                return false;
            }

            /// <summary>Invokes various methods to ascertain the status of the motor, while updating its status indicators</summary>
            /// <returns>Whether the motor responded as expected</returns>
            public bool Check()
            {
                IsConnected = false;
                Status = SLoc.GetValue("SABT_Status_Connecting");
                Graphic = MotorGraphic.Disconnected;

                _smoothedTorque = 0;

                if (!_controller.HasSerial)
                {
                    Status = SLoc.GetValue("SABT_Status_NoDeviceDetected");
                    Temperature = 0;
                    Error = 0;

                    return false;
                }

                if (Query(false))
                {
                    Status = SLoc.GetValue("SABT_Status_CheckingMode");
                    Graphic = MotorGraphic.Communicating;

                    if (Query(true))
                    {
                        IsConnected = true;
                        Status = SLoc.GetValue("SABT_Status_Connected");
                        Graphic = MotorGraphic.Connected;

                        return true;
                    }

                    Status = SLoc.GetValue("SABT_Status_SettingMode");
                    Graphic = MotorGraphic.Communicating;

                    if (SetMode(_torqueMode))
                    {
                        IsConnected = true;
                        Status = SLoc.GetValue("SABT_Status_Connected");
                        Graphic = MotorGraphic.Connected;

                        return true;
                    }
                }

                IsConnected = false;
                Status = SLoc.GetValue("SABT_Status_CommunicationFailure");
                Graphic = MotorGraphic.Error;
                Temperature = 0;
                Error = 0;

                return false;
            }

            /// <summary>Sends a stop (zero torque) command to the motor until a response is received or limited attempts run out, while updating its status indicators</summary>
            /// <returns>Whether the motor responded as expected</returns>
            public bool Stop()
            {
                IsConnected = false;
                Status = SLoc.GetValue("SABT_Status_Stopping");
                Graphic = MotorGraphic.Communicating;

                _smoothedTorque = 0;

                byte[] tx = BuildFrame(Identifier, 0x64, 0x00, 0x00);
                byte[] rx = new byte[10];

                for (int i = 0; i < 5; i++)
                {
                    if (_controller.WriteFrameReadFrame(tx, rx))
                    {
                        Status = SLoc.GetValue("SABT_Status_Disconnected");
                        Graphic = MotorGraphic.Disconnected;
                        Temperature = 0;
                        Error = 0;

                        return true;
                    }
                }

                Status = SLoc.GetValue("SABT_Status_CommunicationFailure");
                Graphic = MotorGraphic.Error;

                Temperature = 0;
                Error = 0;

                return false;
            }

            /// <summary>Sends a status request command to the motor and checks the response (if any) for validity</summary>
            /// <returns>Whether the motor responded as expected</returns>
            public bool Query(bool isInTorqueMode = true)
            {
                byte[] tx = BuildFrame(Identifier, 0x74);
                byte[] rx = new byte[10];

                if (_controller.WriteFrameReadFrame(tx, rx, 300, true))
                {
                    if (rx[0] != Identifier) { return false; }
                    if (isInTorqueMode && rx[1] != _torqueMode) { return false; }
                    if (rx[6] >= 90) { return false; } // Temperature Out Of Range
                    if (rx[8] != 0x00) { return false; } // Error Code Present

                    Temperature = rx[6];
                    Angle = rx[7];
                    Error = rx[8];

                    return true;
                }

                Temperature = 0;
                Angle = 0;
                Error = 0;

                return false;
            }

            /// <summary>Sends a series of torque commands to the motor to oscillate it, while updating its status indicators</summary>
            /// <returns>Whether the motor responded as expected</returns>
            public bool Test(int times = 100, double testTorque = 0.6)
            {
                SLoc.GetValue("SABT_Status_Testing");
                Graphic = MotorGraphic.Communicating;

                if (!Query(true))
                {
                    IsConnected = false;
                    Status = SLoc.GetValue("SABT_Status_TestFailed");
                    Graphic = MotorGraphic.Error;

                    return false;
                }

                int direction = Direction.Multiplier;
                int good = 0;
                int bad = 0;

                short torque = 0;

                for (int i = 0; i < times; i++)
                {
                    byte highByte = (byte)((torque >> 8) & 0xFF);
                    byte lowByte = (byte)(torque & 0xFF);

                    byte[] tx = BuildFrame(Identifier, 0x64, highByte, lowByte);
                    byte[] rx = new byte[10];

                    if (_controller.WriteFrameReadFrame(tx, rx, 3, true, true))
                    {
                        good++;
                    }
                    else
                    {
                        bad++;
                    }

                    Thread.Sleep(TimeSpan.FromMilliseconds(20));

                    torque = (short)(testTorque * direction * _torqueLimit);

                    direction *= -1;
                }

                if (bad > times * 0.1)
                {
                    if (good < 1)
                    {
                        IsConnected = false;
                        Status = SLoc.GetValue("SABT_Status_TestFailed");
                        Graphic = MotorGraphic.Error;

                        return false;

                    }

                    IsConnected = true;
                    Status = SLoc.GetValue("SABT_Status_TestPartiallyFailed") + " (" + bad + "/" + times + ")";
                    Graphic = MotorGraphic.Connected;

                    return true;
                }

                IsConnected = true;
                Status = SLoc.GetValue("SABT_Status_TestPassed");
                Graphic = MotorGraphic.Connected;

                return true;
            }

            /// <summary>Sends a series of identifier allocation commands to the motor</summary>
            /// <remarks>The motor firmware requires 5 repeated commands of this type to actually change the value; and it can only be changed once per power cycle</remarks>
            /// <returns>Whether the motor responded as expected</returns>
            public bool SetIdentifier()
            {
                Status = SLoc.GetValue("SABT_Status_SettingIdentifier");
                Graphic = MotorGraphic.Communicating;

                byte[] tx = BuildFrame(0xAA, 0x55, 0x53, Identifier, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);
                byte[] rx = new byte[10];

                for (int i = 0; i < 5; i++)
                {
                    _controller.FlushSerialBuffer();
                    _controller.WriteFrameReadFrame(tx, rx, 100, false, true);
                }

                Thread.Sleep(500);

                if (Query(false))
                {
                    if (SetMode(_torqueMode))
                    {
                        Status = SLoc.GetValue("SABT_Status_IdentifierSet");
                        Graphic = MotorGraphic.Connected;

                        return true;
                    }
                }

                Status = SLoc.GetValue("SABT_Status_CommunicationFailure");
                Graphic = MotorGraphic.Error;

                return false;
            }

            /// <summary>Sends a mode change command with the given mode byte motor (<see langword="0x01" />: torque, <see langword="0x02" />: velocity, <see langword="0x03" />: position)</summary>
            /// <returns>Whether the motor responded as expected</returns>
            public bool SetMode(byte mode)
            {
                byte[] tx = BuildFrame(Identifier, 0xA0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, mode);
                byte[] rx = new byte[10];

                _controller.WriteFrameReadFrame(tx, rx, 200, false);
                
                return Query(true);
            }

            /// <summary>Sends the given torque value (as a fraction of maximum torque) to the motor; optionally subject to a smoothing factor</summary>
            /// <returns>Whether the motor responded as expected</returns>
            public bool SetTorque(double torque, double smoothingFactor = 0.0)
            {
                _smoothedTorque = (torque * (1.0 - smoothingFactor)) + (_smoothedTorque * smoothingFactor);

                torque = _smoothedTorque;

                short newTorque = ClampValue(
                    (short)(torque * _torqueLimit * -1.0),
                    (short)_torqueLimit * -1,
                    (short)_torqueLimit
                );

                byte highByte = (byte)((newTorque >> 8) & 0xFF);
                byte lowByte = (byte)(newTorque & 0xFF);

                byte[] tx = BuildFrame(Identifier, 0x64, highByte, lowByte);
                byte[] rx = new byte[10];

                if (!_controller.WriteFrameReadFrame(tx, rx, 5))
                {
                    _commandFailures++;

                    if (_commandFailures > Faults)
                    {
                        Faults = _commandFailures;
                    }

                    if (_commandFailures > 1)
                    {
                        Logging.Current.Warn("SABT: #" + this.Identifier + " motor communication failure (" + _commandFailures + "/" + _maximumConsecutiveFaults + " allowed)");
                    }

                    return (_commandFailures < _maximumConsecutiveFaults);
                }

                _commandFailures = 0;

                return true;
            }
        }

        public Motor[] Motors { get; private set; }
        public bool IsBusy {
            get { lock (_actionLock) { return _actionsIdentifiers.Count > 0; } }
        }
        public bool HasSerial
        {
            get { return (_serialPort != null); }
        }

        private string _warningGraphic;
        public string WarningGraphic
        {
            get { return _warningGraphic; }
            private set
            {
                if (_warningGraphic != value)
                {
                    _warningGraphic = value;
                    InvokePropertyChange(nameof(WarningGraphic));
                }
            }
        }

        private string[] _serialPorts = new string[0];
        public string[] SerialPorts
        {
            get { return _serialPorts; }
            private set
            {
                if (!ReferenceEquals(_serialPorts, value))
                {
                    _serialPorts = value ?? new string[0];
                    InvokePropertyChange(nameof(SerialPorts));
                }
            }
        }

        private readonly DevicePlugin _plugin;
        private readonly List<string> _actionsIdentifiers = new List<string>();
        private SerialPort _serialPort;
        private long _actionsCounter = 0;
        private readonly object _actionLock = new object();
        private readonly object _serialLock = new object();
        private bool _hasNotifiedOfLicense = false;

        private byte _motorCommandIdentifier = 1;

        private readonly long _motorCommandTicks;
        private long _lastCommandTicks = 0;
        private readonly long _motorQueryTicks;
        private long _lastQueryTicks = 0;

        public ICommand TriggerConnect { get; }

        public MotorController(DevicePlugin plugin)
        {
            _plugin = plugin;

            Motors = new Motor[] {
                new Motor(this, 1),
                new Motor(this, 2),
                new Motor(this, 3),
                new Motor(this, 4),
            };

            foreach (Motor motor in Motors)
            {
                motor.PropertyChanged += MotorPropertyChanged;
            }

            _motorCommandTicks = (long)(3.0 * System.Diagnostics.Stopwatch.Frequency / 1000.0);
            _motorQueryTicks = (long)(3000.0 * System.Diagnostics.Stopwatch.Frequency / 1000.0); // 3s

            TriggerConnect = new RelayCommand(
                execute: _ => Connect()
            );
        }

        private void MotorPropertyChanged(object origin, PropertyChangedEventArgs e)
        {
            Motor motor = origin as Motor;

            if (motor == null) return;

            //InvokePropertyChange($"{motor.Label}Motor{e.PropertyName}");
            //InvokePropertyChange(nameof(BothMotorsAreConnected));
            //InvokePropertyChange(nameof(OneMotorIsConnected));
        }

        /// <summary>Resets the session diagnostic data for every connected motor</summary>
        public void ResetDiagnostics()
        {
            foreach (Motor motor in Motors)
            {
                motor.ResetDiagnostics();
            }
        }

        /// <summary>Opens the selected serial port; checking motor communication automatically if enabled</summary>
        /// <returns>Whether the serial port was successfully opened</returns>
        public bool Connect()
        {
            StartAction(out string action);

            bool didConnect = false;

            lock (_serialLock)
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    if (_plugin.IsEnabled)
                    {
                        Check();
                    }

                    EndAction(action);

                    return true;
                }

                if (!_plugin.Settings.IsSerialPortValid)
                {
                    Logging.Current.Warn("SABT: Invalid serial port selection");

                    EndAction(action);

                    return false;
                }

                try
                {
                    _serialPort?.Dispose();
                    _serialPort = new SerialPort(
                        portName: _plugin.Settings.SerialPort,
                        baudRate: 115200
                    )
                    {
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        ReadTimeout = 10,
                        WriteTimeout = 100,
                        DtrEnable = false,
                        RtsEnable = false,
                        NewLine = "\n"
                    };

                    int retry = 0;
                    const int retries = 10;

                    while (retry < retries)
                    {
                        try
                        {
                            _serialPort.Open();

                            didConnect = true;

                            break;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            _serialPort.Close();

                            retry++;

                            Logging.Current.Warn("SABT: Serial port opening failure (" + retry + "/" + retries + " retries)");

                            Thread.Sleep(100);
                        }
                    }

                    if (!didConnect)
                    {
                        _serialPort?.Dispose();
                        _serialPort = null;
                    }
                }
                catch (Exception exception)
                {
                    Logging.Current.Warn($"SABT: Unexpected serial communication error: {exception.Message}");

                    MessageBox.Show(
                        exception.Message,
                        SLoc.GetValue("SABT_Plugin"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }

            EndAction(action);

            if (didConnect && _plugin.IsEnabled)
            {
                Check();
            }

            return didConnect;
        }

        /// <summary>Invokes the <see cref="Motor.Check()" /> method on each motor</summary>
        /// <returns>Whether all motors were successfully connected (or marked as unused)</returns>
        public bool Check()
        {
            if (!_plugin.PluginManager.IsSimHubLicenceValid && !_hasNotifiedOfLicense)
            {
                _hasNotifiedOfLicense = true;

                MessageBox.Show(
                    SLoc.GetValue("SABT_Message_SimHubLicenseRequired"),
                    SLoc.GetValue("SABT_Plugin"),
                    MessageBoxButton.OK
                );
            }

            StartAction(out string action);

            bool didConnect = true;

            foreach (Motor motor in Motors)
            {
                didConnect = (motor.Check() || motor.Mapping.Equals(MotorMapping.Unused)) && didConnect;
            }

            EndAction(action);

            return didConnect;
        }

        /// <summary>Invokes the <see cref="Motor.Query()" /> method on each motor</summary>
        /// <returns>Whether all motors responded as expected (or were marked as unused)</returns>
        public bool Query(bool isInTorqueMode = true)
        {
            StartAction(out string action);

            bool didRespond = true;

            foreach (Motor motor in Motors)
            {
                didRespond = (motor.Query(isInTorqueMode) || motor.Mapping.Equals(MotorMapping.Unused)) && didRespond;
            }

            EndAction(action);

            return didRespond;
        }

        /// <summary>Invokes the <see cref="Motor.Stop()" /> method on each motor then closes the serial port</summary>
        public void Disconnect()
        {
            StartAction(out string action);

            lock (_serialLock)
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    foreach (Motor motor in Motors)
                    {
                        motor.Stop();
                    }

                    try
                    {
                        _serialPort.Close();
                        _serialPort.Dispose();
                    }
                    catch
                    {
                        Logging.Current.Warn("SABT: Serial port release failure");
                    }
                }
            }

            EndAction(action);
        }

        /// <summary>An alias of <see cref="Disconnect()" /> for the purposes of fulfilling the <see cref="IDisposable" /> interface</summary>
        public void Dispose()
        {
            Disconnect();
        }

        /// <summary>Sends the given torque values (as fractions of maximum torque) to the motors, cycling through motors to reduce bus conflicts</summary>
        /// <returns>Whether the motor commands were sent successfully (if applicable)</returns>
        /// <remarks>Checks the motor temperatures and reduces output if thresholds are exceeded</remarks>
        public bool SetTorques(
            double shoulderLeft,
            double shoulderRight,
            double waistLeft,
            double waistRight,
            double smoothingFactor = 0.0
        )
        {
            StartAction(out string action);

            if (_serialPort == null || !_serialPort.IsOpen)
            {
                EndAction(action);

                return false;
            }

            bool didSet = true;
            long currentTicks = System.Diagnostics.Stopwatch.GetTimestamp();

            if (currentTicks - _lastQueryTicks >= _motorQueryTicks)
            {
                Query(true);

                _lastQueryTicks = currentTicks;
            }

            if (currentTicks - _lastCommandTicks >= _motorCommandTicks)
            {
                // Apply Temperature-Based Output Reduction
                int reductionRange = _plugin.Settings.StoppedOutputTemperature - _plugin.Settings.ReducedOutputTemperature;
                double outputReduction = 1.0;
                string warningGraphic = String.Empty;

                foreach (Motor motor in Motors)
                {
                    if (motor.Temperature >= _plugin.Settings.ReducedOutputTemperature)
                    {
                        warningGraphic = MotorGraphic.Overheating;

                        double motorReduction = Math.Max(0.0, Math.Min(1.0,
                            (_plugin.Settings.StoppedOutputTemperature - motor.Temperature) / (double)reductionRange
                        ));

                        outputReduction = Math.Min(outputReduction, motorReduction);
                    }

                    if (motor.Temperature >= _plugin.Settings.StoppedOutputTemperature)
                    {
                        warningGraphic = MotorGraphic.Overheated;

                        outputReduction = 0.0;

                        break;
                    }
                }

                WarningGraphic = warningGraphic;

                shoulderLeft *= outputReduction;
                shoulderRight *= outputReduction;
                waistLeft *= outputReduction;
                waistRight *= outputReduction;

                var torqueMapping = new Dictionary<string, double> {
                    { MotorMapping.LeftShoulder.Label, shoulderLeft },
                    { MotorMapping.RightShoulder.Label, shoulderRight },
                    { MotorMapping.LeftWaist.Label, waistLeft },
                    { MotorMapping.RightWaist.Label, waistRight },
                };

                // Apply Smoothing & Output
                didSet = false;
                foreach (Motor motor in Motors)
                {
                    if (motor.Identifier == _motorCommandIdentifier) // IGNORE UNUSED MOTORS?
                    {
                        motor.SetTorque(
                            (
                                torqueMapping.TryGetValue(motor.Mapping.Label, out var motorTorque)
                                    ? motorTorque
                                    : 0.0
                            ) * motor.Direction.Multiplier, // MOVE TO INTERNAL ON SETTORQUE?
                            smoothingFactor
                        );
                    }
                }

                _lastCommandTicks = currentTicks;

                if (++_motorCommandIdentifier > 4)
                {
                    _motorCommandIdentifier = 1;
                }
            }

            EndAction(action);

            return didSet;
        }

        /// <summary>Records the (optionally) given action name as being in-progress. Uses the parent caller name if omitted</summary>
        /// <remarks>Consult <see cref="IsBusy" /> to check if any actions are in-progress and <see cref="EndAction" /> to mark an action as complete</remarks>
        /// <returns>The identifier of the action</returns>
        private void StartAction(out string action, [CallerMemberName] string name = "")
        {
            lock (_actionLock)
            {
                name = $"{name}:{_actionsCounter++}";

                _actionsIdentifiers.Add(name);

                action = name;
            }
        }

        /// <summary>Marks the given action identifier as complete</summary>
        /// <remarks>Consult <see cref="IsBusy" /> to check if any actions are in-progress and <see cref="EndAction" /> to mark an action as complete</remarks>
        /// <returns>The identifier of the action</returns>
        private void EndAction(string name)
        {
            lock (_actionLock)
            {
                _actionsIdentifiers.Remove(name);
            }
        }

        /// <summary>Restricts the given value to the given range</summary>
        private static short ClampValue(short value, short minimum, short maximum)
        {
            if (value < minimum) { return minimum; }

            if (value > maximum) { return maximum; }

            return value;
        }

        /// <summary>Discards any bytes currently within serial buffer</summary>
        /// <returns>The number of bytes cleared</returns>
        private int FlushSerialBuffer()
        {
            int bytes = 0;

            if (_serialPort == null)
            {
                return bytes;
            }

            lock (_serialLock)
            {
                try
                {
                    while (_serialPort.BytesToRead > 0)
                    {
                        if (_serialPort.ReadByte() != -1)
                        {
                            bytes++;
                        }
                    }
                }
                catch { }
            }

            return bytes;
        }

        /// <summary>Sends the given bytes over the serial port connection, then waits for a response and populates the given response buffer</summary>
        /// <remarks>The timeout may be customised and the verification of the checksum can be disabled if needed</remarks>
        /// <returns>Whether the motor responded as expected</returns>
        public bool WriteFrameReadFrame(byte[] tx, byte[] rx, int timeout = 5, bool shouldValidate = true, bool shouldLog = false)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
            {
                Logging.Current.Warn("SABT: Serial port is not avaiable or not open");

                return false;
            }

            lock (_serialLock)
            {
                try
                {
                    while (_serialPort.BytesToRead > 0)
                    {
                        _serialPort.ReadByte();
                    }

                    _serialPort.Write(tx, 0, tx.Length);
                }
                catch
                {
                    return false;
                }

                if (shouldLog)
                {
                    Logging.Current.Info("SABT: Motor TX (" + BitConverter.ToString(tx) + ")");
                }

                long startedAt = System.Diagnostics.Stopwatch.GetTimestamp();
                long timeoutTicks = (long)(timeout * System.Diagnostics.Stopwatch.Frequency / 1000.0);
                int receivedBytes = 0;

                while (receivedBytes < 10)
                {
                    try
                    {
                        int b = _serialPort.ReadByte();
                        if (b < 0) { continue; }
                        rx[receivedBytes++] = (byte)b;
                    }
                    catch (TimeoutException)
                    { }

                    long elapsed = System.Diagnostics.Stopwatch.GetTimestamp() - startedAt;

                    if (elapsed > timeoutTicks)
                    {
                        Array.Clear(rx, 0, rx.Length);

                        return false;
                    }
                }
            }

            if (shouldLog)
            {
                Logging.Current.Info("SABT: Motor RX (" + BitConverter.ToString(rx) + ")");
            }

            if (shouldValidate)
            {
                byte checksum = CalculateChecksum(rx, 9);
                byte given = rx[9];
                bool isValid = (given == checksum);

                if (!isValid)
                {
                    Logging.Current.Warn("SABT: Invalid motor response checksum (" + given.ToString("X2") + " != " + checksum.ToString("X2") + ")");

                    return false;
                }
            }

            return true;
        }

        /// <summary>Constructs a byte 'frame' that can be understood by the motor controller</summary>
        /// <returns>The byte array of the constructed frame</returns>
        private static byte[] BuildFrame(
            byte identifier,
            byte command,
            byte byte0 = 0,
            byte byte1 = 0,
            byte byte2 = 0,
            byte byte3 = 0,
            byte byte4 = 0,
            byte byte5 = 0,
            byte byte6 = 0,
            byte? byte7 = null
        )
        {
            byte[] payload = new byte[10];

            payload[0] = identifier;
            payload[1] = command;
            payload[2] = byte0; payload[3] = byte1; payload[4] = byte2; payload[5] = byte3;
            payload[6] = byte4; payload[7] = byte5; payload[8] = byte6;
            payload[9] = byte7.HasValue ? byte7.Value : CalculateChecksum(payload, 9);

            return payload;
        }

        /// <summary>Determines the checksum byte for the given 'frame' byte array</summary>
        private static byte CalculateChecksum(byte[] data, int dataLength)
        {
            byte checksum = 0x00;

            for (int i = 0; i < dataLength; i++)
            {
                checksum ^= data[i];

                for (int b = 0; b < 8; b++)
                {
                    if ((checksum & 0x01) != 0)
                    {
                        checksum = (byte)((checksum >> 1) ^ 0x8C);
                    }
                    else
                    {
                        checksum >>= 1;
                    }
                }
            }

            return checksum;
        }

        /// <summary>Identifies devices that match the expected VID/PID for the controller board (or more specifically, the serial bridge we using on it)</summary>
        /// <returns>A list of <see cref="DeviceInstance" /> instances that appear to match</returns>
        public string[] UpdateSerialPorts()
        {
            if (_serialPort != null && _serialPort.IsOpen && _plugin.IsEnabled && SerialPorts?.Length > 0)
            {
                return SerialPorts;
            }

            Logging.Current.Info("SABT: Detecting serial ports...");

            const string vidPid = "VID_1A86&PID_55D3";

            Regex portPattern = new Regex(@"\((COM\d+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            List<DeviceInstance> devices = new List<DeviceInstance>();

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "SELECT Name, Caption, PNPDeviceID FROM Win32_PnPEntity"))
            {
                foreach (ManagementObject mo in searcher.Get())
                {
                    string name = mo["Name"] as string ?? string.Empty;
                    string caption = mo["Caption"] as string ?? string.Empty;
                    string pnpDeviceId = mo["PNPDeviceID"] as string ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(pnpDeviceId))
                    {
                        continue;
                    }

                    if (!pnpDeviceId.Contains(vidPid, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string display = !string.IsNullOrWhiteSpace(name) ? name : caption;
                    Match match = portPattern.Match(display);

                    if (!match.Success)
                    {
                        continue;
                    }

                    devices.Add(new DeviceInstance
                    {
                        SerialPort = match.Groups[1].Value.ToUpperInvariant(),
                        Name = display,
                        PnpDeviceId = pnpDeviceId
                    });
                }
            }

            string[] serialPorts = devices
                .OrderBy(d => d.SerialPort, StringComparer.OrdinalIgnoreCase)
                .ToList()
                .Select(device => device.SerialPort)
                .Where(port => !string.IsNullOrWhiteSpace(port))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(port => port, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            SerialPorts = serialPorts;

            if (serialPorts.Length < 1)
            {
                Disconnect();

                _plugin.Settings.SerialPort = null;

                return SerialPorts;
            }

            if (
                string.IsNullOrWhiteSpace(_plugin.Settings.SerialPort) ||
                !serialPorts.Contains(_plugin.Settings.SerialPort, StringComparer.OrdinalIgnoreCase)
            ) {
                _plugin.Settings.SerialPort = serialPorts[0];
            }

            return SerialPorts;
        }

        public sealed class DeviceInstance
        {
            public string SerialPort { get; set; }
            public string Name { get; set; }
            public string PnpDeviceId { get; set; }
        }
    }
}

public sealed class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Predicate<object> _canExecute;

    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
        => _canExecute == null || _canExecute(parameter);

    public void Execute(object parameter)
        => _execute(parameter);

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}
