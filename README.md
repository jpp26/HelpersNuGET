### CryptoHelperLib & DbHelperLibAsync (.NET 8) ### 

##### PROPÓSITO GENERAL #####
Este conjunto de clases permite manejar de forma segura cadenas de conexión SQL y claves JWT, utilizando encriptación AES, archivos XML, y una arquitectura desacoplada para conexiones asincrónicas.

#### COMPONENTES PRINCIPALES ######

#### CryptoHelperLib ######

Encripta y desencripta texto usando AES determinista.

Clave base fija: "xxxxxxx"

Salt fijo: "xxxxxxxx"

Tamaño de clave: 256 bits

###Métodos: ###

EncryptAES(textoPlano, password, bits)

DecryptAES(textoCifrado, password, bits)

EncryptTexto(textoPlano)

DecryptTexto(textoCifrado)

JwtHelperLib

Genera claves JWT encriptadas con AES.

##### Métodos: ####

GenerarJwtKeyEncriptada(clavePlano)

ObtenerJwtKeyDesencriptada(claveEncriptada)

##### FileHelperLib ####

Administra el archivo XML ConnectionString.xml en AppData.

Encripta y desencripta la cadena de conexión.

Sincroniza con appsettings.json si no existe.

Constantes:

XmlPath: ruta en AppData

AtributoConexion: "DBcnString"

PrefijoEncriptado: "ENC:"

Métodos:

AsegurarArchivoEnAppData()

EncriptarCadenaConexion()

DesencriptarCadenaConexion()

SincronizarAppSettings(cadenaEncriptada, claveJwtPlano)

ConexionFactory

Fábrica técnica que devuelve una instancia blindada de IDbHelperAsync.

Método:

GetConexion(): desencripta desde XML y retorna DbHelperLibAsync

DbHelperLibAsync

Implementa conexión SQL asincrónica con reconexión y pooling.

Constructor:

DbHelperLibAsync(string connection)

Métodos:

GetOpenConnectionAsync(): abre conexión con reintentos

CloseConnection(): cierra manualmente

Dispose(): libera recursos

Propiedades:

ErrorMessage

ConnectionString

IsConnected

CommandTimeoutProp = 30

ConnectionTimeoutProp = 15

RetryCountProp = 3

RetryDelayMsProp = 2000

MaxPoolSizeProp = 100

MinPoolSizeProp = 5

PoolingEnabled = true

IDbHelperAsync (Interfaz)

Define el contrato para acceso asincrónico a SQL.

Métodos:

GetOpenConnectionAsync()

Propiedades:

ErrorMessage

ConnectionString

IsConnected

CommandTimeoutProp

ConnectionTimeoutProp

RetryCountProp

RetryDelayMsProp

MaxPoolSizeProp

MinPoolSizeProp

PoolingEnabled

=========================================== ARCHIVOS UTILIZADOS ===========================================

ConnectionString.xml → ubicado en %AppData%\DDPOS\

appsettings.json → generado automáticamente si no existe

===========================================  SEGURIDAD ===========================================

Encriptación AES con clave derivada y salt fijo.

Prefijo "ENC:" para identificar cadenas encriptadas.

JWT sincronizado con appsettings.json..

Reconexión automática en SQL hasta 3 intentos.

===========================================  VENTAJAS ===========================================

Seguridad robusta para cadenas sensibles.

Modularidad y desacoplamiento.

Compatible con .NET 8 y Microsoft.Data.SqlClient.

Ideal para aplicaciones WinForms, WPF, Worker Services, etc.

===========================================  REQUISITOS ===========================================

.NET 8

Microsoft.Data.SqlClient

System.Security.Cryptography

System.Text.Json
