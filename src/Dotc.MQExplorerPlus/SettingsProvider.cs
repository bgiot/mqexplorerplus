#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Xml;
using Dotc.MQExplorerPlus.Core;
using Dotc.MQExplorerPlus.Core.Models;
using Dotc.MQExplorerPlus.Configuration;
using ISettingsProvider = Dotc.MQExplorerPlus.Core.Services.ISettingsProvider;

namespace Dotc.MQExplorerPlus
{
    [Export(typeof(ISettingsProvider))]
    public class SettingsProvider : ISettingsProvider
    {

        public SettingsProvider()
        {
            CurrentAppSettings = new AppSettings();
        }

        public IAppSettings CurrentAppSettings { get; private set; }

        private string GetUserSettingsFilename()
        {
            var rootpath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var settingsFile = Path.Combine(rootpath, "Dot Consulting", "MQExplorerPlus.settings");
            return settingsFile;

        }

        public IUserSettings ReadUserSettings()
        {

            var settingsFile = GetUserSettingsFilename();

            if (File.Exists(settingsFile))
            {
                try
                {

                    var section = new UserSettingsConfiguration();
                    using (var file = File.OpenRead(settingsFile))
                    {
                        var reader = new XmlTextReader(file);
                        section.Deserialize(reader);
                        return section;
                    }

                }
                catch (Exception ex)
                {
                    ex.Log();
                    return null;
                }
            }
            else
            {
                return null;
            }

        }

        public void WriteUserSettings(IUserSettings settings)
        {

            var settingsFile = GetUserSettingsFilename();

            if (!File.Exists(settingsFile))
            {
                var dir = Path.GetDirectoryName(settingsFile);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            var section = new UserSettingsConfiguration();
            section.Set(settings);
            using (var writer = new XmlTextWriter(settingsFile, null ))
            {
                writer.Formatting = Formatting.Indented;
                section.Serialize(writer);
            }
        }

        public IAppSettings GetAppSettings()
        {
            return CurrentAppSettings;
        }
    }

    internal class AppSettings : IAppSettings
    {
        internal AppSettings()
        {
        }

    }
}
