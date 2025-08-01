using System;
using System.Collections.Generic;

namespace TaskManager.CoreMVC.Database
{
    public partial class EmpTask
    {
        public string Id { get; set; } = null!;
        public string EmployeeId { get; set; } = null!;
        public string TaskName { get; set; } = null!;
        public DateTime? EstimatedEndDate { get; set; }
        public bool IsPrority { get; set; }
        public DateTime AssignDate { get; set; }
        public string AssignEmployeeId { get; set; } = null!;
        public string? Remark { get; set; }
        public int Status { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateEmployeeId { get; set; }
        public string? CancelReason { get; set; }
        public DateTime? ActualEndDate { get; set; }
    }
}
