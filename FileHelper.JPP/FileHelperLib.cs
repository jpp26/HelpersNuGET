using CryptoHelper;
using Security.JPP;
using System;
using System.IO;
using System.Text.Json;
using System.Xml;

namespace FileHelper
{
    /// <summary>
    /// Helper para operaciones sobre archivos XML de configuración.
    /// </summary>
    public static class FileHelperLib
    {
        public static readonly string XmlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DDPOS", "ConnectionString.xml");
        public const string AtributoConexion = "DBcnString";
        public const string PrefijoEncriptado = "ENC:";
        #region Archivo base
        public static void AsegurarArchivoEnAppData()
        {
            //Ruta temporal del usuario
            string tempPath = Path.Combine(Path.GetTempPath(), "DDPOS");
            string origen = Path.Combine(tempPath, "ConnectionString.xml");
            if (!File.Exists(XmlPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(XmlPath)!);
                File.Copy(origen, XmlPath, true);
                Console.WriteLine($"Archivo `ConnectionString.xml` movido a: {XmlPath}");
            }
        }
        #endregion
        #region Encriptar cadena de conexión
        public static void EncriptarCadenaConexion()
        {
            Console.WriteLine($"Ruta del archivo XML en AppData: {XmlPath}");
            AsegurarArchivoEnAppData();

            XmlDocument doc = new XmlDocument();
            doc.Load(XmlPath);
            XmlElement root = doc.DocumentElement ?? throw new InvalidOperationException("El XML no tiene raíz.");
            string dbcnString = root.GetAttribute(AtributoConexion);
            if (string.IsNullOrWhiteSpace(dbcnString))
            {
                Console.WriteLine("Error: La cadena de conexión está vacía.");
                return;
            }
            if (!dbcnString.StartsWith(PrefijoEncriptado))
            {
                string encriptado = PrefijoEncriptado + CryptoHelperLib.EncryptAES(
                    dbcnString,
                    CryptoHelperLib.ClaveBaseAES,
                    CryptoHelperLib.KeySizeAES);

                root.SetAttribute(AtributoConexion, encriptado);
                doc.Save(XmlPath);
                Console.WriteLine("Cadena de conexión encriptada correctamente.");
                string claveJwtPlano = CryptoHelperLib.ClaveBaseAES; // Semilla
                // Sincronizar con appsettings.json
                SincronizarAppSettings(encriptado, claveJwtPlano);
            }
            else
            {
                Console.WriteLine("La cadena de conexión ya está encriptada.");
            }
        }
        #endregion
        #region Desencriptar cadena de conexión     
        public static string DesencriptarCadenaConexion()
        {
            Console.WriteLine($"Ruta esperada del archivo XML en AppData: {XmlPath}");
            AsegurarArchivoEnAppData();
            if (!File.Exists(XmlPath))
            {
                return "Error: El archivo `ConnectionString.xml` no existe.";
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlPath);
            XmlElement root = doc.DocumentElement;
            if (root == null)
            {
                return "Error: El archivo XML no tiene nodo raíz.";
            }
            string dbcnString = root.GetAttribute(AtributoConexion);
            if (string.IsNullOrWhiteSpace(dbcnString))
            {
                return $"Error: El atributo `{AtributoConexion}` está vacío o no definido.";
            }
            if (dbcnString.StartsWith(PrefijoEncriptado))
            {
                dbcnString = dbcnString.Substring(PrefijoEncriptado.Length);
            }
            Console.WriteLine($"Antes de desencriptar: {dbcnString}");
            string desencriptado = CryptoHelperLib.DecryptAES(dbcnString, CryptoHelperLib.ClaveBaseAES, CryptoHelperLib.KeySizeAES);
            Console.WriteLine($"Después de desencriptar: {desencriptado}");
            if (string.IsNullOrWhiteSpace(desencriptado))
            {
                return "Error: La desencriptación devolvió una cadena vacía.";
            }
            else
            {
                return desencriptado;
            }

        }
        #endregion
        #region Sincronizar con appsettings.json
        private static void SincronizarAppSettings(string cadenaEncriptada, string claveJwtPlano)
        {
            string jsonPath = Path.Combine(Path.GetDirectoryName(XmlPath)!, "appsettings.json");
            // Crear estructura base si no existe
            if (!File.Exists(jsonPath))
            {
                string claveJwtEncriptada = JwtHelperLib.GenerarJwtKeyEncriptada(claveJwtPlano);

                var jsonBase = new
                {
                    ConnectionStrings = new { SqlServer = cadenaEncriptada },
                    Jwt = new
                    {
                        Key = claveJwtEncriptada,
                        Issuer = "Auth.JwtWorkerService",
                        Audience = "WinFormsClient"
                    },
                    Logging = new
                    {
                        LogLevel = new
                        {
                            Default = "Information",
                            Microsoft = "Warning",
                            Microsoft_Hosting_Lifetime = "Information"
                        }
                    }
                };

                string json = System.Text.Json.JsonSerializer.Serialize(jsonBase, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(jsonPath, json);
                Console.WriteLine("Archivo `appsettings.json` creado y sincronizado.");
            }

        }
        #endregion
        #region Desencriptar configuración completa desde appsettings.json

        /// <summary>
        /// Extrae y desencripta cadena SQL, clave JWT, issuer y audience desde appsettings.json.
        /// </summary>
        public static (string CadenaSql, string ClaveJwt, string Issuer, string Audience) DesencriptarConfiguracionDesdeJson()
        {
            string jsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DDPOS", "appsettings.json");

            if (!File.Exists(jsonPath))
                throw new InvalidOperationException("❌ El archivo `appsettings.json` no existe.");

            try
            {
                string json = File.ReadAllText(jsonPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // 🔓 Cadena SQL
                if (!root.TryGetProperty("ConnectionStrings", out var connSection) ||
                    !connSection.TryGetProperty("SqlServer", out var sqlProp))
                    throw new InvalidOperationException("❌ No se encontró `ConnectionStrings.SqlServer`.");

                string sqlCifrada = sqlProp.GetString() ?? "";
                if (string.IsNullOrWhiteSpace(sqlCifrada))
                    throw new InvalidOperationException("❌ La cadena SQL está vacía.");

                if (sqlCifrada.StartsWith(PrefijoEncriptado))
                    sqlCifrada = sqlCifrada.Substring(PrefijoEncriptado.Length);

                string cadenaSql = JwtHelperLib.ObtenerJwtKeyDesencriptada(sqlCifrada);
                if (string.IsNullOrWhiteSpace(cadenaSql))
                    throw new InvalidOperationException("❌ La cadena SQL no fue desencriptada correctamente.");

                // 🔓 Clave JWT
                if (!root.TryGetProperty("Jwt", out var jwtSection) ||
                    !jwtSection.TryGetProperty("Key", out var keyProp))
                    throw new InvalidOperationException("❌ No se encontró `Jwt:Key`.");

                string jwtCifrada = keyProp.GetString() ?? "";
                if (string.IsNullOrWhiteSpace(jwtCifrada))
                    throw new InvalidOperationException("❌ La clave JWT está vacía.");

                if (jwtCifrada.StartsWith(PrefijoEncriptado))
                    jwtCifrada = jwtCifrada.Substring(PrefijoEncriptado.Length);

                string claveJwt = JwtHelperLib.ObtenerJwtKeyDesencriptada(jwtCifrada);
                if (string.IsNullOrWhiteSpace(claveJwt))
                    throw new InvalidOperationException("❌ La clave JWT no fue desencriptada correctamente.");

                // 🔐 Issuer y Audience
                string issuer = jwtSection.TryGetProperty("Issuer", out var issuerProp) ? issuerProp.GetString() ?? "" : "";
                string audience = jwtSection.TryGetProperty("Audience", out var audienceProp) ? audienceProp.GetString() ?? "" : "";

                if (string.IsNullOrWhiteSpace(issuer))
                    throw new InvalidOperationException("❌ Issuer no definido.");
                if (string.IsNullOrWhiteSpace(audience))
                    throw new InvalidOperationException("❌ Audience no definido.");

                return (cadenaSql, claveJwt, issuer, audience);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"❌ Error al leer o desencriptar configuración JSON: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
