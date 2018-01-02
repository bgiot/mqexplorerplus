#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dotc.MQExplorerPlus.Core.Models
{
    public static class ApplicationInfo
    {
        private static readonly Lazy<Assembly> ProductRootAssembly = new Lazy<Assembly>(GetProductRootAssembly);
        private static readonly Lazy<string> ProductNameLazy = new Lazy<string>(GetProductName);
        private static readonly Lazy<string> VersionLazy = new Lazy<string>(GetVersion);
        private static readonly Lazy<string> CompanyLazy = new Lazy<string>(GetCompany);
        private static readonly Lazy<string> CopyrightLazy = new Lazy<string>(GetCopyright);
        private static readonly Lazy<string> ApplicationPathLazy = new Lazy<string>(GetApplicationPath);


        /// <summary>
        /// Gets the product name of the application.
        /// </summary>
        public static string ProductName => ProductNameLazy.Value;

        /// <summary>
        /// Gets the version number of the application.
        /// </summary>
        public static string Version => VersionLazy.Value;

        /// <summary>
        /// Gets the company of the application.
        /// </summary>
        public static string Company => CompanyLazy.Value;

        /// <summary>
        /// Gets the copyright information of the application.
        /// </summary>
        public static string Copyright => CopyrightLazy.Value;

        /// <summary>
        /// Gets the path for the executable file that started the application, not including the executable name.
        /// </summary>
        public static string ApplicationPath => ApplicationPathLazy.Value;


        private static Assembly GetProductRootAssembly()
        {
            var si = AppDomain.CurrentDomain.SetupInformation;
            var assList = AppDomain.CurrentDomain.GetAssemblies();
            var ass = assList.FirstOrDefault(a => a.Location?.EndsWith("\\" + si.ApplicationName, StringComparison.OrdinalIgnoreCase) ?? false);
            return ass ?? Assembly.GetExecutingAssembly();
        }
        private static string GetProductName()
        {
            var entryAssembly = ProductRootAssembly.Value;
            if (entryAssembly != null)
            {
                var attribute = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(
                    entryAssembly, typeof(AssemblyProductAttribute)));
                return (attribute != null) ? attribute.Product : "";
            }
            return "";
        }

        private static string GetVersion()
        {
            var entryAssembly = ProductRootAssembly.Value;
            if (entryAssembly != null)
            {
                var attribute = ((AssemblyInformationalVersionAttribute)Attribute.GetCustomAttribute(
                    entryAssembly, typeof(AssemblyInformationalVersionAttribute)));
                return (attribute != null) ? attribute.InformationalVersion : "Unknown version";
            }
            return "Unknown version";
        }

        private static string GetCompany()
        {
            var entryAssembly = ProductRootAssembly.Value;
            if (entryAssembly != null)
            {
                var attribute = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(
                    entryAssembly, typeof(AssemblyCompanyAttribute)));
                return (attribute != null) ? attribute.Company : "";
            }
            return "";
        }

        private static string GetCopyright()
        {
            var entryAssembly = ProductRootAssembly.Value;
            if (entryAssembly != null)
            {
                var attribute = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(
                    entryAssembly, typeof(AssemblyCopyrightAttribute));
                return attribute != null ? attribute.Copyright : "";
            }
            return "";
        }

        private static string GetApplicationPath()
        {
            var entryAssembly = ProductRootAssembly.Value;
            return entryAssembly != null ? Path.GetDirectoryName(entryAssembly.Location) : "";
        }

    }
}
