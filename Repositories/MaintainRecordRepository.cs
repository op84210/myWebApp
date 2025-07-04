using myWebApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

public class MaintainRecordRepository : IMaintainRecordRepository
{
    private readonly MyDbContext _db;
    private readonly string m_strConnectionString;
    public MaintainRecordRepository(IConfiguration config,MyDbContext db)
    {
          m_strConnectionString = config.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _db = db;
    }
    public async Task<(List<SearchResultViewModel>, int)> SearchPagedAsync(SearchConditionViewModel model, int page, int pageSize)
    {
        var query = _db.MaintainRecords
            .Include(m => m.Department)
            .Include(m => m.Staff)
            .Include(m => m.ProblemTypeCode)
                .Where(m => m.ProblemTypeCode != null && m.ProblemTypeCode.kind == "QUESTION")
            .Include(m => m.ProcessingTypeCode)
                .Where(m => m.ProcessingTypeCode != null && m.ProcessingTypeCode.kind == "PROCESSING")
            .Include(m => m.ProcessingStaff)
            .AsQueryable();

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

        int totalCount = await query.CountAsync();

        var data = await query
            .OrderByDescending(x => x.record_id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SearchResultViewModel
            {
                record_id = x.record_id ?? 0,
                apply_date = x.apply_date ?? DateTime.MinValue,
                depart_name = x.Department != null ? x.Department.depart_name : null,
                staff_name = x.Staff != null ? x.Staff.staff_name : null,
                problem_type = x.ProblemTypeCode != null ? x.ProblemTypeCode.description : null,
                processing_type = x.ProcessingTypeCode != null ? x.ProcessingTypeCode.description : null,
                processing_staff_name = x.ProcessingStaff != null ? x.ProcessingStaff.staff_name : null,
                completion_date = x.completion_date ?? DateTime.MinValue
            })
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task<MaintainRecord?> GetByIdAsync(int id)
    {
        using (var cn = new SqlConnection(m_strConnectionString))
        {
            await cn.OpenAsync();
            var cmd = cn.CreateCommand();

              cmd.CommandText = @"SELECT * FROM ism_maintain_record WHERE record_id = @id";
                cmd.Parameters.AddWithValue("@id", id);

            using (var dr = await cmd.ExecuteReaderAsync())
            {
                if (await dr.ReadAsync())
                {
                    return new MaintainRecord
                    {
                        record_id = dr["record_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["record_id"]),
                        apply_date = dr["apply_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["apply_date"]),
                        serial_no = dr["serial_no"] == DBNull.Value ? null : dr["serial_no"].ToString(),
                        depart_code = dr["depart_code"] == DBNull.Value ? null : dr["depart_code"].ToString(),
                        staff_id = dr["staff_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["staff_id"]),
                        tel = dr["tel"] == DBNull.Value ? null : dr["tel"].ToString(),
                        problem_type = dr["problem_type"] == DBNull.Value ? null : dr["problem_type"].ToString(),
                        record_staff_id = dr["record_staff_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["record_staff_id"]),
                        processing_staff_id = dr["processing_staff_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["processing_staff_id"]),
                        processing_type = dr["processing_type"] == DBNull.Value ? null : dr["processing_type"].ToString(),
                        description = dr["description"] == DBNull.Value ? null : dr["description"].ToString(),
                        solution = dr["solution"] == DBNull.Value ? null : dr["solution"].ToString(),
                        called_firm = dr["called_firm"] == DBNull.Value ? null : dr["called_firm"].ToString(),
                        completion_date = dr["completion_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["completion_date"]),
                        processing_minutes = dr["processing_minutes"] == DBNull.Value ? null : Convert.ToInt16(dr["processing_minutes"]),
                        update_user_id = dr["update_user_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["update_user_id"]),
                        update_date = dr["update_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["update_date"]),
                        satisfaction = dr["satisfaction"] == DBNull.Value ? null : dr["satisfaction"].ToString(),
                        recommendation = dr["recommendation"] == DBNull.Value ? null : dr["recommendation"].ToString(),
                        satisfaction_update_user_id = dr["satisfaction_update_user_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["satisfaction_update_user_id"]),
                        satisfaction_update_date = dr["satisfaction_update_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["satisfaction_update_date"])
                    };
                }
            }
        }

        return null;
    }

    public async Task<int> CreateAsync(MaintainRecord model)
    {
        using (var cn = new SqlConnection(m_strConnectionString))
        {
            await cn.OpenAsync();
            var cmd = cn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO ism_maintain_record
                (apply_date, serial_no, depart_code, staff_id, tel, problem_type, record_staff_id, processing_staff_id, processing_type, description, solution, called_firm, completion_date, processing_minutes, update_user_id, update_date, satisfaction, recommendation, satisfaction_update_user_id, satisfaction_update_date)
                VALUES
                (@apply_date, @serial_no, @depart_code, @staff_id, @tel, @problem_type, @record_staff_id, @processing_staff_id, @processing_type, @description, @solution, @called_firm, @completion_date, @processing_minutes, @update_user_id, @update_date, @satisfaction, @recommendation, @satisfaction_update_user_id, @satisfaction_update_date)
            ";

            cmd.Parameters.AddWithValue("@apply_date", (object?)model.apply_date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@serial_no", (object?)model.serial_no ?? "");
            cmd.Parameters.AddWithValue("@depart_code", (object?)model.depart_code ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@staff_id", (object?)model.staff_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tel", (object?)model.tel ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@problem_type", (object?)model.problem_type ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@record_staff_id", (object?)model.record_staff_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@processing_staff_id", (object?)model.processing_staff_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@processing_type", (object?)model.processing_type ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@description", (object?)model.description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@solution", (object?)model.solution ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@called_firm", (object?)model.called_firm ?? "");
            cmd.Parameters.AddWithValue("@completion_date", (object?)model.completion_date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@processing_minutes", (object?)model.processing_minutes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@update_user_id", (object?)model.update_user_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@update_date", (object?)model.update_date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@satisfaction", (object?)model.satisfaction ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@recommendation", (object?)model.recommendation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@satisfaction_update_user_id", (object?)model.satisfaction_update_user_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@satisfaction_update_date", (object?)model.satisfaction_update_date ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }

    public async Task<int> UpdateAsync(MaintainRecord model)
    {
        using (var cn = new SqlConnection(m_strConnectionString))
        {
            await cn.OpenAsync();
            var cmd = cn.CreateCommand();
             cmd.CommandText = @"
                UPDATE ism_maintain_record SET
                    apply_date = @apply_date,
                    serial_no = @serial_no,
                    depart_code = @depart_code,
                    staff_id = @staff_id,
                    tel = @tel,
                    problem_type = @problem_type,
                    record_staff_id = @record_staff_id,
                    processing_staff_id = @processing_staff_id,
                    processing_type = @processing_type,
                    description = @description,
                    solution = @solution,
                    called_firm = @called_firm,
                    completion_date = @completion_date,
                    processing_minutes = @processing_minutes,
                    update_user_id = @update_user_id,
                    update_date = @update_date,
                    satisfaction = @satisfaction,
                    recommendation = @recommendation,
                    satisfaction_update_user_id = @satisfaction_update_user_id,
                    satisfaction_update_date = @satisfaction_update_date
                WHERE record_id = @record_id
            ";

            cmd.Parameters.AddWithValue("@apply_date", (object?)model.apply_date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@serial_no", (object?)model.serial_no ?? "");
            cmd.Parameters.AddWithValue("@depart_code", (object?)model.depart_code ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@staff_id", (object?)model.staff_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tel", (object?)model.tel ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@problem_type", (object?)model.problem_type ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@record_staff_id", (object?)model.record_staff_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@processing_staff_id", (object?)model.processing_staff_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@processing_type", (object?)model.processing_type ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@description", (object?)model.description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@solution", (object?)model.solution ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@called_firm", (object?)model.called_firm ?? "");
            cmd.Parameters.AddWithValue("@completion_date", (object?)model.completion_date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@processing_minutes", (object?)model.processing_minutes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@update_user_id", (object?)model.update_user_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@update_date", (object?)model.update_date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@satisfaction", (object?)model.satisfaction ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@recommendation", (object?)model.recommendation ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@satisfaction_update_user_id", (object?)model.satisfaction_update_user_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@satisfaction_update_date", (object?)model.satisfaction_update_date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@record_id", (object?)model.record_id ?? DBNull.Value);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows;
        }
    }
    
    public async Task<int> DeleteAsync(int record_id)
    {
        using (var cn = new SqlConnection(m_strConnectionString))
        {
            await cn.OpenAsync();

            var cmd = cn.CreateCommand();
            cmd.CommandText = "DELETE FROM ism_maintain_record WHERE record_id = @record_id";
            cmd.Parameters.AddWithValue("@record_id", record_id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows;
        }
    }
}