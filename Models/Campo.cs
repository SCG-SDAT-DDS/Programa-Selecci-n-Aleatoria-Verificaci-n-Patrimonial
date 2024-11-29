using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Campo:ICloneable
    {
        public int CampoId { get; set; }

        public Plantilla Plantilla { get; set; }

        public int PlantillaId { get; set; }

        [Display(Name = "Tipo de Campo")]
        public virtual TipoCampo TipoCampo{ get; set; }

        [Display(Name = "Nombre de Campo")]
        public string Nombre { get; set; }

        public string Etiqueta { get; set; }

        [Display(Name = "Longitud de Campo")]
        public int Longitud { get; set; }

        public bool Requerido { get; set; }

        public string Ayuda { get; set; }

        [Display(Name = "Orden Seleccion")]
        [Range(1, 3, ErrorMessage = "Seleccione un valor correcto de orden de seleccion")]
        public OrdenSeleccionPublico? OrdenSeleccionPublico { get; set; }   


        [Display(Name = "Estatus")]
        public bool Activo { get; set; }

        //public virtual CampoFecha Fecha { get; set; }

        //public virtual CampoArchivoAdjunto ArchivoAdjunto { get; set; }

        //public virtual CampoPorcentaje Porcentaje{ get; set; }

        //public virtual Catalogo Catalogos { get; set; }

        public int CatalogoId { get; set; }

        [Display(Name = "Orden")]
        public int Orden { get; set; }


        //public virtual GrupoExtension Extensiones { get; set; }

        //public int GrupoExtensionId { get; set; }

        //nuevos cmapos
        public TipoFecha? _TipoFecha { get; set; }
        public bool? _ConDecimales { get; set; }

        public int? _GrupoExtensionId { get; set; }
        public int? _Size { get; set; }

        [Display(Name = "Relevante")]
        public bool relevantes { get; set; }

        [Display(Name = "ID Campo PNT")]
        public int? IdCampoPNT { get; set; }

        [Display(Name = "ID Tipo de campo PNT")]
        public int? IdTipoCampoPNT { get; set; }


        [NotMapped]
        public string Valor { get; set; }
        [NotMapped]
        public int TablaFisicaId { get; set; }
        [NotMapped]
        public int CatalogoTablaFisicaId { get; set; }
        [NotMapped]
        public string ValorUrl { get; set; }

        [NotMapped]
        public string NombreCatalago { get; set; }
        [NotMapped]
        public string NombreGrupoExtension { get; set; }
        [NotMapped]
        public string NombreTipoFecha { get; set; }
        [NotMapped]
        public string ExcelValor { get; set; }
        [NotMapped]
        public bool ExcelValidacion { get; set; }

        [NotMapped]
        public string ExcelErrorTxt { get; set; }

        [NotMapped]
        public string InputHtml
        {
            get => HMTLHelperExtensions.GetCampo(this);
        }
        [NotMapped]
        public string InputHtmlDetails
        {
            get => HMTLHelperExtensions.GetCampoDetailsDForModal(this,isTable);
        }
        [NotMapped]
        public string InputHtmlEdit
        {
            get => HMTLHelperExtensions.GetCampoEdit(this);
        }
        [NotMapped]
        public string InputHtmlFilter
        {
            get => HMTLHelperExtensions.GetFilters(this);
        }
        [NotMapped]
        public string getNombreTipoCampo
        {
            get => TipoCampo.GetDisplayName();
        }
        [NotMapped]
        public string getNombreOrdenSeleccionPublico
        {
            get => OrdenSeleccionPublico.GetDisplayName();
        }

        [NotMapped]
        public bool isTable { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }

    public class CampoFecha
    {
        [ForeignKey("Campo")]
        public int CampoFechaId { get; set; }

        [Display(Name = "Tipo de Fecha")]
        public TipoFecha TipoFecha { get; set; }

        public virtual Campo Campo { get; set; }
    }

    public class CampoPorcentaje
    {
        [ForeignKey("Campo")]
        public int CampoPorcentajeId { get; set; }

        [Display(Name = "Con decimales")]
        public bool ConDecimales { get; set; }

        public virtual Campo Campo { get; set; }
    }

    public class CampoArchivoAdjunto
    {
        [ForeignKey("Campo")]
        public int CampoArchivoAdjuntoId { get; set; }

        public GrupoExtension GrupoExtension { get; set; }

        [Display(Name ="Tamaño en MB")]
        public int Size { get; set; }

        public virtual Campo Campo { get; set; }
    }

    public enum TipoCampo 
    {
        Texto = 1,
        [Display(Name="Área de Texto")]
        AreaTexto = 2,
        [Display(Name = "Numérico")]
        Numerico = 3,
        [Display(Name = "Alfanumérico")]
        Alfanumerico = 4,
        Dinero = 5,
        Porcentaje = 6,
        Decimal = 7,
        Fecha = 8,
        Hora = 9,
        [Display(Name = "Hipervínculo")]
        Hipervinculo = 10,
        [Display(Name = "Correo electrónico")]
        email = 11,
        [Display(Name = "Teléfono")]
        Telefono = 12,
        [Display(Name = "Archivo Adjunto")]
        ArchivoAdjunto = 13,
        [Display(Name = "Catálogo")]
        Catalogo = 14,
        [Display(Name = "Casilla de Verificación")]
        CasillaVerificacion = 15
    }

    public enum TipoFecha 
    {
        Normal = 1,
        [Display(Name = "Fecha Desde")]
        FechaDesde = 2,
        [Display(Name = "Fecha Hasta")]
        FechaHasta = 3
    }

}