using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class ImportacionMasivo : ICloneable
    {
        //int GlobaliPlanntillaId = 17;
        //int GlobaliOrganismoId = 63;
        //int GlobaliPeriodoId = 4;
        //int GlobalisysFrecuencia = 3;
        //int GlobalisysNumFrecuencia = 2;
        //string GlobaliUsuarioId = "ba3ff24a-8f60-4461-bae7-342818133a2e";
        //bool GlobaliRemplazar = true;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImportacionMasivoId { get; set; }

        [Display(Name = "Plantilla id")]
        [Required(ErrorMessage = "La plantilla es requerido")]
        public int PlanntillaId { get; set; }

        [Display(Name = "Orgaismo id")]
        [Required(ErrorMessage = "El organismo es requerido")]
        public int OrganismoId { get; set; }

        [Display(Name = "Periodo id")]
        [Required(ErrorMessage = "El periodo es requerido")]
        public int PeriodoId { get; set; }

        [Display(Name = "Frecuencia id")]
        [Required(ErrorMessage = "El Frecuencia es requerido")]
        public int sysFrecuencia { get; set; }

        [Display(Name = "Numero Frecuecia id")]
        [Required(ErrorMessage = "El numero de frecuencia es requerido")]
        public int sysNumFrecuencia { get; set; }

        [Display(Name = "Usuario")]
        [Required(ErrorMessage = "El usuario es requerido")]
        public string UsuarioId { get; set; }

        [Display(Name = "Remplazar")]
        [Required(ErrorMessage = "El usuario es requerido")]
        public bool Remplazar { get; set; }

        public bool Activo { get; set; }

        public DateTime Fecha { get; set; }

        public string documento { get; set; }

        public string documentoError { get; set; }

        public EstatusImportacio status { get; set; }

        public ImportacionMasivo()
        {
            this.Activo = true;
            this.Fecha = DateTime.Now;
            this.status = 0;
        }
        [NotMapped]
        public string fechaFromated
        {
            get => Fecha.ToString("dd", CultureInfo.CreateSpecificCulture("es-MX")) + " de " +
                   Fecha.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-MX")) + " del " +
                   Fecha.ToString("yyyy", CultureInfo.CreateSpecificCulture("es-MX"));
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}