using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBeeClass
{
    public class ATCommand : XBeeAPIFrame
    {
        private byte FrameID;
        private string AT;
        private string Parameter;

        public ATCommand(XBeeAPIFrame FrameIn)
        {
            if (FrameIn.getAPIID() == 0x08)
            {
                Raw = false;

                Payload = FrameIn.getPayload();
                APIID = FrameIn.getAPIID();
                Checksum = FrameIn.getChecksum();
                RawFrameData = FrameIn.getRawFrameData();

                byte[] ATb = new byte[2];
                byte[] Parameterb = new byte[Payload.Count - 4];

                FrameID = FrameIn.getPayload()[1];

                for (int i = 2; i < 4; i++)
                {
                    ATb[i - 2] = Payload[i];
                }
                for (int i = 4; i < Payload.Count; i++)
                {
                    Parameterb[i - 4] = Payload[i];
                }
                AT = GetString(ATb);
                Parameter = GetString(Parameterb);

            }
        }
        public ATCommand(string ATCommand, byte Frame = 1, string ATParameter = "")
        {
            AT = ATCommand;
            Parameter = ATParameter;
            FrameID = Frame;
            byte[] Parameterb;
            APIID = 0x08;

            byte[] ATb = GetBytes(AT);
            Parameterb = GetBytes(ATParameter);

            Payload.Add(APIID);            
            Payload.Add(FrameID);
            Payload.Add(ATb[0]);
            Payload.Add(ATb[1]);

            for(int i = 0; i < Parameter.Length; i++)
            {
                Payload.Add(Parameterb[i]);
            }
            createFromExistingData();
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
