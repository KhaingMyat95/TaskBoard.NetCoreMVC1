using Microsoft.Build.Framework;
using System.ComponentModel;
using TaskManager.CoreMVC.Enums;

namespace TaskManager.CoreMVC.Models
{
    public class EmpTaskModel
    {
        public string TaskId { get; set; } = null!;
        public string EmployeeId { get; set; } = null!;

        [DisplayName("Employee Code")]
        public string EmployeeCode { get; set; } = null!;

        [DisplayName("Employee Name")]
        public string EmployeeName { get; set; } = null!;

        [DisplayName("Task")]
        [Required]
        public string TaskName { get; set; } = null!;

        [DisplayName("Expected End Date")]
        public string? EstimatedEndDate { get; set; }

        [DisplayName("Priority")]
        public bool IsPrority { get; set; }

        [DisplayName("Assigned Date")]
        public string AssignDate { get; set; } = null!;
        public string AssignEmployeeId { get; set; } = null!;

        [DisplayName("Assigned By")]
        public string AssignEmployeeName { get; set; } = null!;

        public string? Remark { get; set; }

        public EmpTaskStatus Status { get; set; }

        [DisplayName("Current Status")]
        public string StatusString { get; set; } = String.Empty!;

        [DisplayName("Updated Date")]
        public string? UpdateDate { get; set; }
        public string? UpdateEmployeeId { get; set; }

        [DisplayName("Updated By")]
        public string UpdateEmployeeName { get; set; } = null!;

        [DisplayName("Cancellation Reason")]
        public string? CancelReason { get; set; }

        [DisplayName("Actual End Date")]
        public string? ActualEndDate { get; set; }
    }
}
