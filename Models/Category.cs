using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;

namespace BrightStars.Models
{
    public class Category
    {

        public int Id { get; set; }
        public string Name { get; set; }
        [ValidateNever]
        [JsonIgnore]
        public ICollection<Product> Products { get; set; }
    }
}
