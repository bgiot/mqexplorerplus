#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Globalization;

namespace Dotc.MQExplorerPlus.Core.Models.Parser
{
    public class LoopResult : ParsingResultNode
    {

        public LoopResult(string label) : base(ParsingResultNodeType.Loop)
        {
            if( label==null) throw  new ArgumentNullException(nameof(label));
            Label = label;
        }
        public LoopResult(Configuration.LoopElement element) : this(element?.Label)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            From = element.From;
            To = element.To;
            Step = element.Step;

            Template = new ParsingResultNodeList();
        }

        public string From { get; private set; }
        public string To { get; private set; }
        public string Step { get; private set; }

        public ParsingResultNodeList Template { get; private set; }

        public override void Parse(ParsingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            Children.Clear();

            int from = context.GetIntValue(From);
            int to = context.GetIntValue(To);
            int step = context.GetIntValue(Step);

            if (step == 0) throw  new ParserException("Invalid Loop: step cannot be 0", this);
            if (step > 0 && to < from) throw new ParserException("Invalid loop", this);
            if (step < 0 && to > from) throw new ParserException("Invalid loop", this);

            int index = from;
            while (index*step <= to*step)
            {
                var gp = new GroupResult(index.ToString(CultureInfo.InvariantCulture));
                Children.Add(gp);
                gp.Children.AddRange(Template.Clone());

                index = index + step;
            }

        }

        public override ParsingResultNode Clone()
        {
            var newItem = new LoopResult(Label);
            newItem.From = From;
            newItem.To = To;
            newItem.Step = Step;
            newItem.Template = Template.Clone();
            return newItem;
        }
    }
}
