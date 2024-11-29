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
    public class FraccionesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public bool? changeStatus(int id = 0)
        {
            var activo = false;
            if (id == 0)
            {
                return null;
            }
            var Model = db.Fracciones.Find(id);
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
            ViewBag.CmbFraccion = new List<SelectListItem>();


            ViewBag.CmbArticulo = new SelectList(db.Articulos.OrderBy(x => x.Nombre).Select(x => new { ArticuloId = x.ArticuloId, Nombre = x.Nombre + " (" + x.Leyes.Nombre + " - " + x.Leyes.TipoLeyes.Nombre + ")" }), "ArticuloId", "Nombre");
            ViewBag.CmbActivo = new List<SelectListItem> {
                new SelectListItem { Text = "Activo", Value = "true" },
                new SelectListItem { Text = "Inactivo", Value = "false" }
            };
        }
        // GET: Fracciones
        public ActionResult Index(FiltrosFraccion filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {

            combos();
            string[] searchstringsN = filtros.Nombre != null && filtros.Nombre.Length > 0 ? filtros.Nombre.Split(' ') : "".Split(' ');
            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderArticulo = sOrder == "Articulo" ? "Articulo_desc" : "Articulo";
            ViewBag.OrderOrden = sOrder == "Orden" ? "Orden_desc" : "Orden";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";
            IPagedList<Fraccion> vModel = null;

            var query = db.Fracciones.Include(a => a.Articulos).Include(x => x.Articulos.Leyes).Include(x => x.Articulos.Leyes.TipoLeyes).Where(x =>
            ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.Nombre.ToLower().Contains(y.ToLower())))
            && (filtros.ActivoNull == null || filtros.ActivoNull == x.Activo)
            && (filtros.Articulo == 0 || filtros.Articulo == x.ArticuloId)
            );


            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Articulo":
                    vModel = query.OrderBy(x => x.Articulos.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Articulo_desc":
                    vModel = query.OrderByDescending(x => x.Articulos.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Orden":
                    vModel = query.OrderBy(x => x.Orden).ToPagedList(iPagina, PerPage);
                    break;
                case "Orden_desc":
                    vModel = query.OrderByDescending(x => x.Orden).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo":
                    vModel = query.OrderBy(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo_desc":
                    vModel = query.OrderByDescending(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                default:
                    vModel = query.OrderBy(x => x.Orden).ToPagedList(iPagina, PerPage);
                    break;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("_lista", vModel);
            }
            return View(vModel);
        }

        // GET: Fracciones/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Fraccion fraccion = db.Fracciones.Find(id);
            if (fraccion == null)
            {
                return HttpNotFound();
            }
            if (fraccion.Articulos == null)
                fraccion.Articulos = db.Articulos.Where(x => x.ArticuloId == fraccion.ArticuloId).Include(x => x.Leyes).Include(x=>x.Leyes.TipoLeyes).FirstOrDefault();
            return View(fraccion);
        }

        // GET: Fracciones/Create
        public ActionResult Create()
        {
            ViewBag.ArticuloId = new SelectList(db.Articulos.Where(x => x.Activo).Select(x => new { ArticuloId = x.ArticuloId, Nombre = x.Nombre + " (" + x.Leyes.Nombre + " - "+x.Leyes.TipoLeyes.Nombre+")" }), "ArticuloId", "Nombre");
            return View();
        }

        // POST: Fracciones/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FraccionId,Nombre,ArticuloId,Url,Ayuda,Orden")] Fraccion fraccion)
        {
            if (ModelState.IsValid)
            {
                fraccion.Activo = true;
                db.Fracciones.Add(fraccion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ArticuloId = new SelectList(db.Articulos.Where(x => x.Activo).Select(x => new { ArticuloId = x.ArticuloId, Nombre = x.Nombre + " (" + x.Leyes.Nombre + " - " + x.Leyes.TipoLeyes.Nombre + ")" }), "ArticuloId", "Nombre");
            return View(fraccion);
        }

        // GET: Fracciones/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Fraccion fraccion = db.Fracciones.Find(id);
            if (fraccion == null)
            {
                return HttpNotFound();
            }
            ViewBag.ArticuloId = new SelectList(db.Articulos.Where(x => x.Activo).Select(x => new { ArticuloId = x.ArticuloId, Nombre = x.Nombre + " (" + x.Leyes.Nombre + " - " + x.Leyes.TipoLeyes.Nombre + ")" }), "ArticuloId", "Nombre",fraccion.ArticuloId);
            return View(fraccion);
        }

        // POST: Fracciones/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FraccionId,Nombre,ArticuloId,Url,Ayuda,Orden,Activo")] Fraccion fraccion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fraccion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ArticuloId = new SelectList(db.Articulos.Where(x => x.Activo).Select(x => new { ArticuloId = x.ArticuloId, Nombre = x.Nombre + " (" + x.Leyes.Nombre + " - " + x.Leyes.TipoLeyes.Nombre + ")" }), "ArticuloId", "Nombre", fraccion.ArticuloId);
            return View(fraccion);
        }

        // GET: Fracciones/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Fraccion fraccion = db.Fracciones.Find(id);
            if (fraccion == null)
            {
                return HttpNotFound();
            }
            if(fraccion.Articulos ==null)
                fraccion.Articulos = db.Articulos.Where(x => x.ArticuloId == fraccion.ArticuloId).Include(x => x.Leyes).Include(x => x.Leyes.TipoLeyes).FirstOrDefault();
            return View(fraccion);
        }

        // POST: Fracciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Fraccion fraccion = db.Fracciones.Find(id);
            db.Fracciones.Remove(fraccion);
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
