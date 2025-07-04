using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class DropdownDataRepository : IDropdownDataRepository
{
    private readonly MyDbContext m_db;
    public DropdownDataRepository(MyDbContext db)
    {
        m_db = db;
    }
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