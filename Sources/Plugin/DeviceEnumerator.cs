using SimHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WoteverCommon.Extensions;

namespace User.ActiveBeltTensioner
{
    public static class DeviceEnumerator
    {
        private static readonly Regex PortPattern = new Regex(@"\((COM\d+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>Identifies devices that match the expected VID/PID for the controller board (or more specifically, the serial bridge we using on it)</summary>
        /// <returns>A list of <see cref="DeviceInstance" /> instances that appear to match</returns>
        public static List<DeviceInstance> FindMatchingDevices()
        {
            const string vidPid = "VID_1A86&PID_55D3";

            List<DeviceInstance> results = new List<DeviceInstance>();

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "SELECT Name, Caption, PNPDeviceID FROM Win32_PnPEntity"))
            {
                foreach (ManagementObject mo in searcher.Get())
                {
                    string name = mo["Name"] as string ?? string.Empty;
                    string caption = mo["Caption"] as string ?? string.Empty;
                    string pnpDeviceId = mo["PNPDeviceID"] as string ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(pnpDeviceId))
                    {
                        continue;
                    }

                    if (!pnpDeviceId.Contains(vidPid, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string display = !string.IsNullOrWhiteSpace(name) ? name : caption;
                    Match match = PortPattern.Match(display);

                    if (!match.Success)
                    {
                        continue;
                    }

                    results.Add(new DeviceInstance
                    {
                        SerialPort = match.Groups[1].Value.ToUpperInvariant(),
                        Name = display,
                        PnpDeviceId = pnpDeviceId
                    });
                }
            }

            return results
                .OrderBy(d => d.SerialPort, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
