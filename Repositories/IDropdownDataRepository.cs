using Microsoft.AspNetCore.Mvc.Rendering;

public interface IDropdownDataRepository
{
    Task<List<SelectListItem>> GetDepartmentsAsync();
    Task<List<SelectListItem>> GetProblemTypesAsync();
    Task<List<SelectListItem>> GetProcessingStaffIdsAsync();
    Task<List<SelectListItem>> GetProcessingTypesAsync();
    Task<List<SelectListItem>> GetStaffsAsync();
}