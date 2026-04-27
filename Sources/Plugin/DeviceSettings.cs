using MahApps.Metro.IconPacks;
using SimHub;
using System;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace User.ActiveBeltTensioner
{
    public class DeviceSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private const string _deviceNotFound = "No Device Found";

        private void InvokePropertyChange([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
                }
            }
        }

        private bool _isEnabled = false;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    InvokePropertyChange(nameof(IsEnabled));
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

        private int _minimumSurge = -9;
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

        private int _maximumSurge = 22;
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

        private int _minimumSway = -23;
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

        private int _maximumSway = 23;
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

        private int _smoothingFactor = 300;
        public int SmoothingFactor
        {
            get { return _smoothingFactor; }
            set
            {
                if (_smoothingFactor != value)
                {
                    _smoothingFactor = value;
                    InvokePropertyChange(nameof(SmoothingFactor));
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
    }
}