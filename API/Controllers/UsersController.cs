using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _userRepo = userRepository;
            _mapper = mapper;
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDTO>>> GetUsers([FromQuery]UserParams userParams)
        {
            var username = User.GetUsername();
            var currentUser = await _userRepo.GetUserByUsernameAsync(username);
            userParams.CurrentUsername = currentUser.Username;

            if (userParams.Gender.IsNullOrEmpty())
            {
                userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
            }


            var users = await _userRepo.GetMembersAsync(userParams);
            Response.AddPaginationHeader(new PaginationHeader(userParams.PageNumber, userParams.PageSize, users.TotalCount, users.TotalPage));

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDTO>> GetUser(string username)
        {
            return await _userRepo.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            var username = User.GetUsername();
            var user = await _userRepo.GetUserByUsernameAsync(username);

            if(user == null) return NotFound();

            _mapper.Map(memberUpdateDTO, user);

            if (await _userRepo.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to update");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            var username = User.GetUsername();
            var user = await _userRepo.GetUserByUsernameAsync(username);

            if (user == null) return NotFound();

            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if (await _userRepo.SaveAllAsync()) 
                return CreatedAtAction(nameof(GetUser), new { username = user.Username }, _mapper.Map<PhotoDTO>(photo));

            return BadRequest("Add photo fail");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var username = User.GetUsername();
            var user = await _userRepo.GetUserByUsernameAsync(username);

            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("This photo is already main photo");

            var currentMainPhoto = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMainPhoto != null) currentMainPhoto.IsMain = false;
            photo.IsMain = true;

            if (await _userRepo.SaveAllAsync()) return NoContent();

            return BadRequest("Set main photo fail");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var username = User.GetUsername();
            var user = await _userRepo.GetUserByUsernameAsync(username);
            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You can't delete main photo");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message); 
            }

            user.Photos.Remove(photo);

            if (await _userRepo.SaveAllAsync()) return Ok();

            return BadRequest("Delete photo fail");

        }
    }
}
