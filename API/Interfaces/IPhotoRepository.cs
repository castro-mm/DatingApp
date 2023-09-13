using System.Threading.Tasks;
using API.DTO;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        Task<PagedList<PhotoForApprovalDto>> GetUnapprovedPhotos(PhotoParams photoParams);
        Task<Photo> GetPhotoById(int id);
        void RemovePhoto(Photo photo);
    }
}