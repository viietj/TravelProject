using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TravelProject.Models
{
    public class TravelDbContext : DbContext
    {
        public TravelDbContext(DbContextOptions<TravelDbContext> options) 
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<TourViewModel> TourOverview { get; set; }

         public DbSet<TourByDestination> TourByDestination { get; set; }
         public DbSet<Hotel> Hotels { get; set; }
         public DbSet<HotelBooking> HotelBookings { get; set; }
         public DbSet<HotelByDestination> HotelsByDestination { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TourViewModel>()
                .HasNoKey()
                .ToView("vw_ToursOverview");
            modelBuilder.Entity<TourByDestination>()
                .HasNoKey()
                .ToView("vw_ToursByDestination");

            modelBuilder.Entity<HotelByDestination>()
                .HasNoKey()
                .ToView("vw_HotelsByDestination");
            
        }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Tour> Tours { get; set; } 
        public DbSet<Comment> Comments { get; set; }

        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<CustomTourRequest> CustomTourRequests { get; set; }
        public DbSet<FoodBlog> FoodBlogs  { get; set; }
        public DbSet<FoodRestaurant> FoodRestaurants { get; set; }
        public DbSet<FoodReview> FoodReviews  { get; set; }
    }

}