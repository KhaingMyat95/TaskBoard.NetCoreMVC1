using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.CoreMVC.Models
{
    public class LoginModel
    {
        [Required]
        [DisplayName("Employee Code")]
        public string EmployeeCode { get; set; } = null!;

        [Required]
        [DisplayName("Password")]
        public string Password { get; set; } = null!;
    }
}
