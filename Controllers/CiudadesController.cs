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
    public class CiudadesController : BaseController
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
                var Model = db.Ciudades.Find(id);
                var oldModel = (Ciudad)Model.Clone();
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
                this.CreateBitacora(oldModel, Model, Model.CiudadId);
                
            }
            catch (Exception)
            {

            }
            //return View("Index",horariosModels);
            return activo;

        }

        // GET: Ciudades
        public ActionResult Index(FiltrosCiudades filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            string[] searchstringsN = !string.IsNullOrEmpty(filtros.Nombre) ? filtros.Nombre.Split(' ') : "".Split(' ');
            string[] searchstringsE = !string.IsNullOrEmpty(filtros.Estado) ? filtros.Estado.Split(' ') : "".Split(' ');
            string[] searchstringsP = !string.IsNullOrEmpty(filtros.Pais) ? filtros.Pais.Split(' ') : "".Split(' ');

            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderEstado = sOrder == "Estado" ? "Estado_desc" : "Estado";
            ViewBag.OrderPais = sOrder == "Pais" ? "Pais_desc" : "Pais";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";

            IPagedList<Ciudad> vModel = null;

            var query = db.Ciudades.Include(e => e.Estado.Pais).Include(e=>e.Estado).Where(x =>
                   ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.NombreCiudad.ToLower().Contains(y.ToLower())))
                   && ((filtros.Estado == null || filtros.Estado.Length == 0) || searchstringsE.Any(y => x.Estado.NombreEstado.ToLower().Contains(y.ToLower())))
                   && ((filtros.Pais == null || filtros.Pais.Length == 0) || searchstringsP.Any(y => x.Estado.Pais.NombrePais.ToLower().Contains(y.ToLower())))
                   );
            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.NombreCiudad).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.NombreCiudad).ToPagedList(iPagina, PerPage);
                    break;
                case "Estado":
                    vModel = query.OrderBy(x => x.Estado.NombreEstado).ToPagedList(iPagina, PerPage);
                    break;
                case "Estado_desc":
                    vModel = query.OrderByDescending(x => x.Estado.NombreEstado).ToPagedList(iPagina, PerPage);
                    break;
                case "Pais":
                    vModel = query.OrderBy(x => x.Estado.Pais.NombrePais).ToPagedList(iPagina, PerPage);
                    break;
                case "Pais_desc":
                    vModel = query.OrderByDescending(x => x.Estado.Pais.NombrePais).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo":
                    vModel = query.OrderBy(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo_desc":
                    vModel = query.OrderByDescending(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                default:
                    vModel = query.OrderBy(x => x.NombreCiudad).ToPagedList(iPagina, PerPage);
                    break;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("_listaCiudades", vModel);
            }
            return View(vModel);
        }

        // GET: Ciudades/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ciudad ciudad = db.Ciudades.Include(c => c.Estado).FirstOrDefault(c=>c.CiudadId == id);
            if (ciudad == null)
            {
                return HttpNotFound();
            }
            return View(ciudad);
        }

        // GET: Ciudades/Create
        public ActionResult Create()
        {
            ViewBag.EstadoId = new SelectList(db.Estados, "EstadoId", "NombreEstado");
            return View();
        }

        // POST: Ciudades/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Ciudad ciudad)
        {
            if (ModelState.IsValid)
            {
                ciudad.Activo = true;
                db.Ciudades.Add(ciudad);
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(null, ciudad, ciudad.CiudadId);
                return RedirectToAction("Index");
            }

            ViewBag.EstadoId = new SelectList(db.Estados, "EstadoId", "NombreEstado", ciudad.EstadoId);
            return View(ciudad);
        }

        // GET: Ciudades/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ciudad ciudad = db.Ciudades.Find(id);
            if (ciudad == null)
            {
                return HttpNotFound();
            }
            ViewBag.EstadoId = new SelectList(db.Estados, "EstadoId", "NombreEstado", ciudad.EstadoId);
            return View(ciudad);
        }

        // POST: Ciudades/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CiudadId,NombreCiudad,EstadoId,Orden,Activo")] Ciudad ciudad)
        {
            if (ModelState.IsValid)
            {
                //buscamos modelos Old
                ApplicationDbContext dbOld = new ApplicationDbContext();
                var oldModel = dbOld.Ciudades.Where(x => x.CiudadId == ciudad.CiudadId).FirstOrDefault();
                //guardamos la informaciond e la ciudad 
                db.Entry(ciudad).State = EntityState.Modified;
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(oldModel, ciudad, ciudad.CiudadId);
                return RedirectToAction("Index");
            }
            ViewBag.EstadoId = new SelectList(db.Estados, "EstadoId", "NombreEstado", ciudad.EstadoId);
            return View(ciudad);
        }

        // GET: Ciudades/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    Ciudad ciudad = db.Ciudades.Include(c => c.Estado).FirstOrDefault(c => c.CiudadId == id);

        //    if (ciudad == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    return View(ciudad);
        //}

        //// POST: Ciudades/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Ciudad ciudad = db.Ciudades.Find(id);
        //    ciudad.Activo = false;
        //    db.Entry(ciudad).State = EntityState.Modified;
        //    db.SaveChanges();

        //    return RedirectToAction("Index");
        //}


        public JsonResult ObtenerCiudadesDeEstado(string id)
        {
            int intId = Convert.ToInt32(id);
            IEnumerable<SelectListItem> ciudades = db.Ciudades.Where(c => c.EstadoId == intId && c.Activo).OrderBy(c => c.Orden).
                Select(c => new SelectListItem
                {
                    Value = c.CiudadId.ToString(),
                    Text = c.NombreCiudad
                });

            return Json(ciudades, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ObtenerTodasLasCiudades()
        {

            IEnumerable<SelectListItem> ciudades = db.Ciudades.
                Where(x => x.Activo)
                .Select(c => new SelectListItem
                {
                    Value = c.CiudadId.ToString(),
                    Text = c.NombreCiudad
                });

            return Json(ciudades, JsonRequestBehavior.AllowGet);
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
        private void CreateBitacora(Ciudad oldModel, Ciudad newModel, int id = 0)
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
                    es_modificado = oldModel != null && oldModel.NombreCiudad != newModel.NombreCiudad ? true : false,
                    campo_nuevo = newModel.NombreCiudad,
                    campo_anterior = oldModel != null ? oldModel.NombreCiudad : null
                });

                //Estado
                string newEstado = "";
                string oldEstado = "";

                if (newModel != null && newModel.EstadoId != 0)
                {
                    newEstado = db.Estados.Where(x => x.EstadoId == newModel.EstadoId).FirstOrDefault()?.NombreEstado;
                }
                if (oldModel != null && oldModel.EstadoId != 0)
                {
                    oldEstado = db.Estados.Where(x => x.EstadoId == oldModel.EstadoId).FirstOrDefault()?.NombreEstado;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Estado",
                    es_modificado = oldModel != null && oldModel.EstadoId != newModel.EstadoId ? true : false,
                    campo_nuevo = newEstado,
                    campo_anterior = oldEstado
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
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "Ciudads", "Municipios", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
