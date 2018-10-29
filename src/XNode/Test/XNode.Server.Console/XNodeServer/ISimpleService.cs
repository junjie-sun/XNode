using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Server.Console.XNodeServer
{
    [Service("SimpleService", 20001, true)]
    public interface ISimpleService
    {
        [Action("Test", 1)]
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
