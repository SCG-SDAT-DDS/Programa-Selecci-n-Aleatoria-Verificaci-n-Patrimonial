using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Periodo: ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="Id")]
        public int PeriodoId { get; set; }

        [Display(Name = "Periodo")]
        [Required(ErrorMessage ="El Periodo es requerido.")]
        [StringLength(30, ErrorMessage = "El  Periodo no debe exceder los {1} caracteres.")]
        public string NombrePeriodo { get; set; }

        [Required(ErrorMessage = "Este campo es requerido.")]
        [RegularExpression(@"^\d{1,2}$", ErrorMessage = "Solo se permiten valores numéricos.")]
        [Range(1, 99, ErrorMessage = "Solo se permiten valores numéricos entre 1 y 99.")]
        public int Orden { get; set; }

        public bool Activo { get; set; }

        public virtual ICollection<Plantilla> Plantillas { get; set; }

        public Periodo()
        {
            this.Plantillas = new HashSet<Plantilla>();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}