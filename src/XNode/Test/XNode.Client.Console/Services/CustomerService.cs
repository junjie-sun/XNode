// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Client.Console.Services
{
    public class CustomerService : ICustomerService
    {
        public Task AddCustomer(Customer customer)
        {
            return Task.Run(() =>
            {
                System.Console.WriteLine("AddCustomer local call");
            });
        }

        public Customer GetCustomer(int id)
        {
            System.Console.WriteLine("GetCustomer local call");
            return new Customer();
        }

        public string GetServiceName()
        {
            System.Console.WriteLine("GetServiceName local call");
            return string.Empty;
        }

        public Task<List<Customer>> QueryCustomer(int? id, string name)
        {
            return Task.Run<List<Customer>>(() =>
            {
                System.Console.WriteLine("QueryCustomer local call");
                return new List<Customer>();
            });
        }

        public void RemoveAllCustomer()
        {
            System.Console.WriteLine("RemoveAllCustomer local call");
        }

        public void RemoveCustomer(int id)
        {
            System.Console.WriteLine("RemoveCustomer local call");
        }
    }
}
