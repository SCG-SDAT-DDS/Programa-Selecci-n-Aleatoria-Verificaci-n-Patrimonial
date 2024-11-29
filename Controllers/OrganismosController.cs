using PagedList;
using System;
using System.Collections;
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
    public class OrganismosController : Controller
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
                var Model = db.Organismos.Find(id);
                var oldModel = (Organismo)Model.Clone();
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
                this.CreateBitacora(oldModel, Model, Model.OrganismoID);

            }
            catch (Exception)
            {

            }

            return activo;

        }
        public ActionResult Index(FiltrosUnidades filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            
            string[] searchstringsN = !string.IsNullOrEmpty(filtros.Nombre) ? filtros.Nombre.Split(' ') : "".Split(' ');
            string[] searchstringsS = !string.IsNullOrEmpty(filtros.Siglas) ? filtros.Siglas.Split(' ') : "".Split(' ');

            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";
            ViewBag.OrderSiglas = sOrder == "Siglas" ? "Siglas_desc" : "Siglas";
            IPagedList<Organismo> vModel = null;



            var query = db.Organismos.Where(x =>
                   ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.NombreOrganismo.ToLower().Contains(y.ToLower())))
                   && ((filtros.Siglas == null || filtros.Siglas.Length == 0) || searchstringsS.Any(y => x.Siglas.ToLower().Contains(y.ToLower())))
                 );
            var contar = query.Count();
            var cuantosRows = (contar / PerPage) + 1;
            if(cuantosRows <= iPagina )
            {
                iPagina = 1;
            }
            switch (sOrder)
            {
                case "Nombre":
                    vModel = query.OrderBy(x => x.NombreOrganismo).ToPagedList(iPagina, PerPage);
                    break;
                case "Nombre_desc":
                    vModel = query.OrderByDescending(x => x.NombreOrganismo ).ToPagedList(iPagina, PerPage);
                    break;
                //case "Descripcion":
                //    vModel = query.OrderBy(x => x.Descripcion).ToPagedList(iPagina, PerPage);
                //    break;
                //case "Descripcion_desc":
                //    vModel = query.OrderByDescending(x => x.Descripcion).ToPagedList(iPagina, PerPage);
                //    break;
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
                    vModel = query.OrderBy(x => x.Orden).ToPagedList(iPagina, PerPage);
                    break;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("_listaOrganismos", vModel);
            }
            return View(vModel);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Organismo organismo = db.Organismos.Include(d => d.Ciudad).Include(d => d.Estado).Include(d => d.Pais).
                Include(d => d.TipoOrganismo).Include(d => d.Representante).FirstOrDefault(d => d.OrganismoID == id);
            if (organismo == null)
            {
                return HttpNotFound();
            }
            return View(organismo);
        }

        public ActionResult Create()
        {
            //ViewBag.CiudadId = new SelectList(db.Ciudades, "CiudadId", "NombreCiudad");
            //ViewBag.EstadoId = new SelectList(new ArrayList()/*db.Estados, "EstadoId", "NombreEstado"*/);
            ViewBag.PaisId = new SelectList(db.Paises.Where(x=>x.Activo), "PaisId", "NombrePais");
            ViewBag.RepresentanteId = new SelectList(db.Representantes.Where(x => x.Activo), "RepresentanteId", "NombreCompleto");
            ViewBag.TipoOrganismoId = new SelectList(db.TipoOrganismos.Where(x => x.Activo), "TipoOrganismoId", "Nombre");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Organismo organismo)
        {
            if (ModelState.IsValid)
            {
                organismo.Activo = true;
                db.Organismos.Add(organismo);
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(null, organismo, organismo.OrganismoID);

                return RedirectToAction("Index");
            }

            //ViewBag.CiudadId = new SelectList(new ArrayList()/*db.Ciudades, "CiudadId", "NombreCiudad", organismo.CiudadId*/);
            //ViewBag.EstadoId = new SelectList(new ArrayList()/*db.Estados, "EstadoId", "NombreEstado", organismo.EstadoId*/);
            ViewBag.PaisId = new SelectList(db.Paises.Where(x => x.Activo), "PaisId", "NombrePais", organismo.PaisId);
            ViewBag.RepresentanteId = new SelectList(db.Representantes.Where(x => x.Activo), "RepresentanteId", "NombreCompleto", organismo.RepresentanteId);
            ViewBag.TipoOrganismoId = new SelectList(db.TipoOrganismos.Where(x => x.Activo), "TipoOrganismoId", "Nombre", organismo.TipoOrganismoId);
            return View(organismo);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Organismo organismo = db.Organismos.Find(id);
            if (organismo == null)
            {
                return HttpNotFound();
            }
            ViewBag.CiudadId = new SelectList(db.Ciudades, "CiudadId", "NombreCiudad", organismo.CiudadId);
            ViewBag.EstadoId = new SelectList(db.Estados, "EstadoId", "NombreEstado", organismo.EstadoId);
            ViewBag.PaisId = new SelectList(db.Paises, "PaisId", "NombrePais", organismo.PaisId);
            ViewBag.RepresentanteId = new SelectList(db.Representantes, "RepresentanteId", "NombreCompleto", organismo.RepresentanteId);
            ViewBag.TipoOrganismoId = new SelectList(db.TipoOrganismos, "TipoOrganismoId", "Nombre", organismo.TipoOrganismoId);
            return View(organismo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Organismo organismo)
        {
            if (ModelState.IsValid)
            {
                //para la bitacora
                ApplicationDbContext dbOld = new ApplicationDbContext();
                var oldModel = dbOld.Organismos.Where(x => x.OrganismoID == organismo.OrganismoID).FirstOrDefault();

                db.Entry(organismo).State = EntityState.Modified;
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(oldModel, organismo, organismo.OrganismoID);

                return RedirectToAction("Index");
            }
            ViewBag.CiudadId = new SelectList(db.Ciudades, "CiudadId", "NombreCiudad", organismo.CiudadId);
            ViewBag.EstadoId = new SelectList(db.Estados, "EstadoId", "NombreEstado", organismo.EstadoId);
            ViewBag.PaisId = new SelectList(db.Paises, "PaisId", "NombrePais", organismo.PaisId);
            ViewBag.RepresentanteId = new SelectList(db.Representantes, "RepresentanteId", "NombreCompleto", organismo.RepresentanteId);
            ViewBag.TipoOrganismoId = new SelectList(db.TipoOrganismos, "TipoOrganismoId", "Nombre", organismo.TipoOrganismoId);
            return View(organismo);
        }

        public ActionResult GetOrganismoByTipoOrganismoId(int iId = 0)
        {
            var Organismos = db.Organismos.Where(x => x.TipoOrganismoId == iId && x.Activo).ToList();
            return Json(new { data = Organismos, Encontro = Organismos != null }, JsonRequestBehavior.AllowGet);
        }

        public void campos()
        {
            ViewBag.TipoOrganismoId = db.TipoOrganismos.Where(x => x.Activo).ToList();
            ViewBag.LeyId = db.Leyes.Where(x => x.Activo).ToList();

            ViewBag.TipoOrganismoIds = 0;
            ViewBag.OrganismoIDs = 0;
            ViewBag.LeyIds = 0;
            ViewBag.ArticuloIds = 0;
            ViewBag.OrganismoID = new List<Organismo>();
            ViewBag.ArticuloId = new List<Articulo>();
        }
        public ActionResult AsignarEstructura()
        {
            campos();
            return View();
        }
        public ActionResult GetFraccionByArticulosId(int iId = 0, int iIddOrganismo = 0)
        {
            var Fracciones = db.Fracciones.Where(x => x.ArticuloId == iId && x.Activo).ToList();
            var OrganismosFracciones = db.OrganismosFraccion.Where(x => x.OrganismoID == iIddOrganismo).ToList();
            return Json(new { data = Fracciones,data_organismo_fraciones = OrganismosFracciones, Encontro = Fracciones != null }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AsignarEstructura(List<int> LstFracciones, int OrganismoID = 0)
        {
            LstFracciones = LstFracciones ?? new List<int>();
            DateTime dtNow = DateTime.Now;
            if (LstFracciones.Any() && OrganismoID != 0)
            {

                foreach (var item in LstFracciones)
                {
                    var exite = db.OrganismosFraccion.Where(x => x.OrganismoID == OrganismoID && x.FraccionId == item).FirstOrDefault();
                    if (exite == null)
                    {
                        try
                        {
                            var vModel = new OrganismosFraccion();
                            vModel.OrganismoID = Convert.ToInt32(OrganismoID);
                            vModel.FraccionId = item;
                            db.OrganismosFraccion.Add(vModel);
                            db.SaveChanges();
                        }
                        catch
                        {

                        }
                    }


                }
            }

            var idEliminarOrganismoFraccion = db.OrganismosFraccion.Where(x => x.OrganismoID == OrganismoID && !LstFracciones.Contains(x.FraccionId)).ToList();
            if (idEliminarOrganismoFraccion.Any())
            {
                db.OrganismosFraccion.RemoveRange(idEliminarOrganismoFraccion);
                db.SaveChanges();
            }
            TempData["Mensaje"] = "Se guardo exitosamente";
            return RedirectToAction("AsignarEstructura");
            //campos();
            //return View();
        }

        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    Organismo organismo = db.Organismos.Include(d => d.Ciudad).Include(d => d.Estado).Include(d => d.Pais).
        //      Include(d => d.TipoOrganismo).Include(d => d.Representante).FirstOrDefault(d => d.OrganismoID == id);

        //    if (organismo == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(organismo);
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{

        //    Organismo organismo = db.Organismos.Find(id);
        //    organismo.Activo = false;
        //    db.Entry(organismo).State = EntityState.Modified;
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
        private void CreateBitacora(Organismo oldModel, Organismo newModel, int id = 0)
        {
            var cambioCampos = new List<cambiosCampos>();
            int accion = oldModel != null ? 2 : 1;
            //Creamos la bitacora
            try
            {
                //Nombre
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Nombre Organismo",
                    es_modificado = oldModel != null && oldModel.NombreOrganismo != newModel.NombreOrganismo ? true : false,
                    campo_nuevo = newModel.NombreOrganismo,
                    campo_anterior = oldModel != null ? oldModel.NombreOrganismo : null
                });

                //Tipo Organismo
                string newTipoOrganismo = "";
                string oldTipoOrganismo = "";

                if (newModel != null && newModel.TipoOrganismoId != 0)
                {
                    newTipoOrganismo = db.TipoOrganismos.Where(x => x.TipoOrganismoId == newModel.TipoOrganismoId).FirstOrDefault()?.Nombre;
                }
                if (oldModel != null && oldModel.TipoOrganismoId != 0)
                {
                    oldTipoOrganismo = db.TipoOrganismos.Where(x => x.TipoOrganismoId == oldModel.TipoOrganismoId).FirstOrDefault()?.Nombre;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Tipo de Organismo",
                    es_modificado = oldModel != null && oldModel.TipoOrganismoId != newModel.TipoOrganismoId ? true : false,
                    campo_nuevo = newTipoOrganismo,
                    campo_anterior = oldTipoOrganismo
                });
                //Siglas
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Siglas",
                    es_modificado = oldModel != null && oldModel.Siglas != newModel.Siglas ? true : false,
                    campo_nuevo = newModel.Siglas,
                    campo_anterior = oldModel != null ? oldModel.Siglas : null
                });

                //Descripción
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Descripción",
                    es_modificado = oldModel != null && oldModel.Descripcion != newModel.Descripcion ? true : false,
                    campo_nuevo = newModel.Descripcion,
                    campo_anterior = oldModel != null ? oldModel.Descripcion : null
                });

                //Tipo Organismo
                string newRepresentante = "";
                string oldRepresentante = "";

                if (newModel != null && newModel.RepresentanteId != 0)
                {
                    newRepresentante = db.Representantes.Where(x => x.RepresentanteId == newModel.RepresentanteId).FirstOrDefault()?.NombreCompleto;
                }
                if (oldModel != null && oldModel.RepresentanteId != 0)
                {
                    oldRepresentante = db.Representantes.Where(x => x.RepresentanteId == oldModel.RepresentanteId).FirstOrDefault()?.NombreCompleto;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Representante",
                    es_modificado = oldModel != null && oldModel.RepresentanteId != newModel.RepresentanteId ? true : false,
                    campo_nuevo = newRepresentante,
                    campo_anterior = oldRepresentante
                });

                //Dirección
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Dirección",
                    es_modificado = oldModel != null && oldModel.Direccion != newModel.Direccion ? true : false,
                    campo_nuevo = newModel.Direccion,
                    campo_anterior = oldModel != null ? oldModel.Direccion : null
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

                //Ciudad
                string newCiudad = "";
                string oldCiudad = "";

                if (newModel != null && newModel.CiudadId != 0)
                {
                    newCiudad = db.Ciudades.Where(x => x.CiudadId == newModel.CiudadId).FirstOrDefault()?.NombreCiudad;
                }
                if (oldModel != null && oldModel.CiudadId != 0)
                {
                    oldCiudad = db.Ciudades.Where(x => x.CiudadId == oldModel.CiudadId).FirstOrDefault()?.NombreCiudad;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Ciudad",
                    es_modificado = oldModel != null && oldModel.CiudadId != newModel.CiudadId ? true : false,
                    campo_nuevo = newCiudad,
                    campo_anterior = oldCiudad
                });


                //Código Postal
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Código Postal",
                    es_modificado = oldModel != null && oldModel.CP != newModel.CP ? true : false,
                    campo_nuevo = newModel.CP,
                    campo_anterior = oldModel != null ? oldModel.CP : null
                });

                //Teléfono
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Teléfono",
                    es_modificado = oldModel != null && oldModel.Telefono != newModel.Telefono ? true : false,
                    campo_nuevo = newModel.Telefono,
                    campo_anterior = oldModel != null ? oldModel.Telefono : null
                });

                //Extensión
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Extensión",
                    es_modificado = oldModel != null && oldModel.Extension != newModel.Extension ? true : false,
                    campo_nuevo = newModel.Extension,
                    campo_anterior = oldModel != null ? oldModel.Extension : null
                });

                //URL
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "URL",
                    es_modificado = oldModel != null && oldModel.URL != newModel.URL ? true : false,
                    campo_nuevo = newModel.URL,
                    campo_anterior = oldModel != null ? oldModel.URL : null,
                    link = true
                });

                //Logo
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Logo",
                    es_modificado = oldModel != null && oldModel.Logo != newModel.Logo ? true : false,
                    campo_nuevo = newModel.Logo,
                    campo_anterior = oldModel != null ? oldModel.Logo : null
                });

                //Latitud
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Latitud",
                    es_modificado = oldModel != null && oldModel.Latitud != newModel.Latitud ? true : false,
                    campo_nuevo = newModel.Latitud,
                    campo_anterior = oldModel != null ? oldModel.Latitud : null
                });

                //Longitud
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Longitud",
                    es_modificado = oldModel != null && oldModel.Longitud != newModel.Longitud ? true : false,
                    campo_nuevo = newModel.Longitud,
                    campo_anterior = oldModel != null ? oldModel.Longitud : null
                });

                //Facebook
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Facebook",
                    es_modificado = oldModel != null && oldModel.Facebook != newModel.Facebook ? true : false,
                    campo_nuevo = newModel.Facebook,
                    campo_anterior = oldModel != null ? oldModel.Facebook : null,
                    link = true
                });

                //Twitter
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Twitter",
                    es_modificado = oldModel != null && oldModel.Twitter != newModel.Twitter ? true : false,
                    campo_nuevo = newModel.Twitter,
                    campo_anterior = oldModel != null ? oldModel.Twitter : null,
                    link = true
                });

                //Youtube
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Youtube",
                    es_modificado = oldModel != null && oldModel.Youtube != newModel.Youtube ? true : false,
                    campo_nuevo = newModel.Youtube,
                    campo_anterior = oldModel != null ? oldModel.Youtube : null,
                    link = true
                });

                //Instagram
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Instagram",
                    es_modificado = oldModel != null && oldModel.Instagram != newModel.Instagram ? true : false,
                    campo_nuevo = newModel.Instagram,
                    campo_anterior = oldModel != null ? oldModel.Instagram : null,
                    link = true
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
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "Organismoes", "Organismos", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
