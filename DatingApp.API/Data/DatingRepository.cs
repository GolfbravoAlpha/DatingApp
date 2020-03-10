using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;

        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId)
                .FirstOrDefaultAsync(p => p.IsMain);  //get the users main photo
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            return photo;
        }

        public async Task<User> GetUser(int id)
        {            
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
            
            return user;
        }
        //to return data from the database to return pagedlist
        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            // 'user' object gets user data from the database and photos and converts it to list
            var users = _context.Users.Include(p => p.Photos).AsQueryable();            

            //filter out the current user
            users = users.Where(u => u.Id != userParams.UserId);

            //filter based on gender
            users = users.Where(u => u.Gender == userParams.Gender);

            //filter based on age selected
            if(userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                //converting from D.O.B to age
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge -1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <=maxDob);
            }

            return await PagedList<User>.CreateAsync(users, userParams.Pagenumber, userParams.PageSize);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}