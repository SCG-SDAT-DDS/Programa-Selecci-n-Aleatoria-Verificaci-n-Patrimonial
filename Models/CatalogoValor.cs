using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class CatalogoValor
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id de catalogo")]
        public int CatalogoValorId { get; set; }
        
        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Catalogo")]
        public int CatalogoId { get; set; }

        [Display(Name = "Catalogo")]
        public virtual Catalogo Catalogo { get; set; }

        [StringLength(150, ErrorMessage = "El valor solo acepta 50 caracteres.")]
        [Required(ErrorMessage = "El valor es requerido")]
        [Display(Name = "Valor")]
        public string valor { get; set; }

        [Display(Name = "Orden")]
        public int orden { get; set; }

        [Display(Name = "Estatus")]
        public bool Activo { get; set; }

        [Display(Name = "Pertenece a")]
        public virtual ICollection<CatalogoValor> RelatedCatalogoValor { get; set; }
        public virtual ICollection<CatalogoValor> RelatedCatalogoValor2 { get; set; }


        public CatalogoValor()
        {
            this.RelatedCatalogoValor = new HashSet<CatalogoValor>();
            Activo = true;
        }
    }
}