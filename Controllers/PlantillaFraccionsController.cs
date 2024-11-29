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
    public class PlantillaFraccionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //public bool? changeStatus(int id = 0)
        //{
        //    var activo = false;
        //    if (id == 0)
        //    {
        //        return null;
        //    }
        //    var Model = db.PlantillaFraccions.Find(id);
        //    if (Model == null)
        //    {
        //        return null;
        //    }
        //    if (Model.Activo)
        //    {
        //        Model.Activo = activo = false;
        //    }
        //    else
        //    {
        //        Model.Activo = activo = true;
        //    }
        //    db.SaveChanges();
        //    //return View("Index",horariosModels);
        //    return activo;
        //}

        private void combos()
        {
            ViewBag.CmbActivo = new List<SelectListItem>();
            ViewBag.CmbPlantillas = new List<SelectListItem>();
            ViewBag.CmbOrganismos = new List<SelectListItem>();
            ViewBag.CmbFracciones = new List<SelectListItem>();


            ViewBag.CmbPlantillas = new SelectList(db.Plantillas.OrderBy(x => x.NombreLargo), "PlantillaId", "NombreLargo");
            ViewBag.CmbOrganismos = new SelectList(db.Organismos.OrderBy(x => x.NombreOrganismo), "OrganismoID", "NombreOrganismo");
            ViewBag.CmbFracciones = new SelectList(db.Fracciones.OrderBy(x => x.Nombre).Select(x => new { FraccionId = "Fracción: "+x.FraccionId, Nombre = x.Nombre + " con el artículo: " + x.Articulos.Nombre + ", Ley: " + x.Articulos.Leyes.Nombre + " - "+x.Articulos.Leyes.TipoLeyes.Nombre+"." }), "FraccionId", "Nombre");
            ViewBag.CmbActivo = new List<SelectListItem> {
                new SelectListItem { Text = "Activo", Value = "true" },
                new SelectListItem { Text = "Inactivo", Value = "false" }
            };
        }

        // GET: PlantillaFraccions
        public ActionResult Index(FiltrosPlantillaFraccion filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            combos();
            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderFracciones = sOrder == "Fracciones" ? "Fracciones_desc" : "Fracciones";
            ViewBag.OrderOrganismos = sOrder == "Organismos" ? "Organismos_desc" : "Organismos";
            ViewBag.OrderPlantillas = sOrder == "Plantillas" ? "Plantillas_desc" : "Plantillas";
            IPagedList<PlantillaFraccion> vModel = null;

            var query = db.PlantillaFraccions.Include(a => a.Fracciones).Include(a => a.Fracciones.Articulos).Include(a => a.Fracciones.Articulos.Leyes).Include(a => a.Fracciones.Articulos.Leyes.TipoLeyes).Include(a => a.Plantillas).Where(x =>
             (filtros.Fracciones == 0 || filtros.Fracciones == x.FraccionId)
             && (filtros.Plantillas == 0 || filtros.Plantillas == x.PlantillaId)
             );

            switch (sOrder)
            {
                case "Fracciones":
                    vModel = query.OrderBy(x => x.Fracciones.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Fracciones_desc":
                    vModel = query.OrderByDescending(x => x.Fracciones.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                //case "Organismos":
                //    vModel = query.OrderBy(x => x.Organismos.NombreOrganismo).ToPagedList(iPagina, PerPage);
                //    break;
                //case "Organismos_desc":
                //    vModel = query.OrderByDescending(x => x.Organismos.NombreOrganismo).ToPagedList(iPagina, PerPage);
                //    break;
                case "Plantillas":
                    vModel = query.OrderBy(x => x.Plantillas.NombreLargo).ToPagedList(iPagina, PerPage);
                    break;
                case "Plantillas_desc":
                    vModel = query.OrderByDescending(x => x.Plantillas.NombreLargo).ToPagedList(iPagina, PerPage);
                    break;
                default:
                    vModel = query.OrderBy(x => x.Fracciones.Nombre).ToPagedList(iPagina, PerPage);
                    break;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("_lista", vModel);
            }
            return View(vModel);
        }

        // GET: PlantillaFraccions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlantillaFraccion plantillaFraccion = db.PlantillaFraccions.Find(id);
            if (plantillaFraccion == null)
            {
                return HttpNotFound();
            }
            plantillaFraccion = GetMissing(plantillaFraccion);
            return View(plantillaFraccion);
        }

        private PlantillaFraccion GetMissing(PlantillaFraccion vModel)
        {
            if (vModel.Fracciones == null)
                vModel.Fracciones = db.Fracciones.Where(x => x.FraccionId == vModel.FraccionId).Include(a => a.Articulos).Include(a => a.Articulos.Leyes).Include(a => a.Articulos.Leyes.TipoLeyes).FirstOrDefault();
            
            if (vModel.Plantillas == null)
                vModel.Plantillas = db.Plantillas.Where(x => x.PlantillaId == vModel.PlantillaId).FirstOrDefault();


            return vModel;
        }

        // GET: PlantillaFraccions/Create
        public ActionResult Create()
        {
            ViewBag.FraccionId = new SelectList(db.Fracciones.Where(x=>x.Activo).Select(x => new { FraccionId = x.FraccionId, Nombre = x.Nombre + " con el artículo: " + x.Articulos.Nombre + ", Ley: " + x.Articulos.Leyes.Nombre + " - " + x.Articulos.Leyes.TipoLeyes.Nombre + "." }), "FraccionId", "Nombre");
            ViewBag.OrganismoID = new SelectList(db.Organismos.Where(x=>x.Activo), "OrganismoID", "NombreOrganismo");
            ViewBag.PlantillaId = new SelectList(db.Plantillas.Where(x=>x.Activo), "PlantillaId", "NombreCorto");
            return View();
        }

        // POST: PlantillaFraccions/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PlantillaFraccion plantillaFraccion)
        {
            if (ModelState.IsValid)
            {
                db.PlantillaFraccions.Add(plantillaFraccion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.FraccionId = new SelectList(db.Fracciones.Where(x => x.Activo).Select(x => new { FraccionId = x.FraccionId, Nombre = x.Nombre + " con el artículo: " + x.Articulos.Nombre + ", Ley: " + x.Articulos.Leyes.Nombre + " - " + x.Articulos.Leyes.TipoLeyes.Nombre + "." }), "FraccionId", "Nombre");
            ViewBag.OrganismoID = new SelectList(db.Organismos.Where(x => x.Activo), "OrganismoID", "NombreOrganismo");
            ViewBag.PlantillaId = new SelectList(db.Plantillas.Where(x => x.Activo), "PlantillaId", "NombreCorto");
            return View(plantillaFraccion);
        }

        // GET: PlantillaFraccions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlantillaFraccion plantillaFraccion = db.PlantillaFraccions.Find(id);
            if (plantillaFraccion == null)
            {
                return HttpNotFound();
            }
            ViewBag.FraccionId = new SelectList(db.Fracciones.Where(x => x.Activo).Select(x => new { FraccionId = x.FraccionId, Nombre = x.Nombre + " con el artículo: " + x.Articulos.Nombre + ", Ley: " + x.Articulos.Leyes.Nombre + " - " + x.Articulos.Leyes.TipoLeyes.Nombre + "." }), "FraccionId", "Nombre");
            ViewBag.OrganismoID = new SelectList(db.Organismos.Where(x => x.Activo), "OrganismoID", "NombreOrganismo");
            ViewBag.PlantillaId = new SelectList(db.Plantillas.Where(x => x.Activo), "PlantillaId", "NombreCorto");
            return View(plantillaFraccion);
        }

        // POST: PlantillaFraccions/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PlantillaFraccionId,PlantillaId,FraccionId,OrganismoID")] PlantillaFraccion plantillaFraccion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(plantillaFraccion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FraccionId = new SelectList(db.Fracciones.Where(x => x.Activo).Select(x => new { FraccionId =x.FraccionId, Nombre = x.Nombre + " con el artículo: " + x.Articulos.Nombre + ", Ley: " + x.Articulos.Leyes.Nombre + " - " + x.Articulos.Leyes.TipoLeyes.Nombre + "." }), "FraccionId", "Nombre");
            ViewBag.OrganismoID = new SelectList(db.Organismos.Where(x => x.Activo), "OrganismoID", "NombreOrganismo");
            ViewBag.PlantillaId = new SelectList(db.Plantillas.Where(x => x.Activo), "PlantillaId", "NombreCorto");
            return View(plantillaFraccion);
        }

        // GET: PlantillaFraccions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PlantillaFraccion plantillaFraccion = db.PlantillaFraccions.Find(id);
            if (plantillaFraccion == null)
            {
                return HttpNotFound();
            }
            plantillaFraccion = GetMissing(plantillaFraccion);
            return View(plantillaFraccion);
        }

        // POST: PlantillaFraccions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PlantillaFraccion plantillaFraccion = db.PlantillaFraccions.Find(id);
            db.PlantillaFraccions.Remove(plantillaFraccion);
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
