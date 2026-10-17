using System;
using User.ActiveBeltTensioner;

namespace User.ActiveBeltTensioner.Effects
{
    internal sealed class EngineOscillationEffect : IEffect
    {
        private const double FullCycleRadians = Math.PI * 2.0;

        private double _oscillationPhaseRadians;

        public int Order => 100;

        public void Apply(DeviceSettings settings, TelemetrySnapshot telemetrySnapshot, double millisecondsSinceLastUpdate, ref BeltTensionTargets beltTensionTargets)
        {
            return; // @TODO: NOT WORKING WELL ENOUGH

            if (settings == null)
            {
                _oscillationPhaseRadians = 0.0;

                return;
            }

            double maximumTension = settings.MaximumTension / 100.0;
            double engineStrength = settings.EngineStrength / 100.0;
            double revolutions = telemetrySnapshot.Revolutions;

            if (!telemetrySnapshot.IsActive || revolutions <= 0.0 || engineStrength <= 0.0)
            {
                _oscillationPhaseRadians = 0.0;
                return;
            }

            double deltaTimeSeconds = Math.Max(0.0, millisecondsSinceLastUpdate) / 1000.0;
            double oscillationFrequencyHz = revolutions / 60.0;

            _oscillationPhaseRadians += FullCycleRadians * oscillationFrequencyHz * deltaTimeSeconds;

            if (_oscillationPhaseRadians >= FullCycleRadians)
            {
                _oscillationPhaseRadians %= FullCycleRadians;
            }

            int oscillationDirection = (Math.Sin(_oscillationPhaseRadians) >= 0.0) ? 1 : -1;

            double revolutionsFraction = ConvertToFractionOfRange(revolutions, 0.0, 5000.0);
            double adjustedEngineStrength = (1.0 - revolutionsFraction) * engineStrength * 0.5;

            if (oscillationDirection > 0)
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
