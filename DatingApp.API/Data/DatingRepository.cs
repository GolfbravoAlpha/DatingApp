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

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            //check to see if the userid and recipientId already are in the database          
            return await _context.Likes.FirstOrDefaultAsync(u => 
                u.LikerId == userId && u.LikeeId == recipientId);
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
            var users = _context.Users.Include(p => p.Photos)
                .OrderByDescending(u => u.LastActive).AsQueryable();            

            //filter out the current user
            users = users.Where(u => u.Id != userParams.UserId);

            //filter based on gender
            users = users.Where(u => u.Gender == userParams.Gender);

            //check userparams likers and Likees is true
            //is the inputed argument in the api parameter likers = true || likees == true
            if(userParams.Likers)
            {                
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));
            }

            if(userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));
            }

            //filter based on age selected
            if(userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                //converting from D.O.B to age
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge -1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <=maxDob);
            }

            //order users by the date they were created and then last active
            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                        //gives users a choice, if they don't choose order by created then it defaults to last active
                        default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

          

            return await PagedList<User>.CreateAsync(users, userParams.Pagenumber, userParams.PageSize);
        }

        //method to check the database if likee and liker exists in the database
        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            //assigns the likee and liker data to the 'user' object
            var user = await _context.Users
                .Include(x => x.Likers)
                .Include(x => x.Likees)
                .FirstOrDefaultAsync(u => u.Id == id);

            //we want to either return a list of likers or a list of likees 
            //if likers has been set to true, then return the list of people who have clicked like on the users profile 
            if(likers)
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(id => id.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}