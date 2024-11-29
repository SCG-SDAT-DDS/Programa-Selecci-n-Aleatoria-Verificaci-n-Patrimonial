 using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Transparencia.Helpers;

namespace Transparencia.Models
{
    public class vmPlantillaCampos
    {
        public string NombreTabla { get; set; }
        public string NombreTablaHistory { get; set; }
        public string NombreCorto { get; set; }
        public int IdPlantillaPNT { get; set; }
        public string NombreLargo { get; set; }
        public string Ayuda { get; set; }

        public List<vmCampoForDatos> campos { get; set; }

    }

    public class vmCampoForDatos
    {
        public int CampoId { get; set; }
        public int PlantillaId { get; set; }
        public TipoCampo TipoCampo { get; set; }
        public string Nombre { get; set; }
        public string Etiqueta { get; set; }
        //public int Longitud { get; set; }
        //public bool Requerido { get; set; }

        public string Ayuda { get; set; }
        //public bool Activo { get; set; }

        public int CatalogoId { get; set; }
        //public int Orden { get; set; }

        //nuevos cmapos
        public TipoFecha? _TipoFecha { get; set; }
        public bool? _ConDecimales { get; set; }

        //public int? _GrupoExtensionId { get; set; }
        //public int? _Size { get; set; }
        //public bool relevantes { get; set; }
        //public int? IdCampoPNT { get; set; }

        //public int? IdTipoCampoPNT { get; set; }
        //public vmCampoCatalogoRelevante campoCatalogo { get; set; }

        public string NombreTablaCatalogo { get; set; }
        public string NombreCampoCatalago { get; set; }
        public bool TablaCatalogo { get; set; }
        public string NombreCatalogo { get; set; }
        public TipoCampo TipoCampoCatalogo { get; set; }

        public int IdCampoPNT { get; set; }
        public int IdTipoCampoPNT { get; set; }

        public bool Requerido { get; set; }

        public int Longitud { get; set; }

        public List<camposTabla> camposTabla { get; set; }

        public OrdenSeleccionPublico? OrdenSeleccionPublico { get; set; }



    }
    public class camposTabla
    {
        public int CampoCatalogoId { get; set; }

        public int CatalogoId { get; set; }
        public virtual TipoCampo TipoCampo { get; set; }
        public string Nombre { get; set; }

        public string Etiqueta { get; set; }
        public int Longitud { get; set; }

        public bool Requerido { get; set; }

        public string Ayuda { get; set; }
        public bool Activo { get; set; }

        public bool Principal { get; set; }

        public int iCatalogoId { get; set; }
        public int Orden { get; set; }

        //nuevos cmapos
        public TipoFecha? _TipoFecha { get; set; }
        public bool? _ConDecimales { get; set; }

        public int? _GrupoExtensionId { get; set; }
        public int? _Size { get; set; }
        public int? IdCampoPNT { get; set; }

        public int? IdTipoCampoPNT { get; set; }

        public string NombreTablaCatalogo { get; set; }
        public string NombreCampoCatalago { get; set; }

    }

    public class vmResultadoDatos2
    {
        public int paginaActual { get; set; }
        public int totalPaginas { get; set; }
        public int totalRegistros { get; set; }
        public DateTime fechaCreacion { get; set; }

        public int PlantillaId { get; set; }
        public string NombreTabla { get; set; }
        public List<vmRowDatos> datos { get; set; }
        public List<AuxCatalagoAmbiguos> lstAmbiguos { get; set; }
        public bool IsPreved { get; set; }

        //List<vmcampoDatos> Campos { get; set; }
    }

    public class vmResultPdf
    {
        public List<Campo> campos { get; set; }
        public List<vmResultPdfCatalogos> Tablas { get; set; }

        public bool isPreved { get; set; }

        //List<vmcampoDatos> Campos { get; set; }
    }

    
   public class vmResultPdfCatalogos
    {
        public string nombreTabla { get; set; }
        public List<Campo> camposTabla { get; set; }

        //List<vmcampoDatos> Campos { get; set; }
    }


    public class vmResultadoDatosHistory
    {
        public int paginaActual { get; set; }
        public int totalPaginas { get; set; }
        public int totalRegistros { get; set; }

        public int PlantillaId { get; set; }
        public string NombreTabla { get; set; }
        public List<PlantillaHistory> datos { get; set; }
    }
    public class vmRowDatos
    {
        public int TablaFisicaId { get; set; }
        public bool Activo { get; set; }
        public int Idregistro { get; set; }

        public int catalogoId { get; set; }
        public List<vmcampoDatos> campos { get; set; }
    }
    public class vmDatosPublico
    {
        public string Label { get; set; }
        public string Valor { get; set; }
    }
    public class vmcampoDatos
    {
        public string NombreCampo { get; set; }
        public string Valor { get; set; }
        public TipoCampo TipoCampo { get; set; }
        public bool EsTabla { get; set; }
        public OrdenSeleccionPublico? OrdenSeleccionPublico { get; set; } = 0;
    }

    public class vmExcelTabs
    {
        public int catalogoId { get; set; }
        public string NombreWorksheets { get; set; }
        public int NumberWorksheets { get; set; }
        public int Row { get; set; }
        public int Columns { get; set; }
        
    }

    #region nuevoImportar
    public class vmKeyValue
    {
        public int Key { get; set; }
        public string Value { get; set; }
    }

    public class vmErroresInsert
    {
        public DataTable dtTable { get; set; }
        public string sError { get; set; }
    }

    public class vmDsTablas
    {
        public DataSet dsTable { get; set; }
        public string sError { get; set; }
    }
    public class vmINFORMATION_SCHEMA_COLUMNS
    {
        public string COLUMN_NAME { get; set; }
        public int ORDINAL_POSITION { get; set; }
        public string IS_NULLABLE { get; set; }
        public string DATA_TYPE { get; set; }
    }
    #endregion


    #region Chosen
    public class vmChosenPuiblic
    {
        public int plantillaId { get; set; }
        public FrecuenciaActualizacion sysFrecuencia { get; set; }
        public int sysNumFrecuencia { get; set; }
        public int PeriodoId { get; set; }
        public int organismoId { get; set; }
        public string Descripcion { get; set; }
        public string textPlantilla { get; set; }
        public string Errors { get; set; }
        public vmResultadoDatos2 result { get; set;}

    }
    #endregion

}