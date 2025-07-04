using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using myWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace myWebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> m_logger;
    private readonly IMaintainRecordRepository m_repoMaintainRecord;
    private readonly IDropdownDataRepository m_repoDropdownData;
    private readonly MyDbContext _db;

    public HomeController(MyDbContext db, ILogger<HomeController> logger, IMaintainRecordRepository repoMaintainRecord, IDropdownDataRepository repoDropdownData)
    {
        _db = db;
        m_logger = logger;
        m_repoMaintainRecord = repoMaintainRecord;
        m_repoDropdownData = repoDropdownData;
    }

    [HttpGet("/Home/DbTest")]
    public IActionResult DbTest()
    {
        // 取出前10筆並 Include 關聯
        var records = _db.MaintainRecords
            .Include(m => m.Department)
            .Include(m => m.Staff)
            .Include(m => m.ProblemTypeCode)
                .Where(m => m.ProblemTypeCode != null && m.ProblemTypeCode.kind == "QUESTION")
            .Include(m => m.ProcessingTypeCode)
                .Where(m => m.ProcessingTypeCode != null && m.ProcessingTypeCode.kind == "PROCESSING")
            .Include(m => m.ProcessingStaff)
            .Take(10)
            .ToList();

        if (records.Count == 0)
            return Content("查無資料，DbContext 連線成功但資料表無資料");

        var lines = records.Select(r =>
            $@" record_id: {r.record_id}, 
                apply_date: {r.apply_date}, 
                depart_name: {r.Department?.depart_name}, 
                staff_name: {r.Staff?.staff_name}, 
                problem_type: {r.ProblemTypeCode?.description }, 
                processing_type: {r.ProcessingTypeCode?.description }, 
                processing_staff_name: {r.ProcessingStaff?.staff_name}, 
                completion_date: {r.completion_date}"
        );

        return Content(string.Join("\n", lines));
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            ViewBag.Departments = await m_repoDropdownData.GetDepartmentsAsync();
            ViewBag.problemTypes = await m_repoDropdownData.GetProblemTypesAsync();
            ViewBag.processingStaffIds = await m_repoDropdownData.GetProcessingStaffIdsAsync();
            ViewBag.processingTypes = await m_repoDropdownData.GetProcessingTypesAsync();

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
            staffList = await m_repoDropdownData.GetStaffsAsync();
        }
        else
        {
            staffList = await m_repoDropdownData.GetStaffsByDepartmentAsync(depart_code);
        }

        return Json(staffList);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public async Task<IActionResult> Search([FromForm] SearchConditionViewModel model, int page = 1, int pageSize = 10)
    {
        try
        {
            var (results, totalCount) = await m_repoMaintainRecord.SearchPagedAsync(model, page, pageSize);
            return Json(ApiResponse.Ok(new { results = results, totalCount }));
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

            var model = await m_repoMaintainRecord.GetByIdAsync(intId);
            if (model == null) return NotFound();

            ViewBag.Departments = await m_repoDropdownData.GetDepartmentsAsync();
            ViewBag.problemTypes = await m_repoDropdownData.GetProblemTypesAsync();
            ViewBag.processingTypes = await m_repoDropdownData.GetProcessingTypesAsync();
            ViewBag.staffIds = await m_repoDropdownData.GetStaffsByDepartmentAsync(model.depart_code);

            return View("MaintainForm", model);
        }
        catch (Exception ex)
        {
            return Content("查詢失敗：" + ex.Message);
        }
    }

    // 編輯存檔
    [HttpPost]
    public async Task<IActionResult> Edit(MaintainRecord model)
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
            await m_repoMaintainRecord.UpdateAsync(model);
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
            ViewBag.Departments = await m_repoDropdownData.GetDepartmentsAsync();
            ViewBag.problemTypes = await m_repoDropdownData.GetProblemTypesAsync();
            ViewBag.processingTypes = await m_repoDropdownData.GetProcessingTypesAsync();
            ViewBag.staffIds = await m_repoDropdownData.GetStaffsAsync();

            var model = new MaintainRecord { };
            return View("MaintainForm", model);
        }
        catch (Exception ex)
        {
            return Content("查詢失敗：" + ex.Message);
        }
    }

    // 新增存檔
    [HttpPost]
    public async Task<IActionResult> Create(MaintainRecord model)
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
            await m_repoMaintainRecord.CreateAsync(model);
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
            int intRows = await m_repoMaintainRecord.DeleteAsync(intRecordId);
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
