using DbHelper;
using FileHelper;

namespace CryptoHelperLib
{
    /// <summary>
    /// Factory técnico para obtener una instancia blindada de conexión SQL.
    /// Integra desencriptado desde XML y devuelve IDbHelperAsync desacoplado.
    /// </summary>
    public static class ConexionFactory
    {
        #region Obtener conexión blindada desde XML

        /// <summary>
        /// Devuelve una instancia de DbHelperLibAsync como IDbHelperAsync.
        /// La cadena se desencripta desde ConnectionString.xml en AppData.
        /// </summary>
        public static IDbHelperAsync GetConexion()
        {
            string cadena = FileHelperLib.DesencriptarCadenaConexion();

            if (string.IsNullOrWhiteSpace(cadena) || cadena.StartsWith("Error"))
                throw new InvalidOperationException($"Error al obtener la cadena de conexión: {cadena}");

            return new DbHelperLibAsync(cadena);
        }

        #endregion
    }
}
