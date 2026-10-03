using SimHub;
using System;

namespace User.ActiveBeltTensioner
{
    /// <summary>Physics-based force calculator that models driver and vehicle body dynamic to belt tension output</summary>
    public class PhysicsForceCalculator
    {
        /// <summary>A representation of the physical configuration of a belt</summary>
        public struct BeltGeometry
        {
            /// <summary>TODO</summary>
            public double DistanceFromCenter { get; set; }

            /// <summary>The belt angle relative to the vehicle (with directly vertical from floor to roof being 0 degrees)</summary>
            public double DegreesFromVertical { get; set; }

            /// <summary>The <see cref="DegreesFromVertical" /> value in radians</summary>
            public double RadiansFromVertical => DegreesFromVertical * Math.PI / 180.0;
        }

        /// <summary>A representation of the physical configuration of the driver and seat</summary>
        public struct DriverParameters
        {
            /// <summary>TODO</summary>
            public double Mass { get; set; }

            /// <summary>TODO (0-1)</summary>
            public double DampingFactor { get; set; }

            /// <summary>The seat back angle relative to the vehicle (with directly vertical from floor to roof being 0 degrees)</summary>
            public double DegreesFromVertical { get; set; }

            /// <summary>The <see cref="DegreesFromVertical" /> value in radians</summary>
            public double RadiansFromVertical => DegreesFromVertical * Math.PI / 180.0;
        }

        /// <summary>A representation of a three-dimensional acceleration vector</summary>
        private struct AccelerationVector
        {
            public double X { get; set; } // Surge
            public double Y { get; set; } // Sway
            public double Z { get; set; } // Heave

