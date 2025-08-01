using Microsoft.AspNetCore.Mvc;

using TaskManager.CoreMVC.Enums;
using TaskManager.CoreMVC.Exceptions;
using TaskManager.CoreMVC.Models;
using TaskManager.CoreMVC.Repositories;

namespace TaskManager.CoreMVC.Controllers
{
    /// <summary>
    /// Dashboard Page Controller
    /// </summary>
    public class DashboardController : Controller
    {
        private readonly ITaskRepo _taskRepo;
        private readonly ILogger<DashboardController> _logger;
        public DashboardController(ITaskRepo taskRepo, ILogger<DashboardController> logger)
        {
            _taskRepo = taskRepo;
            _logger = logger;
        }

        #region Controller Action Methods

        /// <summary>
        /// Show Dashboard Page
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
        /// Get data for To Do List table (server side filter)
        /// </summary>
        /// <returns></returns>
        public IActionResult GetMyTasks()
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

                string userId = HttpContext.Session.GetString("Id");
                IEnumerable<EmpTaskModel> list = _taskRepo.GetTasksByEmployeeId(userId);
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
                    data = list.Skip(startIndex).Take(pageSize).ToList() // take data to display
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Show Update Status Dialog
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult UpdateStatus(string id)
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
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Update Task Status (Submit)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(EmpTaskModel model)
        {
            try
            {
                model.UpdateEmployeeId = HttpContext.Session.GetString("Id");

                _taskRepo.UpdateStatus(model);
                await _taskRepo.SaveChangesAsync();
                TempData["success"] = "Successfully updated !";

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
        /// Search by Search field
        /// </summary>
        /// <param name="searchValue"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private IEnumerable<EmpTaskModel> Search(string? searchValue, IEnumerable<EmpTaskModel> list)
        {
            if (!string.IsNullOrEmpty(searchValue))
            {
                list = list.Where(x =>
                    x.TaskName.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.StatusString.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                    || x.EstimatedEndDate != null && x.EstimatedEndDate.Contains(searchValue, StringComparison.OrdinalIgnoreCase)
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
                case (int)ToDoListTableColOrder.Status:
                    orderFunc = x => x.StatusString;
                    break;
                case (int)ToDoListTableColOrder.TaskName:
                    orderFunc = x => x.TaskName;
                    break;
                case (int)ToDoListTableColOrder.Remark:
                    orderFunc = x => x.Remark;
                    break;
                case (int)ToDoListTableColOrder.CancelReason:
                    orderFunc = x => x.CancelReason;
                    break;
                case (int)ToDoListTableColOrder.IsPrority:
                    orderFunc = x => x.IsPrority;
                    break;
                case (int)ToDoListTableColOrder.AssignDate:
                    orderFunc = x => x.AssignDate;
                    break;
                case (int)ToDoListTableColOrder.EstimatedEndDate:
                    orderFunc = x => x.EstimatedEndDate;
                    break;
                case (int)ToDoListTableColOrder.AssignEmployeeName:
                    orderFunc = x => x.AssignEmployeeName;
                    break;
                case (int)ToDoListTableColOrder.UpdateDate:
                    orderFunc = x => x.UpdateDate;
                    break;
                case (int)ToDoListTableColOrder.UpdateEmployeeName:
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
