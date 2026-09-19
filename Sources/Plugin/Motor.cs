
using SimHub;
using SimHub.Plugins;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Threading.Tasks;
using User.ActiveBeltTensioner;
using WoteverLocalization;

namespace User.ActiveBeltTensioner
{
    /// <summary>A representation of a single motor, which receives and responds to commands via the shared serial port of the <see cref="MotorController" /></summary>
    public class Motor : INotifyPropertyChanged
    {
        public struct MotorStatus
        {
            public byte Key { get; }
            public string Label { get; }
            public string Graphic { get; }
            public Brush Color { get; }

            public MotorStatus(byte key, string label, Brush color, string graphic)
            {
                Key = key;
                Label = label;
                Color = color;
                Graphic = graphic;
            }

            public static MotorStatus Disabled = new MotorStatus(0, "Disabled", (Brush)new BrushConverter().ConvertFrom("#454545"), "/User.ActiveBeltTensioner;component/Motor, Disconnected.png");
            public static MotorStatus Unavailable = new MotorStatus(1, "Unavailable", (Brush)new BrushConverter().ConvertFrom("#f44336"), "/User.ActiveBeltTensioner;component/Motor, Error.png");
            public static MotorStatus Connected = new MotorStatus(2, "Connected", (Brush)new BrushConverter().ConvertFrom("#357c38"), "/User.ActiveBeltTensioner;component/Motor, Connected.png");
            public static MotorStatus Communicating = new MotorStatus(3, "Communicating", (Brush)new BrushConverter().ConvertFrom("#119eda"), "/User.ActiveBeltTensioner;component/Motor, Communicating.png");
            public static MotorStatus Tensioning = new MotorStatus(5, "Tensioning", (Brush)new BrushConverter().ConvertFrom("#ffd03a"), "/User.ActiveBeltTensioner;component/Motor, Communicating.png");
            public static MotorStatus Testing = new MotorStatus(6, "Testing", (Brush)new BrushConverter().ConvertFrom("#ffd03a"), "/User.ActiveBeltTensioner;component/Motor, Communicating.png");

            public static MotorStatus Overheating = new MotorStatus(7, "Overheating", (Brush)new BrushConverter().ConvertFrom("#ff9800"), "/User.ActiveBeltTensioner;component/Motor, Overheating.png");
            public static MotorStatus Overheated = new MotorStatus(8, "Overheated", (Brush)new BrushConverter().ConvertFrom("#ff9800"), "/User.ActiveBeltTensioner;component/Motor, Overheated.png");

            public static MotorStatus[] Statuses = {
                Disabled,
                Unavailable,
                Connected,
                Communicating,
                Tensioning,
                Testing,
                Overheating,
                Overheated
            };
        }

        public struct MotorMapping
        {
            public byte Key { get; }
            public string Label { get; }
            public string Graphic { get; }
            public MotorMapping(byte key, string label, string graphic)
            {
                Key = key;
                Label = label;
                Graphic = graphic;
            }

            public static MotorMapping Unused = new MotorMapping(0, "Unused", "/User.ActiveBeltTensioner;component/Mapping.Unused.png");
            public static MotorMapping LeftShoulder = new MotorMapping(1, "Left Shoulder", "/User.ActiveBeltTensioner;component/Mapping.LeftShoulder.png");
            public static MotorMapping RightShoulder = new MotorMapping(2, "Right Shoulder", "/User.ActiveBeltTensioner;component/Mapping.RightShoulder.png");
            public static MotorMapping LeftWaist = new MotorMapping(3, "Left Waist", "/User.ActiveBeltTensioner;component/Mapping.LeftWaist.png");
            public static MotorMapping RightWaist = new MotorMapping(4, "Right Waist", "/User.ActiveBeltTensioner;component/Mapping.RightWaist.png");

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
            public byte Key { get; }
            public string Label { get; }
            public sbyte Multiplier { get; }
            public string Graphic { get; }
            public MotorDirection(byte key, string label, sbyte multiplier, string graphic)
            {
                Key = key;
                Label = label;
                Multiplier = multiplier;
                Graphic = graphic;
            }

            public static MotorDirection Clockwise = new MotorDirection(0, "Clockwise", 1, "/User.ActiveBeltTensioner;component/Direction.Clockwise.png");
            public static MotorDirection AntiClockwise = new MotorDirection(1, "Anti-Clockwise", -1, "/User.ActiveBeltTensioner;component/Direction.AntiClockwise.png");

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

        public byte Identifier { get; set; } = 0;

        private MotorController _controller;
        public MotorController Controller
        {
            get {
                return _controller;
            }
        }

        private MotorStatus _status = MotorStatus.Unavailable;
        public MotorStatus Status
        {
            get { return _status; }
            set
            {
                if (_status.Key != value.Key)
                {
                    _status = value;
                    InvokePropertyChange();
                }
            }
        }

        private MotorMapping _mapping = MotorMapping.Unused;
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

        private byte? _angle = null;
        public byte? Angle
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