            public AccelerationVector(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public static AccelerationVector operator +(AccelerationVector a, AccelerationVector b)
                => new AccelerationVector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

            public static AccelerationVector operator -(AccelerationVector a, AccelerationVector b)
                => new AccelerationVector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

            public static AccelerationVector operator *(AccelerationVector a, double scalar)
                => new AccelerationVector(a.X * scalar, a.Y * scalar, a.Z * scalar);

            public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        private DriverParameters _driverParameters;
        private BeltGeometry _leftShoulderBelt;
        private BeltGeometry _rightShoulderBelt;
        private BeltGeometry _leftWaistBelt;
        private BeltGeometry _rightWaistBelt;

        private AccelerationVector _driverAcceleration = new AccelerationVector(0, 0, 0);

        private double _minimumTension = 0.0;
        private double _maximumTension = 1.0;
        private double _idleTension = 0.1;
        private double _surgeMinimum = -50.0;
        private double _surgeMaximum = 50.0;
        private double _swayMinimum = -50.0;
        private double _swayMaximum = 50.0;
        private double _heaveMinimum = -50.0;
        private double _heaveMaximum = 50.0;
        private double _smoothingFactor = 0.1;
        private double _horizontalBias = 0.0;

        public PhysicsForceCalculator(DeviceSettings settings)
        {
            ApplySettings(settings);
        }

        /// <summary>Applies the given <see cref="DeviceSettings" /> to the relevant internal properties</summary>
        public void ApplySettings(DeviceSettings settings)
        {
            // Physical Configuration
            _driverParameters = new DriverParameters
            {
                Mass = settings.DriverMass,
                DampingFactor = settings.DriverDamping,
                DegreesFromVertical = settings.SeatDegreesFromVertical
            };
            _leftShoulderBelt = new BeltGeometry
            {
                DegreesFromVertical = settings.LeftShoulderBeltDegreesFromVertical,
                DistanceFromCenter = settings.LeftShoulderBeltDistance
            };
            _rightShoulderBelt = new BeltGeometry
            {
                DegreesFromVertical = settings.RightShoulderBeltDegreesFromVertical,
                DistanceFromCenter = settings.RightShoulderBeltDistance
            };
            _leftWaistBelt = new BeltGeometry
            {
                DegreesFromVertical = settings.LeftWaistBeltDegreesFromVertical,
                DistanceFromCenter = settings.LeftWaistBeltDistance
            };
            _rightWaistBelt = new BeltGeometry
            {
                DegreesFromVertical = settings.RightWaistBeltDegreesFromVertical,
                DistanceFromCenter = settings.RightWaistBeltDistance
            };

            // Telemetry Mapping
            _surgeMinimum = settings.MinimumSurge;
            _surgeMaximum = settings.MaximumSurge;
            _swayMinimum = settings.MinimumSway;
            _swayMaximum = settings.MaximumSway;
            _heaveMinimum = settings.MinimumHeave;
            _heaveMaximum = settings.MaximumHeave;

            // General Preferences
            _idleTension = settings.IdleTension / 100.0;
            _minimumTension = settings.MinimumTension / 100.0;
            _maximumTension = settings.MaximumTension / 100.0;
            _smoothingFactor = settings.SmoothingFactor / 100.0;
            _horizontalBias = settings.HorizontalBias / 100.0;
        }

        /// <summary>Applies force calculations to the given <see cref="DevicePlugin.TelemetrySnapshot" /> in reference to the predefined <see cref="DriverParameters" /> and <see cref="BeltGeometry" /> instances, outputting per-belt tension values</summary>
        public void CalculateForcesAndTorques(
            DevicePlugin.TelemetrySnapshot telemetrySnapshot,
            double deltaTimeMsec,
            out double leftShoulderTarget,
            out double rightShoulderTarget,
            out double leftWaistTarget,
            out double rightWaistTarget)
        {
            // TODO: Apply idle tension only when not moving
            /*if (!isMoving)
            {
                leftShoulderTarget = idleTension;
                rightShoulderTarget = idleTension;
                leftWaistTarget = idleTension;
                rightWaistTarget = idleTension;
            }*/

            // TODO: Apply horizontal bias to sway axis (left/right) to account for driver asymmetry



            // Convert Telemetry To Vehicle Acceleration
            AccelerationVector vehicleAcceleration = new AccelerationVector(
                telemetrySnapshot.Surge ?? 0.0,
                telemetrySnapshot.Sway ?? 0.0,
                telemetrySnapshot.Heave ?? 0.0
            );

            // Apply To Driver Acceleration
            UpdateDriverAcceleration(vehicleAcceleration, deltaTimeMsec);

            // Normalize Driver Acceleration (Before Adapting To Seat Frame Of Reference)
            AccelerationVector normalizedAcceleration = new AccelerationVector(
                NormalizeAcceleration(_driverAcceleration.X, _surgeMinimum, _surgeMaximum),
                NormalizeAcceleration(_driverAcceleration.Y, _swayMinimum, _swayMaximum),
                NormalizeAcceleration(_driverAcceleration.Z, _heaveMinimum, _heaveMaximum)
            );

            // Translate To Seat Frame Of Reference & Calculate Belt Tensions
            CalculateBeltTensions(
                TranslateToSeatFrameOfReference(normalizedAcceleration),
                out leftShoulderTarget,
                out rightShoulderTarget,
                out leftWaistTarget,
                out rightWaistTarget
            );
        }

        /// <summary>Applies the given vehicle acceleration to the driver acceleration instance, subject to the damping factor and given time delta</summary>
        /// <remarks>This is intended to simulate the delay in driver body response to vehicle acceleration, but may be removed in favour of minimizing latency</remarks>
        private void UpdateDriverAcceleration(AccelerationVector vehicleAcceleration, double millisecondsSinceLastUpdate)
        {
            double secondsSinceLastUpdate = millisecondsSinceLastUpdate / 1000.0;
            if (secondsSinceLastUpdate <= 0 || secondsSinceLastUpdate > 0.1)
            {
                // Ignore Invalid Intervals
                return;
            }

            // Define Acceleration Latency
            double timeConstant = 0.05 + (_driverParameters.DampingFactor * 0.15);  // 0.05 - 0.2 seconds

            // Exponential Approach Factor
            double approachFactor = 1.0 - Math.Exp(-secondsSinceLastUpdate / (timeConstant * _smoothingFactor));

            // Apply To Each Axis
            _driverAcceleration.X = _driverAcceleration.X + (vehicleAcceleration.X - _driverAcceleration.X) * approachFactor;
            _driverAcceleration.Y = _driverAcceleration.Y + (vehicleAcceleration.Y - _driverAcceleration.Y) * approachFactor;
            _driverAcceleration.Z = _driverAcceleration.Z + (vehicleAcceleration.Z - _driverAcceleration.Z) * approachFactor;
        }

        /// <summary>Changes the frame of reference from the vehicle to the driver seat, accounting for its angle relative to the vehicle's vertical axis</summary>
        private AccelerationVector TranslateToSeatFrameOfReference(AccelerationVector acceleration)
        {
            double cos = Math.Cos(_driverParameters.RadiansFromVertical);
            double sin = Math.Sin(_driverParameters.RadiansFromVertical);

            double rotatedX = acceleration.X * cos - acceleration.Y * sin;
            double rotatedY = acceleration.X * sin + acceleration.Y * cos;
            double rotatedZ = acceleration.Z;

            return new AccelerationVector(rotatedX, rotatedY, rotatedZ);
        }

        /// <summary>Determines belt forces to apply based on the given acceleration vector and predefined belt geometry</summary>
        private void CalculateBeltTensions(
            AccelerationVector acceleration,
            out double leftShoulderTension,
            out double rightShoulderTension,
            out double leftWaistTension,
            out double rightWaistTension)
        {
            leftShoulderTension = CalculateBeltTension(acceleration, _leftShoulderBelt);
            rightShoulderTension = CalculateBeltTension(acceleration, _rightShoulderBelt);
            leftWaistTension = CalculateBeltTension(acceleration, _leftWaistBelt);
            rightWaistTension = CalculateBeltTension(acceleration, _rightWaistBelt);
        }

        /// <summary>Returns the calculated tension value for a single belt based on the given normalized acceleration vector and belt geometry</summary>
        private double CalculateBeltTension(AccelerationVector acceleration, BeltGeometry belt)
        {
            double beltRadiansFromVertical = belt.RadiansFromVertical;
            double beltDistanceFromVertical = Clamp(belt.DistanceFromCenter, 0.0, 1.0); // @TODO: Make Absolute

            // Already Reframed To Seat & Normalized To -1.0 to +1.0
            double seatFramedSurge = acceleration.X;
            double seatFramedSway = acceleration.Y;
            double seatFramedHeave = acceleration.Z;

            // Calculate Force Component (Surge * cos(θ) + Sway * sin(θ) + Heave * Belt Distance)
            double forceComponent = (seatFramedSurge * Math.Cos(beltRadiansFromVertical)) + (seatFramedSway * Math.Sin(beltRadiansFromVertical));

            // Apply Extra Heave Per Distance (TODO: Incorrect)
            forceComponent += seatFramedHeave * beltDistanceFromVertical * 0.5;
            forceComponent = Clamp(forceComponent, -1.0, 1.0);

            // Map To Tension Range
            double baseTension = _idleTension;
            double finalTension;

            if (forceComponent > 0)
            {
                // Positive (Idle ~ Maximum)
                finalTension = baseTension + (forceComponent * (_maximumTension - baseTension));
            }
            else
            {
                // Negative (Idle ~ Minimum)
                finalTension = baseTension + (forceComponent * (baseTension - _minimumTension));
            }

            return Clamp(finalTension, _minimumTension, _maximumTension);
        }

        /// <summary>Normalizes the given acceleration value to -1.0 to +1.0, relative to the given range</summary>
        private static double NormalizeAcceleration(double value, double minimum, double maximum)
        {
            if (value >= 0)
            {
                return (maximum > 0)
                    ? Clamp(value / maximum, 0.0, 1.0)
                    : 0.0;
            }
            else
            {
                return (minimum < 0)
                    ? Clamp(value / Math.Abs(minimum), -1.0, 0.0)
                    : 0.0;
            }
        }

        /// <summary>Clamps the given value to the given minimum and maximum values</summary>
        private static double Clamp(double value, double minimum, double maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }
    }
}
