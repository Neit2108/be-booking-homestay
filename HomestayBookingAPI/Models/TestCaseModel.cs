using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomestayBookingAPI.Models
{
    public class TestCaseModel
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "text")]
        public string TestId { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string Target { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string ImplementationSteps { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string Input { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string ExpectedOutput { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string Status { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
