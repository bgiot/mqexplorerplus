#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotc.MQ.Websphere
{
    internal static class WsUtils
    {

        internal static Encoding GetEncoding(int ccsid)
        {
            switch (ccsid)
            {
                case 420:
                case 0x1a7:
                case 0x1a8:
                case 0x129:
                case 0x115:
                case 0x116:
                case 280:
                case 0x11c:
                case 0x11d:
                case 290:
                case 0x111:
                case 0x346:
                case 0x367:
                case 880:
                case 0x389:
                case 0x401:
                    ccsid += 0x4e20;
                    break;

                case 0x32d:
                    ccsid = 0x6fb5;
                    break;

                case 0x333:
                    ccsid = 0x6faf;
                    break;

                case 0x390:
                    ccsid = 0x6fb0;
                    break;

                case 0x391:
                    ccsid = 0x6fb1;
                    break;

                case 0x393:
                    ccsid = 0x6fb3;
                    break;

                case 0x394:
                    ccsid = 0x6fb6;
                    break;

                case 920:
                    ccsid = 0x6fb7;
                    break;

                case 0x399:
                    ccsid = 0x6fbb;
                    break;

                case 0x39b:
                    ccsid = 0x6fbd;
                    break;

                case 0x3af:
                    ccsid = 0x3a4;
                    break;

                case 0x36e:
                    ccsid = 0x5182;
                    break;

                case 0x3ba:
                case 0x83ba:
                case 0x13ba:
                    ccsid = 0xcadc;
                    break;

                case 970:
                    ccsid = 0xcaed;
                    break;

                case 0x4d0:
                    ccsid = 0xfdee;
                    break;

                case 0x4d2:
                    ccsid = 0xfded;
                    break;

                case 0x4b8:
                    ccsid = 0xfde9;
                    break;

                case 0x441:
                    ccsid = 0x6fb4;
                    break;

                case 0x4b2:
                    ccsid = 0x4b0;
                    break;

                case 0x565:
                case 0x567:
                case 0x56a:
                case 0x1570:
                    ccsid = 0xd698;
                    break;

                case 0x7e6:
                    ccsid = 0xc42c;
                    break;

                case 0x15e1:
                    ccsid = 0x3b5;
                    break;
            }
            return Encoding.GetEncoding(ccsid);
        }

    }
}
