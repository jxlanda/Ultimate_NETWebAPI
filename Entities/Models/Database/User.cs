using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models.Database
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string FirstLastName { get; set; }
        public string SecondLastName { get; set; }
        public string AvatarUrl { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

    }

    public class UserParameters : QueryStringParameters
    {

    }
}
