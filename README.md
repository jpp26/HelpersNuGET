### Encriptación y Sincronización de Cadena de Conexión (XML + JSON) ###

Ubicación del archivo XML: Antes de iniciar el proceso de encriptación, el archivo ConnectionString.xml debe estar ubicado en la ruta temporal del usuario: %TEMP%\DDPOS\ConnectionString.xml

El sistema se encargará de moverlo automáticamente a la ruta definitiva: %APPDATA%\DDPOS\ConnectionString.xml Esto se realiza mediante el método FileHelperLib.AsegurarArchivoEnAppData().

Propósito: Este módulo permite:

Encriptar una cadena de conexión SQL en un archivo XML.

Sincronizar la cadena encriptada y la clave JWT en un archivo appsettings.json.

Desencriptar la cadena para obtener una instancia blindada de conexión (IDbHelperAsync).

Todo el flujo está diseñado para trazabilidad quirúrgica, sin ambigüedad ni estado compartido.

Estructura del Proyecto:

FileHelperLib.cs → Encriptación XML + sincronización JSON

JwtHelperLib.cs → Encriptación y desencriptación JWT

ConexionFactory.cs → Obtención de conexión blindada

ConnectionString.xml → Archivo fuente con cadena de conexión

appsettings.json → Archivo destino sincronizado

Requisitos:

.NET Framework o .NET Core compatible con System.Xml y System.Text.Json.

Archivo ConnectionString.xml debe existir inicialmente en %TEMP%\DDPOS.

Clases CryptoHelperLib, JwtHelperLib, DbHelperLibAsync implementadas y referenciadas.

Cadena de conexión válida en texto plano dentro del atributo DBcnString del XML.

Clave base AES (CryptoHelperLib.ClaveBaseAES) definida para realizar la encriptación.

Tamaño de clave AES (CryptoHelperLib.KeySizeAES) especificado para el algoritmo.

Uso:

Encriptar cadena en XML y sincronizar JSON Método: FileHelperLib.EncriptarCadenaConexion();

Verifica si el archivo XML existe en AppData. Si no, lo copia desde %TEMP%.

Encripta el atributo DBcnString usando AES.

Crea appsettings.json con la cadena encriptada, clave JWT y configuración de logging.

Obtener cadena desencriptada Método: FileHelperLib.DesencriptarCadenaConexion();

Devuelve la cadena original en texto plano.

Si hay errores, devuelve mensaje trazable.

Obtener conexión blindada Método: ConexionFactory.GetConexion();

Devuelve instancia de DbHelperLibAsync con cadena desencriptada.

Lanza excepción si la cadena no es válida.

Detalles de Encriptación:

XML (AES)

Prefijo: ENC:

Algoritmo: AES con clave fija (ClaveBaseAES) y tamaño (KeySizeAES)

Método: CryptoHelperLib.EncryptAES(...)

JSON (JWT)

Clave JWT encriptada con AES y prefijo ENC:

Método: JwtHelperLib.GenerarJwtKeyEncriptada(...)

Ejemplo de appsettings.json generado:

{ "ConnectionStrings": { "SqlServer": "ENC:..." }, "Jwt": { "Key": "ENC:...", "Issuer": "Auth.JwtWorkerService", "Audience": "WinFormsClient" }, "Logging": { "LogLevel": { "Default": "Information", "Microsoft": "Warning", "Microsoft.Hosting.Lifetime": "Information" } } }

Validaciones y Diagnóstico:

Logging técnico por consola.

Validación de existencia de archivos.

Prefijo ENC: para trazabilidad.

Sincronización idempotente: no reescribe si ya está encriptado.

Buenas prácticas:

No modificar manualmente los archivos XML o JSON.

Validar que la cadena no esté vacía antes de encriptar.

Mantener CryptoHelperLib.ClaveBaseAES en entorno seguro.

Usar ConexionFactory para obtener la conexión, nunca directamente desde XML.
