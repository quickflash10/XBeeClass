using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBeeClass
{
    public class ReceivePacket : XBeeAPIFrame
    {
        List<byte> Address64 = new List<byte>();
        List<byte> Address16 = new List<byte>();
        private string Data;
        private byte[] DataBytes;
        private byte Options;
        public ReceivePacket(XBeeAPIFrame FrameIn)
        {
            if (FrameIn.getAPIID() == 0x90)
            {
                Raw = false;

                Payload = FrameIn.getPayload();
                APIID = FrameIn.getAPIID();
                Checksum = FrameIn.getChecksum();
                RawFrameData = FrameIn.getRawFrameData();

                DataBytes = new byte[Payload.Count - 12];

                for (int i = 1; i < 9; i++)
                {
                    Address64.Add(Payload[i]);
                }
                for (int i = 9; i < 11; i++)
                {
                    Address16.Add(Payload[i]);
                }

                Options = Payload[11];

                for (int i = 12; i < Payload.Count; i++)
                {
                    DataBytes[i - 12] = Payload[i];
                }
                Data = GetString(DataBytes);
            }
        }
        public string getDataString()
        {
            return Data;
        }
        public byte[] getDataBytes()
        {
            return DataBytes;
        }
        public byte[] getDataBytesWithoutAPI()
        {
            //byte[] DB = new byte[DataBytes.Length - 1];
            //for(int i = 1; i < DataBytes.Length - 1; i++)
            //{
            //    DB[i] = DataBytes[i+1];
            //}
            return DataBytes;
        }
        public byte getOptions()
        {
            return Options;
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
