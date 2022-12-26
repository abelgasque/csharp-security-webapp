﻿using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SecurityApp.Web.Infrastructure.Entities.DTO;
using SecurityApp.Web.Infrastructure.Entities.Exceptions;
using SecurityApp.Web.Infrastructure.Entities.Models;
using SecurityApp.Web.Infrastructure.Entities.Settings;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

namespace SecurityApp.Web.Infrastructure.Services
{
    public class TokenService
    {
        private readonly ApplicationSettings _settings;

        private readonly CustomerService _service;

        public TokenService(
            IOptions<ApplicationSettings> settings,
            CustomerService service
        )
        {
            _settings = settings.Value;
            _service = service;
        }

        private TokenDTO GenerateToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_settings.Secret);
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(_settings.ExpireIn),
                NotBefore = DateTime.Now,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new TokenDTO()
            {
                AccessToken = tokenHandler.WriteToken(token),
                ExpiresIn = _settings.ExpireIn
            };
        }

        public async Task<TokenDTO> Login(UserDTO pEntity)
        {
            CustomerModel customer = await _service.ReadByMail(pEntity.Username);
            string msgError = string.Empty;
            if ((!customer.Active) || (customer.Block))
            {
                msgError = "Usuário bloqueado ou inátivo!";
            }

            if (customer.AuthAttempts >= _settings.AuthAttempts)
            {
                msgError = "Usuário bloqueado temporariamente!";
                customer.Block = true;
            }

            if (!customer.Password.Equals(pEntity.Password))
            {
                msgError = "Senha inválida!";                
            }

            bool hasError = (!string.IsNullOrEmpty(msgError));
            customer.AuthAttempts = (hasError) ? (customer.AuthAttempts += 1) : 0;
            await _service.UpdateAsync(customer);            

            if (hasError)
            {
                 throw new UnauthorizedException(msgError) { };
            }
            
            var token = GenerateToken();
            token.Customer = new {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Mail = customer.Mail,
            };
            return token;
        }
    }
}
