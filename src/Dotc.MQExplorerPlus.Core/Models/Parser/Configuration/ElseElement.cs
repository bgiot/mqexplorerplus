#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    public class ElseElement : GroupElement
    {
        public new const string ElementName = "else";

        protected override string NodeName
        {
            get
            {
                return ElementName;
            }
        }

    }
}
