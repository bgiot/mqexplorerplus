#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
namespace Dotc.MQExplorerPlus.DesignTime
{
    public class QueueInfoExtDesignTime
    {
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public IQueueInfoExtendedPropertiesDesign ExtendedProperties { get; }
    }

    public interface IQueueInfoExtendedPropertiesDesign
    {
        int UncommittedCount { get; }
        int MaxDepth { get; }
        string UnderlyingName { get; }
        int OpenReadCount { get; }
        int OpenWriteCount { get; }
        int Priority { get; }
        string Format { get; }

    }
}
