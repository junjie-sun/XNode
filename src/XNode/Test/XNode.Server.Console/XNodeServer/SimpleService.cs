using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Server.Console.XNodeServer
{
    public class SimpleService : ISimpleService
    {
        private ISimpleService2 simpleService2;

        public SimpleService(ISimpleService2 simpleService2)
        {
            this.simpleService2 = simpleService2;
        }

        public Task<string> Test(SimpleInfo info)
        {
            return Task.FromResult(info.Id + "-" + info.Name);
        }

        public Task<string> Test2(SimpleInfo info)
        {
            info.Name += "-SimpleService";
            return simpleService2.Test(info);
        }
    }

    public class SimpleService2 : ISimpleService2
    {
        public Task<string> Test(SimpleInfo info)
        {
            return Task.FromResult(info.Id + "-" + info.Name + "-SimpleService2");
        }
    }
}
