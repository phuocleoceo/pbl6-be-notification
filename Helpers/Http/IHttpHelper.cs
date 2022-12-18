namespace Monolithic.Helpers;

public interface IHttpHelper<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(string url, string token);

    Task<T> GetAsync(string url, string token);

    Task<bool> CreateAsync(string url, T obj, string token);

    Task<bool> UpdateAsync(string url, T obj, string token);

    Task<bool> DeleteAsync(string url, string token);
}