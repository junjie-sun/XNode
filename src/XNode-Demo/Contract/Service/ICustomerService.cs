// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Client;
using XNode.Server;

namespace Contract.Service
{
    [Service("CustomerService", 10003)]
    [ServiceProxy("CustomerService", 10003)]
    public interface ICustomerService
    {
        [Action("GetAllCustomer", 1)]
        [ActionProxy("GetAllCustomer", 1)]
        Task<List<Customer>> GetAll();

        [Action("GetCustomer", 2)]
        [ActionProxy("GetCustomer", 2)]
        Task<Customer> Get(int customerId);
    }
}
