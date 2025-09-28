using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DbHelper
{
    public class DbHelperLibAsync : IDisposable, IDbHelperAsync
    {
        #region Configuración interna
        private const int CommandTimeoutConst = 30;
        private const int ConnectionTimeoutConst = 30;
        private const int RetryCountConst = 3;
        private const int RetryDelayMsConst = 500;
        private const int MaxPoolSizeConst = 100;
        private const int MinPoolSizeConst = 5;
        private const bool EnablePoolingConst = true;
        private const bool EnableMarsConst = true;
        private const int LatenciaUmbralMs = 500;
        #endregion
        #region Estado interno
        private string _connectionString;
        private string _errorMessage = string.Empty;
        private bool _disposed = false;
        private long _ultimoTiempoConexionMs = -1;
        private string _ultimoErrorTecnico = string.Empty;
        #endregion
        #region Ruta fija para log
        private static readonly string LogFilePath = @"C:\ERP_DDPOS\ERPDDPOS\ERPDDPOS\ERPDDPOS\bin\x64\Debug\net8.0-windows10.0.19041.0\dbhelper.log";
        #endregion
        #region Constructor con validación y configuración
        public DbHelperLibAsync(string connection)
        {
            _connectionString = connection?.Trim() ?? string.Empty;

            if (!IsValidConnectionString(_connectionString))
            {
                _errorMessage = "Cadena de conexión inválida o vacía.";
                _connectionString = string.Empty;
                LogToFile(_errorMessage);
                return;
            }

            try
            {
                var builder = new SqlConnectionStringBuilder(_connectionString)
                {
                    ConnectTimeout = ConnectionTimeoutConst,
                    MaxPoolSize = MaxPoolSizeConst,
                    MinPoolSize = MinPoolSizeConst,
                    Pooling = EnablePoolingConst,
                    MultipleActiveResultSets = EnableMarsConst
                };

                _connectionString = builder.ConnectionString;
            }
            catch (Exception ex)
            {
                _errorMessage = $"Error al construir la cadena de conexión: {ex.Message}";
                _connectionString = string.Empty;
                LogToFile(_errorMessage);
            }
        }
        #endregion
        #region Validación de cadena de conexión
        private bool IsValidConnectionString(string conn)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(conn);
                return !string.IsNullOrWhiteSpace(builder.DataSource) &&
                       !string.IsNullOrWhiteSpace(builder.InitialCatalog);
            }
            catch
            {
                return false;
            }
        }
        #endregion
        #region Método para obtener la conexión abierta con trazabilidad
        public async Task<IDbConnection?> GetOpenConnectionAsync()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                _errorMessage = "Cadena de conexión no inicializada.";
                LogToFile(_errorMessage);
                return null;
            }

            int attempts = 0;
            Exception? lastError = null;

            while (attempts < RetryCountConst)
            {
                try
                {
                    var sw = Stopwatch.StartNew();
                    var connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    sw.Stop();

                    _ultimoTiempoConexionMs = sw.ElapsedMilliseconds;

                    if (_ultimoTiempoConexionMs > LatenciaUmbralMs)
                    {
                        LogToFile($"⏱️ Apertura lenta: {_ultimoTiempoConexionMs} ms");
                    }
                    else
                    {
                        LogToFile($"✅ Conexión abierta en {_ultimoTiempoConexionMs} ms");
                    }

                    return connection;
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    _ultimoErrorTecnico = $"SQL Error {sqlEx.Number}: {sqlEx.Message}\n{sqlEx.StackTrace}";
                    _errorMessage = $"Error SQL: {sqlEx.Message} (Código: {sqlEx.Number}). Intento {attempts} de {RetryCountConst}.";
                    lastError = sqlEx;
                    LogToFile(_errorMessage);
                    await Task.Delay(RetryDelayMsConst);
                }
                catch (Exception ex)
                {
                    _ultimoErrorTecnico = $"General Error: {ex.Message}\n{ex.StackTrace}";
                    _errorMessage = $"Error general: {ex.Message}";
                    lastError = ex;
                    LogToFile(_errorMessage);
                    break;
                }
            }

            _errorMessage = "❌ No se pudo abrir la conexión tras múltiples intentos.";
            LogToFile(_errorMessage);
            return null;
        }
        #endregion
        #region Método para cerrar la conexión manualmente
        public void CloseConnection(IDbConnection? connection)
        {
            if (connection != null && connection.State != ConnectionState.Closed)
            {
                connection.Close();
                connection.Dispose();
            }
        }
        #endregion
        #region Implementación segura de IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~DbHelperLibAsync()
        {
            Dispose(false);
        }
        #endregion
        #region Propiedades públicas para diagnóstico y configuración
        public string ErrorMessage => _errorMessage;
        public string ConnectionString => _connectionString;
        public int CommandTimeout => CommandTimeoutConst;
        public int ConnectionTimeout => ConnectionTimeoutConst;
        public int RetryCount => RetryCountConst;
        public int RetryDelayMs => RetryDelayMsConst;
        public int MaxPoolSize => MaxPoolSizeConst;
        public int MinPoolSize => MinPoolSizeConst;
        public bool PoolingEnabled => EnablePoolingConst;
        public long UltimoTiempoConexionMs => _ultimoTiempoConexionMs;
        public string UltimoErrorTecnico => _ultimoErrorTecnico;
        #endregion
        #region Diagnóstico en caliente
        public string DiagnosticoActual()
        {
            return $@"
🔍 Diagnóstico de conexión:
- Tiempo última conexión: {_ultimoTiempoConexionMs} ms
- Último error técnico: {_ultimoErrorTecnico}
- Cadena activa: {_connectionString}
- Pooling: {(EnablePoolingConst ? "Activado" : "Desactivado")}
- MARS: {(EnableMarsConst ? "Activado" : "Desactivado")}
- Timeout conexión: {ConnectionTimeoutConst}s
- Timeout comandos: {CommandTimeoutConst}s
";
        }
        #endregion
        #region Registro en archivo de texto
        private void LogToFile(string mensaje)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath)!);
                var log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {mensaje}\n";
                File.AppendAllText(LogFilePath, log);
            }
            catch
            {
                // Silenciar errores de log para no romper flujo principal
            }
        }
        #endregion
    }
}
