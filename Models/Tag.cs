using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Tag:ICloneable
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id de tag")]
        public int TagId { get; set; }

        [StringLength(200, ErrorMessage = "El nombre solo acepta 50 caracteres.")]
        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }
        
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Estatus")]
        public bool Activo { get; set; }

        [Display(Name = "Icono")]
        public string Icono { get; set; }

        [Display(Name = "Color")]
        public string Color { get; set; }

        public virtual ICollection<GrupoTag> GrupoTags { get; set; }

        public Tag()
        {
            this.GrupoTags = new HashSet<GrupoTag>();
            Activo = true;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}