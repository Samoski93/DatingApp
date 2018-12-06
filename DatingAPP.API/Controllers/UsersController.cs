using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Dtos;
using DatingAPP.API.Data;
using DatingAPP.API.Dtos;
using DatingAPP.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DatingAPP.API.Models;

namespace DatingAPP.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            // Get access to the id of the currently logged in from the repo (through the token)
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _repo.GetUser(currentUserId);

            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender)) {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await _repo.GetUsers(userParams);       // var users holds a PageList of users
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            // Because we inside apicontroller, response is accessible
            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);
            var userToReturn = _mapper.Map<UserForDetailedDto>(user);    // Executes a mapping from source object to destination obj

            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(id);

            _mapper.Map(userForUpdateDto, userFromRepo);

            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception($"Updating user {id} failed on save");
        }

        [HttpPost("{id}/like/{recipientId}/")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            // Make sure user is authorized before they can like another user
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var like = await _repo.GetLike(id, recipientId);

            if (like != null)   // If like is not null, means there's already a like for the user
                return BadRequest("You already like this user");
            
            // Check if recipient of like exists
            if (await _repo.GetUser(recipientId) == null)
                return NotFound();
            
            // Create  a new like
            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };

            _repo.Add<Like>(like);

            if (await _repo.SaveAll())
                return Ok();

            // if we don't sae successfully
            return BadRequest("Failed to lke user");
        }
    }
}