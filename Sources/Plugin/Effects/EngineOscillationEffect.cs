using System;
using System.Diagnostics;
using User.ActiveBeltTensioner;

namespace User.ActiveBeltTensioner.Effects
{
    internal sealed class EngineOscillationEffect : IEffect
    {
        private readonly Stopwatch _oscillationStopwatch = new Stopwatch();
        private int _oscillationDirection = 1;
        private readonly PhysicsForceCalculator _calculator;

        public EngineOscillationEffect(PhysicsForceCalculator calculator)
        {
            _calculator = calculator;
        }

        public void Apply(DevicePlugin.TelemetrySnapshot telemetrySnapshot, double millisecondsSinceLastUpdate, ref BeltTensionTargets beltTensionTargets)
        {
            DeviceSettings settings = _calculator.Settings;

            if (settings == null)
            {
                _oscillationStopwatch.Reset();
                _oscillationDirection = 1;

                return;
            }

            double maximumTension = settings.MaximumTension / 100.0;
            double engineStrength = settings.EngineStrength / 100.0;
            double revolutions = telemetrySnapshot.Revolutions ?? 0.0;

            if (!telemetrySnapshot.IsActive || revolutions <= 0.0 || engineStrength <= 0.0)
            {
                _oscillationStopwatch.Reset();
                _oscillationDirection = 1;
                return;
            }

            if (!_oscillationStopwatch.IsRunning)
            {
                _oscillationStopwatch.Restart();
            }

            double oscillationIntervalSeconds = 30.0 / revolutions;

            if (_oscillationStopwatch.Elapsed.TotalSeconds >= oscillationIntervalSeconds)
            {
                _oscillationDirection *= -1;
                _oscillationStopwatch.Restart();
            }

            double revolutionsFraction = ConvertToFractionOfRange(revolutions, 0.0, 5000.0);
            double adjustedEngineStrength = (1.0 - revolutionsFraction) * engineStrength * 0.5;

            if (_oscillationDirection > 0)
            {
                ApplyEngineBias(ref beltTensionTargets.LeftShoulder, ref beltTensionTargets.RightShoulder, adjustedEngineStrength, maximumTension);
                ApplyEngineBias(ref beltTensionTargets.LeftWaist, ref beltTensionTargets.RightWaist, adjustedEngineStrength, maximumTension);
            }
            else
            {
                ApplyEngineBias(ref beltTensionTargets.RightShoulder, ref beltTensionTargets.LeftShoulder, adjustedEngineStrength, maximumTension);
                ApplyEngineBias(ref beltTensionTargets.RightWaist, ref beltTensionTargets.LeftWaist, adjustedEngineStrength, maximumTension);
            }
        }

        private static void ApplyEngineBias(ref double positiveSideTarget, ref double negativeSideTarget, double adjustedEngineStrength, double maximumTension)
        {
            positiveSideTarget = ClampTo(positiveSideTarget + (adjustedEngineStrength * (maximumTension - positiveSideTarget)), 0.0, maximumTension);
            negativeSideTarget = ClampTo(negativeSideTarget - (adjustedEngineStrength * negativeSideTarget), 0.0, maximumTension);
        }

        private static double ConvertToFractionOfRange(double value, double min, double max)
        {
            value = ClampTo(value, min, max);
            return (value - min) / (max - min);
        }

        private static double ClampTo(double value, double min, double max)
        {
            if (value < min) { return min; }
            if (value > max) { return max; }
            return value;
        }
    }
}
