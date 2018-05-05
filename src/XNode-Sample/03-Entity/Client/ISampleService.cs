// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Client;

namespace Client
{
    [ServiceProxy("SampleService", 10001)]
    public interface ISampleService
    {
        [ActionProxy("Welcome", 1)]
        string Welcome(Name name);

        [ActionProxy("GetOrders", 2)]
        Task<IList<Order>> GetOrders();
    }
}
