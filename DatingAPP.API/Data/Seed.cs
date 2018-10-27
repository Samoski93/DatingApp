using System.Collections.Generic;
using DatingAPP.API.Models;
using Newtonsoft.Json;

namespace DatingAPP.API.Data
{
    public class Seed
    {
        private readonly DataContext _context;
        public Seed(DataContext context)
        {
            _context = context;
        }

        public void SeedUsers()
        {
            var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
            // Serialize into objects so as to loop through objects and save into database
            var users = JsonConvert.DeserializeObject<List<User>>(userData);   // Converts from text to object (list of user objects)
            foreach (var user in users)
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash("password", out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.Username = user.Username.ToLower();

                _context.Users.Add(user);
            }

            _context.SaveChanges();
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())      // HMACSHA512() generates a key, Use key to unlock hashed password
            {
                passwordSalt = hmac.Key;    // randomly generates key
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));  // gets password as byte[], computes hash
            }
        }
    }
}