using System;

namespace User.ActiveBeltTensioner.Normalizers
{
    /// <summary>Corrects GRID2019's wierd pre-race wobble</summary>
    internal sealed class Grid2019Normalizer : INormalizer
    {
        private static bool _matchedSpeed;
        private static bool _matchedSway;

        public bool Normalize(ref TelemetrySnapshot telemetrySnapshot)
        {
            if (string.Equals(telemetrySnapshot.Game, "CodemastersGrid2019", StringComparison.OrdinalIgnoreCase))
            {
                if (telemetrySnapshot.IsActive)
                {
                    bool matchesSpeed = (telemetrySnapshot.Speed > 0.18 && telemetrySnapshot.Speed < 0.20);
                    bool matchesSway = (
                        (telemetrySnapshot.Sway > 5.8 && telemetrySnapshot.Sway < 6.2) ||
                        (telemetrySnapshot.Sway > -6.2 && telemetrySnapshot.Sway < -5.8)
                    );

                    if ((matchesSpeed && _matchedSpeed) && (matchesSway && _matchedSway))
                    {
                        telemetrySnapshot.Sway = 0.0;
                    }

                    _matchedSpeed = matchesSpeed;
                    _matchedSway = matchesSway;

                    return true;
                }
            }

            _matchedSpeed = false;
            _matchedSway = false;

            return false;
        }
    }
}