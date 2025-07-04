using myWebApp.Models;

public interface IMaintainRecordRepository
{
    Task<(List<SearchResultViewModel>,int)> SearchPagedAsync(SearchConditionViewModel model, int page, int pageSize);
    Task<MaintainRecord?> GetByIdAsync(int intId);
    Task<int> CreateAsync(MaintainRecord model);
    Task<int> UpdateAsync(MaintainRecord model);
    Task<int> DeleteAsync(int intId);
}