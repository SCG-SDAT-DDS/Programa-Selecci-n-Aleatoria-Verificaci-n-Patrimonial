using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Fraccion
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id de Fracción")]
        public int FraccionId { get; set; }

        [StringLength(200, ErrorMessage = "El nombre solo acepta 200 caracteres.")]
        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Artículos")]
        [Required(ErrorMessage = "Los Articulos son requeridos")]
        public int ArticuloId { get; set; }

        public Articulo Articulos { get; set; }

        [Display(Name = "Url")]
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Url { get; set; }

        [Display(Name = "Ayuda")]
        public string Ayuda { get; set; }

        [Display(Name = "Orden")]
        public int Orden { get; set; }

        [Display(Name = "Estatus")]
        public bool Activo { get; set; }
    }
}