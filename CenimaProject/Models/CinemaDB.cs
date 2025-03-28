using Microsoft.EntityFrameworkCore;
using CinemaProject.Models;

namespace CinemaProject.Models
{
    public class CinemaDB : DbContext
    {
        public CinemaDB(DbContextOptions<CinemaDB> options) : base(options) 
        {

        }
        public DbSet<UsersDB> Users { get; set; }
      
        public DbSet<MoviesDB> Movies { get; set; }

    }
}
