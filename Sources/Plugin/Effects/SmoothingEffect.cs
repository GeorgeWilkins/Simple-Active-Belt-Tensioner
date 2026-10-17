using User.ActiveBeltTensioner;

namespace User.ActiveBeltTensioner.Effects
{
    internal sealed class SmoothingEffect : IEffect
    {
        private double _leftShoulder;
        private double _rightShoulder;
        private double _leftWaist;
        private double _rightWaist;

        public int Order => 1;

        public void Apply(DeviceSettings settings, TelemetrySnapshot telemetrySnapshot, double millisecondsSinceLastUpdate, ref BeltTensionTargets beltTensionTargets)
        {
            if (settings == null)
            {
                return;
            }

            double smoothingFactor = settings.SmoothingFactor / 100.0;

            _leftShoulder = ApplySmoothing(_leftShoulder, beltTensionTargets.LeftShoulder, smoothingFactor);
            _rightShoulder = ApplySmoothing(_rightShoulder, beltTensionTargets.RightShoulder, smoothingFactor);
            _leftWaist = ApplySmoothing(_leftWaist, beltTensionTargets.LeftWaist, smoothingFactor);
            _rightWaist = ApplySmoothing(_rightWaist, beltTensionTargets.RightWaist, smoothingFactor);

            beltTensionTargets.LeftShoulder = _leftShoulder;
            beltTensionTargets.RightShoulder = _rightShoulder;
            beltTensionTargets.LeftWaist = _leftWaist;
            beltTensionTargets.RightWaist = _rightWaist;
        }

        private static double ApplySmoothing(double previous, double current, double smoothingFactor)
        {
            return (current * (1.0 - smoothingFactor)) + (previous * smoothingFactor);
        }
    }
}
