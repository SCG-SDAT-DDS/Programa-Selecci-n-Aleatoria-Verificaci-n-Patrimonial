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
    public class TagsController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public bool? changeStatus(int id = 0)
        {
            var activo = false;
            try
            {
               
                if (id == 0)
                {
                    return null;
                }
                var Model = db.Tags.Find(id);
                var oldModel = (Tag)Model.Clone();
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
                this.CreateBitacora(oldModel, Model, Model.TagId);

            }
            catch(Exception ex)
            {

            }
            return activo;
        }

        private void combos()
        {
            ViewBag.CmbActivo = new List<SelectListItem>();
            ViewBag.CmbActivo = new List<SelectListItem> {
                new SelectListItem { Text = "Activo", Value = "true" },
                new SelectListItem { Text = "Inactivo", Value = "false" }
            };
        }
        // GET: Tags
        public ActionResult Index(FiltrosCatalogos filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            combos();
            string[] searchstringsN = filtros.Nombre != null && filtros.Nombre.Length > 0 ? filtros.Nombre.Split(' ') : "".Split(' ');
            string[] searchstringsD = filtros.Descripcion != null && filtros.Descripcion.Length > 0 ? filtros.Descripcion.Split(' ') : "".Split(' ');

            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderDescripcion = sOrder == "Descripcion" ? "Descripcion_desc" : "Descripcion";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";
            IPagedList<Tag> vModel = null;

            var query = db.Tags.Where(x =>
                ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.Nombre.ToLower().Contains(y.ToLower())))
                && ((filtros.Descripcion == null || filtros.Descripcion.Length == 0) || searchstringsD.Any(y => x.Descripcion.ToLower().Contains(y.ToLower())))
                && (filtros.ActivoNull == null || filtros.ActivoNull == x.Activo)
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
                return PartialView("_lista", vModel);
            }
            return View(vModel);
        }

        // GET: Tags/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tag tag = db.Tags.Find(id);
            if (tag == null)
            {
                return HttpNotFound();
            }
            return View(tag);
        }

        // GET: Tags/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tags/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TagId,Nombre,Descripcion,Activo,Icono,Color")] Tag tag)
        {
            if (ModelState.IsValid)
            {
                db.Tags.Add(tag);
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(null, tag, tag.TagId);

                return RedirectToAction("Index");
            }

            return View(tag);
        }

        // GET: Tags/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tag tag = db.Tags.Find(id);
            if (tag == null)
            {
                return HttpNotFound();
            }
            return View(tag);
        }

        // POST: Tags/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TagId,Nombre,Descripcion,Activo,Icono,Color")] Tag tag)
        {
            if (ModelState.IsValid)
            {
                //para la bitacora
                ApplicationDbContext dbOld = new ApplicationDbContext();
                var oldModel = dbOld.Tags.Where(x => x.TagId == tag.TagId).FirstOrDefault();

                db.Entry(tag).State = EntityState.Modified;
                db.SaveChanges();

                //bitacora
                this.CreateBitacora(oldModel, tag, tag.TagId);

                return RedirectToAction("Index");
            }
            return View(tag);
        }

        //// GET: Tags/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Tag tag = db.Tags.Find(id);
        //    if (tag == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(tag);
        //}

        //// POST: Tags/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Tag tag = db.Tags.Find(id);
        //    db.Tags.Remove(tag);
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
        private void CreateBitacora(Tag oldModel, Tag newModel, int id = 0)
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

                //Activo
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Activo",
                    es_modificado = oldModel != null && oldModel.Activo != newModel?.Activo ? true : false,
                    campo_nuevo = HMTLHelperExtensions.getStringBoolBitacora(newModel.Activo),
                    campo_anterior = oldModel != null ? HMTLHelperExtensions.getStringBoolBitacora(oldModel.Activo) : null
                });
                //Icono
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Icono",
                    es_modificado = oldModel != null && oldModel.Icono != newModel.Icono ? true : false,
                    campo_nuevo = newModel.Icono,
                    campo_anterior = oldModel != null ? oldModel.Icono : null
                });
                //Color
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Color",
                    es_modificado = oldModel != null && oldModel.Color != newModel.Color ? true : false,
                    campo_nuevo = newModel.Color,
                    campo_anterior = oldModel != null ? oldModel.Color : null
                });
                //Bitacora
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "tags", "Tags", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
