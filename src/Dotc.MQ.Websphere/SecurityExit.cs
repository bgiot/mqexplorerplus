#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Text;
using IBM.WMQ;

namespace Dotc.MQ.Websphere
{
    public sealed class SecurityExit : MQSecurityExit
    {
        byte[] MQSecurityExit.SecurityExit(MQChannelExit channelExitParms, MQChannelDefinition channelDefinition, byte[] dataBuffer, ref int dataOffset, ref int dataLength, ref int dataMaxLength)
        {

            if (channelExitParms == null) throw new ArgumentNullException(nameof(channelExitParms));
            if (channelDefinition == null) throw new ArgumentNullException(nameof(channelDefinition));

            byte[] result = null;

            

            if (channelExitParms.ExitID == MQC.MQXT_CHANNEL_SEC_EXIT)
            {
                switch (channelExitParms.ExitReason)
                {
                    // MCA Initializtion
                    case MQC.MQXR_INIT:
                        channelExitParms.ExitResponse = MQC.MQCC_OK;
                        break;

                    // Initiate Security Exchange state
                    case MQC.MQXR_INIT_SEC:
                        if (channelDefinition.ChannelType == MQC.MQCHT_SVRCONN) // server sends msg, requires client to respond
                        {
                            channelExitParms.ExitResponse = MQC.MQXCC_SEND_AND_REQUEST_SEC_MSG;
                        }
                        else if (channelDefinition.ChannelType == MQC.MQCHT_CLNTCONN) // client end does nothing at this point
                        {
                            channelExitParms.ExitResponse = MQC.MQCC_OK;
                        }
                        break;

                    // Security Message Received
                    case MQC.MQXR_SEC_MSG:
                        if (channelDefinition.ChannelType == MQC.MQCHT_SVRCONN) // server side receives message
                        {
                            // channel starts
                            channelExitParms.ExitResponse = MQC.MQCC_OK;
                        }
                        else if (channelDefinition.ChannelType == MQC.MQCHT_CLNTCONN) // client side receives message from server
                        {
                            result = Encoding.ASCII.GetBytes(channelDefinition.SecurityUserData);
                            dataLength = result.Length;
                            channelExitParms.ExitResponse = MQC.MQXCC_SEND_SEC_MSG;
                        }
                            break;

                    case MQC.MQXR_TERM:
                        channelExitParms.ExitResponse = MQC.MQCC_OK;
                        break;
                }
            }
            return result;
        }
    }
}
