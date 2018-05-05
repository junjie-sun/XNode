// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XNode.Server.Console.XNodeServer
{
    [Service("CustomerService", 10001, true)]
    public class CustomerService : ServiceBase, ICustomerService
    {
        private static ConcurrentBag<Customer> customers = new ConcurrentBag<Customer>();

        [Action("GetServiceName", 1)]
        public override string GetServiceName()
        {
            return "CustomerService";
        }

        [Action("AddCustomer", 2)]
        public Task AddCustomer(Customer customer)
        {
            return Task.Run(() =>
            {
                customers.Add(customer);
            });
        }

        [Action("GetCustomer", 3)]
        public Customer GetCustomer(int id)
        {
            return customers.Where((customer) => customer.Id == id).FirstOrDefault();
        }

        [Action("QueryCustomer", 4)]
        public Task<List<Customer>> QueryCustomer(int? id, string name)
        {
            return Task.Run<List<Customer>>(() =>
            {
                return customers.Where(c => (id == null || c.Id == id.Value) && (string.IsNullOrEmpty(name) || c.Name == name)).OrderBy((c) => c.Id).ToList();
            });
        }

        [Action("RemoveCustomer", 5)]
        public void RemoveCustomer(int id)
        {
            var customer = customers.Where((c) => c.Id == id).FirstOrDefault();
            if (customer == null)
            {
                return;
            }
            customers.TryTake(out customer);
        }

        [Action("RemoveAllCustomer", 6)]
        protected void RemoveAllCustomer()
        {
            customers = new ConcurrentBag<Customer>();
        }
    }
}
