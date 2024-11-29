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
    public class InputConfigsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public bool? changeStatus(int id = 0)
        {
            var activo = false;
            if (id == 0)
            {
                return null;
            }
            var Model = db.InputConfigs.Find(id);
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
            //return View("Index",horariosModels);
            return activo;
        }

        private void combos()
        {
            ViewBag.CmbActivo = new List<SelectListItem>();
            ViewBag.CmbTipoInputs = new List<SelectListItem>();
            ViewBag.CmbDataTypeMs = new List<SelectListItem>();


            ViewBag.CmbTipoInputs = new SelectList(db.TipoInputs.OrderBy(x => x.Nombre), "TipoInputId", "Nombre");
            ViewBag.CmbDataTypeMs = new SelectList(db.DataTypeMs.OrderBy(x => x.Nombre), "DataTypeMIId", "Nombre");
            ViewBag.CmbActivo = new List<SelectListItem> {
                new SelectListItem { Text = "Activo", Value = "true" },
                new SelectListItem { Text = "Inactivo", Value = "false" }
            };
        }
        // GET: InputConfigs
        public ActionResult Index(FiltrosInputConfig filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            combos();
            string[] searchstringsN = filtros.Nombre != null && filtros.Nombre.Length > 0 ? filtros.Nombre.Split(' ') : "".Split(' ');
            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderTipoDataType = sOrder == "TipoDataType" ? "TipoDataType_desc" : "TipoDataType";
            ViewBag.OrderTipoInput = sOrder == "TipoInput" ? "TipoInput_desc" : "TipoInput";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";
            IPagedList<InputConfig> vModel = null;

            var query = db.InputConfigs.Include(a => a.TipoInputs).Include(x=>x.DataTypeMs).Where(x =>
             ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.Nombre.ToLower().Contains(y.ToLower())))
             && (filtros.ActivoNull == null || filtros.ActivoNull == x.Activo)
             && (filtros.TipoInputs == 0 || filtros.TipoInputs == x.TipoInputId)
             && (filtros.DataTypeMs == 0 || filtros.DataTypeMs == x.DataTypeMIId)
             );

            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "TipoDataType":
                    vModel = query.OrderBy(x => x.DataTypeMs.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "TipoDataType_desc":
                    vModel = query.OrderByDescending(x => x.DataTypeMs.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "TipoInput":
                    vModel = query.OrderBy(x => x.TipoInputs.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "TipoInput_desc":
                    vModel = query.OrderByDescending(x => x.TipoInputs.Nombre).ToPagedList(iPagina, PerPage);
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

        // GET: InputConfigs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InputConfig inputConfig = db.InputConfigs.Find(id);
            if (inputConfig == null)
            {
                return HttpNotFound();
            }
            if (inputConfig.TipoInputs == null)
                inputConfig.TipoInputs = db.TipoInputs.Where(x => x.TipoInputId == inputConfig.TipoInputId).FirstOrDefault();
            if (inputConfig.DataTypeMs == null)
                inputConfig.DataTypeMs = db.DataTypeMs.Where(x => x.DataTypeMIId == inputConfig.DataTypeMIId).FirstOrDefault();
            return View(inputConfig);
        }

        // GET: InputConfigs/Create
        public ActionResult Create()
        {
            ViewBag.DataTypeMIId = new SelectList(db.DataTypeMs, "DataTypeMIId", "Nombre");
            ViewBag.TipoInputId = new SelectList(db.TipoInputs, "TipoInputId", "Nombre");
            return View();
        }

        // POST: InputConfigs/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "InputConfigId,TipoInputId,DataTypeMIId,Nombre,Mask,Ayuda")] InputConfig inputConfig)
        {
            if (ModelState.IsValid)
            {
                inputConfig.Activo = true;
                db.InputConfigs.Add(inputConfig);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DataTypeMIId = new SelectList(db.DataTypeMs, "DataTypeMIId", "Nombre", inputConfig.DataTypeMIId);
            ViewBag.TipoInputId = new SelectList(db.TipoInputs, "TipoInputId", "Nombre", inputConfig.TipoInputId);
            return View(inputConfig);
        }

        // GET: InputConfigs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InputConfig inputConfig = db.InputConfigs.Find(id);
            if (inputConfig == null)
            {
                return HttpNotFound();
            }
            ViewBag.DataTypeMIId = new SelectList(db.DataTypeMs, "DataTypeMIId", "Nombre", inputConfig.DataTypeMIId);
            ViewBag.TipoInputId = new SelectList(db.TipoInputs, "TipoInputId", "Nombre", inputConfig.TipoInputId);
            return View(inputConfig);
        }

        // POST: InputConfigs/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "InputConfigId,TipoInputId,DataTypeMIId,Nombre,Mask,Ayuda,Activo")] InputConfig inputConfig)
        {
            if (ModelState.IsValid)
            {
                db.Entry(inputConfig).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DataTypeMIId = new SelectList(db.DataTypeMs, "DataTypeMIId", "Nombre", inputConfig.DataTypeMIId);
            ViewBag.TipoInputId = new SelectList(db.TipoInputs, "TipoInputId", "Nombre", inputConfig.TipoInputId);
            return View(inputConfig);
        }

        // GET: InputConfigs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InputConfig inputConfig = db.InputConfigs.Find(id);
            if (inputConfig == null)
            {
                return HttpNotFound();
            }
            if (inputConfig.TipoInputs == null)
                inputConfig.TipoInputs = db.TipoInputs.Where(x => x.TipoInputId == inputConfig.TipoInputId).FirstOrDefault();
            if (inputConfig.DataTypeMs == null)
                inputConfig.DataTypeMs = db.DataTypeMs.Where(x => x.DataTypeMIId == inputConfig.DataTypeMIId).FirstOrDefault();
            return View(inputConfig);
        }

        // POST: InputConfigs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            InputConfig inputConfig = db.InputConfigs.Find(id);
            db.InputConfigs.Remove(inputConfig);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
