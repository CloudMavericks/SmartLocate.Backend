namespace SmartLocate.Infrastructure.Commons.Contracts;

public class ResultSet<T>(IEnumerable<T> items, long totalCount) where T : class
{
    public List<T> Items { get; set; } = items.ToList();
    
    public long TotalCount { get; set; } = totalCount;
}