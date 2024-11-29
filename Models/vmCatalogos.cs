using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    
    public class vmCatalagoExcel
    {
        public string NombreTabla { get; set; }
        public string Descripcion { get; set; }
        public string Nombre { get; set; }
        public List<vmCatalagoCampos> catalagoCampos { get; set; }
    }

    public class vmCatalagoCampos
    {
        public int CampoCatalogoId { get; set; }
        public TipoCampo TipoCampo { get; set; }
        public string Nombre { get; set; }
        public string Etiqueta { get; set; }
        public string Ayuda { get; set; }
        public bool? _ConDecimales { get; set; }
    }
}