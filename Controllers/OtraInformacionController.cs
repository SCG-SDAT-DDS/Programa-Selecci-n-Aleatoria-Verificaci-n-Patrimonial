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
    public class OtraInformacionController : BaseController
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
                var Model = db.OtraInformacion.Find(id);
                var oldModel = (OtraInformacion)Model.Clone();
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
                this.CreateBitacora(oldModel, Model, Model.OtraInformacionId);
                
            }
            catch (Exception)
            {

            }
            //return View("Index",horariosModels);
            return activo;

        }

        public void getRole()
        {
            ViewBag.Administrador = HMTLHelperExtensions.GetRoles(User.Identity.Name, "Administrador");
            ViewBag.ListOrganismo = new SelectList(db.Organismos.Where(x => x.Activo), "OrganismoID", "NombreOrganismo");
        }

        // GET: Ciudades
        public ActionResult Index(FiltrosOtraInfo filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            bool Administrador = false;
            ViewBag.TipoOtraInformacion = new SelectList(db.TipoOtraInformacion.Where(x => x.Activo), "TipoOtraInformacionId", "Nombre");
            ViewBag.Administrador = Administrador = HMTLHelperExtensions.GetRoles(User.Identity.Name, "Administrador");
            ViewBag.ListOrganismo = new SelectList(db.Organismos.Where(x => x.Activo), "OrganismoID", "NombreOrganismo");
            if (!Administrador)
            {
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                filtros.OtraInfoOrganismoId = usuario.OrganismoID.HasValue ? usuario.OrganismoID.Value : 0;
            }

            
            string[] searchstringsNombre = !string.IsNullOrEmpty(filtros.OtraInfoNombre) ? filtros.OtraInfoNombre.Split(' ') : "".Split(' ');
            string[] searchstringsNotas = !string.IsNullOrEmpty(filtros.OtraInfoNotas) ? filtros.OtraInfoNotas.Split(' ') : "".Split(' ');
            string[] searchstringsURL = !string.IsNullOrEmpty(filtros.OtraInfoURL) ? filtros.OtraInfoURL.Split(' ') : "".Split(' ');

            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderNotas = sOrder == "Notas" ? "Notas_desc" : "Notas";
            ViewBag.OrderUrl = sOrder == "URL" ? "URL_desc" : "URL";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";

            IPagedList<OtraInformacion> vModel = null;

            var query = db.OtraInformacion.Where(x =>
                   ((filtros.OtraInfoNombre == null || filtros.OtraInfoNombre.Length == 0) || searchstringsNombre.Any(y => x.Nombre.ToLower().Contains(y.ToLower())))
                   && ((filtros.OtraInfoNotas == null || filtros.OtraInfoNotas.Length == 0) || searchstringsNotas.Any(y => x.Notas.ToLower().Contains(y.ToLower())))
                   && ((filtros.OtraInfoURL == null || filtros.OtraInfoURL.Length == 0) || searchstringsURL.Any(y => x.URL.ToLower().Contains(y.ToLower())))
                   && ( filtros.TipoOtraInformacionId == 0 || x.TipoOtraInformacionId == filtros.TipoOtraInformacionId )
                   && ( filtros.OtraInfoOrganismoId == 0 || x.OrganismoID == filtros.OtraInfoOrganismoId)                 
                   );
            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.Nombre).ToPagedList(iPagina, PerPage);
                    break;
                case "Notas":
                    vModel = query.OrderBy(x => x.Notas).ToPagedList(iPagina, PerPage);
                    break;
                case "Notas_desc":
                    vModel = query.OrderByDescending(x => x.Notas).ToPagedList(iPagina, PerPage);
                    break;
                case "URL":
                    vModel = query.OrderBy(x => x.URL).ToPagedList(iPagina, PerPage);
                    break;
                case "URL_desc":
                    vModel = query.OrderByDescending(x => x.URL).ToPagedList(iPagina, PerPage);
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
            //vModel = vModel.Select(x => new IPagedList<OtraInformacion> {
            //     x.Nombre = x.Nombre,
            //     x.Notas = x.Notas.Substring(0,30),
            //     x.URL = x.URL.Substring(0,30)
            //});
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
            OtraInformacion model  = db.OtraInformacion.FirstOrDefault(c=>c.OtraInformacionId == id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        // GET: Ciudades/Create
        public ActionResult Create()
        {
            getRole();
            ViewBag.TipoOtraInformacion = new SelectList(db.TipoOtraInformacion.Where(x =>x.Activo), "TipoOtraInformacionId", "Nombre");
            return View();
        }

        // POST: Ciudades/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(OtraInformacion model)
        {
            getRole();
            if (model.OrganismoID == 0)
            {
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                model.OrganismoID = usuario.OrganismoID.HasValue ? usuario.OrganismoID.Value : 0;

            }
            if (ModelState.IsValid)
            {
                model.Activo = true;
                db.OtraInformacion.Add(model);
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(null, model, model.OtraInformacionId);
                return RedirectToAction("Index");
            }

            ViewBag.TipoOtraInformacion = new SelectList(db.TipoOtraInformacion.Where(x => x.Activo), "TipoOtraInformacionId", "Nombre",model.TipoOtraInformacionId);
            return View(model);
        }

        // GET: Ciudades/Edit/5
        public ActionResult Edit(int? id)
        {
            getRole();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OtraInformacion model = db.OtraInformacion.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            ViewBag.TipoOtraInformacion = new SelectList(db.TipoOtraInformacion.Where(x => x.Activo), "TipoOtraInformacionId", "Nombre", model.TipoOtraInformacionId);
            return View(model);
        }

        // POST: Ciudades/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OtraInformacionId,Nombre,TipoOtraInformacionId,URL,Notas,OrganismoID,Activo")] OtraInformacion model)
        {
            getRole();
            if (model.OrganismoID == 0)
            {
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                model.OrganismoID = usuario.OrganismoID.HasValue ? usuario.OrganismoID.Value : 0 ;

            }
            if (ModelState.IsValid)
            {
                //buscamos modelos Old
                ApplicationDbContext dbOld = new ApplicationDbContext();
                var oldModel = dbOld.OtraInformacion.Where(x => x.OtraInformacionId == model.OtraInformacionId).FirstOrDefault();
                //guardamos la informaciond e la ciudad 
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(oldModel, model, model.OtraInformacionId);
                return RedirectToAction("Index");
            }
            ViewBag.TipoOtraInformacion = new SelectList(db.TipoOtraInformacion.Where(x => x.Activo), "TipoOtraInformacionId", "Nombre", model.TipoOtraInformacionId);
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
        private void CreateBitacora(OtraInformacion oldModel, OtraInformacion newModel, int id = 0)
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
                //Nombre
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Notas",
                    es_modificado = oldModel != null && oldModel.Notas != newModel.Notas ? true : false,
                    campo_nuevo = newModel.Notas,
                    campo_anterior = oldModel != null ? oldModel.Notas : null
                });
                //URL
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "URL",
                    es_modificado = oldModel != null && oldModel.URL != newModel.URL ? true : false,
                    campo_nuevo = newModel.URL,
                    campo_anterior = oldModel != null ? oldModel.URL : null
                });

                //Tipo
                string newTipo = "";
                string oldTipo = "";

                if (newModel != null && newModel.TipoOtraInformacionId != 0)
                {
                    newTipo = db.TipoOtraInformacion.Where(x => x.TipoOtraInformacionId == newModel.TipoOtraInformacionId).FirstOrDefault()?.Nombre;
                }
                if (oldModel != null && oldModel.TipoOtraInformacionId != 0)
                {
                    oldTipo = db.TipoOtraInformacion.Where(x => x.TipoOtraInformacionId == oldModel.TipoOtraInformacionId).FirstOrDefault()?.Nombre;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Tipo otra información",
                    es_modificado = oldModel != null && oldModel.TipoOtraInformacionId != newModel.TipoOtraInformacionId ? true : false,
                    campo_nuevo = newTipo,
                    campo_anterior = oldTipo
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
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "OtraInformacions", "Otra información", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
