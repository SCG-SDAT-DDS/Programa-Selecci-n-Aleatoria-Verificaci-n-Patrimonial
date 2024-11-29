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
    public class ArticulosController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public bool? changeStatus(int id = 0)
        {
            //var oldModel = new Articulo();
            var activo = false;
            try
            {
                if (id == 0)
                {
                    return null;
                }
                var Model = db.Articulos.Find(id);
                var oldModel = (Articulo)Model.Clone();
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
                this.CreateBitacora(oldModel, Model, Model.ArticuloId);

            }
            catch(Exception ex)
            {

            }
            //return View("Index",horariosModels);
            return activo;
        }

        private void combos()
        {
            ViewBag.CmbActivo = new List<SelectListItem>();
            ViewBag.CmbLey = new List<SelectListItem>();
            
            

            ViewBag.CmbLey = new SelectList(db.Leyes.OrderBy(x => x.Nombre).Select(x => new { LeyId = x.LeyId, Nombre = x.Nombre + " (" + x.TipoLeyes.Nombre + ")" }), "LeyId", "Nombre");
            ViewBag.CmbActivo = new List<SelectListItem> {
                new SelectListItem { Text = "Activo", Value = "true" },
                new SelectListItem { Text = "Inactivo", Value = "false" }
            };
        }
        // GET: Articulos
        public ActionResult Index(FiltrosArticulo filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            combos();
            string[] searchstringsN = filtros.Nombre != null && filtros.Nombre.Length > 0 ? filtros.Nombre.Split(' ') : "".Split(' ');
            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderLey = sOrder == "Ley" ? "Ley_desc" : "Ley";
            ViewBag.OrderOrden = sOrder == "Orden" ? "Orden_desc" : "Orden";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";
            IPagedList<Articulo> vModel = null;


            var query = db.Articulos.Include(a => a.Leyes).Include(x => x.Leyes.TipoLeyes).Where(x =>
            ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.Nombre.ToLower().Contains(y.ToLower())))
            && (filtros.ActivoNull == null || filtros.ActivoNull == x.Activo)
            && (filtros.Ley == 0 || filtros.Ley == x.LeyId)
            );


            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Ley":
                    vModel = query.OrderBy(x => x.Leyes.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Ley_desc":
                    vModel = query.OrderByDescending(x => x.Leyes.Nombre).ToPagedList(iPagina, PerPage);
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
                case "Activo_desc3":
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

        // GET: Articulos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Articulo articulo = db.Articulos.Find(id);
            if (articulo == null) 
            {
                return HttpNotFound();
            }
            if (articulo.Leyes == null)
                articulo.Leyes = db.Leyes.Where(x => x.LeyId == articulo.LeyId).Include(x=>x.TipoLeyes).FirstOrDefault();
            return View(articulo);
        }

        // GET: Articulos/Create
        public ActionResult Create()
        {
            ViewBag.LeyId = new SelectList(db.Leyes.Where(x=>x.Activo).Select(x=> new { LeyId = x.LeyId, Nombre=x.Nombre+" ("+x.TipoLeyes.Nombre+")" }), "LeyId", "Nombre");
            return View();
        }

        // POST: Articulos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ArticuloId,Nombre,LeyId,Ayuda,Orden")] Articulo articulo)
        {
            if (ModelState.IsValid)
            {
                articulo.Activo = true;
                db.Articulos.Add(articulo);
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(null, articulo, articulo.ArticuloId);


                return RedirectToAction("Index");
            }

            ViewBag.LeyId = new SelectList(db.Leyes.Select(x => new { LeyId = x.LeyId, Nombre = x.Nombre + " (" + x.TipoLeyes.Nombre + ")" }), "LeyId", "Nombre", articulo.LeyId);
            return View(articulo);
        }

        // GET: Articulos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Articulo articulo = db.Articulos.Find(id);
            if (articulo == null)
            {
                return HttpNotFound();
            }
            ViewBag.LeyId = new SelectList(db.Leyes.Select(x => new { LeyId = x.LeyId, Nombre = x.Nombre + " (" + x.TipoLeyes.Nombre + ")" }), "LeyId", "Nombre", articulo.LeyId);
            return View(articulo);
        }

        // POST: Articulos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ArticuloId,Nombre,LeyId,Ayuda,Orden,Activo")] Articulo articulo)
        {
            if (ModelState.IsValid)
            {
                //para la bitacora
                ApplicationDbContext dbOld = new ApplicationDbContext();
                var oldModel = dbOld.Articulos.Where(x => x.ArticuloId == articulo.ArticuloId).FirstOrDefault();
                db.Entry(articulo).State = EntityState.Modified;
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(oldModel, articulo, articulo.ArticuloId);

                return RedirectToAction("Index");
            }
            ViewBag.LeyId = new SelectList(db.Leyes.Select(x => new { LeyId = x.LeyId, Nombre = x.Nombre + " (" + x.TipoLeyes.Nombre + ")" }), "LeyId", "Nombre", articulo.LeyId);
            return View(articulo);
        }

        // GET: Articulos/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Articulo articulo = db.Articulos.Find(id);
        //    if (articulo == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    if(articulo.Leyes == null)
        //        articulo.Leyes = db.Leyes.Where(x => x.LeyId == articulo.LeyId).Include(x => x.TipoLeyes).FirstOrDefault();
        //    return View(articulo);
        //}

        //// POST: Articulos/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Articulo articulo = db.Articulos.Find(id);
        //    db.Articulos.Remove(articulo);
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
        private void CreateBitacora(Articulo oldModel,Articulo newModel,int id= 0)
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

                //Ley
                string newLey = "";
                string oldLey = "";

                if (newModel != null && newModel.LeyId != 0)
                {
                    newLey = db.Leyes.Where(x => x.LeyId == newModel.LeyId).FirstOrDefault()?.Nombre;
                }
                if (oldModel != null && oldModel.LeyId != 0)
                {
                    oldLey = db.Leyes.Where(x => x.LeyId == oldModel.LeyId).FirstOrDefault()?.Nombre;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Ley",
                    es_modificado = oldModel != null && oldModel.LeyId != newModel.LeyId ? true : false,
                    campo_nuevo = newLey,
                    campo_anterior = oldLey
                });
                //Orden
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Orden",
                    es_modificado = oldModel != null && oldModel.Orden != newModel.Orden ? true : false,
                    campo_nuevo = newModel.Orden.ToString(),
                    campo_anterior = oldModel != null ? oldModel.Orden.ToString() : null
                });

                //Ayuda
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Ayuda",
                    es_modificado = oldModel != null && oldModel.Ayuda != newModel.Ayuda ? true : false,
                    campo_nuevo = newModel.Ayuda,
                    campo_anterior = oldModel != null ? oldModel.Ayuda : null
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
                var res = HMTLHelperExtensions.Bitacora(cambioCampos,"Articuloes", "Artículos", id, usuario?.Id,accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
