using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class TipoOrganismo: ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int TipoOrganismoId { get; set; }

        [Display(Name = "Tipo de Organismo")]
        [Required(ErrorMessage = "El Tipo de Organismo es requerido.")]
        [StringLength(50, ErrorMessage = "El Tipo de Organismo no debe exceder los {1} caracteres.")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La Descripción es requerida.")]
        [StringLength(50, ErrorMessage = "La Descripción no debe exceder los {1} caracteres.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Este campo es requerido.")]
        [RegularExpression(@"^\d{1,2}$", ErrorMessage = "Solo se permiten valores numéricos.")]
        [Range(1, 99, ErrorMessage = "Solo se permiten valores numéricos entre 1 y 99.")]
        public int Orden { get; set; }

        [Display(Name = "Icono")]
        public string Icono { get; set; }

        [Display(Name = "Color")]
        public string Color { get; set; }

        public bool Activo { get; set; }

        public TipoOrganismo()
        {
            this.Activo = true;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}