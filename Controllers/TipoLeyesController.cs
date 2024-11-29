using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Transparencia.Models;

namespace Transparencia.Controllers
{
    [Authorize]
    public class TipoLeyesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: TipoLeyes
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
            IPagedList<TipoLey> vModel = null;



            var query = db.TipoLeyes.Where(x =>
                 ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.Nombre.ToLower().Contains(y.ToLower())))
                 && ((filtros.Descripcion == null || filtros.Descripcion.Length == 0) || searchstringsD.Any(y => x.Descripcion.ToLower().Contains(y.ToLower())))
                 && (filtros.ActivoNull == null || filtros.ActivoNull == x.Activo)
                // && (filtros.EstatusId.Count <= 0 || filtros.EstatusId.Contains(x.Activo)) 
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

        public bool? changeStatus(int id=0)
        {
            var activo = false;
            if (id == 0)
            {
                return null;
            }
            var Model = db.TipoLeyes.Find(id);
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
            ViewBag.CmbActivo = new List <SelectListItem> {
                new SelectListItem { Text = "Activo", Value = "true" },
                new SelectListItem { Text = "Inactivo", Value = "false" }
            };
        }

        // GET: TipoLeyes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoLey tipoLey = db.TipoLeyes.Find(id);
            if (tipoLey == null)
            {
                return HttpNotFound();
            }
            return View(tipoLey);
        }

        // GET: TipoLeyes/Create
        public ActionResult Create()
        {
            
            return View();
        }

        // POST: TipoLeyes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TipoLeyId,Nombre,Descripcion")] TipoLey tipoLey)
        {
            if (ModelState.IsValid)
            {
                tipoLey.Activo = true;
                db.TipoLeyes.Add(tipoLey);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tipoLey);
        }

        // GET: TipoLeyes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoLey tipoLey = db.TipoLeyes.Find(id);
            if (tipoLey == null)
            {
                return HttpNotFound();
            }
            return View(tipoLey);
        }

        // POST: TipoLeyes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TipoLeyId,Nombre,Descripcion,Activo")] TipoLey tipoLey)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tipoLey).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tipoLey);
        }

        // GET: TipoLeyes/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TipoLey tipoLey = db.TipoLeyes.Find(id);
        //    if (tipoLey == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(tipoLey);
        //}

        // POST: TipoLeyes/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    TipoLey tipoLey = db.TipoLeyes.Find(id);
        //    db.TipoLeyes.Remove(tipoLey);
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

        public void CreateBitacora(TipoLey oldModel, TipoLey newModel, int id = 0)
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
                    nombre_campo = "Orden",
                    es_modificado = oldModel != null && oldModel.Descripcion != newModel.Descripcion ? true : false,
                    campo_nuevo = newModel.Descripcion.ToString(),
                    campo_anterior = oldModel != null ? oldModel.Descripcion.ToString() : null
                });

                //Activo
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Activo",
                    es_modificado = oldModel != null && oldModel.Activo != newModel.Activo ? true : false,
                    campo_nuevo = HMTLHelperExtensions.getStringBoolBitacora(newModel.Activo),
                    campo_anterior = HMTLHelperExtensions.getStringBoolBitacora(oldModel.Activo)
                });

                //Bitacora
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "TipoLeyes", "Tipo de leyes", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