        private byte? _temperature = null;
        public byte? Temperature
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
                    if (HighestTemperature == null || value > HighestTemperature)
                    {
                        HighestTemperature = value;
                    }
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

        private int _mostFaults = 0;
        public int MostFaults
        {
            get { return _mostFaults; }
            private set
            {
                if (_mostFaults != value)
                {
                    _mostFaults = value;
                    InvokePropertyChange();
                }
            }
        }

        private const short _maximumConsecutiveFaults = 100;
        private const byte _torqueMode = 0x01;
        private const short _torqueLimit = 12000;

        private int _commandFailures = 0;
        private double _smoothedTorque = 0.0;

        public ICommand TriggerTest { get; }
        public ICommand TriggerTension { get; }
        public ICommand TriggerAssign { get; }

        public Motor(MotorController controller, byte identifier)
        {
            _controller = controller;

            Identifier = identifier;

            TriggerTest = new RelayCommand(
                execute: _ => _ = Task.Run(() => Test())
            );

            TriggerTension = new RelayCommand(
                execute: _ => _ = Task.Run(() => Tension())
            );

            TriggerAssign = new RelayCommand(
                execute: _ => _ = Task.Run(() => Assign())
            );
        }

        /// <summary>Resets the session diagnostic data for the motor</summary>
        public void ResetDiagnostics()
        {
            Angle = null;
            Temperature = null;
            HighestTemperature = null;
            MostFaults = 0;
        }

