#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.IO;
using static  System.FormattableString;

namespace Dotc.MQExplorerPlus.Core.Models.Parser
{
    public sealed class StringReaderEx : IDisposable
    {

        private readonly StringReader _internalReader;
        private readonly System.Reflection.FieldInfo _posField;

        private readonly int _length;

        public StringReaderEx(string data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data)); 
            _length = data.Length;
            _internalReader = new StringReader(data);
            _posField = typeof(StringReader).GetField("_pos", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField);
            GC.SuppressFinalize(this);
        }

        public int GetPosition()
        {
            return (int)_posField.GetValue(_internalReader);
        }

        private string ReadForward(int count, out int missingCount)
        {
            char[] buffer = new char[count];
            int start = GetPosition();
            missingCount = (start + count) - _length;
            if (missingCount <= 0)
            {
                _internalReader.ReadBlock(buffer, 0, count);
                return new string(buffer);
            }
            else
            {
                return null;
            }
        }

        public void ReadForward(ParsingResultNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            var missingCount = 0;
            node.Start = GetPosition();
            if (node.Length != null) node.Value = ReadForward(node.Length.Value, out missingCount);
            if (node.Value == null)
            {
                node.HasError = true;
                node.Value = ReadToEnd();
                throw new ParserException(Invariant($"End of message reached! Missing {missingCount} char(s)!"), node);
            }
        }

        public string ReadToEnd()
        {
            return _internalReader.ReadToEnd();
        }

        public int Peek()
        {
            return _internalReader.Peek();
        }

        public void Dispose()
        {
            _internalReader.Dispose();
        }
    }
}
