using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class TipoFrecuencia : ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TipoFrecuenciaId { get; set; }

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El nombre del tipo de frecuencia es requerido")]
        [StringLength(200, ErrorMessage = "El nombre de la frecuencia no debe exceder los {1} caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Este campo es requerido.")]
        [RegularExpression(@"^\d{1,2}$", ErrorMessage = "Solo se permiten valores numéricos.")]
        [Range(1, 99, ErrorMessage = "Solo se permiten valores numéricos entre 1 y 99.")]
        public int Orden { get; set; }

        public bool Activo { get; set; }


        public TipoFrecuencia()
        {
            this.Activo = true;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }


}