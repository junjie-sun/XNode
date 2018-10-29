using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Client.Console.Services
{
    public class SimpleService : ISimpleService
    {
        public Task<string> Test(SimpleInfo info)
        {
            throw new NotImplementedException();
        }
    }
}
