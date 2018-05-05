// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using XNode.Client;

namespace Client
{
    [ServiceProxy("SampleService", 10001)]
    public interface ISampleService
    {
        [ActionProxy("Welcome", 1)]
        string Welcome(string name);
    }
}
