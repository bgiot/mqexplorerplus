#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.ComponentModel;

namespace Dotc.MQ
{
    public interface IMessage : INotifyPropertyChanged
    {
        int? Index { get; }
        IQueue Queue { get; }
        byte[] MessageId { get; }
        byte[] Bytes { get; }
        string Text { get; }
        DateTime PutTimestamp { get; }
        int Length { get; }
        dynamic ExtendedProperties { get; }

    }
}
