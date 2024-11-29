using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class BitIniSesion
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id")]
        public int BitIniSesionId { get; set; }

        public string usuarioId { get; set; }

        [Display(Name = "user agent")]
        public string userAgent { get; set; }

        [Display(Name = "IP")]
        public string ip { get; set; }

        [Display(Name = "Navegador")]
        public string browser { get; set; }

        [Display(Name = "Version del navegador")]
        public string browserVersion { get; set; }

        [Display(Name = "Sistema operativo")]
        public string os { get; set; }

        [Display(Name = "Fecha")]
        public DateTime fecha { get; set; }

        [Display(Name = "Movil")]
        public bool movil { get; set; }

        [NotMapped]
        public string FechatoString
        {
            get => fecha.ToString("dd", CultureInfo.CreateSpecificCulture("es-MX")) + " de " +
                   fecha.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-MX")) + " del " +
                   fecha.ToString("yyyy", CultureInfo.CreateSpecificCulture("es-MX"))+  " <br/> "+fecha.ToString("hh:mm:ss tt");
        }

        [NotMapped]
        public string NombreUsuario
        {
            get => HMTLHelperExtensions.getNombreUsuarioById(this.usuarioId);
        }

        [NotMapped]
        public string LabelBroswer
        {
            get => $"<span class='badge badge-pill badge-primary'> {this.browser} / {this.browserVersion } </span>";
        }

        [NotMapped]
        public string LabelOS
        {
            get => $"<span class='badge badge-pill badge-success' > {this.os} </span>";
        }

    }
}