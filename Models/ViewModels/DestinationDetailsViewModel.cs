using System.Collections.Generic;

namespace TravelProject.Models.ViewModels
{
    public class DestinationDetailsViewModel
    {
        public Destination Destination { get; set; } = null!;
        public List<DestinationComment> Comments { get; set; } = new List<DestinationComment>();
        public string? UserName { get; set; }
        public string? Content { get; set; }
        public int Rating { get; set; } = 5;
        public double AverageRating { get; set; }
        public int TotalComments { get; set; }
    }
}
