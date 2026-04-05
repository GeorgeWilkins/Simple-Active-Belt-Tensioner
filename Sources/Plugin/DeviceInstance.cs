using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.ActiveBeltTensioner
{
    public sealed class DeviceInstance
    {
        public string SerialPort { get; set; }
        public string Name { get; set; }
        public string PnpDeviceId { get; set; }
    }
}
