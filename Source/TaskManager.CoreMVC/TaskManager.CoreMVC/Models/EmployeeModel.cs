using System.ComponentModel.DataAnnotations;

namespace TaskManager.CoreMVC.Models
{
    public class EmployeeModel
    {
        public string Id { get; set; } = null!;

        [Display(Name = "Employee Code")]
        [Required]
        public string EmployeeCode { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Password { get; set; } = string.Empty!;

        [Display(Name = "Admin")]
        public bool IsAdmin { get; set; }

        [Display(Name = "Registered Date")]
        public string RegistDate { get; set; }

        [Display(Name = "Updated Date")]
        public string? UpdateDate { get; set; }
    }
}
