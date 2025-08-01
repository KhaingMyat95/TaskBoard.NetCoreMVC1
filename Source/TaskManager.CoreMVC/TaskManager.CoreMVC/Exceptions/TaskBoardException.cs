using TaskManager.CoreMVC.Enums;

namespace TaskManager.CoreMVC.Exceptions
{
    public class TaskBoardException : Exception
    {
        private static string ErrorMessage = string.Empty;

        public TaskBoardException(string errCode, string param) : base(ErrorMessage) 
        { 
            if(errCode == nameof(TaskBoardExceptionCode.DOES_NOT_EXIST))
            {
                ErrorMessage = string.Format("{0} does not exists.", param);
            }

            if (errCode == nameof(TaskBoardExceptionCode.INVALID_STATUS_DELETE))
            {
                ErrorMessage = string.Format("Invalid status to delete task. status = {0}", param);
            }
        }
    }
}
