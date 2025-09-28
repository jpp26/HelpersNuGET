using DbHelper;
using FileHelper;
using Security.JPP;
using System;
using System.IO;
using System.Text.Json;
namespace CryptoHelperLib
{
    /// <summary>
    /// Factory técnico para obtener una instancia blindada de conexión SQL.
    /// Integra desencriptado desde XML o JSON y devuelve IDbHelperAsync desacoplado.
    /// </summary>
    public static class ConexionFactory
    {
        #region 🔐 Obtener conexión desde XML

        /// <summary>
        /// Devuelve una instancia de DbHelperLibAsync usando cadena desencriptada desde ConnectionString.xml.
        /// </summary>
        public static IDbHelperAsync GetConexion()
        {
            string cadena = FileHelperLib.DesencriptarCadenaConexion();

            if (string.IsNullOrWhiteSpace(cadena) || cadena.StartsWith("Error"))
            {
                LogToFile($"❌ Error al desencriptar cadena desde XML: {cadena}");
                throw new InvalidOperationException($"Error al obtener la cadena de conexión desde XML: {cadena}");
            }

            try
            {
                var helper = new DbHelperLibAsync(cadena);
                LogToFile("✅ Conexión inicializada correctamente desde XML.");
                return helper;
            }
            catch (Exception ex)
            {
                LogToFile($"❌ Error al construir DbHelper desde XML: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        #endregion
        #region 🧩 Obtener conexión desde JSON

        /// <summary>
        /// Devuelve una instancia de DbHelperLibAsync usando cadena desencriptada desde appsettings.json.
        /// </summary>
        public static IDbHelperAsync GetConexionJSON()
        {
            string cadena = FileHelperLib.DesencriptarCadenaConexion();

            if (string.IsNullOrWhiteSpace(cadena) || cadena.StartsWith("Error"))
            {
                LogToFile($"❌ Error al desencriptar cadena desde JSON: {cadena}");
                throw new InvalidOperationException($"Error al obtener la cadena de conexión desde JSON: {cadena}");
            }

            try
            {
                var helper = new DbHelperLibAsync(cadena);
                LogToFile("✅ Conexión inicializada correctamente desde JSON.");
                return helper;
            }
            catch (Exception ex)
            {
                LogToFile($"❌ Error al construir DbHelper desde JSON: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        #endregion
        #region 🔐 Obtener clave JWT desde JSON

        /// <summary>
        /// Extrae y desencripta la clave JWT desde appsettings.json en AppData.
        /// </summary>
        public static string GetJwtKeyFromJson()
        {
            string jsonPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DDPOS",
                "appsettings.json");

            if (!File.Exists(jsonPath))
            {
                LogToFile("❌ El archivo appsettings.json no existe.");
                throw new InvalidOperationException("El archivo appsettings.json no existe.");
            }

            try
            {
                string json = File.ReadAllText(jsonPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("Jwt", out var jwtSection) ||
                    !jwtSection.TryGetProperty("Key", out var keyProp))
                {
                    LogToFile("❌ No se encontró Jwt:Key en el JSON.");
                    throw new InvalidOperationException("La clave JWT no está definida.");
                }

                string jwtCifrada = keyProp.GetString() ?? "";
                if (string.IsNullOrWhiteSpace(jwtCifrada))
                    throw new InvalidOperationException("La clave JWT está vacía.");

                if (jwtCifrada.StartsWith(FileHelperLib.PrefijoEncriptado))
                    jwtCifrada = jwtCifrada.Substring(FileHelperLib.PrefijoEncriptado.Length);

                string claveJwt = JwtHelperLib.ObtenerJwtKeyDesencriptada(jwtCifrada);
                if (string.IsNullOrWhiteSpace(claveJwt))
                {
                    LogToFile("❌ La clave JWT no fue desencriptada correctamente.");
                    throw new InvalidOperationException("La clave JWT no fue desencriptada correctamente.");
                }

                LogToFile("✅ Clave JWT desencriptada correctamente.");
                return claveJwt;
            }
            catch (Exception ex)
            {
                LogToFile($"❌ Error al desencriptar clave JWT: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        #endregion
        #region Obtener issuer y audience desde JSON

        /// <summary>
        /// Extrae issuer y audience desde appsettings.json en AppData.
        /// </summary>
        public static (string Issuer, string Audience) GetJwtIssuerAudience()
        {
            string jsonPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DDPOS",
                "appsettings.json");

            if (!File.Exists(jsonPath))
                throw new InvalidOperationException("❌ El archivo `appsettings.json` no existe.");

            try
            {
                string json = File.ReadAllText(jsonPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("Jwt", out var jwtSection))
                    throw new InvalidOperationException("❌ No se encontró sección Jwt en el JSON.");

                string issuer = jwtSection.TryGetProperty("Issuer", out var issuerProp) ? issuerProp.GetString() ?? "" : "";
                string audience = jwtSection.TryGetProperty("Audience", out var audienceProp) ? audienceProp.GetString() ?? "" : "";

                if (string.IsNullOrWhiteSpace(issuer))
                    throw new InvalidOperationException("❌ Issuer no definido.");
                if (string.IsNullOrWhiteSpace(audience))
                    throw new InvalidOperationException("❌ Audience no definido.");

                return (issuer, audience);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"❌ Error al extraer issuer/audience: {ex.Message}", ex);
            }
        }

        #endregion
        #region 📝 Registro técnico en archivo       
        public static void LogToFile(string mensaje)
        {
            try
            {
                string rutaLog = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "DDPOS",
                    "logs",
                    "diagnostico.log");

                Directory.CreateDirectory(Path.GetDirectoryName(rutaLog)!);

                string log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {mensaje}\n";
                File.AppendAllText(rutaLog, log);
            }
            catch
            {
                // Silenciar errores de log para no romper flujo principal
            }
        }


        #endregion
    }
}
