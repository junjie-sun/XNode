using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace XNode.Common
{
    /// <summary>
    /// Host字符串扩展方法 
    /// </summary>
    public static class HostStringExtensions
    {
        /// <summary>
        /// 将Host字符串转换为IPAddress
        /// </summary>
        /// <param name="host">Host字符串，可以是IP、主机名或域名</param>
        /// <returns></returns>
        public async static Task<IPAddress> ToIPAddress(this string host)
        {
            if (!IPAddress.TryParse(host, out IPAddress ipAddress))
            {
                var ips = await Dns.GetHostAddressesAsync(host);
                if (ips.Length > 0)
                {
                    if (ips[0].IsIPv4MappedToIPv6)
                    {
                        ipAddress = ips[0].MapToIPv4();
                    }
                    else
                    {
                        ipAddress = ips[0];
                    }
                }
                if (ipAddress == null)
                {
                    throw new InvalidOperationException("Host is invalid.");
                }
            }
            return ipAddress;
        }
    }

    /// <summary>
    /// IPAddress扩展方法
    /// </summary>
    public static class IPAddressExtensions
    {
        /// <summary>
        /// 获取IP地址
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static string ToIPString(this IPAddress ipAddress)
        {
            if (ipAddress.IsIPv4MappedToIPv6)
            {
                return ipAddress.MapToIPv4().ToString();
            }
            return ipAddress.ToString();
        }
    }
}
