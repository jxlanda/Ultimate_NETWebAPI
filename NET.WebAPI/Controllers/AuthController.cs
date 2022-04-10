using Contracts;
using Entities.DataTransferObjects.Authorization;
using Entities.Models;
using Entities.Models.Database;
using Entities.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NET.WebAPI.Extensions;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NET5.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRepositoryWrapper _repository;
        private readonly EncryptionService _encryption;
        private readonly JWTService _jwtService;

        public AuthController(EncryptionService encryption, JWTService jwtService,
            IRepositoryWrapper repository)
        {
            _repository = repository;
            _encryption = encryption;
            _jwtService = jwtService;
        }

        [HttpPost, Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserForAuthenticationDTO user)
        {
            // string encryptedPassword = _encryption.EncryptString(user.Password);
            IQueryable<User> query = await _repository.User.GetAllByConditionAsync(x =>
                x.Email == user.Email.ToLowerInvariant() &&
                x.Password == user.Password);

            User userFound = query.FirstOrDefault();

            if (userFound == null) return NotFound();

            SigningCredentials signingCredentials = _jwtService.GetSigningCredentials(JWTType.Token);
            List<Claim> claims = _jwtService.GetClaims(userFound);
            // JWT
            JwtSecurityToken tokenOptions = _jwtService.GenerateTokenOptions(signingCredentials, claims, JWTType.Token);
            string token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            // Refresh Token
            SigningCredentials refreshSigningCredentials = _jwtService.GetSigningCredentials(JWTType.RefreshToken);
            JwtSecurityToken refreshTokenOptions = _jwtService.GenerateTokenOptions(refreshSigningCredentials, claims, JWTType.RefreshToken);
            string refreshToken = new JwtSecurityTokenHandler().WriteToken(refreshTokenOptions);

            return Ok(new { token, refreshToken });
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IQueryable<User> query = await _repository.User.GetAllByConditionAsync(x => x.Email == forgotPasswordDTO.Email);
            User user = query.FirstOrDefault();

            if (user == null)
                return BadRequest("Invalid Request");

            SigningCredentials signingCredentials = _jwtService.GetSigningCredentials(JWTType.ResetPassword);
            List<Claim> claims = _jwtService.GetClaims(user);
            JwtSecurityToken tokenOptions = _jwtService.GenerateTokenOptions(signingCredentials, claims, JWTType.ResetPassword);
            string token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            Dictionary<string, string> param = new()
            {
                { "token", token }
            };

            string callback = QueryHelpers.AddQueryString(forgotPasswordDTO.ClientURI, param);

            // Send Email Notification
            // Update ResetToken DB

            return Ok();
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IQueryable<User> query = await _repository.User.GetAllByConditionAsync(x => x.Email == resetPasswordDTO.Email);
            User user = query.FirstOrDefault();

            if (user == null)
                return BadRequest("Invalid Request");

            bool isValidToken = _jwtService.IsTokenValid(resetPasswordDTO.Token, JWTType.ResetPassword);
            if (!isValidToken) return Unauthorized();

            // Validate Token DB
            
            return Ok();
        }

        [HttpPost("Token")]
        public async Task<IActionResult> Token([FromBody] RefreshTokenDTO tokenDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            bool isValidToken = _jwtService.IsTokenValid(tokenDTO.RefreshToken, JWTType.RefreshToken);
            if (!isValidToken) return Unauthorized();

            IQueryable<User> query = await _repository.User.GetAllByConditionAsync(x =>
                x.Email == tokenDTO.Email.ToLowerInvariant());
            User userFound = query.FirstOrDefault();

            if (userFound == null)
                return NotFound();

            // JWT
            SigningCredentials signingCredentials = _jwtService.GetSigningCredentials(JWTType.Token);
            List<Claim> claims = _jwtService.GetClaims(userFound);
            JwtSecurityToken tokenOptions = _jwtService.GenerateTokenOptions(signingCredentials, claims, JWTType.Token);
            string token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            // Refresh Token
            SigningCredentials refreshSigningCredentials = _jwtService.GetSigningCredentials(JWTType.RefreshToken);
            JwtSecurityToken refreshTokenOptions = _jwtService.GenerateTokenOptions(refreshSigningCredentials, claims, JWTType.RefreshToken);
            string refreshToken = new JwtSecurityTokenHandler().WriteToken(refreshTokenOptions);

            return Ok(new { token, refreshToken });
        }

        [HttpPost]
        [Route("RevokeToken/{refreshToken}")]
        public async Task<IActionResult> RevokeRefreshToken(string refreshToken)
        {
            if (refreshToken == null) return NotFound();
            Claim emailClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
            if (emailClaim == null) return BadRequest();

            IQueryable<User> query = await _repository.User.GetAllByConditionAsync(x =>
                x.Email == emailClaim.Value.ToLowerInvariant());
            User userFound = query.AsNoTracking().FirstOrDefault();
            if (userFound == null) return NotFound();

            // Revoke token in DB

            return Ok();
        }
    }
}
