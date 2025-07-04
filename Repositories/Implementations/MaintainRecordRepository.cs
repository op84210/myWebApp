using myWebApp.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// 維護紀錄資料存取層，提供CRUD與分頁查詢等功能（EF Core）。
/// </summary>
public class MaintainRecordRepository : IMaintainRecordRepository
{
    private readonly MyDbContext m_db;

    /// <summary>
    /// 建構式，注入DbContext
    /// </summary>
    /// <param name="db">資料庫DbContext</param>
    public MaintainRecordRepository(MyDbContext db)
    {
        m_db = db;
    }

    /// <summary>
    /// 分頁查詢維護紀錄（多條件、關聯、投影）
    /// </summary>
    /// <param name="model">查詢條件</param>
    /// <param name="page">頁碼</param>
    /// <param name="pageSize">每頁筆數</param>
    /// <returns>查詢結果與總筆數</returns>
    public async Task<(List<SearchResult>, int)> SearchPagedAsync(SearchCondition model, int page, int pageSize)
    {
        // 建立查詢：僅取出問題類別為QUESTION、處理類別為PROCESSING的資料（含關聯）
        var query = m_db.MaintainRecords
            .AsNoTracking()
            .Where(x => (x.ProblemTypeCode == null || x.ProblemTypeCode.kind == "QUESTION") &&
                        (x.ProcessingTypeCode == null || x.ProcessingTypeCode.kind == "PROCESSING"));

        // 動態組合查詢條件（依使用者輸入）
        if (model.ApplyStartDate != null)
            query = query.Where(x => x.apply_date >= model.ApplyStartDate); // 申報起始日
        if (model.ApplyEndDate != null)
            query = query.Where(x => x.apply_date <= model.ApplyEndDate);   // 申報結束日
        if (!string.IsNullOrEmpty(model.depart_code))
            query = query.Where(x => x.depart_code == model.depart_code);   // 單位
        if (!string.IsNullOrEmpty(model.problem_type))
            query = query.Where(x => x.problem_type == model.problem_type); // 問題類別
        if (model.processing_staff_id != null)
            query = query.Where(x => x.processing_staff_id == model.processing_staff_id); // 處理人員
        if (!string.IsNullOrEmpty(model.processing_type))
            query = query.Where(x => x.processing_type == model.processing_type); // 處理類別
        if (model.CompletionStartDate != null)
            query = query.Where(x => x.completion_date >= model.CompletionStartDate); // 完成起始日
        if (model.CompletionEndDate != null)
            query = query.Where(x => x.completion_date <= model.CompletionEndDate);   // 完成結束日
        
        // 投影：只取出前端需要的欄位，並處理關聯欄位
        var projected = query
            .Select(x => new SearchResult
            {
                record_id = x.record_id ?? 0,
                apply_date = x.apply_date ?? DateTime.MinValue,
                depart_name = x.Department != null ? x.Department.depart_name : null,
                staff_name = x.Staff != null ? x.Staff.staff_name : null,
                problem_type = x.ProblemTypeCode != null ? x.ProblemTypeCode.description : null,
                processing_type = x.ProcessingTypeCode != null ? x.ProcessingTypeCode.description : null,
                processing_staff_name = x.ProcessingStaff != null ? x.ProcessingStaff.staff_name : null,
                completion_date = x.completion_date ?? DateTime.MinValue
            });

        // 取得總筆數（分頁用）
        int totalCount = await projected.CountAsync();

        // 依分頁參數取出資料，並依 record_id 由新到舊排序
        var data = await projected
            .OrderByDescending(x => x.record_id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 回傳分頁資料與總筆數
        return (data, totalCount);
    }

    /// <summary>
    /// 依ID查詢單筆維護紀錄
    /// </summary>
    /// <param name="id">維護紀錄ID</param>
    /// <returns>維護紀錄物件或null</returns>
    public async Task<MaintainRecord?> GetByIdAsync(int id)
    {
        return await m_db.MaintainRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.record_id == id);
    }

    /// <summary>
    /// 新增維護紀錄
    /// </summary>
    /// <param name="model">維護紀錄物件</param>
    /// <returns>新增後的ID</returns>
    public async Task<int> CreateAsync(MaintainRecord model)
    {
        await m_db.MaintainRecords.AddAsync(model);
        await m_db.SaveChangesAsync();
        return model.record_id ?? 0;
    }

    /// <summary>
    /// 更新維護紀錄
    /// </summary>
    /// <param name="model">維護紀錄物件</param>
    /// <returns>受影響筆數</returns>
    public async Task<int> UpdateAsync(MaintainRecord model)
    {
        model.called_firm ??= "";
        model.serial_no ??= "";
        m_db.MaintainRecords.Update(model);
        return await m_db.SaveChangesAsync();
    }

    /// <summary>
    /// 刪除維護紀錄
    /// </summary>
    /// <param name="record_id">維護紀錄ID</param>
    /// <returns>受影響筆數</returns>
    public async Task<int> DeleteAsync(int record_id)
    {
        var entity = await m_db.MaintainRecords.FindAsync(record_id);
        if (entity == null) return 0;
        m_db.MaintainRecords.Remove(entity);
        return await m_db.SaveChangesAsync();
    }
}