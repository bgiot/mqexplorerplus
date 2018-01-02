#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Dotc.MQExplorerPlus.Core.Services;

namespace Dotc.MQExplorerPlus.Core.Models.Parser.Configuration
{

    public class ParserConfiguration : ConfigurationSection
    {

        public static FileType[] FILE_EXTENSIONS = new FileType[]
        {
            new FileType ("Message Parser Definition",".mpd" ),
            new FileType("Xml Document",".xml"),
            new FileType("All Files",".*"),
        };

        public const string SECTION_NAME = "parser";

        public static ParserConfiguration Open(string configFile)
        {

            using (var reader = File.OpenText(configFile))
            {
                string data = reader.ReadToEnd();
                return LoadFromString(data);
            }
        }

        public static ParserConfiguration LoadFromString(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) throw new ArgumentNullException(nameof(data));

            var config = new ParserConfiguration();
            var reader = new XmlTextReader(new StringReader(data.Trim()));
            try
            {
                config.DeserializeSection(reader);
            }
            catch (XmlException ex)
            {
                throw new ParserException("The parsing definition is not a valid xml!", ex);
            }
            var el = (ConfigurationElement)config;
            if (el.ElementInformation.Errors.Count > 0)
            {

                var errorStack = new StringBuilder();
                foreach (var err in el.ElementInformation.Errors)
                {
                    errorStack.AppendLine(((ConfigurationException)err).Message);
                }

                throw new ConfigurationErrorsException(errorStack.ToString());

            }
            return config;
        }

        public string GetRawData()
        {
            var output = new StringWriter(CultureInfo.InvariantCulture);

            using (var writer = new XmlTextWriter(output))
            {
                SaveToStream(writer);
            }
            return output.ToString();

        }

        public void SaveTo(string filename)
        {
            using (var writer = new XmlTextWriter(filename, null))
            {
                SaveToStream(writer);
            }
        }

        private void SaveToStream(XmlTextWriter writer)
        {
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 3;
            writer.IndentChar = ' ';
            SerializeToXmlElement(writer, SECTION_NAME);
        }

        [ConfigurationProperty("parts")]
        public PartCollection Parts
        {
            get
            {
                return (PartCollection)base["parts"];
            }
        }

        [ConfigurationProperty("message")]
        public MessageElement Message
        {
            get { return (MessageElement)base["message"]; }
        }
    }
}
