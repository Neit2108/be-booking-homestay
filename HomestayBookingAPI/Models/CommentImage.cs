using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HomestayBookingAPI.Models
{
    public class CommentImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; }

        public int CommentId { get; set; }

        [ForeignKey("CommentId")]
        public virtual Comment Comment { get; set; }
    }
}
