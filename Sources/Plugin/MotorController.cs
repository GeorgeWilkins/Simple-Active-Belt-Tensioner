using MahApps.Metro.IconPacks;
using SimHub;
using SimHub.Plugins;
using SimHub.Plugins.DataPlugins.RGBDriverCommon.Settings;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Media.Media3D;

namespace User.ActiveBeltTensioner
{
    /// <summary>A representation of the motor control system, which is technically one serial port shared by multiple <see cref="Motor" /> objects</summary>
    public class MotorController : IDisposable
    {
        /// <summary>A representation of a single motor, which receives and responds to commands via the shared serial port of the parent <see cref="MotorController" /></summary>
        public class Motor
        {
            public byte Identifier { get; set; } = 0;
            public double Torque { get; set; } = 0.0;
            public long LastCommand { get; set; }
            public bool IsConnected { get; set; } = false;
            public string Status { get; set; } = "Not Connected";
            public PackIconMaterialKind Icon { get; set; } = PackIconMaterialKind.HelpCircleOutline;

            private const byte _torqueMode = 0x01;
            private const short _torqueLimit = 12000;
            private double _maximumCommandGap = 100;

            private readonly long _maximumCommandTicks;
            private MotorController _controller;

            public Motor(MotorController controller, byte identifier)
            {
                _controller = controller;
                Identifier = identifier;

                _maximumCommandTicks = TimeSpan.FromMilliseconds(_maximumCommandGap).Ticks;
            }

            /// <summary>Invokes various methods to ascertain the status of the motor, while updating its status indicators</summary>
            /// <returns>Whether a connection could be established and the motor responded as expected</returns>
            public bool Connect()
            {
                IsConnected = false;
                Status = "Connecting...";
                Icon = PackIconMaterialKind.CircleSlice3;

                _controller.RefreshStatus();

                if (!_controller.HasSerial)
                {
                    Status = "No Device Detected";
                    Icon = PackIconMaterialKind.CloseCircleOutline;

                    _controller.RefreshStatus();

                    return false;
                }

                if (Check(false))
                {
                    Status = "Checking Mode...";
                    Icon = PackIconMaterialKind.CircleSlice5;

                    _controller.RefreshStatus();

                    if (Check(true))
                    {
                        IsConnected = true;
                        Status = "Connected";
                        Icon = PackIconMaterialKind.CheckCircle;

                        _controller.RefreshStatus();

                        return true;
                    }

                    Status = "Setting Mode...";
                    Icon = PackIconMaterialKind.CircleSlice7;

                    _controller.RefreshStatus();

                    if (SetMode(_torqueMode))
                    {
                        IsConnected = true;
                        Status = "Connected";
                        Icon = PackIconMaterialKind.CheckCircle;

                        _controller.RefreshStatus();

                        return true;
                    }
                }

                IsConnected = false;
                Status = "Failed To Connect";
                Icon = PackIconMaterialKind.CloseCircleOutline;

                _controller.RefreshStatus();

                return false;
            }

            /// <summary>Sends a stop (zero torque) command to the motor until a response is received or limited attempts run out, while updating its status indicators</summary>
            /// <returns>Whether the motor responded as expected</returns>
            public bool Stop()
            {
                IsConnected = false;
                Status = "Stopping...";
                Icon = PackIconMaterialKind.StopCircle;

                _controller.RefreshStatus();

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
                    Status = "Disconnected";
                    Icon = PackIconMaterialKind.CircleOffOutline;

                    _controller.RefreshStatus();

                    return true;
                }

                Status = "Failed To Stop";
                Icon = PackIconMaterialKind.CarBrakeAlert;

                _controller.RefreshStatus();

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

                _controller.RefreshStatus();

                if (!Check(true))
                {
                    IsConnected = false;
                    Status = "Failed To Test (Disconnected)";
                    Icon = PackIconMaterialKind.CloseCircleOutline;

                    _controller.RefreshStatus();

                    return false;
                }

                int good = 0;
                int bad = 0;

                times += 5;

                short torque = (short)(maximumTorque * _torqueLimit);
                for (int i = 0; i < times; i++)
                {
                    Icon = GetProgressIcon(i, times);

                    _controller.RefreshStatus();

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

                        _controller.RefreshStatus();

                        return false;

                    }

                    IsConnected = true;
                    Status = "Test Partially Passed (Bad Responses)";
                    Icon = PackIconMaterialKind.AlertCircleCheckOutline;

                    _controller.RefreshStatus();

                    return true;
                }

                IsConnected = true;
                Status = "Test Passed";
                Icon = PackIconMaterialKind.CheckCircle;

                _controller.RefreshStatus();

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

                    _controller.RefreshStatus();
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

