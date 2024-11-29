using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Extension: ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExtensionId { get; set; }

        [Display(Name ="Nombre Extensión")]
        [Required(ErrorMessage = "El nombre de la Extensión es requerido.")]
        [StringLength(25, ErrorMessage = "El nombre de la Extensión no debe exceder los {1} caracteres.")]
        public string Nombre { get; set; }

        public bool Activo { get; set; }

        public ICollection<GrupoExtension> GrupoExtensiones { get; set; }

        public Extension()
        {
            this.GrupoExtensiones = new HashSet<GrupoExtension>();
            this.Activo = true;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}