using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelProject.Models
{
    public class FoodBlog
    {
        [Key]
        public int BlogID { get; set; }

        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public string Content { get; set; } = "";

        // 'North' | 'Central' | 'South'
        [Required]
        public string Region { get; set; } = "";

        // 'Spring' | 'Summer' | 'Autumn' | 'Winter'
        [Required]
        public string Season { get; set; } = "";

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<FoodRestaurant> Restaurants { get; set; } = new List<FoodRestaurant>();
    }

    public class FoodRestaurant
    {
        [Key]
        public int RestaurantID { get; set; }

        public int BlogID { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string Address { get; set; } = "";

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string? PhoneNumber { get; set; }
        public string? PriceRange { get; set; }
        public string? ImageUrl { get; set; }

        [ForeignKey("BlogID")]
        public FoodBlog? Blog { get; set; }

        public ICollection<FoodReview> Reviews { get; set; } = new List<FoodReview>();
    }

    public class FoodReview
    {
        [Key]
        public int ReviewID { get; set; }

        public int RestaurantID { get; set; }
        public int UserID { get; set; }

        [Required]
        public string UserName { get; set; } = "";

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; } = "";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("RestaurantID")]
        public FoodRestaurant? Restaurant { get; set; }
    }
}