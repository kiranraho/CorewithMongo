using System.ComponentModel.DataAnnotations;

namespace CoreApiWithMongo.API.Dtos
{
    public class UserforRegister
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(8,MinimumLength=4,ErrorMessage="invalid password.")]
        public string Password { get; set; }
    }
}