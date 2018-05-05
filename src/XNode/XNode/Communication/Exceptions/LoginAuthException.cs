// Copyright (c) junjie sun. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace XNode.Communication
{
    public class LoginAuthException : Exception
    {
        public LoginAuthException(byte loginResult, string message) : base(message)
        {
            LoginResult = loginResult;
        }

        public byte LoginResult { get; }
    }
}
