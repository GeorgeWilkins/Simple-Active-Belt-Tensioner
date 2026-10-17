using System;
using User.ActiveBeltTensioner;

namespace User.ActiveBeltTensioner.Effects
{
    internal sealed class SideBiasEffect : IEffect
    {
        public int Order => 10;

        public void Apply(DeviceSettings settings, TelemetrySnapshot telemetrySnapshot, double millisecondsSinceLastUpdate, ref BeltTensionTargets beltTensionTargets)
        {
            if (settings == null || !telemetrySnapshot.IsActive)
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
