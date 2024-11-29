using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class InputConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InputConfigId { get; set; }

        [Display(Name = "Tipo de input")]
        [Required(ErrorMessage = "El tipo de input es requerido")]
        public int TipoInputId { get; set; }

        public TipoInput TipoInputs { get; set; }

        [Display(Name = "Tipo de data type")]
        [Required(ErrorMessage = "El tipo de data type es requerido")]
        public int DataTypeMIId { get; set; }

        public DataTypeM DataTypeMs { get; set; }

        [Display(Name = "Nombre")]
        [StringLength(50, ErrorMessage = "El nombre solo acepta 50 caracteres.")]
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; }

        [Display(Name = "Mascara")]
        [StringLength(50, ErrorMessage = "El nombre solo acepta 50 caracteres.")]
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Mask { get; set; }

        [Display(Name = "Ayuda")]
        public string Ayuda { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }
    }
}