using System.ComponentModel.DataAnnotations;

namespace WebProgramlama.Data
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ProductName { get; set; }
        
        [Required]
        public int ProductOwnerID { get; set; }

        [Required]
        public int Amount { get; set; }
    }
}
