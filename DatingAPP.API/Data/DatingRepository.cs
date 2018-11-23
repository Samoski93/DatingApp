using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingAPP.API.Helpers;
using DatingAPP.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingAPP.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        // Brought in the DataContext because we will be saving changes in the database
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

        // Check to see if a user already liked another user
        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId)
                .FirstOrDefaultAsync(p => p.IsMain);
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

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();

            users = users.Where(u => u.Id != userParams.UserId);

            users = users.Where(u => u.Gender == userParams.Gender);    // return gender of user in userParams

            // Get a list of Ids a user has liked and also a list of users that has liked the currently logged in users
            if (userParams.Likers) // if set to true
            {
                var userLikers = await GetUserLikes(userParams.Id, userParams.Likers);  // userLikers is a list of ints of the currently logged in user likers
                users = users.Where(u => userLikers.Contains(u.Id));    // If userLikers matches any Id in the user table, return
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.Id, userParams.Likers);  // userLikers is a list of ints of the currently logged in user likers
                users = users.Where(u => userLikees.Contains(u.Id));
            }

            // check if there is a minimum age or max age
            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                // Age in is stored as DOb in the database, so we calculate
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);   // minimum date of birth (minus numb of years from today based on the maxAge user is looking for)
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch(userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        // Method to return list of likers and likees (Ids)
        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            // Return currently logged in user, includes the likers and likees collection
            var user = _context.User
                .Include(u => u.Likers)
                .Include(u => u.Likees)
                .FirstOrDefaultAsync(u => u.Id == id);

            // If true, return all  the users that has liked the currently logged in user
            if (likers) 
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikeeId);
            }
            else
            {
                return user.Likers.Where(u => u.LikerId == id).Select(i => l.LikeeId);
            }
        }

        public async Task<bool> SaveAll()
        {
            // If this returns more than 0, for whatever changes made to the database, return true.
            // If equals to 0, means nothing is been save to the database, return false
            return await _context.SaveChangesAsync() > 0;
        }
    }
}