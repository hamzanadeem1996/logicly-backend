using Apex.DataAccess.Models;
using Apex.DataAccess.Repositories;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Apex.DataAccess
{
    public interface IUserService
    {
        AuthToken Authenticate(string email, string password);

        IEnumerable<User> GetAll();
    }

    public class UserService : IUserService
    {
        private List<User> _users = new List<User>();
        private string Secretkey = "12b6fb24-adb8-4ce5-aa49-79b265ebf256";

        public AuthToken Authenticate(string email, string password)
        {
            AuthToken authToken = new AuthToken();
            var user = new UserRepo().Login(email, password);
            if (user == null) return null;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Secretkey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(1095),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            authToken.Id = user.Id;
            authToken.RoleId = user.RoleId;
            authToken.FirstName = user.FirstName;
            authToken.LastName = user.LastName;
            authToken.RoleName = user.RoleName;
            authToken.Email = user.Email;
            authToken.Password = user.Password;
            authToken.Lat = user.Lat;
            authToken.Long = user.Long;
            authToken.RcUserId = user.RcUserId;
            authToken.RcUserName = user.RcUserName;
            authToken.RcPassword = user.RcPassword;
            if (string.IsNullOrEmpty(user.Token))
            {
                user.Token = tokenHandler.WriteToken(token);
                new UserRepo().Save(user);
                authToken.Token = user.Token;
            }
            else
            {
                authToken.Token = user.Token;
            }
            return authToken;
        }

        public IEnumerable<User> GetAll()
        {
            return _users;
        }
    }
}