            /// <summary>Sends the given torque value (as a fraction of maximum torque) to the motor, where changed or enough time has passed since the last command</summary>
            /// <returns>Whether the motor responded as expected</returns>
            public bool SetTorque(double torque)
            {
                long now = System.Diagnostics.Stopwatch.GetTimestamp();

                if (
                    (Math.Abs(Torque - torque) < double.Epsilon) ||
                    LastCommand == 0 ||
                    (now - LastCommand) > _maximumCommandTicks
                )
                {
                    short newTorque = ClampValue(
                        (short)(torque * _torqueLimit * -1.0),
                        (short)_torqueLimit * -1,
                        (short)_torqueLimit
                    );
                    byte highByte = (byte)((newTorque >> 8) & 0xFF);
                    byte lowByte = (byte)(newTorque & 0xFF);

                    byte[] tx = BuildFrame(Identifier, 0x64, highByte, lowByte);
                    byte[] rx = new byte[10];

                    bool hasResponded = _controller.WriteFrameReadFrame(tx, rx);
                    
                    LastCommand = hasResponded ? now : LastCommand;

                    return hasResponded;
                }

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

        private readonly DevicePlugin _plugin;
        private readonly SerialPort _serialPort;
        private readonly List<string> _actionsIdentifiers = new List<string>();
        private long _actionsCounter = 0;
        private readonly object _actionLock = new object();
        private readonly object _serialLock = new object();
        private bool _hasNotifiedOfLicense = false;

        public MotorController(DevicePlugin plugin, string comPort, int baud = 115200)
        {
            _plugin = plugin;

            _serialPort = new SerialPort(comPort, baud)
            {
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReadTimeout = 100,
                WriteTimeout = 100,
                DtrEnable = false,
                RtsEnable = false,
                NewLine = "\n"
            };

            Motors = new Motor[2];
            Motors[0] = new Motor(this, 0x01);
            Motors[1] = new Motor(this, 0x02);
        }

        /// <summary>Opens the serial port, then invokes the <see cref="Motor.Connect()" /> method on each motor</summary>
        /// <returns>Whether all indivdual <see cref="Motor.Connect()" /> methods succeeded</returns>
        public bool Connect()
        {
            string action = StartAction();

            if (!_plugin.PluginManager.IsSimHubLicenceValid && !_hasNotifiedOfLicense)
            {
                _hasNotifiedOfLicense = true;

                MessageBox.Show("Your installation of SimHub is not licensed, so telemetry updates will be limited to 10Hz", "SimHub License Suggested", MessageBoxButton.OK);
            }

            lock (_serialLock)
            {
                try
                {
                    if (_serialPort != null && !_serialPort.IsOpen)
                    {
                        _serialPort.Open();

                        bool didConnect = true;

                        foreach (Motor motor in Motors)
                        {
                            didConnect = didConnect && motor.Connect();
                        }

                        EndAction(action);

                        return didConnect;
                    }
                }
                catch
                {
                    foreach (Motor motor in Motors)
                    {
                        motor.IsConnected = false;
                        motor.Status = "Serial Failure";
                        motor.Icon = PackIconMaterialKind.HelpCircleOutline;
                    }

                    _plugin.RefreshStatus();
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

            RefreshStatus();

            if (_serialPort == null || !_serialPort.IsOpen)
            {
                Logging.Current.Warn("SABT: Serial port is not avaiable or not open");

                motor.IsConnected = false;
                motor.Status = "Serial Failure";
                motor.Icon = PackIconMaterialKind.HelpCircleOutline;

                RefreshStatus();

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

                RefreshStatus();

                EndAction(action);

                return false;
            }

            if (responders > 1)
            {
                motor.Status = "Multiple Motors Detected";
                motor.Icon = PackIconMaterialKind.CheckboxMultipleBlankCircleOutline;

                RefreshStatus();

                EndAction(action);

                return false;
            }

            motor.Status = "Setting Identifier...";

            if (motor.SetIdentifier())
            {
                foreach (Motor other in Motors)
                {
                    other.IsConnected = false;
                    other.Status = "Disconnected";
                    other.Icon = PackIconMaterialKind.CloseCircleOutline;
                }

                motor.IsConnected = true;
                motor.Status = "Identifier Set";
                motor.Icon = PackIconMaterialKind.CheckCircle;

                RefreshStatus();

                EndAction(action);

                return true;
            }

            motor.IsConnected = false;
            motor.Status = "Failed To Set Identifier";
            motor.Icon = PackIconMaterialKind.AlertCircle;

            RefreshStatus();

            EndAction(action);

            return false;
        }

        /// <summary>Sends the given torque values (as fractions of maximum torque) to the two motors, accounting for the current channel mapping</summary>
        /// <returns>Whether all indivdual <see cref="Motor.SetTorque(double)" /> methods succeeded</returns>
        public bool SetTorques(double left, double right)
        {
            string action = StartAction();

            if (_serialPort == null || !_serialPort.IsOpen)
            {
                Logging.Current.Warn("SABT: Serial port is not avaiable or not open");

                EndAction(action);

                return false;
            }

            bool didSet = (
                GetLeftMotor().SetTorque(left) &&
                GetRightMotor().SetTorque(right * -1)
            );

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
            return IsFlipped ? Motors[1] : Motors[0];
        }

        /// <summary>Provides the motor instance currently mapped to the `right` channel</summary>
        public Motor GetRightMotor()
        {
            return IsFlipped ? Motors[0] : Motors[1];
        }

        /// <summary>Tells the plugin to refresh the motor states for the UI</summary>
        /// <remarks>This feels hacky and needs a better implementation</remarks>
        public void RefreshStatus()
        {
            _plugin.RefreshStatus();
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

                int startedAt = Environment.TickCount;
                int receivedBytes = 0;

                while (receivedBytes < 10)
                {
                    if (Environment.TickCount - startedAt > timeout)
                    {
                        Array.Clear(rx, 0, rx.Length);

                        return false;
                    }

                    try
                    {
                        int b = _serialPort.ReadByte();
                        if (b < 0) { continue; }
                        rx[receivedBytes++] = (byte)b;
                    }
                    catch (TimeoutException)
                    { }
                }
            }

            if (shouldLog)
            {
                Logging.Current.Info("SABT: RX " + BitConverter.ToString(rx));
            }

            if (shouldValidate)
            {
                byte checksum = CalculateChecksum(rx, 9);
                bool isValid = (rx[9] == checksum);

                if (!isValid)
                {
                    Logging.Current.Warn("SABT: INVALID CHECKSUM (" + rx[9].ToString("X2") + " != " + checksum.ToString("X2") + ")");

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
    }
}
