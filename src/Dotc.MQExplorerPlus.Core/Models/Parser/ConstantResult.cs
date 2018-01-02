#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;

namespace Dotc.MQExplorerPlus.Core.Models.Parser
{
    public class ConstantResult : ParsingResultNode
    {
        public ConstantResult(string label) : base(ParsingResultNodeType.Constant)
        {
            if (label == null) throw new ArgumentNullException(nameof(label));
            Label = label;
        }
        public ConstantResult(Configuration.ConstantElement element) : this(element?.Label)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            ExpectedValue = element.Value;
            Length = element.Value.Length;
        }

        public string ExpectedValue { get; private set; }

        public override void Parse(ParsingContext context)
        {
            if (context == null)throw new ArgumentNullException(nameof(context));
            context.MessageReader.ReadForward(this);
            if (Value != ExpectedValue)
            {
                HasError = true;
                throw new ParserException("Invalid constant check");
            }
        }

        public override ParsingResultNode Clone()
        {
            var newItem = new ConstantResult(Label)
            {
                Length = Length,
                ExpectedValue = ExpectedValue,
            };
            newItem.Children = Children.Clone();
            return newItem;
        }
    }
}

