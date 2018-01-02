#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Dotc.MQExplorerPlus.Core
{
    public static class TraceHelper
    {
        private static readonly Lazy<TraceSource> TraceSourceLoader = new Lazy<TraceSource>(
            () => new TraceSource("mqexplorerplus"));
        public static TraceSource Source
        {
            get { return TraceSourceLoader.Value; }
        }
        public static void Log(this Exception ex)
        {
            if (ex == null) return;

            Source.TraceEvent(TraceEventType.Error, 1, ex.FullMessage());
        }

        private static string FullMessage(this Exception ex)
        {
            return FullMessageCore(ex, 0);
        }

        private static string FullMessageCore(Exception ex, int indent)
        {
            var indentString = new string('|', indent * 3);
            return ex.InnerException == null
                ? DumpException(ex, indentString)
                : DumpException(ex, indentString) + indentString + "--- INNER EXCEPTION ---" + Environment.NewLine + FullMessageCore(ex.InnerException, ++indent);
        }

        private static string DumpException(Exception ex, string indentString)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indentString + string.Format( CultureInfo.InvariantCulture, "{0}: {1}", ex.GetType().FullName, ex.Message));
            sb.AppendLine(indentString + ex.StackTrace);
            if (ex.Data.Count > 0)
            {
                sb.AppendLine(indentString + "  Extra details:");
                foreach (DictionaryEntry entry in ex.Data)
                {
                    sb.AppendLine(indentString + string.Format(CultureInfo.InvariantCulture, "    Key: {0,-20}      Value: {1}", entry.Key, entry.Value));
                }
            }
            return sb.ToString();
        }
    }
}
