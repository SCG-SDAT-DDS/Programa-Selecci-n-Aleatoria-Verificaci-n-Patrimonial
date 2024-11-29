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
    public class UnidadesAdministrativasController : BaseController
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
                var Model = db.UnidadesAdministrativas.Find(id);
                var oldModel = (UnidadAdministrativa)Model.Clone();
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
                this.CreateBitacora(oldModel, Model, Model.UnidadAdministrativaId);

            }
            catch (Exception)
            {
            }
            return activo;

        }

        // GET: UnidadesAdministrativas
        public ActionResult Index(FiltrosUnidades filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            string[] searchstringsN = !string.IsNullOrEmpty(filtros.Nombre) ? filtros.Nombre.Split(' ') : "".Split(' ');
            string[] searchstringsD = !string.IsNullOrEmpty(filtros.Descripcion) ? filtros.Descripcion.Split(' ') : "".Split(' ');
            string[] searchstringsO = !string.IsNullOrEmpty(filtros.Organismo) ? filtros.Organismo.Split(' ') : "".Split(' ');
            string[] searchstringsS = !string.IsNullOrEmpty(filtros.Siglas) ? filtros.Siglas.Split(' ') : "".Split(' ');

            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderOrganismo = sOrder == "Organismo" ? "Organismo_desc" : "Organismo";
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderDescripcion = sOrder == "Descripcion" ? "Descripcion_desc" : "Descripcion";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";
            ViewBag.OrderSiglas = sOrder == "Siglas" ? "Siglas_desc" : "Siglas";
            IPagedList<UnidadAdministrativa> vModel = null;



            var query = db.UnidadesAdministrativas.Include(o=>o.Organismos).Where(x =>
                 ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.NombreUnidad.ToLower().Contains(y.ToLower())))
                 && ((filtros.Organismo == null || filtros.Organismo.Length == 0) || searchstringsO.Any(y => x.Organismos.NombreOrganismo.ToLower().Contains(y.ToLower())))
                 && ((filtros.Descripcion == null || filtros.Descripcion.Length == 0) || searchstringsD.Any(y => x.Descripcion.ToLower().Contains(y.ToLower())))
                 && ((filtros.Siglas == null || filtros.Siglas.Length == 0) || searchstringsS.Any(y => x.Siglas.ToLower().Contains(y.ToLower())))
                 );
            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.NombreUnidad).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.NombreUnidad).ToPagedList(iPagina, PerPage);
                    break;
                case "Organismo":
                    vModel = query.OrderBy(x => x.Organismos.NombreOrganismo).ToPagedList(iPagina, PerPage);
                    break;
                case "Organismo_desc":
                    vModel = query.OrderByDescending(x => x.Organismos.NombreOrganismo).ToPagedList(iPagina, PerPage);
                    break;
                case "Descripcion":
                    vModel = query.OrderBy(x => x.Descripcion).ToPagedList(iPagina, PerPage);
                    break;
                case "Descripcion_desc":
                    vModel = query.OrderByDescending(x => x.Descripcion).ToPagedList(iPagina, PerPage);
                    break;
                case "Siglas":
                    vModel = query.OrderBy(x => x.Siglas).ToPagedList(iPagina, PerPage);
                    break;
                case "Siglas_desc":
                    vModel = query.OrderByDescending(x => x.Siglas).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo":
                    vModel = query.OrderBy(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo_desc":
                    vModel = query.OrderByDescending(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                default:
                    vModel = query.OrderBy(x => x.NombreUnidad).ToPagedList(iPagina, PerPage);
                    break;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("_listaUnidades", vModel);
            }
            return View(vModel);
        }

        // GET: UnidadesAdministrativas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UnidadAdministrativa unidadAdministrativa = db.UnidadesAdministrativas.Include(u=>u.Organismos).FirstOrDefault(u=>u.UnidadAdministrativaId == id);
            if (unidadAdministrativa == null)
            {
                return HttpNotFound();
            }
            return View(unidadAdministrativa);
        }

        // GET: UnidadesAdministrativas/Create
        public ActionResult Create()
        {
            ViewBag.OrganismoId = new SelectList(db.Organismos, "OrganismoID", "NombreOrganismo");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UnidadAdministrativa unidadAdministrativa)
        {
            if (ModelState.IsValid)
            {
                unidadAdministrativa.Activo = true;
                db.UnidadesAdministrativas.Add(unidadAdministrativa);
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(null, unidadAdministrativa, unidadAdministrativa.UnidadAdministrativaId);

                return RedirectToAction("Index");
            }

            ViewBag.OrganismoId = new SelectList(db.Organismos, "OrganismoID", "NombreOrganismo", unidadAdministrativa.OrganismoId);
            return View(unidadAdministrativa);
        }

        // GET: UnidadesAdministrativas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UnidadAdministrativa unidadAdministrativa = db.UnidadesAdministrativas.Find(id);
            if (unidadAdministrativa == null)
            {
                return HttpNotFound();
            }
            ViewBag.OrganismoId = new SelectList(db.Organismos, "OrganismoID", "NombreOrganismo", unidadAdministrativa.OrganismoId);
            return View(unidadAdministrativa);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UnidadAdministrativa unidadAdministrativa)
        {
            if (ModelState.IsValid)
            {
                //para la bitacora
                ApplicationDbContext dbOld = new ApplicationDbContext();
                var oldModel = dbOld.UnidadesAdministrativas.Where(x => x.UnidadAdministrativaId == unidadAdministrativa.UnidadAdministrativaId).FirstOrDefault();

                db.Entry(unidadAdministrativa).State = EntityState.Modified;
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(oldModel, unidadAdministrativa, unidadAdministrativa.UnidadAdministrativaId);

                return RedirectToAction("Index");
            }
            ViewBag.OrganismoId = new SelectList(db.Organismos, "OrganismoID", "NombreOrganismo", unidadAdministrativa.OrganismoId);
            return View(unidadAdministrativa);
        }

        public ActionResult GetUAByOrganismoId(int iId = 0)
        {
            var UA = db.UnidadesAdministrativas.Where(x => x.OrganismoId == iId && x.Activo).ToList();
            return Json(new { data = UA, Encontro = UA != null }, JsonRequestBehavior.AllowGet);
        }

        //// GET: UnidadesAdministrativas/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    UnidadAdministrativa unidadAdministrativa = db.UnidadesAdministrativas.Include(u => u.Organismos).FirstOrDefault(u => u.UnidadAdministrativaId == id);
        //    if (unidadAdministrativa == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(unidadAdministrativa);
        //}

        //// POST: UnidadesAdministrativas/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    UnidadAdministrativa unidadAdministrativa = db.UnidadesAdministrativas.Find(id);
        //    unidadAdministrativa.Activo = false;
        //    db.Entry(unidadAdministrativa).State = EntityState.Modified;
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
        private void CreateBitacora(UnidadAdministrativa oldModel, UnidadAdministrativa newModel, int id = 0)
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
                    es_modificado = oldModel != null && oldModel.NombreUnidad != newModel.NombreUnidad ? true : false,
                    campo_nuevo = newModel.NombreUnidad,
                    campo_anterior = oldModel != null ? oldModel.NombreUnidad : null
                });

                //Organismo
                string newOrgsanismo = "";
                string oldOrganismo = "";

                if (newModel != null && newModel.OrganismoId != 0)
                {
                    newOrgsanismo = db.Organismos.Where(x => x.OrganismoID == newModel.OrganismoId).FirstOrDefault()?.NombreOrganismo;
                }
                if (oldModel != null && oldModel.OrganismoId != 0)
                {
                    oldOrganismo = db.Organismos.Where(x => x.OrganismoID == oldModel.OrganismoId).FirstOrDefault()?.NombreOrganismo;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Organismo",
                    es_modificado = oldModel != null && oldModel.OrganismoId != newModel.OrganismoId ? true : false,
                    campo_nuevo = newOrgsanismo,
                    campo_anterior = oldOrganismo
                });

                //Descripción
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Descripción",
                    es_modificado = oldModel != null && oldModel.Descripcion != newModel.Descripcion ? true : false,
                    campo_nuevo = newModel.Descripcion,
                    campo_anterior = oldModel != null ? oldModel.Descripcion : null
                });

                //Siglas
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Siglas",
                    es_modificado = oldModel != null && oldModel.Siglas != newModel.Siglas ? true : false,
                    campo_nuevo = newModel.Siglas,
                    campo_anterior = oldModel != null ? oldModel.Siglas : null
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
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "UnidadAdministrativas", "Unidades Administrativas", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
