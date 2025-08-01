using System;
using System.Collections.Generic;

namespace TaskManager.CoreMVC.Database
{
    public partial class Employee
    {
        public string Id { get; set; } = null!;
        public string EmployeeCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string PwSalt { get; set; } = null!;
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public DateTime RegistDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
