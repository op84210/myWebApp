using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;

public class DropdownDataRepository : IDropdownDataRepository
{
    private readonly string m_strConnectionString;
    public DropdownDataRepository(IConfiguration config)
    {
        m_strConnectionString = config.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }
    public async Task<List<SelectListItem>> GetStaffsByDepartmentAsync(string strDepartCode)
    {
        var list = new List<SelectListItem>();
        using (var cn = new SqlConnection(m_strConnectionString))
        {
            await cn.OpenAsync();
            var cmd = cn.CreateCommand();
            cmd.CommandText = @"SELECT staff_id, staff_name FROM ism_staff WHERE depart_code = @depart_code ORDER BY staff_name";
            cmd.Parameters.AddWithValue("@depart_code", strDepartCode);
            using (var dr = await cmd.ExecuteReaderAsync())
            {
                while (await dr.ReadAsync())
                {
                    list.Add(new SelectListItem
                    {
                        Value = dr["staff_id"].ToString(),
                        Text = dr["staff_name"].ToString()
                    });
                }
            }
        }
        return list;
    }
    public async Task<List<SelectListItem>> GetDepartmentsAsync()
    {
        var list = new List<SelectListItem>();
        using (var cn = new SqlConnection(m_strConnectionString))
        {
            await cn.OpenAsync();
            var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT depart_code, depart_name FROM ism_department ORDER BY depart_name";
            using (var dr = await cmd.ExecuteReaderAsync())
            {
                while (await dr.ReadAsync())
                {
                    list.Add(new SelectListItem
                    {
                        Value = dr["depart_code"].ToString(),
                        Text = dr["depart_name"].ToString()
                    });
                }
            }
        }
        return list;
    }

    public async Task<List<SelectListItem>> GetProblemTypesAsync()
    {
        var list = new List<SelectListItem>();
        using (var cn = new SqlConnection(m_strConnectionString))
        {
            await cn.OpenAsync();
            var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT code, description FROM ism_code WHERE kind = 'QUESTION' ORDER BY description";
            using (var dr = await cmd.ExecuteReaderAsync())
            {
                while (await dr.ReadAsync())
                {
                    list.Add(new SelectListItem
                    {
                        Value = dr["code"].ToString(),
                        Text = dr["description"].ToString()
                    });
                }
            }
        }
        return list;
    }

    public async Task<List<SelectListItem>> GetProcessingStaffIdsAsync()
    {
        var list = new List<SelectListItem>();
        using (var cn = new SqlConnection(m_strConnectionString))
        {
            await cn.OpenAsync();
            var cmd = cn.CreateCommand();
            cmd.CommandText = @"SELECT staff_id, staff_name FROM ism_staff
                                WHERE EXISTS (
                                    SELECT 1 FROM ism_maintain_record
                                    WHERE processing_staff_id = ism_staff.staff_id
                                )
                                ORDER BY staff_name";
            using (var dr = await cmd.ExecuteReaderAsync())
            {
                while (await dr.ReadAsync())
                {
                    list.Add(new SelectListItem
                    {
                        Value = dr["staff_id"].ToString(),
                        Text = dr["staff_name"].ToString()
                    });
                }
            }
        }
        return list;
    }

    public async Task<List<SelectListItem>> GetProcessingTypesAsync()
    {
        var list = new List<SelectListItem>();
        using (var cn = new SqlConnection(m_strConnectionString))
        {
            await cn.OpenAsync();
            var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT code, description FROM ism_code WHERE kind = 'PROCESSING' ORDER BY code";
            using (var dr = await cmd.ExecuteReaderAsync())
            {
                while (await dr.ReadAsync())
                {
                    list.Add(new SelectListItem
                    {
                        Value = dr["code"].ToString(),
                        Text = dr["description"].ToString()
                    });
                }
            }
        }
        return list;
    }

    public async Task<List<SelectListItem>> GetStaffsAsync()
    {
        var list = new List<SelectListItem>();
        using (var cn = new SqlConnection(m_strConnectionString))
        {
            await cn.OpenAsync();
            var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT staff_id, staff_name FROM ism_staff ORDER BY staff_name";
            using (var dr = await cmd.ExecuteReaderAsync())
            {
                while (await dr.ReadAsync())
                {
                    list.Add(new SelectListItem
                    {
                        Value = dr["staff_id"].ToString(),
                        Text = dr["staff_name"].ToString()
                    });
                }
            }
        }
        return list;
    }
}