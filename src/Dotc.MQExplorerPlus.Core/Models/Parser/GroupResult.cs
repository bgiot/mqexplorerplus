#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;

namespace Dotc.MQExplorerPlus.Core.Models.Parser
{
    public class GroupResult : ParsingResultNode
    {

        public GroupResult(string label) : base(ParsingResultNodeType.Group)
        {
            if (label==null)throw new ArgumentNullException(nameof(label));
            Label = label;
        }
        public GroupResult(Configuration.GroupElement element) : this(element?.Label)
        {         
        }

        public override ParsingResultNode Clone()
        {
            var newItem = new GroupResult(Label);
            newItem.Children = Children.Clone();
            return newItem;

        }
    }
}
