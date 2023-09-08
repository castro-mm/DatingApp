using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTO;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IUnityOfWork _unityOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        
        public MessageHub(IUnityOfWork unityOfWork, IMapper mapper, IHubContext<PresenceHub> presenceHub)
        {
            this._unityOfWork = unityOfWork;
            this._mapper = mapper;
            this._presenceHub = presenceHub;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var ohterUser = httpContext.Request.Query["user"];
            var groupName = this.GetGroupName(Context.User.GetUsername(), ohterUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await this.AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await this._unityOfWork.MessagesRepository.GetMessageThread(Context.User.GetUsername(), ohterUser);

            if (this._unityOfWork.HasChanges()) await this._unityOfWork.Complete();

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await this.RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup");

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower()) throw new HubException("You cannot send messages to yourself");

            var sender = await this._unityOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await this._unityOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) throw new HubException("Not found user");

            var message = new Message {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = createMessageDto.RecipientUsername,
                Content = createMessageDto.Content
            };

            var groupName = this.GetGroupName(sender.UserName, recipient.UserName);
            var group = await this._unityOfWork.MessagesRepository.GetMessageGroup(groupName);
            
            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else 
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await this._presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new{username = sender.UserName, knownAs = sender.KnownAs});
                }
                
            }

            this._unityOfWork.MessagesRepository.AddMessage(message);

            if (await this._unityOfWork.Complete()) 
            {
                await Clients.Group(groupName).SendAsync("NewMessage", this._mapper.Map<MessageDto>(message));
            }  
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller,other) < 0;

            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";

        }

        private async Task<Group> AddToGroup(string groupName) 
        {
            var group = await this._unityOfWork.MessagesRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                this._unityOfWork.MessagesRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if(await this._unityOfWork.Complete()) return group;

            throw new HubException("Failed to add to group");
        }

        private async Task<Group> RemoveFromMessageGroup() 
        {
            var group = await this._unityOfWork.MessagesRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            this._unityOfWork.MessagesRepository.RemoveConnection(connection);

            if (await this._unityOfWork.Complete()) return group;

            throw new HubException("Failed to remove from group");
        }
    }
}