using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class CampoViewModel
    {
        public int CampoId { get; set; }

        public int PlantillaId { get; set; }

        //[Display(Name = "Tipo de Campo")]
        public virtual TipoCampo TipoCampo { get; set; }

        //[MaxLength(10)]
        //[Display(Name = "Nombre de Campo")]
        public string Nombre { get; set; }

        //[Required]
        public string Etiqueta { get; set; }

        //[Display(Name = "Longitud de Campo")]
        public int Longitud { get; set; }

        public bool Requerido { get; set; }

        public string Ayuda { get; set; }

        //[Display(Name = "Estatus")]
        public bool Activo { get; set; }

        //[Display(Name = "Tipo de Fecha")]
        public TipoFecha TipoFecha { get; set; }

        //[Display(Name = "Tamaño en MB")]
        public int Size { get; set; }

        //[Display(Name ="Grupo de Extensiones")]
        public int GrupoExtensionId { get; set; }

        //[Display(Name = "Catálogo")]
        public int CatalogoId { get; set; }

        //[Display(Name = "Con Decimales")]
        public bool ConDecimales { get; set; }
        //[Display(Name = "Orden")]
        public int Orden { get; set; }

        public TipoFecha? _TipoFecha { get; set; }
        public bool? _ConDecimales { get; set; }

        public int? _GrupoExtensionId { get; set; }
        public int? _Size { get; set; }
        public OrdenSeleccionPublico? OrdenSeleccionPublico { get; set; }

        public string sOrdenSeleccionPublico
        {
            get => OrdenSeleccionPublico == Models.OrdenSeleccionPublico.Primera ? "Primera Seccion" : OrdenSeleccionPublico == Models.OrdenSeleccionPublico.Segunda ? "Segunda Seccion" :
                OrdenSeleccionPublico == Models.OrdenSeleccionPublico.Tercera ? "Tercera Seccion" : "";
        }
    }
}