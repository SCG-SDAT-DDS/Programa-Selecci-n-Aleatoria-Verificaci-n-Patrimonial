using Dapper;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Transparencia.FiltersClass;
using Transparencia.Helpers;
using Transparencia.Models;

namespace Transparencia.Controllers
{
    [Authorize]
    public class CatalogosController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private string coneccion = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

        public bool? changeStatus(int id = 0)
        {
            var activo = false;
            try
            {
                if (id == 0)
                {
                    return null;
                }
                var Model = db.Catalogoes.Find(id);
                var oldModel = (Catalogo)Model.Clone();
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
                //bitacora
                this.CreateBitacora(oldModel, Model, Model.CatalogoId);
            }
            catch(Exception ex)
            {

            }
            return activo;
        }

        private void combos()
        {
            ViewBag.CmbActivo = new List<SelectListItem>();
            ViewBag.CmbActivo = new List<SelectListItem> {
                new SelectListItem { Text = "Activo", Value = "true" },
                new SelectListItem { Text = "Inactivo", Value = "false" }
            };
        }
        // GET: Catalogos
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
            IPagedList<Catalogo> vModel = null;

            var query = db.Catalogoes.Where(x =>
                ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.Nombre.ToLower().Contains(y.ToLower())))
                && ((filtros.Descripcion == null || filtros.Descripcion.Length == 0) || searchstringsD.Any(y => x.Descripcion.ToLower().Contains(y.ToLower())))
                && (filtros.ActivoNull == null || filtros.ActivoNull == x.Activo)
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

        // GET: Catalogos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogo catalogo = db.Catalogoes.Find(id);
            if (catalogo == null)
            {
                return HttpNotFound();
            }
            return View(catalogo);
        }




        // GET: Catalogos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Catalogos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CatalogoId,Nombre,Descripcion,orden,dinamico,Tabla")] Catalogo catalogo)
        {
            catalogo.NombreTabla = GenerarNombreTabla(catalogo.Nombre);
            if (ModelState.IsValid)
            {
                var result = CrearTablaFisica(catalogo);
                if (!result.Result)
                {
                    return ReturErrorCreate(catalogo, "Ocurrio un error al momento de crear la tabla fisica");
                }
                catalogo.Activo = true;
                db.Catalogoes.Add(catalogo);
                db.SaveChanges();

                //Bitacora
                this.CreateBitacora(null, catalogo, catalogo.CatalogoId);

                return RedirectToAction("Index");
            }

            return View(catalogo);
        }

        // GET: Catalogos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogo catalogo = db.Catalogoes.Find(id);
            if (catalogo == null)
            {
                return HttpNotFound();
            }
            return View(catalogo);
        }

        // POST: Catalogos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CatalogoId,Nombre,Descripcion,orden,dinamico,Activo,Tabla")] Catalogo catalogo)
        {
            if (ModelState.IsValid)
            {
                //para la bitacora
                ApplicationDbContext dbOld = new ApplicationDbContext();
                var oldModel = dbOld.Catalogoes.Where(x => x.CatalogoId == catalogo.CatalogoId).FirstOrDefault();

                var model = db.Catalogoes.Where(x=>x.CatalogoId == catalogo.CatalogoId).FirstOrDefault();
                model.Nombre = catalogo.Nombre;
                model.Descripcion = catalogo.Descripcion;
                model.orden = catalogo.orden;
                model.Activo = catalogo.Activo;
                model.dinamico = catalogo.dinamico;
                model.Tabla = catalogo.Tabla;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();

                //Bitacora
                this.CreateBitacora(oldModel, model, model.CatalogoId);

                return RedirectToAction("Index");
            }
            return View(catalogo);
        }

        //// GET: Catalogos/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Catalogo catalogo = db.Catalogoes.Find(id);
        //    if (catalogo == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(catalogo);
        //}

        //// POST: Catalogos/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Catalogo catalogo = db.Catalogoes.Find(id);
        //    db.Catalogoes.Remove(catalogo);
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


        #region NuevosAgregarCapos

        public ActionResult ConfigurarCampos(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Catalogos");
            }
            //var campo = new CampoViewModel();
            VerifyNecessaryColumn((int)id);
            ViewBag.Catalogo_name = db.Catalogoes.Where(x => x.CatalogoId == id).FirstOrDefault().Nombre;
            var campos = db.CampoCatalogo.Where(x => x.CatalogoId == id).OrderBy(x => x.Orden).ToList();// generarViewModel();
            ViewData["error"] = TempData["error"];
            ViewBag.CatalogoId = id;
            ViewBag.GrupoExtension = db.GrupoExtesiones.Select(x => new ListaSeleccion { Text = x.Nombre, Value = x.GrupoExtensionId }).ToList();
            ViewBag.listaCatalogos = db.Catalogoes.Select(x => new ListaSeleccion { Text = x.Nombre, Value = x.CatalogoId }).ToList();
            return View(campos);
        }
        private ActionResult ReturErrorCreate(Catalogo model, string sMessageError)
        {
            ViewBag.Error = sMessageError;
            return View(model);
        }

        public bool CheckIfExistTable(string NombreTabla)
        {
            var bResult = false;
            try
            {
                string sQuery = $@"IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{NombreTabla}')
	                            SELECT CAST(1 AS BIT)
                            ELSE
	                            SELECT CAST(0 AS BIT)";


                bResult = db.Database.SqlQuery<bool>(sQuery).FirstOrDefault();

            }
            catch (Exception ex)
            {

            }

            return bResult;
        }
        public bool CheckIfExistRowsInTable(Catalogo model)
        {
            string sQuery = $@"IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{model.NombreTabla}')
	                            SELECT 'true'
                            ELSE
	                            SELECT 'false'";


            var bResult = db.Database.SqlQuery<bool>(sQuery).FirstOrDefault();

            if (!bResult)
                return bResult;
            sQuery = $@"IF EXISTS(SELECT TOP 1 * FROM {model.NombreTabla})
	                            SELECT 'true'
                            ELSE
	                            SELECT 'false'";
            bResult = db.Database.SqlQuery<bool>(sQuery).FirstOrDefault();

            return bResult;
        }
        /// <summary>
        /// Genrar nombre tabla
        /// </summary>
        /// <param name="nombre"></param>
        /// <returns></returns>
        /// 
        private string GenerarNombreTabla(string NombreCorto)
        {
            int num = RandomNumber(1000, 10000);
            var NombreTabla = RemoveAccentsWithRegEx(NombreCorto);
            var Tabla = NombreTabla + num.ToString();
            List<string> results = db.Database.SqlQuery<string>("SELECT name FROM sys.tables ORDER BY name").ToList();

            while (results.Any(x => Tabla.ToLower() == x.ToLower()))
            {
                num = RandomNumber(1000, 10000);
                Tabla = NombreTabla + num.ToString();
            }

            return Tabla;

        }

        private static string RemoveAccentsWithRegEx(string inputString)
        {
            Regex replace_a_Accents = new Regex("[á|à|ä|â]", RegexOptions.Compiled);
            Regex replace_e_Accents = new Regex("[é|è|ë|ê]", RegexOptions.Compiled);
            Regex replace_i_Accents = new Regex("[í|ì|ï|î]", RegexOptions.Compiled);
            Regex replace_o_Accents = new Regex("[ó|ò|ö|ô]", RegexOptions.Compiled);
            Regex replace_u_Accents = new Regex("[ú|ù|ü|û]", RegexOptions.Compiled);
            inputString = replace_a_Accents.Replace(inputString, "a");
            inputString = replace_e_Accents.Replace(inputString, "e");
            inputString = replace_i_Accents.Replace(inputString, "i");
            inputString = replace_o_Accents.Replace(inputString, "o");
            inputString = replace_u_Accents.Replace(inputString, "u");
            inputString = Regex.Replace(inputString, @"[^\w\s.!@$%^&*()\-\/]+", "");
            inputString = inputString.Replace(" ", "_");
            inputString = inputString.Replace("-", "_");
            inputString = inputString.Replace("(", "_");
            inputString = inputString.Replace(")", "_");
            inputString = inputString.Replace(".", "_");
            return inputString;
        }


        private int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }


        /// <summary>
        /// Tablas Query SQL
        /// </summary>
        /// <param name="nombre"></param>
        /// <returns></returns>
        /// 


        //ESTE DATO TAMBIEN ESTA EN HMTLHELPER 
        public ResultGuardarTablaFisica CrearTablaFisica(Catalogo model)
        {


            string sDropTableScript = $@"IF OBJECT_ID('{model.NombreTabla}', 'U') IS NOT NULL 
                                            DROP TABLE {model.NombreTabla}; ";

            string sTablaScript = $@"CREATE TABLE {model.NombreTabla}(
	                [TablaFisicaId] [int] IDENTITY(1,1) NOT NULL,
                    [OrganismoID] [int] NOT NULL,
                    [UsuarioId] [nvarchar](128) NOT NULL,
                    [Activo] BIT DEFAULT 1,
                    [FechaCreacion] DATE DEFAULT '',
                    [FechaBaja] DATE DEFAULT '',
                    [OrganismoCapturoID]  [int] NOT NULL DEFAULT 0,
                    [IdFederal] [int] Not NULL DEFAULT 0,
                 CONSTRAINT [PK_dbo.{model.NombreTabla}] PRIMARY KEY CLUSTERED 
                (
	                [TablaFisicaId] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]";
            //foreach (var item in lstCamposTablero.Where(r => r.EsFiltro))
            //{
            //    sTablaScript += $@" CREATE NONCLUSTERED INDEX IX_{model.NombreTablaBD}_{item.NombreCampoBD}
            //                        ON {model.NombreTablaBD}({item.NombreCampoBD} ASC) ";
            //}

            ResultGuardarTablaFisica bResult = new ResultGuardarTablaFisica
            {
                Result = true
            };
            try
            {

                db.Database.ExecuteSqlCommand(sDropTableScript + sTablaScript);
            }
            catch (Exception ex)
            {
                bResult = new ResultGuardarTablaFisica
                {
                    Result = false
                };
            }
            return bResult;
        }
        //nuevoCreateAjax
        public string GenerarNombreCampos(string nombre = "")
        {
            nombre = nombre.Replace(@"á", "a").Replace(@"Á", "A")
            .Replace(@"é", "e").Replace(@"É", "E")
            .Replace(@"ó", "o").Replace(@"Ó", "O")
            .Replace(@"í", "i").Replace(@"Í", "I")
            .Replace(@"ú", "u").Replace(@"Ú", "U")
            .Replace(@",", "_").Replace(@"\", "_").Replace(" ", "_").Replace(@"/", "_").Replace(@"-", "_")
            .Replace(@"(", "_").Replace(@")", "_").Replace(@".", "_");
            string palabaSinTildes = Regex.Replace(nombre.Normalize(NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");
            palabaSinTildes = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nombre.ToLower());
            palabaSinTildes = palabaSinTildes.Replace(@"\", "").Replace(" ", "").Replace(@"/", "");
            return palabaSinTildes;
        }
        public string GenerarNombreArchivo(string nombre = "")
        {
            nombre = nombre.Replace(@"á", "a").Replace(@"Á", "A")
            .Replace(@"é", "e").Replace(@"É", "E")
            .Replace(@"ó", "o").Replace(@"Ó", "O")
            .Replace(@"í", "i").Replace(@"Í", "I")
            .Replace(@"ú", "u").Replace(@"Ú", "U")
            .Replace(@",", "_").Replace(@"\", "_").Replace(" ", "_").Replace(@"/", "_").Replace(@"-", "_")
            .Replace(@"(", "_").Replace(@")", "_").Replace(@".", "_");
            string palabaSinTildes = Regex.Replace(nombre.Normalize(NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");
            palabaSinTildes = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nombre.ToLower());
            palabaSinTildes = palabaSinTildes.Replace(@"\", "").Replace(" ", "_").Replace(@"/", "").Replace(@"-", "");
            return palabaSinTildes;
        }
        public List<CampoCatalogo> GetCampos(int iIdA = 0)
        {
            return db.CampoCatalogo.Where(x => x.CatalogoId == iIdA).OrderBy(x => x.Orden).ToList();
        }

        private string ConvertViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (StringWriter writer = new StringWriter())
            {
                ViewEngineResult vResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext vContext = new ViewContext(this.ControllerContext, vResult.View, ViewData, new TempDataDictionary(), writer);
                vResult.View.Render(vContext, writer);
                return writer.ToString();
            }
        }


        private string RemoveAllPrincipal(int iId = 0)
        {
            string resultado = "";
            try
            {
                var mCamposCatalogos = db.CampoCatalogo.Where(x => x.CatalogoId == iId).ToList();
                if (mCamposCatalogos.Where(x => x.Principal).Any())
                {
                    var mCampoCatalogo = mCamposCatalogos.Where(x => x.Principal).FirstOrDefault();
                    mCampoCatalogo.Principal = false;
                    db.Entry(mCampoCatalogo).State = EntityState.Modified;
                    db.SaveChanges();

                }

            } catch (Exception ex)
            {
                resultado = ex.Message;
            }
            return resultado;

        }

        public ActionResult AjaxAgregarCampo(LstCampos tblCampoDinamico, int iIdA = 0)
        {

            if (iIdA == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el Catalogo." }, JsonRequestBehavior.AllowGet);
            }
            var campos = new CampoCatalogo();
            var CCampos = db.CampoCatalogo.Where(x => x.CatalogoId == iIdA).Count();

            campos.Activo = true;
            campos.Ayuda = tblCampoDinamico.AddAyuda;
            campos.iCatalogoId = tblCampoDinamico.AddCatalogoId;
            campos._ConDecimales = tblCampoDinamico.AddConDecimales;
            campos.Etiqueta = tblCampoDinamico.AddEtiqueta;
            campos._GrupoExtensionId = tblCampoDinamico.AddGrupoExtensionId;
            campos.Longitud = tblCampoDinamico.AddLongitud; string output =
            campos.Nombre = GenerarNombreCampos(tblCampoDinamico.AddNombre);
            campos.Orden = CCampos + 1;
            campos.Requerido = tblCampoDinamico.AddRequerido;
            campos._Size = tblCampoDinamico.AddSize;
            campos.TipoCampo = (TipoCampo)tblCampoDinamico.AddTipoCampo;
            campos._TipoFecha = (TipoFecha)tblCampoDinamico.AddTipoFecha;
            campos._TipoFecha = (TipoFecha)tblCampoDinamico.AddTipoFecha;
            campos._ConDecimales = tblCampoDinamico.AddConDecimales;
            campos._GrupoExtensionId = tblCampoDinamico.AddGrupoExtensionId;
            campos._Size = tblCampoDinamico.AddSize;
            campos.Principal = tblCampoDinamico.AddPrincipal;
            campos.IdCampoPNT = tblCampoDinamico.AddCampoPNT;
            campos.IdTipoCampoPNT = tblCampoDinamico.AddTipoCampoPNT;
            campos.CatalogoId = iIdA;

            if (tblCampoDinamico.AddPrincipal)
            {
                var resultado = RemoveAllPrincipal(iIdA);
            }

            try
            {

                db.CampoCatalogo.Add(campos);
                db.SaveChanges();
                var CatalogoTableName = GetTableName(iIdA);
                string Respuesta = this.AddColumToTable(CatalogoTableName, campos.Nombre, this.GetAtributosInput(campos));
                string viewContent = ConvertViewToString("_ListaCampos", GetCampos(iIdA));
                //Bitacora
                var catalogo = db.Catalogoes.Where(x => x.CatalogoId == iIdA).FirstOrDefault();
                this.CreateBitacora(null, campos, campos.CatalogoId,catalogo?.Nombre);
                return Json(new { Hecho = true, Mensaje = "se guardo exitosamente", Partial = viewContent }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        //public ActionResult AjaxEliminarCampo(int CampoCatalogoId = 0)
        //{

        //    if (CampoCatalogoId == 0)
        //    {
        //        return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el catalogo." }, JsonRequestBehavior.AllowGet);
        //    }


        //    try
        //    {
        //        var campo = db.CampoCatalogo.Where(x => x.CampoCatalogoId == CampoCatalogoId).FirstOrDefault();
        //        if (campo != null)
        //        {
        //            db.CampoCatalogo.Remove(campo);
        //            db.SaveChanges();
        //            var CatalogoNombreTabla = db.Catalogoes.Where(x => x.CatalogoId == campo.CatalogoId).Select(x => x.NombreTabla).FirstOrDefault();
        //            string Respuesta = this.RemoveColumToTable(CatalogoNombreTabla, campo.Nombre);
        //        }
        //        return Json(new { Hecho = true, Mensaje = "Se elimino el registro" }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }

        //}

        public ActionResult AjaxEliminarCampo(int CampoCatalogoId = 0, bool yaLoValido = false)
        {

            if (CampoCatalogoId == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener la Plantilla." }, JsonRequestBehavior.AllowGet);
            }


            try
            {

                var campo = db.CampoCatalogo.Where(x => x.CampoCatalogoId == CampoCatalogoId).FirstOrDefault();
                var Registros = HMTLHelperExtensions.GetInfoOfRemove(campo);
                //verifiicamos si tiene error
                if (Registros.Length > 0 && !yaLoValido)
                {

                    return Json(new { Hecho = true, Respuesta = Registros }, JsonRequestBehavior.AllowGet);

                }
                if (campo != null)
                {
                    db.CampoCatalogo.Remove(campo);
                    db.SaveChanges();
                    //Bitacora
                    var catalogo = db.Catalogoes.Where(x => x.CatalogoId == campo.CatalogoId).FirstOrDefault();
                    this.CreateBitacora(campo, null, 0, catalogo?.Nombre);
                    var PlantillaNombre = db.Catalogoes.Where(x => x.CatalogoId == campo.CatalogoId).Select(x => x.NombreTabla).FirstOrDefault();
                    string Respuesta = this.RemoveColumToTable(PlantillaNombre, campo.Nombre);
                }
                return Json(new { Hecho = true, Mensaje = "Se elimino el registro", Respuesta = Registros }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult AjaxCambiarOrden(int CampoCatalogoId = 0, int iOrden = 0, int iCatalogoId = 0)
        {

            if (CampoCatalogoId == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el campo." }, JsonRequestBehavior.AllowGet);
            }

            if (iCatalogoId == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el catalogo." }, JsonRequestBehavior.AllowGet);
            }


            try
            {
                var campo = db.CampoCatalogo.Where(x => x.CampoCatalogoId == CampoCatalogoId).FirstOrDefault();
                var ExisteOrden = db.CampoCatalogo.Where(x => x.CatalogoId == iCatalogoId && x.Orden == iOrden).Any();

                if (ExisteOrden)
                {
                    var ListaCampos = db.CampoCatalogo.Where(x => x.CatalogoId == iCatalogoId && x.Orden >= iOrden).ToList();
                    if (ListaCampos.Count > 0)
                    {
                        foreach (var item in ListaCampos)
                        {
                            item.Orden++;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                if (campo != null)
                {
                    campo.Orden = iOrden;
                    db.Entry(campo).State = EntityState.Modified;
                    db.SaveChanges();
                }
                string viewContent = ConvertViewToString("_ListaCampos", GetCampos(iCatalogoId));
                return Json(new { Hecho = true, Mensaje = "se guardo exitosamente", Partial = viewContent }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        public string AddColumToTable(string Table, string ColumName, string sOpciones)
        {
            ColumName = GenerarNombreCampos(ColumName);
            string Respuesta = "";

            var Alter = "";

            Alter = $@"ALTER TABLE { Table }
                    ADD { ColumName } { sOpciones };";

            try
            {
                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                sqlConnection.Query(Alter);
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                Respuesta = ex.Message;
            }

            return Respuesta;

        }

        public List<string> GetColumnTable(string TableName)
        {
            List<string> LstColumnName = new List<string>();

            string sQuery = $@"SELECT COLUMN_NAME
                                    FROM INFORMATION_SCHEMA.COLUMNS
                                    WHERE TABLE_NAME = N'{ TableName }'";
            //string sQuery = $@"IF EXISTS(SELECT * FROM  {nombreTabla} WHERE {nombreCampo} IS NOT NULL)
            //                     SELECT 'true'
            //                    ELSE
            //                     SELECT 'false'";
            try
            {
                SqlConnection sqlConnections = new SqlConnection(coneccion);
                sqlConnections.Open();
                LstColumnName = sqlConnections.Query<string>(sQuery).ToList();
                sqlConnections.Close();
                return LstColumnName;
            }
            catch (Exception ex)
            {
            }



            return LstColumnName;
        }


        public void VerifyNecessaryColumn(int iId = 0)
        {
            try
            {
                var columnas = new List<CampoCatalogo>();

                var catalogo = db.Catalogoes.Where(x => x.CatalogoId == iId).FirstOrDefault();
                if (catalogo != null && catalogo.NombreTabla == null || catalogo.NombreTabla.Length == 0)
                {
                    catalogo.NombreTabla = GenerarNombreTabla(catalogo.Nombre);
                    db.Entry(catalogo).State = EntityState.Modified;
                    db.SaveChanges();
                }
                if (!CheckIfExistTable(catalogo.NombreTabla))
                {
                    var result = CrearTablaFisica(catalogo);
                }

                if (catalogo.NombreTabla.Length > 0)
                {
                    var LstColumns = this.GetColumnTable(catalogo.NombreTabla);
                    if (LstColumns.Count > 0)
                    {
                        columnas = db.CampoCatalogo.Where(x => x.CatalogoId == catalogo.CatalogoId && x.Activo).ToList();

                        if (columnas.Count > 0)
                        {
                            foreach (var item in columnas)
                            {
                                if (!LstColumns.Contains(item.Nombre))
                                {

                                    if (item.Nombre.Contains(" "))
                                    {
                                        var Respuesta2 = HMTLHelperExtensions.modificarCampoErroneoCatalogo(item.CampoCatalogoId);
                                        var mColumna = db.CampoCatalogo.Where(x => x.CampoCatalogoId == item.CampoCatalogoId).FirstOrDefault();
                                        if (mColumna != null)
                                        {
                                            string Respuesta = this.AddColumToTable(catalogo.NombreTabla, mColumna.Nombre, this.GetAtributosInput(mColumna));
                                        }
                                    }
                                    else
                                    {
                                        string Respuesta = this.AddColumToTable(catalogo.NombreTabla, item.Nombre, this.GetAtributosInput(item));
                                    }

                                    

                                }
                                


                            }
                        }

                        //[OrganismoID] [int] NOT NULL,
                        //[UsuarioId] [nvarchar] (128) NOT NULL,

                        //[Activo] BIT DEFAULT 1,
                        if (!LstColumns.Contains("OrganismoID"))
                        {
                            this.AddColumToTable(catalogo.NombreTabla, "OrganismoID", "[int] NOT NULL DEFAULT ''");
                        }
                        if (!LstColumns.Contains("UsuarioId"))
                        {
                            this.AddColumToTable(catalogo.NombreTabla, "UsuarioId", "[nvarchar] (128) NOT NULL DEFAULT ''");
                        }
                        if (!LstColumns.Contains("Activo"))
                        {
                            this.AddColumToTable(catalogo.NombreTabla, "Activo", "BIT DEFAULT 1");
                        }
                        if (!LstColumns.Contains("FechaCreacion"))
                        {
                            this.AddColumToTable(catalogo.NombreTabla, "FechaCreacion", "DATE DEFAULT ''");
                        }
                        if (!LstColumns.Contains("FechaBaja"))
                        {
                            this.AddColumToTable(catalogo.NombreTabla, "FechaBaja", "DATE DEFAULT ''");
                        }
                        if (!LstColumns.Contains("OrganismoCapturoID"))
                        {
                            this.AddColumToTable(catalogo.NombreTabla, "OrganismoCapturoID", "[int] NOT NULL DEFAULT ''");
                        }
                        if (!LstColumns.Contains("IdFederal"))
                        {
                            this.AddColumToTable(catalogo.NombreTabla, "IdFederal", "[int] NOT NULL DEFAULT 0");
                        }
                        if (!LstColumns.Contains("RegistroId"))
                        {
                            this.AddColumToTable(catalogo.NombreTabla, "IdRegistro", "[int] NOT NULL DEFAULT 0");
                        }
                    }
                }

                if (columnas.Count > 0)
                {
                    this.CheckIndexes(catalogo.NombreTabla, columnas);
                }

            }
            catch (Exception ex)
            {

            }

        }
        public void CheckIndexes(string NombreTabla, List<CampoCatalogo> campos)
        {

            try
            {

                string sQuery = $@"EXEC sp_helpindex '{NombreTabla}'";
                var resultIndex = db.Database.SqlQuery<ResultIndexes>(sQuery).ToList();
                if (resultIndex.Where(x => x.index_keys == "TablaFisicaId, Activo").Count() <= 0)
                {
                    var r = new Random();
                    sQuery = $@"CREATE INDEX index_custom_{DateTime.Now.ToString("yyyy_mm_dd") + r.Next(10000, 7777777) } ON {NombreTabla} (TablaFisicaId, Activo);";
                    var result = db.Database.SqlQuery<bool>(sQuery).FirstOrDefault();
                }

                var principal = campos.Where(x => x.Principal).FirstOrDefault();

                if(principal != null)
                {
                    var nombreCampo = principal.Nombre;
                    if(principal.TipoCampo == TipoCampo.Catalogo)
                    {
                        nombreCampo = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(principal.iCatalogoId, true);
                    }
                    if (resultIndex.Where(x => x.index_keys == $"TablaFisicaId, {principal.Nombre}").Count() <= 0)
                    {
                        var r = new Random();
                        sQuery = $@"CREATE INDEX index_custom_{DateTime.Now.ToString("yyyy_mm_dd") + r.Next(10000, 7777777) } ON {NombreTabla} (TablaFisicaId, {nombreCampo});";
                        var result = db.Database.SqlQuery<bool>(sQuery).FirstOrDefault();
                    }
                }
                

            }
            catch (Exception ex)
            {

            }

        }


        public List<AuxConstraints> GetContraintsForColumn(string TableName, string ColumnName)
        {
            List<AuxConstraints> LstConstrtainst = new List<AuxConstraints>();

            string sQuery = $@"SELECT
                                df.name 'Constraint' ,
                                t.name 'Table',
                                c.NAME 'Column'
                            FROM sys.default_constraints df
                            INNER JOIN sys.tables t ON df.parent_object_id = t.object_id
                            INNER JOIN sys.columns c ON df.parent_object_id = c.object_id AND df.parent_column_id = c.column_id
                            where t.name='{TableName}' and c.name='{ColumnName}'";
            try
            {
                SqlConnection sqlConnections = new SqlConnection(coneccion);
                sqlConnections.Open();
                LstConstrtainst = sqlConnections.Query<AuxConstraints>(sQuery).ToList();
                sqlConnections.Close();
                return LstConstrtainst;
            }
            catch (Exception ex)
            {
            }



            return LstConstrtainst;
        }

        public string GetTableName(int iId)
            => db.Catalogoes.Where(x => x.CatalogoId == iId).Select(x => x.NombreTabla).FirstOrDefault();

        public string GetAtributosInput(CampoCatalogo campo)
        {
            string sOpciones = "";
            string sRequerido = campo.Requerido ? "NOT NULL " : "NULL";
            string sDecimmantes = campo._ConDecimales == true ? "2" : "0";
            string sDefult = "";
            switch (campo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.AreaTexto:
                case TipoCampo.Alfanumerico:
                case TipoCampo.Hora:
                case TipoCampo.email:
                case TipoCampo.Telefono:
                case TipoCampo.Hipervinculo:
                    sDefult = campo.Requerido ? " DEFAULT '' " : "";
                    if (campo.TipoCampo == TipoCampo.Hora)
                    {
                        campo.Longitud = 8;
                    }
                    sOpciones = $@"VARCHAR({campo.Longitud}) { sRequerido }{ sDefult } ";

                    break;

                case TipoCampo.ArchivoAdjunto:
                    sDefult = campo.Requerido ? " DEFAULT '' " : "";
                    sOpciones = $@"nvarchar(max) { sRequerido }{ sDefult } ";
                    break;
                case TipoCampo.Numerico:
                case TipoCampo.Catalogo:
                    sDefult = campo.Requerido ? " DEFAULT 0 " : "";
                    sOpciones = $@"INT { sRequerido }{ sDefult }";

                    break;
                case TipoCampo.Dinero:
                case TipoCampo.Decimal:
                    sDefult = campo.Requerido ? " DEFAULT 0 " : "";
                    sOpciones = $@"decimal({campo.Longitud},2) { sRequerido } {sDefult}";
                    break;
                case TipoCampo.Porcentaje:
                    sDefult = campo.Requerido ? " DEFAULT 0 " : "";
                    sOpciones = $@"decimal({campo.Longitud},{sDecimmantes}) { sRequerido }{sDefult}";
                    break;
                case TipoCampo.Fecha:
                    sDefult = campo.Requerido ? " DEFAULT '' " : "";
                    sOpciones = $@"DATE { sRequerido }{sDefult} ";
                    break;
                case TipoCampo.CasillaVerificacion:
                    sDefult = campo.Requerido ? " DEFAULT 0 " : "";
                    sOpciones = $@"BIT { sRequerido }{sDefult}";
                    break;
                default:
                    break;
            }

            return sOpciones;
        }
        //public string GetValueByCampo(CampoCatalogo campo, string valor,bool bLike=false)
        //{
        //    var respuesta = "";
        //    try
        //    {
        //        switch (campo.TipoCampo)
        //        {
        //            case TipoCampo.Texto:
        //            case TipoCampo.AreaTexto:
        //            case TipoCampo.Alfanumerico:
        //            case TipoCampo.Hora:
        //            case TipoCampo.email:
        //            case TipoCampo.Telefono:
        //            case TipoCampo.ArchivoAdjunto:
        //            case TipoCampo.Hipervinculo:
        //            case TipoCampo.Fecha:
        //                respuesta = $@"'{valor}{ (bLike ? "%" : "")}'";

        //                break;
        //            case TipoCampo.Numerico:
        //            case TipoCampo.Catalogo:
        //            case TipoCampo.Dinero:
        //            case TipoCampo.Decimal:
        //            case TipoCampo.Porcentaje:
        //            case TipoCampo.CasillaVerificacion:
        //                respuesta = $@"{ valor }";
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }


        //    return respuesta;
        //}
        public string GetValueByCampo(CampoCatalogo campo, string valor, bool bLike = false)
        {
            var respuesta = "";
            try
            {

                switch (campo.TipoCampo)
                {
                    case TipoCampo.Texto:
                    case TipoCampo.AreaTexto:
                    case TipoCampo.Alfanumerico:
                    case TipoCampo.Hora:
                    case TipoCampo.email:
                    case TipoCampo.Telefono:
                    case TipoCampo.ArchivoAdjunto:
                    case TipoCampo.Hipervinculo:
                    case TipoCampo.Fecha:
                        respuesta = $@"'{valor}{ (bLike ? "%" : "")}'";
                        if (valor == "NULL")
                            return valor;
                        break;
                    case TipoCampo.Numerico:
                    case TipoCampo.Catalogo:
                    case TipoCampo.Dinero:
                    case TipoCampo.Decimal:
                    case TipoCampo.Porcentaje:
                    case TipoCampo.CasillaVerificacion:
                        respuesta = $@"{ valor }";
                        if (valor == "NULL")
                            return valor;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {

            }


            return respuesta;
        }

        public string ModifyColumToTable(string Table, string ColumName, string sOpcionesNuevas)
        {
            string Respuesta = "";

            var Alter = "";

            Alter = $@"ALTER TABLE { Table }
                     ALTER COLUMN { ColumName } { sOpcionesNuevas };";

            try
            {
                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                sqlConnection.Query(Alter);
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                Respuesta = ex.Message;
            }

            return Respuesta;

        }

        public string RemoveColumToTable(string Table, string ColumName)
        {
            string Respuesta = "";

            var Alter = "";

            var ListaContrain = this.GetContraintsForColumn(Table, ColumName);
            if (ListaContrain.Count > 0)
            {
                foreach (var item in ListaContrain)
                {
                    var R = this.RemoveContrains(item.Table, item.Constraint);
                }
            }

            Alter = $@"ALTER TABLE { Table }
                    DROP COLUMN { ColumName };";

            try
            {
                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                sqlConnection.Query(Alter);
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                Respuesta = ex.Message;
            }

            return Respuesta;

        }
        public string RemoveContrains(string Table, string ConstrainName)
        {
            string Respuesta = "";

            var Alter = $@"ALTER TABLE {Table} drop constraint {ConstrainName};";

            try
            {
                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                sqlConnection.Query(Alter);
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                Respuesta = ex.Message;
            }

            return Respuesta;

        }

        public bool ExistColumTable(string nombreTabla = "", string nombreCampo = "")
        {
            bool bResult = false;
            if (nombreTabla == "" || nombreCampo == "")
            {
                return bResult;
            }

            string sQuery = $@"IF EXISTS 
                            (
                                SELECT * 
                                FROM INFORMATION_SCHEMA.COLUMNS 
                                WHERE table_name = '{nombreTabla}'
                                AND column_name = '{nombreCampo}'
                            )
                                SELECT 'true'
                            ELSE
                                SELECT 'false'";
            //string sQuery = $@"IF EXISTS(SELECT * FROM  {nombreTabla} WHERE {nombreCampo} IS NOT NULL)
            //                     SELECT 'true'
            //                    ELSE
            //                     SELECT 'false'";
            try
            {
                SqlConnection sqlConnections = new SqlConnection(coneccion);
                sqlConnections.Open();
                bResult = sqlConnections.QueryFirst<bool>(sQuery);
                sqlConnections.Close();
                return bResult;
            }
            catch (Exception ex)
            {
            }


            return bResult;
        }



        public ActionResult CreateDynamic(int Id = 0)
        {
            if (Id == 0)
            {
                return RedirectToAction("Index", "Catalogos");
            }
            this.VerifyNecessaryColumn(Id);
            List<CampoCatalogo> campos = GetCamposByColumn(Id);
            ViewBag.CatalogoId = Id;
            ViewBag.Validation = HMTLHelperExtensions.GetValidationJquery(campos);
            return View(campos);
        }

        [HttpPost]
        public ActionResult CreateDynamic(int iId, FormCollection Form)
        {

            try
            {
                AuxInsertCatalogo Respuesta = new AuxInsertCatalogo();
                Respuesta = InserDynamicScript(this.GetTableName(iId), this.GetTableEjercicio(iId), GetCamposByColumn(iId,true), Form);
                if (Respuesta.Respuesta != null && Respuesta.Respuesta.Length > 0)
                {
                    return Json(new { Hecho = false, Mensaje = Respuesta }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Hecho = true, Mensaje = "se guardo exitosamente", InsertedId = Respuesta.insertedId }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateDynamicForTabla(int iId,int Idregistro, FormCollection Form)
        {

            try
            {
                if(iId == 0 || Idregistro == 0)
                {
                    return Json(new { Hecho = false, Mensaje = "No pudimos obtener el registro o el catalago, por favor intentelo de nuevo." }, JsonRequestBehavior.AllowGet);
                }
                AuxInsertCatalogo Respuesta = new AuxInsertCatalogo();
                Respuesta = InserDynamicScriptForTable(this.GetTableName(iId), this.GetTableEjercicio(iId), GetCamposByColumn(iId, true), Form, Idregistro,iId);
                if (Respuesta.Respuesta != null && Respuesta.Respuesta.Length > 0)
                {
                    return Json(new { Hecho = false, Mensaje = Respuesta.Respuesta }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Hecho = true, Mensaje = "se guardo exitosamente", InsertedId = Respuesta.insertedId }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditDynamicForTabla(int CatalogoId, int TablaFisicaId, FormCollection Form)
        {
            
            try
            {
                if (CatalogoId == 0 || TablaFisicaId == 0)
                {
                    return Json(new { Hecho = false, Mensaje = "No pudimos obtener el registro o el catalago, por favor intentelo de nuevo." }, JsonRequestBehavior.AllowGet);
                }
                //AuxInsertCatalogo Respuesta = new AuxInsertCatalogo();
                var Respuesta = EditDynamicScriptForTable(this.GetTableName(CatalogoId), GetCamposByColumn(CatalogoId), Form, TablaFisicaId,CatalogoId);
                if (Respuesta.Length > 0)
                {
                    return Json(new { Hecho = false, Mensaje = Respuesta }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Hecho = true, Mensaje = "se guardo exitosamente" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AjaxEliminarRegistro(int CatalogoId = 0,int TablaFisicaId = 0)
        {

            if (CatalogoId == 0 || TablaFisicaId == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el catalogo y el registro del catalogo." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var catalogo = db.Catalogoes.Where(x => x.CatalogoId == CatalogoId && x.Activo).FirstOrDefault();
                //verifiicamos si no encontramos un catalogo
                if (catalogo == null)
                {
                    return Json(new { Hecho = false, Mensaje = "El catalogo no se ecuentra o esta inactivo, por favor volverlo a intentar mas tarde." }, JsonRequestBehavior.AllowGet);
                }
                var CamposCatalagos = this.GetValuesFromTable(CatalogoId, TablaFisicaId, catalogo.NombreTabla, usuario.OrganismoID ?? 0);
                List<cambiosCampos> cambiosCampos = new List<cambiosCampos>();
                foreach (var item in CamposCatalagos)
                {
                    cambiosCampos.Add(new cambiosCampos
                    {
                        campo_anterior= item.Valor,
                        campo_nuevo = null,
                        es_modificado=true,
                        nombre_campo=item.Etiqueta
                    });
                }

                var respuesta = this.eliminarDatosCatalogo(catalogo.NombreTabla, catalogo.Nombre,TablaFisicaId, usuario, cambiosCampos);
                if(!respuesta.Result)
                    return Json(new { Hecho = false, Mensaje = respuesta.Valor }, JsonRequestBehavior.AllowGet);

                return Json(new { Hecho = true, Mensaje = "Se elimino el registro"}, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        public ResultGuardarTablaFisica eliminarDatosCatalogo(string Table, string nombreLargo, int TablaFisciaId, ApplicationUser usuario, List<cambiosCampos> cambioCampos)
        {
            ResultGuardarTablaFisica respueesta = new ResultGuardarTablaFisica();
            respueesta.Result = true;
            try
            {
                //ncesitamos borrar la informaciónn que se guardo anteriormente
                var sQueryEliminar = $"DELETE TOP(1) FROM {Table} WHERE TablaFisicaId = {TablaFisciaId}; ";
                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                sqlConnection.Query(sQueryEliminar);
                sqlConnection.Close();

                //volteamos los datos para informarle que no se eliminaron
                var camposElimiandos = new List<cambiosCampos>();
                foreach (var campo in cambioCampos)
                {
                    camposElimiandos.Add(new cambiosCampos() { campo_anterior = campo.campo_nuevo, campo_nuevo = null, es_modificado = true, nombre_campo = campo.nombre_campo });
                }
                //Bitacora
                var res = HMTLHelperExtensions.Bitacora(camposElimiandos, Table, $"Catalogo(Tabla): {nombreLargo}", TablaFisciaId, usuario?.Id, 3);
            }
            catch (Exception ex)
            {
                respueesta.Result = false;
                respueesta.Valor = ex.Message;
            }
            return respueesta;
        }

        public string GetTableEjercicio(int iPlantillaId)
        {
            string sPeriodos = "";
            try
            {


                sPeriodos = DateTime.Now.ToString("yyyy");
            }
            catch (Exception ex)
            {

            }
            return sPeriodos;

        }
        //INSET QUERY
        public AuxInsertCatalogo InserDynamicScript(string Table, string Ejercicio, List<CampoCatalogo> Campos, FormCollection form)
        {
            AuxInsertCatalogo Respuesta = new AuxInsertCatalogo();
           
            try
            {


                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var Inseert = "";
                if (Campos.Count > 0)
                {
                    var first = true;
                    Inseert = $@"INSERT INTO { Table }";
                    Inseert += "(";

                    foreach (var item in Campos)
                    {
                        if (!first)
                        {
                            Inseert += ",";
                        }
                        Inseert += $@"{item.Nombre}";
                        first = false;
                    }
                    //campos estaticos

                    Inseert += ",Activo,OrganismoID,UsuarioId,OrganismoCapturoID,FechaCreacion";
                    Inseert += ") OUTPUT INSERTED.TablaFisicaId ";
                    Inseert += "VALUES (";
                    first = true;
                    foreach (var item in Campos)
                    {

                        string valor = "";
                        if (!first)
                        {
                            Inseert += ",";
                        }
                        if (item.Activo)
                        {
                            //string valor = item.TipoCampo != TipoCampo.ArchivoAdjunto ? form[$@"{item.Nombre}"] : NombreArchivo;
                            //HttpPostedFileBase file = Request.Files[0];
                            //
                            valor = form[$@"{item.Nombre}"];

                            if (item.TipoCampo == TipoCampo.ArchivoAdjunto)
                            {
                                var file = Request.Files[$"{item.Nombre}"];
                                if (file != null && file.ContentLength > 0)
                                {
                                    var respuestaFile = this.GuardarArchivos(file, Table, Ejercicio);
                                    if (respuestaFile.Result)
                                    {
                                        valor = respuestaFile.Valor;
                                    }
                                    else
                                    {
                                        Respuesta.Respuesta = respuestaFile.Valor;
                                        return Respuesta;
                                    }
                                }
                            }
                            if (item.TipoCampo == TipoCampo.CasillaVerificacion)
                            {
                                valor = valor == null ? "0" : valor;
                            }
                            if (valor == "" || valor == null &&
                                (item.TipoCampo == TipoCampo.Decimal ||
                                item.TipoCampo == TipoCampo.Dinero ||
                                item.TipoCampo == TipoCampo.Numerico ||
                                item.TipoCampo == TipoCampo.Porcentaje ||
                                item.TipoCampo == TipoCampo.Fecha ||
                                item.TipoCampo == TipoCampo.ArchivoAdjunto ||
                                item.TipoCampo == TipoCampo.Catalogo))
                            {
                                valor = "NULL";
                            }
                            if (valor == "0" || valor == null && item.TipoCampo == TipoCampo.Catalogo)
                            {
                                valor = "NULL";
                            }
                            if (item.TipoCampo == TipoCampo.Decimal || item.TipoCampo == TipoCampo.Dinero || item.TipoCampo == TipoCampo.Numerico || item.TipoCampo == TipoCampo.Porcentaje)
                            {
                                valor = valor.Replace("(", "").Replace(")", "").Replace(",", "");
                            }

                        }
                        else
                        {
                            valor = "NULL";
                            if (item.Requerido)
                            {
                                valor = HMTLHelperExtensions.GetDefaultValue(item, true);
                            }

                        }



                        Inseert += GetValueByCampo(item, valor);
                        first = false;
                    }
                    Inseert += $@",1,{usuario.OrganismoID ?? 0},'{usuario.Id}',{usuario.OrganismoID ?? 0},'{ DateTime.Now.ToString("dd/MM/yyyy") }'";
                    Inseert += ") ";

                }

                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                var insertedId = sqlConnection.Query<int>(Inseert).FirstOrDefault();
                sqlConnection.Close();
                Respuesta.insertedId = insertedId;

                //Bitacora
                //var res = HMTLHelperExtensions.Bitacora(cambioCampos, Table, $"Plantilla: {nombreLargo}", inserrtedId, usuario?.Id);
            }
            catch (Exception ex)
            {
                Respuesta.Respuesta = ex.Message;
            }

            return Respuesta;

        }
        public AuxInsertCatalogo InserDynamicScriptForTable(string Table, string Ejercicio, List<CampoCatalogo> Campos, FormCollection form,int Idregistro,int CatalogoId)
        {
            AuxInsertCatalogo Respuesta = new AuxInsertCatalogo();

            try
            {


                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var Inseert = "";
                if (Campos.Count > 0)
                {
                    var first = true;
                    Inseert = $@"INSERT INTO { Table }";
                    Inseert += "(";

                    foreach (var item in Campos)
                    {
                        if (!first)
                        {
                            Inseert += ",";
                        }
                        Inseert += $@"{item.Nombre}";
                        first = false;
                    }
                    //campos estaticos

                    Inseert += ",Activo,OrganismoID,UsuarioId,OrganismoCapturoID,FechaCreacion,Idregistro";
                    Inseert += ") OUTPUT INSERTED.TablaFisicaId ";
                    Inseert += "VALUES (";
                    first = true;
                    foreach (var item in Campos)
                    {

                        string valor = "";
                        if (!first)
                        {
                            Inseert += ",";
                        }
                        if (item.Activo)
                        {
                            //string valor = item.TipoCampo != TipoCampo.ArchivoAdjunto ? form[$@"{item.Nombre}"] : NombreArchivo;
                            //HttpPostedFileBase file = Request.Files[0];
                            //

                            string nombreField = $"field{CatalogoId}_{item.Nombre}";
                            valor = form[$@"{nombreField}"];

                            if (item.TipoCampo == TipoCampo.ArchivoAdjunto)
                            {
                                var file = Request.Files[$"{nombreField}"];
                                if (file != null && file.ContentLength > 0)
                                {
                                    var respuestaFile = this.GuardarArchivos(file, Table, Ejercicio);
                                    if (respuestaFile.Result)
                                    {
                                        valor = respuestaFile.Valor;
                                    }
                                    else
                                    {
                                        Respuesta.Respuesta = respuestaFile.Valor;
                                        return Respuesta;
                                    }
                                }
                            }
                            if (item.TipoCampo == TipoCampo.CasillaVerificacion)
                            {
                                valor = valor == null ? "0" : valor;
                            }
                            if (valor == "" || valor == null &&
                                (item.TipoCampo == TipoCampo.Decimal ||
                                item.TipoCampo == TipoCampo.Dinero ||
                                item.TipoCampo == TipoCampo.Numerico ||
                                item.TipoCampo == TipoCampo.Porcentaje ||
                                item.TipoCampo == TipoCampo.Fecha ||
                                item.TipoCampo == TipoCampo.ArchivoAdjunto ||
                                item.TipoCampo == TipoCampo.Catalogo))
                            {
                                valor = "NULL";
                            }
                            if (valor == "0" || valor == null && item.TipoCampo == TipoCampo.Catalogo)
                            {
                                valor = "NULL";
                            }
                            if (item.TipoCampo == TipoCampo.Decimal || item.TipoCampo == TipoCampo.Dinero || item.TipoCampo == TipoCampo.Numerico || item.TipoCampo == TipoCampo.Porcentaje)
                            {
                                valor = valor.Replace("(", "").Replace(")", "").Replace(",", "");
                            }

                        }
                        else
                        {
                            valor = "NULL";
                            if (item.Requerido)
                            {
                                valor = HMTLHelperExtensions.GetDefaultValue(item, true);
                            }

                        }



                        Inseert += GetValueByCampo(item, valor);
                        first = false;
                    }
                    Inseert += $@",1,{usuario.OrganismoID ?? 0},'{usuario.Id}',{usuario.OrganismoID ?? 0},'{ DateTime.Now.ToString("dd/MM/yyyy") }',{Idregistro}";
                    Inseert += ") "; 

                }

                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                var insertedId = sqlConnection.Query<int>(Inseert).FirstOrDefault();
                sqlConnection.Close();
                Respuesta.insertedId = insertedId;

                //Bitacora
                //var res = HMTLHelperExtensions.Bitacora(cambioCampos, Table, $"Plantilla: {nombreLargo}", inserrtedId, usuario?.Id);
            }
            catch (Exception ex)
            {
                Respuesta.Respuesta = ex.Message;
            }

            return Respuesta;

        }

        ////Guardar archivos
        //public ResultGuardarTablaFisica GuardarArchivos(HttpPostedFileBase Archivo, string nombreCarpeta = "")
        //{
        //    var respuesta = new ResultGuardarTablaFisica();
        //    respuesta.Result = false;
        //    respuesta.Valor = "ocurrio un error al momento de subir el archivo.";
        //    try
        //    {

        //        //Creamos directoriro
        //        string Directorio = Server.MapPath("~/") + "\\Documentos\\" + nombreCarpeta;

        //        bool exists = System.IO.Directory.Exists(Directorio);
        //        if (!exists)
        //            System.IO.Directory.CreateDirectory(Directorio);

        //        if (Archivo != null)
        //        {
        //            var random = new Random();
        //            var r = random.Next(1234567, 12345678);

        //            //string tempName = Path.GetFileNameWithoutExtension(Archivo.FileName);
        //            //tempName = tempName.Substring(0, ((tempName.Trim().Length) > 20 ? 20 : tempName.Trim().Length));
        //            string nombreArchAnexo = r.ToString() + "_" + (DateTime.Now.ToString("yyyyMMddHHmmss")) + Path.GetExtension(Archivo.FileName);
        //            //mandeee
        //            var PathArchivo = Directorio + @"\" + nombreArchAnexo;
        //            Archivo.SaveAs(PathArchivo);
        //            respuesta.Result = true;
        //            respuesta.Valor = $@"\{nombreCarpeta}\{nombreArchAnexo}";
        //        }
        //        return respuesta;
        //    }
        //    catch (Exception ex)
        //    {
        //        respuesta.Result = false;
        //        respuesta.Valor = ex.Message;
        //    }
        //    return respuesta;
        //}


        //Guardar archivos
        public ResultGuardarTablaFisica GuardarArchivos(HttpPostedFileBase Archivo, string nombreCarpeta = "", string EjercicioTxt = "")
        {
            EjercicioTxt = EjercicioTxt != null && EjercicioTxt.Length > 0 ? EjercicioTxt : "NoEspecificado";
            nombreCarpeta = nombreCarpeta != null && nombreCarpeta.Length > 0 ? nombreCarpeta : "NoEspecificado";
            var respuesta = new ResultGuardarTablaFisica();
            respuesta.Result = false;
            respuesta.Valor = "ocurrio un error al momento de subir el archivo.";
            try
            {

                //ruta: Archivos/nombrePlantilla/ejercicio/mes/

                //Creamos directoriro
                string Directorio = $@"{Server.MapPath("~/")}Documentos\{nombreCarpeta}";

                bool exists = System.IO.Directory.Exists(Directorio);
                if (!exists)
                    System.IO.Directory.CreateDirectory(Directorio);

                if (Archivo != null)
                {
                    var random = new Random();
                    var r = random.Next(1234567, 12345678);

                    //string tempName = Path.GetFileNameWithoutExtension(Archivo.FileName);
                    //tempName = tempName.Substring(0, ((tempName.Trim().Length) > 20 ? 20 : tempName.Trim().Length));
                    string nombreArchAnexo = r.ToString() + "_" + (DateTime.Now.ToString("yyyyMMddHHmmss")) + Path.GetExtension(Archivo.FileName);
                    //mandeee
                    var PathArchivo = Directorio + @"\" + nombreArchAnexo;
                    Archivo.SaveAs(PathArchivo);
                    respuesta.Result = true;
                    respuesta.Valor = $@"\{nombreCarpeta}\{nombreArchAnexo}";
                }
                return respuesta;
            }
            catch (Exception ex)
            {
                respuesta.Result = false;
                respuesta.Valor = ex.Message;
            }
            return respuesta;
        }

        public ActionResult DetailDynamic(int? Id, int? iCatalogoId = 0, string TablaNombre = "")
        {
            if (Id == null)
            {
                return RedirectToAction("IndexPlantillas", "Plantillas");
            }
            var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();

            if (usuario == null)
            {
                return RedirectToAction("IndexPlantillas", "Plantillas");
            }

            List<CampoCatalogo> LstCampos = this.GetValuesFromTable((int)iCatalogoId, (int)Id, TablaNombre, usuario.OrganismoID ?? 0);
            ViewBag.Validation = HMTLHelperExtensions.GetValidationJquery(LstCampos);
            ViewBag.TablaFisicaId = Id;
            ViewBag.TablaNombre = TablaNombre;
            ViewBag.CatalogoId = iCatalogoId;
            return View(LstCampos);
        }

        //Edit Dynamic
        public ActionResult EditDynamic(int? Id, int? iCatalogoId = 0, string TablaNombre = "")
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Catalogos");
            }
            if (TablaNombre == null || TablaNombre.Length == 0)
            {
                return RedirectToAction("Index", "Catalogos");
            }
            var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();

            if (usuario == null)
            {
                return RedirectToAction("Index", "Catalogos");
            }
            this.VerifyNecessaryColumn((int)iCatalogoId);
            List<CampoCatalogo> LstCampos = this.GetValuesFromTable((int)iCatalogoId, (int)Id, TablaNombre, usuario.OrganismoID ?? 0);
            //List<Campo> campos = GetCamposByColumn(Id);
            ViewBag.CatalogoId = iCatalogoId;
            ViewBag.Validation = HMTLHelperExtensions.GetValidationJquery(LstCampos);
            return View(LstCampos);
        }

        [HttpPost]
        public ActionResult EditDynamic(int CatalogoId, FormCollection Form, int TablaFisicaId = 0)
        {

            try
            {

                var Respuesta = EditDynamicScript(this.GetTableName(CatalogoId), GetCamposByColumn(CatalogoId), Form, TablaFisicaId);
                if (Respuesta.Length > 0)
                {
                    return Json(new { Hecho = false, Mensaje = Respuesta }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Hecho = true, Mensaje = "se guardo exitosamente" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public List<CampoCatalogo> GetValuesFromTable(int Id, int TablaFisicaId, string TablaNombre, int OrganismoID)
        {
            List<CampoCatalogo> LstCampos = new List<CampoCatalogo>();
            try
            {
                List<CampoCatalogo> Campos = GetCamposByColumn(Id);
                string sQuery = "";
                bool first = true;
                if (Campos.Count > 0)
                {
                    sQuery = $@"SELECT TOP 1 TablaFisicaId,";
                    foreach (var item in Campos)
                    {
                        if (!first)
                        {
                            sQuery += ",";
                        }
                        sQuery += $@"{item.Nombre}";
                        first = false;
                    }
                    sQuery += $@" FROM {TablaNombre} WHERE TablaFisicaId={TablaFisicaId}";

                }

                SqlConnection con = new SqlConnection(coneccion);
                con.Open();
                SqlCommand cmd = new SqlCommand(sQuery, con);
                DataTable myTable = new DataTable();
                myTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in myTable.Rows)
                {
                    foreach (var item in Campos)
                    {
                        item.Valor = row[$"{item.Nombre}"].ToString();
                        item.ValorUrl = Url.Action("GetAttachment", "Plantillas", new { url = item.Valor });
                        LstCampos.Add(item);

                    }
                    ViewBag.TablaFisicaId = int.Parse(row["TablaFisicaId"].ToString());
                }
                //DataColumn column = myTable.Columns[0];
                //// zero based index of column, alternatively use column name
                //string typeOfColumn = column.DataType.Name;
                //// or column.DataType.FullName to get the fully qualified name of the System.Type
                con.Close();
            }
            catch (Exception ex)
            {

            }


            return LstCampos;
        }


        public List<VMCampos> GetSelectFromTable(int Id, int OrganismoID, FormCollection form, List<CampoCatalogo> Campos = null)
        {
            ViewBag.sPrincipal = "";
            string sQuery = "";
            List<VMCampos> LstVMCampos = new List<VMCampos>();
            try
            {
                //var urls = Url.Action("Action", "Controller");
                string NombreTabla = db.Catalogoes.Where(x => x.CatalogoId == Id).Select(x => x.NombreTabla).FirstOrDefault();
                var mCamposCatalogos = db.CampoCatalogo.Where(x => x.CatalogoId == Id && x.Principal).FirstOrDefault();
                //List<CampoCatalogo> Campos = GetCamposByColumn(Id);

                string sPrincipal = mCamposCatalogos?.Nombre;
                ViewBag.sPrincipal = mCamposCatalogos != null ?  mCamposCatalogos?.Etiqueta : "No especificado";
                var InnerA = "a.";
                var InnerB = $"{sPrincipal}";
                var nombreInnerBTable = "";
                if (mCamposCatalogos != null &&  mCamposCatalogos.TipoCampo == TipoCampo.Catalogo)
                {
                    sPrincipal = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(mCamposCatalogos.iCatalogoId, true);
                    
                    InnerB = $"b.{sPrincipal}";
                    nombreInnerBTable = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(mCamposCatalogos.iCatalogoId);
                }

                sQuery = $@"SELECT {InnerA}TablaFisicaId as aTablaFisicaId, {InnerA}Activo as aActivo, {InnerA}FechaCreacion as aFechaCreacion";
                if (sPrincipal != null && sPrincipal.Length > 0)
                {

                    sQuery += $",{InnerB}";
                }
                //{(mCamposCatalogos.TipoCampo == TipoCampo.Catalogo ? " a" : "")}
                sQuery += $@" FROM {NombreTabla} a";

                sQuery += $@" {(mCamposCatalogos != null && mCamposCatalogos.TipoCampo == TipoCampo.Catalogo ? (" LEFT JOIN " +nombreInnerBTable + $" b ON a.{mCamposCatalogos?.Nombre} = b.TablaFisicaId "): "")} ";

                //where Secction
                if (Campos != null && Campos.Count > 0)
                {
                    var bWhereFirst = true;
                    foreach (var item in Campos.Where(x => x.TipoCampo != TipoCampo.ArchivoAdjunto).ToList())
                    {
                        string valor = form[$@"{item.Nombre}"];

                        if (valor != null && valor != "")
                        {
                            sQuery += bWhereFirst ? "WHERE " : "";
                            sQuery += !bWhereFirst ? " AND " : "";
                            var sLikeIgual = "=";
                            
                            if (item.TipoCampo == TipoCampo.Texto || item.TipoCampo == TipoCampo.AreaTexto || item.TipoCampo == TipoCampo.Alfanumerico || item.TipoCampo == TipoCampo.email
                                || item.TipoCampo == TipoCampo.Hipervinculo || item.TipoCampo == TipoCampo.Telefono)
                            {
                                sLikeIgual = "LIKE";
                            }
                            if (item.TipoCampo == TipoCampo.Decimal || item.TipoCampo == TipoCampo.Dinero || item.TipoCampo == TipoCampo.Numerico || item.TipoCampo == TipoCampo.Porcentaje)
                            {
                                valor = valor.Replace("(", "").Replace(")", "").Replace(",", "");
                            }
                            //poneemos el where
                            if (item._TipoFecha == TipoFecha.FechaHasta)
                            {

                            } else if (item._TipoFecha == TipoFecha.FechaDesde)
                            {

                            }
                            else
                            {

                                sQuery += $@"{item.Nombre} {sLikeIgual} {GetValueByCampo(item, valor, (sLikeIgual == "LIKE"))}";

                            }

                            bWhereFirst = false;

                        }
                    }
                }

                sQuery += $" ORDER BY {InnerA}TablaFisicaId DESC";




                
                SqlConnection con = new SqlConnection(coneccion);
                con.Open();
                SqlCommand cmd = new SqlCommand(sQuery, con);
                DataTable myTable = new DataTable();
                myTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in myTable.Rows)
                {
                    var VMCampo = new VMCampos();
                    VMCampo.TablaFisicaId = Convert.ToInt32(row[$"aTablaFisicaId"].ToString());
                    VMCampo.Activo = Convert.ToBoolean(row[$"aActivo"].ToString());
                    try { VMCampo.FechaCreacion = Convert.ToDateTime(row[$"aFechaCreacion"].ToString()); }catch(Exception) { }
                    try { VMCampo.SFechaCreacion = HMTLHelperExtensions.GetFormatForSelect(TipoCampo.Fecha, row[$"aFechaCreacion"].ToString()); } catch (Exception) { } 
                    if(mCamposCatalogos != null)
                    {
                        VMCampo.DatoPrincipal = HMTLHelperExtensions.GetFormatForSelect(mCamposCatalogos.TipoCampo, row[$"{sPrincipal}"].ToString(), mCamposCatalogos._ConDecimales);
                    }
                    else
                    {
                        VMCampo.DatoPrincipal = "No se especifico campo principal";
                    }
                    
                    VMCampo.TablaNombre = NombreTabla;
                    VMCampo.CatalogoId = Id;
                    LstVMCampos.Add(VMCampo);
                }
                con.Close();
            }
            catch (Exception ex)
            {
                var VMCampo = new VMCampos();
                VMCampo.DatoPrincipal = ex.Message;

                VMCampo.CatalogoId = Id;
                LstVMCampos.Add(VMCampo);
            }


            return LstVMCampos;
        }


        public void GenerarFiltros(int Id = 0)
        {
            try
            {
                var mCamposCatalogos = db.CampoCatalogo.Where(x => x.CatalogoId == Id && x.Activo).ToList();
                ViewBag.Filtros = mCamposCatalogos;

            }
            catch (Exception ex) {
            }
        }


        public ActionResult IndexDatosCatalogos(FormCollection form, int id = 0)
        {


            //GetCamposByColumn(CatalogoId)


            var lista = GetSelectFromTable(id, 0, form, GetCamposByColumn(id));


            //combos();
            //string[] searchstringsN = filtros.Nombre != null && filtros.Nombre.Length > 0 ? filtros.Nombre.Split(' ') : "".Split(' ');
            //string[] searchstringsD = filtros.Descripcion != null && filtros.Descripcion.Length > 0 ? filtros.Descripcion.Split(' ') : "".Split(' ');

            //ViewBag.Filtros = filtros;
            //ViewBag.Order = sOrder;
            //ViewBag.PerPage = PerPage;
            //ViewBag.Pagina = iPagina;
            //ViewBag.OrderNombre = sOrder == "Nombre" ? "Nombre_desc" : "Nombre";
            //ViewBag.OrderDescripcion = sOrder == "Descripcion" ? "Descripcion_desc" : "Descripcion";
            //ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";
            //IPagedList<Catalogo> vModel = null;

            //var query = db.Catalogoes.Where(x =>
            //    ((filtros.Nombre == null || filtros.Nombre.Length == 0) || searchstringsN.Any(y => x.Nombre.ToLower().Contains(y.ToLower())))
            //    && ((filtros.Descripcion == null || filtros.Descripcion.Length == 0) || searchstringsD.Any(y => x.Descripcion.ToLower().Contains(y.ToLower())))
            //    && (filtros.ActivoNull == null || filtros.ActivoNull == x.Activo)
            //   );

            //switch (sOrder)
            //{
            //    case "Nombre":
            //        vModel = query.OrderBy(x => x.Nombre).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "Nombre_desc":
            //        vModel = query.OrderByDescending(x => x.Nombre).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "Descripcion":
            //        vModel = query.OrderBy(x => x.Descripcion).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "Descripcion_desc":
            //        vModel = query.OrderByDescending(x => x.Descripcion).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "Activo":
            //        vModel = query.OrderBy(x => x.Activo).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "Activo_desc":
            //        vModel = query.OrderByDescending(x => x.Activo).ToPagedList(iPagina, PerPage);
            //        break;
            //    default:
            //        vModel = query.OrderBy(x => x.Nombre).ToPagedList(iPagina, PerPage);
            //        break;
            //}
            if (Request.IsAjaxRequest())
            {
                return PartialView("_listaDatosCatalogos", lista);
            }
            GenerarFiltros(id);
            ViewBag.iId = id;
            return View(lista);
        }

       [HttpPost] 
        public ActionResult IndexDatosCatalogosPOST(FormCollection form, int iId = 0)
        {

            var lista = GetSelectFromTable(iId, 0, form, GetCamposByColumn(iId));

           
            if (Request.IsAjaxRequest())
            {
                return PartialView("_listaDatosCatalogos", lista);
            }
            GenerarFiltros(iId);
            ViewBag.iId = iId;
            return View(lista);
        }

       

        //Edit QUERY
        public string EditDynamicScript(string Table, List<CampoCatalogo> Campos, FormCollection form, int TablaFisicaId = 0)
        {

            string Respuesta = "";
            try
            {


                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var Inseert = "";

                if (Campos.Count > 0)
                {
                    var first = true;
                    Inseert = $@"UPDATE { Table } ";
                    Inseert += "SET ";

                    foreach (var item in Campos)
                    {
                        if (!first)
                        {
                            Inseert += ",";
                        }

                        string valor = form[$@"{item.Nombre}"];
                        if (item.TipoCampo == TipoCampo.ArchivoAdjunto)
                        {
                            var file = Request.Files[$"{item.Nombre}"];
                            if (file != null && file.ContentLength > 0)
                            {
                                var respuestaFile = this.GuardarArchivos(file, Table);
                                if (respuestaFile.Result)
                                {
                                    valor = respuestaFile.Valor;
                                }
                                else
                                {
                                    return respuestaFile.Valor;
                                }
                            }

                        }
                        if (item.TipoCampo == TipoCampo.CasillaVerificacion)
                        {
                            valor = valor == null ? "0" : valor;
                        }
                        if (valor == "" &&
                            (item.TipoCampo == TipoCampo.Decimal ||
                            item.TipoCampo == TipoCampo.Dinero ||
                            item.TipoCampo == TipoCampo.Numerico ||
                            item.TipoCampo == TipoCampo.Porcentaje))
                        {
                            valor = "NULL";
                        }
                        if (item.TipoCampo == TipoCampo.Decimal || item.TipoCampo == TipoCampo.Dinero || item.TipoCampo == TipoCampo.Numerico || item.TipoCampo == TipoCampo.Porcentaje)
                        {
                            valor = valor.Replace("(", "").Replace(")", "").Replace(",", "");
                        }
                        Inseert += item.TipoCampo == TipoCampo.ArchivoAdjunto && valor == null ? "" : $@"{item.Nombre}=";
                        Inseert += item.TipoCampo == TipoCampo.ArchivoAdjunto && valor == null ? "" : GetValueByCampo(item, valor);

                        if (item.TipoCampo == TipoCampo.ArchivoAdjunto && valor == null && !first)
                        {
                            Inseert = Inseert.Substring(0, (Inseert.Length - 1));
                        }

                        first = false;
                    }
                    Inseert += $@" WHERE TablaFisicaId={TablaFisicaId}";

                }

                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                sqlConnection.Query(Inseert);
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                Respuesta = ex.Message;
            }

            return Respuesta;

        }
        public string EditDynamicScriptForTable(string Table, List<CampoCatalogo> Campos, FormCollection form, int TablaFisicaId = 0,int CatalogoId = 0)
        {

            string Respuesta = "";
            try
            {


                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var Inseert = "";

                if (Campos.Count > 0)
                {
                    var first = true;
                    Inseert = $@"UPDATE { Table } ";
                    Inseert += "SET ";

                    foreach (var item in Campos)
                    {
                        if (!first)
                        {
                            Inseert += ",";
                        }
                        string strNombre = $"field{CatalogoId}_{item.Nombre}";
                        string valor = form[$@"{strNombre}"];
                        if (item.TipoCampo == TipoCampo.ArchivoAdjunto)
                        {
                            var file = Request.Files[$"{strNombre}"];
                            if (file != null && file.ContentLength > 0)
                            {
                                var respuestaFile = this.GuardarArchivos(file, Table);
                                if (respuestaFile.Result)
                                {
                                    valor = respuestaFile.Valor;
                                }
                                else
                                {
                                    return respuestaFile.Valor;
                                }
                            }

                        }
                        if (item.TipoCampo == TipoCampo.CasillaVerificacion)
                        {
                            valor = valor == null ? "0" : valor;
                        }
                        if (valor == "" &&
                            (item.TipoCampo == TipoCampo.Decimal ||
                            item.TipoCampo == TipoCampo.Dinero ||
                            item.TipoCampo == TipoCampo.Numerico ||
                            item.TipoCampo == TipoCampo.Porcentaje))
                        {
                            valor = "NULL";
                        }
                        if (item.TipoCampo == TipoCampo.Decimal || item.TipoCampo == TipoCampo.Dinero || item.TipoCampo == TipoCampo.Numerico || item.TipoCampo == TipoCampo.Porcentaje)
                        {
                            valor = valor.Replace("(", "").Replace(")", "").Replace(",", "");
                        }
                        Inseert += item.TipoCampo == TipoCampo.ArchivoAdjunto && valor == null ? "" : $@"{item.Nombre}=";
                        Inseert += item.TipoCampo == TipoCampo.ArchivoAdjunto && valor == null ? "" : GetValueByCampo(item, valor);

                        if (item.TipoCampo == TipoCampo.ArchivoAdjunto && valor == null && !first)
                        {
                            Inseert = Inseert.Substring(0, (Inseert.Length - 1));
                        }

                        first = false;
                    }
                    Inseert += $@" WHERE TablaFisicaId={TablaFisicaId}";

                }

                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                sqlConnection.Query(Inseert);
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                Respuesta = ex.Message;
            }

            return Respuesta;

        }
        public List<CampoCatalogo> GetCamposByColumn(int? iId = 0,bool procesar= false)
        {
            List<CampoCatalogo> ListaCampos = new List<CampoCatalogo>();
            try
            {
                var NombreTabla = db.Catalogoes.Where(x => x.CatalogoId == iId).Select(x => x.NombreTabla).FirstOrDefault();
                var LstCampos = GetColumnTable(NombreTabla);
                if (procesar)
                {
                    ListaCampos = db.CampoCatalogo.Where(x => x.CatalogoId == iId && LstCampos.Contains(x.Nombre)).ToList();
                }
                else
                {
                    ListaCampos = db.CampoCatalogo.Where(x => x.CatalogoId == iId && LstCampos.Contains(x.Nombre) && x.Activo).ToList();
                }
                
            }
            catch (Exception ex)
            {

            }
            return ListaCampos;
        }

        [AllowAnonymous]
        public ActionResult GetAttachment(string url)
        {
            try
            {
                string Directorio = Server.MapPath("~/") + "\\Documentos" + url;
                FileStream fs = new FileStream(Directorio, FileMode.Open, FileAccess.Read);
                var file = File(fs.Name, MimeMapping.GetMimeMapping(fs.Name), Path.GetFileName(fs.Name));
                fs.Close();
                return file;
            }
            catch (Exception e)
            {
                return HttpNotFound();
            }
        }


        public bool? changeStatusDynamic(int TablaFisicaId = 0,string NombreTabla="")
        {
            //bitacora
            var activo = false;
            List<cambiosCampos> cambioCampos = new List<cambiosCampos>();
            try
            {
                string sUpdate = "";
                string sQuery = "";
                if (TablaFisicaId == 0)
                {
                    return null;
                }
                if (NombreTabla == null || NombreTabla.Length == 0)
                {
                    return null;
                }

                sQuery = $@"SELECT TOP 1 TablaFisicaId, Activo FROM {NombreTabla} WHERE TablaFisicaId={TablaFisicaId}";
                var mVMCampos = db.Database.SqlQuery<VMCampos>(sQuery).FirstOrDefault();

                if (mVMCampos == null)
                {
                    return null;
                }
                if (mVMCampos.Activo == true)
                {
                    mVMCampos.Activo = activo = false;
                }
                else
                {
                    mVMCampos.Activo = activo = true;
                }
                var iActivo = mVMCampos.Activo != null && mVMCampos.Activo == true ? 1 : 0;
                sUpdate = $@"UPDATE  {NombreTabla} SET Activo={ iActivo } WHERE TablaFisicaId={TablaFisicaId}";
                int noOfRowUpdated = db.Database.ExecuteSqlCommand(sUpdate);

                //Bitacora
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Estatus del registro",
                    es_modificado = false,
                    campo_nuevo = mVMCampos.Activo.HasValue && mVMCampos.Activo.Value ? "Activo" : "Inactivo",
                    campo_anterior = mVMCampos.Activo.HasValue && mVMCampos.Activo.Value ? "Inactivo" : "Activo"
                });
                //Bitacora
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, NombreTabla, $"Tabla: {NombreTabla}", TablaFisicaId, usuario?.Id, 0);

            }
            catch(Exception ex)
            {

            }
            //return View("Index",horariosModels);
            return activo;
        }


        public bool? CambiarStatusCampo(int id = 0)
        {
            var activo = false;
            if (id == 0)
            {
                return null;
            }
            var Model = db.CampoCatalogo.Where(X=>X.CampoCatalogoId == id).FirstOrDefault();
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
        
        //updateCatalogos
        [HttpPost]
        public ActionResult GetCampoById(int iId=0)
        {
            if (iId == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el campo." }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var mCampo = db.CampoCatalogo.Where(x => x.CampoCatalogoId == iId).FirstOrDefault();

                if (mCampo != null)
                {
                    //var mCatalago = db.Catalogoes.Where(x => x.CatalogoId == mCampo.iCatalogoId).FirstOrDefault();
                    //mCampo.NombreCatalago = mCatalago != null ? mCatalago.Nombre : "";
                    return Json(new { Hecho = true, Campo = mCampo }, JsonRequestBehavior.AllowGet); 
                }

                return Json(new { Hecho = false, Mensaje = $"No se encontro el campo, intentelo de nuevo." }, JsonRequestBehavior.AllowGet);  
                  
            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = $"Ocurrio un error,Error: '{ex.Message}' " }, JsonRequestBehavior.AllowGet);  
            }
            
        }
        public ActionResult AjaxModificarCampo(LstCampos tblCampoDinamico, int iIdA = 0,bool yaLoValido = false)
        {

            if (iIdA == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el Catalogo." }, JsonRequestBehavior.AllowGet);
            }
            var mCampoNuevo = new CampoCatalogo();
            //var CCampos = db.CampoCatalogo.Where(x => x.CatalogoId == iIdA).Count();
            var mCampoViejo = db.CampoCatalogo.Where(x => x.CampoCatalogoId == tblCampoDinamico.CampoCatalogoId).FirstOrDefault();
            //paraBitacora
            var oldModel = (CampoCatalogo)mCampoViejo.Clone();

            if (mCampoViejo == null)
            {
                return Json(new { Hecho = false, Mensaje = "No podimos encontrar el campo intentelo de nuievo." }, JsonRequestBehavior.AllowGet);
            }
            //verificamos el campo viejo
            mCampoViejo.Nombre = GenerarNombreCampos(mCampoViejo.Nombre);

            mCampoNuevo.Ayuda = tblCampoDinamico.AddAyuda;
            mCampoNuevo._ConDecimales = tblCampoDinamico.AddConDecimales;
            mCampoNuevo.Etiqueta = tblCampoDinamico.AddEtiqueta;
            mCampoNuevo._GrupoExtensionId = tblCampoDinamico.AddGrupoExtensionId;
            mCampoNuevo.Longitud = tblCampoDinamico.AddLongitud;
            mCampoNuevo.Nombre = GenerarNombreCampos(tblCampoDinamico.AddNombre);
            mCampoNuevo.Requerido = tblCampoDinamico.AddRequerido;
            mCampoNuevo._Size = tblCampoDinamico.AddSize;
            mCampoNuevo.TipoCampo = (TipoCampo)tblCampoDinamico.AddTipoCampo;
            mCampoNuevo._TipoFecha = (TipoFecha)tblCampoDinamico.AddTipoFecha;
            mCampoNuevo._TipoFecha = (TipoFecha)tblCampoDinamico.AddTipoFecha;
            mCampoNuevo._ConDecimales = tblCampoDinamico.AddConDecimales;
            mCampoNuevo._GrupoExtensionId = tblCampoDinamico.AddGrupoExtensionId;
            mCampoNuevo._Size = tblCampoDinamico.AddSize;
            mCampoNuevo.Principal = tblCampoDinamico.AddPrincipal;
            mCampoNuevo.iCatalogoId = tblCampoDinamico.AddCatalogoId;
            mCampoNuevo.IdCampoPNT = tblCampoDinamico.AddCampoPNT;
            mCampoNuevo.IdTipoCampoPNT = tblCampoDinamico.AddTipoCampoPNT;
            try
            {
                var Respuesta = HMTLHelperExtensions.GetInfoOfUpdate(mCampoViejo, mCampoNuevo);
                if (Respuesta.Length > 0)
                {
                    if (yaLoValido)
                    {
                        Respuesta = HMTLHelperExtensions.RemoveInformationFromTable(mCampoViejo,mCampoNuevo);
                        if(Respuesta.Length > 0)
                        {
                            return Json(new { Hecho = false, Mensaje = Respuesta }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { Hecho = true, Respuesta = Respuesta }, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    Respuesta = HMTLHelperExtensions.ModificarCampoWithoutRemove(mCampoViejo, mCampoNuevo);
                    if (Respuesta.Length > 0)
                    {
                        return Json(new { Hecho = false, Mensaje = Respuesta }, JsonRequestBehavior.AllowGet);
                    }
                }

                if (tblCampoDinamico.AddPrincipal)
                {
                    var resultado = RemoveAllPrincipal(iIdA);
                }

                mCampoViejo.Ayuda = tblCampoDinamico.AddAyuda;
                mCampoViejo._ConDecimales = tblCampoDinamico.AddConDecimales;
                mCampoViejo.Etiqueta = tblCampoDinamico.AddEtiqueta;
                mCampoViejo._GrupoExtensionId = tblCampoDinamico.AddGrupoExtensionId;
                mCampoViejo.Longitud = tblCampoDinamico.AddLongitud;
                mCampoViejo.Nombre = GenerarNombreCampos(tblCampoDinamico.AddNombre);
                mCampoViejo._Size = tblCampoDinamico.AddSize;
                mCampoViejo.TipoCampo = (TipoCampo)tblCampoDinamico.AddTipoCampo;
                mCampoViejo._TipoFecha = (TipoFecha)tblCampoDinamico.AddTipoFecha;
                mCampoViejo._ConDecimales = tblCampoDinamico.AddConDecimales;
                mCampoViejo._GrupoExtensionId = tblCampoDinamico.AddGrupoExtensionId;
                mCampoViejo._Size = tblCampoDinamico.AddSize;
                mCampoViejo.Requerido = tblCampoDinamico.AddRequerido;
                mCampoViejo.Principal = tblCampoDinamico.AddPrincipal;
                mCampoViejo.iCatalogoId = tblCampoDinamico.AddCatalogoId;
                mCampoViejo.IdCampoPNT = tblCampoDinamico.AddCampoPNT;
                mCampoViejo.IdTipoCampoPNT = tblCampoDinamico.AddTipoCampoPNT;


                db.Entry(mCampoViejo).State = EntityState.Modified;
                db.SaveChanges();


                string viewContent = ConvertViewToString("_ListaCampos", GetCampos(iIdA));
                //Bitacora
                var catalogo = db.Catalogoes.Where(x => x.CatalogoId == mCampoViejo.CatalogoId).FirstOrDefault();
                this.CreateBitacora(oldModel, mCampoViejo, mCampoViejo.CatalogoId, catalogo?.Nombre);

                return Json(new { Hecho = true, Respuesta= Respuesta, Mensaje = "se guardo exitosamente", Partial = viewContent }, JsonRequestBehavior.AllowGet);

                }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult GetDetailCapo(int iId = 0)
        {
            if (iId == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el campo." }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var mCampo = db.CampoCatalogo.Where(x => x.CampoCatalogoId == iId).FirstOrDefault();

                if (mCampo != null)
                {
                    if(mCampo.TipoCampo == TipoCampo.Catalogo)
                    {
                        var mCatalago = db.Catalogoes.Where(x => x.CatalogoId == mCampo.iCatalogoId).FirstOrDefault();
                        mCampo.NombreCatalago = mCatalago != null ? mCatalago.Nombre : "";
                    }

                    if (mCampo.TipoCampo == TipoCampo.ArchivoAdjunto)
                    {
                        var mGrupoExtension = db.GrupoExtesiones.Where(x => x.GrupoExtensionId == mCampo._GrupoExtensionId).FirstOrDefault();
                        mCampo.NombreGrupoExtension = mGrupoExtension != null ? mGrupoExtension.Nombre : "";
                    }

                    if (mCampo.TipoCampo == TipoCampo.Fecha)
                    {
                        mCampo.NombreTipoFecha = mCampo._TipoFecha.GetDisplayName();
                    }


                    string viewContent = ConvertViewToString("_ConfigurarCamposDetails",mCampo);
                    return Json(new { Hecho = true, Partial = viewContent }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Hecho = false, Mensaje = $"No se encontro el campo, intentelo de nuevo." }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = $"Ocurrio un error,Error: '{ex.Message}' " }, JsonRequestBehavior.AllowGet);
            }

        }

        //updateCatalogos
        //[HttpPost]
        //public ActionResult GetCampoById(int iId = 0)
        //{
        //    if (iId == 0)
        //    {
        //        return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el campo." }, JsonRequestBehavior.AllowGet);
        //    }
        //    try
        //    {
        //        var mCampo = db.CampoCatalogo.Where(x => x.CampoCatalogoId == iId).FirstOrDefault();

        //        if (mCampo != null)
        //        {
        //            //var mCatalago = db.Catalogoes.Where(x => x.CatalogoId == mCampo.iCatalogoId).FirstOrDefault();
        //            //mCampo.NombreCatalago = mCatalago != null ? mCatalago.Nombre : "";
        //            return Json(new { Hecho = true, Campo = mCampo }, JsonRequestBehavior.AllowGet);
        //        }

        //        return Json(new { Hecho = false, Mensaje = $"No se encontro el campo, intentelo de nuevo." }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Hecho = false, Mensaje = $"Ocurrio un error,Error: '{ex.Message}' " }, JsonRequestBehavior.AllowGet);
        //    }

        //}


        //SUB TABLAS

        [HttpPost]
        public ActionResult GetCreateDynamicPartial(int CatalogoId = 0)
        {
            AuxCatalogoTablas auxCatalogoTablas = new AuxCatalogoTablas();
            try
            {
                if (CatalogoId == 0)
                {
                    return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el Catalogo." }, JsonRequestBehavior.AllowGet);
                }
                var Catalogo = db.Catalogoes.Where(x => x.CatalogoId == CatalogoId).FirstOrDefault();
                var CamposCatalagos = db.CampoCatalogo.Where(x => x.CatalogoId == CatalogoId && x.Activo).ToList();

                if (Catalogo == null)
                {
                    return Json(new { Hecho = false, Mensaje = "No podimos encontrar los campo intentelo de nuevo." }, JsonRequestBehavior.AllowGet);
                }
                if (!Catalogo.Activo)
                {
                    return Json(new { Hecho = false, Mensaje = "Este catalogo se encuentro inactivo, no se puede utilizar." }, JsonRequestBehavior.AllowGet);
                }

                this.VerifyNecessaryColumn(CatalogoId);

                //verificamos que cuente con los campos
                if (CamposCatalagos != null && CamposCatalagos.Count > 0)
                {
                    foreach (var item in CamposCatalagos)
                    {
                        //le ponesmo un nombre provicional para trabajr con los campos
                        item.NombreProvicional = $"field{Catalogo.CatalogoId}_{item.Nombre}";
                        auxCatalogoTablas.camposString += HMTLHelperExtensions.GetCampo(item,true);
                    }
                }
                //solicitamos las mascaras y validaciones necesarias
                auxCatalogoTablas.Validation = HMTLHelperExtensions.GetValidationJquery(CamposCatalagos, true,true, Catalogo.CatalogoId);
                auxCatalogoTablas.CatalogoId = Catalogo.CatalogoId;
                string viewContent = ConvertViewToString("_CamposTabla", auxCatalogoTablas);

                return Json(new { Hecho = true, Respuesta = "", Mensaje = Catalogo.Nombre, Partial = viewContent }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult GetEditDynamicPartial(int CatalogoId = 0,int TablaFisicaId = 0)
        {
            AuxCatalogoTablas auxCatalogoTablas = new AuxCatalogoTablas();
            try
            {
                if (CatalogoId == 0 || TablaFisicaId == 0)
                {
                    return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el Catalogo." }, JsonRequestBehavior.AllowGet);
                }
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                this.VerifyNecessaryColumn(CatalogoId);
                var Catalogo = db.Catalogoes.Where(x => x.CatalogoId == CatalogoId).FirstOrDefault();
                var CamposCatalagos = this.GetValuesFromTable(CatalogoId, TablaFisicaId, Catalogo.NombreTabla, usuario.OrganismoID ?? 0);

                if (Catalogo == null)
                {
                    return Json(new { Hecho = false, Mensaje = "No podimos encontrar los campo intentelo de nuevo." }, JsonRequestBehavior.AllowGet);
                }
                if (!Catalogo.Activo)
                {
                    return Json(new { Hecho = false, Mensaje = "Este catalogo se encuentro inactivo, no se puede utilizar." }, JsonRequestBehavior.AllowGet);
                }

                //verificamos que cuente con los campos
                if (CamposCatalagos != null && CamposCatalagos.Count > 0)
                {
                    foreach (var item in CamposCatalagos)
                    {
                        //le ponesmo un nombre provicional para trabajr con los campos
                        item.NombreProvicional = $"field{Catalogo.CatalogoId}_{item.Nombre}";
                        auxCatalogoTablas.camposString += HMTLHelperExtensions.GetCampoEdit(item, true);
                    }
                }
                //solicitamos las mascaras y validaciones necesarias
                auxCatalogoTablas.Validation = HMTLHelperExtensions.GetValidationJquery(CamposCatalagos, true, true, Catalogo.CatalogoId);
                auxCatalogoTablas.CatalogoId = Catalogo.CatalogoId;
                string viewContent = ConvertViewToString("_CamposTablaEdit", auxCatalogoTablas);

                return Json(new { Hecho = true, Respuesta = "", Mensaje = Catalogo.Nombre, Partial = viewContent }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion


        //Bitacora
        private void CreateBitacora(Catalogo oldModel, Catalogo newModel, int id = 0)
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

                //NombreTablass
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Nombre de tabla",
                    es_modificado = oldModel != null && oldModel.NombreTabla != newModel.NombreTabla ? true : false,
                    campo_nuevo = newModel.NombreTabla,
                    campo_anterior = oldModel != null ? oldModel.NombreTabla : null
                });

                //Descripcion
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Descripción",
                    es_modificado = oldModel != null && oldModel.Descripcion != newModel.Descripcion ? true : false,
                    campo_nuevo = newModel.Descripcion,
                    campo_anterior = oldModel != null ? oldModel.Descripcion : null
                });

                //Dinamico
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Dinamico",
                    es_modificado = oldModel != null && oldModel.dinamico != newModel.dinamico ? true : false,
                    campo_nuevo = HMTLHelperExtensions.getStringBoolBitacora(newModel.dinamico),
                    campo_anterior = oldModel != null ? HMTLHelperExtensions.getStringBoolBitacora(oldModel.dinamico) : null
                });

                //Tabla
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Tabla",
                    es_modificado = oldModel != null && oldModel.Tabla != newModel.Tabla ? true : false,
                    campo_nuevo = HMTLHelperExtensions.getStringBoolBitacora(newModel.Tabla),
                    campo_anterior = oldModel != null ? HMTLHelperExtensions.getStringBoolBitacora(oldModel.Tabla) : null
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
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "Catalogoes", "Catalogos", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }

        //Bitacora para campoes
        private void CreateBitacora(CampoCatalogo oldModel, CampoCatalogo newModel, int id = 0,string Plantilla="")
        {
            var cambioCampos = new List<cambiosCampos>();
            int accion = oldModel != null ? 2 : 1;
            if (newModel == null)
            {
                accion = 3;
            }
            //Creamos la bitacora
            try
            {
                //Tipo de campo
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Tipo de campo",
                    es_modificado = oldModel != null && newModel != null && oldModel.TipoCampo != newModel.TipoCampo ? true : false,
                    campo_nuevo = newModel != null ? newModel.TipoCampo.GetDisplayName() : null,
                    campo_anterior = oldModel != null ? oldModel.TipoCampo.GetDisplayName() : null
                });

                //Nombre
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Nombre",
                    es_modificado = oldModel != null && newModel != null && oldModel.Nombre != newModel.Nombre ? true : false,
                    campo_nuevo = newModel != null ? newModel.Nombre : null,
                    campo_anterior = oldModel != null ? oldModel.Nombre : null
                });

                //Etiqueta
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Etiqueta",
                    es_modificado = oldModel != null && newModel != null && oldModel.Etiqueta != newModel.Etiqueta ? true : false,
                    campo_nuevo = newModel != null ? newModel.Etiqueta : null,
                    campo_anterior = oldModel != null ? oldModel.Etiqueta : null
                });

                //Longitud
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Longitud",
                    es_modificado = oldModel != null && newModel != null && oldModel.Longitud != newModel.Longitud ? true : false,
                    campo_nuevo = newModel != null ? newModel.Longitud.ToString() : null,
                    campo_anterior = oldModel != null ? oldModel.Longitud.ToString() : null
                });




                //CatalogoId
                string newCatalogo = "";
                string oldCatalogo = "";

                if (newModel != null && newModel.iCatalogoId != 0)
                {
                    newCatalogo = db.Catalogoes.Where(x => x.CatalogoId == newModel.iCatalogoId).FirstOrDefault()?.Nombre;
                }
                if (oldModel != null && oldModel.CatalogoId != 0)
                {
                    oldCatalogo = db.Catalogoes.Where(x => x.CatalogoId == oldModel.iCatalogoId).FirstOrDefault()?.Nombre;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Catálogo",
                    es_modificado = oldModel != null && newModel != null && oldModel.iCatalogoId != newModel.iCatalogoId ? true : false,
                    campo_nuevo = newCatalogo,
                    campo_anterior = oldCatalogo
                });





                //GrupoExtensionId
                string newGrupoExtensionId = "";
                string oldGrupoExtensionId = "";

                if (newModel != null && newModel._GrupoExtensionId != 0)
                {
                    newGrupoExtensionId = db.GrupoExtesiones.Where(x => x.GrupoExtensionId == newModel._GrupoExtensionId).FirstOrDefault()?.Nombre;
                }
                if (oldModel != null && oldModel._GrupoExtensionId != 0)
                {
                    oldGrupoExtensionId = db.GrupoExtesiones.Where(x => x.GrupoExtensionId == oldModel._GrupoExtensionId).FirstOrDefault()?.Nombre;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Grupo de extensión",
                    es_modificado = oldModel != null && newModel != null && oldModel._GrupoExtensionId != newModel._GrupoExtensionId ? true : false,
                    campo_nuevo = newGrupoExtensionId,
                    campo_anterior = oldGrupoExtensionId
                });

                //Tamaño
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Tamaño",
                    es_modificado = oldModel != null && newModel != null && oldModel._Size != newModel?._Size ? true : false,
                    campo_nuevo = newModel != null && newModel._Size.HasValue ? newModel._Size.Value.ToString() : null,
                    campo_anterior = oldModel != null && oldModel._Size.HasValue ? oldModel._Size.Value.ToString() : null
                });

                //Tipo fecha
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Tipo de fecha",
                    es_modificado = oldModel != null && newModel != null && oldModel?._TipoFecha != newModel?._TipoFecha ? true : false,
                    campo_nuevo = newModel != null && newModel?._TipoFecha != null && newModel?._TipoFecha != 0 ? newModel?._TipoFecha.GetDisplayName() : null,
                    campo_anterior = oldModel != null && oldModel?._TipoFecha != null && oldModel?._TipoFecha != 0 ? oldModel?._TipoFecha.GetDisplayName() : null
                });

                //Ayuda
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Ayuda",
                    es_modificado = oldModel != null && newModel != null && oldModel.Ayuda != newModel.Ayuda ? true : false,
                    campo_nuevo = newModel != null ? newModel.Ayuda : null,
                    campo_anterior = oldModel != null ? oldModel.Ayuda : null
                });

                //IdCampoPNT
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "ID de campo PNT",
                    es_modificado = oldModel != null && newModel != null && oldModel.IdCampoPNT != newModel.IdCampoPNT ? true : false,
                    campo_nuevo = newModel != null && newModel.IdCampoPNT.HasValue ? newModel.IdCampoPNT.Value.ToString() : null,
                    campo_anterior = oldModel != null && oldModel.IdCampoPNT.HasValue ? oldModel.IdCampoPNT.Value.ToString() : null
                });

                //idTipoCampoPNT
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "ID de tipo de campo PNT",
                    es_modificado = oldModel != null && newModel != null && oldModel.IdTipoCampoPNT != newModel.IdTipoCampoPNT ? true : false,
                    campo_nuevo = newModel != null && newModel.IdTipoCampoPNT.HasValue ? newModel.IdTipoCampoPNT.Value.ToString() : null,
                    campo_anterior = oldModel != null && oldModel.IdTipoCampoPNT.HasValue ? oldModel.IdTipoCampoPNT.Value.ToString() : null
                });

                //Orden
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Orden",
                    es_modificado = oldModel != null && newModel != null && oldModel.Orden != newModel.Orden ? true : false,
                    campo_nuevo = newModel != null ? newModel.Orden.ToString() : null,
                    campo_anterior = oldModel != null ? oldModel.Orden.ToString() : null
                });
                //Requerido
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Requerido",
                    es_modificado = oldModel != null && newModel != null && oldModel.Requerido != newModel?.Requerido ? true : false,
                    campo_nuevo = newModel != null ? HMTLHelperExtensions.getStringBoolBitacora(newModel.Requerido) : null,
                    campo_anterior = oldModel != null ? HMTLHelperExtensions.getStringBoolBitacora(oldModel.Requerido) : null
                });

                //Conn decimales
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Con decimales",
                    es_modificado = oldModel != null && newModel != null && oldModel._ConDecimales != newModel?._ConDecimales ? true : false,
                    campo_nuevo = newModel != null && newModel._ConDecimales.HasValue ? HMTLHelperExtensions.getStringBoolBitacora(newModel._ConDecimales.Value) : null,
                    campo_anterior = oldModel != null && oldModel._ConDecimales.HasValue ? HMTLHelperExtensions.getStringBoolBitacora(oldModel._ConDecimales.Value) : null
                });

                //Principal
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Principal",
                    es_modificado = oldModel != null && newModel != null && oldModel.Principal != newModel?.Principal ? true : false,
                    campo_nuevo = newModel != null ? HMTLHelperExtensions.getStringBoolBitacora(newModel.Principal) : null,
                    campo_anterior = oldModel != null ? HMTLHelperExtensions.getStringBoolBitacora(oldModel.Principal) : null
                });

                //Activo
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Activo",
                    es_modificado = oldModel != null && newModel != null && oldModel.Activo != newModel?.Activo ? true : false,
                    campo_nuevo = newModel != null ? HMTLHelperExtensions.getStringBoolBitacora(newModel.Activo) : null,
                    campo_anterior = oldModel != null ? HMTLHelperExtensions.getStringBoolBitacora(oldModel.Activo) : null
                });

                //Bitacora
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "CampoCatalogoes", $"Campos del catalago: {Plantilla}", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
