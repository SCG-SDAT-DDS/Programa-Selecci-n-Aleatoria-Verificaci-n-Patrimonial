using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Catalogo:ICloneable
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id de catálogo")]
        public int CatalogoId { get; set; }

        [StringLength(50, ErrorMessage = "El nombre solo acepta 50 caracteres.")]
        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Nombre tabla")]
        public string NombreTabla { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }
        [Display(Name = "Orden")]
        public int orden { get; set; }

        [Display(Name = "Dinámico")]
        public bool dinamico { get; set; }

        [Display(Name = "Sub Plantilla")]
        public bool Tabla { get; set; }

        [Display(Name = "Estatus")]
        public bool Activo { get; set; }

        public Catalogo()
        {
            Activo = true;
            dinamico = true;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}