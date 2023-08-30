using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Extensions;
using API.Entities;
using API.DTO;
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        public ILikesRepository _likesRepository { get; }
        
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            this._likesRepository = likesRepository;
            this._userRepository = userRepository;
            
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();

            var likedUser = await this._userRepository.GetUserByUsernameAsync(username);
            if (likedUser == null) return NotFound();

            var sourceUser = await this._likesRepository.GetUserWithLikes(sourceUserId);
            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

            var userLike = await this._likesRepository.GetUserLike(sourceUserId, likedUser.Id);
            if (userLike != null) return BadRequest("You already like this user");

            userLike = new UserLike {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await this._userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery] LikeParams likeParams) 
        {
            likeParams.UserId = User.GetUserId();

            var users = await this._likesRepository.GetUserLikes(likeParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }

    }
}