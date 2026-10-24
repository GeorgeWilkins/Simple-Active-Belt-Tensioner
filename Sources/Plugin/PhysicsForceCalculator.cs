using SimHub;
using System;
using User.ActiveBeltTensioner.Effects;

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

            //public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        private BeltGeometry _leftWaist;
        private BeltGeometry _rightWaist;

        private DeviceSettings _settings;

        private readonly EffectPipeline _effectPipeline;

        public PhysicsForceCalculator(DeviceSettings settings)
        {
            _effectPipeline = new EffectPipeline(this);

            ApplySettings(settings);
        }

        /// <summary>Applies the given <see cref="DeviceSettings" /> to the relevant internal properties</summary>
        public void ApplySettings(DeviceSettings settings)
        {
            _settings = settings;

            _leftWaist = new BeltGeometry
            {
                DegreesFromVertical = settings.WaistBeltDegreesFromVertical * -1.0,
            };

            _rightWaist = new BeltGeometry
            {
                DegreesFromVertical = settings.WaistBeltDegreesFromVertical,
            };
        }

        /// <summary>Applies force calculations to the given <see cref="TelemetrySnapshot" />, translating acceleration by seat angle and projecting waist belts by their configured angles</summary>
        public void CalculateForcesAndTorques(
            TelemetrySnapshot telemetrySnapshot,
            double millisecondsSinceLastUpdate,
            out double leftShoulderTarget,
            out double rightShoulderTarget,
            out double leftWaistTarget,
            out double rightWaistTarget
        )
        {
            // Apply Idle Tension While In-Menus
            if (!telemetrySnapshot.IsActive)
            {
                double idleTension = _settings.IdleTension / 100.0;

                leftShoulderTarget = idleTension;
                rightShoulderTarget = idleTension;
                leftWaistTarget = idleTension;
                rightWaistTarget = idleTension;

                return;
            }

            // Convert Telemetry To Vehicle Acceleration
            AccelerationVector vehicleAcceleration = new AccelerationVector(
                telemetrySnapshot.Surge,
                telemetrySnapshot.Sway,
                telemetrySnapshot.Heave
            );

            // Normalize Vehicle Acceleration (Before Adapting To Seat Frame Of Reference)
            AccelerationVector normalizedAcceleration = new AccelerationVector(
                NormalizeAcceleration(vehicleAcceleration.X, _settings.MinimumSurge, _settings.MaximumSurge),
                NormalizeAcceleration(vehicleAcceleration.Y, _settings.MinimumSway, _settings.MaximumSway),
                NormalizeAcceleration(vehicleAcceleration.Z, _settings.MinimumHeave, _settings.MaximumHeave)
            );

            // Apply Per-Axis Strength Gains
            AccelerationVector strengthAdjustedAcceleration = ApplyStrengthGains(normalizedAcceleration);

            // Translate To Seat Frame Of Reference & Calculate Belt Tensions
            CalculateBeltTensions(
                TranslateToSeatFrameOfReference(strengthAdjustedAcceleration),
                out leftShoulderTarget,
                out rightShoulderTarget,
                out leftWaistTarget,
                out rightWaistTarget
            );

            // Apply Additional Effects
            BeltTensionTargets beltTensionTargets = new BeltTensionTargets(
                leftShoulderTarget,
                rightShoulderTarget,
                leftWaistTarget,
                rightWaistTarget
            );

            _effectPipeline.Apply(
                _settings,
                telemetrySnapshot,
                millisecondsSinceLastUpdate,
                ref beltTensionTargets
            );

            leftShoulderTarget = beltTensionTargets.LeftShoulder;
            rightShoulderTarget = beltTensionTargets.RightShoulder;
            leftWaistTarget = beltTensionTargets.LeftWaist;
            rightWaistTarget = beltTensionTargets.RightWaist;
        }

        /// <summary>Changes the frame of reference from the vehicle to the driver seat, accounting for its angle relative to the vehicle's vertical axis</summary>
        private AccelerationVector TranslateToSeatFrameOfReference(AccelerationVector acceleration)
        {
            double seatRadiansFromVertical = _settings.SeatRadiansFromVertical;
            double cos = Math.Cos(seatRadiansFromVertical);
            double sin = Math.Sin(seatRadiansFromVertical);

            double rotatedX = acceleration.X * cos - acceleration.Z * sin;
            double rotatedY = acceleration.Y;
            double rotatedZ = acceleration.X * sin + acceleration.Z * cos;

            return new AccelerationVector(rotatedX, rotatedY, rotatedZ);
        }

        /// <summary>Applies configurable per-axis strength gains to normalized acceleration</summary>
        private AccelerationVector ApplyStrengthGains(AccelerationVector acceleration)
        {
            double surgeGain = PercentageToGain(_settings.SurgeStrength);
            double swayGain = PercentageToGain(_settings.SwayStrength);
            double heaveGain = PercentageToGain(_settings.HeaveStrength);

            return new AccelerationVector(
                acceleration.X * surgeGain,
                acceleration.Y * swayGain,
                acceleration.Z * heaveGain
            );
        }

        /// <summary>Converts a 0-200 strength slider to gain where 100 = 1.0x</summary>
        private static double PercentageToGain(double percentage)
        {
            return Clamp(percentage, 0.0, 200.0) / 100.0;
        }

        /// <summary>Determines belt forces to apply based on the given acceleration vector and predefined belt geometry</summary>
        private void CalculateBeltTensions(
            AccelerationVector acceleration,
            out double leftShoulderTension,
            out double rightShoulderTension,
            out double leftWaistTension,
            out double rightWaistTension)
        {
            leftShoulderTension = CalculateShoulderBeltTension(acceleration, -1.0);
            rightShoulderTension = CalculateShoulderBeltTension(acceleration, 1.0);
            leftWaistTension = CalculateWaistBeltTension(acceleration, _leftWaist.RadiansFromVertical);
            rightWaistTension = CalculateWaistBeltTension(acceleration, _rightWaist.RadiansFromVertical);
        }

        /// <summary>Returns the calculated tension for a shoulder belt, based on the given acceleration vector</summary>
        private double CalculateShoulderBeltTension(AccelerationVector acceleration, double sideSign)
        {
            double forceComponent = acceleration.X + (acceleration.Y * sideSign);

            return MapForceComponentToTension(Clamp(forceComponent, -1.0, 1.0));
        }

        /// <summary>Returns the calculated tension for a waist belt, based on the given acceleration vector and belt angle</summary>
        private double CalculateWaistBeltTension(AccelerationVector acceleration, double beltRadiansFromVertical)
        {
            double beltCos = Math.Cos(beltRadiansFromVertical);
            double beltSin = Math.Sin(beltRadiansFromVertical);

            double surgeComponent = acceleration.X * beltCos;
            double swayComponent = 0.0;

            if (Math.Abs(beltSin) > 0.01)
            {
                swayComponent = (acceleration.Y * -beltSin) / Math.Abs(beltSin);
            }

            double forceComponent = surgeComponent + swayComponent;

            return MapForceComponentToTension(Clamp(forceComponent, -1.0, 1.0));
        }

        private double MapForceComponentToTension(double forceComponent)
        {
            // Map To Tension Range
            double minimumTension = _settings.MinimumTension / 100.0;
            double maximumTension = _settings.MaximumTension / 100.0;
            double baseTension = minimumTension;
            double finalTension;

            if (forceComponent > 0)
            {
                // Positive (Minimum ~ Maximum)
                finalTension = baseTension + (forceComponent * (maximumTension - baseTension));
            }
            else
            {
                // Negative (Minimum ~ Zero)
                finalTension = baseTension + (forceComponent * baseTension);
            }

            return Clamp(finalTension, 0.0, maximumTension);
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
