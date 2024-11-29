using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class TipoInput
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TipoInputId { get; set; }

        [Display(Name = "Nombre")]
        [StringLength(50, ErrorMessage = "El nombre solo acepta 50 caracteres.")]
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; }

        [Display(Name = "Ayuda")]
        public string Ayuda { get; set; }

        [Display(Name = "Orden")]
        public int Orden { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }
    }
}