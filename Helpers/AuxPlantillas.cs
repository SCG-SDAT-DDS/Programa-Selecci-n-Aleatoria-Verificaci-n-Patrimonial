using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Transparencia.Models;

namespace Transparencia.Helpers
{
    public class AuxPlantillas
    {
        public string LeyNombre { get; set; }
        public int LeyId { get; set; }
        public string ArticuloNombre { get; set; }
        public int ArticuloId { get; set; }
        public string FracionNombre { get; set; }
        public int FraccionId { get; set; }

        public int PlantillaId { get; set; }
        public string NombreCorto { get; set; }
        public string NombreLargo { get; set; }
        public string NombreTabla { get; set; }
        public string Ayuda { get; set; }
        public int Orden { get; set; }
        public bool Publicado { get; set; }
        public bool Activo { get; set; }
        public DateTime? PeriodoDesde { get; set; }
        public DateTime? PeriodoHasta { get; set; }
        public FrecuenciaActualizacion Frecuencia { get; set; }

        public int PlantillaFraccionId { get; set; }

        public bool Seleccionado { get; set; }

        public string Ejercicios { get; set; }
    }

    public class AuxTitlePlantillas
    {
        public string LbNombre { get; set; }

        public int Orden { get; set; }


    }

    public class AuxCatalagoAmbiguos
    {
        public string NombreOriginal { get; set; }

        public string NombreGenerico { get; set; }


    }

}