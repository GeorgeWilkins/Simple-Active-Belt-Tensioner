namespace User.ActiveBeltTensioner.Normalizers
{
    /// <summary>Defines an input normalizer that resolves game-specific problems with their telemetry data</summary>
    internal interface INormalizer
    {
        bool Normalize(ref TelemetrySnapshot telemetrySnapshot);
    }
}
