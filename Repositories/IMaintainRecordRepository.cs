using myWebApp.Models;

public interface IMaintainRecordRepository
{
    Task<List<SearchResultViewModel>> SearchAsync(SearchConditionViewModel model);
    Task<MaintainRecordViewModel?> GetByIdAsync(int intId);
    Task<int> CreateAsync(MaintainRecordViewModel model);
    Task<int> UpdateAsync(MaintainRecordViewModel model);
    Task<int> DeleteAsync(int intId);
}