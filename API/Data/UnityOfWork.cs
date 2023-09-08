using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Interfaces;
using AutoMapper;

namespace API.Data
{
    public class UnityOfWork : IUnityOfWork
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UnityOfWork(DataContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        public IUserRepository UserRepository => new UserRepository(this._context);

        public IMessagesRepository MessagesRepository => new MessagesRepository(this._context, this._mapper);

        public ILikesRepository LikesRepository => new LikesRepository(this._context);

        public async Task<bool> Complete()
        {
            return await this._context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return this._context.ChangeTracker.HasChanges();
        }
    }
}