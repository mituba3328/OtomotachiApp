using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtomotachiApp.Uuids
{
    class OtomoDeviceUuids
    {
        public static Guid[] OtomoDeviceServiceUuids { get; private set; } = new Guid[] { new Guid("73eb73e5-eec5-4bd2-8a53-2f059253ac96") };
        public static Guid OtomoPenServiceUuid { get; private set; } = new Guid("73eb73e5-eec5-4bd2-8a53-2f059253ac96");
        public static Guid OtomoPencheerLEDUuid { get; private set; } = new Guid("14f79a96-6e28-48c2-9920-ec065df069ab") ;
        public static Guid OtomoPenAlertLEDUuid { get; private set; } = new Guid("e877a7d3-c1eb-4ada-92cf-d15dfcb01cd9") ;
        public static Guid OtomoPenIsUsingUuid { get; private set; } = new Guid("c5479cc0-3854-487d-aefd-47c981617c55") ;
        public static Guid OtomoPenIsEnableUuid { get; private set; } = new Guid("2f801b84-9b8a-44b2-bd03-a208aede2320");
    }
}
