// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Client;
using XNode.Server;

namespace Contract
{
    [Service("CustomerService", 10001, true)]
    [ServiceProxy("CustomerService", 10001)]
    public interface ICustomerService
    {
        [Action("GetCustomers", 1)]
        [ActionProxy("GetCustomers", 1)]
        Task<Customer> GetCustomers(int customerId);
    }
}
