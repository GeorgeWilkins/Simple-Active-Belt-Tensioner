using System;
using User.ActiveBeltTensioner;

namespace User.ActiveBeltTensioner.Effects
{
    internal sealed class SideBiasEffect : IEffect
    {
        private readonly PhysicsForceCalculator _calculator;

        public SideBiasEffect(PhysicsForceCalculator calculator)
        {
            _calculator = calculator;
        }

        public void Apply(DevicePlugin.TelemetrySnapshot telemetrySnapshot, double millisecondsSinceLastUpdate, ref BeltTensionTargets beltTensionTargets)
        {
            if (!telemetrySnapshot.IsActive)
            {
                return;
            }

            var settings = _calculator.Settings;
            if (settings == null)
            {
                return;
            }

            double horizontalBias = settings.HorizontalBias / 100.0;

            if (horizontalBias < 0.0)
            {
                double biasFactor = 1.0 - Math.Abs(horizontalBias);

                beltTensionTargets.RightShoulder *= biasFactor;
                beltTensionTargets.RightWaist *= biasFactor;
            }
            else if (horizontalBias > 0.0)
            {
                double biasFactor = 1.0 - horizontalBias;

                beltTensionTargets.LeftShoulder *= biasFactor;
                beltTensionTargets.LeftWaist *= biasFactor;
            }
        }
    }
}
