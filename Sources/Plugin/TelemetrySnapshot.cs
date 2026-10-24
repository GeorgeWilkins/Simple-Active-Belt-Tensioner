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
        public bool DidUpshift;
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
                    DidUpshift = false,
                    IsActive = false,
                    Game = string.Empty,
                };
            }

            short oldGear = 0;
            short newGear = 0;
            bool inGear = Int16.TryParse(data.OldData?.Gear, out oldGear) && Int16.TryParse(data.NewData?.Gear, out newGear);

            TelemetrySnapshot telemetrySnapshot = new TelemetrySnapshot
            {
                Surge = data.NewData?.AccelerationSurge ?? 0.0,
                Sway = data.NewData?.AccelerationSway ?? 0.0,
                Heave = data.NewData?.AccelerationHeave ?? 0.0,
                Speed = data.NewData?.SpeedKmh ?? 0.0,
                Revolutions = data.NewData?.Rpms ?? 0.0,
                DidUpshift = inGear && (newGear > oldGear),
                IsActive = (data.GameRunning && !data.GameInMenu && !data.GamePaused) || data.GameReplay,
                Game = data.GameName ?? string.Empty,
            };

            NormalizerPipeline.Normalize(ref telemetrySnapshot);

            return telemetrySnapshot;
        }
    }
}
