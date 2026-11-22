namespace User.ActiveBeltTensioner.Effects
{
    /// <summary>Defines an effect that is applied on top of standard physics-based output</summary>
    internal interface IEffect
    {
        int Order { get; }

        void Apply(DeviceSettings settings, TelemetrySnapshot telemetrySnapshot, double millisecondsSinceLastUpdate, ref BeltTensionTargets beltTensionTargets);
    }
}
