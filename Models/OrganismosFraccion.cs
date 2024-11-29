using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class OrganismosFraccion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrganismosFraccionId { get; set; }


        [Display(Name = "Fracción")]
        //[Required(ErrorMessage = "La Fracción es requerida")]
        public int FraccionId { get; set; }

        public Fraccion Fracciones { get; set; }

        [Display(Name = "Organismo")]
        //[Required(ErrorMessage = "La Fracción es requerida")]
        public int OrganismoID { get; set; }

        public Organismo Organismo { get; set; }
    }
}