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

    public HomeController(ILogger<HomeController> logger, IConfiguration config)
    {
        string connectionString = config?.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(config), "Configuration cannot be null");
        _logger = logger;
        _repoMaintainRecord = new MaintainRecordRepository(connectionString);
        _repoDropdownData = new DropdownDataRepository(connectionString);
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

    public IActionResult Privacy()
    {
        return View();
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

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            ViewBag.Mode = "Edit";
            ViewBag.Departments = await _repoDropdownData.GetDepartmentsAsync();
            ViewBag.problemTypes = await _repoDropdownData.GetProblemTypesAsync();
            ViewBag.processingTypes = await _repoDropdownData.GetProcessingTypesAsync();
            ViewBag.staffIds = await _repoDropdownData.GetStaffsAsync();

            var model = await _repoMaintainRecord.GetByIdAsync(id);
            if (model == null) return NotFound();

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
            return Json(ApiResponse.Fail("新增失敗",  new[] { "新增失敗：" + ex.Message } ));
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
            int record_id = req.record_id ?? 0;
            int rows = await _repoMaintainRecord.DeleteAsync(record_id);
            if (rows > 0)
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
