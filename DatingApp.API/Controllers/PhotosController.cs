using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo; //to connect to repository for filtering data 
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper,
        IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _repo = repo; 

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc); //to connect to cloudinary
        }

        [HttpGet("{id}", Name = "getPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id); //get photo from repository which will then get it from the database on your behalf

            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, 
            [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(userId);

            var file = photoForCreationDto.File;

            var uploadResult = new ImageUploadResult();

            if(file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation()
                            .Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                     
                }
            }

            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDto);

            if(!userFromRepo.Photos.Any(userFromRepo => userFromRepo.IsMain))
                photo.IsMain = true;
            
            userFromRepo.Photos.Add(photo);   

            if(await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new {userId = userId, id = photo.Id },
                photoToReturn);
            };

            return BadRequest("Could not add the photo");
        }

        [HttpPost("{id}/setMain")]  //to set main photo
        public async Task<IActionResult> SetMainPhoto(int userId, int id)  //takes in user id and the photo id to check 
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) //checks user is authorised
                return Unauthorized();  

            var user = await _repo.GetUser(userId);  //get user data 

            if(!user.Photos.Any(p => p.Id == id)) //does the user have this photo already uploaded
                return Unauthorized(); //if not then return unauthorised 
            
            var photoFromRepo = await _repo.GetPhoto(id); //assign the photo from the gallary to the photoFromRepo object 

            if(photoFromRepo.IsMain) //is this photo already main 
                return BadRequest("this is already the main photo");

            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId); //get the current Main photo from the repository (database)
            currentMainPhoto.IsMain = false; //removes current photo from main 

            photoFromRepo.IsMain = true;  //adds the new photo as the main photo, this is why the photoFromrepo has been set as main

            if(await _repo.SaveAll()) //save changes 
                return NoContent(); //no context is being returned 

            return BadRequest("Could not set photo to main"); //if the above fails 
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) //checks user is authorised
                return Unauthorized();  

            var user = await _repo.GetUser(userId);  //get user data 

            if(!user.Photos.Any(p => p.Id == id)) //does the user have this photo already uploaded
                return Unauthorized(); //if not then return unauthorised 
            
            var photoFromRepo = await _repo.GetPhoto(id); //assign the photo from the gallary to the photoFromRepo object 

            if(photoFromRepo.IsMain) //is this photo already main 
                return BadRequest("You cannot delete your main photo");

            if(photoFromRepo.PublicId != null) // is it in cloudinary 
            {
                // cloudinary deletionParams method
                //photoFromRepo is used to get the public id, which is the id of cloudinary
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);

                var result = _cloudinary.Destroy(deleteParams); //cloudinary method stored in result variable 

                if(result.Result == "ok") //testing out result against the cloudinary result
                {
                    _repo.Delete(photoFromRepo); //remove photo information from our own database
                }
            }
            if(photoFromRepo.PublicId == null) //is it not in cloudinary 
            {
                _repo.Delete(photoFromRepo); //remove photo information from our own database
            }         
    
            if(await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to delete the photo");
        }
    }
}