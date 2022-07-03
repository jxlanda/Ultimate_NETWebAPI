using Entities.Models.Database;
using Entities.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NET.WebAPI.Services
{
    public class JWTService
    {
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _jwtSettings;

        public JWTService(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtSettings = _configuration.GetSection("Jwt");
        }

        public SigningCredentials GetSigningCredentials(JWTType type)
        {
            var keySection = type switch
            {
                JWTType.Token => _jwtSettings.GetSection("Key").Value,
                JWTType.RefreshToken => _jwtSettings.GetSection("RefreshTokenKey").Value,
                JWTType.ResetPassword => _jwtSettings.GetSection("ResetPasswordKey").Value,
                _ => _jwtSettings.GetSection("ResetPasswordKey").Value
            };
            var key = Encoding.UTF8.GetBytes(keySection);
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        public List<Claim> GetClaims(User user)
        {
            List<Claim> claims = new()
            {
                new Claim("email", user.Email),
            };

            return claims;
        }

        public bool IsTokenValid(string token, JWTType type)
        {
            try
            {
                var keySection = type switch
                {
                    JWTType.Token => _jwtSettings.GetSection("Key").Value,
                    JWTType.RefreshToken => _jwtSettings.GetSection("RefreshTokenKey").Value,
                    JWTType.ResetPassword => _jwtSettings.GetSection("ResetPasswordKey").Value,
                    _ => _jwtSettings.GetSection("ResetPasswordKey").Value
                };

                JwtSecurityTokenHandler tokenHandler = new();
                var key = Encoding.UTF8.GetBytes(keySection);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims, JWTType type)
        {

            var expire = type switch
            {
                JWTType.Token => DateTime.Now.AddMinutes(Convert.ToDouble(_jwtSettings.GetSection("ExpiryInMinutes").Value)),
                JWTType.RefreshToken => DateTime.Now.AddMinutes(Convert.ToDouble(_jwtSettings.GetSection("RefreshTokenExpiryInMinutes").Value)),
                JWTType.ResetPassword => DateTime.Now.AddMinutes(Convert.ToDouble(_jwtSettings.GetSection("ResetPasswordExpiryInMinutes").Value)),
                _ => DateTime.Now.AddMinutes(Convert.ToDouble(_jwtSettings.GetSection("ExpiryInMinutes").Value)),
            };

            var tokenOptions = new JwtSecurityToken(
                issuer: _jwtSettings.GetSection("Issuer").Value,
                audience: _jwtSettings.GetSection("Audience").Value,
                claims: claims,
                expires: expire,
                signingCredentials: signingCredentials);


            return tokenOptions;
        }
    }
}
