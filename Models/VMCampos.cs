using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class VMCampos
    {
        public int TablaFisicaId { get; set; }

        public DateTime? FechaCreacion { get; set; }
        public string SFechaCreacion { get; set; }

        public bool? Activo { get; set; }

        public string DatoPrincipal { get; set; }

        public string TablaNombre { get; set; }

        public int? CatalogoId { get; set; }

        public string UsuarioId { get; set; }

        public string NombreUsuario { get; set; }
    }
}