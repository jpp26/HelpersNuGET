using CryptoHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Security.JPP
{
    public class JwtHelperLib
    {
        public static string GenerarJwtKeyEncriptada(string clavePlano)
        {
            return "ENC:" + CryptoHelperLib.EncryptTexto(clavePlano);
        }

        public static string ObtenerJwtKeyDesencriptada(string claveEncriptada)
        {
            if (claveEncriptada.StartsWith("ENC:"))
                claveEncriptada = claveEncriptada.Substring("ENC:".Length);

            return CryptoHelperLib.DecryptTexto(claveEncriptada);
        }
    }
}
