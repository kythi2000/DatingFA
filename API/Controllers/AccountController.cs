using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _dataContext;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext dataContext, ITokenService tokenService)
        {
            _dataContext= dataContext;
            _tokenService= tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if(await UserExist(registerDTO.Username)) return BadRequest("Username is existed");

            using var hmac = new HMACSHA512();
            AppUser user = new()
            {
                Username= registerDTO.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = hmac.Key
            };

            _dataContext.Add(user);
            await _dataContext.SaveChangesAsync();

            return Ok(new UserDTO
            {
                Username = user.Username,
                Token = _tokenService.CreateToken(user)
            });
        }

        private async Task<bool> UserExist(string username)
        {
            return await _dataContext.Users.AnyAsync(x => x.Username == username.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<AppUser>> Login(LoginDTO loginDTO)
        {
            if (loginDTO == null) return BadRequest();
            var user = await _dataContext.Users.SingleOrDefaultAsync(x => x.Username == loginDTO.Username.ToLower());
            if (user == null) return Unauthorized("User isn't exist");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var hashPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

            for(int i = 0; i < hashPassword.Length; i++)
            {
                if (hashPassword[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            return Ok(new UserDTO
            {
                Username = user.Username,
                Token = _tokenService.CreateToken(user)
            }); 
        }

    }
}
