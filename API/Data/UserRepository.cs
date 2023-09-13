using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        public UserRepository(DataContext context)
        {
            this._context = context;           
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            //return await this._context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
            return await this._context.Users.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await this._context.Users
                .IgnoreQueryFilters()
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<string> GetUserGender(string username)
        {
            return await this._context.Users.Where(x => x.UserName == username).Select(s => s.Gender).FirstOrDefaultAsync();
        }

        public async Task<PagedList<AppUser>> GetUsersAsync(UserParams userParams)
        {
            var query = this._context.Users.Include(p => p.Photos).AsQueryable();
            
            // Filtering
            query = query
                .Where(u => u.UserName != userParams.CurrentUsername)
                .Where(u => u.Gender == userParams.Gender);

            // var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge -1));
            // var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));
            
            // query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };

            return await PagedList<AppUser>.CreateAsync(query, userParams.PageNumber, userParams.PageSize);
        }

        public void Update(AppUser user)
        {
            this._context.Entry(user).State = EntityState.Modified;
        }
    }
}