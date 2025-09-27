using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace CryptoHelper
{
    public static class CryptoHelperLib
    {
        //Clave base para derivación AES
        public static readonly string ClaveBaseAES = "13Tuyuti.Cavichu.Pochy";
        // Salt fijo para derivación determinista
        public static readonly byte[] SaltAES = Encoding.UTF8.GetBytes("Kavichupochy_2025");
        // Tamaño de clave por defecto
        public const int KeySizeAES = 256;

        #region API pública

        /// <summary>Encripta texto plano usando AES determinista.</summary>
        public static string EncryptTexto(string textoPlano)
            => EncryptAES(textoPlano, ClaveBaseAES, KeySizeAES);

        /// <summary>Desencripta texto cifrado usando AES determinista.</summary>
        public static string DecryptTexto(string textoCifrado)
            => DecryptAES(textoCifrado, ClaveBaseAES, KeySizeAES);

        #endregion
        #region AES Interno

        public static string EncryptAES(string textoPlano, string password, int bits)
        {
            ValidarParametros(textoPlano, password, bits);

            byte[] datos = Encoding.Unicode.GetBytes(textoPlano);
            var (clave, vector) = DerivarClaveYVector(password, bits);

            using var aes = Aes.Create();
            aes.Key = clave;
            aes.IV = vector;

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(datos, 0, datos.Length);
                cs.Close();
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static string DecryptAES(string textoCifrado, string password, int bits)
        {
            ValidarParametros(textoCifrado, password, bits);

            byte[] datosCifrados = Convert.FromBase64String(textoCifrado);
            var (clave, vector) = DerivarClaveYVector(password, bits);

            using var aes = Aes.Create();
            aes.Key = clave;
            aes.IV = vector;

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(datosCifrados, 0, datosCifrados.Length);
                cs.Close();
                return Encoding.Unicode.GetString(ms.ToArray());
            }
        }

        #endregion
        #region Utilidades

        private static (byte[] clave, byte[] vector) DerivarClaveYVector(string password, int bits)
        {
            using var pdb = new Rfc2898DeriveBytes(password, SaltAES, 100_000, HashAlgorithmName.SHA256);
            byte[] clave = pdb.GetBytes(bits / 8);
            byte[] vector = pdb.GetBytes(16);
            return (clave, vector);
        }

        private static void ValidarParametros(string texto, string password, int bits)
        {
            if (string.IsNullOrWhiteSpace(texto))
                throw new ArgumentException("El texto no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía.");
            if (bits != 128 && bits != 192 && bits != 256)
                throw new ArgumentException("El tamaño de clave debe ser 128, 192 o 256 bits.");
        }
        #endregion
    }
}
