using MahApps.Metro.IconPacks;
using SimHub;
using System;
using System.ComponentModel;
using System.Runtime;
using System.Windows;
using System.Windows.Threading;

namespace User.ActiveBeltTensioner
{
    public class DeviceSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private const string _deviceNotFound = "No Device Found";

        private void InvokePropertyChange(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private PackIconMaterialKind _leftMotorIcon = PackIconMaterialKind.AlertCircleOutline;
        public PackIconMaterialKind LeftMotorIcon
        {
            get { return _leftMotorIcon; }
            set
            {
                if (_leftMotorIcon != value)
                {
                    _leftMotorIcon = value;

                    InvokePropertyChange(nameof(LeftMotorIcon));
                }
            }
        }

        private PackIconMaterialKind _rightMotorIcon = PackIconMaterialKind.AlertCircleOutline;
        public PackIconMaterialKind RightMotorIcon
        {
            get { return _rightMotorIcon; }
            set
            {
                if (_rightMotorIcon != value)
                {
                    _rightMotorIcon = value;

                    InvokePropertyChange(nameof(RightMotorIcon));
                }
            }
        }

        private string _leftMotorStatus = "";
        public string LeftMotorStatus
        {
            get { return _leftMotorStatus; }
            set
            {
                if (_leftMotorStatus != value)
                {
                    _leftMotorStatus = value;

                    InvokePropertyChange(nameof(LeftMotorStatus));
                }
            }
        }

        private string _rightMotorStatus = "";
        public string RightMotorStatus
        {
            get { return _rightMotorStatus; }
            set
            {
                if (_rightMotorStatus != value)
                {
                    _rightMotorStatus = value;

                    InvokePropertyChange(nameof(RightMotorStatus));
                }
            }
        }

        private string[] _serialPorts = new string[0];
        public string[] SerialPorts
        {
            get { return _serialPorts; }
            set
            {
                if (!ReferenceEquals(_serialPorts, value))
                {
                    _serialPorts = value ?? new string[0];
                    InvokePropertyChange(nameof(SerialPorts));
                }
            }
        }

        private string _serialPort = _deviceNotFound;
        public string SerialPort
        {
            get { return _serialPort; }
            set
            {
                if (_serialPort != value)
                {
                    _serialPort = value ?? _deviceNotFound;
                    InvokePropertyChange(nameof(SerialPort));
                    InvokePropertyChange(nameof(IsSerialPortValid));
                    InvokePropertyChange(nameof(IsAvailable));
                }
            }
        }

        private bool _isEnabled = false;
        public bool IsEnabled
        {
            get { return _isEnabled; } // @TODO: Turn off when there is no connection?
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    InvokePropertyChange(nameof(IsEnabled));
                    InvokePropertyChange(nameof(IsAvailable));
                }
            }
        }

        private bool _isFlipped = false;
        public bool IsFlipped
        {
            get { return _isFlipped; }
            set
            {
                if (_isFlipped != value)
                {
                    _isFlipped = value;
                    InvokePropertyChange(nameof(IsFlipped));
                }
            }
        }

        private int _idleTension = 150;
        public int IdleTension
        {
            get { return _idleTension; }
            set
            {
                if (_idleTension != value)
                {
                    _idleTension = value;
                    InvokePropertyChange(nameof(IdleTension));
                }
            }
        }

        private int _minimumTension = 200;
        public int MinimumTension
        {
            get { return _minimumTension; }
            set
            {
                if (_minimumTension != value)
                {
                    _minimumTension = Math.Min(value, _maximumTension);
                    InvokePropertyChange(nameof(MinimumTension));
                    InvokePropertyChange(nameof(IsMinimumTensionNonZero));
                }
            }
        }

        private int _maximumTension = 1000;
        public int MaximumTension
        {
            get { return _maximumTension; }
            set
            {
                if (_maximumTension != value)
                {
                    _maximumTension = Math.Max(value, _minimumTension);
                    InvokePropertyChange(nameof(MaximumTension));
                }
            }
        }

        private int _minimumSurge = -10;
        public int MinimumSurge
        {
            get { return _minimumSurge; }
            set
            {
                if (_minimumSurge != value)
                {
                    _minimumSurge = Math.Min(value, _maximumSurge);
                    InvokePropertyChange(nameof(MinimumSurge));
                }
            }
        }

        private int _maximumSurge = 20;
        public int MaximumSurge
        {
            get { return _maximumSurge; }
            set
            {
                if (_maximumSurge != value)
                {
                    _maximumSurge = Math.Max(value, _minimumSurge);
                    InvokePropertyChange(nameof(MaximumSurge));
                }
            }
        }

        private int _minimumSway = -17;
        public int MinimumSway
        {
            get { return _minimumSway; }
            set
            {
                if (_minimumSway != value)
                {
                    _minimumSway = Math.Min(value, _maximumSway);
                    InvokePropertyChange(nameof(MinimumSway));

                }
            }
        }

        private int _maximumSway = 17;
        public int MaximumSway
        {
            get { return _maximumSway; }
            set
            {
                if (_maximumSway != value)
                {
                    _maximumSway = Math.Max(value, _minimumSway);
                    InvokePropertyChange(nameof(MaximumSway));

                }
            }
        }

        private int _minimumHeave = -25;
        public int MinimumHeave
        {
            get { return _minimumHeave; }
            set
            {
                if (_minimumHeave != value)
                {
                    _minimumHeave = Math.Min(value, _maximumHeave);
                    InvokePropertyChange(nameof(MinimumHeave));
                }
            }
        }

        private int _maximumHeave = 110;
        public int MaximumHeave
        {
            get { return _maximumHeave; }
            set
            {
                if (_maximumHeave != value)
                {
                    _maximumHeave = Math.Max(value, _minimumHeave);
                    InvokePropertyChange(nameof(MaximumHeave));
                }
            }
        }

        private int _sideBias = 0;
        public int SideBias
        {
            get { return _sideBias; }
            set {
                if (_sideBias != value)
                {
                    _sideBias = value;
                    InvokePropertyChange(nameof(SideBias));
                }
            }
        }

        private int _corneringStrength = 1000;
        public int CorneringStrength
        {
            get { return _corneringStrength; }
            set
            {
                if (_corneringStrength != value)
                {
                    _corneringStrength = value;
                    InvokePropertyChange(nameof(CorneringStrength));
                }
            }
        }

        private int _accelerationStrength = 1000;
        public int AccelerationStrength
        {
            get { return _accelerationStrength; }
            set
            {
                if (_accelerationStrength != value)
                {
                    _accelerationStrength = value;
                    InvokePropertyChange(nameof(AccelerationStrength));
                }
            }
        }

        private int _brakingStrength = 1000;
        public int BrakingStrength
        {
            get { return _brakingStrength; }
            set
            {
                if (_brakingStrength != value)
                {
                    _brakingStrength = value;
                    InvokePropertyChange(nameof(BrakingStrength));
                }
            }
        }

        private int _jumpingStrength = 1000;
        public int JumpingStrength
        {
            get { return _jumpingStrength; }
            set
            {
                if (_jumpingStrength != value)
                {
                    _jumpingStrength = value;
                    InvokePropertyChange(nameof(JumpingStrength));
                }
            }
        }

        private int _landingStrength = 1000;
        public int LandingStrength
        {
            get { return _landingStrength; }
            set
            {
                if (_landingStrength != value)
                {
                    _landingStrength = value;
                    InvokePropertyChange(nameof(LandingStrength));
                }
            }
        }

        private int _shiftingStrength = 0;
        public int ShiftingStrength
        {
            get { return _shiftingStrength; }
            set
            {
                if (_shiftingStrength != value)
                {
                    _shiftingStrength = value;
                    InvokePropertyChange(nameof(ShiftingStrength));
                }
            }
        }





        public bool IsMinimumTensionNonZero
        {
            get { return MinimumTension > 0; }
        }

        public bool IsSerialPortValid
        {
            get { return !String.IsNullOrEmpty(_serialPort) && (_serialPort != _deviceNotFound); }
        }

        public bool IsAvailable
        {
            get { return IsSerialPortValid && IsEnabled; }
        }
    }
}