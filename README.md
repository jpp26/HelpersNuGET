## Si estás desarrollando en .NET 8 y te importa la seguridad, la modularidad y el rendimiento, te recomiendo probar el paquete SafeConnString. Es una solución elegante y robusta para manejar cadenas de conexión SQL y claves JWT de forma segura.

## ¿Qué ofrece?

- Encriptación AES determinista para proteger datos sensibles.

- Gestión de claves JWT sincronizadas con appsettings.json.

- Almacenamiento seguro en XML Y JSON para cadena de conexion a BBDD dentro de %AppData%.

- Reconexión automática en SQL con pooling y reintentos.

- Arquitectura desacoplada y fácil de integrar.

## Ideal para:
-Aplicaciones WinForms, WPF, Worker Services.
-Proyectos que usan Microsoft.Data.SqlClient.
-Equipos que buscan seguridad sin complicarse.

## Ventajas:

-Seguridad sólida sin sacrificar simplicidad.
-Compatible con buenas prácticas modernas.
-Listo para producción en entornos exigentes.
# Funcionamiento.
Encriptación y Sincronización de Cadena de Conexión (XML + JSON) Ubicación del archivo XML: Antes de iniciar el proceso de encriptación, el archivo ConnectionString.xml debe estar ubicado en la ruta temporal del usuario: %TEMP%\DDPOS\ConnectionString.xml 
Ejemplo de candena de conexion :
 ConnectionString.xml: <?xml version="1.0"?>
<database DBcnString="Server=localhost;Database=ERP_DDPOS_PROD;User Id=ArtesanoDBO;Password=Mopbi2025;TrustServerCertificate=True;">
</database>
Se genera dos archivos de conexión, uno XML y otro JSON, xlm es ideal para la Conexion de app desktop y Json para api/web.

El sistema se encargará de moverlo automáticamente a la ruta definitiva: %APPDATA%\DDPOS\ConnectionString.xml Esto se realiza mediante el método FileHelperLib.AsegurarArchivoEnAppData().

## Propósito: Este módulo permite:
- Encriptar una cadena de conexión SQL en un archivo XML y Json.
- Sincronizar la cadena encriptada y la clave JWT en un archivo appsettings.json.
- Desencriptar la cadena para obtener una instancia blindada de conexión (IDbHelperAsync).
- Todo el flujo está diseñado para trazabilidad quirúrgica, sin ambigüedad ni estado compartido.

## Estructura del Proyecto:
- FileHelperLib.cs → Encriptación XML + sincronización JSON.
- JwtHelperLib.cs → Encriptación y desencriptación JWT.
- ConexionFactory.cs → Obtención de conexión blindada.
- ConnectionString.xml → Archivo fuente con cadena de conexión XML appsettings.json.json → Archivo fuente con cadena de conexión Json.

## Requisitos:

.NET 8. Archivo ConnectionString.xml debe existir inicialmente en %TEMP%\DDPOS. Clases CryptoHelperLib, JwtHelperLib, DbHelperLibAsync implementadas y referenciadas. Cadena de conexión válida en texto plano dentro del atributo DBcnString del XML. Clave base AES (CryptoHelperLib.ClaveBaseAES) definida para realizar la encriptación.

Tamaño de clave AES (CryptoHelperLib.KeySizeAES) especificado para el algoritmo.

## Uso:
- Encriptar cadena en XML y sincronizar JSON Método: FileHelperLib.EncriptarCadenaConexion();
- Verifica si el archivo XML existe en AppData. Si no, lo copia desde %TEMP%.
- Encripta el atributo DBcnString usando AES.
- Crea appsettings.json con la cadena encriptada, clave JWT y configuración de logging.
  ### Obtener cadena desencriptada Método: FileHelperLib.DesencriptarCadenaConexion();
- Devuelve la cadena original en texto plano.
- Si hay errores, devuelve mensaje trazable.
### Obtener conexión blindada Método:
ConexionFactory.GetConexion(); → XML Obtener conexión blindada Método: ConexionFactory.GetConexionJSON(); → JSON
#### Implementación:
public class ModuleRepository : IModuleRepository
{
    #region Inicialización

