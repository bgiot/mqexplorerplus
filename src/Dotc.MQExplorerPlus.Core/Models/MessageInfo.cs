#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Xml;
using Dotc.MQ;
using Dotc.Wpf.Controls.HexViewer;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace Dotc.MQExplorerPlus.Core.Models
{
    public class MessageInfo : SelectableItem//, IMessage
    {

        public IMessage MessageSource { get; }

        public IByteCharConverter CurrentConverter { get; private set; }

        private Lazy<HexViewerModel> HexLoader;
        private Lazy<XmlDocument> XmlLoader;
        private Lazy<JToken> JsonLoader;

        public MessageInfo(IMessage message, IByteCharConverter converter, int indexOffset = 0)
        {
            CurrentConverter = converter;
            MessageSource = message;
            BuildData(indexOffset);
        }

        private void BuildData(int indexOffset)
        {

            if (MessageSource.Index.HasValue)
            {
                _index = MessageSource.Index.Value + indexOffset;
            }

            _preview = Text?.Replace("\n", "").Replace("\r", "");

            HexLoader = new Lazy<HexViewerModel>(() => new HexViewerModel(MessageSource.Bytes, CurrentConverter), true);

            XmlLoader = new Lazy<XmlDocument>(
                () =>
                {
                    try
                    {
                        var x = new XmlDocument();
                        x.LoadXml(MessageSource.Text ?? "<not_an_xml_structure />");
                        return x;
                    }
                    catch (XmlException)
                    {
                        var x = new XmlDocument();
                        x.LoadXml("<not_an_xml_structure />");
                        return x;
                    }
                }, true);


            JsonLoader = new Lazy<JToken>(
                () =>
                {
                    if (!string.IsNullOrEmpty((MessageSource.Text)))
                    {
                        try
                        {
                            return JToken.Parse(MessageSource.Text);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    return null;
                }, true);

            ProcessExtendedProperties();

        }

        private void ProcessExtendedProperties()
        {
            if (MessageId != null)
            {
                MessageIdHexString = MessageId.ToHexString();
                MessageIdString = CurrentConverter.ToString(MessageId);
            }
            if (MessageSource.ExtendedProperties.GroupId != null)
            {
                var b = (byte[])MessageSource.ExtendedProperties.GroupId;
                GroupIdHexString = b.ToHexString();
                GroupIdString = CurrentConverter.ToString(b);
            }
            if (MessageSource.ExtendedProperties.CorrelationId != null)
            {
                var b = (byte[])MessageSource.ExtendedProperties.CorrelationId;
                CorrelationIdHexString = b.ToHexString();
                CorrelationIdString = CurrentConverter.ToString(b);
            }
        }

        public void ChangeConverter (IByteCharConverter newConverter)
        {
            if( newConverter != CurrentConverter)
            {
                CurrentConverter = newConverter;
                if (MessageId != null)
                {
                    MessageIdString = CurrentConverter.ToString(MessageId);
                    OnPropertyChanged(nameof(MessageIdString));
                }
                if (MessageSource.ExtendedProperties.GroupId != null)
                {
                    var b = (byte[])MessageSource.ExtendedProperties.GroupId;
                    GroupIdString = CurrentConverter.ToString(b);
                    OnPropertyChanged(nameof(GroupIdString));
                }
                if (MessageSource.ExtendedProperties.CorrelationId != null)
                {
                    var b = (byte[])MessageSource.ExtendedProperties.CorrelationId;
                    CorrelationIdString = CurrentConverter.ToString(b);
                    OnPropertyChanged(nameof(CorrelationIdString));
                }

                if (HexLoader.IsValueCreated)
                {
                    HexLoader.Value.CharConverter = newConverter;
                }
            }
        }

        private int? _index;
        public int? Index => _index;

        public IQueue Queue => MessageSource.Queue;

        public dynamic ExtendedProperties => MessageSource.ExtendedProperties;

        public byte[] MessageId => MessageSource.MessageId;

        public string MessageIdHexString { get; private set; }
        public string MessageIdString { get; private set; }

        public string CorrelationIdHexString { get; private set; }
        public string CorrelationIdString { get; private set; }

        public string GroupIdHexString { get; private set; }
        public string GroupIdString { get; private set; }

        public string Text => MessageSource.Text;

        public DateTime PutTimestamp => MessageSource.PutTimestamp;

        public int Length => MessageSource.Length;

        private string _preview;
        public string PreviewText => _preview;

        public HexViewerModel Hex
        {
            get
            {
                return HexLoader.Value;
            }
        }

        public XmlDocument Xml
        {
            get
            {
                return XmlLoader.Value;
            }
        }

        public JToken JsonToken
        {
            get
            {
                return JsonLoader.Value;
            }
        }
    }
}
