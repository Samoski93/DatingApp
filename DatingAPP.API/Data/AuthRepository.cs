using System;
using System.Threading.Tasks;
using DatingAPP.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingAPP.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;

        public AuthRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User> Login(string username, string password)
        {
            // Use the username to identify the user in our db and store in a var.
            // Compare the password not with the string of password but the hashed password,
            // so we compute the hash this password generates & then compare with this password hash stored in our database
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
            if (user != null)
                return null;

            if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }

        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))    // Use key to unlock hashed password
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));  // computes hash from password but uses the key passed in to the hmac var
                for (int i = 0; i < computedHash.Length; i++)
                    if (computedHash[i] != passwordHash[i])
                        return false;
            }
            return true;
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            // With the out keyword, the parameters in this method is referencing the vars, so when they are updated in the method,
            // They will be updated in the vars as well.
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())      // HMACSHA512() generates a key, Use key to unlock hashed password
            {
                passwordSalt = hmac.Key;    // randomly generates key
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));  // gets password as byte[], computes hash
            }
        }

        public async Task<bool> UserExists(string username)
        {
            if (await _context.Users.AnyAsync(x => x.Username == username))
                return true;

            return false;
        }
    }
}