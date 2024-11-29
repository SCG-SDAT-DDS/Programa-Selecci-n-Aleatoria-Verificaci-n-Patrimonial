using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class PlantillaUnidadAdministrativa
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlantillaUnidadAdministrativaId { get; set; }

        [Display(Name = "Plantilla")]
        [Required(ErrorMessage = "La plantilla es requerida")]
        public int PlantillaId { get; set; }

        public Plantilla Plantillas { get; set; }

        [Display(Name = "Unidad Administrativa")]
        [Required(ErrorMessage = "La Unidad Administrativa es requerido")]
        public int UnidadAdministrativaId { get; set; }

        public UnidadAdministrativa Organismos { get; set; }
    }
}