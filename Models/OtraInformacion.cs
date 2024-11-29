using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class OtraInformacion : ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OtraInformacionId { get; set; }

        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; }

        [Display(Name = "Organismo")]
        [Required(ErrorMessage = "El organismo es requerido")]
        public int OrganismoID { get; set; }
        public Organismo Orgaismo { get; set; }

        [Display(Name = "Tipo")]
        [Required(ErrorMessage = "El tipo es requerido")]
        public int TipoOtraInformacionId { get; set; }
        public TipoOtraInformacion TipoOtraInformacion { get; set; }

        [Display(Name = "URL")]
        [Required(ErrorMessage = "La URL es requerida")]
        public string URL { get; set; }

        [Display(Name = "Contenido")]
        public string Notas { get; set; }

        public bool Activo { get; set; }

        public OtraInformacion()
        {
            this.Activo = true;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}