    private readonly IDbHelperAsync _dbHelper;

    public ModuleRepository()
    {
        _dbHelper = ConexionFactory.GetConexionJSON();
        ConexionFactory.LogToFile("🟢 ModuloRepositorio inicializado.");
    }

    private async Task<IDbConnection> GetConnectionAsync()
    {
        var connection = await _dbHelper.GetOpenConnectionAsync();
        if (connection == null)
        {
            ConexionFactory.LogToFile("❌ Error al obtener conexión.");
            throw new InvalidOperationException($"No se pudo obtener una conexión válida. Detalle: {_dbHelper.ErrorMessage}");
        }
        return connection;
    }
    #endregion
    ........... resto del código.

### Devuelve instancia de DbHelperLibAsync con cadena desencriptada.
Lanza excepción si la cadena no es válida.

## Detalles de Encriptación:

XML (AES) JSON (AES)

## Prefijo: ENC:

Algoritmo: AES con clave fija (ClaveBaseAES) y tamaño (KeySizeAES)

Método: CryptoHelperLib.EncryptAES(...)

## JSON (JWT)

Clave JWT encriptada con AES y prefijo ENC:

Método: JwtHelperLib.GenerarJwtKeyEncriptada(...)

## Ejemplo de appsettings.json generado:
Cadena Json: 
{
  "ConnectionStrings": {
    "SqlServer": "ENC:z46TgqFRK7zy/CSFjWO6yXHWW2HNQFsvD8UqHkoDh/cruodbGolpxPfqYGQpCA2RLK5bx\u002BsXb0jWitKnL9txSVmC9Ar8/1HeEDctrDUSRuVKdJOvX8y/VJ9L0ikLxGaqVEsQ41bjNj2gWMyBBEd1rERtnKfiwA46JPlHxjh/tbR6lDF8dk09eBn7pNbEwo\u002BoEjrEMUh7Z0LvERQkZDRHCeCQ4E71nTkQIcDAxZ9l1mM4bIcVqRT7vaWkpsbr89\u002BEoaCr9lOmFIlMRUTkvWkRzuCrg8oWhgYc8tWsxMvOD5w="
  },
  "Jwt": {
    "Key": "ENC:ErqOFSYGkm\u002BUaj\u002Bbn6FCsyuxjERQpUEN1Bc6MudfltIiBIqKPRV3qM3MEoy9gxVe",
    "Issuer": "Auth.JwtWorkerService",
    "Audience": "WinFormsClient"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft_Hosting_Lifetime": "Information"
    }
  }
}

Cadena XML: 
<?xml version="1.0"?>
<database 'DBcnString="ENC:z46TgqFRK7zy/CSFjWO6yXHWW2HNQFsvD8UqHkoDh/cruodbGolpxPfqYGQpCA2RLK5bx+sXb0jWitKnL9txSVmC9Ar8/1HeEDctrDUSRuVKdJOvX8y/VJ9L0ikLxGaqVEsQ41bjNj2gWMyBBEd1rERtnKfiwA46JPlHxjh/tbR6lDF8dk09eBn7pNbEwo+oEjrEMUh7Z0LvERQkZDRHCeCQ4E71nTkQIcDAxZ9l1mM4bIcVqRT7vaWkpsbr89+EoaCr9lOmFIlMRUTkvWkRzuCrg8oWhgYc8tWsxMvOD5w=">
</database>

## Validaciones y Diagnóstico:
- Logging técnico por consola.
- Validación de existencia de archivos.
- Prefijo ENC: para trazabilidad.
- Sincronización idempotente: no reescribe si ya está encriptado.

## Buenas prácticas:
- No modificar manualmente los archivos XML o JSON una vez generado por los encriptadores.
- Validar que la cadena no esté vacía antes de encriptar.
- Mantener CryptoHelperLib.ClaveBaseAES en entorno seguro.
- Usar ConexionFactory para obtener la conexión, nunca directamente desde XML o de JSON.
