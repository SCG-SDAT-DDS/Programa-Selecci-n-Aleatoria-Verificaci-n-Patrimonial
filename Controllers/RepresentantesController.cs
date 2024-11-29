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
    public class RepresentantesController : BaseController
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
                var Model = db.Representantes.Find(id);
                var oldModel = (Representante)Model.Clone();
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
                this.CreateBitacora(oldModel, Model, Model.RepresentanteId);

            }
            catch (Exception)
            {
            }

            return activo;

        }

        // GET: Representantes
        public ActionResult Index(FiltrosCatalogos filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            string[] searchstringsN = !string.IsNullOrEmpty(filtros.Nombre) ? filtros.Nombre.Split(' ') : "".Split(' ');

            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";
            ViewBag.OrderPuesto = sOrder == "Puesto" ? "Puesto_desc" : "Puesto";

            IPagedList<Representante> vModel = null;

            var query = db.Representantes.Where(x =>
                   ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.NombreCompleto.ToLower().Contains(y.ToLower())))
                 );
            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.NombreCompleto).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.NombreCompleto).ToPagedList(iPagina, PerPage);
                    break;
                case "Puesto":
                    vModel = query.OrderBy(x => x.Puesto).ToPagedList(iPagina, PerPage);
                    break;
                case "Puesto_desc":
                    vModel = query.OrderByDescending(x => x.Puesto).ToPagedList(iPagina, PerPage);
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
                return PartialView("_listaRepresentantes", vModel);
            }
            return View(vModel);
        }

        // GET: Representantes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Representante representante = db.Representantes.Find(id);
            if (representante == null)
            {
                return HttpNotFound();
            }
            return View(representante);
        }

        // GET: Representantes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Representantes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Representante representante)
        {
            if (ModelState.IsValid)
            {
                representante.Activo = true;
                db.Representantes.Add(representante);
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(null, representante, representante.RepresentanteId);

                return RedirectToAction("Index");
            }

            return View(representante);
        }

        // GET: Representantes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Representante representante = db.Representantes.Find(id);
            if (representante == null)
            {
                return HttpNotFound();
            }
            return View(representante);
        }

        // POST: Representantes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Representante representante)
        {
            if (ModelState.IsValid)
            {
                //para la bitacora
                ApplicationDbContext dbOld = new ApplicationDbContext();
                var oldModel = dbOld.Representantes.Where(x => x.RepresentanteId == representante.RepresentanteId).FirstOrDefault();

                db.Entry(representante).State = EntityState.Modified;
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(oldModel, representante, representante.RepresentanteId);

                return RedirectToAction("Index");
            }
            return View(representante);
        }

        //// GET: Representantes/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Representante representante = db.Representantes.Find(id);
        //    if (representante == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(representante);
        //}

        //// POST: Representantes/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Representante representante = db.Representantes.Find(id);
        //    representante.Activo = false;
        //    db.Entry(representante).State = EntityState.Modified;
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
        private void CreateBitacora(Representante oldModel, Representante newModel, int id = 0)
        {
            var cambioCampos = new List<cambiosCampos>();
            int accion = oldModel != null ? 2 : 1;
            //Creamos la bitacora
            try
            {
                //Nombre
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Nombre Completo",
                    es_modificado = oldModel != null && oldModel.NombreCompleto != newModel.NombreCompleto ? true : false,
                    campo_nuevo = newModel.NombreCompleto,
                    campo_anterior = oldModel != null ? oldModel.NombreCompleto : null
                });

                //Título
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Título",
                    es_modificado = oldModel != null && oldModel.Titulo != newModel.Titulo ? true : false,
                    campo_nuevo = newModel.Titulo,
                    campo_anterior = oldModel != null ? oldModel.Titulo : null
                });

                //Teléfono
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Teléfono",
                    es_modificado = oldModel != null && oldModel.Telefono != newModel.Telefono ? true : false,
                    campo_nuevo = newModel.Telefono,
                    campo_anterior = oldModel != null ? oldModel.Telefono : null
                });

                //Extensión
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Extensión",
                    es_modificado = oldModel != null && oldModel.Extension != newModel.Extension ? true : false,
                    campo_nuevo = newModel.Extension,
                    campo_anterior = oldModel != null ? oldModel.Extension : null
                });

                //Puesto
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Puesto",
                    es_modificado = oldModel != null && oldModel.Puesto != newModel.Puesto ? true : false,
                    campo_nuevo = newModel.Puesto,
                    campo_anterior = oldModel != null ? oldModel.Puesto : null
                });

                //Url de DAP Sonora
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Url de DAP Sonora",
                    es_modificado = oldModel != null && oldModel.DAPUrl != newModel.DAPUrl ? true : false,
                    campo_nuevo = newModel.DAPUrl,
                    campo_anterior = oldModel != null ? oldModel.DAPUrl : null,
                    link = true
                });

                //Orden
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Orden",
                    es_modificado = oldModel != null && oldModel.Orden != newModel.Orden ? true : false,
                    campo_nuevo = newModel.Orden.ToString(),
                    campo_anterior = oldModel != null ? oldModel.Orden.ToString() : null
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
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "Representantes", "Representantes", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
