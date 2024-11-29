using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Net;
using System.Web.Mvc;
using Transparencia.Models;


namespace Transparencia.Controllers
{
    public sealed class UsersEngine
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            }
            set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                _userManager = value;
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
            }
            set
            {
                _roleManager = value;
            }
        }

        public List<SelectListItem> ListaUnidadAdministrativa(int? OrganismoId = 0)
        {
            var rolesList = new List<SelectListItem>();


                rolesList.Add(new SelectListItem { Text = "Seleccione...", Value = "0" });

            if(OrganismoId != 0  && OrganismoId != null)
            {
                var UnidadAdministrativa = db.UnidadesAdministrativas.Where(x => x.OrganismoId == OrganismoId && x.Activo).ToList();
                foreach (var unidad in UnidadAdministrativa)
                {
                    var item = new SelectListItem();
                    item.Text = unidad.NombreUnidad;
                    item.Value = unidad.UnidadAdministrativaId.ToString();

                    rolesList.Add(item);
                }
            }
            
            return rolesList;
        }

        public List<SelectListItem> ListaRolesCombo(bool esEnlace=false)
        {
            var rolesList = new List<SelectListItem>();

            if (esEnlace)
            {
                var roles = RoleManager.Roles.Where(x => x.Name == "Unidad Administrativa" && x.Activo).FirstOrDefault();
                if(roles != null)
                {
                    var item = new SelectListItem();
                    item.Text = roles?.Name;
                    item.Value = roles?.Name;
                    rolesList.Add(item);
                }
            }
            else
            {

                rolesList.Add(new SelectListItem { Text = "Seleccione...", Value = "0" });

                foreach (var rol in RoleManager.Roles.Where(x=>x.Activo))
                {
                    var item = new SelectListItem();
                    item.Text = rol.Name;
                    item.Value = rol.Name;

                    rolesList.Add(item);
                }

            }



            //    RoleManager.Roles.ToList()
            //.Select(x => new SelectListItem()
            //{
            //    // Selected = roles.Contains(x.Name),
            //    Text = x.Name,
            //    Value = x.Name
            //}).OrderBy(x => x.Value);

            return rolesList;
        }

        public string AsignaRolAUsuario(string id, string nombreRol)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(nombreRol))
            {
                return "fallo";
            }

            if (nombreRol == "0")
            {
                return "noexisterol";
            }


            if (UserManager.IsInRole(id, nombreRol))
            {
                return "existe";
            }
            else
            {
                var result = UserManager.AddToRole(id, nombreRol);

                if (!result.Succeeded)
                {
                    return "falloasignacion";
                }
            }

            return "ok";
        }

        public List<ApplicationRole> GetUserRoles(string userId)
        {
            List<ApplicationRole> userRoles = new List<ApplicationRole>();

            var roles = UserManager.GetRoles(userId);
            foreach (var item in roles)
            {
                var rol = RoleManager.FindByName(item);
                if(rol.Name == "Enlace" || rol.Name == "Unidad Administrativa")
                {
                    var usuario = db.Users.Where(x => x.Id == userId).FirstOrDefault();
                    rol.NombreOrganismo = usuario?.Organismo?.NombreOrganismo == "" ? "<span class='text-danger'>No Especificado</span>": usuario?.Organismo?.NombreOrganismo;
                    rol.NombreUnidadAdministrativa = usuario?.UnidadAdministrativa?.NombreUnidad == null ? "<span class='text-danger'>No Especificado</span>" : usuario?.UnidadAdministrativa?.NombreUnidad;
                }
                userRoles.Add(rol);
            }

            return userRoles;
        }
    }
}