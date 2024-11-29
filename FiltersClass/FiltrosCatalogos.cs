using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Transparencia.Models;

namespace Transparencia
{

    public class FiltrosCatalogos
    {

        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool? ActivoNull { get; set; }
        public bool Activo { get; set; }
        public bool? EstatusNull { get; set; }



    }

    public class FiltrosUsuarios : FiltrosCatalogos
    {
        public string Usuario { get; set; }

        public string email { get; set; }
    }

    public class FiltrosFraccion:FiltrosCatalogos
    {
        
        public int Articulo { get; set; }
    }
    public class FiltrosLeyes : FiltrosCatalogos
    {

        public int TipoLeyes { get; set; }
    }
    public class FiltrosArticulo : FiltrosCatalogos
    {

        public int Ley { get; set; }
    }

    public class FiltrosInputConfig : FiltrosCatalogos
    {

        public int TipoInputs { get; set; }
        public int DataTypeMs { get; set; }
    }

    public class FiltrosPlantilla : FiltrosCatalogos
    {

        public int TipoEstatus { get; set; }
        public string NombreCorto { get; set; }
        public string NombreLargo { get; set; }
        public string NombreTabla { get; set; }
        public int LeyId { get; set; }
        public int ArticuloId { get; set; }
        public int FraccionId { get; set; }
        public int PeriodoId { get; set; }
        public DateTime? PeriodoDesde { get; set; }
        public DateTime? PeriodoHasta { get; set; }
        public string UsuarioId { get; set; }
    }

    public class FiltrosPlantillaHistory
    {
        public int PeriodoId { get; set; }
        public int OrganismoID { get; set; }
        public FrecuenciaActualizacion? SysFrecuencia { get; set; }
        public int SysNumFrecuencia { get; set; }
    }
    public class FiltrosImpMasivo : FiltrosCatalogos
    {

        public int PlantillaId { get; set; }
        public int OrganismoId { get; set; }
        public int PeriodoId { get; set; }
        public string UsuarioId { get; set; }
        public DateTime? Fecha { get; set; }
        public int sysFrecuencia { get; set; }
        public int sysNumFrecuencia { get; set; }


    }

    public class FiltrosOtraInfo : FiltrosCatalogos
    {

        public string OtraInfoNombre { get; set; }
        public string OtraInfoURL { get; set; }
        public string OtraInfoNotas { get; set; }
        public int TipoOtraInformacionId { get; set; }
        public bool? OtraInfoActivo { get; set; }
        public int OtraInfoOrganismoId { get; set; }
    }
    public class FiltrosTipoOtraInfo : FiltrosCatalogos
    {

        public string TipoOtraInfoNombre { get; set; }
        public string TipoOtraInfoDesc { get; set; }
        public bool? TipoOtraInfoActivo { get; set; }
    }

    public class FiltrosPlantillaFraccion : FiltrosCatalogos
    {
        public int Plantillas { get; set; }
        public int Organismos { get; set; }
        public int Fracciones { get; set; }
    }

    public class FiltrosUnidades:FiltrosCatalogos
    {
        public string Organismo { get; set; }

        public string Siglas { get; set; }
    }

    public class FiltrosCiudades:FiltrosCatalogos
    {
        public string Pais { get; set; }

        public string Estado { get; set; }
    }
  
}