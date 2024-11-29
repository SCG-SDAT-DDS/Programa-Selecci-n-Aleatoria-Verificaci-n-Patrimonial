using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class RecuperarContrasenaViewModel
    {
        [Required]
        [Display(Name = "Usuario id")]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Key")]
        public string Key { get; set; }
        
        [Display(Name = "Información")]
        public string Iformacion { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de largo.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirme la nueva contraseña")]
        [Compare("NewPassword", ErrorMessage = "La nueva contraseña y la confirmación de la contraseña no coinciden.")]
        public string ConfirmPassword { get; set; }
    }
}