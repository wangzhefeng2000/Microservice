﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xigadee;

namespace Xigadee
{
    /// <summary>
    /// This class contains and validates the Jwt Token.
    /// This class currently only supports simple HMAC-based verification.
    /// Thanks to http://kjur.github.io/jsjws/tool_jwt.html for verification.
    /// </summary>
    public static partial class JwtTokenExtensionMethods
    {
        public static void ShortcutSetRole(this JwtClaims claims, string role)
        {
            claims[ClaimsIdentity.DefaultRoleClaimType] = role;
        }
    
        public static void ShortcutSetName(this JwtClaims claims, string name)
        {
            claims[ClaimsIdentity.DefaultNameClaimType] = name;
        }
    }
}
