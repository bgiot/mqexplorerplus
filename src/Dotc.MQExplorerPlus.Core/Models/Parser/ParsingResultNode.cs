#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using Dotc.Common;

namespace Dotc.MQExplorerPlus.Core.Models.Parser
{

    public enum ParsingResultNodeType
    {
        Value = 0,
        Group,
        Switch,
        Loop,
        Constant,
    }
    public class ParsingResultNode : ObservableObject
    {

        protected ParsingResultNode(ParsingResultNodeType type)
        {
            NodeType = type;
            Children = new ParsingResultNodeList();
        }

        public ParsingResultNodeList Children { get; protected set; }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                SetPropertyAndNotify(ref _value, value);
            }
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            protected set
            {
                SetPropertyAndNotify(ref _id, value);
            }
        }

        private int? _start;
        public int? Start
        {
            get { return _start; }
            set { SetPropertyAndNotify(ref _start, value); }
        }

        private int? _length;

        public int? Length
        {
            get
            {
                return _length;
            }
            protected set { SetPropertyAndNotify(ref _length, value); }
        }

        public ParsingResultNodeType NodeType { get; private set; }

        public string Label { get; set; }

        private bool _hasError;
        public bool HasError
        {
            get { return _hasError; }
            set
            {
                SetPropertyAndNotify(ref _hasError, value);
            }
        }

        public void Reset()
        {
            HasError = false;
            if (NodeType == ParsingResultNodeType.Value && Length.HasValue && Length.Value > 0)
            {
                Value = null;
            }
            if (Children != null && Children.Count > 0)
            {
                Children.Reset();
            }
        }

        public virtual void Parse(ParsingContext context)
        { }

        public virtual ParsingResultNode Clone()
        {
            throw new InvalidOperationException();
        }


    }
}
