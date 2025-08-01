using TaskManager.CoreMVC.Models;

namespace TaskManager.CoreMVC.Repositories
{
    public interface ITaskRepo
    {
        public IEnumerable<EmpTaskModel> GetTasksByEmployeeId(string employeeId);
        public IEnumerable<EmpTaskModel> GetAllEmployeeTasks(string? empCode, string? taskStatus);
        public EmpTaskModel? GetTaskById(string id);
        public List<TaskStatusSelectBoxModel> GetTaskStatusSelectBoxModel();
        public Task CreateEmpTask(EmpTaskModel empTask);
        public void UpdateEmpTask(EmpTaskModel empTask);
        public void UpdateStatus(EmpTaskModel empTaskModal);
        public void DeleteEmpTask(string id);
        public void CancelEmpTask(EmpTaskModel empTaskModel);
        public Task SaveChangesAsync();
    }
}
