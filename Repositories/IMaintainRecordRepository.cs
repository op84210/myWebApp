using myWebApp.Models;

public interface IMaintainRecordRepository
{
    Task<(List<SearchResultViewModel>,int)> SearchPagedAsync(SearchConditionViewModel model, int page, int pageSize);
    Task<MaintainRecordViewModel?> GetByIdAsync(int intId);
    Task<int> CreateAsync(MaintainRecordViewModel model);
    Task<int> UpdateAsync(MaintainRecordViewModel model);
    Task<int> DeleteAsync(int intId);
}