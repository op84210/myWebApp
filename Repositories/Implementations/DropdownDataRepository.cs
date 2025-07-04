using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// 下拉選單資料存取層，提供各種下拉選單資料查詢（EF Core）。
/// </summary>
public class DropdownDataRepository : IDropdownDataRepository
{
    private readonly MyDbContext m_db;

    /// <summary>
    /// 建構式，注入DbContext
    /// </summary>
    /// <param name="db">資料庫DbContext</param>
    public DropdownDataRepository(MyDbContext db)
    {
        m_db = db;
    }

    /// <summary>
    /// 依單位查詢人員下拉選單
    /// </summary>
    /// <param name="strDepartCode">單位代碼</param>
    /// <returns>人員下拉選單清單</returns>
    public async Task<List<SelectListItem>> GetStaffsByDepartmentAsync(string strDepartCode)
    {
        // EF Core 改寫
        var list = await m_db.Staffs
            .Where(s => s.depart_code == strDepartCode)
            .OrderBy(s => s.staff_name)
            .Select(s => new SelectListItem
            {
                Value = s.staff_id.ToString(),
                Text = s.staff_name
            })
            .ToListAsync();
        return list;
    }
    /// <summary>
    /// 查詢所有單位下拉選單
    /// </summary>
    /// <returns>單位下拉選單清單</returns>
    public async Task<List<SelectListItem>> GetDepartmentsAsync()
    {
        var list = await m_db.Departments
            .OrderBy(d => d.depart_name)
            .Select(d => new SelectListItem
            {
                Value = d.depart_code,
                Text = d.depart_name
            })
            .ToListAsync();
        return list;
    }

    /// <summary>
    /// 查詢所有問題類別下拉選單
    /// </summary>
    /// <returns>問題類別下拉選單清單</returns>
    public async Task<List<SelectListItem>> GetProblemTypesAsync()
    {
        var list = await m_db.Codes
            .Where(c => c.kind == "QUESTION")
            .OrderBy(c => c.description)
            .Select(c => new SelectListItem
            {
                Value = c.code,
                Text = c.description
            })
            .ToListAsync();
        return list;
    }

    /// <summary>
    /// 查詢所有處理人員下拉選單（僅出現在維護紀錄的）
    /// </summary>
    /// <returns>處理人員下拉選單清單</returns>
    public async Task<List<SelectListItem>> GetProcessingStaffIdsAsync()
    {
        var list = await m_db.Staffs
            .Where(s => m_db.MaintainRecords.Any(r => r.processing_staff_id == s.staff_id))
            .OrderBy(s => s.staff_name)
            .Select(s => new SelectListItem
            {
                Value = s.staff_id.ToString(),
                Text = s.staff_name
            })
            .ToListAsync();
        return list;
    }

    /// <summary>
    /// 查詢所有處理類別下拉選單
    /// </summary>
    /// <returns>處理類別下拉選單清單</returns>
    public async Task<List<SelectListItem>> GetProcessingTypesAsync()
    {
        var list = await m_db.Codes
            .Where(c => c.kind == "PROCESSING")
            .OrderBy(c => c.code)
            .Select(c => new SelectListItem
            {
                Value = c.code,
                Text = c.description
            })
            .ToListAsync();
        return list;
    }

    /// <summary>
    /// 查詢所有人員下拉選單
    /// </summary>
    /// <returns>人員下拉選單清單</returns>
    public async Task<List<SelectListItem>> GetStaffsAsync()
    {
        var list = await m_db.Staffs
            .OrderBy(s => s.staff_name)
            .Select(s => new SelectListItem
            {
                Value = s.staff_id.ToString(),
                Text = s.staff_name
            })
            .ToListAsync();
        return list;
    }
}