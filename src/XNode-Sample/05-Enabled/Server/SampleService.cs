// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using XNode.Server;

namespace Server
{
    [Service("SampleService", 10001)]
    public interface ISampleService
    {
        [Action("Welcome", 1)]
        string Welcome(string name);
    }

    public class SampleService : ISampleService
    {
        public string Welcome(string name)
        {
            return $"Hello {name}";
        }
    }
}
