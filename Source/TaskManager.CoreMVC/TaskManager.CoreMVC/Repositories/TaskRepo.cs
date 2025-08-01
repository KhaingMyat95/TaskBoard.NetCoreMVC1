using TaskManager.CoreMVC.Database;
using TaskManager.CoreMVC.Enums;
using TaskManager.CoreMVC.Exceptions;
using TaskManager.CoreMVC.Models;

namespace TaskManager.CoreMVC.Repositories
{
    public class TaskRepo : ITaskRepo
    {
        private readonly EmployeeTaskContext _context;
        public TaskRepo(EmployeeTaskContext context)
        {
            _context = context;
        }

        public void CancelEmpTask(EmpTaskModel empTaskModel)
        {
            EmpTask? empTask = _context.EmpTasks.Find(empTaskModel.TaskId);
            if (empTask != null)
            {
                empTask.Status = (int)EmpTaskStatus.Cancelled;
                empTask.CancelReason = empTaskModel.CancelReason;
                empTask.UpdateDate = DateTime.Now;
                empTask.UpdateEmployeeId = empTaskModel.UpdateEmployeeId;

                _context.EmpTasks.Update(empTask);
            }
        }

        public async Task CreateEmpTask(EmpTaskModel empTaskModal)
        {
            EmpTask empTask = new EmpTask();
            empTask.Id = empTaskModal.TaskId;
            empTask.EmployeeId = empTaskModal.EmployeeId;
            empTask.TaskName = empTaskModal.TaskName;
            empTask.EstimatedEndDate = empTaskModal.EstimatedEndDate != null ? DateTime.Parse(empTaskModal.EstimatedEndDate) : null;
            empTask.IsPrority = empTaskModal.IsPrority;
            empTask.AssignEmployeeId = empTaskModal.AssignEmployeeId;
            empTask.AssignDate = DateTime.Now;
            empTask.Remark = empTaskModal.Remark;
            empTask.Status = (int)EmpTaskStatus.Assign;

            await _context.EmpTasks.AddAsync(empTask);
        }

        public void DeleteEmpTask(string id)
        {
            EmpTask? empTask = _context.EmpTasks.Find(id);
            if(empTask != null)
            {
                if(empTask.Status != (int)EmpTaskStatus.Assign)
                {
                    throw new TaskBoardException(nameof(TaskBoardExceptionCode.INVALID_STATUS_DELETE), GetStatusString(empTask.Status));
                }

                _context.EmpTasks.Remove(empTask);
            }
        }

        public IEnumerable<EmpTaskModel> GetAllEmployeeTasks(string? empCode, string? taskStatus)
        {
            IQueryable<EmpTask> query = _context.EmpTasks;

            if(empCode != null && empCode.ToLower() != "all")
            {
                Employee? employee = _context.Employees.Where(x => x.EmployeeCode == empCode).FirstOrDefault();
                if(employee == null)
                {
                    return new List<EmpTaskModel>(); 
                }

                query = query.Where(x => x.EmployeeId == employee.Id);
            }

            if(taskStatus != null && taskStatus.ToLower() != "all")
            {
                query = query.Where(x => x.Status == int.Parse(taskStatus));
            }

            IEnumerable<EmpTaskModel> taskModels = query
                .AsEnumerable()
                .Select(s => new EmpTaskModel
                {
                    TaskId = s.Id,
                    EmployeeId = s.EmployeeId,
                    EmployeeCode = _context.Employees.Find(s.EmployeeId)!.EmployeeCode,
                    EmployeeName = _context.Employees.Find(s.EmployeeId)!.Name,
                    TaskName = s.TaskName,
                    EstimatedEndDate = s.EstimatedEndDate?.ToString("dd/MM/yyy HH:mm:ss tt"),
                    IsPrority = s.IsPrority,
                    AssignDate = s.AssignDate.ToString("dd/MM/yyy HH:mm:ss tt"),
                    AssignEmployeeId = s.AssignEmployeeId,
                    AssignEmployeeName = _context.Employees.Find(s.AssignEmployeeId)!.Name,
                    Remark = s.Remark,
                    Status = (EmpTaskStatus)s.Status,
                    StatusString = GetStatusString(s.Status),
                    UpdateDate = s.UpdateDate?.ToString("dd/MM/yyy HH:mm:ss tt"),
                    UpdateEmployeeId = s.UpdateEmployeeId,
                    UpdateEmployeeName = s.UpdateEmployeeId != null ? _context.Employees.Find(s.UpdateEmployeeId)!.Name : string.Empty,
                    CancelReason = s.CancelReason,
                    ActualEndDate = s.ActualEndDate?.ToString("dd/MM/yyy HH:mm:ss tt")
                })
                .OrderBy(o => o.AssignDate)
                .ToList();

            return taskModels;
        }

        public EmpTaskModel? GetTaskById(string id)
        {
            return _context.EmpTasks
                .Where(x => x.Id == id)
                .Select(s => new EmpTaskModel
                {
                    TaskId = s.Id,
                    EmployeeId = s.EmployeeId,
                    TaskName = s.TaskName,
                    EstimatedEndDate = s.EstimatedEndDate.ToString(),
                    IsPrority = s.IsPrority,
                    Remark = s.Remark,
                    Status = (EmpTaskStatus)s.Status
                }).SingleOrDefault();
        }

