using OxyPlot;
using SimHub;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        public Motor[] Motors { get; private set; }
        public bool IsBusy {
            get { lock (_actionLock) { return _actionsIdentifiers.Count > 0; } }
        }

        public bool HasDevice
        {
            get { return (_device != null); }
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

        private DeviceInstance[] _devices = new DeviceInstance[0];
        public DeviceInstance[] Devices
        {
            get { return _devices; }
            private set
            {
                if (!ReferenceEquals(_devices, value))
                {
                    _devices = value ?? new DeviceInstance[0];
                    InvokePropertyChange(nameof(Devices));
                }
            }
        }

        private DeviceInstance _device;
        public DeviceInstance Device
        {
            get { return _device; }
            set
            {
                if (!ReferenceEquals(_device, value))
                {
                    Logging.Current.Info("DEVICE CHANGED: " + (value != null ? value.Name : "null"));
                    _device = value;

                    _plugin.Settings.DeviceIdentifier = _device?.Identifier;

                    InvokePropertyChange(nameof(Device));
                    InvokePropertyChange(nameof(HasDevice));
                }
            }
        }

        private SerialPort _serialPort;


        private readonly DevicePlugin _plugin;
        private readonly List<string> _actionsIdentifiers = new List<string>();
        private long _actionsCounter = 0;
        private readonly object _actionLock = new object();
        private readonly object _serialLock = new object();
        private bool _hasNotifiedOfLicense = false;

        private byte _motorCommandIdentifier = 1;

        private readonly long _motorCommandTicks;
        private long _lastCommandTicks = 0;
        private readonly long _motorQueryTicks;
        private long _lastQueryTicks = 0;

        public MotorController(DevicePlugin plugin)
        {
            _plugin = plugin;

            Motors = new Motor[] {
                new Motor(this, 1),
                new Motor(this, 2),
                new Motor(this, 3),
                new Motor(this, 4),
            };

            _motorCommandTicks = (long)(3.0 * System.Diagnostics.Stopwatch.Frequency / 1000.0);
            _motorQueryTicks = (long)(3000.0 * System.Diagnostics.Stopwatch.Frequency / 1000.0);
        }

        /// <summary>Resets the session diagnostic data for every connected motor</summary>
        public void ResetDiagnostics()
        {
            foreach (Motor motor in Motors)
            {
                motor.ResetDiagnostics();
            }
        }

        /// <summary>Initializes and returns a serial port object with the given port name</summary>
        private SerialPort CreateSerialPort(string portName)
        {
            return new SerialPort(portName, 115200)
            {
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReadTimeout = 10,
                WriteTimeout = 100,
                DtrEnable = false,
                RtsEnable = false,
                NewLine = "\n"
            };
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

                if (Device == null)
                {
                    Logging.Current.Warn("SABT: Invalid device selection");

                    EndAction(action);

                    return false;
                }

                try
                {
                    Logging.Current.Info("SABT: Opening serial port on device: " + Device.Port + " (" + Device.Name + ")");

                    int retry = 0;
                    const int retries = 10;

                    while (retry < retries)
                    {
                        try
                        {
                            _serialPort?.Dispose();

                            if (Array.IndexOf(SerialPort.GetPortNames(), Device.Port) < 0)
                            {
                                throw new IOException("device port mismatch");
                            }

                            _serialPort = CreateSerialPort(Device.Port);
                            _serialPort.Open();

                            didConnect = true;

                            break;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            Logging.Current.Warn("SABT: Serial port access failure (" + (retry++) + "/" + retries + "): " + ex.Message);

                            Thread.Sleep(150);
                        }
                        catch (IOException ex)
                        {
                            Logging.Current.Warn("SABT: Serial I/O open failure (" + (retry++) + "/" + retries + "): " + ex.Message);

                            Thread.Sleep(300);
                        }
                        catch (InvalidOperationException ex)
                        {
                            Logging.Current.Warn("SABT: Serial state failure (" + (retry++) + "/" + retries + "): " + ex.Message);

                            Thread.Sleep(150);
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
                    Logging.Current.Warn("SABT: Serial communication error (" + exception.Message + ")");

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
                didConnect = (motor.Check() || motor.Mapping.Key == Motor.MotorMapping.Unused.Key) && didConnect;
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
                didRespond = (motor.Query(isInTorqueMode) || motor.Mapping.Key == Motor.MotorMapping.Unused.Key) && didRespond;
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
        /// <remarks>Periodically checks motor status and applies thermal throttling if necessary</remarks>
        /// <returns>Whether the motor commands were sent successfully (if applicable)</returns>
        public bool? SetTorques(
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

            long currentTicks = System.Diagnostics.Stopwatch.GetTimestamp();

            if (currentTicks - _lastCommandTicks >= _motorCommandTicks)
            {
                // Handle Motor Status
                Motor motor = Motors.First(m => m.Identifier == _motorCommandIdentifier);

                if (motor.IsConnected && motor.Mapping.Key != Motor.MotorMapping.Unused.Key)
                {
                    // Query Motor Status
                    if (currentTicks - _lastQueryTicks >= _motorQueryTicks)
                    {
                        motor.Query(true);

                        _lastQueryTicks = currentTicks;
                    }

                    // Apply Thermal Throttling
                    int reductionRange = _plugin.Settings.StoppedOutputTemperature - _plugin.Settings.ReducedOutputTemperature;
                    double outputReduction = 1.0;

                    if (motor.Temperature >= _plugin.Settings.ReducedOutputTemperature)
                    {
                        motor.Status = Motor.MotorStatus.Overheating;

                        double motorReduction = Math.Max(0.0, Math.Min(1.0,
                            (_plugin.Settings.StoppedOutputTemperature - (motor.Temperature ?? 0)) / (double)reductionRange
                        ));

                        outputReduction = Math.Min(outputReduction, motorReduction);
                    }

                    if (motor.Temperature >= _plugin.Settings.StoppedOutputTemperature)
                    {
                        motor.Status = Motor.MotorStatus.Overheated;

                        outputReduction = 0.0;
                    }

                    // Map Torque Output
                    double motorTorque = 0.0;
                    if (motor.Mapping.Key == Motor.MotorMapping.LeftShoulder.Key)
                    {
                        motorTorque = shoulderLeft * outputReduction;
                    }
                    else if (motor.Mapping.Key == Motor.MotorMapping.RightShoulder.Key)
                    {
                        motorTorque = shoulderRight * outputReduction;
                    }
                    else if (motor.Mapping.Key == Motor.MotorMapping.LeftWaist.Key)
                    {
                        motorTorque = waistLeft * outputReduction;
                    }
                    else if (motor.Mapping.Key == Motor.MotorMapping.RightWaist.Key)
                    {
                        motorTorque = waistRight * outputReduction;
                    }

                    motor.SetTorque(motorTorque, smoothingFactor); // ERROR HANDLING?

                    _lastCommandTicks = currentTicks;
                }

                // Select Next Motor
                if (++_motorCommandIdentifier > 4)
                {
                    _motorCommandIdentifier = 1;
                }

                Logging.Current.Info("#" + _motorCommandIdentifier);
            }

            EndAction(action);

            return true;
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

        /// <summary>Discards any bytes currently within serial buffer</summary>
        /// <returns>The number of bytes cleared</returns>
        public int FlushSerialBuffer()
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
                byte checksum = Motor.CalculateChecksum(rx, 9);
                byte given = rx[9];
                bool isValid = (given == checksum);

                if (!isValid)
                {
                    if (shouldLog)
                    {
                        Logging.Current.Warn("SABT: Invalid motor response checksum (" + given.ToString("X2") + " != " + checksum.ToString("X2") + ")");
                    }

                    return false;
                }
            }

            return true;
        }







        public void LoadMotorConfigurations()
        {
            StartAction(out string action);

            foreach (Motor motor in Motors)
            {
                MotorConfiguration motorConfiguration = _plugin.Settings.MotorConfigurations.FirstOrDefault(m => m.Identifier == motor.Identifier);

                if (motorConfiguration == null)
                {
                    motorConfiguration = new MotorConfiguration {
                        Identifier = motor.Identifier,
                        Mapping = motor.Mapping.Key,
                        Direction = motor.Direction.Key
                    };

                    _plugin.Settings.MotorConfigurations.Add(motorConfiguration);
                }

                motor.Mapping = Motor.MotorMapping.Mappings.DefaultIfEmpty(Motor.MotorMapping.Unused).First(x => x.Key == motorConfiguration.Mapping);
                motor.Direction = Motor.MotorDirection.Directions.DefaultIfEmpty(Motor.MotorDirection.Clockwise).First(x => x.Key == motorConfiguration.Direction);
            }

            EndAction(action);
        }

        public int DetectMotors()
        {
            Logging.Current.Debug("SABT: Detecting motors...");

            Connect();

            StartAction(out string action);

            // Detect Assigned Motors
            int detectedMotors = 0;

            foreach (Motor motor in Motors)
            {
                if (motor.Check())
                {
                    detectedMotors++;
                }
            }

            Logging.Current.Info("SABT: Detected " + detectedMotors + " motors");

            if (detectedMotors == 0)
            {
                MessageBox.Show(
                    SLoc.GetValue("SABT_Message_NoAssignedMotorsDetected"),
                    SLoc.GetValue("SABT_Plugin"),
                    MessageBoxButton.OK
                );
            }

            EndAction(action);

            return detectedMotors;
        }

        /// <summary>Identifies system devices that match the expected VID/PID for the controller board (or more specifically, the serial bridge we using on it)</summary>
        /// <returns>A list of <see cref="DeviceInstance" /> instances that match the expected parameters</returns>
        public DeviceInstance[] DetectDevices()
        {
            if (Device != null && Devices?.Length > 0)
            {
                return Devices;
            }

            Logging.Current.Debug("SABT: Detecting devices...");

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
                    string identifier = mo["PNPDeviceID"] as string ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(identifier))
                    {
                        continue;
                    }

                    if (!identifier.Contains(vidPid, StringComparison.OrdinalIgnoreCase))
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
                        Port = match.Groups[1].Value.ToUpperInvariant(),
                        Name = display,
                        Identifier = identifier
                    });
                }
            }

            Devices = devices
                .Where(device => !string.IsNullOrWhiteSpace(device.Port))
                .OrderBy(device => device.Port, StringComparer.OrdinalIgnoreCase)
                .ToArray();




            string selectedPort = Device?.Port;

            if (devices.Count < 1)
            {
                Disconnect();

                Device = null;

                _plugin.Settings.DeviceIdentifier = null;

                return Devices;
            }

            Device = Devices.FirstOrDefault(device => string.Equals(device.Port, selectedPort, StringComparison.OrdinalIgnoreCase))
                ?? Devices.First();

            _plugin.Settings.DeviceIdentifier = _device?.Identifier;

            /*if (
                string.IsNullOrWhiteSpace(_plugin.Settings.SerialPort) ||
                !devices.Contains(_plugin.Settings.SerialPort)
            ) {
                Device = Devices.First();
            }*/

            return Devices;
        }

        public sealed class DeviceInstance
        {
            public string Port { get; set; }
            public string Name { get; set; }
            public string Identifier { get; set; }
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
