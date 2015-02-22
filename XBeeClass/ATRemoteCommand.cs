using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBeeClass
{
    public class ATRemoteCommand :XBeeAPIFrame
    {
        private byte FrameID;
        private string AT;
        private string Parameter = "";
        private byte Options;
        List<byte> Address64 = new List<byte>();
        List<byte> Address16 = new List<byte>();

        public ATRemoteCommand(XBeeAPIFrame FrameIn)
        {
            if (FrameIn.getAPIID() == 0x17)
            {
                Raw = false;

                Payload = FrameIn.getPayload();
                APIID = FrameIn.getAPIID();
                Checksum = FrameIn.getChecksum();
                RawFrameData = FrameIn.getRawFrameData();

                byte[] ATb = new byte[2];

                FrameID = FrameIn.getPayload()[1];
                for (int i = 2; i < 10; i++)
                {
                    Address64.Add(Payload[i]);
                }
                for (int i = 10; i < 12; i++)
                {
                    Address16.Add(Payload[i]);
                }
                Options = Payload[12];
                for (int i = 13; i < 15; i++)
                {
                    ATb[i - 13] = Payload[i];
                }
                AT = GetString(ATb);
                if (Payload.Count > 15)
                {
                    byte[] Prb = new byte[Payload.Count - 15];
                    for(int i = 15; i < Payload.Count; i++)
                    {
                        Prb[i - 15] = Payload[i];
                    }
                    Parameter = GetString(Prb);
                }
            }
        }
        public ATRemoteCommand(string ATCommand, XBeeModule Destination, byte Frame = 1, byte Option = 2, string ATParameter = "")
        {
            AT = ATCommand;
            Parameter = ATParameter;
            FrameID = Frame;
            APIID = 0x17;

            byte[] ATb = GetBytes(AT);
            byte[] Parameterb = GetBytes(ATParameter);
            Address64.AddRange(Destination.get64bitAddress().ToArray());
            Address16.AddRange(Destination.get16bitAddress().ToArray());

            Payload.Add(APIID);
            Payload.Add(FrameID);
            foreach(byte t in Address64) { Payload.Add(t); }
            foreach(byte t in Address16) { Payload.Add(t); }

            Payload.Add(Option);

            Payload.Add(ATb[0]);
            Payload.Add(ATb[1]);

            for (int i = 0; i < Parameter.Length; i++)
            {
                Payload.Add(Parameterb[i]);
            }
            createFromExistingData();

            Destination.addPacketLink(0x17, FrameID, 0x97);
        }
        byte[] GetBytes(string str)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(str);
            return bytes;
        }

        string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

    }
}
