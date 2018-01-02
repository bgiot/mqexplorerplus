#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
namespace Dotc.MQExplorerPlus.Core
{
    public class LabelValuePair<T>
    {
        public string Label { get; set; }
        public T Value { get; set; }
    }
}
