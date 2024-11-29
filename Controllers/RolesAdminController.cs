using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Transparencia.Models;

namespace Transparencia.Controllers
{
    [Authorize]
    public class RolesAdminController : Controller
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                _userManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        // GET: RolesAdmin
        public ActionResult ListaRoles(FiltrosCatalogos filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            string[] searchstringsN = filtros.Nombre != null && filtros.Nombre.Length > 0 ? filtros.Nombre.Split(' ') : "".Split(' ');
            string[] searchstringsD = filtros.Descripcion != null && filtros.Descripcion.Length > 0 ? filtros.Descripcion.Split(' ') : "".Split(' ');

            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderDescripcion = sOrder == "Descripcion" ? "Descripcion_desc" : "Descripcion";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";

            IPagedList<ApplicationRole> vModel = null;

            var Db = new ApplicationDbContext();

            //var query2 = Db.Roles.Where(x =>
            //     ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.Name.ToLower().Contains(y.ToLower())))
            //     && ((filtros.Descripcion == null || filtros.Descripcion.Length == 0) || searchstringsD.Any(y => x.Descripcion.ToLower().Contains(y.ToLower())))
            //    );

            var query = RoleManager.Roles.Where(x =>
                ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.Name.ToLower().Contains(y.ToLower())))
                && ((filtros.Descripcion == null || filtros.Descripcion.Length == 0) || searchstringsD.Any(y => x.Descripcion.ToLower().Contains(y.ToLower())))
                );

            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.Name).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.Name).ToPagedList(iPagina, PerPage);
                    break;
                case "Descripcion":
                    vModel = query.OrderBy(x => x.Descripcion).ToPagedList(iPagina, PerPage);
                    break;
                case "Descripcion_desc":
                    vModel = query.OrderByDescending(x => x.Descripcion).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo":
                    vModel = query.OrderBy(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo_desc":
                    vModel = query.OrderByDescending(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                default:
                    vModel = query.OrderBy(x => x.Name).ToPagedList(iPagina, PerPage);
                    break;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("_listaRoles", vModel);
            }

            return View(vModel);
        }

        public ActionResult CrearRol()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CrearRol(RolViewModel rol)
        {
            if (ModelState.IsValid)
            {
                var rolNuevo = new ApplicationRole(rol.Name);

                rolNuevo.Descripcion = rol.Descripcion;
                if (!string.IsNullOrEmpty(rol.FechaDesde))
                {
                    rolNuevo.FechaDesde = DateTime.Parse(rol.FechaDesde);
                }
                if (!string.IsNullOrEmpty(rol.FechaHasta))
                {
                    rolNuevo.FechaHasta = DateTime.Parse(rol.FechaHasta);
                }
                rolNuevo.Activo = true;

                var result = await RoleManager.CreateAsync(rolNuevo);

                if (!result.Succeeded)
                {
                    AddErrors(result);
                    return View(rol);
                }
                return RedirectToAction("ListaRoles");
            }
            return View(rol);
        }

        public async Task<ActionResult> ModificarRol(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var rol = await RoleManager.FindByIdAsync(id);

            if (rol == null)
            {
                return HttpNotFound();
            }

            RolViewModel rolVM = new RolViewModel { Id = rol.Id, Name = rol.Name, Descripcion = rol.Descripcion, Activo = rol.Activo };
            rolVM.FechaDesde = rol.FechaDesde == null ? "" : ((DateTime)rol.FechaDesde).ToShortDateString();
            rolVM.FechaHasta = rol.FechaHasta == null ? "" : ((DateTime)rol.FechaHasta).ToShortDateString();

            return View(rolVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ModificarRol(RolViewModel rolEdit)
        {
            if (ModelState.IsValid)
            {
                var rol = await RoleManager.FindByIdAsync(rolEdit.Id);

                if (rol == null)
                {
                    return HttpNotFound();
                }

                rol.Activo = rolEdit.Activo;
                rol.Descripcion = rolEdit.Descripcion;
                rol.Name = rolEdit.Name;
                if (!string.IsNullOrEmpty(rolEdit.FechaDesde))
                {
                    rol.FechaDesde = DateTime.Parse(rolEdit.FechaDesde);
                }
                if (!string.IsNullOrEmpty(rolEdit.FechaHasta))
                {
                    rol.FechaHasta = DateTime.Parse(rolEdit.FechaHasta);
                }

                var result = await RoleManager.UpdateAsync(rol);
                if (!result.Succeeded)
                {
                    AddErrors(result);
                    return View(rolEdit);
                }

                return RedirectToAction("ListaRoles");
            }

            return View();

        }

        public async Task<ActionResult> DetallesRol(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var rol = await RoleManager.FindByIdAsync(id);

            return View(rol);
        }

        public async Task<ActionResult> EliminarRol(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var rol = await RoleManager.FindByIdAsync(id);

            return View(rol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("EliminarRol")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var rol = await RoleManager.FindByIdAsync(id);

            if (rol == null)
            {
                return HttpNotFound();
            }

            rol.Activo = false;

            var result = await RoleManager.UpdateAsync(rol);

            if (!result.Succeeded)
            {
                AddErrors(result);
                return View();
            }
            return RedirectToAction("ListaRoles");
        }

        public async Task<bool?> changeStatus(string id = "")
        {
            var activo = false;
            try
            {
                //var Model = db.UnidadesAdministrativas.Find(id);
                var rol = await RoleManager.FindByIdAsync(id);
                if (rol == null)
                {
                    return null;
                }
                if (rol.Activo)
                {
                    rol.Activo = activo = false;
                }
                else
                {
                    rol.Activo = activo = true;
                }
                await RoleManager.UpdateAsync(rol);
                //return View("Index",horariosModels);
                return activo;
            }
            catch (Exception)
            {

                return false;
            }

        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}