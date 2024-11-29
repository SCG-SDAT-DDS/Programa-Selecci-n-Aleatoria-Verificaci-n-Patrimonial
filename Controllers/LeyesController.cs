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
    public class LeyesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public bool? changeStatus(int id = 0)
        {
            var activo = false;
            if (id == 0)
            {
                return null;
            }
            var Model = db.Leyes.Find(id);
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
            ViewBag.CmbTipoLeyes = new List<SelectListItem>();


            ViewBag.CmbTipoLeyes = new SelectList(db.TipoLeyes.OrderBy(x => x.Nombre), "TipoLeyId", "Nombre");
            ViewBag.CmbActivo = new List<SelectListItem> {
                new SelectListItem { Text = "Activo", Value = "true" },
                new SelectListItem { Text = "Inactivo", Value = "false" }
            };
        }
        // GET: Leyes
        public ActionResult Index(FiltrosLeyes filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            combos();
            string[] searchstringsN = filtros.Nombre != null && filtros.Nombre.Length > 0 ? filtros.Nombre.Split(' ') : "".Split(' ');
            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderTipoLeyes = sOrder == "TipoLeyes" ? "TipoLeyes_desc" : "TipoLeyes";
            ViewBag.OrderOrden = sOrder == "Orden" ? "Orden_desc" : "Orden";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";
            IPagedList<Ley> vModel = null;

            var query = db.Leyes.Include(a => a.TipoLeyes).Where(x =>
             ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.Nombre.ToLower().Contains(y.ToLower())))
             && (filtros.ActivoNull == null || filtros.ActivoNull == x.Activo)
             && (filtros.TipoLeyes == 0 || filtros.TipoLeyes == x.TipoLeyId)
             );

            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "TipoLeyes":
                    vModel = query.OrderBy(x => x.TipoLeyes.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "TipoLeyes_desc":
                    vModel = query.OrderByDescending(x => x.TipoLeyes.Nombre).ToPagedList(iPagina, PerPage);
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

        // GET: Leyes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ley ley = db.Leyes.Find(id);
            if (ley == null)
            {
                return HttpNotFound();
            }
            if (ley.TipoLeyes == null)
                ley.TipoLeyes = db.TipoLeyes.Where(x => x.TipoLeyId == ley.TipoLeyId).FirstOrDefault();
            return View(ley);
        }

        // GET: Leyes/Create
        public ActionResult Create()
        {
            ViewBag.TipoLeyId = new SelectList(db.TipoLeyes.Where(x => x.Activo), "TipoLeyId", "Nombre");
            return View();
        }

        // POST: Leyes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "LeyId,Nombre,Url,Ayuda,Orden,TipoLeyId")] Ley ley)
        {
            if (ModelState.IsValid) { 
            
                ley.Activo=true;
                db.Leyes.Add(ley);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TipoLeyId = new SelectList(db.TipoLeyes.Where(x => x.Activo), "TipoLeyId", "Nombre");
            return View(ley);
        }

        // GET: Leyes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ley ley = db.Leyes.Find(id);
            if (ley == null)
            {
                return HttpNotFound();
            }
            ViewBag.TipoLeyId = new SelectList(db.TipoLeyes.Where(x => x.Activo), "TipoLeyId", "Nombre",ley.TipoLeyId);
            return View(ley);
        }

        // POST: Leyes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "LeyId,Nombre,Url,Ayuda,Orden,Activo,TipoLeyId")] Ley ley)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ley).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TipoLeyId = new SelectList(db.TipoLeyes.Where(x => x.Activo), "TipoLeyId", "Nombre", ley.TipoLeyId);
            return View(ley);
        }

        // GET: Leyes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ley ley = db.Leyes.Find(id);
            if (ley == null)
            {
                return HttpNotFound();
            }
            if (ley.TipoLeyes == null)
                ley.TipoLeyes = db.TipoLeyes.Where(x => x.TipoLeyId == ley.TipoLeyId).FirstOrDefault();
            return View(ley);
        }

        // POST: Leyes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ley ley = db.Leyes.Find(id);
            db.Leyes.Remove(ley);
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
