using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBeeClass
{
    public class XBeeAPIFrame
    {
        protected List<byte> RawFrameData;
        protected byte APIID;
        protected int DataSize;
        protected byte Checksum;
        protected List<byte> Payload;
        protected bool Raw = true;

        protected XBeeAPIFrame() {
            RawFrameData = new List<byte>();
            Payload = new List<byte>();
        }

        public XBeeAPIFrame(List<byte> VerifiedPacket)
        {
            Payload = new List<byte>();
            RawFrameData = VerifiedPacket;
            DataSize = RawFrameData[1] << 8;
            DataSize = DataSize + RawFrameData[2];
            APIID = RawFrameData[3];
            for(int i = 3; i < RawFrameData.Count - 1; i++)
            {
                Payload.Add(RawFrameData[i]);
            }
            Checksum = RawFrameData.Last();
        }
        public XBeeAPIFrame(byte API_ID, List<byte> PayLoad)
        {
            createFromAPIandData(API_ID, PayLoad);
        }
        protected void createFromAPIandData(byte API_ID, List<byte> PayLoad)
        {
            APIID = API_ID;
            Payload = PayLoad;
            createFromExistingData();
        }
        protected void createFromExistingData()
        {
            int Csum = 0;
            DataSize = Payload.Count;

            RawFrameData.Clear();
            RawFrameData.Add(0x7E);
            RawFrameData.Add((byte)((0xFF00 & (Payload.Count)) >> 8));
            RawFrameData.Add((byte)(0xFF & (Payload.Count)));
            for (int i = 0; i < Payload.Count; i++)
            {
                Csum = Csum + Payload[i];
                RawFrameData.Add(Payload[i]);
            }
            Checksum = (byte)(0xFF - (0xFF & Csum));
            RawFrameData.Add(Checksum);
        }
        public byte getAPIID()
        {
            return APIID;
        }
        public int getDataSize()
        {
            return DataSize;
        }
        public List<byte> getPayload()
        {
            return Payload;
        }
        public byte getChecksum()
        {
            return Checksum;
        }
        public List<byte> getRawFrameData()
        {
            return RawFrameData;
        }

        public bool isRawPacket()
        {
            return Raw;
        }
    }
}
