using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomestayBookingAPI.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }
        public string? SenderId { get; set; }
        [Required]
        public string SenderName { get; set; }
        [Required]
        public string SenderEmail { get; set; }
        [Required]
        public string SenderPhone { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]        
        public string Message { get; set; }
        [ForeignKey("SenderId")]
        public virtual ApplicationUser? Sender { get; set; }
    }
}
