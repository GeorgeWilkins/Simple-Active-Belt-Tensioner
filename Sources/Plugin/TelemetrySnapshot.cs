using GameReaderCommon;
using System;
using User.ActiveBeltTensioner.Normalizers;

namespace User.ActiveBeltTensioner
{
    public struct TelemetrySnapshot
    {
        public double Surge;
        public double Sway;
        public double Heave;
        public double Speed;
        public double Revolutions;
        public short Gear;
        public bool IsActive;
        public string Game;

        public static TelemetrySnapshot Create(GameData data)
        {
            if (data == null)
            {
                return new TelemetrySnapshot
                {
                    Surge = 0.0,
                    Sway = 0.0,
                    Heave = 0.0,
                    Speed = 0.0,
                    Revolutions = 0.0,
                    Gear = 0,
                    IsActive = false,
                    Game = string.Empty,
                };
            }

            short gear = 0;

            Int16.TryParse(data.NewData?.Gear, out gear);

            TelemetrySnapshot telemetrySnapshot = new TelemetrySnapshot
            {
                Surge = data.NewData?.AccelerationSurge ?? 0.0,
                Sway = data.NewData?.AccelerationSway ?? 0.0,
                Heave = data.NewData?.AccelerationHeave ?? 0.0,
                Speed = data.NewData?.SpeedKmh ?? 0.0,
                Revolutions = data.NewData?.Rpms ?? 0.0,
                Gear = gear,
                IsActive = (data.GameRunning && !data.GameInMenu && !data.GamePaused) || data.GameReplay,
                Game = data.GameName ?? string.Empty,
            };

            NormalizerPipeline.Normalize(ref telemetrySnapshot);

            return telemetrySnapshot;
        }
    }
}
