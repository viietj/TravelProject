using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelProject.Models
{
    public class Comment
    {
        [Key]
        public int CommentID { get; set; }

        public int DestinationID { get; set; }

        public int? ParentCommentID { get; set; }

        public string UserName { get; set; } = null!;

        public string Content { get; set; } = null!;

        public int? Rating { get; set; }

        public string? ImageUrl { get; set; }

        public int Likes { get; set; }

        public bool IsAdmin { get; set; }

        public int? UserID { get; set; }

        public DateTime CreatedDate { get; set; }

        [ForeignKey("DestinationID")]
        public Destination? Destination { get; set; }
    }
}