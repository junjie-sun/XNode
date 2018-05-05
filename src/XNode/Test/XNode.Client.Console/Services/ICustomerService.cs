// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Client.Console.Services
{
    [ServiceProxy("CustomerService", 10001, true)]
    public interface ICustomerService
    {
        [ActionProxy("GetServiceName", 1)]
        string GetServiceName();

        [ActionProxy("AddCustomer", 2)]
        Task AddCustomer(Customer customer);

        [ActionProxy("GetCustomer", 3)]
        Customer GetCustomer(int id);

        [ActionProxy("QueryCustomer", 4)]
        Task<List<Customer>> QueryCustomer(int? id, string name);

        [ActionProxy("RemoveCustomer", 5)]
        void RemoveCustomer(int id);

        [ActionProxy("RemoveAllCustomer", 6)]
        void RemoveAllCustomer();
    }
}
