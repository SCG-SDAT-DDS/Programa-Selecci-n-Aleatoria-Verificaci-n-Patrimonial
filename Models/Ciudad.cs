using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Ciudad:ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CiudadId { get; set; }

        [Display(Name = "Municipio")]
        [Required(ErrorMessage = "El nombre de la Ciudad es requerido")]
        [StringLength(50, ErrorMessage = "El nombre de la Ciudad no debe exceder los {1} caracteres.")]
        public string NombreCiudad { get; set; }

        public Estado Estado { get; set; }

        [Display(Name = "Estado")]
        public int EstadoId { get; set; }

        [Required(ErrorMessage = "Este campo es requerido.")]
        [RegularExpression(@"^\d{1,2}$", ErrorMessage = "Solo se permiten valores numéricos.")]
        [Range(1, 99, ErrorMessage = "Solo se permiten valores numéricos entre 1 y 99.")]
        public int Orden { get; set; }

        public bool Activo { get; set; }


        public Ciudad()
        {
            this.Activo = true;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }


}