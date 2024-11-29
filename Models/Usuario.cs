using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class Usuario : RegisterViewModel 
    {
        public Usuario() { }

        public Usuario(ApplicationUser user)
        {
            this.Id = user.Id;
            this.UserName = user.UserName;
            this.Email = user.Email;
            this.Activo = user.Activo;
            this.NombreCompleto = user.NombreCompleto;
           
        }

        public string Id { get; set; }

        [Required]
        [Display(Name = "Usuario")]
        public string UserName { get; set; }
        //[Required]
        //[StringLength(100, ErrorMessage = "El número de caracteres de {0} debe ser al menos {10}.")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; }

        public bool Activo { get; set; }

        [Display(Name = "Organimos")]
        public int? OrganismoID { get; set; }

        public virtual Organismo Organismo { get; set; }

        public int? UnidadAdministrativaId { get; set; }


        public virtual UnidadAdministrativa UnidadAdministrativa { get; set; }

        public List<ApplicationRole> ListaRoles { get; set; }



        public ApplicationUser GetUser()
        {
            var user = new ApplicationUser()
            {
                UserName = this.UserName,
                Email = this.Email,
                NombreCompleto = this.NombreCompleto,
                Activo = this.Activo
            };
            return user;
        }
    }

    public class ModificaUsuarioViewModel: Usuario
    {
        public List<ApplicationRole> ListaRoles { get; set; }
    }
}