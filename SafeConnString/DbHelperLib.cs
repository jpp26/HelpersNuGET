using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System;

namespace DbHelper
{
    public class DbHelperLibAsync : IDisposable, IDbHelperAsync
    {
        #region Configuración interna
        private const int CommandTimeout = 30;
        private const int ConnectionTimeout = 15;
        private const int RetryCount = 3;
        private const int RetryDelayMs = 2000;
        private const int MaxPoolSize = 100;
        private const int MinPoolSize = 5;
        private const bool EnablePooling = true;
        #endregion
        #region Estado interno
        private string _connectionString;
        private SqlConnection? _sqlConnection;
        private string _errorMessage = string.Empty;
        private bool _disposed = false;
        #endregion
        #region Constructor con inyección de cadena de conexión
        public DbHelperLibAsync(string connection)
        {
            _connectionString = connection?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                _errorMessage = "Connection string cannot be empty.";
                return;
            }

            try
            {
                var builder = new SqlConnectionStringBuilder(_connectionString)
                {
                    ConnectTimeout = ConnectionTimeout,
                    MaxPoolSize = MaxPoolSize,
                    MinPoolSize = MinPoolSize,
                    Pooling = EnablePooling
                };

                _connectionString = builder.ConnectionString;
                _sqlConnection = new SqlConnection(_connectionString);
            }
            catch (Exception ex)
            {
                _errorMessage = $"Connection string build error: {ex.Message}";
                _sqlConnection = null;
            }
        }
        #endregion
        #region Método para obtener la conexión abierta con lógica de reconexión
        public async Task<IDbConnection?> GetOpenConnectionAsync()
        {
            if (_sqlConnection == null)
            {
                _errorMessage = "Connection is not initialized.";
                return null;
            }

            if (_sqlConnection.State == ConnectionState.Broken || _sqlConnection.State == ConnectionState.Closed)
            {
                _sqlConnection.Dispose();
                _sqlConnection = new SqlConnection(_connectionString);
            }

            int attempts = 0;
            while (attempts < RetryCount)
            {
                try
                {
                    if (_sqlConnection.State != ConnectionState.Open)
                    {
                        await _sqlConnection.OpenAsync();
                    }
                    return _sqlConnection;
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    _errorMessage = $"SQL Error: {sqlEx.Message} (Code: {sqlEx.Number}). Attempt {attempts} of {RetryCount}.";
                    await Task.Delay(RetryDelayMs);
                }
                catch (Exception ex)
                {
                    _errorMessage = $"General error: {ex.Message}";
                    return null;
                }
            }

            _errorMessage = "Failed to open connection after multiple attempts.";
            return null;
        }
        #endregion
        #region Método para cerrar la conexión manualmente
        public void CloseConnection()
        {
            if (_sqlConnection != null && _sqlConnection.State != ConnectionState.Closed)
            {
                _sqlConnection.Close();
            }
        }
        #endregion
        #region Implementación segura de IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                CloseConnection();
                _sqlConnection?.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DbHelperLibAsync()
        {
            Dispose(false);
        }
        #endregion
        #region Propiedades públicas para diagnóstico y configuración
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }
        public string ConnectionString
        {
            get { return _connectionString; }
        }
        public bool IsConnected
        {
            get
            {
                return _sqlConnection != null && _sqlConnection.State == ConnectionState.Open;
            }
        }
        public int CommandTimeoutProp
        {
            get { return CommandTimeout; }
        }
        public int ConnectionTimeoutProp
        {
            get { return ConnectionTimeout; }
        }
        public int RetryCountProp
        {
            get { return RetryCount; }
        }
        public int RetryDelayMsProp
        {
            get { return RetryDelayMs; }
        }
        public int MaxPoolSizeProp
        {
            get { return MaxPoolSize; }
        }
        public int MinPoolSizeProp
        {
            get { return MinPoolSize; }
        }
        public bool PoolingEnabled
        {
            get { return EnablePooling; }
        }
        #endregion
    }
}
