using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Transparencia.Helpers
{
    public class AuxRecuperarContrasena
    {
        public AuxRecuperarContrasena(string _nombreUsuario, DateTime _fecha, string _usuario, string _contrasena, string _url)
        {
            NombreUsuario = _nombreUsuario;
            Fecha = _fecha;
            Usuario = _usuario;
            Contrasena = _contrasena;
            Url = _url;
        }

        public AuxRecuperarContrasena()
        {

        }
        public string NombreUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
        public string Contrasena { get; set; }
        public string Url { get; set; }
    }
}