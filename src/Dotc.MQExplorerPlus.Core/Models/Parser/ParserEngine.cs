#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using Dotc.Common;
using Dotc.MQExplorerPlus.Core.Models.Parser.Configuration;
using static System.FormattableString;

namespace Dotc.MQExplorerPlus.Core.Models.Parser
{
    public sealed class ParserEngine : ObservableObject
    {

        public ParserEngine()
        {
            Result = new ParsingResult();
        }

        private ParsingResult _result;
        public ParsingResult Result
        {
            get
            { return _result; }

            private set
            {
                SetPropertyAndNotify(ref _result, value);
            }
        }

        private ParserConfiguration _configuration;
        public ParserConfiguration Configuration
        {
            get { return _configuration; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _configuration = value;
                BuildResultTree();
            }
        }

        private string _lastMessageParsed;
        public string LastMessageParsed
        {
            get { return _lastMessageParsed; }
            private set { SetPropertyAndNotify(ref _lastMessageParsed, value); }
        }

        private void BuildResultTree()
        {
            var pr = new ParsingResult();
            BuildResultTreeNodes(pr.Nodes, Configuration.Message.Fields);
            Result = pr;
            if (LastMessageParsed != null)
                ParseMessage(LastMessageParsed);
        }

        private void BuildResultTreeNodes(ParsingResultNodeList prnl, FieldCollectionBase confItems)
        {
            foreach (FieldElementBase item in confItems)
            {
                if (item is ConstantElement)
                {
                    var element = (ConstantElement)item;
                    prnl.Add(new ConstantResult(element));
                }
                else if (item is SwitchElement) // as switch is of field type, it must be checked before
                {
                    var element = (SwitchElement) item;
                    var sw = new SwitchResult(element);
                    prnl.Add(sw);

                    foreach (GroupElement c in element.Cases)
                    {
                        if (c is CaseElement)
                        {
                            var caseE = (CaseElement) c;
                            var gp = new GroupResult(caseE.Label);
                            sw.Cases.Add(caseE.When, gp);
                            BuildResultTreeNodes(gp.Children, caseE.Fields);
                        }
                        else if (c is ElseElement)
                        {
                            if (sw.Else != null)
                                throw new ParserException("Only 1 else case can be defined in a switch!");
                            var elseE = (ElseElement) c;
                            var gp = new GroupResult(elseE.Label);
                            sw.Else = gp;
                            BuildResultTreeNodes(gp.Children, elseE.Fields);
                        }
                    }

                }
                else if (item is FieldElement)
                {
                    var element = (FieldElement)item;
                    prnl.Add(new FieldResult(element));
                }
                else if (item is LoopElement) // as loop is of type group, it must be checked before
                {
                    var element = (LoopElement)item;
                    var node = new LoopResult(element);
                    prnl.Add(node);
                    BuildResultTreeNodes(node.Template, element.Fields);

                }
                else if (item is GroupElement)
                {
                    var element = (GroupElement)item;
                    var node = new GroupResult(element);
                    prnl.Add(node);
                    BuildResultTreeNodes(node.Children, element.Fields);

                }
                else if (item is PartRefElement)
                {
                    var element = (PartRefElement)item;
                    PartElement pe;
                    if (!Configuration.Parts.TryFindById(element.PartId, out pe))
                        throw new ParserException(Invariant($"Invalid partRef: unknow part id '{element.PartId}'"));
                    var node = new GroupResult(element.Label);
                    prnl.Add(node);
                    BuildResultTreeNodes(node.Children, pe.Fields);
                }
                else
                {
                    throw new ParserException(Invariant($"Unsupported field with name '{item.Label}'"));
                }
            }
        }

        public bool ParseMessage(string message)
        {
            if (Configuration == null) return false;
            Result.Reset();
            LastMessageParsed = message;

            if (message != null)
            {
                var context = new ParsingContext(message);


                try
                {
                    ParseMessageInternal(Result.Nodes, context);
                }
                catch (ParserException)
                {
                    return false;
                }

                if (context.MessageReader.Peek() > 0)
                {
                    Result.UnparsedData = context.MessageReader.ReadToEnd();
                    return false;
                }
            }

            return true;

        }

        private void ParseMessageInternal(ParsingResultNodeList nodes, ParsingContext context)
        {
            foreach (var node in nodes)
            {

                node.Parse(context);
                if (node.Children != null && node.Children.Count > 0)
                {
                    ParseMessageInternal(node.Children, context);
                }
            }
        }
    }
}
