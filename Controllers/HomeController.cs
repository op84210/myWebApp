using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using myWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace myWebApp.Controllers;

/// <summary>
/// 主控台 Controller，負責首頁、CRUD、下拉選單等功能。
/// </summary>
public class HomeController : Controller
{
    // 日誌物件
    private readonly ILogger<HomeController> m_logger;
    // 維護紀錄資料存取層（Repository）
    private readonly IMaintainRecordRepository m_repoMaintainRecord;
    // 下拉選單資料存取層（Repository）
    private readonly IDropdownDataRepository m_repoDropdownData;

    /// <summary>
    /// 建構式，注入Repository
    /// </summary>
    /// <param name="logger">日誌物件</param>
    /// <param name="repoMaintainRecord">維護紀錄Repository</param>
    /// <param name="repoDropdownData">下拉選單Repository</param>
    public HomeController(ILogger<HomeController> logger, IMaintainRecordRepository repoMaintainRecord, IDropdownDataRepository repoDropdownData)
    {
        m_logger = logger;
        m_repoMaintainRecord = repoMaintainRecord;
        m_repoDropdownData = repoDropdownData;
    }

    /// <summary>
    /// 首頁，載入所有下拉選單資料
    /// </summary>
    /// <returns>首頁View</returns>
    public async Task<IActionResult> Index()
    {
        try
        {
            ViewBag.Departments = await m_repoDropdownData.GetDepartmentsAsync(); // 單位下拉選單
            ViewBag.problemTypes = await m_repoDropdownData.GetProblemTypesAsync(); // 問題類別下拉選單
            ViewBag.processingStaffIds = await m_repoDropdownData.GetProcessingStaffIdsAsync(); // 處理人員下拉選單
            ViewBag.processingTypes = await m_repoDropdownData.GetProcessingTypesAsync(); // 處理類別下拉選單

            return View();
        }
        catch (Exception ex)
        {
            return Content("連線失敗：" + ex.Message);
        }
    }

    /// <summary>
    /// 依單位取得人員下拉選單（AJAX）
    /// </summary>
    /// <param name="depart_code">單位代碼，若為空則回傳全部人員</param>
    /// <returns>人員下拉選單資料（JSON）</returns>
    [HttpGet("/Home/dropdown/staffs")]
    public async Task<IActionResult> GetStaffsByDepartment(string depart_code)
    {
        object staffList;

        if (string.IsNullOrEmpty(depart_code))
        {
            staffList = await m_repoDropdownData.GetStaffsAsync(); // 全部人員
        }
        else
        {
            staffList = await m_repoDropdownData.GetStaffsByDepartmentAsync(depart_code); // 指定單位人員
        }

        return Json(staffList);
    }

    /// <summary>
    /// 查詢（分頁、多條件）
    /// </summary>
    /// <param name="model">查詢條件</param>
    /// <param name="page">頁碼</param>
    /// <param name="pageSize">每頁筆數</param>
    /// <returns>查詢結果（JSON）</returns>
    [HttpPost]
    public async Task<IActionResult> Search([FromForm] SearchCondition model, int page = 1, int pageSize = 10)
    {
        try
        {
            // ApplyEndDate、CompletionEndDate 時間設為 23:59:59，確保查詢區間正確
            if (model.ApplyEndDate.HasValue)
                model.ApplyEndDate = model.ApplyEndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            if (model.CompletionEndDate.HasValue)
                model.CompletionEndDate= model.CompletionEndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            // 執行分頁查詢
            var (results, totalCount) = await m_repoMaintainRecord.SearchPagedAsync(model, page, pageSize);
            return Json(ApiResponse.Ok(new { results = results, totalCount }));
        }
        catch (Exception ex)
        {
            return Content("連線失敗：" + ex.Message);
        }
    }

    /// <summary>
    /// 編輯頁面（GET）
    /// </summary>
    /// <param name="intId">維護紀錄ID</param>
    /// <returns>編輯頁面View</returns>
    [HttpGet("Home/Edit/{intId}")]
    public async Task<IActionResult> Edit(int intId)
    {
        try
        {
            ViewBag.Mode = "Edit";

            var model = await m_repoMaintainRecord.GetByIdAsync(intId);
            if (model == null) return NotFound();

            // 載入下拉選單資料
            ViewBag.Departments = await m_repoDropdownData.GetDepartmentsAsync();
            ViewBag.problemTypes = await m_repoDropdownData.GetProblemTypesAsync();
            ViewBag.processingTypes = await m_repoDropdownData.GetProcessingTypesAsync();
            ViewBag.staffIdsByDepartment = await m_repoDropdownData.GetStaffsByDepartmentAsync(model.depart_code ?? string.Empty);
            ViewBag.staffIds = await m_repoDropdownData.GetStaffsAsync();

            return View("MaintainForm", model);
        }
        catch (Exception ex)
        {
            return Content("查詢失敗：" + ex.Message);
        }
    }

    /// <summary>
    /// 編輯存檔（POST）
    /// </summary>
    /// <param name="model">維護紀錄資料</param>
    /// <returns>儲存結果（JSON）</returns>
    [HttpPost]
    public async Task<IActionResult> Edit(MaintainRecord model)
    {
        // 更新人員、日期（範例寫死）
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

    /// <summary>
    /// 新增頁面（GET）
    /// </summary>
    /// <returns>新增頁面View</returns>
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

    /// <summary>
    /// 新增存檔（POST）
    /// </summary>
    /// <param name="model">維護紀錄資料</param>
    /// <returns>儲存結果（JSON）</returns>
    [HttpPost]
    public async Task<IActionResult> Create(MaintainRecord model)
    {
        // 更新人員、日期（範例寫死）
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

    /// <summary>
    /// 刪除（POST）
    /// </summary>
    /// <param name="req">刪除請求物件</param>
    /// <returns>刪除結果（JSON）</returns>
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
