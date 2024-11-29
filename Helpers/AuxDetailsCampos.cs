using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Transparencia.Helpers
{
    public class AuxDetailsCampos
    {
        public string Etiqueta { get; set; }
        public string Valor { get; set; }
        public string Ayuda { get; set; }
    }

    public class AuxErroresEcxel
    {
        public int NoRenglon { get; set; }
        public List<Models.Campo> Campo { get; set; }
    }
}