using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;

public class DropdownDataRepository : IDropdownDataRepository
{
    private readonly string _connStr;
    public DropdownDataRepository(string connStr) { _connStr = connStr; }

    public async Task<List<SelectListItem>> GetDepartmentsAsync()
    {
        var list = new List<SelectListItem>();
        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT depart_code, depart_name FROM ism_department ORDER BY depart_name";
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new SelectListItem
                    {
                        Value = reader["depart_code"].ToString(),
                        Text = reader["depart_name"].ToString()
                    });
                }
            }
        }
        return list;
    }

    public async Task<List<SelectListItem>> GetProblemTypesAsync()
    {
        var list = new List<SelectListItem>();
        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT code, description FROM ism_code WHERE kind = 'QUESTION' ORDER BY description";
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new SelectListItem
                    {
                        Value = reader["code"].ToString(),
                        Text = reader["description"].ToString()
                    });
                }
            }
        }
        return list;
    }

    public async Task<List<SelectListItem>> GetProcessingStaffIdsAsync()
    {
        var list = new List<SelectListItem>();
        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT staff_id, staff_name FROM ism_staff
                                WHERE EXISTS (
                                    SELECT 1 FROM ism_maintain_record
                                    WHERE processing_staff_id = ism_staff.staff_id
                                )
                                ORDER BY staff_name";
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new SelectListItem
                    {
                        Value = reader["staff_id"].ToString(),
                        Text = reader["staff_name"].ToString()
                    });
                }
            }
        }
        return list;
    }

    public async Task<List<SelectListItem>> GetProcessingTypesAsync()
    {
        var list = new List<SelectListItem>();
        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT code, description FROM ism_code WHERE kind = 'PROCESSING' ORDER BY code";
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new SelectListItem
                    {
                        Value = reader["code"].ToString(),
                        Text = reader["description"].ToString()
                    });
                }
            }
        }
        return list;
    }

    public async Task<List<SelectListItem>> GetStaffsAsync()
    {
        var list = new List<SelectListItem>();
        using (var conn = new SqlConnection(_connStr))
        {
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT staff_id, staff_name FROM ism_staff ORDER BY staff_name";
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new SelectListItem
                    {
                        Value = reader["staff_id"].ToString(),
                        Text = reader["staff_name"].ToString()
                    });
                }
            }
        }
        return list;
    }
}