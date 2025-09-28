using System.Data;
using System.Threading.Tasks;

namespace DbHelper
{
    public interface IDbHelperAsync
    {
        Task<IDbConnection?> GetOpenConnectionAsync();
        string ErrorMessage { get; }
        string ConnectionString { get; }
        int CommandTimeout { get; }
        int ConnectionTimeout { get; }
        int RetryCount { get; }
        int RetryDelayMs { get; }
        int MaxPoolSize { get; }
        int MinPoolSize { get; }
        bool PoolingEnabled { get; }
    }
}
