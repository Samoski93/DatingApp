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

        // Using UserParams to pass in pagination information
        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();

            users = users.Where(u => u.Id != userParams.UserId);

            users = users.Where(u => u.Gender == userParams.Gender);    // return gender of user in userParams

            // Get a list of Ids of users a user has liked and also a list of Ids of users that has liked the currently logged in user
            if (userParams.Likers) // if set to true
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);  // userLikers is a list of ints of the currently logged in user likers
                users = users.Where(u => userLikers.Contains(u.Id));    // If userLikers matches any Id in the user table, return
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);  // userLikers is a list of ints of the currently logged in user likers
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
                switch (userParams.OrderBy)
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
            var user = await _context.Users
                .Include(u => u.Likers)
                .Include(u => u.Likees)
                .FirstOrDefaultAsync(u => u.Id == id);

            // If true, return all  the users that has liked the currently logged in user
            if (likers)
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }
        }

        public async Task<bool> SaveAll()
        {
            // If this returns more than 0, for whatever changes made to the database, return true.
            // If equals to 0, means nothing is been save to the database, return false
            return await _context.SaveChangesAsync() > 0;
        }

        // Get a single message from the database
        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        // Get messages for a user
        // Inbox, Outbox and Read/Unread messages
        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            // Get messages and include sender info
        var messages = _context.Messages
                .Include(u => u.Sender).ThenInclude(u => u.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .AsQueryable();

            // Filter out messages we don't want to return
            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(u => u.SenderId == messageParams.UserId && u.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted == false && u.IsRead == false);
                    break;
            }

            // Order the messages - The most recent messages first
            messages = messages.OrderByDescending(d => d.MessageSent);
            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        // Conversation between 2 users
        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
               .Include(u => u.Sender).ThenInclude(u => u.Photos)
               .Include(u => u.Recipient).ThenInclude(p => p.Photos)
               // The recipientId matches the userId and the senderId matches the recipientId - Returns the conversation between 2 users
               .Where(m => m.RecipientId == userId && m.RecipientDeleted == false && m.SenderId == recipientId || m.RecipientId == recipientId && m.SenderId == userId
                    && m.SenderDeleted == false)
               .OrderByDescending(m => m.MessageSent)
               .ToListAsync();

            return messages;
        }
    }
}