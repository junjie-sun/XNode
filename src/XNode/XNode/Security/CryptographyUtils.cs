// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace XNode.Security
{
    public static class CryptographyUtils
    {
        #region MD5

        /// <summary>
        /// MD5 加密
        /// </summary>
        /// <param name="original">The input.</param>
        /// <returns></returns>
        public static string MD5Encrypt(string original)
        {
            byte[] hash = null;
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(original);
                hash = md5.ComputeHash(inputBytes);
            }
            if (hash == null)
            {
                return null;
            }
            return GetHashString(hash);
        }

        #endregion

        #region SHA1

        /// <summary>
        /// SHA1 加密
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static string SHA1Encrypt(string original)
        {
            byte[] hash = null;
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(original);
                hash = sha1.ComputeHash(inputBytes);
            }
            if (hash == null)
            {
                return null;
            }
            return GetHashString(hash);
        }

        #endregion

        #region SHA256

        public static string SHA256Encrypt(string original)
        {
            byte[] hash = null;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(original);
                hash = sha256.ComputeHash(inputBytes);
            }
            if (hash == null)
            {
                return null;
            }
            return GetHashString(hash);
        }

        #endregion

        #region Signature

        /// <summary>
        /// 默认签名算法
        /// </summary>
        /// <param name="accountName">账号名称</param>
        /// <param name="noncestr">随机字符串</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="accountKey">账号密钥</param>
        /// <returns></returns>
        public static string GetDefaultSignature(string accountName, string noncestr, string timestamp, string accountKey)
        {
            return MD5Encrypt($"accountName={accountName}&noncestr={noncestr}&timestamp={timestamp}&accountKey={accountKey}");
        }

        #endregion

        #region Other

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="hasComplex">是否含有复杂字符</param>
        /// <param name="length">生成的字符串长度</param>
        /// <returns></returns>
        public static string getNoncestr(bool hasComplex, int length)
        {
            string str = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (hasComplex)
            {
                str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
            }
            StringBuilder SB = new StringBuilder();
            Random rd = new Random();
            for (int i = 0; i < length; i++)
            {
                SB.Append(str.Substring(rd.Next(0, str.Length), 1));
            }
            return SB.ToString();
        }

        #endregion

        #region 私有方法

        private static string GetHashString(byte[] hash)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        #endregion
    }
}
