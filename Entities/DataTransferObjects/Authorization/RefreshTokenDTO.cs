using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DataTransferObjects.Authorization
{
    public class RefreshTokenDTO
    {
        public string RefreshToken { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
