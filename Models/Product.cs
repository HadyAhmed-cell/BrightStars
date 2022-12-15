using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrightStars.Models
{
    public class Product
    {
        public int Id { get; set; }
        [MinLength(2, ErrorMessage = "name must not be less than 2 chars")]
        [MaxLength(20, ErrorMessage = "name must not exceed 20 chars")]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime ProductionDate { get; set; }
        [ValidateNever]
        public DateTime CreatedAt { get; set; }
        [ValidateNever]
        public DateTime LastUpdatedAt { get; set; }
        [ValidateNever]
        public string ImageUrl { get; set; }
        [ValidateNever]
        [NotMapped]
        public IFormFile ImageFile { get; set; }

        public int CategoryId { get; set; }
        [ValidateNever]
        public Category Category { get; set; }

    }
}
