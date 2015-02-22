using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBeeClass
{
    public class ModemStatus : XBeeAPIFrame
    {
        private byte Status;
        public ModemStatus(XBeeAPIFrame FrameIn)
        {
            if (FrameIn.getAPIID() == 0x8A)
            {
                Raw = false;

                Payload = FrameIn.getPayload();
                APIID = FrameIn.getAPIID();
                Checksum = FrameIn.getChecksum();
                RawFrameData = FrameIn.getRawFrameData();
                Status = RawFrameData[4];
            }
        }
        public string getStatus()
        {
            string Status_S;
            switch(Status)
            {
                case 0:
                    Status_S = "Hardware Reset";
                    break;

                case 1:
                    Status_S = "Watchdog Timer Reset";
                    break;

                case 2:
                    Status_S = "Joined Network";
                    break;

                case 3:
                    Status_S = "Dissassociated";
                    break;

                case 6:
                    Status_S = "Coordinator started";
                    break;

                case 7:
                    Status_S = "Network Security key was updated";
                    break;

                default:
                    StringBuilder hex = new StringBuilder(2);
                    hex.AppendFormat("{0:x2}", Status);

                    Status_S = hex.ToString();
                    break;
            }
            return Status_S;
        }
    }
}
