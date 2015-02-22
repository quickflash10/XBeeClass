using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBeeClass
{
    public class TransmitStatus : XBeeAPIFrame
    {
        private byte FrameID;
        private List<byte> Address16 = new List<byte>();
        private byte TRC;
        private byte DeliveryStatus;
        private byte DiscoveryStatus;

        public TransmitStatus(XBeeAPIFrame FrameIn)
        {
            if (FrameIn.getAPIID() == 0x8B)
            {
                Raw = false;

                Payload = FrameIn.getPayload();
                APIID = FrameIn.getAPIID();
                Checksum = FrameIn.getChecksum();
                RawFrameData = FrameIn.getRawFrameData();
                FrameID = RawFrameData[4];
                Address16.Add(RawFrameData[5]);
                Address16.Add(RawFrameData[6]);
                TRC = RawFrameData[7];
                DeliveryStatus = RawFrameData[8];
                DiscoveryStatus = RawFrameData[9];
            }
        }
        public byte getFrameID()
        {
            return FrameID;
        }
        public List<byte> getAddress16()
        {
            return Address16;
        }
        public int getTRC()
        {
            return (int)TRC;
        }
        public string getDeliveryStatus()
        {
            string Status;
            switch (DeliveryStatus)
            {
                case 0x00:
                    Status = "Success";
                    break;
                case 0x01:
                    Status = "MAC ACK Failure";
                    break;
                case 0x02:
                    Status = "CCA Failure";
                    break;
                case 0x15:
                    Status = "Invalid destination endpoint";
                    break;
                case 0x21:
                    Status = "Network ACK Failure";
                    break;
                case 0x22:
                    Status = "Not Joined to Network";
                    break;
                case 0x23:
                    Status = "Self-addressed";
                    break;
                case 0x24:
                    Status = "Address Not Found";
                    break;
                case 0x25:
                    Status = "Route Not Found";
                    break;
                case 0x26:
                    Status = "Broadcast source failed to hear a neighbor relay the message";
                    break;
                case 0x2B:
                    Status = "Invalid binding table index";
                    break;
                case 0x2C:
                    Status = "Resource error lack of free buffers, timers, etc.";
                    break;
                case 0x2D:
                    Status = "Attempted broadcast with APS transmission";
                    break;
                case 0x2E:
                    Status = "Attempted unicast with APS transmission, but EE = 0";
                    break;
                case 0x32:
                    Status = "Resource error lack of free buffers, timers, etc.";
                    break;
                case 0x74:
                    Status = "Data payload too large";
                    break;
                default:
                    StringBuilder hex = new StringBuilder(2);
                    hex.AppendFormat("{0:x2}", DeliveryStatus);

                    Status = hex.ToString();
                    break;
            }
            return Status;
        }
        public string getDiscoveryStatus()
        {
            string Status;
            switch (DiscoveryStatus)
            {
                case 0x00:
                    Status = "Success";
                    break;
                case 0x01:
                    Status = "Address Discovery";
                    break;
                case 0x02:
                    Status = "Route Discovery";
                    break;
                case 0x03:
                    Status = "Address and Route";
                    break;
                case 0x40:
                    Status = "Extended Timeout Discover";
                    break;
                default:
                    StringBuilder hex = new StringBuilder(2);
                    hex.AppendFormat("{0:x2}", DeliveryStatus);

                    Status = hex.ToString();
                    break;
            }
            return Status;
        }
    }
}
