using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBeeClass
{
    public class TransmitRequest : XBeeAPIFrame
    {
        private List<byte> Destination64 = new List<byte>();
        private List<byte> Destination16 = new List<byte>();
        private List<byte> Data = new List<byte>();
        private byte FrameID;
        private byte Options;
        private byte BroadcastRadius;

        public TransmitRequest(XBeeAPIFrame FrameIn)
        {
            if (FrameIn.getAPIID() == 0x10)
            {
                Raw = false;
                Payload = FrameIn.getPayload();
                APIID = FrameIn.getAPIID();
                Checksum = FrameIn.getChecksum();
                RawFrameData = FrameIn.getRawFrameData();

                FrameID = FrameIn.getPayload()[1];
                for (int i = 2; i < 10; i++)
                {
                    Destination64.Add(FrameIn.getPayload()[i]);
                }
                for (int i = 10; i < 12; i++)
                {
                    Destination16.Add(FrameIn.getPayload()[i]);
                }
                BroadcastRadius = FrameIn.getPayload()[12];
                Options = FrameIn.getPayload()[13];
                for (int i = 13; i < FrameIn.getPayload().Count; i++)
                {
                    Data.Add(FrameIn.getPayload()[i]);
                }
            }
        }
        public TransmitRequest(XBeeModule To, List<byte> DataToTransfer, byte Frame = 1, byte OptionsForTransfer = 0, byte Radius = 0)
        {
            Raw = false;
            Options = OptionsForTransfer;
            BroadcastRadius = Radius;
            Data = DataToTransfer;
            Payload.Add(0x10);
            Payload.Add(Frame);
            foreach(byte t in To.get64bitAddress())
            {
                Payload.Add(t);
            }
            foreach (byte t in To.get16bitAddress())
            {
                Payload.Add(t);
            }
            Payload.Add(BroadcastRadius);
            Payload.Add(Options);
            foreach(byte d in Data)
            {
                Payload.Add(d);
            }
            createFromExistingData();
            To.addPacketLink(0x10, FrameID, 0x99);
        }
        
    }
}
