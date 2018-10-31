using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using XNode.Server;

namespace XNode.Client.Console.Services
{
    [ServiceProxy("SimpleService", 20001)]
    public interface ISimpleService
    {
        [ActionProxy("Test", 1)]
        Task<string> Test(SimpleInfo info);

        [ActionProxy("Test2", 2)]
        Task<string> Test2(SimpleInfo info);
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
