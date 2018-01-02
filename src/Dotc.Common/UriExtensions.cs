using System;
using System.Diagnostics;

namespace Dotc.Common
{
    public static class UriExtensions
    {
        public static void OpenInBrowser(this Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            try
            {
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
            }
            catch (Exception)
            { }
        }
    }
}
