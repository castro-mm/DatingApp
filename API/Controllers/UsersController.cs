using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUnityOfWork _unityOfWork;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public UsersController(IUnityOfWork unityOfWork, IMapper mapper, IPhotoService photoService) 
    {
        this._photoService = photoService;
        this._unityOfWork = unityOfWork;
        this._mapper = mapper;
    }
 
    [HttpGet]
    public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        // Filtering
        userParams.CurrentUsername = User.GetUsername();
        var gender = await this._unityOfWork.UserRepository.GetUserGender(User.GetUsername());
        if (String.IsNullOrEmpty(userParams.Gender))
        {
            userParams.Gender = gender == "male" ? "female" : "male";
        }

        var users = await this._unityOfWork.UserRepository.GetUsersAsync(userParams);        

        var usersToReturn = this._mapper.Map<IEnumerable<MemberDto>>(users);

        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

        return Ok(usersToReturn);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await this._unityOfWork.UserRepository.GetUserByUsernameAsync(username);

        var userToReturn = this._mapper.Map<MemberDto>(user);

        return Ok(userToReturn);
    }
    
    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto) 
    {
        var user = await this._unityOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        this._mapper.Map(memberUpdateDto, user); // updates specific properties into the user object from memberUpdateDto attributes

        if (await this._unityOfWork.Complete()) return NoContent();

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file) 
    {
        var user = await this._unityOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if(user == null) return NotFound();

        var result = await this._photoService.AddPhotoAsync(file);
        if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
            IsApproved = false
        };

        //photo.IsMain = user.Photos.Count == 0;

        user.Photos.Add(photo);

        if(await this._unityOfWork.Complete()) 
        {
            return CreatedAtAction(nameof(GetUser), new {username = User.GetUsername()}, this._mapper.Map<PhotoDto>(photo));
        }

        return BadRequest("Failed to upload photo");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await this._unityOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return NotFound();

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if(photo == null) return NotFound();

        if(photo.IsMain) return BadRequest("This is already your main photo");

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;

        if (await this._unityOfWork.Complete()) return NoContent();

        return BadRequest("Problem setting the main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId) 
    {
        var photo = await this._unityOfWork.PhotoRepository.GetPhotoById(photoId);

        if (photo == null) return NotFound();

        if (photo.IsMain) return BadRequest("You cannot delete your main photo");

        if (photo.PublicId != null) 
        {
            var result = await this._photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);            
        }

        this._unityOfWork.PhotoRepository.RemovePhoto(photo);       
        
        if (await this._unityOfWork.Complete()) return Ok();

        return BadRequest("Problem deleting photo");
    }
}
