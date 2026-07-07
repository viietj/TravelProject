using System;

namespace TravelProject.Models
{
    public class Favorite
    {
        public int FavoriteID { get; set; }
        public int UserID { get; set; }
        public int DestinationID { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public User? User { get; set; }
        public Destination? Destination { get; set; }
    }
}
