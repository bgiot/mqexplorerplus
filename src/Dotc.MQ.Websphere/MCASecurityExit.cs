#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using IBM.WMQ;
using System.Text;

namespace Dotc.MQ.Websphere
{
    public class MCASecurityExit : MQSecurityExit
    {
        public byte[] SecurityExit(MQChannelExit channelExitParms, MQChannelDefinition channelDefinition, byte[] dataBuffer, ref int dataOffset, ref int dataLength, ref int dataMaxLength)
        {

            if (channelExitParms.ExitID != MQC.MQXT_CHANNEL_SEC_EXIT)
            {
                channelExitParms.ExitResponse = MQC.MQXCC_SUPPRESS_FUNCTION;
                return null;
            }
            else
            {
                switch (channelExitParms.ExitReason)
                {
                    case MQC.MQXR_INIT:
                    case MQC.MQXR_INIT_SEC:
                    case MQC.MQXR_SEC_MSG:
                    case MQC.MQXR_TERM:
                        channelExitParms.ExitResponse = MQC.MQXCC_OK;
                        break;

                    case MQC.MQXR_SEC_PARMS:
                        string userId = channelExitParms.SecurityParms?.UserId;
                        if ((userId != null) && (userId.Length < 13))
                        {
                            channelDefinition.MCAUserIdentifier = Encoding.UTF8.GetBytes(channelExitParms.SecurityParms.UserId.PadRight(12, ' '));
                        }
                        channelExitParms.ExitResponse = MQC.MQXCC_OK;
                        break;

                    default:
                        channelExitParms.ExitResponse = MQC.MQXCC_SUPPRESS_FUNCTION;
                        break;
                }
                return null;
            }
        }
    }
}


