using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HomestayBookingAPI.Models
{
    public class Voucher
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [Required]
        [StringLength(10)]
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
