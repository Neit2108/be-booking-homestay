using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomestayBookingAPI.Models
{
    public class Voucher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string Name { get; set; }

        [Required]
        [StringLength(30)]
        public string Code { get; set; }

        [Required]
        public int UsageCount { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int MaxUsage { get; set; } = 0;

        [Required]
        [Range(0, 100)]
        public double Discount { get; set; } // % 

        [Required]
        public DateTime From { get; set; }
        
        [Required]
        public DateTime To { get; set; }

    }
}
