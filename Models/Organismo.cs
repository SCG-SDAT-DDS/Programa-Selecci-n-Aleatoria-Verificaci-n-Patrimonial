using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Organismo: ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name ="Id")]
        public int OrganismoID { get; set; }

        [Display(Name ="Organismo")]
        [Required(ErrorMessage = "El nombre del Organismo es requerido.")]
        [StringLength(90, ErrorMessage = "El nombre del Organismo no debe exceder los {1} caracteres.")]
        public string NombreOrganismo { get; set; }

        [Display(Name ="Tipo de Organismo")]
        public TipoOrganismo TipoOrganismo { get; set; }

        public int TipoOrganismoId { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La Descripción del Organismo es requerida.")]
        [StringLength(50, ErrorMessage = "La Descripción del Organismo no debe exceder los {1} caracteres.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Las Siglas del Organismo es requerida.")]
        [StringLength(10, ErrorMessage = "Las Siglas del Organismo no deben exceder los {1} caracteres.")]
        public string Siglas { get; set; }

        public Representante Representante { get; set; }

        [Display(Name = "Representante")]
        public int RepresentanteId { get; set; }

        public Pais Pais { get; set; }

        [Display(Name ="País")]
        public int? PaisId { get; set; }

        public Estado Estado { get; set; }

        [Display(Name = "Estado")]
        public int? EstadoId { get; set; }

        public Ciudad Ciudad { get; set; }

        [Display(Name = "Ciudad")]
        public int? CiudadId { get; set; }

        [Display(Name ="Dirección")]
        [Required(ErrorMessage = "La Dirección del Organismo es requerida.")]
        [StringLength(100, ErrorMessage = "La Dirección del Organismo no debe exceder los {1} caracteres.")]
        public string Direccion { get; set; }

        [Display(Name ="Código Postal")]
        [Required(ErrorMessage = "El Código Postal del Organismo es requerida.")]
        [StringLength(10, ErrorMessage = "El Código Postal del Organismo no debe exceder los {1} caracteres.")]
        public string CP { get; set; }

        [Display(Name ="Teléfono")]
        //[Required(ErrorMessage = "El Teléfono del Organismo es requerida.")]
        [StringLength(15, ErrorMessage = "El Teléfono del Organismo no debe exceder los {1} caracteres.")]
        public string Telefono { get; set; }

        [Display(Name = "Extensión")]
        [StringLength(10, ErrorMessage = "La Extensión del Organismo no debe exceder los {1} caracteres.")]
        public string Extension { get; set; }

        //[Required(ErrorMessage = "El URL del Organismo es requerido.")]
        [StringLength(250, ErrorMessage = "El URL del Organismo no debe exceder los {1} caracteres.")]
        [Url(ErrorMessage ="Dirección web inválida.")]
        public string URL { get; set; }

        [StringLength(100, ErrorMessage = "La ruta del archivo del Logo no debe exceder los {1} caracteres.")]
        public string Logo { get; set; }

        [StringLength(25, ErrorMessage = "La Latitud no debe exceder los {1} caracteres.")]
        public string Latitud { get; set; }

        [StringLength(100, ErrorMessage = "La Longitud no debe exceder los {1} caracteres.")]
        public string Longitud { get; set; }

        [StringLength(100, ErrorMessage = "El URL de Facebook no debe exceder los {1} caracteres.")]
        [Url(ErrorMessage = "Dirección web inválida.")]
        public string Facebook { get; set; }

        [StringLength(100, ErrorMessage = "El URL de Twitter no debe exceder los {1} caracteres.")]
        [Url(ErrorMessage = "Dirección web inválida.")]
        public string Twitter { get; set; }

        [StringLength(100, ErrorMessage = "El URL de Youtube no debe exceder los {1} caracteres.")]
        [Url(ErrorMessage = "Dirección web inválida.")]
        public string Youtube { get; set; }

        [StringLength(100, ErrorMessage = "El URL de Instagram no debe exceder los {1} caracteres.")]
        [Url(ErrorMessage = "Dirección web inválida.")]
        public string Instagram { get; set; }

        [StringLength(5000, ErrorMessage = "El URL del historico no debe exceder los {1} caracteres.")]
        [Url(ErrorMessage = "Url no valida.")]
        public string UrlHistorico { get; set; }

        [Required(ErrorMessage = "Este campo es requerido.")]
        [RegularExpression(@"^\d{1,2}$", ErrorMessage = "Solo se permiten valores numéricos.")]
        [Range(1, 99, ErrorMessage = "Solo se permiten valores numéricos entre 1 y 99.")]
        public int Orden { get; set; }

        public bool Activo { get; set; }

        public Organismo()
        {
            this.Activo = true;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }
}