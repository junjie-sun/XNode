using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using XNode.Client;

namespace XNode.Server.Console.XNodeServer
{
    [Service("SimpleService", 20001)]
    public interface ISimpleService
    {
        [Action("Test", 1)]
        Task<string> Test(SimpleInfo info);

        [Action("Test2", 2)]
        Task<string> Test2(SimpleInfo info);
    }

    [Service("SimpleService2", 20002)]
    [ServiceProxy("SimpleService2", 20002, true)]
    public interface ISimpleService2
    {
        [Action("Test", 1)]
        [ActionProxy("Test", 1)]
        Task<string> Test(SimpleInfo info);
    }

    [DataContract]
    public class SimpleInfo
    {
        [DataMember(Order = 1)]
        public int Id { get; set; }

        [DataMember(Order = 2)]
        public string Name { get; set; }
    }
}
