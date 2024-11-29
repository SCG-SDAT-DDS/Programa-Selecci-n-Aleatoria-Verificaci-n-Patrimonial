using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Ley
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id de Ley")]
        public int LeyId { get; set; }

        [StringLength(200, ErrorMessage = "El nombre solo acepta 200 caracteres.")]
        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Tipo de leyes")]
        [Required(ErrorMessage = "El tipo de leyes son requeridas")]
        public int TipoLeyId { get; set; }

        public TipoLey TipoLeyes { get; set; }

        [Display(Name = "Url")]
        [Required(ErrorMessage = "La URL es requerido")]
        public string Url { get; set; }

        [Display(Name = "Ayuda")]
        public string Ayuda { get; set; }

        [Display(Name = "Orden")]
        public int Orden { get; set; }

        [Display(Name = "Estatus")]
        public bool Activo { get; set; }
    }
}