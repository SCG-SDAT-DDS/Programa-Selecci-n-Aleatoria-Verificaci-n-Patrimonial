using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class ListarGrupoTag
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id de Listar grupo tag")]
        public int ListarGrupoTagId { get; set; }
        
        [Required(ErrorMessage = "El grupo de tags es requerido")]
        [Display(Name = "Grupo tags")]
        public int GrupoTagId { get; set; }

        public virtual GrupoTag GrupoTags { get; set; }

        [Required(ErrorMessage = "por lomenos un tag es requerido")]
        [Display(Name = "Tags")]
        public int TagId { get; set; }

        public ICollection<Tag> Tags { get; set; }


    }
}