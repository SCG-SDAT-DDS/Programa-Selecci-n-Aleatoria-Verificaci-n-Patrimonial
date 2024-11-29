using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Bitacora
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int BitacoraId { get; set; }

        public string usuarioId { get; set; }

        public string tabla { get; set; }

        public string tablaPublico { get; set; }

        public int registroId { get; set; }

        public bool? nuevo { get; set; }

        public bool? editar { get; set; }

        public bool? eliminar { get; set; }

        public bool? otro { get; set; }

        public string cambios { get; set; }

        public DateTime fechaCreacion { get; set; }


        [NotMapped]
        public string FechatoString
        {
            get => fechaCreacion.ToString("dd", CultureInfo.CreateSpecificCulture("es-MX")) + " de " +
                   fechaCreacion.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-MX")) + " del " +
                   fechaCreacion.ToString("yyyy", CultureInfo.CreateSpecificCulture("es-MX")) + " <br/> " + fechaCreacion.ToString("hh:mm:ss tt");
        }

        [NotMapped]
        public string NombreUsuario
        {
            get => HMTLHelperExtensions.getNombreUsuarioById(this.usuarioId);
        }

        [NotMapped]
        public string Accion
        {
            get => nuevo.HasValue && nuevo.Value ? $"<span class='badge badge-pill badge-primary'> Nuevo registro </span>" : 
                editar.HasValue && editar.Value ? $"<span class='badge badge-pill badge-warning'> Modificación de registro </span>":
                eliminar.HasValue && eliminar.Value ? $"<span class='badge badge-pill badge-danger'> Eliminación de registro </span>" :
                otro.HasValue && otro.Value ? $"<span class='badge badge-pill badge-info'> Activación/Desactivación de registro </span>" : "<span class='badge badge-pill badge-secondary'> No especificado </span>";
        }


    }

    public class cambiosCampos
    {
        public string campo_nuevo { get; set;  }
        public string nombre_campo { get; set; }
        public string campo_anterior { get; set; }
        public bool es_modificado { get; set; }
        public bool link { get; set; }
        public bool file { get; set; }
    }
}