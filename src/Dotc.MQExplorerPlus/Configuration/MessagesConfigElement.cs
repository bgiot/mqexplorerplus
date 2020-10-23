#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Configuration;
using Dotc.MQExplorerPlus.Core.Models;

namespace Dotc.MQExplorerPlus.Configuration
{
    public class MessagesConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("browseLimit", DefaultValue = UserSettings.DefaultBrowseLimit)]
        public int BrowseLimit
        {
            get
            {
                return (int)base["browseLimit"];
            }
            set
            {
                base["browseLimit"] = value;
            }               
        }

        //[ConfigurationProperty("browseMultiThread", DefaultValue = UserSettings.DefaultBrowseMultiThread)]
        //[Obsolete()]
        //public bool BrowseMultiThread
        //{
        //    get
        //    {
        //        return (bool)base["browseMultiThread"];
        //    }
        //    set
        //    {
        //        base["browseMultiThread"] = value;
        //    }
        //}

        [ConfigurationProperty("putPriority", DefaultValue = UserSettings.DefaultPutPriority)]
        public int PutPriority
        {
            get
            {
                return (int)base["putPriority"];
            }
            set
            {
                base["putPriority"] = value;
            }
        }

        //[ConfigurationProperty("searchInMemory", DefaultValue = false)]
        //[Obsolete()]
        //public bool SearchInMemory
        //{
        //    get
        //    {
        //        return (bool)base["searchInMemory"];
        //    }
        //    set
        //    {
        //        base["searchInMemory"] = value;
        //    }
        //}

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            // These 2 settings are not supported anymore
            if (name == "searchInMemory" || name == "browseMultiThread")
                return true;

            return base.OnDeserializeUnrecognizedAttribute(name, value);
        }
    }
}
