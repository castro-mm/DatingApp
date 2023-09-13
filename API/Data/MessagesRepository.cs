using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTO;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessagesRepository : IMessagesRepository
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public MessagesRepository(DataContext dataContext, IMapper mapper)
        {
            this._mapper = mapper;
            this._dataContext = dataContext;
        }

        public void AddGroup(Group group)
        {
            this._dataContext.Add(group);
        }

        public void AddMessage(Message message)
        {
            this._dataContext.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            this._dataContext.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await this._dataContext.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await this._dataContext.Groups.Include(x => x.Connections).Where(x => x.Connections.Any(c => c.ConnectionId == connectionId)).FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await this._dataContext.Messages.FindAsync(id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await this._dataContext.Groups.Include(x => x.Connections).FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = this._dataContext.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();

            // Filter options
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username && u.SenderDeleted == false),
                _ => query.Where(u => u.RecipientUsername == messageParams.Username && u.DateRead == null && u.RecipientDeleted == false)
            };

            var messages = query.ProjectTo<MessageDto>(this._mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var query = this._dataContext.Messages
                .Where(m => 
                    (m.RecipientUsername == currentUsername && m.SenderUsername == recipientUsername && m.RecipientDeleted == false) ||
                    (m.RecipientUsername == recipientUsername && m.SenderUsername == currentUsername && m.SenderDeleted == false)
                ).OrderBy(m => m.MessageSent)
                .AsQueryable();
            
            var unreadMessages = query.Where(m => m.DateRead == null && m.RecipientUsername == currentUsername).ToList();

            if (unreadMessages.Any()) 
            {
                foreach (var message in unreadMessages) {
                    message.DateRead = DateTime.UtcNow;
                }
            }

            return await query.ProjectTo<MessageDto>(this._mapper.ConfigurationProvider).ToListAsync();
        }

        public void RemoveConnection(Connection connection)
        {
            this._dataContext.Connections.Remove(connection);
        }
    }
}