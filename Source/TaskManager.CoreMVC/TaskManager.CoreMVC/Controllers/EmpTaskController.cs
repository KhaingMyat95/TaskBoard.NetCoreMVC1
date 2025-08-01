using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManager.CoreMVC.Enums;
using TaskManager.CoreMVC.Exceptions;
using TaskManager.CoreMVC.Models;
using TaskManager.CoreMVC.Repositories;

namespace TaskManager.CoreMVC.Controllers
{
    /// <summary>
    /// Employee Task Page Controller
    /// </summary>
    public class EmpTaskController : Controller
    {
        private readonly ITaskRepo _taskRepo;
        private readonly IEmployeeRepo _empRepo;
        private readonly ILogger<EmpTaskController> _logger;
        public EmpTaskController(ITaskRepo taskRepo, IEmployeeRepo empRepo, ILogger<EmpTaskController> logger)
        {
            _taskRepo = taskRepo;
            _empRepo = empRepo;
            _logger = logger;
        }

        #region Controller Action Methods

        /// <summary>
        /// Show Employee Task page
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            try
            {
                IEnumerable<EmpTaskModel> list = new List<EmpTaskModel>();
                return View(list);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get data for Employee Task table (Server side filter)
        /// </summary>
        /// <param name="empCode"></param>
        /// <param name="taskStatus"></param>
        /// <returns></returns>
        public IActionResult GetEmpTasks(string? empCode, string? taskStatus)
        {
            try
            {
                // Getting table filter info
                int draw = Convert.ToInt32(Request.Form["draw"]);
                int startIndex = Convert.ToInt32(Request.Form["start"]);
                int pageSize = Convert.ToInt32(Request.Form["length"]);
                int sortCol = Convert.ToInt32(Request.Form["order[0][column]"]);
                string sortDir = Request.Form["order[0][dir]"];
                string? searchField = Request.Form["search[value]"].FirstOrDefault()?.Trim();

                IEnumerable<EmpTaskModel> list = _taskRepo.GetAllEmployeeTasks(empCode, taskStatus);
                int totalCount = list.Count();

                // Perform search
                list = Search(searchField, list);

                // Perform sort
                list = Sort(sortCol, sortDir, list);

                int resultCount = list.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalCount,
                    recordsFiltered = resultCount,
                    data = list.Skip(startIndex).Take(pageSize).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get data for employee select box filter
        /// </summary>
        /// <returns></returns>
        public IActionResult GetEmployeeSelectBoxModel()
        {
            try
            {
                IEnumerable<EmpSelectBoxModel> list = _empRepo.GetEmpSelectBoxModel();
                return Json(list.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get data for task status select box filter
        /// </summary>
        /// <returns></returns>
        public IActionResult GetTaskStatusSelectBoxModel()
        {
            try
            {
                List<TaskStatusSelectBoxModel> list = _taskRepo.GetTaskStatusSelectBoxModel();
                return Json(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Show Create or Edit Task dialog
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public IActionResult CreateOrEditTask(string? taskId)
        {
            try
            {
                #region get data for employee select box

                IEnumerable<EmpSelectBoxModel> list = _empRepo.GetEmpSelectBoxModel();
                List<SelectListItem> employees = new List<SelectListItem>();
                foreach (var item in list)
                {
                    employees.Add(new SelectListItem { Text = item.DisplayName, Value = item.EmployeeId });
                }
                ViewBag.Employees = employees;

                #endregion

                if (string.IsNullOrWhiteSpace(taskId))
                {
                    return View(new EmpTaskModel());
                }

                EmpTaskModel? empTaskModel = _taskRepo.GetTaskById(taskId);
                if (empTaskModel == null)
                {
                    return NotFound();
                }

                return View(empTaskModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                TempData["error"] = "Unexpected error occurred !";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Create or Edit Task (Submit)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEditTask(EmpTaskModel model)
        {
            string employeeCode = _empRepo.GetEmployeeCodeById(model.EmployeeId);
            try
            {
                string requestUserId = HttpContext.Session.GetString("Id");

                string successMessage = string.Empty;
                // Create
                if (string.IsNullOrWhiteSpace(model.TaskId))
                {
                    Guid id = Guid.NewGuid();
                    model.TaskId = id.ToString();
                    model.AssignEmployeeId = requestUserId;

                    await _taskRepo.CreateEmpTask(model);
                    successMessage = "Successfully created !";
                }
                // Edit
                else
                {
                    model.UpdateEmployeeId = requestUserId;

                    _taskRepo.UpdateEmpTask(model);
                    successMessage = "Successfully updated !";
                }
                await _taskRepo.SaveChangesAsync();
                TempData["success"] = successMessage;

                return RedirectToAction("Index");
            }
            catch (TaskBoardException e)
            {
                TempData["error"] = e.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                TempData["error"] = "Unexpected error occurred !";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Show Delete Task dialog
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult DeleteTask(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest();
                }

                EmpTaskModel? empTaskModel = _taskRepo.GetTaskById(id);
                if (empTaskModel == null)
                {
                    return NotFound();
                }

                return View(empTaskModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Delete Task (Submit)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTask(EmpTaskModel model)
        {
            string employeeCode = _empRepo.GetEmployeeCodeById(model.EmployeeId);
            try
            {
                _taskRepo.DeleteEmpTask(model.TaskId);
                await _taskRepo.SaveChangesAsync();
                TempData["success"] = "Successfully deleted !";

                return RedirectToAction("Index");
            }
            catch (TaskBoardException ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                TempData["error"] = "Unexpected error occurred !";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Show Cancel Task dialog
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CancelTask(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest();
                }

                EmpTaskModel? empTaskModel = _taskRepo.GetTaskById(id);
                if (empTaskModel == null)
                {
                    return NotFound();
                }

                return View(empTaskModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Cancel Task (Submit)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelTask(EmpTaskModel model)
        {
            string employeeCode = _empRepo.GetEmployeeCodeById(model.EmployeeId);
            try
            {
                model.UpdateEmployeeId = HttpContext.Session.GetString("Id");

                _taskRepo.CancelEmpTask(model);
                await _taskRepo.SaveChangesAsync();
                TempData["success"] = "Successfully cancelled !";

                return RedirectToAction("Index");
            }
            catch (TaskBoardException ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                TempData["error"] = "Unexpected error occurred !";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Search by Search Field
        /// </summary>
        /// <param name="searchValue"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IEnumerable<EmpTaskModel> Search(string? searchValue, IEnumerable<EmpTaskModel> list)
        {
            if (!string.IsNullOrEmpty(searchValue))
            {
                list = list.Where(x =>
                    x.EmployeeCode.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.EmployeeName.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.TaskName.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.StatusString.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.EstimatedEndDate != null && x.EstimatedEndDate.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.ActualEndDate != null && x.ActualEndDate.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.Remark != null && x.Remark.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.CancelReason != null && x.CancelReason.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.UpdateDate != null && x.UpdateDate.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.AssignDate.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.AssignEmployeeName.Contains(searchValue, StringComparison.OrdinalIgnoreCase));
            }
            return list;
        }

        /// <summary>
        /// Sort by sort column and order
        /// </summary>
        /// <param name="orderCol"></param>
        /// <param name="orderDir"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IEnumerable<EmpTaskModel> Sort(int orderCol, string orderDir, IEnumerable<EmpTaskModel> list)
        {
            Func<EmpTaskModel, object> orderFunc = null;
            switch (orderCol)
            {
                case (int)EmpTaskTableColOrder.Status:
                    orderFunc = x => x.StatusString;
                    break;
                case (int)EmpTaskTableColOrder.EmpCode:
                    orderFunc = x => x.EmployeeCode;
                    break;
                case (int)EmpTaskTableColOrder.EmpName:
                    orderFunc = x => x.EmployeeName;
                    break;
                case (int)EmpTaskTableColOrder.TaskName:
                    orderFunc = x => x.TaskName;
                    break;
                case (int)EmpTaskTableColOrder.Remark:
                    orderFunc = x => x.Remark;
                    break;
                case (int)EmpTaskTableColOrder.CancelReason:
                    orderFunc = x => x.CancelReason;
                    break;
                case (int)EmpTaskTableColOrder.IsPrority:
                    orderFunc = x => x.IsPrority;
                    break;
                case (int)EmpTaskTableColOrder.AssignDate:
                    orderFunc = x => x.AssignDate;
                    break;
                case (int)EmpTaskTableColOrder.EstimatedEndDate:
                    orderFunc = x => x.EstimatedEndDate;
                    break;
                case (int)EmpTaskTableColOrder.ActualEndDate:
                    orderFunc = x => x.ActualEndDate;
                    break;
                case (int)EmpTaskTableColOrder.AssignEmployeeName:
                    orderFunc = x => x.AssignEmployeeName;
                    break;
                case (int)EmpTaskTableColOrder.UpdateDate:
                    orderFunc = x => x.UpdateDate;
                    break;
                case (int)EmpTaskTableColOrder.UpdateEmployeeName:
                    orderFunc = x => x.UpdateEmployeeName;
                    break;
            }

            if (orderDir != null && orderFunc != null)
            {
                if (orderDir.ToLower() == "desc")
                {
                    list = list.OrderByDescending(orderFunc);
                }
                else
                {
                    list = list.OrderBy(orderFunc);
                }
            }

            return list;
        }

        #endregion
    }
}