        /// <summary>Assigns the identifier to the motor via a series of guided prompts</summary>
        /// <returns>Whether the process succeeded</returns>
        public bool Assign()
        {
            int detectedMotors = _controller.DetectMotors();

            if (detectedMotors == 1)
            {
                switch (
                    MessageBox.Show(
                        SLoc.GetValue("SABT_Message_PreviouslyAssignedMotorDetected"),
                        SLoc.GetValue("SABT_Plugin"),
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Warning
                    )
                )
                {
                    case MessageBoxResult.Yes:
                    case MessageBoxResult.No:

                        break;

                    case MessageBoxResult.Cancel:

                        return false;
                }
            }
            else if (detectedMotors > 1)
            {
                MessageBox.Show(
                    SLoc.GetValue("SABT_Message_PreviouslyAssignedMotorsDetected"),
                    SLoc.GetValue("SABT_Plugin"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return false;
            }

            if (
                MessageBox.Show(
                    SLoc.GetValue("SABT_Message_PlugInOneMotorOnly"),
                    SLoc.GetValue("SABT_Plugin"),
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Information
                ) == MessageBoxResult.Yes
            )
            {
                if (!SetIdentifier() || _controller.DetectMotors() != 1)
                {
                    MessageBox.Show(
                        SLoc.GetValue("SABT_Message_FailedToAssignMotorIdentifier"),
                        SLoc.GetValue("SABT_Plugin"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    return false;
                }

                MessageBox.Show(
                    SLoc.GetValue("SABT_Message_AssignedMotorIdentifier"),
                    SLoc.GetValue("SABT_Plugin"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                return true;
            }

            Status = MotorStatus.Unavailable;

            return false;
        }

        /// <summary>Invokes various methods to ascertain the status of the motor, while updating its status indicators</summary>
        /// <returns>Whether the motor responded as expected (or has been set as disabled)</returns>
        public bool Check()
        {
            IsConnected = false;
            Status = MotorStatus.Communicating;

            _smoothedTorque = 0;

            if (!_controller.HasDevice)
            {
                IsConnected = false;
                Status = MotorStatus.Unavailable;

                ResetDiagnostics();

                return false;
            }

            if (Query(false))
            {
                if (Query(true))
                {
                    IsConnected = true;
                    Status = MotorStatus.Connected;

                    return true;
                }

                Status = MotorStatus.Communicating;

                if (SetMode(_torqueMode))
                {
                    IsConnected = true;
                    Status = MotorStatus.Connected;

                    return true;
                }
            }

            IsConnected = false;
            Status = (Mapping.Key == MotorMapping.Unused.Key)
                ? MotorStatus.Disabled
                :MotorStatus.Unavailable;
            Angle = null;
            Temperature = null;

            return false;
        }

        /// <summary>Sends a stop (zero torque) command to the motor until a response is received or limited attempts run out, while updating its status indicators</summary>
        /// <returns>Whether the motor responded as expected</returns>
        public bool Stop()
        {
            IsConnected = false;
            Status = MotorStatus.Communicating;

            _smoothedTorque = 0;

            byte[] tx = BuildFrame(Identifier, 0x64, 0x00, 0x00);
            byte[] rx = new byte[10];

            for (int i = 0; i < 5; i++)
            {
                if (_controller.WriteFrameReadFrame(tx, rx))
                {
                    IsConnected = true;
                    Status = MotorStatus.Connected;

                    return true;
                }
            }

            Status = MotorStatus.Unavailable;
            Angle = null;
            Temperature = null;

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
                // Error = rx[8];

                return true;
            }

            Angle = null;
            Temperature = null;

            return false;
        }

        /// <summary>Sends a series of torque commands to the motor to oscillate it, while updating its status indicators</summary>
        /// <param name="times">The number of times to oscillate the motor</param>
        /// <param name="testTorque">The fraction of the torque limit to apply during testing</param>
        /// <returns>Whether the motor responded as expected</returns>
        public bool Test(int times = 50, double testTorque = 0.6)
        {
            Status = MotorStatus.Testing;

            if (!Query(true))
            {
                IsConnected = false;
                Status = MotorStatus.Unavailable;

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

                if (_controller.WriteFrameReadFrame(tx, rx, 3, true))
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

            Stop();

            if (bad > times * 0.1)
            {
                if (good < 1)
                {
                    IsConnected = false;
                    Status = MotorStatus.Unavailable;

                    return false;

                }

                Logging.Current.Warn("SABT: #" + this.Identifier + " motor test partially failed (" + bad + "/" + times + ")");

                return true;
            }

            IsConnected = true;
            Status = MotorStatus.Connected;

            return true;
        }


        /// <summary>Applies a smoothed tension to the motor over a period of time for the purposes of taking up slack in the belts/cord</summary>
        /// <param name="duration">The total time in milliseconds to apply the tension</param>
        /// <param name="tensionTorque">The fraction of the torque limit to apply during tensioning</param>
        /// <returns>Whether the motor responded as expected</returns>
        public bool Tension(int duration = 3000, double tensionTorque = 0.4)
        {
            Status = MotorStatus.Tensioning;

            if (!Query(true))
            {
                IsConnected = false;
                Status = MotorStatus.Unavailable;

                return false;
            }

            int direction = Direction.Multiplier;
            
            const int commandInterval = 25;

            int totalTime = Math.Abs(duration) + 1;
            int elapsedTime = 0;

            bool didRespond = false;

            while (elapsedTime <= totalTime)
            {
                double progress = (double) elapsedTime / totalTime;
                double currentTorque;

                if (progress < 0.2)
                {
                    // <20% Ramp Up
                    currentTorque = (progress / 0.2) * tensionTorque;
                }
                else if (progress > 0.8)
                {
                    // >80% Ramp Down
                    currentTorque = ((1.0 - progress) / 0.2) * tensionTorque;
                }
                else
                {
                    // 20%-80% At Target Torque
                    currentTorque = tensionTorque;
                }

                short torque = (short)(currentTorque * direction * _torqueLimit);

                byte highByte = (byte)((torque >> 8) & 0xFF);
                byte lowByte = (byte)(torque & 0xFF);

                byte[] tx = BuildFrame(Identifier, 0x64, highByte, lowByte);
                byte[] rx = new byte[10];

                if (_controller.WriteFrameReadFrame(tx, rx, 3, true))
                {
                    didRespond = true;
                }

                if (elapsedTime >= totalTime)
                {
                    break;
                }

                int pauseTime = Math.Min(commandInterval, totalTime - elapsedTime);

                Thread.Sleep(TimeSpan.FromMilliseconds(pauseTime));

                elapsedTime += pauseTime;
            }

            if (didRespond)
            {
                IsConnected = true;
                Status = MotorStatus.Connected;

                return true;
            }

            IsConnected = false;
            Status = MotorStatus.Unavailable;

            Logging.Current.Warn("SABT: #" + this.Identifier + " motor tensioning failed");

            return false;
        }

        /// <summary>Sends a series of identifier allocation commands to the motor</summary>
        /// <remarks>The motor firmware requires 5 repeated commands of this type to actually change the value; and it can only be changed once per power cycle</remarks>
        /// <returns>Whether the motor responded as expected</returns>
        public bool SetIdentifier()
        {
            Logging.Current.Info("SABT: Assigning #" + this.Identifier + " identifer to motor");

            Status = MotorStatus.Communicating;

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
                    Logging.Current.Info("SABT: Assigned #" + this.Identifier + " identifer to motor successfully");

                    Status = MotorStatus.Connected;

                    return true;
                }
            }

            Logging.Current.Warn("SABT: Failed to assign #" + this.Identifier + " identifer to motor");

            Status = MotorStatus.Unavailable;

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

            torque = _smoothedTorque * Direction.Multiplier;

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

                if (_commandFailures > MostFaults)
                {
                    MostFaults = _commandFailures;
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

        /// <summary>Constructs a byte 'frame' that can be understood by the motor controller</summary>
        /// <remarks>Some commands have an additional byte and no checksum; these can be built by providing the final optional parameter</remarks>
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
        public static byte CalculateChecksum(byte[] data, int dataLength)
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

        /// <summary>Restricts the given value to the given range</summary>
        private static short ClampValue(short value, short minimum, short maximum)
        {
            if (value < minimum) { return minimum; }

            if (value > maximum) { return maximum; }

            return value;
        }
    }
}