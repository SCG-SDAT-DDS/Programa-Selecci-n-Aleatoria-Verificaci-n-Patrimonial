using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class PlantillaFraccion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlantillaFraccionId { get; set; }

        [Display(Name = "Plantilla")]
        //[Required(ErrorMessage = "La plantilla es requerida")]
        public int? PlantillaId { get; set; }

        public Plantilla Plantillas { get; set; }

        [Display(Name = "Fracción")]
        //[Required(ErrorMessage = "La Fracción es requerida")]
        public int FraccionId { get; set; }

        public Fraccion Fracciones { get; set; }
    }
}