using TaskManager.CoreMVC.Database;
using TaskManager.CoreMVC.Enums;
using TaskManager.CoreMVC.Exceptions;
using TaskManager.CoreMVC.Models;

namespace TaskManager.CoreMVC.Repositories
{
    public class EmployeeRepo : IEmployeeRepo
    {
        private readonly EmployeeTaskContext _context;
        public EmployeeRepo(EmployeeTaskContext context)
        {
            _context = context;
        }

        public void ChangePassword(string id, string salt, string password)
        {
            Employee? employee = _context.Employees
                .Where(x => x.Id == id && x.IsActive == true)
                .SingleOrDefault();
            if (employee == null)
            {
                throw new TaskBoardException(nameof(TaskBoardExceptionCode.DOES_NOT_EXIST), "Employee");
            }

            employee.Password = password;
            employee.PwSalt = salt;
            employee.UpdateDate = DateTime.Now;
            _context.Employees.Update(employee);
        }

        public async Task CreateEmployee(string id, EmployeeModel model, string salt, string password)
        {
            Employee employee = new Employee()
            {
                Id = id,
                Name = model.Name,
                EmployeeCode = model.EmployeeCode,
                PwSalt = salt,
                Password = password,
                IsActive = true,
                IsAdmin = model.IsAdmin,
                RegistDate = DateTime.Now
            };
            await _context.Employees.AddAsync(employee);
        }

        public void DeleteEmployee(string id)
        {
            Employee? employee = _context.Employees.Find(id);
            if(employee != null)
            {
                // Check reference in EmpTask table
                bool isRefer = _context.EmpTasks.Any(x => x.EmployeeId == employee.Id 
                || x.AssignEmployeeId == employee.Id);

                if (isRefer)
                {
                    employee.IsActive = false;
                    _context.Employees.Update(employee);
                }
                else
                {
                    _context.Employees.Remove(employee);
                }
            }
        }

        public string GenerateEmployeeCode()
        {
            Employee? emp = _context
                .Employees.OrderByDescending(x => x.RegistDate)
                .FirstOrDefault();

            long number = 1;
            if (emp != null)
            {
                string code = emp.EmployeeCode.Substring(1);
                number = Int64.Parse(code);
                number++;
            }
            return string.Format("U{0}", number.ToString("0000"));
        }

        public IEnumerable<EmployeeModel> GetAllEmployees()
        {
            IEnumerable<EmployeeModel> employees = _context.Employees
                .Where(x => x.IsActive == true)
                .AsEnumerable()
                .Select(x => new EmployeeModel
                {
                    Id = x.Id,
                    EmployeeCode = x.EmployeeCode,
                    Name = x.Name,
                    IsAdmin = x.IsAdmin,
                    RegistDate = x.RegistDate.ToString("dd/MM/yyy HH:mm:ss tt"),
                    UpdateDate = x.UpdateDate?.ToString("dd/MM/yyy HH:mm:ss tt")
                }).
                OrderBy(o => o.EmployeeCode).ToList();

            return employees;
        }

        public IEnumerable<EmpSelectBoxModel> GetEmpSelectBoxModel()
        {
            IEnumerable<EmpSelectBoxModel> employees = _context.Employees
                .Where(x => x.IsActive == true)
                .Select(x => new EmpSelectBoxModel
                {
                    EmployeeId = x.Id,
                    EmployeeCode = x.EmployeeCode,
                    DisplayName = string.Format("{0}({1})", x.EmployeeCode, x.Name)
                })
                .OrderBy(o => o.EmployeeCode).ToList();

            return employees;
        }

        public Employee? GetEmployee(string employeeCode)
        {
            return _context.Employees
                .Where(x => x.EmployeeCode == employeeCode && x.IsActive == true)
                .SingleOrDefault();
        }

        public EmployeeModel? GetEmployeeById(string id)
        {
            return _context.Employees
                .Where(x => x.Id == id && x.IsActive == true)
                .Select(s => new EmployeeModel
                {
                    Id = s.Id,
                    EmployeeCode = s.EmployeeCode,
                    Name = s.Name,
                    IsAdmin = s.IsAdmin,
                    Password = "dummyPassword"
                })
                .SingleOrDefault();
        }

        public string GetEmployeeCodeById(string id)
        {
            return _context.Employees.Find(id)?.EmployeeCode;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void UpdateEmployee(EmployeeModel model)
        {
            Employee? employee = _context.Employees
                .Where(x => x.Id == model.Id && x.IsActive == true)
                .SingleOrDefault();
            if(employee == null)
            {
                throw new TaskBoardException(nameof(TaskBoardExceptionCode.DOES_NOT_EXIST), "Employee");
            }

            employee.Name = model.Name;
            employee.IsAdmin = model.IsAdmin;
            employee.UpdateDate = DateTime.Now;
            _context.Employees.Update(employee);
        }
    }
}
