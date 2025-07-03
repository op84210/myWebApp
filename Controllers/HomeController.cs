using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using myWebApp.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace myWebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IMaintainRecordRepository _repoMaintainRecord;
    private readonly IDropdownDataRepository _repoDropdownData;

    public HomeController(ILogger<HomeController> logger, IMaintainRecordRepository repoMaintainRecord, IDropdownDataRepository repoDropdownData)
    {
        _logger = logger;
        _repoMaintainRecord = repoMaintainRecord;
        _repoDropdownData = repoDropdownData;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            ViewBag.Departments = await _repoDropdownData.GetDepartmentsAsync();
            ViewBag.problemTypes = await _repoDropdownData.GetProblemTypesAsync();
            ViewBag.processingStaffIds = await _repoDropdownData.GetProcessingStaffIdsAsync();
            ViewBag.processingTypes = await _repoDropdownData.GetProcessingTypesAsync();

            return View();
        }
        catch (Exception ex)
        {
            return Content("連線失敗：" + ex.Message);
        }
    }

    [HttpGet("/Home/dropdown/staffs")]
    public async Task<IActionResult> GetStaffsByDepartment(string depart_code)
    {
        object staffList;

        if (string.IsNullOrEmpty(depart_code))
        {
            staffList = await _repoDropdownData.GetStaffsAsync();
        }
        else
        {
            staffList = await _repoDropdownData.GetStaffsByDepartmentAsync(depart_code);
        }

        return Json(staffList);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public async Task<IActionResult> Search([FromForm] SearchConditionViewModel model)
    {
        try
        {
            var results = await _repoMaintainRecord.SearchAsync(model);
            return Json(ApiResponse.Ok(results));
        }
        catch (Exception ex)
        {
            return Content("連線失敗：" + ex.Message);
        }
    }

    [HttpGet("Home/Edit/{intId}")]
    public async Task<IActionResult> Edit(int intId)
    {
        try
        {
            ViewBag.Mode = "Edit";

            var model = await _repoMaintainRecord.GetByIdAsync(intId);
            if (model == null) return NotFound();

            ViewBag.Departments = await _repoDropdownData.GetDepartmentsAsync();
            ViewBag.problemTypes = await _repoDropdownData.GetProblemTypesAsync();
            ViewBag.processingTypes = await _repoDropdownData.GetProcessingTypesAsync();
            ViewBag.staffIds = await _repoDropdownData.GetStaffsByDepartmentAsync(model.depart_code);

            return View("MaintainForm", model);
        }
        catch (Exception ex)
        {
            return Content("查詢失敗：" + ex.Message);
        }
    }

    // 編輯存檔
    [HttpPost]
    public async Task<IActionResult> Edit(MaintainRecordViewModel model)
    {
        //更新人員, 日期寫死
        model.update_user_id = 520;
        model.update_date = DateTime.Now;
        // 重新驗證 Model
        ModelState.Clear();
        TryValidateModel(model);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(ApiResponse.Fail("驗證失敗", errors));
        }

        try
        {
            await _repoMaintainRecord.UpdateAsync(model);
            return Json(ApiResponse.Ok());
        }
        catch (Exception ex)
        {
            return Json(ApiResponse.Fail("編輯失敗", new[] { "編輯失敗：" + ex.Message }));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            ViewBag.Mode = "Create";
            ViewBag.Departments = await _repoDropdownData.GetDepartmentsAsync();
            ViewBag.problemTypes = await _repoDropdownData.GetProblemTypesAsync();
            ViewBag.processingTypes = await _repoDropdownData.GetProcessingTypesAsync();
            ViewBag.staffIds = await _repoDropdownData.GetStaffsAsync();

            var model = new MaintainRecordViewModel { };
            return View("MaintainForm", model);
        }
        catch (Exception ex)
        {
            return Content("查詢失敗：" + ex.Message);
        }
    }

    // 新增存檔
    [HttpPost]
    public async Task<IActionResult> Create(MaintainRecordViewModel model)
    {
        //更新人員, 日期寫死
        model.update_user_id = 520;
        model.update_date = DateTime.Now;
        // 重新驗證 Model
        ModelState.Clear();
        TryValidateModel(model);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(ApiResponse.Fail("驗證失敗", errors));
        }

        // 新增資料邏輯
        try
        {
            await _repoMaintainRecord.CreateAsync(model);
            return Json(ApiResponse.Ok());
        }
        catch (Exception ex)
        {
            return Json(ApiResponse.Fail("新增失敗", new[] { "新增失敗：" + ex.Message }));
        }
    }

    //刪除
    [HttpPost]
    public async Task<IActionResult> Delete([FromBody] DeleteRequest req)
    {
        if (req == null || req.record_id == null)
            return Json(ApiResponse.Fail("缺少刪除參數"));

        try
        {
            int intRecordId = req.record_id ?? 0;
            int intRows = await _repoMaintainRecord.DeleteAsync(intRecordId);
            if (intRows > 0)
                return Json(ApiResponse.Ok());
            else
                return Json(ApiResponse.Fail("找不到資料或已刪除"));
        }
        catch (Exception ex)
        {
            return Json(ApiResponse.Fail("刪除失敗：" + ex.Message));
        }
    }

}
