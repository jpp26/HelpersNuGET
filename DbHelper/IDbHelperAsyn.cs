using System.Data;
using System.Threading.Tasks;

namespace DbHelper
{
    public interface IDbHelperAsync
    {
        Task<IDbConnection?> GetOpenConnectionAsync();
        string ErrorMessage { get; }
        string ConnectionString { get; }
        bool IsConnected { get; }
        int CommandTimeoutProp { get; }
        int ConnectionTimeoutProp { get; }
        int RetryCountProp { get; }
        int RetryDelayMsProp { get; }
        int MaxPoolSizeProp { get; }
        int MinPoolSizeProp { get; }
        bool PoolingEnabled { get; }
    }
}
