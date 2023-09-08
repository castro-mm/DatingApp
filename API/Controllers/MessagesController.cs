using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUnityOfWork _unityOfWork;
        private readonly IMapper _mapper;

        public MessagesController(IUnityOfWork unityOfWork, IMapper mapper)
        {
            this._unityOfWork = unityOfWork;
            this._mapper = mapper;
            
        }   

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto) 
        {
            var username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("You cannot send messages to yourself");

            var sender = await this._unityOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await this._unityOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound();

            var message = new Message {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = createMessageDto.RecipientUsername,
                Content = createMessageDto.Content
            };

            this._unityOfWork.MessagesRepository.AddMessage(message);

            if (await this._unityOfWork.Complete()) return Ok(this._mapper.Map<MessageDto>(message));

            return BadRequest("Something went wrong during add a message");


        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessageForUser ([FromQuery] MessageParams messageParams) 
        {
            messageParams.Username = User.GetUsername();

            var messages = await this._unityOfWork.MessagesRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));

            return Ok(messages);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id) 
        {
            var username = User.GetUsername();

            var message = await this._unityOfWork.MessagesRepository.GetMessage(id);
            
            if (message == null) return NotFound();
            
            if(message.SenderUsername != username && message.RecipientUsername != username) return Unauthorized();

            message.SenderDeleted = message.SenderUsername == username;
            message.RecipientDeleted = message.RecipientUsername == username;

            if(await this._unityOfWork.Complete()) return Ok();

            return BadRequest("Problems deleting the message");

        }
    }
}