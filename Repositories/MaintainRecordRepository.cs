using myWebApp.Models;
using Microsoft.Data.SqlClient;

public class MaintainRecordRepository : IMaintainRecordRepository
{
    private readonly string _connStr;
    public MaintainRecordRepository(string connStr) { _connStr = connStr; }

    public async Task<List<SearchResultViewModel>> SearchAsync(SearchConditionViewModel model)
    {
        var results = new List<SearchResultViewModel>();
      
        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT 
                    A.record_id as record_id,
                    A.apply_date as apply_date,
                    B.depart_name as depart_name,
                    C.staff_name as staff_name,
                    D.description as problem_type,
                    E.description as processing_type,
                    F.staff_name as processing_staff_name,
                    A.completion_date as completion_date
                FROM 
                    ism_maintain_record A LEFT JOIN 
                    ism_department B ON A.depart_code = B.depart_code LEFT JOIN 
                    ism_staff C ON A.staff_id = C.staff_id LEFT JOIN 
                    ism_code D ON A.problem_type = D.code AND D.kind = 'QUESTION' LEFT JOIN 
                    ism_code E ON A.processing_type = E.code AND E.kind = 'PROCESSING' LEFT JOIN 
                    ism_staff F ON A.processing_staff_id = F.staff_id
                WHERE 
                    (@applyStartDate IS NULL OR A.apply_date >= @applyStartDate) AND 
                    (@applyEndDate IS NULL OR A.apply_date <= @applyEndDate) AND 
                    (@completionStartDate IS NULL OR A.completion_date >= @completionStartDate) AND 
                    (@completionEndDate IS NULL OR A.completion_date <= @completionEndDate) AND 
                    (@depart_code IS NULL OR A.depart_code = @depart_code) AND 
                    (@problem_type IS NULL OR A.problem_type = @problem_type) AND 
                    (@processing_staff_id IS NULL OR A.processing_staff_id = @processing_staff_id) AND 
                    (@processing_type IS NULL OR A.processing_type = @processing_type)";

            cmd.Parameters.AddWithValue("@applyStartDate", (object?)model.ApplyStartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@applyEndDate", (object?)model.ApplyEndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@completionStartDate", (object?)model.CompletionStartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@completionEndDate", (object?)model.CompletionEndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@depart_code", (object?)model.depart_code ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@problem_type", (object?)model.problem_type ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@processing_staff_id", (object?)model.processing_staff_id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@processing_type", (object?)model.processing_type ?? DBNull.Value);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    results.Add(new SearchResultViewModel
                    {
                        record_id = (int)reader["record_id"],//序號
                        apply_date = (DateTime)reader["apply_date"],//申報日期
                        depart_name = reader["depart_name"].ToString(),//使用單位
                        staff_name = reader["staff_name"].ToString(),//使用者
                        problem_type = reader["problem_type"].ToString(),//問題類別
                        processing_type = reader["processing_type"].ToString(),//處理類別
                        processing_staff_name = reader["processing_staff_name"].ToString(),//處理人員
                        completion_date = (DateTime)reader["completion_date"]//完成日期
                    });
                }
            }
        }
        return results;
    }

    public async Task<MaintainRecordViewModel?> GetByIdAsync(int id)
    {
        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();

              cmd.CommandText = @"SELECT * FROM ism_maintain_record WHERE record_id = @id";
                cmd.Parameters.AddWithValue("@id", id);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new MaintainRecordViewModel
                    {
                        record_id = reader["record_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["record_id"]),
                        apply_date = reader["apply_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["apply_date"]),
                        serial_no = reader["serial_no"] == DBNull.Value ? null : reader["serial_no"].ToString(),
                        depart_code = reader["depart_code"] == DBNull.Value ? null : reader["depart_code"].ToString(),
                        staff_id = reader["staff_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["staff_id"]),
                        tel = reader["tel"] == DBNull.Value ? null : reader["tel"].ToString(),
                        problem_type = reader["problem_type"] == DBNull.Value ? null : reader["problem_type"].ToString(),
                        record_staff_id = reader["record_staff_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["record_staff_id"]),
                        processing_staff_id = reader["processing_staff_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["processing_staff_id"]),
                        processing_type = reader["processing_type"] == DBNull.Value ? null : reader["processing_type"].ToString(),
                        description = reader["description"] == DBNull.Value ? null : reader["description"].ToString(),
                        solution = reader["solution"] == DBNull.Value ? null : reader["solution"].ToString(),
                        called_firm = reader["called_firm"] == DBNull.Value ? null : reader["called_firm"].ToString(),
                        completion_date = reader["completion_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["completion_date"]),
                        processing_minutes = reader["processing_minutes"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["processing_minutes"]),
                        update_user_id = reader["update_user_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["update_user_id"]),
                        update_date = reader["update_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["update_date"]),
                        satisfaction = reader["satisfaction"] == DBNull.Value ? null : reader["satisfaction"].ToString(),
                        recommendation = reader["recommendation"] == DBNull.Value ? null : reader["recommendation"].ToString(),
                        satisfaction_update_user_id = reader["satisfaction_update_user_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["satisfaction_update_user_id"]),
                        satisfaction_update_date = reader["satisfaction_update_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["satisfaction_update_date"])
                    };
                }
            }
        }

        return null;
    }

    public async Task<int> CreateAsync(MaintainRecordViewModel model)
    {
        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
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

    public async Task<int> UpdateAsync(MaintainRecordViewModel model)
    {
        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
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
        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM ism_maintain_record WHERE record_id = @record_id";
            cmd.Parameters.AddWithValue("@record_id", record_id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows;
        }
    }
}