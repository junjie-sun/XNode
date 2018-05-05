// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contract.Service;
using Entity;
using Microsoft.AspNetCore.Mvc;
using Web.DTO;

namespace Web.Controllers
{
    [Route("api/[controller]/[action]")]
    public class DemoController : Controller
    {
        private ICustomerService _customerService;

        private IOrderService _orderService;

        private IGoodsService _goodsService;

        public DemoController(ICustomerService customerService,
            IOrderService orderService,
            IGoodsService goodsService)
        {
            _customerService = customerService;
            _orderService = orderService;
            _goodsService = goodsService;
        }

        [HttpGet]
        public async Task<List<CustomerDTO>> GetAllCustomers()
        {
            return CustomerDTO.From(await _customerService.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<CustomerDTO> GetCustomer(int id)
        {
            return CustomerDTO.From(await _customerService.Get(id));
        }

        [HttpGet]
        public async Task<List<GoodsDTO>> GetAllGoods()
        {
            return GoodsDTO.From(await _goodsService.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<GoodsDTO> GetGoods(int id)
        {
            return GoodsDTO.From(await _goodsService.Get(id));
        }
    }
}
