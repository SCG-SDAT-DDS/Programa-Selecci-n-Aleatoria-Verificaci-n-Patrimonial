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
    public class PeriodosController : BaseController
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
                var Model = db.Periodos.Find(id);
                var oldModel = (Periodo)Model.Clone();
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
                this.CreateBitacora(oldModel, Model, Model.PeriodoId);


            }
            catch (Exception)
            {

                return false;
            }
            return activo;
        }

        // GET: Paises
        public ActionResult Index(FiltrosCatalogos filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            string[] searchstringsN = !string.IsNullOrEmpty(filtros.Nombre) ? filtros.Nombre.Split(' ') : "".Split(' ');

            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";

            IPagedList<Periodo> vModel = null;

            var query = db.Periodos.Where(x =>
                   ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.NombrePeriodo.ToLower().Contains(y.ToLower())))
                 );
            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.NombrePeriodo).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.NombrePeriodo).ToPagedList(iPagina, PerPage);
                    break;

                case "Activo":
                    vModel = query.OrderBy(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo_desc":
                    vModel = query.OrderByDescending(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                default:
                    vModel = query.OrderBy(x => x.NombrePeriodo).ToPagedList(iPagina, PerPage);
                    break;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("_listaPeriodos", vModel);
            }
            return View(vModel);
        }

        // GET: Paises/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Periodo periodo = db.Periodos.Find(id);
            if (periodo == null)
            {
                return HttpNotFound();
            }
            return View(periodo);
        }

        // GET: Paises/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Paises/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Periodo periodo)
        {
            if (ModelState.IsValid)
            {
                periodo.Activo = true;
                db.Periodos.Add(periodo);
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(null, periodo, periodo.PeriodoId);

                return RedirectToAction("Index");
            }

            return View(periodo);
        }

        // GET: Paises/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Periodo perriodo = db.Periodos.Find(id);
            if (perriodo == null)
            {
                return HttpNotFound();
            }
            return View(perriodo);
        }

        // POST: Paises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Periodo periodo)
        {
            if (ModelState.IsValid)
            {
                //para la bitacora
                ApplicationDbContext dbOld = new ApplicationDbContext();
                var oldModel = dbOld.Periodos.Where(x => x.PeriodoId == periodo.PeriodoId).FirstOrDefault();

                db.Entry(periodo).State = EntityState.Modified;
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(oldModel, periodo, periodo.PeriodoId);

                return RedirectToAction("Index");
            }
            return View(periodo);
        }

        //// GET: Paises/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Periodo periodo = db.Periodos.Find(id);
        //    if (periodo == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(periodo);
        //}

        //// POST: Paises/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Periodo periodo = db.Periodos.Find(id);
        //    periodo.Activo = false;
        //    db.Entry(periodo).State = EntityState.Modified;
        //    db.SaveChanges();

        //    return RedirectToAction("Index");
        //}

        public JsonResult ObtenerTodosLosPeriodos()
        {

            IEnumerable<SelectListItem> periodos = db.Periodos.
                Where(x=>x.Activo)
                .Select(c => new SelectListItem
                {
                    Value = c.PeriodoId.ToString(),
                    Text = c.NombrePeriodo
                });

            return Json(periodos, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //Bitacora
        private void CreateBitacora(Periodo oldModel, Periodo newModel, int id = 0)
        {
            var cambioCampos = new List<cambiosCampos>();
            int accion = oldModel != null ? 2 : 1;
            //Creamos la bitacora
            try
            {
                //Periodo
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Periodo",
                    es_modificado = oldModel != null && oldModel.NombrePeriodo != newModel.NombrePeriodo ? true : false,
                    campo_nuevo = newModel.NombrePeriodo,
                    campo_anterior = oldModel != null ? oldModel.NombrePeriodo : null
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
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "periodoes", "Periodos", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
