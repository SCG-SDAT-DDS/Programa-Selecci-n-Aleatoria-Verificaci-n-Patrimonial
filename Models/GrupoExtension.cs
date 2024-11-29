using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class GrupoExtension: ICloneable
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GrupoExtensionId { get; set; }

        [Display(Name = "Nombre del Grupo de Extensiones")]
        [Required(ErrorMessage = "El nombre del Grupo de Extensiones es requerido.")]
        [StringLength(25, ErrorMessage = "El nombre del Grupo de Extensiones no debe exceder los {1} caracteres.")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La Descripción del Grupo de Extensiones es requerida.")]
        [StringLength(50, ErrorMessage = "La Descripción del Grupo de Extensiones no debe exceder los {1} caracteres.")]
        public string Descripcion { get; set; }

        public bool Activo { get; set; }

        public virtual ICollection<Extension> Extensiones { get; set; }

        public GrupoExtension()
        {
            this.Extensiones = new HashSet<Extension>();
            this.Activo = true;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}