using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelProject.Models
{
    public class CommentLike
    {
        [Key]
        public int LikeID { get; set; }
        public int CommentID { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("CommentID")]
        public Comment? Comment { get; set; }
    }
}