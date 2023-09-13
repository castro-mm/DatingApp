using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IUnityOfWork
    {
        IUserRepository UserRepository { get; }
        IMessagesRepository MessagesRepository { get; }
        ILikesRepository LikesRepository { get; }
        IPhotoRepository PhotoRepository { get; }

        Task<bool> Complete();
        bool HasChanges();
    }
}