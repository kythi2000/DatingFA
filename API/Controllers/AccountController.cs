using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
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
        private readonly IMapper _mapper;
        public AccountController(DataContext dataContext, ITokenService tokenService, IMapper mapper)
        {
            _dataContext= dataContext;
            _tokenService= tokenService;
            _mapper= mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if(await UserExist(registerDTO.Username)) return BadRequest("Username is existed");

            var user = _mapper.Map<AppUser>(registerDTO);

            using var hmac = new HMACSHA512();

            user.Username= registerDTO.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
            user.PasswordSalt = hmac.Key;


            _dataContext.Add(user);
            await _dataContext.SaveChangesAsync();

            return Ok(new UserDTO
            {
                Username = user.Username,
                Token = _tokenService.CreateToken(user),
                KnownAs= user.KnownAs,
                Gender=user.Gender
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
            var user = await _dataContext.Users
                .Include(x => x.Photos)
                .SingleOrDefaultAsync(x => x.Username == loginDTO.Username.ToLower());
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
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs= user.KnownAs,
                Gender=user.Gender
            }); 
        }

    }
}
