// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class SampleService : ISampleService
    {
        public string Welcome(Name name)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Order>> GetOrders()
        {
            throw new NotImplementedException();
        }
    }
}
