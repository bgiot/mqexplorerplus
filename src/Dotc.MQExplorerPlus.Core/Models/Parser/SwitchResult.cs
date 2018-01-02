#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using Dotc.MQExplorerPlus.Core.Models.Parser.Configuration;

namespace Dotc.MQExplorerPlus.Core.Models.Parser
{
    public class SwitchResult : ParsingResultNode
    {
        public SwitchResult(string label) : base(ParsingResultNodeType.Switch)
        {
            if (label == null) throw  new ArgumentNullException(nameof(label));
            Label = label;
            Cases = new Dictionary<string, GroupResult>();
        }

        public SwitchResult(SwitchElement element) : this(element?.Label)
        {
            if (element == null) throw  new ArgumentNullException(nameof(element));
            Value = element.On;
        }

        public Dictionary<string, GroupResult> Cases { get; } 
        public GroupResult Else { get; set; }

        public override void Parse(ParsingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var valueOfSwitch = context.GetStringValue(Value);
            Children.Clear();

            foreach (var key in Cases.Keys)
            {
                var valueOfCase = context.GetStringValue(key);
                if (valueOfCase == valueOfSwitch)
                {
                    var gp = Cases[key].Clone();
                    gp.Value = key;
                    Children.Add(gp);
                    return;
                }
            }

            if (Else != null)
            {
                var gp = Else.Clone();
                Children.Add(gp);
            }
            else
                throw new ParserException("Invalid switch: case not found and else not defined", this);

        }

        public override ParsingResultNode Clone()
        {
            var newItem = new SwitchResult(Label);
            newItem.Value = Value;
            foreach (var key in Cases.Keys)
            {
                newItem.Cases.Add(key, (GroupResult) Cases[key].Clone());
            }
            newItem.Else = (GroupResult) Else.Clone();

            return newItem;


        }
    }
}
