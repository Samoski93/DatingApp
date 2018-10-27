using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var users = await _context.Users.Include(p => p.Photos).ToListAsync();
            return users; ;
        }

        public async Task<bool> SaveAll()
        {
            // If this returns more than 0, for whatever changes made to the database, return true.
            // If equals to 0, means nothing is been save to the database, return false
            return await _context.SaveChangesAsync() > 0;
        }
    }
}