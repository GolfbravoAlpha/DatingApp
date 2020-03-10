using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data

{
    public class DataContext : DbContext
    {
       public DataContext(DbContextOptions<DataContext> options): base(options){}   
       
       public DbSet<Value> Values {get; set;}
       public DbSet<User> Users { get; set; }
       public DbSet<Photo> Photos { get; set; }
       public DbSet<Like> Likes { get; set; }
       //need to override the DbContext method to allow likes to become a link table
       protected override void OnModelCreating(ModelBuilder builder)
       {
           // form the primary key, remember link tables use two primary to create a single primary
           builder.Entity<Like>()           
            .HasKey(k => new {k.LikerId, k.LikeeId});    

            //tell entitframework about the relationship itself
            //one likee has a one to many relationship with a liker
            builder.Entity<Like>()
                .HasOne(u => u.Likee)
                .WithMany(u => u.Likers)
                .HasForeignKey(u => u.LikeeId)
                .OnDelete(DeleteBehavior.Restrict);

            //one liker has a one to many relationship with a likee
            builder.Entity<Like>()
                .HasOne(u => u.Liker)
                .WithMany(u => u.Likees)
                .HasForeignKey(u => u.LikerId)
                .OnDelete(DeleteBehavior.Restrict);
       }

    }
}