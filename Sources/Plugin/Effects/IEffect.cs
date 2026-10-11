namespace User.ActiveBeltTensioner.Effects
{
    internal interface IEffect
    {
        int Order { get; }

        void Apply(DeviceSettings settings, DevicePlugin.TelemetrySnapshot telemetrySnapshot, double millisecondsSinceLastUpdate, ref BeltTensionTargets beltTensionTargets);
    }
}
