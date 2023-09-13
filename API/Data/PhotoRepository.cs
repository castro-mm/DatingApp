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
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public PhotoRepository(DataContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        public async Task<Photo> GetPhotoById(int id)
        {
            return await this._context.Photos.FindAsync(id);
        }

        public async Task<PagedList<PhotoForApprovalDto>> GetUnapprovedPhotos(PhotoParams photoParams)
        {
            var query = this._context.Photos.Include(x => x.AppUser).IgnoreQueryFilters().AsQueryable();
            
            //query = query.Where(x => !x.IsApproved).OrderByDescending(x => x.Id);

            var photos = query.ProjectTo<PhotoForApprovalDto>(this._mapper.ConfigurationProvider);

            return await PagedList<PhotoForApprovalDto>.CreateAsync(photos, photoParams.PageNumber, photoParams.PageSize);
        }

        public void RemovePhoto(Photo photo)
        {
            this._context.Photos.Remove(photo);
        }
    }
}