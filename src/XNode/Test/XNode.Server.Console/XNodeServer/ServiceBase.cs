// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Server.Console.XNodeServer
{
    public abstract class ServiceBase
    {
        public abstract string GetServiceName();

        [Action(1001)]
        public string Test()
        {
            return "Test";
        }
    }
}
