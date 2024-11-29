using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Representante:ICloneable { 
    
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name="Id")]
        public int RepresentanteId { get; set; }

        [Display(Name ="Nombre Representante")]
        [Required(ErrorMessage = "El nombre del Representante es requerido.")]
        [StringLength(200, ErrorMessage = "El nombre del Representante no debe exceder los {1} caracteres.")]
        public string NombreCompleto { get; set; }

        [Display(Name = "Teléfono")]
        //[Required(ErrorMessage = "El Teléfono del Organismo es requerido.")]
        [StringLength(15, ErrorMessage = "El Teléfono no debe exceder los {1} caracteres.")]
        public string Telefono { get; set; }

        [Display(Name = "Extensión")]
        [StringLength(10, ErrorMessage = "La Extensión no debe exceder los {1} caracteres.")]
        public string Extension { get; set; }

        [Display(Name = "Título")]
        [Required(ErrorMessage = "El Título del Representante es requerido.")]
        [StringLength(100, ErrorMessage = "El Título del Representante no debe exceder los {1} caracteres.")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "El Puesto del Representante es requerido.")]
        [StringLength(100, ErrorMessage = "El Puesto del Representante no debe exceder los {1} caracteres.")]
        public string Puesto { get; set; }

        [Display(Name = "Url de DAP Sonora")]
        [StringLength(500, ErrorMessage = "El URL del Organismo no debe exceder los {1} caracteres.")]
        [Url(ErrorMessage = "Dirección web inválida.")]
        public string DAPUrl { get; set; }

        [Display(Name = "Correo electrónico")]
        [StringLength(500, ErrorMessage = "El Correo electrónico del Representante no debe exceder los {1} caracteres.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Este campo es requerido.")]
        [RegularExpression(@"^\d{1,2}$", ErrorMessage = "Solo se permiten valores numéricos.")]
        [Range(1, 99, ErrorMessage = "Solo se permiten valores numéricos entre 1 y 99.")]
        public int Orden { get; set; }

        public bool Activo { get; set; }

        public Representante()
        {
            this.Activo = true;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}