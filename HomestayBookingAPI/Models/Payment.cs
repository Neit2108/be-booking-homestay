using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomestayBookingAPI.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public double Amount { get; set; }

        public string PaymentMethod { get; set; } 

        public string Status { get; set; } 

        public string? TransactionId { get; set; }
        
        [DataType(DataType.Date)]

        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } 
    }
}
