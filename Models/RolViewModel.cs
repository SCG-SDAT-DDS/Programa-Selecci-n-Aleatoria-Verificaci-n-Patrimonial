using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class RolViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Rol")]
        public string Name { get; set; }
        [Display(Name = "Descripción")]
        public virtual string Descripcion { get; set; }

        [Display(Name = "Desde")]
        public virtual string FechaDesde { get; set; }

        [Display(Name = "Hasta")]
        public virtual string FechaHasta { get; set; }

        public virtual bool Activo { get; set; }
    }
}