#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Dotc.MQExplorerPlus
{
    class Program
    {
        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        static void Main()
        {
            // Read the main configuration file
            var xmlDoc = ReadConfiguration();

            // if MQ is installed, we need to inject in the configuration file
            // the necessary bindingredirect to use the amqmdnet installed in the GAC (even if older than the one ship with the tool)
            bool mqInstalled = CheckMqInstalled(out string mqVersion);
            if (mqInstalled)
            {
                AddBindingRedirect(xmlDoc, mqVersion);
            }

            // Prepare a new application domain setup with the modified configuration file
            var config = xmlDoc.ToString();
            var setup = new AppDomainSetup();
            setup.SetConfigurationBytes(Encoding.Default.GetBytes(config));
            var newdomain = AppDomain.CreateDomain("MQExplorerPlusWithRightAPI", new Evidence(), setup);

            void startupaction()
            {
                Thread thread = new Thread(() =>
                   {
                       App.Main();
                   });
                thread.SetApartmentState(
                   ApartmentState.STA);
                thread.Start();
            }

            newdomain.DoCallBack(startupaction);

        }

        private static void AddBindingRedirect(XDocument xmlDoc, string destinationVersion)
        {

            var currentVersion = GetLocalApiVersion();

            if (currentVersion == destinationVersion) return; // No binding to add because it's same version

            XNamespace localNs = "urn:schemas-microsoft-com:asm.v1";

            var bindingElement = new XElement(
                localNs + "dependentAssembly",
                new XElement(localNs + "assemblyIdentity",
                    new XAttribute("name", "amqmdnet"),
                    new XAttribute("publicKeyToken", "dd3cb1c9aae9ec97"),
                    new XAttribute("culture", "neutral")),
                new XElement(localNs + "bindingRedirect",
                    new XAttribute("oldVersion", currentVersion),
                    new XAttribute("newVersion", destinationVersion)));

            // Add the new binding to the main configuration
            var assBindingNode = xmlDoc.XPathSelectElement("/configuration/runtime/assemblyBinding");
            if (assBindingNode == null)
            {
                var runtimeNode = xmlDoc.XPathSelectElement("/configuration/runtime");
                if (runtimeNode == null)
                {
                    // create the missing runtime node inside configuration
                    runtimeNode = new XElement("runtime");
                    xmlDoc.XPathSelectElement("/configuration").Add(runtimeNode);
                }

                // create the missing assemblyBinding node inside the runtime node 
                assBindingNode = new XElement(localNs + "assemblyBinding");

                runtimeNode.Add(assBindingNode);

            }

            assBindingNode.Add(bindingElement);

        }

        private static string GetLocalApiVersion()
        {

            // Find version of the local amqmdnet dll ship with the tool
            var localPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var localApiFile = Path.Combine(localPath, "amqmdnet.dll");
            var localApidll = Assembly.LoadFile(localApiFile);
            return localApidll.GetName().Version.ToString();
        }

        private static XDocument ReadConfiguration()
        {
            return XDocument.Load(File.OpenRead(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static bool CheckMqInstalled(out string version)
        {
            version = null;
            try
            {
                // To check if Websphere MQ is installed we rely on the fact the the product installs
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
    }

}
