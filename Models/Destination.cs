using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravelProject.Models
{
    public class Destination
    {
        
        public int DestinationID { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Address { get; set; } = null!;
        public string Region { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int ViewCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public ICollection<DestinationComment>? DestinationComments { get; set; }
    }
}
