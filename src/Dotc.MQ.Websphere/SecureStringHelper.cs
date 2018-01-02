#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Dotc.MQ.Websphere
{
    internal static class SecureStringHelper
    {

        internal static string ConvertToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException(nameof(securePassword));
            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        internal static SecureString ConvertToSecureString(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            unsafe
            {
                fixed (char* passwordChars = password)
                {
                    var securePassword = new SecureString(passwordChars, password.Length);
                    securePassword.MakeReadOnly();
                    return securePassword;
                }
            }
        }

    }
}
