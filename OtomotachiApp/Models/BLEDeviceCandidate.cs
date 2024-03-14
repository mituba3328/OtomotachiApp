using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtomotachiApp.Models
{
    public class BLEDeviceCandidate
    {
        public Guid Id { get; internal set; }
        public string Name { get; internal set; }
    }
}
