using TaskManager.CoreMVC.Database;
using TaskManager.CoreMVC.Models;

namespace TaskManager.CoreMVC.Repositories
{
    public interface IEmployeeRepo
    {
        public Employee? GetEmployee(string employeeCode);
        public EmployeeModel? GetEmployeeById(string id);
        public string GenerateEmployeeCode();
        public IEnumerable<EmployeeModel> GetAllEmployees();
        public IEnumerable<EmpSelectBoxModel> GetEmpSelectBoxModel();
        public string GetEmployeeCodeById(string id);
        public Task CreateEmployee(string id, EmployeeModel model, string salt, string password);
        public void UpdateEmployee(EmployeeModel model);
        public void DeleteEmployee(string id);
        public void ChangePassword(string id, string salt, string password);
        public Task SaveChangesAsync();
    }
}
