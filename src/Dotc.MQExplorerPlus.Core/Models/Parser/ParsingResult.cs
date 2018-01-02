#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.Common;

namespace Dotc.MQExplorerPlus.Core.Models.Parser
{
    public class ParsingResult : ObservableObject
    {

        public ParsingResult()
        {
            Nodes = new ParsingResultNodeList();
        }
        public ParsingResultNodeList Nodes { get; private set; }

        private string _unparsedData;
        public string UnparsedData
        {
            get { return _unparsedData; }
            set { SetPropertyAndNotify(ref _unparsedData, value); }
        }

        public void Reset()
        {
            Nodes.Reset();
            UnparsedData = null;
        }
    }
}
