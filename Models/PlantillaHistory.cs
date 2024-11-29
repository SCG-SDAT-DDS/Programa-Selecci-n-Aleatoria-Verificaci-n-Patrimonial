using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class PlantillaHistory : ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int PlantillaHistoryId { get; set; }

        [Display(Name = "Plantilla")]
        [Required(ErrorMessage = "Este campo es requerido.")]
        public int PlantillaId { get; set; }

        public Plantilla Plantillas { get; set; }

        [Display(Name = "Fecha creación")]
        [Required(ErrorMessage = "Este campo es requerido.")]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Usuario Creo")]
        [Required(ErrorMessage = "Este campo es requerido.")]
        [StringLength(128, ErrorMessage = "El usuario no debe exceder los {1} caracteres.")]
        public string UsuarioId { get; set; }

        [Display(Name = "Organismo")]
        [Required(ErrorMessage = "Este campo es requerido.")]
        public int OrganismoID { get; set; }

        public Organismo Organismo { get; set; }

        [Display(Name = "Periodo")]
        [Required(ErrorMessage = "Este campo es requerido.")]
        public int PeriodoId { get; set; }

        public Periodo Periodo { get; set; }

        [Display(Name = "Tipo de frecuencia")]
        [Required(ErrorMessage = "Este campo es requerido.")]
        public FrecuenciaActualizacion SysFrecuencia { get; set; }
        
        [Display(Name = "frecuencia")]
        [Required(ErrorMessage = "Este campo es requerido.")]
        public int SysNumFrecuencia { get; set; }

        public bool Activo { get; set; }

        [NotMapped]
        public string FechaCreacionToString
        {
            get => FechaCreacion.ToString("dd", CultureInfo.CreateSpecificCulture("es-MX")) + " de " +
                   FechaCreacion.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-MX")) + " del " +
                   FechaCreacion.ToString("yyyy", CultureInfo.CreateSpecificCulture("es-MX")) 
                +" "+ FechaCreacion.ToString("HH:mm:ss");
        }

        [NotMapped]
        public string SysNumFrecuenciaToString
        {
            get => SysFrecuencia.GetFrecuencia(SysNumFrecuencia);
        }


        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}