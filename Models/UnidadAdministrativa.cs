using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class UnidadAdministrativa: ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UnidadAdministrativaId { get; set; }

        [Display(Name = "Organismo")]
        public int OrganismoId { get; set; }

        public Organismo Organismos { get; set; }

        [Display(Name="Nombre Unidad Administrativa")]
        [Required(ErrorMessage = "El nombre de la Unidad Administrativa es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre de la Unidad Administrativa no debe exceder los {1} caracteres.")]
        public string NombreUnidad { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La Descripción es requerida.")]
        [StringLength(200, ErrorMessage = "La Descripción no debe exceder los {1} caracteres.")]
        public string Descripcion { get; set; }

        [StringLength(10, ErrorMessage = "Las Siglas de la Unidad Administrativa no deben exceder los {1} caracteres.")]
        public string Siglas { get; set; }

        [Required(ErrorMessage = "Este campo es requerido.")]
        [RegularExpression(@"^\d{1,2}$", ErrorMessage = "Solo se permiten valores numéricos.")]
        [Range(1, 99, ErrorMessage = "Solo se permiten valores numéricos entre 1 y 99.")]
        public int Orden { get; set; }

        public bool Activo { get; set; }

        public UnidadAdministrativa()
        {
            this.Activo = true;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}