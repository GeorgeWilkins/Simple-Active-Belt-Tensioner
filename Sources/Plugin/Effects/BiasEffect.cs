using System;
using User.ActiveBeltTensioner;

namespace User.ActiveBeltTensioner.Effects
{
    internal sealed class BiasEffect : IEffect
    {
        public int Order => 10;

        public void Apply(DeviceSettings settings, TelemetrySnapshot telemetrySnapshot, double millisecondsSinceLastUpdate, ref BeltTensionTargets beltTensionTargets)
        {
            if (settings == null || !telemetrySnapshot.IsActive)
            {
                return;
            }

            double leftStrength = settings.LeftStrength / 100.0;
            double rightStrength = settings.RightStrength / 100.0;
            double waistStrength = settings.WaistStrength / 100.0;
            double shoulderStrength = settings.ShoulderStrength / 100.0;

            beltTensionTargets.LeftShoulder *= leftStrength * shoulderStrength;
            beltTensionTargets.RightShoulder *= rightStrength * shoulderStrength;
            beltTensionTargets.LeftWaist *= leftStrength * waistStrength;
            beltTensionTargets.RightWaist *= rightStrength * waistStrength;
        }
    }
}
