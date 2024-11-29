using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Plantilla
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlantillaId { get; set; }

        //[Display(Name = "Tipo de Estatus")]
        //[Required(ErrorMessage = "El tipo de input es requerido")]
        //public int TipoEstatusId { get; set; }

        //public TipoEstatus TipoEstatus { get; set; }

        [Display(Name = "Nombre corto")]
        [StringLength(50, ErrorMessage = "El nombre corto solo acepta 50 caracteres.")]
        [Required(ErrorMessage = "El nombre corto es requerido")]
        public string NombreCorto { get; set; }

        [Display(Name = "Nombre largo")]
        [StringLength(200, ErrorMessage = "El nombre largo solo acepta 50 caracteres.")]
        [Required(ErrorMessage = "El nombre largo es requerido")]
        public string NombreLargo { get; set; }

        [Display(Name = "Nombre tabla")]
        public string NombreTabla { get; set; }
        
        [Display(Name = "Nombre tabla")]
        public string NombreTablaHistory { get; set; }


        [Display(Name = "Ayuda")]
        public string Ayuda { get; set; }

        [Display(Name = "Frecuencia de conservación")]
        public int? FrecuenciaConservacion { get; set; }

        [Display(Name = "Descripción de frecuencia de conservación.")]
        public string DescripcionFrecuenciaConservacion { get; set; }

        [Display(Name = "Orden")]
        public int Orden { get; set; }

        [Display(Name = "Estatus")]
        public bool Activo { get; set; }

        [Display(Name = "Publicado")]
        public bool Publicado { get; set; }

        [Display(Name = "ES PREVED")]
        public bool IsPreved { get; set; }

        [Display(Name = "Período desde")]
        public DateTime? PeriodoDesde { get; set; }

        [Display(Name = "Período hasta")]
        public DateTime? PeriodoHasta { get; set; }

        [Display(Name = "Frecuencia de Actualización")]
        [Range(0, 5, ErrorMessage = "Seleccione un valor de Frecuencia")]
        public FrecuenciaActualizacion Frecuencia { get; set; }

        [Display(Name = "ID Plantilla PNT")]
        public int? IdPlantillaPNT { get; set; }

        [Display(Name = "Fecha de modificacións")]
        public DateTime? FechaModificacion { get; set; }

        [Display(Name = "Número de selección")]
        [Required(ErrorMessage = "El número es requerido")]
        [DisplayFormat(DataFormatString = "{0:0.###}", ApplyFormatInEditMode = true)]
        public decimal Porcentage { get; set; }

        [NotMapped]
        public virtual PlantillaFraccion PlantillaFraccion { get; set; }

        [Display(Name = "Periodos")]
        public virtual ICollection<Periodo> Periodos { get; set; }

        [Display(Name = "Tags")]
        public virtual ICollection<GrupoTag> Tags { get; set; }

        public Plantilla()
        {
            this.Periodos = new HashSet<Periodo>();
            this.Tags = new HashSet<GrupoTag>();
        }


        [NotMapped]
        public string PeriodoDesdeToString
        {
            get => PeriodoDesde.HasValue ? PeriodoDesde.Value.ToString("dd", CultureInfo.CreateSpecificCulture("es-MX")) + " de " +
                   PeriodoDesde.Value.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-MX")) + " del " +
                   PeriodoDesde.Value.ToString("yyyy", CultureInfo.CreateSpecificCulture("es-MX")) : "";
        }

        [NotMapped]
        public string PeriodoHastaToString
        {
            get => PeriodoHasta.HasValue ? "hasta "+ PeriodoHasta.Value.ToString("dd", CultureInfo.CreateSpecificCulture("es-MX")) + " de " +
                PeriodoHasta.Value.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-MX")) + " del " +
                PeriodoHasta.Value.ToString("yyyy", CultureInfo.CreateSpecificCulture("es-MX")) : PeriodoDesde.HasValue ? " en adelante" : "No registrado.";
        }





    }
}