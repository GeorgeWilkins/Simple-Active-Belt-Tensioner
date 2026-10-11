namespace User.ActiveBeltTensioner.Effects
{
    internal interface IEffect
    {
        void Apply(DevicePlugin.TelemetrySnapshot telemetrySnapshot, double millisecondsSinceLastUpdate, ref BeltTensionTargets beltTensionTargets);
    }
}
