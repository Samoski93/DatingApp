using DatingAPP.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatingAPP.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {

        }

        public DbSet<Value> Values { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Make both the LikerId and LikeeId the primary key, so a user can't another more than once
            builder.Entity<Like>().
                HasKey(k => new {k.LikerId, k.LikeeId});

            // Tell EntityFramework about the relationship
            builder.Entity<Like>()
                .HasOne(u => u.Likee)
                .WithMany(u => u.Likers)
                .HasForeignKey(u => u.LikeeId)      // goes back to the user
                .OnDelete(DeleteBehavior.Restrict); // Deleting user a like should not delete a user

            builder.Entity<Like>()
                .HasOne(u => u.Liker)
                .WithMany(u => u.Likees)
                .HasForeignKey(u => u.LikeeId)      // goes back to the user
                .OnDelete(DeleteBehavior.Restrict);
        } 
    }
}