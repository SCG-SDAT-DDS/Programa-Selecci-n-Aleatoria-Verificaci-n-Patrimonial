using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Pais: ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="Id")]
        public int PaisId { get; set; }

        [Display(Name ="País")]
        [Required(ErrorMessage = "El nombre del País es requerido.")]
        [StringLength(30, ErrorMessage = "El nombre del País no debe exceder los {1} caracteres.")]
        public string NombrePais { get; set; }

        [Required(ErrorMessage = "Este campo es requerido.")]
        [RegularExpression(@"^\d{1,2}$", ErrorMessage = "Solo se permiten valores numéricos.")]
        [Range(1, 99, ErrorMessage = "Solo se permiten valores numéricos entre 1 y 99.")]
        public int Orden { get; set; }

        public bool Activo { get; set; }

        public Pais()
        {
            this.Activo = true;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}