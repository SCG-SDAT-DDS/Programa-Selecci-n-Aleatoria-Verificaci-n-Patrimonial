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
    public class EstadosController : BaseController
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
                var Model = db.Estados.Find(id);
                var oldModel = (Estado)Model.Clone();
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
                this.CreateBitacora(oldModel, Model, Model.EstadoId);
            }
            catch (Exception)
            {

            }
            return activo;

        }

        // GET: Estados
        public ActionResult Index(FiltrosCiudades filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            string[] searchstringsN = !string.IsNullOrEmpty(filtros.Nombre) ? filtros.Nombre.Split(' ') : "".Split(' ');
            string[] searchstringsP = !string.IsNullOrEmpty(filtros.Pais) ? filtros.Pais.Split(' ') : "".Split(' ');

            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderPais = sOrder == "Pais" ? "Pais_desc" : "Pais";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";

            IPagedList<Estado> vModel = null;

            var query = db.Estados.Include(e => e.Pais).Where(x =>
                     ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.NombreEstado.ToLower().Contains(y.ToLower())))
                     && ((filtros.Pais == null || filtros.Pais.Length == 0) || searchstringsP.Any(y => x.Pais.NombrePais.ToLower().Contains(y.ToLower())))
                   );
            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.NombreEstado).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.NombreEstado).ToPagedList(iPagina, PerPage);
                    break;
                case "Pais":
                    vModel = query.OrderBy(x => x.Pais.NombrePais).ToPagedList(iPagina, PerPage);
                    break;
                case "Pais_desc":
                    vModel = query.OrderByDescending(x => x.Pais.NombrePais).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo":
                    vModel = query.OrderBy(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo_desc":
                    vModel = query.OrderByDescending(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                default:
                    vModel = query.OrderBy(x => x.NombreEstado).ToPagedList(iPagina, PerPage);
                    break;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("_listaEstados", vModel);
            }
            return View(vModel);
        }

        // GET: Estados/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Estado estado = db.Estados.Include(e=>e.Pais).FirstOrDefault(e=>e.EstadoId == id);
            if (estado == null)
            {
                return HttpNotFound();
            }
            return View(estado);
        }

        // GET: Estados/Create
        public ActionResult Create()
        {
            ViewBag.PaisId = new SelectList(db.Paises, "PaisId", "NombrePais");
            return View();
        }

        // POST: Estados/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EstadoId,NombreEstado,PaisId,Orden,Activo")] Estado estado)
        {
            if (ModelState.IsValid)
            {
                estado.Activo = true;
                db.Estados.Add(estado);
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(null, estado, estado.EstadoId);

                return RedirectToAction("Index");
            }

            ViewBag.PaisId = new SelectList(db.Paises, "PaisId", "NombrePais", estado.PaisId);
            return View(estado);
        }

        // GET: Estados/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Estado estado = db.Estados.Find(id);
            if (estado == null)
            {
                return HttpNotFound();
            }
            ViewBag.PaisId = new SelectList(db.Paises, "PaisId", "NombrePais", estado.PaisId);
            return View(estado);
        }

        // POST: Estados/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EstadoId,NombreEstado,PaisId,Orden,Activo")] Estado estado)
        {
            if (ModelState.IsValid)
            {
                //para la bitacora
                ApplicationDbContext dbOld = new ApplicationDbContext();
                var oldModel = dbOld.Estados.Where(x => x.EstadoId == estado.EstadoId).FirstOrDefault();

                db.Entry(estado).State = EntityState.Modified;
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(oldModel, estado, estado.EstadoId);

                return RedirectToAction("Index");
            }
            ViewBag.PaisId = new SelectList(db.Paises, "PaisId", "NombrePais", estado.PaisId);
            return View(estado);
        }

        //// GET: Estados/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Estado estado = db.Estados.Include(e=>e.Pais).FirstOrDefault(e=>e.EstadoId == id );
        //    if (estado == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(estado);
        //}

        //// POST: Estados/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Estado estado = db.Estados.Find(id);
        //    estado.Activo = false;
        //    db.Entry(estado).State = EntityState.Modified;
        //    db.SaveChanges();

        //    return RedirectToAction("Index");
        //}

        public JsonResult ObtenerEstadosDePais(string id)
        {
            int intId = Convert.ToInt32(id);
            IEnumerable<SelectListItem> estados = db.Estados.Where(c => c.PaisId == intId && c.Activo).OrderBy(c=>c.Orden).
                Select(c=>new SelectListItem  {
                    Value= c.EstadoId.ToString(),
                    Text= c.NombreEstado
                });

            return Json(estados, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ObtenerTodosLosEstados()
        {
            
            IEnumerable<SelectListItem> estados = db.Estados.
                Where(x => x.Activo)
                .Select(c => new SelectListItem
                {
                    Value = c.EstadoId.ToString(),
                    Text = c.NombreEstado
                });

            return Json(estados, JsonRequestBehavior.AllowGet);
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
        private void CreateBitacora(Estado oldModel, Estado newModel, int id = 0)
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
                    es_modificado = oldModel != null && oldModel.NombreEstado != newModel.NombreEstado ? true : false,
                    campo_nuevo = newModel.NombreEstado,
                    campo_anterior = oldModel != null ? oldModel.NombreEstado : null
                });

                //Pais
                string newPais = "";
                string oldPais = "";

                if (newModel != null && newModel.PaisId != 0)
                {
                    newPais = db.Paises.Where(x => x.PaisId == newModel.PaisId).FirstOrDefault()?.NombrePais;
                }
                if (oldModel != null && oldModel.PaisId != 0)
                {
                    oldPais = db.Paises.Where(x => x.PaisId == oldModel.PaisId).FirstOrDefault()?.NombrePais;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Pais",
                    es_modificado = oldModel != null && oldModel.PaisId != newModel.PaisId ? true : false,
                    campo_nuevo = newPais,
                    campo_anterior = oldPais
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
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "Estadoes", "Estados", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
