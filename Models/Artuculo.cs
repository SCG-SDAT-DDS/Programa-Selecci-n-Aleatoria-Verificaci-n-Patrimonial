using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Articulo: ICloneable
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id de Articulo")]
        public int ArticuloId { get; set; }

        [StringLength(200, ErrorMessage = "El nombre solo acepta 200 caracteres.")]
        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Leyes")]
        [Required(ErrorMessage = "Las Leyes son requeridas")]
        public int LeyId { get; set; }

        public Ley Leyes { get; set; }

        [Display(Name = "Ayuda")]
        public string Ayuda { get; set; }

        [Display(Name = "Orden")]
        public int Orden { get; set; }

        [Display(Name = "Estatus")]
        public bool Activo { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}