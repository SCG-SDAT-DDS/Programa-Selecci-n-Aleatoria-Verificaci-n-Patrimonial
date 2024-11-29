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
    public class ExtensionesController : BaseController
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
                var Model = db.Extensiones.Find(id);
                var oldModel = (Extension)Model.Clone();
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
                this.CreateBitacora(oldModel, Model, Model.ExtensionId);

            }
            catch (Exception)
            {

            }
            return activo;

        }

        // GET: Extensiones
        public ActionResult Index(FiltrosCatalogos filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            string[] searchstringsN = !string.IsNullOrEmpty(filtros.Nombre) ? filtros.Nombre.Split(' ') : "".Split(' ');

            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            //ViewBag.OrderDescripcion = sOrder == "Descripcion" ? "Descripcion_desc" : "Descripcion";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";
            IPagedList<Extension> vModel = null;



            var query = db.Extensiones.Where(x =>
                 ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.Nombre.ToLower().Contains(y.ToLower())))
                 //&& (x.Activo == filtros.Activo)
                 );
            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.Nombre).ToPagedList(iPagina, PerPage);
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
                return PartialView("_listaExtension", vModel);
            }
            return View(vModel);
        }

        // GET: Extensiones/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Extension extension = db.Extensiones.Find(id);
            if (extension == null)
            {
                return HttpNotFound();
            }
            return View(extension);
        }

        // GET: Extensiones/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Extensiones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Extension extension)
        {
            if (ModelState.IsValid)
            {
                db.Extensiones.Add(extension);
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(null, extension, extension.ExtensionId);

                return RedirectToAction("Index");
            }

            return View(extension);
        }

        // GET: Extensiones/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Extension extension = db.Extensiones.Find(id);
            if (extension == null)
            {
                return HttpNotFound();
            }
            return View(extension);
        }

        // POST: Extensiones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Extension extension)
        {
            if (ModelState.IsValid)
            {
                //para la bitacora
                ApplicationDbContext dbOld = new ApplicationDbContext();
                var oldModel = dbOld.Extensiones.Where(x => x.ExtensionId == extension.ExtensionId).FirstOrDefault();

                db.Entry(extension).State = EntityState.Modified;
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(oldModel, extension, extension.ExtensionId);

                return RedirectToAction("Index");
            }
            return View(extension);
        }

        //// GET: Extensiones/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Extension extension = db.Extensiones.Find(id);
        //    if (extension == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(extension);
        //}

        //// POST: Extensiones/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Extension extension = db.Extensiones.Find(id);
        //    extension.Activo = false;
        //    db.Entry(extension).State = EntityState.Modified;
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
        private void CreateBitacora(Extension oldModel, Extension newModel, int id = 0)
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
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "Extensions", "Extensiones", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
