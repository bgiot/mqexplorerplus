#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Reflection;
using Dotc.MQ.Websphere.Configuration;
using IBM.WMQ;
using System;

namespace Dotc.MQ.Websphere
{
    internal static class WsSoftwareInfo
    {

        private static void EnsureInit()
        {
            if (!IsInit)
            {
                Init();
                IsInit = true;
            }
        }

        public static bool IsInit { get; private set; }
        private static void Init()
        {

            var ass = Assembly.GetAssembly(typeof(MQQueueManager));
            ApiVersion = ass.GetName().Version.ToString();
            string gacVersion;
            LocalMqIsInstalled = CheckMqInstalled(out gacVersion);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static bool CheckMqInstalled(out string version)
        {
            version = null;
            try
            {
                // To check if Websphere MQ is installed we rely on the fact the product installs
                // a binding policy that redirect all version for amqmdnet to the one installed in the GAC
                // So if version 1.0.0.3 (very first version of the amqmdnet dll) is found, MQ is installed for sure
                var ass = Assembly.Load("amqmdnet, Culture=neutral, Version=1.0.0.3, PublicKeyToken=dd3cb1c9aae9ec97");

                // Thanks to the binding policy the version of the assembly found is of the one inside the GAC
                version = ass.GetName().Version.ToString();
                return ass.GlobalAssemblyCache;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string _apiVersion;
        public static string ApiVersion
        {
            get
            {
                EnsureInit();
                return _apiVersion;
            }
            private set
            {
                _apiVersion = value;
            }
        }

        private static bool _localMqIsInstalled;
        public static bool LocalMqIsInstalled
        {
            get
            {
                EnsureInit();
                return _localMqIsInstalled;
            }
            private set
            {
                _localMqIsInstalled = value;
            }
        }

        public static bool AvoidPCFWhenPossible
        {
            get { return WSConfiguration.Current.AvoidPCFWhenPossible; }
        }
    }
}
