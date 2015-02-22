using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBeeClass
{
    public class ATResponse :XBeeAPIFrame
    {
        private byte FrameID;
        private string AT;
        private string Data;
        private byte[] DataBytes;
        private byte StatusFlag;
        public ATResponse(XBeeAPIFrame FrameIn)
        {
            if (FrameIn.getAPIID() == 0x88)
            {
                Raw = false;

                Payload = FrameIn.getPayload();
                APIID = FrameIn.getAPIID();
                Checksum = FrameIn.getChecksum();
                RawFrameData = FrameIn.getRawFrameData();

                byte[] ATb = new byte[2];
                DataBytes = new byte[Payload.Count - 5];

                FrameID = FrameIn.getPayload()[1];

                for (int i = 2; i < 4; i++)
                {
                    ATb[i - 2] = Payload[i];
                }
                StatusFlag = Payload[4];
                for (int i = 5; i < Payload.Count; i++)
                {
                    DataBytes[i - 5] = Payload[i];
                }
                AT = GetString(ATb);
                Data = GetString(DataBytes);
            }
        }
        public string getATCommand()
        {
            return AT;
        }
        public string getDataString()
        {
            return Data;
        }
        public byte[] getDataBytes()
        {
            return DataBytes;
        }
        string GetString(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes);
        }
    }
}
