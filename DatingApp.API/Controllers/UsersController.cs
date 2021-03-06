using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))] //to log when user last logged in
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
        //this api uses pagination
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            //filtering the api
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var userFromRepo = await _repo.GetUser(currentUserId);

            userParams.UserId = currentUserId;

            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            //get users based on the pagination. so only get users who would be on page 2, therefore users from 6 to 10 only
            //this returns back items, count, pageNumner and pageSize from pagedList.cs on the createSync() used by DatingRepository
            var users = await _repo.GetUsers(userParams);

            //now map the data to the dto to filter it not to show the users hashed passwords
            var userToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);
  
            //method from extensions used 
            //the extension allows the pagination information to be added to the http response header to the client
            Response.AddPagination(users.CurrentPage, 
                users.PageSize, users.TotalCount, users.TotalPages);

            //it returns the filtered users but also the pagination information in the header, as shown in the above code.
            return Ok(userToReturn);
        }

        [HttpGet("{id}", Name="GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);

            var userToReturn = _mapper.Map<UserForDetailedDto>(user);

            return Ok(userToReturn);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UsersForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(id);

            _mapper.Map(userForUpdateDto, userFromRepo);

            if(await _repo.SaveAll())
                return NoContent();

            throw new System.Exception($"Updating user {id} failed on save");
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            //check that the user is authorised
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            //does this like already exist in the database 
            var like = await _repo.GetLike(id, recipientId);

            //if the like already exists in the database
            if(like != null)
                return BadRequest("you already like this user");
            
            //does the recipient id exist in the database
            if(await _repo.GetUser(recipientId) == null)
                return NotFound();
            
            //create a new instance of the like
            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };

            //add the new likee and liker to the database
            _repo.Add<Like>(like);

            //save the database
            if(await _repo.SaveAll())
                return Ok();
            
            //if all else fails 
            return BadRequest("Failed to like user");
        }
    }
}