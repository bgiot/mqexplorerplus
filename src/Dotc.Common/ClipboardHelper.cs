/* 
  
The MIT License (MIT) 
 
Copyright (c) 2014 Kolibri 
 
Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 copies of the Software, and to permit persons to whom the Software is 
 furnished to do so, subject to the following conditions: 
  
 The above copyright notice and this permission notice shall be included in 
 all copies or substantial portions of the Software. 
  
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 THE SOFTWARE. 
   */ 

using System;
using System.Runtime.InteropServices;
using System.Text;


namespace Dotc.Common
{
    public static class ClipboardHelper
    {

        public enum ResultCode
        {
            Success = 0,


            ErrorOpenClipboard = 1,
            ErrorGlobalAlloc = 2,
            ErrorGlobalLock = 3,
            ErrorSetClipboardData = 4,
            ErrorOutOfMemoryException = 5,
            ErrorArgumentOutOfRangeException = 6,
            ErrorException = 7,
            ErrorInvalidArgs = 8,
            ErrorGetLastError = 9
        };


        public class Result
        {
            public ResultCode ResultCode { get; internal set; }

            public uint LastError { get; internal set; }

            public bool OK => ResultCode.Success == ResultCode;
        }



        [STAThread]
        public static Result PushStringToClipboard(string message)
        {
            var isAscii = (message != null && (message == Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(message))));
            if (!isAscii)
            {
                return PushUnicodeStringToClipboard(message);
            }
            else
            {
                return PushAnsiStringToClipboard(message);
            }
        }


        [STAThread]
        public static Result PushUnicodeStringToClipboard(string message)
        {
            return __PushStringToClipboard(message, CF_UNICODETEXT);
        }


        [STAThread]
        public static Result PushAnsiStringToClipboard(string message)
        {
            return __PushStringToClipboard(message, CF_TEXT);
        }


        // ReSharper disable InconsistentNaming 
        const uint CF_TEXT = 1;
        const uint CF_UNICODETEXT = 13;
        // ReSharper restore InconsistentNaming 


        [STAThread]
        private static Result __PushStringToClipboard(string message, uint format)
        {
            try
            {
                try
                {
                    if (message == null)
                    {
                        return new Result {ResultCode = ResultCode.ErrorInvalidArgs};
                    }

                    if (!NativeMethods.OpenClipboard(IntPtr.Zero))
                    {
                        var lastError = NativeMethods.GetLastError();
                        return new Result {ResultCode = ResultCode.ErrorOpenClipboard, LastError = lastError};
                    }

                    if (!NativeMethods.EmptyClipboard())
                    {
                        var lastError = NativeMethods.GetLastError();
                        return new Result { ResultCode = ResultCode.ErrorOpenClipboard, LastError = lastError };
                    }

                    try
                    {
                        uint sizeOfChar;
                        switch (format)
                        {
                            case CF_TEXT:
                                sizeOfChar = 1;
                                break;
                            case CF_UNICODETEXT:
                                sizeOfChar = 2;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("format");
                        }


                        var characters = (uint) message.Length;
                        uint bytes = (characters + 1)*sizeOfChar;


                        // ReSharper disable once InconsistentNaming 
                        const int GMEM_MOVABLE = 0x0002;
                        // ReSharper disable once InconsistentNaming 
                        const int GMEM_ZEROINIT = 0x0040;
                        // ReSharper disable once InconsistentNaming 
                        const int GHND = GMEM_MOVABLE | GMEM_ZEROINIT;


                        // IMPORTANT: SetClipboardData requires memory that was acquired with GlobalAlloc using GMEM_MOVABLE. 
                        var hGlobal = NativeMethods.GlobalAlloc(GHND, (UIntPtr) bytes);
                        if (hGlobal == IntPtr.Zero)
                        {
                            var lastError = NativeMethods.GetLastError();
                            return new Result {ResultCode = ResultCode.ErrorGlobalAlloc, LastError =lastError};
                        }


                        try
                        {
                            // IMPORTANT: Marshal.StringToHGlobalUni allocates using LocalAlloc with LMEM_FIXED. 
                            //            Note that LMEM_FIXED implies that LocalLock / LocalUnlock is not required. 
                            IntPtr source;
                            switch (format)
                            {
                                case CF_TEXT:
                                    source = Marshal.StringToHGlobalAnsi(message);
                                    break;
                                case CF_UNICODETEXT:
                                    source = Marshal.StringToHGlobalUni(message);
                                    break;
                                default:
                                    throw new  ArgumentOutOfRangeException("format");
                            }

                            try
                            {
                                var target = NativeMethods.GlobalLock(hGlobal);
                                if (target == IntPtr.Zero)
                                {
                                    var lastError = NativeMethods.GetLastError();
                                    return new Result
                                    {
                                        ResultCode = ResultCode.ErrorGlobalLock,
                                        LastError = lastError
                                    };
                                }


                                try
                                {
                                    NativeMethods.CopyMemory(target, source, bytes);
                                }
                                finally
                                {
                                    var ignore = NativeMethods.GlobalUnlock(target);
                                }


                                if (NativeMethods.SetClipboardData(format, hGlobal).ToInt64() != 0)
                                {
                                    // IMPORTANT: SetClipboardData takes ownership of hGlobal upon success. 
                                    hGlobal = IntPtr.Zero;
                                }
                                else
                                {
                                    var lastError = NativeMethods.GetLastError();
                                    return new Result
                                    {
                                        ResultCode = ResultCode.ErrorSetClipboardData,
                                        LastError = lastError
                                    };
                                }
                            }
                            finally
                            {
                                // Marshal.StringToHGlobalUni actually allocates with LocalAlloc, thus we should theorhetically use LocalFree to free the memory... 
                                // ... but Marshal.FreeHGlobal actully uses a corresponding version of LocalFree internally, so this works, even though it doesn't 
                                //  behave exactly as expected. 
                                Marshal.FreeHGlobal(source);
                            }
                        }
                        catch (OutOfMemoryException)
                        {
                            var lastError = NativeMethods.GetLastError();
                            return new Result
                            {
                                ResultCode = ResultCode.ErrorOutOfMemoryException,
                                LastError = lastError
                            };
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            var lastError = NativeMethods.GetLastError();
                            return new Result
                            {
                                ResultCode = ResultCode.ErrorArgumentOutOfRangeException,
                                LastError = lastError
                            };
                        }
                        finally
                        {
                            if (hGlobal != IntPtr.Zero)
                            {
                                var ignore = NativeMethods.GlobalFree(hGlobal);
                            }
                        }
                    }
                    finally
                    {
                        NativeMethods.CloseClipboard();
                    }
                    return new Result {ResultCode = ResultCode.Success};
                }
                catch (Exception)
                {
                    var lastError = NativeMethods.GetLastError();
                    return new Result {ResultCode = ResultCode.ErrorException, LastError = lastError};
                }
            }
            catch (Exception)
            {
                return new Result {ResultCode = ResultCode.ErrorGetLastError};
            }
        }

    }
}
