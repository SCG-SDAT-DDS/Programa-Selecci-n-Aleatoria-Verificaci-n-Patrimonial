using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Frecuencia : ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FrecuenciaId { get; set; }

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El nombre de la frecuencia es requerido")]
        [StringLength(200, ErrorMessage = "El nombre de la frecuencia no debe exceder los {1} caracteres.")]
        public string Nombre { get; set; }

        public TipoFrecuencia TipoFrecuencia { get; set; }

        [Display(Name = "Tipo de frecuencia")]
        public int TipoFrecuenciaId { get; set; }

        [Display(Name = "Clave")]
        public int Clave { get; set; }

        [Required(ErrorMessage = "Este campo es requerido.")]
        [RegularExpression(@"^\d{1,2}$", ErrorMessage = "Solo se permiten valores numéricos.")]
        [Range(1, 99, ErrorMessage = "Solo se permiten valores numéricos entre 1 y 99.")]
        public int Orden { get; set; }

        public bool Activo { get; set; }


        public Frecuencia()
        {
            this.Activo = true;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }
}