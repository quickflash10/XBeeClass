using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBeeClass
{
    public class ATRemoteResponse : XBeeAPIFrame
    {
        private byte FrameID;
        private string AT;
        List<byte> Address64 = new List<byte>();
        List<byte> Address16 = new List<byte>();
        private string Data;
        private byte[] DataBytes;
        private byte StatusFlag;
        public ATRemoteResponse(XBeeAPIFrame FrameIn)
        {
            if (FrameIn.getAPIID() == 0x97)
            {
                Raw = false;

                Payload = FrameIn.getPayload();
                APIID = FrameIn.getAPIID();
                Checksum = FrameIn.getChecksum();
                RawFrameData = FrameIn.getRawFrameData();

                byte[] ATb = new byte[2];
                DataBytes = new byte[Payload.Count - 15];

                FrameID = FrameIn.getPayload()[1];

                for (int i = 2; i < 10; i++)
                {
                    Address64.Add(Payload[i]);
                }
                for (int i = 10; i < 12; i++)
                {
                    Address16.Add(Payload[i]);
                }

                for (int i = 12; i < 14; i++)
                {
                    ATb[i - 12] = Payload[i];
                }

                StatusFlag = Payload[14];

                for (int i = 15; i < Payload.Count; i++)
                {
                    DataBytes[i - 15] = Payload[i];
                }
                AT = GetString(ATb);
                Data = GetString(DataBytes);
            }
        }
        public string getATCommand()
        {
            return AT;
        }
        public byte getFrameID()
        {
            return FrameID;
        }
        public string getDataString()
        {
            return Data;
        }
        public byte[] getDataBytes()
        {
            return DataBytes;
        }
        public byte getStatus()
        {
            return StatusFlag;
        }
        public List<byte> getAddress64()
        {
            return Address64;
        }
        public List<byte> getAddress16()
        {
            return Address16;
        }
        string GetString(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes);
        }
    }
}

