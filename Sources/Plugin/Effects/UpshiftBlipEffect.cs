using System;
using System.Diagnostics;
using User.ActiveBeltTensioner;

namespace User.ActiveBeltTensioner.Effects
{
    internal sealed class UpshiftBlipEffect : IEffect
    {
        private readonly Stopwatch _upshiftStopwatch = new Stopwatch();
        private const double _neutralPeriod = 100.0;
        private double _neutralDuration;
        private short? _previousGear;
        private bool _inNeutral;

        public int Order => 20;

        public void Apply(DeviceSettings settings, TelemetrySnapshot telemetrySnapshot, double millisecondsSinceLastUpdate, ref BeltTensionTargets beltTensionTargets)
        {
            if (settings == null || !telemetrySnapshot.IsActive)
            {
                Reset();
                return;
            }

            double maximumTension = settings.MaximumTension / 100.0;
            double upshiftingStrength = settings.UpshiftingStrength / 100.0;
            var modifierCurve = settings.ParsedUpshiftingModifiers;

            if (upshiftingStrength <= 0.0 || modifierCurve == null || modifierCurve.Length == 0)
            {
                Reset();
                return;
            }

            short currentGear = telemetrySnapshot.Gear;
            if (_inNeutral)
            {
                _neutralDuration += Math.Max(0.0, millisecondsSinceLastUpdate);
                if (_neutralDuration > _neutralPeriod)
                {
                    _previousGear = null;
                }
            }

            if (currentGear == 0)
            {
                if (!_inNeutral)
                {
                    _inNeutral = true;
                    _neutralDuration = 0.0;
                    if (_previousGear <= 0)
                    {
                        _previousGear = null;
                    }
                }
            }
            else
            {
                if (_previousGear.HasValue && currentGear > _previousGear.Value && (!_inNeutral || currentGear > 0))
                {
                    _upshiftStopwatch.Restart();
                }

                _previousGear = currentGear;
                _inNeutral = false;
            }

            if (!_upshiftStopwatch.IsRunning)
            {
                return;
            }

            double upshiftDuration = _upshiftStopwatch.Elapsed.TotalMilliseconds;

            if (upshiftDuration >= modifierCurve[modifierCurve.Length - 1].timeOffset)
            {
                _upshiftStopwatch.Stop();

                return;
            }

            double upshiftModifier = GetUpshiftModifier(upshiftDuration, modifierCurve) * upshiftingStrength;

            if (upshiftModifier > 0.0)
            {
                ApplyTowardsMaximum(ref beltTensionTargets.LeftShoulder, upshiftModifier, maximumTension);
                ApplyTowardsMaximum(ref beltTensionTargets.RightShoulder, upshiftModifier, maximumTension);
                ApplyTowardsMaximum(ref beltTensionTargets.LeftWaist, upshiftModifier, maximumTension);
                ApplyTowardsMaximum(ref beltTensionTargets.RightWaist, upshiftModifier, maximumTension);
            }
            else if (upshiftModifier < 0.0)
            {
                ApplyTowardsZero(ref beltTensionTargets.LeftShoulder, upshiftModifier);
                ApplyTowardsZero(ref beltTensionTargets.RightShoulder, upshiftModifier);
                ApplyTowardsZero(ref beltTensionTargets.LeftWaist, upshiftModifier);
                ApplyTowardsZero(ref beltTensionTargets.RightWaist, upshiftModifier);
            }
        }

        private void Reset()
        {
            _upshiftStopwatch.Reset();
            _previousGear = null;
            _inNeutral = false;
            _neutralDuration = 0.0;
        }

        private static double GetUpshiftModifier(double upshiftOffset, (int timeOffset, int tensionModifier)[] modifierCurve)
        {
            if (upshiftOffset <= modifierCurve[0].timeOffset)
            {
                return modifierCurve[0].tensionModifier / 100.0;
            }

            if (upshiftOffset >= modifierCurve[modifierCurve.Length - 1].timeOffset)
            {
                return 0.0;
            }

            for (int i = 0; i < modifierCurve.Length - 1; i++)
            {
                double fromTime = modifierCurve[i].timeOffset;
                double fromValue = modifierCurve[i].tensionModifier / 100.0;
                double toTime = modifierCurve[i + 1].timeOffset;
                double toValue = modifierCurve[i + 1].tensionModifier / 100.0;

                if (upshiftOffset >= fromTime && upshiftOffset < toTime)
                {
                    double t = (upshiftOffset - fromTime) / (toTime - fromTime);

                    return fromValue + t * (toValue - fromValue);
                }
            }

            return 0.0;
        }

        private static void ApplyTowardsMaximum(ref double target, double modifier, double maximumTension)
        {
            target += modifier * (maximumTension - target);
        }

        private static void ApplyTowardsZero(ref double target, double modifier)
        {
            target += modifier * target;
        }
    }
}
