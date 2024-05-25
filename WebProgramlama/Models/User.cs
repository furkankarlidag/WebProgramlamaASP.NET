using System.ComponentModel.DataAnnotations;

namespace WebProgramlama.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

      
        public string role { get; set; }
    }
}
