using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace TaskManager.CoreMVC.Models
{
    public class EmpSelectBoxModel
    {
        public string EmployeeId { get; set; } = null!;
        public string EmployeeCode { get; set; } = null!;

        public string DisplayName { get; set; } = null!;
    }
}