        public IEnumerable<EmpTaskModel> GetTasksByEmployeeId(string employeeId)
        {
            IEnumerable<EmpTaskModel> taskModels = _context.EmpTasks
                .Where(x => x.EmployeeId == employeeId
                && (x.Status < (int)EmpTaskStatus.Completed
                || x.AssignDate.Date == DateTime.Now.Date))
                .AsEnumerable()
                .Select(s => new EmpTaskModel
                {
                    TaskId = s.Id,
                    EmployeeId = s.EmployeeId,
                    EmployeeCode = _context.Employees.Find(s.EmployeeId)!.EmployeeCode,
                    EmployeeName = _context.Employees.Find(s.EmployeeId)!.Name,
                    TaskName = s.TaskName,
                    EstimatedEndDate = s.EstimatedEndDate?.ToString("dd/MM/yyy HH:mm:ss tt"),
                    IsPrority = s.IsPrority,
                    AssignDate = s.AssignDate.ToString("dd/MM/yyy HH:mm:ss tt"),
                    AssignEmployeeId = s.AssignEmployeeId,
                    AssignEmployeeName = _context.Employees.Find(s.AssignEmployeeId)!.Name,
                    Remark = s.Remark,
                    Status = (EmpTaskStatus)s.Status,
                    StatusString = GetStatusString(s.Status),
                    UpdateDate = s.UpdateDate?.ToString("dd/MM/yyy HH:mm:ss tt"),
                    UpdateEmployeeId = s.UpdateEmployeeId,
                    UpdateEmployeeName = s.UpdateEmployeeId != null ? _context.Employees.Find(s.UpdateEmployeeId)!.Name : string.Empty,
                    CancelReason = s.CancelReason
                })
                .OrderBy(o => o.AssignDate)
                .ToList();

            return taskModels;
        }

        public List<TaskStatusSelectBoxModel> GetTaskStatusSelectBoxModel()
        {
            List<TaskStatusSelectBoxModel> list = new List<TaskStatusSelectBoxModel>();
            list.Add(new TaskStatusSelectBoxModel { Status = (int)EmpTaskStatus.Assign, DisplayName = GetStatusString((int)EmpTaskStatus.Assign) });
            list.Add(new TaskStatusSelectBoxModel { Status = (int)EmpTaskStatus.Processing, DisplayName = GetStatusString((int)EmpTaskStatus.Processing) });
            list.Add(new TaskStatusSelectBoxModel { Status = (int)EmpTaskStatus.Completed, DisplayName = GetStatusString((int)EmpTaskStatus.Completed) });
            list.Add(new TaskStatusSelectBoxModel { Status = (int)EmpTaskStatus.Cancelled, DisplayName = GetStatusString((int)EmpTaskStatus.Cancelled) });

            return list;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void UpdateEmpTask(EmpTaskModel empTaskModel)
        {
            EmpTask? empTask = _context.EmpTasks
                .Where(x => x.Id == empTaskModel.TaskId)
                .SingleOrDefault();

            if (empTask == null)
            {
                throw new TaskBoardException(nameof(TaskBoardExceptionCode.DOES_NOT_EXIST), "Task");
            }

            empTask.EmployeeId = empTaskModel.EmployeeId ?? empTask.EmployeeId;
            empTask.TaskName = empTaskModel.TaskName;
            empTask.EstimatedEndDate = empTaskModel.EstimatedEndDate != null ? Convert.ToDateTime(empTaskModel.EstimatedEndDate) : null;
            empTask.IsPrority = empTaskModel.IsPrority;
            empTask.Remark = empTaskModel.Remark;
            empTask.UpdateDate = DateTime.Now;
            empTask.UpdateEmployeeId = empTaskModel.UpdateEmployeeId;

            _context.EmpTasks.Update(empTask);
        }

        public void UpdateStatus(EmpTaskModel empTaskModel)
        {
            EmpTask? empTask = _context.EmpTasks
                .Where(x => x.Id == empTaskModel.TaskId)
                .SingleOrDefault();

            if (empTask == null)
            {
                throw new TaskBoardException(nameof(TaskBoardExceptionCode.DOES_NOT_EXIST), "Task");
            }

            empTask.Status = (int)empTaskModel.Status;
            empTask.UpdateDate = DateTime.Now;
            empTask.UpdateEmployeeId = empTaskModel.UpdateEmployeeId;

            if (empTaskModel.Status == EmpTaskStatus.Completed)
            {
                empTask.ActualEndDate = DateTime.Now;
            }           
            _context.EmpTasks.Update(empTask);
        }

        #region private methods

        private string GetStatusString(int status)
        {
            switch (status)
            {
                case (int)EmpTaskStatus.Assign:
                    return "Assigned";
                case (int)EmpTaskStatus.Processing:
                    return "In Progress";
                case (int)EmpTaskStatus.Completed:
                    return "Completed";
                case (int)EmpTaskStatus.Cancelled:
                    return "Cancelled";
            }

            return string.Empty;
        }
        #endregion
    }
}
