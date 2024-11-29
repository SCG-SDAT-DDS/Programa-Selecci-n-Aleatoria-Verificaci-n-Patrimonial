using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class PlantillaOrganismos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlantillaOrganismosId { get; set; }

        [Display(Name = "Plantilla")]
        [Required(ErrorMessage = "La plantilla es requerida")]
        public int PlantillaId { get; set; }

        public Plantilla Plantillas { get; set; }

        [Display(Name = "Organismo")]
        [Required(ErrorMessage = "El organismo es requerido")]
        public int OrganismoID { get; set; }

        public Organismo Organismos { get; set; }
    }
}