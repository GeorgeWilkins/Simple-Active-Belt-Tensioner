using MahApps.Metro.IconPacks;
using SimHub;
using SimHub.Plugins;
using SimHub.Plugins.DataPlugins.RGBDriverCommon.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using WoteverCommon.Extensions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace User.ActiveBeltTensioner
{
    /// <summary>A representation of the motor control system, which is technically one serial port shared by multiple <see cref="Motor" /> objects</summary>
    public class MotorController : INotifyPropertyChanged, IDisposable
    {
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
            public string Label { get; set; }

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

            private string _status = "Not Connected";
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

            private PackIconMaterialKind _icon = PackIconMaterialKind.HelpCircleOutline;
            public PackIconMaterialKind Icon
            {
                get { return _icon; }
                set
                {
                    if (_icon != value)
                    {
                        _icon = value;
                        InvokePropertyChange();
                    }
                }
            }

            private const byte _torqueMode = 0x01;
            private const short _torqueLimit = 12000;
            private MotorController _controller;

            private int _commandFailures = 0;
            private double _smoothedTorque = 0.0;

            public Motor(MotorController controller, byte identifier, string label = "Unassigned")
            {
                _controller = controller;

                Identifier = identifier;
                Label = label;
            }

            /// <summary>Invokes various methods to ascertain the status of the motor, while updating its status indicators</summary>
            /// <returns>Whether a connection could be established and the motor responded as expected</returns>
            public bool Connect()
            {
                IsConnected = false;
                Status = "Connecting...";
                Icon = PackIconMaterialKind.CircleSlice3;

                _smoothedTorque = 0;

                if (!_controller.HasSerial)
                {
                    Status = "No Device Detected";
                    Icon = PackIconMaterialKind.CloseCircleOutline;

                    return false;
                }

                if (Check(false))
                {
                    Status = "Checking Mode...";
                    Icon = PackIconMaterialKind.CircleSlice5;

                    if (Check(true))
                    {
                        IsConnected = true;
                        Status = "Connected";
                        Icon = PackIconMaterialKind.CheckCircle;

                        return true;
                    }

                    Status = "Setting Mode...";
                    Icon = PackIconMaterialKind.CircleSlice7;

                    if (SetMode(_torqueMode))
                    {
                        IsConnected = true;
                        Status = "Connected";
                        Icon = PackIconMaterialKind.CheckCircle;

                        return true;
                    }
                }

                IsConnected = false;
                Status = "Failed To Connect";
                Icon = PackIconMaterialKind.CloseCircleOutline;

                return false;
            }

            /// <summary>Sends a stop (zero torque) command to the motor until a response is received or limited attempts run out, while updating its status indicators</summary>
            /// <returns>Whether the motor responded as expected</returns>
            public bool Stop()
            {
                IsConnected = false;
                Status = "Stopping...";
                Icon = PackIconMaterialKind.StopCircle;

                _smoothedTorque = 0;

                byte[] tx = BuildFrame(Identifier, 0x64, 0x00, 0x00);
                byte[] rx = new byte[10];

                bool hasStopped = false;
                for (int i = 0; i < 5; i++)
                {
                    if (_controller.WriteFrameReadFrame(tx, rx))
                    {
                        hasStopped = true;
                        break;
                    }
                }

                if (hasStopped)
                {
                    Status = "Disconnected (Stopped)";
                    Icon = PackIconMaterialKind.CircleOffOutline;

                    return true;
                }

                Status = "Failed To Stop";
                Icon = PackIconMaterialKind.CarBrakeAlert;

                return false;
            }

            /// <summary>Sends a status request command to the motor and checks the response (if any) for validity</summary>
            /// <returns>Whether the motor responded as expected</returns>
            public bool Check(bool isInTorqueMode = true)
            {
                byte[] tx = BuildFrame(Identifier, 0x74);
                byte[] rx = new byte[10];

                if (_controller.WriteFrameReadFrame(tx, rx, 300, true, true))
                {
                    if (rx[0] != Identifier) { return false; }
                    if (isInTorqueMode && rx[1] != _torqueMode) { return false; }
                    if (rx[6] >= 60) { return false; } // Temperature
                    if (rx[8] != 0x00) { return false; } // Error

                    return true;
                }

                return false;
            }

            /// <summary>Sends a series of torque commands to the motor to oscillate it, while updating its status indicators</summary>
            /// <returns>Whether the motor responded as expected</returns>
            public bool Test(int times = 8, double maximumTorque = 0.10)
            {
                Status = "Testing...";
                Icon = PackIconMaterialKind.AccessPoint;

                if (!Check(true))
                {
                    IsConnected = false;
                    Status = "Failed To Test (Disconnected)";
                    Icon = PackIconMaterialKind.CloseCircleOutline;

                    return false;
                }

                int good = 0;
                int bad = 0;

                times += 5;

                short torque = (short)(maximumTorque * _torqueLimit);
                for (int i = 0; i < times; i++)
                {
                    Icon = GetProgressIcon(i, times);

                    byte highByte = (byte)((torque >> 8) & 0xFF);
                    byte lowByte = (byte)(torque & 0xFF);

                    byte[] tx = BuildFrame(Identifier, 0x64, highByte, lowByte);
                    byte[] rx = new byte[10];

                    if (_controller.WriteFrameReadFrame(tx, rx, 20, true, true))
                    {
                        good++;
                    }
                    else
                    {
                        bad++;
                    }

                    Thread.Sleep(200);

                    if (i >= times - 5) {
                        torque = 0;
                    }

                    torque *= -1;
                }

                if (bad > 0)
                {
                    if (good < 1)
                    {
                        IsConnected = false;
                        Status = "Test Failed (No Response)";
                        Icon = PackIconMaterialKind.AlertCircleOutline;

                        return false;

                    }

                    IsConnected = true;
                    Status = "Test Partially Passed (Bad Responses)";
                    Icon = PackIconMaterialKind.AlertCircleCheckOutline;

                    return true;
                }

                IsConnected = true;
                Status = "Test Passed";
                Icon = PackIconMaterialKind.CheckCircle;

                return true;
            }

            /// <summary>Sends a series of identifier allocation commands to the motor</summary>
            /// <remarks>The motor firmware requires 5 repeated commands of this type to actually change the value; and it can only be changed once per power cycle</remarks>
            /// <returns>Whether the motor responded as expected</returns>
            public bool SetIdentifier()
            {
                byte[] tx = BuildFrame(0xAA, 0x55, 0x53, Identifier, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);
                byte[] rx = new byte[10];

                for (int i = 0; i < 5; i++)
                {
                    Icon = GetProgressIcon(i, 5);

                    _controller.FlushSerialBuffer();
                    _controller.WriteFrameReadFrame(tx, rx, 100, false, true);
                }

                Thread.Sleep(500);

                if (Check(false))
                {
                    if (SetMode(_torqueMode))
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>Sends a mode change command with the given mode byte motor (<see langword="0x01" />: torque, <see langword="0x02" />: velocity, <see langword="0x03" />: position)</summary>
            /// <returns>Whether the motor responded as expected</returns>
            public bool SetMode(byte mode)
            {
                byte[] tx = BuildFrame(Identifier, 0xA0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, mode);
                byte[] rx = new byte[10];

                _controller.WriteFrameReadFrame(tx, rx, 200, false, true);
                
                return Check(true);
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

                if (!_controller.WriteFrameReadFrame(tx, rx, 10))
                {
                    _commandFailures++;
                    
                    Logging.Current.Warn("SABT: " + this.Label + " Motor RX Failure (" + _commandFailures + ")");

                    return (_commandFailures < 10);
                }
                
                _commandFailures = 0;

                return true;
            }

            /// <summary>Provides an appropiately mapped icon object for the given values, affording a crude progress indicator</summary>
            public static PackIconMaterialKind GetProgressIcon(int current, int total)
            {
                PackIconMaterialKind[] icons = {
                    PackIconMaterialKind.CircleOutline,
                    PackIconMaterialKind.CircleSlice1,
                    PackIconMaterialKind.CircleSlice2,
                    PackIconMaterialKind.CircleSlice3,
                    PackIconMaterialKind.CircleSlice4,
                    PackIconMaterialKind.CircleSlice5,
                    PackIconMaterialKind.CircleSlice6,
                    PackIconMaterialKind.CircleSlice7,
                    PackIconMaterialKind.CircleSlice8,
                };

                int index = Math.Min(
                    (current * icons.Length) / total, icons.Length - 1
                );

                return icons[index];
            }
        }

































        public Motor[] Motors { get; private set; }
        public bool IsFlipped { get; set; }
        public bool IsBusy {
            get { lock (_actionLock) { return _actionsIdentifiers.Count > 0; } }
        }
        public bool HasSerial
        {
            get { return (_serialPort != null); }
        }





        public bool BothMotorsAreConnected {
            get { return GetLeftMotor().IsConnected && GetRightMotor().IsConnected; }
        }

        public bool OneMotorIsConnected {
            get { return GetLeftMotor().IsConnected != GetRightMotor().IsConnected; }
        }

        public bool LeftMotorIsConnected {
            get { return GetLeftMotor().IsConnected; }
        }
        public bool RightMotorIsConnected
        {
            get { return GetRightMotor().IsConnected; }
        }
        public string LeftMotorStatus {
            get { return GetLeftMotor().Status; }
        }
        public string RightMotorStatus {
            get { return GetRightMotor().Status; }
        }
        public PackIconMaterialKind LeftMotorIcon {
            get { return GetLeftMotor().Icon; }
        }
        public PackIconMaterialKind RightMotorIcon
        {
            get { return GetRightMotor().Icon; }
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




        private bool _motorCommandSwitch = true;

        private readonly long _motorCommandTicks;
        private long _lastCommandTicks = 0;








        public MotorController(DevicePlugin plugin)
        {
            _plugin = plugin;

            Motors = new Motor[] {
                new Motor(this, 0x01, "Left"),
                new Motor(this, 0x02, "Right")
            };

            foreach (Motor motor in Motors)
            {
                motor.PropertyChanged += MotorPropertyChanged;
            }




            
            _motorCommandTicks = (long)(16.67 * System.Diagnostics.Stopwatch.Frequency / 1000.0); // 60Hz
        }

        private void MotorPropertyChanged(object origin, PropertyChangedEventArgs e)
        {
            Motor motor = origin as Motor;

            if (motor == null) return;

            InvokePropertyChange($"{motor.Label}Motor{e.PropertyName}");

            InvokePropertyChange(nameof(BothMotorsAreConnected));
            InvokePropertyChange(nameof(OneMotorIsConnected));
        }

        /// <summary>Opens the serial port, then invokes the <see cref="Motor.Connect()" /> method on each motor</summary>
        /// <returns>Whether all indivdual <see cref="Motor.Connect()" /> methods succeeded</returns>
        public bool Connect()
        {
            string action = StartAction();







            if (!_plugin.PluginManager.IsSimHubLicenceValid && !_hasNotifiedOfLicense)
            {
                _hasNotifiedOfLicense = true;

                MessageBox.Show("Your installation of SimHub is not licensed, so telemetry updates will be limited to 10Hz", "SABT: SimHub License Suggested", MessageBoxButton.OK);
            }

            lock (_serialLock)
            {
                try
                {
                    _serialPort = new SerialPort(_plugin.Settings.SerialPort, 115200)
                    {
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        ReadTimeout = 10,
                        WriteTimeout = 100,
                        DtrEnable = false,
                        RtsEnable = false,
                        NewLine = "\n"
                    };

                    _serialPort.Open();

                    bool didConnect = true;

                    foreach (Motor motor in Motors)
                    {
                        didConnect = motor.Connect() && didConnect;
                    }

                    EndAction(action);

                    return didConnect;
                }
                catch
                {
                    foreach (Motor motor in Motors)
                    {
                        motor.IsConnected = false;
                        motor.Status = "Serial Failure";
                        motor.Icon = PackIconMaterialKind.HelpCircleOutline;
                    }
                }
            }

            EndAction(action);

            return false;
        }

        /// <summary>Invokes the <see cref="Motor.Stop()" /> method on each motor then closes the serial port</summary>
        public void Disconnect()
        {
            string action = StartAction();

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
                        Logging.Current.Warn("SABT: Serial port could not be released");
                    }
                }

                // @TODO: Add Close() method to motors for non-connected situations?
            }

            EndAction(action);
        }

        /// <summary>An alias of <see cref="Disconnect()" /> for the purposes of fulfilling the <see cref="IDisposable" /> interface</summary>
        public void Dispose()
        {
            Disconnect();
        }

        /// <summary>Invokes the <see cref="Motor.Test(int)" /> method on the given <see cref="Motor" /> instance</summary>
        /// <returns>Whether the motor responded as expected</returns>
        public bool TestMotor(Motor motor)
        {
            string action = StartAction();

            bool didPass = motor.Test();

            EndAction(action);

            return didPass;
        }

        /// <summary>Sends the identifier allocation command to the given <see cref="Motor" /> instance</summary>
        /// <returns>Whether the motor responded as expected</returns>
        public bool SetMotorIdentifier(Motor motor)
        {
            string action = StartAction();

            motor.Status = "Checking Motors...";
            motor.Icon = PackIconMaterialKind.RecordCircleOutline;

            if (_serialPort == null || !_serialPort.IsOpen)
            {
                Logging.Current.Warn("SABT: Serial port is not avaiable or not open");

                motor.IsConnected = false;
                motor.Status = "Serial Failure";
                motor.Icon = PackIconMaterialKind.HelpCircleOutline;

                EndAction(action);

                return false;
            }

            int responders = 0;

            foreach (Motor responder in new Motor[] { new Motor(this, 0x00) }.Concat(Motors))
            {
                if (responder.Check(false))
                {
                    responders++;
                }
            }

            if (responders < 1)
            {
                motor.IsConnected = false;
                motor.Status = "Not Detected";
                motor.Icon = PackIconMaterialKind.CrosshairsQuestion;

                EndAction(action);

                return false;
            }

            if (responders > 1)
            {
                motor.Status = "Multiple Motors Detected";
                motor.Icon = PackIconMaterialKind.CheckboxMultipleBlankCircleOutline;

                EndAction(action);

                return false;
            }

            motor.Status = "Setting Identifier...";

            if (motor.SetIdentifier())
            {
                foreach (Motor other in Motors)
                {
                    other.IsConnected = false;
                    other.Status = "Disconnected (Setting Other Identifer)";
                    other.Icon = PackIconMaterialKind.CloseCircleOutline;
                }

                motor.IsConnected = true;
                motor.Status = "Identifier Set";
                motor.Icon = PackIconMaterialKind.CheckCircle;

                EndAction(action);

                return true;
            }

            motor.IsConnected = false;
            motor.Status = "Failed To Set Identifier";
            motor.Icon = PackIconMaterialKind.AlertCircle;

            EndAction(action);

            return false;
        }

        /// <summary>Sends the given torque values (as fractions of maximum torque) to the two motors, alternating between motors at 30Hz per motor (60Hz overall)</summary>
        /// <returns>Whether the motor command was sent successfully (if applicable)</returns>
        public bool SetTorques(double left, double right, double smoothingFactor = 0.0)
        {
            string action = StartAction();

            if (_serialPort == null || !_serialPort.IsOpen)
            {
                Logging.Current.Warn("SABT: Serial port is not avaiable or not open");

                //EndAction(action);

                return false;
            }








            bool didSet = true;
            long currentTicks = System.Diagnostics.Stopwatch.GetTimestamp();

            if (currentTicks - _lastCommandTicks >= _motorCommandTicks)
            {
                didSet = _motorCommandSwitch
                    ? GetLeftMotor().SetTorque(left, smoothingFactor)
                    : GetRightMotor().SetTorque(right * -1, smoothingFactor);

                _lastCommandTicks = currentTicks;
                _motorCommandSwitch = !_motorCommandSwitch;
            }







            EndAction(action);

            return didSet;
        }

        public bool Stop()
        {
            string action = StartAction();

            if (_serialPort == null || !_serialPort.IsOpen)
            {
                Logging.Current.Warn("SABT: Serial port is not avaiable or not open");

                EndAction(action);

                return false;
            }

            bool didStop = (
                GetLeftMotor().Stop() &&
                GetRightMotor().Stop()
            );

            EndAction(action);

            return didStop;
        }

        /// <summary>Provides the motor instance currently mapped to the `left` channel</summary>
        public Motor GetLeftMotor()
        {
            foreach (Motor motor in Motors)
            {
                if (motor.Label == "Left")
                {
                    return motor;
                }
            }

            return null;
        }

        /// <summary>Provides the motor instance currently mapped to the `right` channel</summary>
        public Motor GetRightMotor()
        {
            foreach (Motor motor in Motors)
            {
                if (motor.Label == "Right")
                {
                    return motor;
                }
            }

            return null;
        }

        /// <summary>Records the (optionally) given action name as being in-progress. Uses the parent caller name if omitted</summary>
        /// <remarks>Consult <see cref="IsBusy" /> to check if any actions are in-progress and <see cref="EndAction" /> to mark an action as complete</remarks>
        /// <returns>The identifier of the action</returns>
        private string StartAction([CallerMemberName] string name = "")
        {
            lock (_actionLock)
            {
                name = $"{name}:{_actionsCounter++}";

                _actionsIdentifiers.Add(name);

                return name;
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
        private static short ClampValue(short value, short min, short max)
        {
            if (value < min) { return min; }

            if (value > max) { return max; }

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
        public bool WriteFrameReadFrame(byte[] tx, byte[] rx, int timeout = 10, bool shouldValidate = true, bool shouldLog = false)
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
                }
                catch
                {
                    return false;
                }

                if (shouldLog)
                {
                    Logging.Current.Info("SABT: TX " + BitConverter.ToString(tx));
                }

                _serialPort.Write(tx, 0, tx.Length);

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
                Logging.Current.Info("SABT: RX " + BitConverter.ToString(rx));
            }

            if (shouldValidate)
            {
                byte checksum = CalculateChecksum(rx, 9);
                byte given = rx[9];
                bool isValid = (given == checksum);

                if (!isValid)
                {
                    Logging.Current.Warn("SABT: INVALID CHECKSUM (" + given.ToString("X2") + " != " + checksum.ToString("X2") + ")");

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
            Logging.Current.Info("SABT: Updating serial ports...");

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



            /*
            string[] ports = MotorController.FindMatchingDevices()
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
            */




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
                _plugin.Settings.SerialPort = null;

                return SerialPorts;
            }

            if (
                !string.IsNullOrWhiteSpace(_plugin.Settings.SerialPort) ||
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
