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
    public class TipoOtraInformacionController : BaseController
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
                var Model = db.TipoOtraInformacion.Find(id);
                var oldModel = (TipoOtraInformacion)Model.Clone();
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
                this.CreateBitacora(oldModel, Model, Model.TipoOtraInformacionId);
                
            }
            catch (Exception)
            {

            }
            //return View("Index",horariosModels);
            return activo;

        }

        // GET: Ciudades
        public ActionResult Index(FiltrosTipoOtraInfo filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            string[] searchstringsNombre = !string.IsNullOrEmpty(filtros.TipoOtraInfoNombre) ? filtros.TipoOtraInfoNombre.Split(' ') : "".Split(' ');
            string[] searchstringsDesc = !string.IsNullOrEmpty(filtros.TipoOtraInfoDesc) ? filtros.TipoOtraInfoDesc.Split(' ') : "".Split(' ');
            

            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderDesc = sOrder == "Desc" ? "Desc_desc" : "Desc";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";

            IPagedList<TipoOtraInformacion> vModel = null;

            var query = db.TipoOtraInformacion.Where(x =>
                   ((filtros.TipoOtraInfoNombre == null || filtros.TipoOtraInfoNombre.Length == 0) || searchstringsNombre.Any(y => x.Nombre.ToLower().Contains(y.ToLower())))
                   && ((filtros.TipoOtraInfoDesc == null || filtros.TipoOtraInfoDesc.Length == 0) || searchstringsDesc.Any(y => x.Descripcion.ToLower().Contains(y.ToLower())))
                   && ( filtros.TipoOtraInfoActivo == null || x.Activo == filtros.TipoOtraInfoActivo.Value)
                   );
            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Desc":
                    vModel = query.OrderBy(x => x.Descripcion).ToPagedList(iPagina, PerPage);
                    break;
                case "Desc_desc":
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
                return PartialView("_list", vModel);
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
            TipoOtraInformacion model  = db.TipoOtraInformacion.FirstOrDefault(c=>c.TipoOtraInformacionId == id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        // GET: Ciudades/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Ciudades/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TipoOtraInformacion model)
        {
            if (ModelState.IsValid)
            {
                model.Activo = true;
                db.TipoOtraInformacion.Add(model);
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(null, model, model.TipoOtraInformacionId);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Ciudades/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoOtraInformacion model = db.TipoOtraInformacion.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        // POST: Ciudades/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TipoOtraInformacionId,Nombre,Descripcion,Activo")] TipoOtraInformacion model)
        {
            if (ModelState.IsValid)
            {
                //buscamos modelos Old
                ApplicationDbContext dbOld = new ApplicationDbContext();
                var oldModel = dbOld.TipoOtraInformacion.Where(x => x.TipoOtraInformacionId == model.TipoOtraInformacionId).FirstOrDefault();
                //guardamos la informaciond e la ciudad 
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(oldModel, model, model.TipoOtraInformacionId);
                return RedirectToAction("Index");
            }
            return View(model);
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


        //public JsonResult ObtenerCiudadesDeEstado(string id)
        //{
        //    int intId = Convert.ToInt32(id);
        //    IEnumerable<SelectListItem> ciudades = db.Ciudades.Where(c => c.EstadoId == intId && c.Activo).OrderBy(c => c.Orden).
        //        Select(c => new SelectListItem
        //        {
        //            Value = c.CiudadId.ToString(),
        //            Text = c.NombreCiudad
        //        });

        //    return Json(ciudades, JsonRequestBehavior.AllowGet);
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
        private void CreateBitacora(TipoOtraInformacion oldModel, TipoOtraInformacion newModel, int id = 0)
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
                    nombre_campo = "Descripción",
                    es_modificado = oldModel != null && oldModel.Descripcion != newModel.Descripcion ? true : false,
                    campo_nuevo = newModel.Descripcion,
                    campo_anterior = oldModel != null ? oldModel.Descripcion : null
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
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "TipoOtraInformacions", "Tipo Otra información", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
