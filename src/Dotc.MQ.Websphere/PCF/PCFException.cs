#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections;

namespace Dotc.MQ.Websphere.PCF
{
    public class PcfException : IBM.WMQ.MQException
    {
        public const int MqrccfActionValueError = 0xc13;
        public const int MqrccfAllocateFailed = 0xfa9;
        public const int MqrccfAlreadyJoined = 0xc55;
        public const int MqrccfAttrValueError = 0xfa5;
        public const int MqrccfBatchIntError = 0xff6;
        public const int MqrccfBatchIntWrongType = 0xff7;
        public const int MqrccfBatchSizeError = 0xbdd;
        public const int MqrccfBindFailed = 0xfb8;
        public const int MqrccfBrokerCommandFailed = 0xc16;
        public const int MqrccfBrokerDeleted = 0xbfe;
        public const int MqrccfCcsidError = 0xbe9;
        public const int MqrccfCellDirNotAvailable = 0xfe4;
        public const int MqrccfCfhCommandError = 0xbbf;
        public const int MqrccfCfhControlError = 0xbbd;
        public const int MqrccfCfhLengthError = 0xbba;
        public const int MqrccfCfhMsgSeqNumberErr = 0xbbc;
        public const int MqrccfCfhParmCountError = 0xbbe;
        public const int MqrccfCfhTypeError = 0xbb9;
        public const int MqrccfCfhVersionError = 0xbbb;
        public const int MqrccfCfilCountError = 0xbd3;
        public const int MqrccfCfilDuplicateValue = 0xbd2;
        public const int MqrccfCfilLengthError = 0xbd4;
        public const int MqrccfCfilParmIdError = 0xbe7;
        public const int MqrccfCfinDuplicateParm = 0xbc9;
        public const int MqrccfCfinLengthError = 0xbc1;
        public const int MqrccfCfinParmIdError = 0xbc6;
        public const int MqrccfCfslCountError = 0xbfc;
        public const int MqrccfCfslDuplicateParm = 0xbfa;
        public const int MqrccfCfslLengthError = 0xbd0;
        public const int MqrccfCfslParmIdError = 0xbd9;
        public const int MqrccfCfslStringLengthErr = 0xbfd;
        public const int MqrccfCfslTotalLengthError = 0xbfb;
        public const int MqrccfCfstConflictingParm = 0xc17;
        public const int MqrccfCfstDuplicateParm = 0xbca;
        public const int MqrccfCfstLengthError = 0xbc2;
        public const int MqrccfCfstParmIdError = 0xbc7;
        public const int MqrccfCfstStringLengthErr = 0xbc3;
        public const int MqrccfChadError = 0xfef;
        public const int MqrccfChadEventError = 0xff1;
        public const int MqrccfChadEventWrongType = 0xff2;
        public const int MqrccfChadExitError = 0xff3;
        public const int MqrccfChadExitWrongType = 0xff4;
        public const int MqrccfChadWrongType = 0xff0;
        public const int MqrccfChannelAlreadyExists = 0xfca;
        public const int MqrccfChannelClosed = 0xffa;
        public const int MqrccfChannelDisabled = 0xfc6;
        public const int MqrccfChannelInUse = 0xfbf;
        public const int MqrccfChannelIndoubt = 0xfb9;
        public const int MqrccfChannelNameError = 0xfcc;
        public const int MqrccfChannelNotActive = 0xfe0;
        public const int MqrccfChannelNotFound = 0xfc0;
        public const int MqrccfChannelTableError = 0xbf6;
        public const int MqrccfChannelTypeError = 0xbda;
        public const int MqrccfChlInstTypeError = 0xbf8;
        public const int MqrccfChlStatusNotFound = 0xbf9;
        public const int MqrccfClusterNameConflict = 0xc10;
        public const int MqrccfClusterQUsageError = 0xc12;
        public const int MqrccfCommandFailed = 0xbc0;
        public const int MqrccfCommitFailed = 0xfc8;
        public const int MqrccfCommsLibraryError = 0xc14;
        public const int MqrccfConfigurationError = 0xfab;
        public const int MqrccfConnNameError = 0xfde;
        public const int MqrccfConnectionClosed = 0xfb1;
        public const int MqrccfConnectionRefused = 0xfac;
        public const int MqrccfCorrelIdError = 0xc08;
        public const int MqrccfDataConvValueError = 0xbec;
        public const int MqrccfDataTooLarge = 0xfcb;
        public const int MqrccfDelOptionsError = 0xc0f;
        public const int MqrccfDiscIntError = 0xbde;
        public const int MqrccfDiscIntWrongType = 0xfd6;
        public const int MqrccfDuplicateIdentity = 0xc06;
        public const int MqrccfDuplicateSubscription = 0xc50;
        public const int MqrccfDynamicQScopeError = 0xfe3;
        public const int MqrccfEncodingError = 0xbea;
        public const int MqrccfEntryError = 0xfad;
        public const int MqrccfEscapeTypeError = 0xbee;
        public const int MqrccfFilterError = 0xc4e;
        public const int MqrccfForceValueError = 0xbc4;
        public const int MqrccfHbIntervalError = 0xfed;
        public const int MqrccfHbIntervalWrongType = 0xfee;
        public const int MqrccfHostNotAvailable = 0xfaa;
        public const int MqrccfIncorrectQ = 0xc07;
        public const int MqrccfIncorrectStream = 0xc03;
        public const int MqrccfIndoubtValueError = 0xbed;
        public const int MqrccfKeepAliveIntError = 0xfdc;
        public const int MqrccfLikeObjectWrongType = 0xfa3;
        public const int MqrccfListenerNotStarted = 0xfb4;
        public const int MqrccfLongRetryError = 0xbe1;
        public const int MqrccfLongRetryWrongType = 0xfd9;
        public const int MqrccfLongTimerError = 0xbe2;
        public const int MqrccfLongTimerWrongType = 0xfda;
        public const int MqrccfMaxMsgLengthError = 0xbe4;
        public const int MqrccfMcaNameError = 0xfcf;
        public const int MqrccfMcaNameWrongType = 0xfd5;
        public const int MqrccfMcaTypeError = 0xbf7;
        public const int MqrccfMdFormatError = 0xbcf;
        public const int MqrccfMissingConnName = 0xfdd;
        public const int MqrccfModeValueError = 0xbd5;
        public const int MqrccfMqconnFailed = 0xfba;
        public const int MqrccfMqgetFailed = 0xfbc;
        public const int MqrccfMqinqFailed = 0xfc4;
        public const int MqrccfMqopenFailed = 0xfbb;
        public const int MqrccfMqputFailed = 0xfbd;
        public const int MqrccfMqsetFailed = 0xfdf;
        public const int MqrccfMrCountError = 0xfe5;
        public const int MqrccfMrCountWrongType = 0xfe6;
        public const int MqrccfMrExitNameError = 0xfe7;
        public const int MqrccfMrExitNameWrongType = 0xfe8;
        public const int MqrccfMrIntervalError = 0xfe9;
        public const int MqrccfMrIntervalWrongType = 0xfea;
        public const int MqrccfMsgExitNameError = 0xfd2;
        public const int MqrccfMsgLengthError = 0xbc8;
        public const int MqrccfMsgSeqNumberError = 0xbd6;
        public const int MqrccfMsgTruncated = 0xbe8;
        public const int MqrccfNetPriorityError = 0xff8;
        public const int MqrccfNetPriorityWrongType = 0xff9;
        public const int MqrccfNetbiosNameError = 0xc15;
        public const int MqrccfNoCommsManager = 0xfb3;
        public const int MqrccfNoRetainedMsg = 0xc05;
        public const int MqrccfNoStorage = 0xfb2;
        public const int MqrccfNotAuthorized = 0xc09;
        public const int MqrccfNotRegistered = 0xc01;
        public const int MqrccfNotXmitQ = 0xfc5;
        public const int MqrccfNpmSpeedError = 0xfeb;
        public const int MqrccfNpmSpeedWrongType = 0xfec;
        public const int MqrccfObjectAlreadyExists = 0xfa1;
        public const int MqrccfObjectNameError = 0xfa8;
        public const int MqrccfObjectOpen = 0xfa4;
        public const int MqrccfObjectWrongType = 0xfa2;
        public const int MqrccfParmCountTooBig = 0xbcc;
        public const int MqrccfParmCountTooSmall = 0xbcb;
        public const int MqrccfParmSequenceError = 0xbdb;
        public const int MqrccfParmSyntaxError = 0xc19;
        public const int MqrccfPathNotValid = 0xc18;
        public const int MqrccfPingDataCompareError = 0xbd8;
        public const int MqrccfPingDataCountError = 0xbd7;
        public const int MqrccfPingError = 0xfbe;
        public const int MqrccfPubOptionsError = 0xc0c;
        public const int MqrccfPurgeValueError = 0xbe6;
        public const int MqrccfPutAuthError = 0xbe5;
        public const int MqrccfPutAuthWrongType = 0xfdb;
        public const int MqrccfPwdLengthError = 0xc1a;
        public const int MqrccfQAlreadyInCell = 0xbcd;
        public const int MqrccfQMgrCcsidError = 0xc0e;
        public const int MqrccfQMgrNameError = 0xc02;
        public const int MqrccfQNameError = 0xc04;
        public const int MqrccfQStatusNotFound = 0xffb;
        public const int MqrccfQTypeError = 0xbce;
        public const int MqrccfQWrongType = 0xfa7;
        public const int MqrccfQueuesValueError = 0xbeb;
        public const int MqrccfQuiesceValueError = 0xbd5;
        public const int MqrccfRcvExitNameError = 0xfd3;
        public const int MqrccfReceiveFailed = 0xfb0;
        public const int MqrccfReceivedDataError = 0xfaf;
        public const int MqrccfRegOptionsError = 0xc0b;
        public const int MqrccfRemoteQmTerminating = 0xfc3;
        public const int MqrccfRemoteQmUnavailable = 0xfc2;
        public const int MqrccfReplaceValueError = 0xbd1;
        public const int MqrccfReposNameConflict = 0xc11;
        public const int MqrccfReposValueError = 0xbef;
        public const int MqrccfSecExitNameError = 0xfd1;
        public const int MqrccfSendExitNameError = 0xfd0;
        public const int MqrccfSendFailed = 0xfae;
        public const int MqrccfSeqNumberWrapError = 0xbe3;
        public const int MqrccfShortRetryError = 0xbdf;
        public const int MqrccfShortRetryWrongType = 0xfd7;
        public const int MqrccfShortTimerError = 0xbe0;
        public const int MqrccfShortTimerWrongType = 0xfd8;
        public const int MqrccfSslCipherSpecError = 0xffc;
        public const int MqrccfSslClientAuthError = 0xffe;
        public const int MqrccfSslPeerNameError = 0xffd;
        public const int MqrccfStreamError = 0xbff;
        public const int MqrccfStructureTypeError = 0xbc5;
        public const int MqrccfSubIdentityError = 0xc52;
        public const int MqrccfSubNameError = 0xc51;
        public const int MqrccfSubscriptionInUse = 0xc53;
        public const int MqrccfSubscriptionLocked = 0xc54;
        public const int MqrccfSuppressedByExit = 0xff5;
        public const int MqrccfTerminatedBySecExit = 0xfe1;
        public const int MqrccfTopicError = 0xc00;
        public const int MqrccfUnknownBroker = 0xc0d;
        public const int MqrccfUnknownQMgr = 0xfa6;
        public const int MqrccfUnknownRemoteChannel = 0xfc1;
        public const int MqrccfUnknownStream = 0xc0a;
        public const int MqrccfUserExitNotAvailable = 0xfc7;
        public const int MqrccfWrongChannelType = 0xfc9;
        public const int MqrccfWrongUser = 0xc4f;
        public const int MqrccfXmitProtocolTypeErr = 0xbdc;
        public const int MqrccfXmitQNameError = 0xfcd;
        public const int MqrccfXmitQNameWrongType = 0xfd4;

        public PcfException()
        {
        }
        public PcfException(int compCode, int reason) : base(compCode, reason)
        {
        }

        public static PcfException Build(int compCode, int reason, Action<IDictionary> extraInfo = null)
        {
            var ex = new PcfException(compCode, reason);
            if (extraInfo != null)
            {
                extraInfo.Invoke(ex.Data);
            }
            return ex;
        }

    }
}
