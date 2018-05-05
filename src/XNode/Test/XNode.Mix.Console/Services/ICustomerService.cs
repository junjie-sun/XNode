// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XNode.Client;
using XNode.Server;

namespace XNode.Mix.Console.Services
{
    [Service("CustomerService", 10001)]
    [ServiceProxy("CustomerService", 10001)]
    public interface ICustomerService
    {
        [Action("AddCustomer", 1)]
        [ActionProxy("AddCustomer", 1)]
        Task AddCustomer(Customer customer);

        [Action("GetCustomer", 2)]
        [ActionProxy("GetCustomer", 2)]
        Customer GetCustomer(int id);

        [Action("QueryCustomer", 3)]
        [ActionProxy("QueryCustomer", 3)]
        Task<List<Customer>> QueryCustomer();

        [Action("RemoveCustomer", 4)]
        [ActionProxy("RemoveCustomer", 4)]
        void RemoveCustomer(int id);

        [Action("RemoveAllCustomer", 5)]
        [ActionProxy("RemoveAllCustomer", 5)]
        void RemoveAllCustomer();

        [Action("GetOrders", 6)]
        [ActionProxy("GetOrders", 6)]
        Task<List<Order>> GetOrders(int customerId);

        [Action("SaveCustomerPhoto", 7)]
        [ActionProxy("SaveCustomerPhoto", 7)]
        Task<bool> SaveCustomerPhoto(byte[] data);

        [Action("GetCustomerPhoto", 8)]
        [ActionProxy("GetCustomerPhoto", 8)]
        Task<byte[]> GetCustomerPhoto();
    }
}
