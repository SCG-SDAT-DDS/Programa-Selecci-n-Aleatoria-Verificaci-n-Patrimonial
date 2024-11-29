using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Transparencia.Models;

namespace Transparencia.Controllers
{
    [Authorize]
    public class GrupoExtensionesController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        public bool? changeStatus(int id = 0)
        {
            var activo = false;
            if (id == 0)
            {
                return null;
            }
            try
            {
                var Model = db.GrupoExtesiones.Where(x=>x.GrupoExtensionId == id).Include(y => y.Extensiones).FirstOrDefault();
                //bitacora
                ApplicationDbContext olddb = new ApplicationDbContext();
                var oldModel = olddb.GrupoExtesiones.Where(x => x.GrupoExtensionId == id).Include(y => y.Extensiones).FirstOrDefault();
                if (Model == null)
                {
                    return null;
                }
                if (Model.Activo)
                {
                    Model.Activo = activo = false;
                }
                else
                {
                    Model.Activo = activo = true;
                }
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(oldModel, Model, Model.GrupoExtensionId);

            }
            catch (Exception)
            {

            }

            return activo;
        }
        // GET: GrupoExtensiones
        public ActionResult Index(FiltrosCatalogos filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            string[] searchstringsN = !string.IsNullOrEmpty(filtros.Nombre) ? filtros.Nombre.Split(' ') : "".Split(' ');
            string[] searchstringsD = !string.IsNullOrEmpty(filtros.Descripcion) ? filtros.Descripcion.Split(' ') : "".Split(' ');

            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderDescripcion = sOrder == "Descripcion" ? "Descripcion_desc" : "Descripcion";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";
            IPagedList<GrupoExtension> vModel = null;

            var query = db.GrupoExtesiones.Where(x =>
                 ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.Nombre.ToLower().Contains(y.ToLower())))
                 && ((filtros.Descripcion == null || filtros.Descripcion.Length == 0) || searchstringsD.Any(y => x.Descripcion.ToLower().Contains(y.ToLower())))
                 );
            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.Nombre).ToPagedList(iPagina, PerPage);
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
                    vModel = query.OrderBy(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_listaGrupoExtension", vModel);
            }

            return View(vModel);
        }

        // GET: GrupoExtensiones/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GrupoExtension grupoExtension = db.GrupoExtesiones.Find(id);
            if (grupoExtension == null)
            {
                return HttpNotFound();
            }
            return View(grupoExtension);
        }

        // GET: GrupoExtensiones/Create
        public ActionResult Create()
        {
            GrupoExtension grupoExtension = new GrupoExtension();
            var extensiones = db.Extensiones.Where(x => x.Activo).ToList();

            ViewBag.Extensiones = extensiones.Select(o => new SelectListItem
            {
                Text = o.Nombre,
                Value = o.ExtensionId.ToString()
            });
            return View();
        }

        // POST: GrupoExtensiones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GrupoExtensionesViewModel GrupoExtensionesViewModel)
        {
            if (ModelState.IsValid)
            {
                var updatedJobTags = new HashSet<int>(GrupoExtensionesViewModel.SelectedExtensions);

                foreach (Extension extension in db.Extensiones.Where(x => x.Activo))
                {
                    if (!updatedJobTags.Contains(extension.ExtensionId))
                    {
                        GrupoExtensionesViewModel.GrupoExtension.Extensiones.Remove(extension);
                    }
                    else
                    {
                        GrupoExtensionesViewModel.GrupoExtension.Extensiones.Add((extension));
                    }
                }


                db.GrupoExtesiones.Add(GrupoExtensionesViewModel.GrupoExtension);
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(null, GrupoExtensionesViewModel.GrupoExtension, GrupoExtensionesViewModel.GrupoExtension.GrupoExtensionId);

                return RedirectToAction("Index");
            }

            var extensiones = db.Extensiones.Where(x => x.Activo).ToList();
            ViewBag.Extensiones = extensiones.Select(o => new SelectListItem
            {
                Text = o.Nombre,
                Value = o.ExtensionId.ToString()
            });

            return View(GrupoExtensionesViewModel);
        }

        // GET: GrupoExtensiones/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var grupoExtension = new GrupoExtensionesViewModel
            {
                GrupoExtension = db.GrupoExtesiones.Include(i => i.Extensiones).First(i => i.GrupoExtensionId == id),
            };
            if (grupoExtension.GrupoExtension == null)
            {
                return HttpNotFound();
            }

            var allExtensionesList = db.Extensiones.Where(x => x.Activo).ToList();
            grupoExtension.AllExtensions = allExtensionesList.Select(o => new SelectListItem
            {
                Text = o.Nombre,
                Value = o.ExtensionId.ToString()
            });

            return View(grupoExtension);
        }

        // POST: GrupoExtensiones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GrupoExtensionesViewModel GrupoExtensionesViewModel)
        {
            if (GrupoExtensionesViewModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (ModelState.IsValid)
            {
                //para la bitacora
                ApplicationDbContext dbOld = new ApplicationDbContext();
                var oldModel = dbOld.GrupoExtesiones.Where(x => x.GrupoExtensionId == GrupoExtensionesViewModel.GrupoExtension.GrupoExtensionId).Include(x=>x.Extensiones).FirstOrDefault();

                var grupoExtensiones = db.GrupoExtesiones.Include(i => i.Extensiones).First(i => i.GrupoExtensionId == GrupoExtensionesViewModel.GrupoExtension.GrupoExtensionId);
                if (TryUpdateModel(grupoExtensiones, "GrupoExtension", new string[] { "Nombre", "Descripcion", "Activo" }))
                {
                    var newExtensions = db.Extensiones.Where(m => GrupoExtensionesViewModel.SelectedExtensions.Contains(m.ExtensionId)).ToList();
                    var updatedTags = new HashSet<int>(GrupoExtensionesViewModel.SelectedExtensions);

                    foreach (Extension extension in db.Extensiones)
                    {
                        if (!updatedTags.Contains(extension.ExtensionId))
                        {
                            grupoExtensiones.Extensiones.Remove(extension);
                        }
                        else
                        {
                            grupoExtensiones.Extensiones.Add(extension);
                        }
                    }
                    db.Entry(grupoExtensiones).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    
                    //bitacora
                    this.CreateBitacora(oldModel, grupoExtensiones, grupoExtensiones.GrupoExtensionId);


                }

                return RedirectToAction("Index");
            }

            GrupoExtensionesViewModel.GrupoExtension = db.GrupoExtesiones.Include(i => i.Extensiones)
                .First(i => i.GrupoExtensionId == GrupoExtensionesViewModel.GrupoExtension.GrupoExtensionId);

            var allExtensionesList = db.Extensiones.Where(x => x.Activo).ToList();
            GrupoExtensionesViewModel.AllExtensions = allExtensionesList.Select(o => new SelectListItem
            {
                Text = o.Nombre,
                Value = o.ExtensionId.ToString()
            });

            return View(GrupoExtensionesViewModel);
        }

        //// GET: GrupoExtensiones/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    GrupoExtension grupoExtension = db.GrupoExtesiones.Find(id);
        //    if (grupoExtension == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(grupoExtension);
        //}

        //// POST: GrupoExtensiones/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    GrupoExtension grupoExtension = db.GrupoExtesiones.Find(id);
        //    grupoExtension.Activo = false;
        //    db.Entry(grupoExtension).State = EntityState.Modified;
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        //Bitacora
        private void CreateBitacora(GrupoExtension oldModel, GrupoExtension newModel, int id = 0)
        {
            var cambioCampos = new List<cambiosCampos>();
            int accion = oldModel != null ? 2 : 1;
            //Creamos la bitacora
            try
            {
                //Nombre
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Nombre",
                    es_modificado = oldModel != null && oldModel.Nombre != newModel.Nombre ? true : false,
                    campo_nuevo = newModel.Nombre,
                    campo_anterior = oldModel != null ? oldModel.Nombre : null
                });
                //Descripción
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Descripción",
                    es_modificado = oldModel != null && oldModel.Descripcion != newModel.Descripcion ? true : false,
                    campo_nuevo = newModel.Descripcion,
                    campo_anterior = oldModel != null ? oldModel.Descripcion : null
                });

                //Extensiones
                string newExtensiones = "";
                bool newFirst = true;
                string oldExtensiones = "";
                bool oldFirst = true;
                if (newModel != null && newModel.Extensiones != null && newModel.Extensiones.Count > 0)
                {
                    foreach (var item in newModel.Extensiones)
                    {
                        newExtensiones += !newFirst ? ", " : "";
                        newExtensiones += $@".{item.Nombre}";
                        newFirst = false;
                    }
                }

                if (oldModel != null && oldModel.Extensiones != null && oldModel.Extensiones.Count > 0)
                {
                    foreach (var item in oldModel.Extensiones)
                    {
                        oldExtensiones += !oldFirst ? ", " : "";
                        oldExtensiones += $@".{item.Nombre}";
                        oldFirst = false;
                    }
                }

                //Extensiones
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Extensiones",
                    es_modificado = oldModel != null && oldExtensiones != newExtensiones ? true : false,
                    campo_nuevo = newExtensiones,
                    campo_anterior = oldExtensiones
                });

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
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "GrupoExtensions", "Grupo de extensiones", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
