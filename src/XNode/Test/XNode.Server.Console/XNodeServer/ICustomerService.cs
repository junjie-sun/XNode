// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Server.Console.XNodeServer
{
    public interface ICustomerService
    {
        Task AddCustomer(Customer customer);

        Customer GetCustomer(int id);

        Task<List<Customer>> QueryCustomer(int? id, string name);

        void RemoveCustomer(int id);
    }
}
