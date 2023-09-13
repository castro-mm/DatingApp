using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnityOfWork _unityOfWork;

        public AdminController(UserManager<AppUser> userManager, IUnityOfWork unityOfWork)
        {
            this._userManager = userManager;
            this._unityOfWork = unityOfWork;
        }

        [Authorize(Policy = "RequiredAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await this._userManager.Users
                .OrderBy(u => u.UserName)
                .Select(u => new 
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()    
                }).ToListAsync();

            return Ok(users);

        }

        [Authorize(Policy = "RequiredAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var user = await this._userManager.FindByNameAsync(username);
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");
            var selectedRoles = roles.Split(",").ToArray();

            var userRoles = await this._userManager.GetRolesAsync(user);

            var result = await this._userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded) return BadRequest("Failed to add to roles");

            result = await this._userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(await this._userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratedPhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult<PagedList<PhotoForApprovalDto>>> GetPhotosForModeration([FromQuery] PhotoParams photoParams)
        {
            var photos = await this._unityOfWork.PhotoRepository.GetUnapprovedPhotos(photoParams);
            
            Response.AddPaginationHeader(new PaginationHeader(photos.CurrentPage, photos.PageSize, photos.TotalCount, photos.TotalPages));

            return Ok(photos);
        }  

        [Authorize(Policy = "ModeratedPhotoRole")]      
        [HttpPut("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var photo = await this._unityOfWork.PhotoRepository.GetPhotoById(photoId);

            if(photo == null) return NotFound("This photo doesn't exists");

            photo.IsApproved = true;

            var user = await this._unityOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            if (!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;

            if (await this._unityOfWork.Complete()) return Ok();

            return BadRequest("Failure to approve the photo");
        }

        [Authorize(Policy = "ModeratedPhotoRole")]      
        [HttpDelete("{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId) 
        {
            var photo = await this._unityOfWork.PhotoRepository.GetPhotoById(photoId);

            if (photo == null) return NotFound("This photo doesn't exists");

            this._unityOfWork.PhotoRepository.RemovePhoto(photo);

            if (await this._unityOfWork.Complete()) return Ok();

            return BadRequest("Failure to delete the photo");
        }
    }
}