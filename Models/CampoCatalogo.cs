using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class CampoCatalogo: ICloneable
    {
        public int CampoCatalogoId { get; set; }

        public Catalogo Catalogo { get; set; }

        public int CatalogoId { get; set; }

        [Display(Name = "Tipo de Campo")]
        public virtual TipoCampo TipoCampo { get; set; }

        [Display(Name = "Nombre de Campo")]
        public string Nombre { get; set; }

        public string Etiqueta { get; set; }

        [Display(Name = "Longitud de Campo")]
        public int Longitud { get; set; }

        public bool Requerido { get; set; }

        public string Ayuda { get; set; }

        [Display(Name = "Estatus")]
        public bool Activo { get; set; }

        public bool Principal { get; set; }

        //public virtual CampoFecha Fecha { get; set; }

        //public virtual CampoArchivoAdjunto ArchivoAdjunto { get; set; }

        //public virtual CampoPorcentaje Porcentaje{ get; set; }

        //public virtual Catalogo Catalogos { get; set; }

        public int iCatalogoId { get; set; }

        [Display(Name = "Orden")]
        public int Orden { get; set; }


        //public virtual GrupoExtension Extensiones { get; set; }

        //public int GrupoExtensionId { get; set; }

        //nuevos cmapos
        public TipoFecha? _TipoFecha { get; set; }
        public bool? _ConDecimales { get; set; }

        public int? _GrupoExtensionId { get; set; }
        public int? _Size { get; set; }


        [Display(Name = "ID Campo PNT")]
        public int? IdCampoPNT { get; set; }

        [Display(Name = "ID Tipo de campo PNT")]
        public int? IdTipoCampoPNT { get; set; }

        [NotMapped]
        public string Valor { get; set; }
        [NotMapped]
        public string ValorUrl { get; set; }

        [NotMapped]
        public string NombreCatalago { get; set; }
        [NotMapped]
        public string NombreGrupoExtension { get; set; }
        [NotMapped]
        public string NombreTipoFecha { get; set; }

        [NotMapped]
        public string NombreProvicional { get; set; }

        [NotMapped]
        public string InputHtml
        {
            get => HMTLHelperExtensions.GetCampo(this);
        }
        [NotMapped]
        public string InputHtmlDetails
        {
            get => HMTLHelperExtensions.GetCampoDetails(this);
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

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }
}