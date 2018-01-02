#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;

namespace Dotc.MQExplorerPlus.Core.Models.Parser
{
    public class FieldResult : ParsingResultNode
    {

        public FieldResult(string label) : base(ParsingResultNodeType.Value)
        {
            Label = label;
        }

        public FieldResult(Configuration.FieldElement element) : this(element?.Label)
        {
            if(element == null) throw  new ArgumentNullException(nameof(element));
            Length = element.Length;
            if (!string.IsNullOrEmpty(element.Id) && element.Id != "*")
            {
                Id = element.Id;
            }
        }


        public override void Parse(ParsingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.MessageReader.ReadForward(this);
            if (Id != null)
            {
                context.SaveValue(Id, Value);
            }
        }

        public override ParsingResultNode Clone()
        {
            var newItem = new FieldResult(Label)
            {
                Length = Length,
                Id = Id,
            };

            newItem.Children = Children.Clone();
            return newItem;
        }


    }
}