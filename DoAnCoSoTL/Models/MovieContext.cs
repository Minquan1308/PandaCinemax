using DoAnCoSoTL.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSoTL.Models
{
    public class MovieContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Movie> Movies { get; set; }
                                               
        // Các DbSet khác
                                               
        // public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<Cinema> Cinemas { get; set; }
        public virtual DbSet<Actor> Actors { get; set; }
        public virtual DbSet<Producer> Producers { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<MovieActor> MovieActors { get; set; }
        public virtual DbSet<MoviesInCinema> MovieInCinemas { get; set; }
        public virtual DbSet<Cart> Cart { get; set; }
        public DbSet<Screening> Screenings { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<SeatOrder> SeatOrders { get; set; }
        public DbSet<SeatOrderDetail> SeatOrderDetails { get; set; }

        //public MovieContext() { }

        public MovieContext(DbContextOptions<MovieContext> options) : base(options)
        {
           
        }
    }
}
