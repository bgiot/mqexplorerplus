#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Dotc.MQExplorerPlus.Core.Models.Parser
{
    public sealed class ParsingContext : IDisposable
    {
        public ParsingContext(string message)
        {
            Values = new Dictionary<string, string>();
            MessageReader = new StringReaderEx(message);
            GC.SuppressFinalize(this);
        }

        private Dictionary<string, string> Values { get; }

        public void SaveValue(string id, string value)
        {
            if (Values.ContainsKey(id))
            {
                Values[id] = value;
            }
            else
            {
                Values.Add(id, value);
            }
        }
        
        public StringReaderEx MessageReader { get; private set; }


        private bool IsIdRef(string value, out string id)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.StartsWith("{", StringComparison.Ordinal) && value.EndsWith("}", StringComparison.Ordinal))
            {
                // this is a ref id
                id = value.Substring(1, value.Length - 2);
                return true;
            }
            else
            {
                id = null;
                return false;
            }
        }

        public int GetIntValue(string value)
        {
            string id;
            if (IsIdRef(value, out id))
            {
                return int.Parse(Values[id], CultureInfo.InvariantCulture);
            }
            else
            {
                return int.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        public string GetStringValue(string value)
        {
            string id;
            if (IsIdRef(value, out id))
            {
                return Values[id];
            }
            else
            {
                return value;
            }
        }

        public void Dispose()
        {
            MessageReader.Dispose();
        }
    }
}
