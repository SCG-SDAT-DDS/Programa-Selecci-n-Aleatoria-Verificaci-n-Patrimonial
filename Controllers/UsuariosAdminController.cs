using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Transparencia.Models;

namespace Transparencia.Controllers
{
    [Authorize]
    public class UsuariosAdminController : Controller
    {
        private UsersEngine engine = new UsersEngine();
        private ApplicationDbContext db = new ApplicationDbContext();

        private int? GetOrganismoEnlace()
        {
            return  HMTLHelperExtensions.GetOrganissmoId(User.Identity.Name, "Enlace");
        }


        private bool CheckRolesEnlace()
        {
            return HMTLHelperExtensions.GetRoles(User.Identity.Name, "Enlace");
        }

        // GET: UsuariosAdmin
        public ActionResult ListaUsuarios(FiltrosUsuarios filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            string[] searchstringsN = filtros.Nombre != null && filtros.Nombre.Length > 0 ? filtros.Nombre.Split(' ') : "".Split(' ');
            string[] searchstringsU = filtros.Usuario != null && filtros.Usuario.Length > 0 ? filtros.Usuario.Split(' ') : "".Split(' ');
            string[] searchstringsE = filtros.email != null && filtros.email.Length > 0 ? filtros.email.Split(' ') : "".Split(' ');

            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderUsuario = sOrder == "Usuario" ? "Usuario_desc" : "Usuario";
            ViewBag.OrderEmail = sOrder == "Email" ? "Email_desc" : "Email";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";

            //si ees enlace solo podra ver los de su unidad administrativa

            var OrganismoID = GetOrganismoEnlace();



            IPagedList<ApplicationUser> vModel = null;

            var Db = new ApplicationDbContext();

            var query = Db.Users.Where(x =>
                 ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.NombreCompleto.ToLower().Contains(y.ToLower())))
                 && ((filtros.Usuario == null || filtros.Usuario.Length == 0) || searchstringsU.Any(y => x.UserName.ToLower().Contains(y.ToLower())))
                 && ( (filtros.email == null || filtros.email.Length == 0) || searchstringsE.Any(y => x.Email.ToLower().Contains(y.ToLower())) )
                 && ( (OrganismoID == null || OrganismoID == 0 ) ||  x.OrganismoID == OrganismoID )
                );

            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.NombreCompleto).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.NombreCompleto).ToPagedList(iPagina, PerPage);
                    break;
                case "Usuario":
                    vModel = query.OrderBy(x => x.UserName).ToPagedList(iPagina, PerPage);
                    break;
                case "Usuario_desc":
                    vModel = query.OrderByDescending(x => x.UserName).ToPagedList(iPagina, PerPage);
                    break;
                case "Email":
                    vModel = query.OrderBy(x => x.UserName).ToPagedList(iPagina, PerPage);
                    break;
                case "Email_desc":
                    vModel = query.OrderByDescending(x => x.UserName).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo":
                    vModel = query.OrderBy(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo_desc":
                    vModel = query.OrderByDescending(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                default:
                    vModel = query.OrderBy(x => x.NombreCompleto).ToPagedList(iPagina, PerPage);
                    break;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("_listaUsuarios", vModel);
            }
            return View(vModel);
        }


        public void campos()
        {
            ViewBag.TipoOrganismoId = db.TipoOrganismos.Where(x => x.Activo).ToList();
            ViewBag.TipoOrganismoIds = 0;
            ViewBag.OrganismoIDs = 0;
            ViewBag.OrganismoID = new List<Organismo>();
            ViewBag.lstUnidadesAdministrativas = new List<UnidadAdministrativa>();
            var esEnlace = CheckRolesEnlace();
            ViewBag.ListaRoles = engine.ListaRolesCombo(esEnlace);
            ViewBag.esEnlace = esEnlace;
            ViewBag.Organismo = HMTLHelperExtensions.GetOrganismoName(User.Identity.Name);
            ViewBag.lstUnidadesAdministrativas = engine.ListaUnidadAdministrativa(GetOrganismoEnlace());
            //if (esEnlace)
            //{
               
            //}
           
        }
        public ActionResult CrearUsuario()
        {
            
            campos();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CrearUsuario(Usuario model, string Rol)
        {
            if (ModelState.IsValid)
            {
                if (CheckRolesEnlace())
                {
                    model.OrganismoID = GetOrganismoEnlace();
                }

                model.UnidadAdministrativaId = model.UnidadAdministrativaId != null && model.UnidadAdministrativaId != 0 ?model.UnidadAdministrativaId : null;

                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    NombreCompleto = model.NombreCompleto,
                    Activo = true,
                    OrganismoID = model.OrganismoID,
                    UnidadAdministrativaId = model.UnidadAdministrativaId
                   
                };

                var result = await engine.UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //await engine.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    engine.AsignaRolAUsuario(user.Id, Rol);
                    //bitacora
                    this.CreateBitacora(null, user, 0);

                    return RedirectToAction("ListaUsuarios", "UsuariosAdmin");
                }

                AddErrors(result);
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            //ViewBag.ListaRoles = engine.ListaRolesCombo();
            campos();
            return View(model);
        }

        public async Task<ActionResult> ModificarUsuario(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await engine.UserManager.FindByIdAsync(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            campos();
            return View(DatosModificaUsuario(user));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ModificarUsuario(Usuario userEdit)
        {
            if (ModelState.IsValid)
            {
                var user = await engine.UserManager.FindByIdAsync(userEdit.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.Activo = userEdit.Activo;
                user.NombreCompleto = userEdit.NombreCompleto;
                user.Email = userEdit.Email;
                user.UserName = userEdit.UserName;

                if (db.Users.Where(x => x.UserName == userEdit.UserName && x.Id != user.Id).Any())
                {
                    campos();
                    ModelState.AddModelError("UserName", $"Ya existe un usuario con este nombre de usuario: '{ userEdit.UserName }', por favor escriba otro nombre de usuario.");
                    return View(DatosModificaUsuario(userEdit));
                }

                if (db.Users.Where(x=>x.Email == userEdit.Email && x.Id != user.Id).Any())
                {
                    campos();
                    ModelState.AddModelError("Email", $"Ya existe un usuario con este correo electrónico: '{ userEdit.Email }', por favor escriba otro correo electrónico.");
                    return View(DatosModificaUsuario(userEdit));
                }

                var result = await engine.UserManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    AddErrors(result);

                    return View(userEdit);
                }
                return RedirectToAction("ListaUsuarios");
            }
            campos();
            return View(userEdit);
        }

        [HttpPost]
        public async Task<PartialViewResult> ActualizaListaRoles(string id)
        {
            var user = await engine.UserManager.FindByIdAsync(id);
            var rolesList = engine.RoleManager.Roles.ToList()
                .Select(x => new SelectListItem()
                {
                    // Selected = roles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                }).OrderBy(x => x.Value);

            ViewBag.ListaRoles = rolesList;

            return PartialView("_RolesUsuario", DatosModificaUsuario(user));
        }

        public async Task<ActionResult> DetallesUsuario(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await engine.UserManager.FindByIdAsync(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            //return View(UserToUsuario(user));
            return View(DatosModificaUsuario(user));
        }

        public async Task<ActionResult> EliminarUsuario(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await engine.UserManager.FindByIdAsync(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(UserToUsuario(user));
        }

        [HttpPost, ActionName("EliminarUsuario")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await engine.UserManager.FindByIdAsync(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            user.Activo = false;

            var result = await engine.UserManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                AddErrors(result);

                return View();
            }
            return RedirectToAction("ListaUsuarios");
        }

        public async Task<bool?> changeStatus(string id = "")
        {
            var activo = false;
            try
            {
                //var Model = db.UnidadesAdministrativas.Find(id);
                var user = await engine.UserManager.FindByIdAsync(id);

               
                if (user == null)
                {
                    return null;
                }
                //bitacora
                ApplicationDbContext oldDb = new ApplicationDbContext();
                var oldModel = oldDb.Users.Find(id);
                if (user.Activo)
                {
                    user.Activo = activo = false;
                }
                else
                {
                    user.Activo = activo = true;
                }
                await engine.UserManager.UpdateAsync(user);

                //bitacora
                this.CreateBitacora(oldModel, oldModel,0);

            }
            catch (Exception)
            {

            }
            return activo;
        }

        //private List<ApplicationRole> GetUserRoles(string id)
        //{
        //    List<ApplicationRole> userRoles = new List<ApplicationRole>();

        //    var roles = UserManager.GetRoles(id);
        //    foreach (var item in roles)
        //    {
        //        var rol = RoleManager.FindByName(item);
        //        userRoles.Add(rol);
        //    }

        //    return userRoles;
        //}

        private ModificaUsuarioViewModel DatosModificaUsuario(ApplicationUser user)
        {
            ModificaUsuarioViewModel usuario = new ModificaUsuarioViewModel();
            usuario.Id = user.Id;
            usuario.NombreCompleto = user.NombreCompleto;
            usuario.UserName = user.UserName;
            usuario.Email = user.Email;
            usuario.Password = user.PasswordHash;
            usuario.ConfirmPassword = user.PasswordHash;
            usuario.ListaRoles = engine.GetUserRoles(user.Id);
            usuario.Activo = user.Activo;

            return usuario;
        }
        private ModificaUsuarioViewModel DatosModificaUsuario(Usuario user)
        {
            ModificaUsuarioViewModel usuario = new ModificaUsuarioViewModel();
            usuario.Id = user.Id;
            usuario.NombreCompleto = user.NombreCompleto;
            usuario.UserName = user.UserName;
            usuario.Email = user.Email;
            usuario.ListaRoles = engine.GetUserRoles(user.Id);
            usuario.Activo = user.Activo;

            return usuario;
        }

        private Usuario UserToUsuario(ApplicationUser user)
        {
            Usuario usuario = new Usuario();
            usuario.Id = user.Id;
            usuario.NombreCompleto = user.NombreCompleto;
            usuario.UserName = user.UserName;
            usuario.Email = user.Email;
            usuario.Password = user.PasswordHash;
            usuario.ConfirmPassword = user.PasswordHash;

            usuario.Activo = user.Activo;

            return usuario;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        public JsonResult AsignaRolAUsuario(string id, string nombreRol,int OrganismoID=0,int UnidadAdministrativaId=0)
        {
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("ModificarUsuario", "UsuariosAdmin", new { id = id });
           
            var respuesta = "ok";
            if (OrganismoID == 0 && nombreRol == "Enlace")
            {
                respuesta = "falloOrganismo";
                return Json(new { Url = redirectUrl, status = respuesta }, JsonRequestBehavior.AllowGet);
            }
            if (UnidadAdministrativaId == 0 && nombreRol == "Unidad Administrativa")
            {
                respuesta = "falloUnidadAdministrativa";
                return Json(new { Url = redirectUrl, status = respuesta }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(nombreRol))
            {
                respuesta = "fallo";
                return Json(new { Url = redirectUrl, status = respuesta }, JsonRequestBehavior.AllowGet);
            }

            if (nombreRol == "0")
            {
                respuesta = "noexisterol";
                return Json(new { Url = redirectUrl, status = respuesta }, JsonRequestBehavior.AllowGet);
            }
            //buscamos el usuario para bitacoras
            ApplicationDbContext dbOld = new ApplicationDbContext();
            var oldModel = dbOld.Users.Where(x => x.Id == id).FirstOrDefault();

            //agremamos el rol
            var oldRoles = engine.UserManager.GetRoles(id).ToArray();
            if (!engine.UserManager.IsInRole(id, nombreRol))
            {
                var result = engine.UserManager.AddToRole(id, nombreRol);
                if (!result.Succeeded)
                {
                    respuesta = "fallo";
                    return Json(new { Url = redirectUrl, status = respuesta }, JsonRequestBehavior.AllowGet);
                }

                //removemos los antiguos roles
                result = engine.UserManager.RemoveFromRoles(id,oldRoles);
                if (!result.Succeeded)
                {
                    respuesta = "fallo";
                    return Json(new { Url = redirectUrl, status = respuesta }, JsonRequestBehavior.AllowGet);
                }

            }
            

            
            try
            {
                if (OrganismoID != 0 && nombreRol == "Enlace")
                {
                    var user = db.Users.Where(x => x.Id == id).FirstOrDefault();
                    if (user != null)
                    {
                        user.OrganismoID = OrganismoID;
                        user.UnidadAdministrativaId = null;
                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }else if (UnidadAdministrativaId != 0 && nombreRol == "Unidad Administrativa")
                {
                    var user = db.Users.Where(x => x.Id == id).FirstOrDefault();
                    if (user != null)
                    {
                        user.UnidadAdministrativaId = UnidadAdministrativaId;
                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                //bitacora
                var newuser = db.Users.Where(x => x.Id == id).FirstOrDefault();
                this.CreateBitacora(oldModel, newuser, 0);

            }
            catch (Exception ex)
            {

            }
            //}

            return Json(new {Url=redirectUrl, status= respuesta }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EliminaRolAUsuario(string usuarioId, string nombreRol)
        {
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("ModificarUsuario", "UsuariosAdmin", new { id = usuarioId });
            var respuesta = "ok";
          
            if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(nombreRol))
            {
                respuesta = "fallo";
            }

            if (engine.UserManager.IsInRole(usuarioId, nombreRol))
            {
                if(!ValidaRolAdministrador(usuarioId, nombreRol))
                {
                    return Json("admin", JsonRequestBehavior.AllowGet);
                }
                var totalRoles = engine.UserManager.GetRoles(usuarioId).Count();
                //var result = new IdentityResult();
                //if (totalRoles > 1)
                //{
                //    result = engine.UserManager.RemoveFromRole(usuarioId, nombreRol);
                //}
                //else
                //{
                //    return Json(new { Url = redirectUrl, status = "fallo" }, JsonRequestBehavior.AllowGet);
                //}
                

                //if (!result.Succeeded)
                //{
                //    respuesta = "fallo";
                //}
                //else
                //{
                    if(nombreRol == "Enlace")
                    {
                        try
                        {
                            var user = db.Users.Where(x => x.Id == usuarioId).FirstOrDefault();
                            if (user != null)
                            {
                                user.OrganismoID = null;
                                db.Entry(user).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    if (nombreRol == "Unidad Administrativa")
                    {
                        try
                        {
                            var user = db.Users.Where(x => x.Id == usuarioId).FirstOrDefault();
                            if (user != null)
                            {
                                user.UnidadAdministrativaId = null;
                                db.Entry(user).State = EntityState.Modified;
                                db.SaveChanges();

                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                //}
            }
            else
            {
                respuesta = "noexiste";
            }

            return Json(new { Url = redirectUrl, status = respuesta }, JsonRequestBehavior.AllowGet);
            //return Json(respuesta, JsonRequestBehavior.AllowGet);
        }

        

        private bool ValidaRolAdministrador(string usuarioId, string nombreRol)
        {
            bool valida = true;
            var user = engine.UserManager.FindById(usuarioId);

            if (user != null)
            {
                if (user.UserName.ToLower() == "administrador" && nombreRol == "Administrador")
                {
                    return false;
                }
            }

            return valida;
        }

        //Bitacora
        private void CreateBitacora(ApplicationUser oldModel, ApplicationUser newModel, int id = 0)
        {
            var cambioCampos = new List<cambiosCampos>();
            int accion = oldModel != null ? 2 : 1;
            //Creamos la bitacora
            try
            {
                //Nombre Completo
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Nombre Completo",
                    es_modificado = oldModel != null && oldModel.NombreCompleto != newModel.NombreCompleto ? true : false,
                    campo_nuevo = newModel.NombreCompleto,
                    campo_anterior = oldModel != null ? oldModel.NombreCompleto : null
                });

                //UserName
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Orden",
                    es_modificado = oldModel != null && oldModel.UserName != newModel.UserName ? true : false,
                    campo_nuevo = newModel.UserName,
                    campo_anterior = oldModel != null ? oldModel.UserName : null
                });

                //Email
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Correo Electrónico",
                    es_modificado = oldModel != null && oldModel.Email != newModel.Email ? true : false,
                    campo_nuevo = newModel.Email,
                    campo_anterior = oldModel != null ? oldModel.Email : null
                });

                //Roles
                string newRol = "";
                bool newFirst = true;
                string oldRol = "";
                bool oldFirst = true;
                if (newModel != null && newModel.Roles != null && newModel.Roles.Count > 0)
                {
                    foreach (var item in newModel.Roles)
                    {
                        string nombreRol = "";
                        if (item.RoleId != null && item.RoleId != "")
                        {
                            nombreRol = db.Roles.Where(x => x.Id == item.RoleId).FirstOrDefault()?.Name;
                        }
                        newRol += !newFirst ? ", " : "";
                        newRol += $@"{nombreRol}";
                        newFirst = false;
                    }
                }

                if (oldModel != null && oldModel.Roles != null && oldModel.Roles.Count > 0)
                {
                    foreach (var item in oldModel.Roles)
                    {
                        string nombreRol = "";
                        if (item.RoleId != null && item.RoleId != "")
                        {
                            nombreRol = db.Roles.Where(x => x.Id == item.RoleId).FirstOrDefault()?.Name;
                        }
                        oldRol += !oldFirst ? ", " : "";
                        oldRol += $@"{nombreRol}";
                        oldFirst = false;
                    }
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Rol",
                    es_modificado = oldModel != null && oldRol != newRol ? true : false,
                    campo_nuevo = newRol,
                    campo_anterior = oldRol
                });

                //Organismo
                var oldOrganismoHasValue = oldModel != null && oldModel.OrganismoID != null && oldModel.OrganismoID != 0 ? true : false;
                var newOrganismoHasValue = newModel != null && newModel.OrganismoID != null && newModel.OrganismoID != 0 ? true : false;

                if(oldOrganismoHasValue || newOrganismoHasValue)
                {
                    //Organismo
                    string newOrganismo = "";
                    string oldOrganismo = "";

                    if (newModel != null && newModel.OrganismoID != 0)
                    {
                        newOrganismo = db.Organismos.Where(x => x.OrganismoID == newModel.OrganismoID).FirstOrDefault()?.NombreOrganismo;
                    }
                    if (oldModel != null && oldModel.OrganismoID != 0)
                    {
                        oldOrganismo = db.Organismos.Where(x => x.OrganismoID == oldModel.OrganismoID).FirstOrDefault()?.NombreOrganismo;
                    }
                    cambioCampos.Add(new cambiosCampos
                    {
                        nombre_campo = "Organismo",
                        es_modificado = oldModel != null && oldModel.OrganismoID != newModel.OrganismoID ? true : false,
                        campo_nuevo = newOrganismo,
                        campo_anterior = oldOrganismo
                    });
                }

                //Unidad Administrativa
                var oldUAHasValue = oldModel != null && oldModel.UnidadAdministrativaId != null && oldModel.UnidadAdministrativaId != 0 ? true : false;
                var newUAHasValue = newModel != null && newModel.UnidadAdministrativaId != null && newModel.UnidadAdministrativaId != 0 ? true : false;

                if (oldUAHasValue || newUAHasValue)
                {
                    //Ley
                    string newUA = "";
                    string oldUA = "";

                    if (newModel != null && newModel.UnidadAdministrativaId != 0)
                    {
                        newUA = db.UnidadesAdministrativas.Where(x => x.UnidadAdministrativaId == newModel.UnidadAdministrativaId).FirstOrDefault()?.NombreUnidad;
                    }
                    if (oldModel != null && oldModel.OrganismoID != 0)
                    {
                        oldUA = db.UnidadesAdministrativas.Where(x => x.UnidadAdministrativaId == oldModel.UnidadAdministrativaId).FirstOrDefault()?.NombreUnidad;
                    }
                    cambioCampos.Add(new cambiosCampos
                    {
                        nombre_campo = "unidad Administrativa",
                        es_modificado = oldModel != null && oldModel.UnidadAdministrativaId != newModel.UnidadAdministrativaId ? true : false,
                        campo_nuevo = newUA,
                        campo_anterior = oldUA
                    });
                }



                //Activo
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Activo",
                    es_modificado = oldModel != null && oldModel.Activo != newModel?.Activo ? true : false,
                    campo_nuevo = HMTLHelperExtensions.getStringBoolBitacora(newModel.Activo),
                    campo_anterior = oldModel != null ? HMTLHelperExtensions.getStringBoolBitacora(oldModel.Activo) : null
                });

                //Bitacora
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "AspNetUsers", "Usuarios", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}