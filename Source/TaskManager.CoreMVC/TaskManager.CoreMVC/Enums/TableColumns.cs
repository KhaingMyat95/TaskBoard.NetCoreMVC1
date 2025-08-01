namespace TaskManager.CoreMVC.Enums
{
    public enum ToDoListTableColOrder
    {
        Status = 1,
        TaskName,
        Remark,
        CancelReason,
        IsPrority,
        AssignDate,
        EstimatedEndDate,
        AssignEmployeeName,
        UpdateDate,
        UpdateEmployeeName
    }

    public enum EmpTaskTableColOrder
    {
        Status = 1,
        EmpCode,
        EmpName,
        TaskName,
        Remark,
        CancelReason,
        IsPrority,
        AssignDate,
        EstimatedEndDate,
        ActualEndDate,
        AssignEmployeeName,
        UpdateDate,
        UpdateEmployeeName
    }

    public enum EmpTableColOrder
    {
        Code = 1,
        Name,
        Admin,
        RegisteredDate,
        UpdatedDate,
    }
}
