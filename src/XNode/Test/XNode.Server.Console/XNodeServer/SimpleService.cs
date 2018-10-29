using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Server.Console.XNodeServer
{
    public class SimpleService : ISimpleService
    {
        public Task<string> Test(SimpleInfo info)
        {
            return Task.FromResult(info.Id + "-" + info.Name);
        }
    }
}
