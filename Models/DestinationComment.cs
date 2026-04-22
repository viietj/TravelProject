using System;

namespace TravelProject.Models
{
    public class DestinationComment
    {
        public int DestinationCommentID { get; set; }
        public int DestinationID { get; set; }
        public string UserName { get; set; } = null!;
        public int Rating { get; set; }
        public string Content { get; set; } = null!;
        public bool IsApproved { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public Destination? Destination { get; set; }
    }
}
