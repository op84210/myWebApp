using myWebApp.Models;
using Microsoft.EntityFrameworkCore;

public class MaintainRecordRepository : IMaintainRecordRepository
{
    private readonly MyDbContext m_db;
    public MaintainRecordRepository(MyDbContext db)
    {
        m_db = db;
    }
    public async Task<(List<SearchResult>, int)> SearchPagedAsync(SearchCondition model, int page, int pageSize)
    {
        var query = m_db.MaintainRecords
            .AsNoTracking()
            .Where(x => (x.ProblemTypeCode == null || x.ProblemTypeCode.kind == "QUESTION") &&
                        (x.ProcessingTypeCode == null || x.ProcessingTypeCode.kind == "PROCESSING"));

        if (model.ApplyStartDate != null)
            query = query.Where(x => x.apply_date >= model.ApplyStartDate);
        if (model.ApplyEndDate != null)
            query = query.Where(x => x.apply_date <= model.ApplyEndDate);
        if (!string.IsNullOrEmpty(model.depart_code))
            query = query.Where(x => x.depart_code == model.depart_code);
        if (!string.IsNullOrEmpty(model.problem_type))
            query = query.Where(x => x.problem_type == model.problem_type);
        if (model.processing_staff_id != null)
            query = query.Where(x => x.processing_staff_id == model.processing_staff_id);
        if (!string.IsNullOrEmpty(model.processing_type))
            query = query.Where(x => x.processing_type == model.processing_type);
        if (model.CompletionStartDate != null)
            query = query.Where(x => x.completion_date >= model.CompletionStartDate);
        if (model.CompletionEndDate != null)
            query = query.Where(x => x.completion_date <= model.CompletionEndDate);

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

        int totalCount = await projected.CountAsync();

        var data = await projected
            .OrderByDescending(x => x.record_id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task<MaintainRecord?> GetByIdAsync(int id)
    {
        return await m_db.MaintainRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.record_id == id);
    }

    public async Task<int> CreateAsync(MaintainRecord model)
    {
        await m_db.MaintainRecords.AddAsync(model);
        await m_db.SaveChangesAsync();
        return model.record_id ?? 0;
    }

    public async Task<int> UpdateAsync(MaintainRecord model)
    {
        model.called_firm ??= "";
        model.serial_no ??= "";
        m_db.MaintainRecords.Update(model);
        return await m_db.SaveChangesAsync();
    }
    
    public async Task<int> DeleteAsync(int record_id)
    {
        var entity = await m_db.MaintainRecords.FindAsync(record_id);
        if (entity == null) return 0;
        m_db.MaintainRecords.Remove(entity);
        return await m_db.SaveChangesAsync();
    }
}