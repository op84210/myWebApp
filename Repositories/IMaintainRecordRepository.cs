using myWebApp.Models;

public interface IMaintainRecordRepository
{
    Task<List<SearchResultViewModel>> SearchAsync(SearchConditionViewModel model);
    Task<MaintainRecordViewModel?> GetByIdAsync(int id);
    Task<int> CreateAsync(MaintainRecordViewModel model);
    Task<int> UpdateAsync(MaintainRecordViewModel model);
    Task<int> DeleteAsync(int id);
}