#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{
    public static class Extensions
    {
        public static MessageElement Constant(this MessageElement me, string label, int length,
            string value)
        {
            me?.Fields.Add(new ConstantElement { Label = label, Value = value });
            return me;
        }
        public static MessageElement Field(this MessageElement me, string label, int length, string id = "*")
        {
            me?.Fields.Add(new FieldElement { Label = label, Length = length, Id = id });
            return me;
        }

        public static GroupElement Group(this MessageElement me, string label)
        {
            var group = new GroupElement {Label = label};
            me?.Fields.Add(group);
            return group;
        }

        public static GroupElement Constant(this GroupElement me, string label, int length,
            string value)
        {
            me?.Fields.Add(new ConstantElement { Label = label, Value = value });
            return me;
        }
        public static GroupElement Field(this GroupElement me, string label, int length, string id = "*")
        {
            me?.Fields.Add(new FieldElement { Label = label, Length = length, Id = id });
            return me;
        }

        public static LoopElement Loop(this MessageElement me, string label, string from, string to, string step = "1")
        {
            var loop = new LoopElement {Label = label, From = from, To = to, Step = step};
            me?.Fields.Add(loop);
            return loop;
        }

        public static SwitchElement Switch(this MessageElement me, string label, string on)
        {
            var sw = new SwitchElement {Label = label, On = on};
            me?.Fields.Add(sw);
            return sw;
        }

        public static CaseElement Case(this SwitchElement me, string label, string when)
        {
            var c = new CaseElement {Label = label, When = when};
            me?.Cases.Add(c);
            return c;
        }

        public static ElseElement Else(this SwitchElement me, string label)
        {
            var c = new ElseElement { Label = label};
            me?.Cases.Add(c);
            return c;
        }

        public static PartElement Part(this PartCollection me, string id)
        {
            var p = new PartElement {Id = id};         
            me?.Add(p);
            return p;
        }

        public static PartElement Constant(this PartElement me, string label, int length,
    string value)
        {
            me?.Fields.Add(new ConstantElement { Label = label, Value = value });
            return me;
        }
        public static PartElement Field(this PartElement me, string label, int length, string id = "*")
        {
            me?.Fields.Add(new FieldElement { Label = label, Length = length, Id = id });
            return me;
        }

        public static GroupElement Group(this PartElement me, string label)
        {
            var group = new GroupElement { Label = label };
            me?.Fields.Add(group);
            return group;
        }

        public static LoopElement Loop(this PartElement me, string label, string from, string to, string step = "1")
        {
            var loop = new LoopElement { Label = label, From = from, To = to, Step = step };
            me?.Fields.Add(loop);
            return loop;
        }

        public static SwitchElement Switch(this PartElement me, string label, string on)
        {
            var sw = new SwitchElement { Label = label, On = on };
            me?.Fields.Add(sw);
            return sw;
        }

        public static PartRefElement PartRef(this MessageElement me, string label, string partId)
        {
            var pr = new PartRefElement {PartId = partId, Label = label};
            me?.Fields.Add(pr);
            return pr;
        }

        public static PartRefElement PartRef(this GroupElement me, string label, string partId)
        {
            var pr = new PartRefElement { PartId = partId, Label = label };
            me?.Fields.Add(pr);
            return pr;
        }
    }
}
