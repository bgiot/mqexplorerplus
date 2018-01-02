#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dotc.MQExplorerPlus.Core.Models.Parser
{
    public sealed class ParsingResultNodeList : ObservableCollection<ParsingResultNode>
    {
        public void Reset()
        {
            foreach (var item in Items)
            {
                item.Reset();
            }
        }

        public ParsingResultNodeList Clone()
        {
            var copy = new ParsingResultNodeList();
            foreach (var node in this)
            {
                copy.Add(node.Clone());
            }
            return copy;
        }

        public void AddRange(IEnumerable<ParsingResultNode> nodes)
        {
            if (nodes == null) throw  new ArgumentNullException(nameof(nodes));
            foreach (var node in nodes)
            {
                Add(node);
            }
        }
    }
}
