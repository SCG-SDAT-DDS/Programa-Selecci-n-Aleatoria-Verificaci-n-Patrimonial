using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Transparencia.Models;

namespace Transparencia.Controllers
{
    [Authorize]
    public class ImportacionMasivoController : Controller
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
                var Model = db.ImportacionMasivo.Find(id);
                var oldModel = (ImportacionMasivo)Model.Clone();
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
                this.CreateBitacora(oldModel, Model, Model.ImportacionMasivoId);
                
            }
            catch (Exception)
            {

            }
            //return View("Index",horariosModels);
            return activo;

        }

        // GET: Ciudades
        public ActionResult Index(int PlantillaId,FiltrosImpMasivo filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
            Catalogos(PlantillaId);
            //string[] searchstringsN = !string.IsNullOrEmpty(filtros.Nombre) ? filtros.Nombre.Split(' ') : "".Split(' ');
            //string[] searchstringsE = !string.IsNullOrEmpty(filtros.Estado) ? filtros.Estado.Split(' ') : "".Split(' ');
            //string[] searchstringsP = !string.IsNullOrEmpty(filtros.Pais) ? filtros.Pais.Split(' ') : "".Split(' ');
            ViewBag.PlantillaId = PlantillaId;
            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderPèriodo = sOrder == "Periodo" ? "Periodo_desc" : "Periodo";
            ViewBag.OrderEstado = sOrder == "Estado" ? "Estado_desc" : "Estado";
            ViewBag.OrderSysFrecuencia = sOrder == "SysFrecuencia" ? "SysFrecuencia_desc" : "SysFrecuencia";
            ViewBag.OrderSysNumFrecuencia = sOrder == "SysNumFrecuencia" ? "SysNumFrecuencia_desc" : "SysNumFrecuencia";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";

            IPagedList<ImportacionMasivo> vModel = null;

            var query = db.ImportacionMasivo.Where(x=>
               (filtros.PeriodoId == 0 || x.PeriodoId== filtros.PeriodoId)
               && (filtros.sysFrecuencia == 0 || x.sysFrecuencia == filtros.sysFrecuencia)
               && (filtros.sysNumFrecuencia == 0 || x.sysNumFrecuencia == filtros.sysNumFrecuencia)
               && x.OrganismoId == usuario.OrganismoID
               && x.OrganismoId != 0
               && x.PlanntillaId == PlantillaId
               );
            switch (sOrder)
            {
                case "Periodo":
                    vModel = query.OrderBy(x => x.PeriodoId).ToPagedList(iPagina, PerPage);
                    break;
                case "Periodo_desc":
                    vModel = query.OrderByDescending(x => x.PeriodoId).ToPagedList(iPagina, PerPage);
                    break;
                case "SysFrecuencia":
                    vModel = query.OrderBy(x => x.sysFrecuencia).ToPagedList(iPagina, PerPage);
                    break;
                case "SysFrecuencia_desc":
                    vModel = query.OrderByDescending(x => x.sysFrecuencia).ToPagedList(iPagina, PerPage);
                    break;
                case "SysNumFrecuencia":
                    vModel = query.OrderBy(x => x.sysNumFrecuencia).ToPagedList(iPagina, PerPage);
                    break;
                case "SysNumFrecuencia_desc":
                    vModel = query.OrderByDescending(x => x.sysNumFrecuencia).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo":
                    vModel = query.OrderBy(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                case "Activo_desc":
                    vModel = query.OrderByDescending(x => x.Activo).ToPagedList(iPagina, PerPage);
                    break;
                default:
                    vModel = query.OrderBy(x => x.Fecha).ToPagedList(iPagina, PerPage);
                    break;
            }
            if (Request.IsAjaxRequest()) 
            {
                return PartialView("_lista", vModel);
            }
            return View(vModel);
        }

        public ActionResult GetAttachmentAsignado(int ImportacionMasivoId)
        {
            try
            {
                var registro = db.ImportacionMasivo.FirstOrDefault(m => m.ImportacionMasivoId == ImportacionMasivoId);
                var path = registro.documentoError;
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                var a = File(fs.Name, MimeMapping.GetMimeMapping(fs.Name), Path.GetFileName(fs.Name));
                fs.Close();
                return a;
            }
            catch (Exception e)
            {
                return HttpNotFound();
            }
        }

        // GET: Ciudades/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ImportacionMasivo ciudad = db.ImportacionMasivo.FirstOrDefault(c=>c.ImportacionMasivoId == id);
            if (ciudad == null)
            {
                return HttpNotFound();
            }
            return View(ciudad);
        }

        public void Catalogos(int PlantillaId)
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            ViewBag.AllPeriodos = new List<SelectListItem>();
            ViewBag.AllFrecuencias = new List<SelectListItem>();
            ViewBag.AllSubFrecuencia = new List<SelectListItem>();
            try
            {
               
                var mPlantillas = db.Plantillas.Where(x => x.PlantillaId == PlantillaId).FirstOrDefault();
                if (mPlantillas != null)
                {
                    var AllPeriodos = mPlantillas.Periodos;
                    ViewBag.AllPeriodos = AllPeriodos.OrderBy(x => x.Orden).Select(o => new SelectListItem
                    {
                        Text = o.NombrePeriodo,
                        Value = o.PeriodoId.ToString()
                    });
                    var EnumFrecuencia = mPlantillas.Frecuencia;
                    lista.Add(new SelectListItem { Value = ((int)EnumFrecuencia).ToString(), Text = EnumFrecuencia.GetDisplayName() });
                    ViewBag.AllFrecuencias = lista;
                    ViewBag.AllSubFrecuencia = GetFrecuenciasModel(EnumFrecuencia);
                }
            }
            catch (Exception)
            {
            }
        }
        // GET: Ciudades/Create
        public ActionResult Create(int PlantillaId = 0)
        {
            Catalogos(PlantillaId);
            Plantilla model = db.Plantillas.Where(x => x.PlantillaId == PlantillaId).FirstOrDefault();
            ViewBag.PlantillaId = PlantillaId;
            return View("ImportarExcelMasivo",model);
        }

        // POST: Ciudades/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ImportacionMasivo model, HttpPostedFileBase ExcelFile)
        {
            try
            {
                //if (ModelState.IsValid)
                //{

                //submos el arvhivo
                var extencion = ExcelFile.ContentType;
                var splitArchivo = ExcelFile.FileName.Split('.');
                string directorio_sub = $@"Archivos\Plantillas\ImportacionMasivo\";
                string Directorio = $@"{Server.MapPath("~/")}{directorio_sub}";
                string directorio_sub_error = $@"Archivos\Plantillas\ImportacionMasivoError\";
                string Directorio_error = $@"{Server.MapPath("~/")}{directorio_sub_error}";
                bool exists = System.IO.Directory.Exists(Directorio);
                bool existsErorr = System.IO.Directory.Exists(Directorio_error);
                if (!exists)
                    System.IO.Directory.CreateDirectory(Directorio);
                if (!existsErorr)
                    System.IO.Directory.CreateDirectory(Directorio_error);
                var Input = Directorio + ExcelFile.FileName;
                ExcelFile.SaveAs(Input);

                //guardamos la infromacion
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                model.OrganismoId = usuario.OrganismoID.HasValue ? usuario.OrganismoID.Value : 0;
                model.UsuarioId = usuario.Id;
                model.documento = Input;
                model.documentoError = Directorio_error;
                db.ImportacionMasivo.Add(model);
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(null, model, model.ImportacionMasivoId);
                //return RedirectToAction("Index");
                return Json(new { Hecho = true, Mensaje = $"El archivo fué mandado a revisión." }, JsonRequestBehavior.AllowGet);
                //}
                //Catalogos(model.PlanntillaId);
                //return View("ImportarExcelMasivo", model);
                //return Json(new { Hecho = false, Mensaje = $"Datos no validos" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = $"Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
           
        }

        // GET: Ciudades/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Ciudad ciudad = db.Ciudades.Find(id);
        //    if (ciudad == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.EstadoId = new SelectList(db.Estados, "EstadoId", "NombreEstado", ciudad.EstadoId);
        //    return View(ciudad);
        //}

        // POST: Ciudades/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "CiudadId,NombreCiudad,EstadoId,Orden,Activo")] Ciudad ciudad)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        //buscamos modelos Old
        //        ApplicationDbContext dbOld = new ApplicationDbContext();
        //        var oldModel = dbOld.Ciudades.Where(x => x.CiudadId == ciudad.CiudadId).FirstOrDefault();
        //        //guardamos la informaciond e la ciudad 
        //        db.Entry(ciudad).State = EntityState.Modified;
        //        db.SaveChanges();
        //        //bitacora
        //        this.CreateBitacora(oldModel, ciudad, ciudad.CiudadId);
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.EstadoId = new SelectList(db.Estados, "EstadoId", "NombreEstado", ciudad.EstadoId);
        //    return View(ciudad);
        //}

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




        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //Bitacora
        private void CreateBitacora(ImportacionMasivo oldModel, ImportacionMasivo newModel, int id = 0)
        {
            var cambioCampos = new List<cambiosCampos>();
            int accion = oldModel != null ? 2 : 1;
            //Creamos la bitacora
            try
            {
              

                //Plantillas
                string newPlantilla = "";
                string oldPlatilla = "";

                if (newModel != null && newModel.PlanntillaId != 0)
                {
                    newPlantilla = db.Plantillas.Where(x => x.PlantillaId == newModel.PlanntillaId).FirstOrDefault()?.NombreLargo;
                }
                if (oldModel != null && oldModel.PlanntillaId != 0)
                {
                    oldPlatilla = db.Plantillas.Where(x => x.PlantillaId == oldModel.PlanntillaId).FirstOrDefault()?.NombreLargo;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Plantilla",
                    es_modificado = oldModel != null && oldModel.PlanntillaId != newModel.PlanntillaId ? true : false,
                    campo_nuevo = newPlantilla,
                    campo_anterior = oldPlatilla
                });
                //Organismo
                string newOrgaismo = "";
                string oldOrganismo = "";

                if (newModel != null && newModel.OrganismoId != 0)
                {
                    newOrgaismo = db.Organismos.Where(x => x.OrganismoID == newModel.OrganismoId).FirstOrDefault()?.NombreOrganismo;
                }
                if (oldModel != null && oldModel.OrganismoId != 0)
                {
                    oldOrganismo = db.Organismos.Where(x => x.OrganismoID == oldModel.OrganismoId).FirstOrDefault()?.NombreOrganismo;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Organismo",
                    es_modificado = oldModel != null && oldModel.OrganismoId != newModel.OrganismoId ? true : false,
                    campo_nuevo = newOrgaismo,
                    campo_anterior = oldOrganismo
                });

                //Periodo
                string newPeriodo = "";
                string oldPeriodo = "";

                if (newModel != null && newModel.PeriodoId != 0)
                {
                    newPeriodo = db.Periodos.Where(x => x.PeriodoId == newModel.PeriodoId).FirstOrDefault()?.NombrePeriodo;
                }
                if (oldModel != null && oldModel.PeriodoId != 0)
                {
                    oldPeriodo = db.Periodos.Where(x => x.PeriodoId == oldModel.PeriodoId).FirstOrDefault()?.NombrePeriodo;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Periodo",
                    es_modificado = oldModel != null && oldModel.PeriodoId != newModel.PeriodoId ? true : false,
                    campo_nuevo = newPeriodo,
                    campo_anterior = oldPeriodo
                });

                //SysFrecuencia
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "sysFrecuencia",
                    es_modificado = oldModel != null && oldModel.sysFrecuencia != newModel.sysFrecuencia ? true : false,
                    campo_nuevo = newModel.sysFrecuencia.ToString(),
                    campo_anterior = oldModel != null ? oldModel.sysFrecuencia.ToString() : null
                });

                //sysNumFrecuencia
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "SysNumFrecuencia",
                    es_modificado = oldModel != null && oldModel.sysNumFrecuencia != newModel.sysNumFrecuencia ? true : false,
                    campo_nuevo = newModel.sysNumFrecuencia.ToString(),
                    campo_anterior = oldModel != null ? oldModel.sysNumFrecuencia.ToString() : null
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
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "ImportacionMasivoes","Importacion Masiva", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }

        public List<SelectListItem> GetFrecuenciasModel(FrecuenciaActualizacion vTipoFrecuencia = 0)
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            lista.Add(new SelectListItem { Value = "0", Text = "Seleccione..." });
            try
            {
                switch (vTipoFrecuencia)
                {
                    case FrecuenciaActualizacion.Anual:
                        lista.Add(new SelectListItem { Value = "1", Text = "Enero - Diciembre" });
                        break;
                    case FrecuenciaActualizacion.Bimestral:
                        lista.Add(new SelectListItem { Value = "1", Text = "Enero - Febrero" });
                        lista.Add(new SelectListItem { Value = "2", Text = "Marzo - Abril" });
                        lista.Add(new SelectListItem { Value = "3", Text = "Mayo - Junio" });
                        lista.Add(new SelectListItem { Value = "4", Text = "Julio - Agosto" });
                        lista.Add(new SelectListItem { Value = "5", Text = "Septiembre - Octubre" });
                        lista.Add(new SelectListItem { Value = "6", Text = "Noviembre - Diciembre" });
                        break;
                    case FrecuenciaActualizacion.Mensual:
                        lista.Add(new SelectListItem { Value = "1", Text = "Enero" });
                        lista.Add(new SelectListItem { Value = "2", Text = "Febrero" });
                        lista.Add(new SelectListItem { Value = "3", Text = "Marzo" });
                        lista.Add(new SelectListItem { Value = "4", Text = "Abril" });
                        lista.Add(new SelectListItem { Value = "5", Text = "Mayo" });
                        lista.Add(new SelectListItem { Value = "6", Text = "Junio" });
                        lista.Add(new SelectListItem { Value = "7", Text = "Julio" });
                        lista.Add(new SelectListItem { Value = "8", Text = "Agosto" });
                        lista.Add(new SelectListItem { Value = "9", Text = "Septiembre" });
                        lista.Add(new SelectListItem { Value = "10", Text = "Octubre" });
                        lista.Add(new SelectListItem { Value = "11", Text = "Noviembre" });
                        lista.Add(new SelectListItem { Value = "11", Text = "Diciembre" });
                        break;

                    case FrecuenciaActualizacion.Semestral:
                        lista.Add(new SelectListItem { Value = "1", Text = "Enero - Junio" });
                        lista.Add(new SelectListItem { Value = "2", Text = "Julio -Diciembre" });
                        break;
                    case FrecuenciaActualizacion.Trimestral:
                        lista.Add(new SelectListItem { Value = "1", Text = "Enero - Marzo" });
                        lista.Add(new SelectListItem { Value = "2", Text = "Abril - Junio" });
                        lista.Add(new SelectListItem { Value = "3", Text = "Julio - Septiembre" });
                        lista.Add(new SelectListItem { Value = "4", Text = "Octubre - Diciembre" });
                        break;
                }
            }
            catch (Exception ex)
            {

            }
            return lista;

        }
    }
}
