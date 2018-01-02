#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
namespace Dotc.MQ.Websphere
{
    public static class WsQueueType
    {
        public const int Unknown = 0;
        public const int Alias = 1;
        public const int Local = 2;
        public const int Remote = 3;
        public const int Transmission = 4;
        public const int Model = 5;

        public static string GetName(int type)
        {
            switch(type)
            {
                case 1: return "Alias";
                case 2: return "Local";
                case 3: return "Remote";
                case 4: return "Transmission";
                case 5: return "Model";
                default: return "Unknown";
            }
        }
    }

}
