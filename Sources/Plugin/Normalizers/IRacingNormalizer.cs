using System;

namespace User.ActiveBeltTensioner.Normalizers
{
    /// <summary>Corrects iRacing's bizarre '300RPM & -1G in menus' behaviour</summary>
    internal sealed class IRacingNormalizer : INormalizer
    {
        private static bool _matchedRevolutions;
        private static bool _matchedHeave;

        public bool Normalize(ref TelemetrySnapshot telemetrySnapshot)
        {
            if (string.Equals(telemetrySnapshot.Game, "IRacing", StringComparison.OrdinalIgnoreCase))
            {
                if (telemetrySnapshot.IsActive)
                {
                    bool matchesRevolutions = (Math.Abs(telemetrySnapshot.Revolutions - 300.0) < 0.001);
                    bool matchesHeave = (Math.Abs(telemetrySnapshot.Heave + 9.80665) < 0.001);

                    if ((matchesRevolutions && _matchedRevolutions) || (matchesHeave && _matchedHeave))
                    {
                        telemetrySnapshot.Revolutions = 0.0;
                        telemetrySnapshot.Surge = 0.0;
                        telemetrySnapshot.Sway = 0.0;
                        telemetrySnapshot.Heave = 0.0;
                    }

                    _matchedRevolutions = matchesRevolutions;
                    _matchedHeave = matchesHeave;

                    return true;
                }
            }

            _matchedRevolutions = false;
            _matchedHeave = false;

            return false;
        }
    }
}