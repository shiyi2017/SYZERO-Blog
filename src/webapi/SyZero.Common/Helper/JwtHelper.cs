﻿using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;

namespace SyZero.Common
{
   public  class JwtHelper
    {
        public static string GetToken(IEnumerable<Claim> claims)
        {     
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationUtil.GetSection("JWT:SecurityKey")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var authTime = DateTime.UtcNow;
            var expiresAt = authTime.AddDays(int.Parse(ConfigurationUtil.GetSection("JWT:expires")));
            var token = new JwtSecurityToken(
                issuer: ConfigurationUtil.GetSection("JWT:issuer"),
                audience: ConfigurationUtil.GetSection("JWT:audience"),
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds);
            string usertoken = new JwtSecurityTokenHandler().WriteToken(token);
            return usertoken;
        }
    }
}
