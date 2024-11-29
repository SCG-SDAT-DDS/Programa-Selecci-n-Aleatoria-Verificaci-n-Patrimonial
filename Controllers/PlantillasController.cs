using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Transparencia.FiltersClass;
using Transparencia.Models;
using Dapper;
using System.Text;
using System.Globalization;
using System.IO;
using Transparencia.Helpers;
using ClosedXML.Excel;
using System.Reflection.Emit;
using System.Reflection;
using System.Dynamic;
using static Dapper.SqlMapper;
using ApiTransparencia.sqlHandlers;
using Spire.Xls;
using ExcelDataReader;
using Z.BulkOperations;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Web.WebPages;
using DocumentFormat.OpenXml.Drawing.Charts;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Xml;
using DataTable = System.Data.DataTable;
using Microsoft.AspNet.Identity;
using System.Web.Configuration;
using Org.BouncyCastle.Utilities;
using System.Security.Cryptography;
using Rotativa;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Security.Policy;

namespace Transparencia.Controllers
{
    [Authorize]
    public class PlantillasController : Controller
    {
        SqlConnection conString = new SqlConnection(@"Data Source=192.168.90.156\MSSQLSERVERPCA;Initial Catalog=PROVED;uid=TRANSPARENCIA;password=ccgjt16477;pooling=true;connection lifetime=120;max pool size=50000;");
        List<KeyValuePair<int, KeyValuePair<int, string>>> ListaCatalogos = new List<KeyValuePair<int, KeyValuePair<int, string>>>();
        private ApplicationDbContext db = new ApplicationDbContext();
        private string coneccion = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        private bool onlyChosen = false; 

        private int? GetOrganismoEnlace()
        {
            return HMTLHelperExtensions.GetOrganissmoId(User.Identity.Name, "Enlace");
        }

        //private void combos()
        //{
        //    ViewBag.CmbActivo = new List<SelectListItem>();
        //    ViewBag.CmbPlantillas = new List<SelectListItem>();
        //    ViewBag.CmbOrganismos = new List<SelectListItem>();
        //    ViewBag.CmbFracciones = new List<SelectListItem>();


        //    ViewBag.CmbPlantillas = new SelectList(db.Plantillas.OrderBy(x => x.NombreLargo), "PlantillaId", "NombreLargo");
        //    ViewBag.CmbOrganismos = new SelectList(db.Organismos.OrderBy(x => x.NombreOrganismo), "OrganismoID", "NombreOrganismo");
        //    ViewBag.CmbFracciones = new SelectList(db.Fracciones.OrderBy(x => x.Nombre).Select(x => new { FraccionId = "Fracción: " + x.FraccionId, Nombre = x.Nombre + " con el artículo: " + x.Articulos.Nombre + ", Ley: " + x.Articulos.Leyes.Nombre + " - " + x.Articulos.Leyes.TipoLeyes.Nombre + "." }), "FraccionId", "Nombre");
        //    ViewBag.CmbActivo = new List<SelectListItem> {
        //        new SelectListItem { Text = "Activo", Value = "true" },
        //        new SelectListItem { Text = "Inactivo", Value = "false" }
        //    };
        //}
        public bool? changeStatus(int id = 0)
        {
            var activo = false;
            if (id == 0)
            {
                return null;
            }
            var Model = db.Plantillas.Find(id);
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
            ViewBag.CmbTipoEstatus = new List<SelectListItem>();


            ViewBag.CmbTipoEstatus = new SelectList(db.TipoEstatus.OrderBy(x => x.Nombre), "TipoEstatusId", "Nombre");
            ViewBag.CmbActivo = new List<SelectListItem> {
                new SelectListItem { Text = "Activo", Value = "true" },
                new SelectListItem { Text = "Inactivo", Value = "false" }
            };
            ViewBag.CmbEstado = new List<SelectListItem> {
                new SelectListItem { Text = "Publicado", Value = "true" },
                new SelectListItem { Text = "Diseño", Value = "false" }
            };
        }

        #region Generacion de tabla
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
            inputString = inputString.Replace("(", "");
            inputString = inputString.Replace(")", "");
            inputString = inputString.Replace("/", "");
            inputString = inputString.Replace(@"\", "");
            inputString = inputString.Replace("&", "");
            inputString = inputString.Replace("%", "");
            inputString = inputString.Replace("$", "");
            inputString = inputString.Replace("^", "");
            inputString = inputString.Replace("*", "");
            inputString = inputString.Replace("@", "");

            return inputString;
        }


        private int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }
        #endregion
        // GET: Plantillas
        public ActionResult Index(FiltrosPlantilla filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {

            var UnidadAdministrativaId = HMTLHelperExtensions.GetUnidadId(User.Identity.Name);
            var plantillasUnidadAdministrativa = new List<int>();
            if (UnidadAdministrativaId != null && UnidadAdministrativaId != 0)
            {
                return RedirectToAction("IndexPlantillas", "Plantillas");
            }
            //PerPage = 50;
            combos();
            string[] searchstringsNombreCorto = filtros.NombreCorto != null && filtros.NombreCorto.Length > 0 ? filtros.NombreCorto.Split(' ') : "".Split(' ');
            string[] searchstringsNombreLargo = filtros.NombreLargo != null && filtros.NombreLargo.Length > 0 ? filtros.NombreLargo.Split(' ') : "".Split(' ');
            string[] searchstringsNombreTabla = filtros.NombreTabla != null && filtros.NombreTabla.Length > 0 ? filtros.NombreTabla.Split(' ') : "".Split(' ');
            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombreCorto = sOrder == "NombreCorto" ? "NombreCorto_desc" : "NombreCorto";
            ViewBag.OrderNombreLargo = sOrder == "NombreLargo" ? "NombreLargo_desc" : "NombreLargo";
            ViewBag.OrderNombreTabla = sOrder == "NombreTabla" ? "NombreTabla_desc" : "NombreTabla";
            ViewBag.OrderTipoEstatus = sOrder == "TipoEstatus" ? "TipoEstatus_desc" : "TipoEstatus";
            ViewBag.OrderOrden = sOrder == "Orden" ? "Orden_desc" : "Orden";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";
            IPagedList<AuxPlantillas> vModel = null;

            var OrganismoID = HMTLHelperExtensions.GetOrganissmoId(User.Identity.Name, "Enlace");
            var plantillasOrganissmo = new List<int>();
            if (OrganismoID != null && OrganismoID != 0)
            {
                plantillasOrganissmo = db.PlantillaOrganismos.Where(x => x.OrganismoID == OrganismoID).Select(x => x.PlantillaId).ToList();
            }

            var mPlantillaFraccions = new List<int?>();
            var entroLeyes = false;
            if (filtros.LeyId != 0 || filtros.ArticuloId != 0 || filtros.FraccionId != 0)
            {
                mPlantillaFraccions = db.PlantillaFraccions.Where(x => (filtros.LeyId == 0 || x.Fracciones.Articulos.LeyId == filtros.LeyId)
                && (filtros.ArticuloId == 0 || x.Fracciones.ArticuloId == filtros.ArticuloId)
                && (filtros.FraccionId == 0 || x.FraccionId == filtros.FraccionId)
                ).Select(x => x.PlantillaId).ToList();
                entroLeyes = true;
            }



            SqlConnection sqlConnection = new SqlConnection(coneccion);
            sqlConnection.Open();

            string sQuery = @"
                             SELECT e.*,
                                   stuff((select ', ' + convert(char(4), b.NombrePeriodo) from PeriodoPlantillas a
                                    INNER JOIN Periodoes b ON a.Periodo_PeriodoId = b.PeriodoId
                                    WHERE Plantilla_PlantillaId = e.PlantillaId 
                                    ORDER BY b.Orden ASC for xml path('')),1,1, '') as Ejercicios
                            FROM Plantillas e";

                
            if (filtros.PeriodoId != 0)
            {
                sQuery += $@" WHERE (SELECT COUNT(*) FROM PeriodoPlantillas WHERE Plantilla_PlantillaId = e.PlantillaId AND Periodo_PeriodoId = {filtros.PeriodoId} ) > 0 ";

            }
            sQuery += " ORDER BY e.Orden";
            //var vParameters = new
            //{
            //    Correo = sCorreo,
            //    EmpresaId = iEmpresaId,
            //    Domain = sDomain,
            //    ceros = "00000"
            //};
            vModel = sqlConnection.Query<AuxPlantillas>(sQuery)
                .Where(x => 
               // (filtros.LeyId == 0 || x.LeyId == filtros.LeyId)
               //&& (filtros.ArticuloId == 0 || x.ArticuloId == filtros.ArticuloId)
               //&& (filtros.FraccionId == 0 || x.FraccionId == filtros.FraccionId)
                ((filtros.NombreCorto == null || filtros.NombreCorto.Length == 0) || searchstringsNombreCorto.Any(y => x.NombreCorto.ToLower().Contains(y.ToLower())))
               && ((filtros.NombreLargo == null || filtros.NombreLargo.Length == 0) || searchstringsNombreLargo.Any(y => x.NombreLargo.ToLower().Contains(y.ToLower())))
               && ((filtros.NombreTabla == null || filtros.NombreTabla.Length == 0) || searchstringsNombreTabla.Any(y => x.NombreTabla.ToLower().Contains(y.ToLower())))
               && (filtros.ActivoNull == null || filtros.ActivoNull == x.Activo)
               && (filtros.EstatusNull == null || filtros.EstatusNull == x.Publicado)
               && (filtros.PeriodoDesde == null || x.PeriodoDesde >= filtros.PeriodoDesde)
               && (filtros.PeriodoHasta == null || x.PeriodoHasta <= filtros.PeriodoHasta)
                //&& (filtros.PeriodoId == 0 || x.Periodos.Where(y => y.PeriodoId == filtros.PeriodoId).Count() > 0)
               && ((OrganismoID == null || OrganismoID == 0) || plantillasOrganissmo.Contains(x.PlantillaId))
               && ((OrganismoID == null || OrganismoID == 0) || x.Activo)
               //&& (!entroLeyes || mPlantillaFraccions.Contains(x.PlantillaId))
               ).ToPagedList(iPagina, PerPage);
            sqlConnection.Close();

            //var query = db.Plantillas.Where(x =>
            // ((filtros.NombreCorto == null || filtros.NombreCorto.Length == 0) || searchstringsNombreCorto.Any(y => x.NombreCorto.ToLower().Contains(y.ToLower())))
            // && ((filtros.NombreLargo == null || filtros.NombreLargo.Length == 0) || searchstringsNombreLargo.Any(y => x.NombreLargo.ToLower().Contains(y.ToLower())))
            // && ((filtros.NombreTabla == null || filtros.NombreTabla.Length == 0) || searchstringsNombreTabla.Any(y => x.NombreTabla.ToLower().Contains(y.ToLower())))
            // && (filtros.ActivoNull == null || filtros.ActivoNull == x.Activo)
            // && (filtros.EstatusNull == null || filtros.EstatusNull == x.Publicado)
            // && (filtros.PeriodoDesde == null || x.PeriodoDesde >= filtros.PeriodoDesde)
            // && (filtros.PeriodoHasta == null || x.PeriodoHasta <= filtros.PeriodoHasta)
            // && ((OrganismoID == null || OrganismoID == 0) || plantillasOrganissmo.Contains(x.PlantillaId))
            // && ((OrganismoID == null || OrganismoID == 0) || x.Publicado)
            // && (!entroLeyes || mPlantillaFraccions.Contains(x.PlantillaId))
            // );

            //var query = db.PlantillaFraccions
            //    .Include(x => x.Plantillas).Include(x => x.Fracciones).Include(x => x.Fracciones.Articulos).Include(x => x.Fracciones.Articulos.Leyes)
            //   // .Where(x => (filtros.LeyId == 0 || x.Fracciones.Articulos.LeyId == filtros.LeyId)
            //   //&& (filtros.ArticuloId == 0 || x.Fracciones.ArticuloId == filtros.ArticuloId)
            //   //&& (filtros.FraccionId == 0 || x.FraccionId == filtros.FraccionId)
            //   .Where( x => ((filtros.NombreCorto == null || filtros.NombreCorto.Length == 0) || searchstringsNombreCorto.Any(y => x.Plantillas.NombreCorto.ToLower().Contains(y.ToLower())))
            //   && ((filtros.NombreLargo == null || filtros.NombreLargo.Length == 0) || searchstringsNombreLargo.Any(y => x.Plantillas.NombreLargo.ToLower().Contains(y.ToLower())))
            //   && ((filtros.NombreTabla == null || filtros.NombreTabla.Length == 0) || searchstringsNombreTabla.Any(y => x.Plantillas.NombreTabla.ToLower().Contains(y.ToLower())))
            //   && (filtros.ActivoNull == null || filtros.ActivoNull == x.Plantillas.Activo)
            //   && (filtros.EstatusNull == null || filtros.EstatusNull == x.Plantillas.Publicado)
            //   && (filtros.PeriodoDesde == null || x.Plantillas.PeriodoDesde >= filtros.PeriodoDesde)
            //   && (filtros.PeriodoHasta == null || x.Plantillas.PeriodoHasta <= filtros.PeriodoHasta)
            //   && (filtros.PeriodoId == 0 || x.Plantillas.Periodos.Where(y => y.PeriodoId == filtros.PeriodoId).Count() > 0)
            //   && ((OrganismoID == null || OrganismoID == 0) || plantillasOrganissmo.Contains(x.Plantillas.PlantillaId))
            //   && ((OrganismoID == null || OrganismoID == 0) || x.Plantillas.Publicado)
            //   && (!entroLeyes || mPlantillaFraccions.Contains(x.PlantillaId))
            //   );




            //var query1 = from mPlantillas in db.Plantillas
            //            join mPlantillaFraccions in db.PlantillaFraccions on  mPlantillas.PlantillaId equals mPlantillaFraccions.PlantillaId
            //switch (sOrder)
            //{
            //    case "NombreCorto":
            //        vModel = query.OrderBy(x => x.Plantillas.NombreCorto).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "NombreCorto_desc":
            //        vModel = query.OrderByDescending(x => x.Plantillas.NombreCorto).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "NombreLargo":
            //        vModel = query.OrderBy(x => x.Plantillas.NombreLargo).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "NombreLargo_desc":
            //        vModel = query.OrderByDescending(x => x.Plantillas.NombreLargo).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "NombreTabla":
            //        vModel = query.OrderBy(x => x.Plantillas.NombreTabla).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "NombreTabla_desc":
            //        vModel = query.OrderByDescending(x => x.Plantillas.NombreTabla).ToPagedList(iPagina, PerPage);
            //        break;
            //    //case "TipoEstatus":
            //    //    vModel = query.OrderBy(x => x.TipoEstatus.Nombre).ToPagedList(iPagina, PerPage);
            //    //    break;
            //    //case "TipoEstatus_desc":
            //    //    vModel = query.OrderByDescending(x => x.TipoEstatus.Nombre).ToPagedList(iPagina, PerPage);
            //    //    break;
            //    case "Orden":
            //        vModel = query.OrderBy(x => x.Plantillas.Orden).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "Orden_desc":
            //        vModel = query.OrderByDescending(x => x.Plantillas.Orden).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "Activo":
            //        vModel = query.OrderBy(x => x.Plantillas.Activo).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "Activo_desc":
            //        vModel = query.OrderByDescending(x => x.Plantillas.Activo).ToPagedList(iPagina, PerPage);
            //        break;
            //    default:
            //        vModel = query.OrderBy(x => x.Fracciones.Articulos.Orden).ToPagedList(iPagina, PerPage);
            //        vModel = query.OrderBy(x => x.Fracciones.Orden).ToPagedList(iPagina, PerPage);
            //        break;
            //}
            if (Request.IsAjaxRequest())
            {
                return PartialView("_lista", vModel);
            }
            ViewBag.LeyIds = filtros.LeyId;
            ViewBag.ArticuloIds = filtros.ArticuloId;
            ViewBag.FraccionIds = filtros.FraccionId;
            ViewBag.LeyId = db.Leyes.Where(x => x.Activo).OrderBy(x => x.Orden).ToList();
            if (filtros.LeyId != 0)
                ViewBag.ArticuloId = this.GetArticulosByLeyIdController(filtros.LeyId); //db.Articulos.Where(x => x.LeyId == filtros.LeyId).OrderBy(x => x.Orden).ToList();
            else
                ViewBag.ArticuloId = new List<Articulo>();

            if (filtros.ArticuloId != 0)
                ViewBag.FraccionId = this.GetFraccionByArticulosIdController(filtros.ArticuloId);// db.Fracciones.Where(x => x.ArticuloId == filtros.ArticuloId).OrderBy(x => x.Orden).ToList();
            else
                ViewBag.FraccionId = new List<Articulo>();

            ViewBag.LstPeriodo = db.Periodos.Where(x => x.Activo).OrderBy(x => x.Orden).Select(x => new ListaSeleccion { Text = x.NombrePeriodo, Value = x.PeriodoId }).ToList();

            return View(vModel);
        }

        public ActionResult IndexPlantillas(FiltrosPlantilla filtros, string sOrder = "", int PerPage = 10, int iPagina = 1)
        {
            //PerPage = 50;
            combos();
            string[] searchstringsNombreCorto = filtros.NombreCorto != null && filtros.NombreCorto.Length > 0 ? filtros.NombreCorto.Split(' ') : "".Split(' ');
            string[] searchstringsNombreLargo = filtros.NombreLargo != null && filtros.NombreLargo.Length > 0 ? filtros.NombreLargo.Split(' ') : "".Split(' ');
            string[] searchstringsNombreTabla = filtros.NombreTabla != null && filtros.NombreTabla.Length > 0 ? filtros.NombreTabla.Split(' ') : "".Split(' ');
            ViewBag.Filtros = filtros;
            ViewBag.Order = sOrder;
            ViewBag.PerPage = PerPage;
            ViewBag.Pagina = iPagina;
            ViewBag.OrderNombreCorto = sOrder == "NombreCorto" ? "NombreCorto_desc" : "NombreCorto";
            ViewBag.OrderNombreLargo = sOrder == "NombreLargo" ? "NombreLargo_desc" : "NombreLargo";
            ViewBag.OrderOrden = sOrder == "Orden" ? "Orden_desc" : "Orden";
            ViewBag.OrderActivo = sOrder == "Activo" ? "Activo_desc" : "Activo";
            //IPagedList<PlantillaFraccion> vModel = null;
            IPagedList<AuxPlantillas> vModel = null;

            var UnidadAdministrativaId = HMTLHelperExtensions.GetUnidadId(User.Identity.Name);
            var plantillasUnidadAdministrativa = new List<int>();
            if (UnidadAdministrativaId != null && UnidadAdministrativaId != 0)
            {
                plantillasUnidadAdministrativa = db.PlantillaUnidadAdministrativa.Where(x => x.UnidadAdministrativaId == UnidadAdministrativaId).Select(x => x.PlantillaId).ToList();
            }

            var mPlantillaFraccions = new List<int?>();
            var entroLeyes = false;
            if (filtros.LeyId != 0 || filtros.ArticuloId != 0 || filtros.FraccionId != 0)
            {
                mPlantillaFraccions = db.PlantillaFraccions.Where(x => (filtros.LeyId == 0 || x.Fracciones.Articulos.LeyId == filtros.LeyId)
                && (filtros.ArticuloId == 0 || x.Fracciones.ArticuloId == filtros.ArticuloId)
                && (filtros.FraccionId == 0 || x.FraccionId == filtros.FraccionId)
                ).Select(x => x.PlantillaId).ToList();
                entroLeyes = true;
            }

            //var  query = db.PlantillaFraccions
            //    .Include(x=>x.Plantillas).Include(x => x.Fracciones).Include(x => x.Fracciones.Articulos).Include(x => x.Fracciones.Articulos.Leyes)
            //    .Where(x => (filtros.LeyId == 0 || x.Fracciones.Articulos.LeyId == filtros.LeyId)
            //   && (filtros.ArticuloId == 0 || x.Fracciones.ArticuloId == filtros.ArticuloId)
            //   && (filtros.FraccionId == 0 || x.FraccionId == filtros.FraccionId)
            //   && ((filtros.NombreCorto == null || filtros.NombreCorto.Length == 0) || searchstringsNombreCorto.Any(y => x.Plantillas.NombreCorto.ToLower().Contains(y.ToLower())))
            //   && ((filtros.NombreLargo == null || filtros.NombreLargo.Length == 0) || searchstringsNombreLargo.Any(y => x.Plantillas.NombreLargo.ToLower().Contains(y.ToLower())))
            //   && ((filtros.NombreTabla == null || filtros.NombreTabla.Length == 0) || searchstringsNombreTabla.Any(y => x.Plantillas.NombreTabla.ToLower().Contains(y.ToLower())))
            //   && (filtros.ActivoNull == null || filtros.ActivoNull == x.Plantillas.Activo)
            //   && (filtros.EstatusNull == null || filtros.EstatusNull == x.Plantillas.Publicado)
            //   && (filtros.PeriodoDesde == null || x.Plantillas.PeriodoDesde >= filtros.PeriodoDesde)
            //   && (filtros.PeriodoHasta == null || x.Plantillas.PeriodoHasta <= filtros.PeriodoHasta)
            //   && (filtros.PeriodoId == 0 || x.Plantillas.Periodos.Where(y=>y.PeriodoId == filtros.PeriodoId).Count() > 0)
            //   && ((UnidadAdministrativaId == null || UnidadAdministrativaId == 0) || plantillasUnidadAdministrativa.Contains(x.Plantillas.PlantillaId))
            //   && (!entroLeyes || mPlantillaFraccions.Contains(x.PlantillaId))
            //   );

            //var Plantillas =  from a in db.PlantillaFraccions
            //       join b in db.Fracciones on a.FraccionId equals b.FraccionId
            //       join c in db.Articulos on b.ArticuloId equals c.ArticuloId
            //       join d in db.Leyes on c.ArticuloId equals d.LeyId
            //       //join e in db.Plantillas on a.PlantillaId equals e.PlantillaId
            //       orderby d.Orden,c.Orden,b.Orden 
            //       select new AuxPlantillas { 
            //            LeyNombre = d.Nombre,
            //           ArticuloNombre = c.Nombre,
            //           FracionNombre = d.Nombre

            //       };
            //Empezamos a makinar el sql
            SqlConnection sqlConnection = new SqlConnection(coneccion);
            sqlConnection.Open();

            string sQuery = @"
                            SELECT d.Nombre as LeyNombre,d.LeyId,
                                   c.Nombre as ArticuloNombre, c.ArticuloId,
                                   b.Nombre as FracionNombre, 
                                   e.*,
                                   a.* ,
                                   stuff((select ', ' + convert(char(4), b.NombrePeriodo) from PeriodoPlantillas a
                                    INNER JOIN Periodoes b ON a.Periodo_PeriodoId = b.PeriodoId
                                    WHERE Plantilla_PlantillaId = e.PlantillaId 
                                    ORDER BY b.Orden ASC for xml path('')),1,1, '') as Ejercicios
                            FROM PlantillaFraccions a
                            INNER JOIN Fraccions b ON a.FraccionId = b.FraccionId
                            INNER JOIN Articuloes c ON b.ArticuloId = c.ArticuloId
                            INNER JOIN Leys d ON c.LeyId = d.LeyId
                            INNER JOIN Plantillas e ON a.PlantillaId = e.PlantillaId ";
            //LEFT JOIN PeriodoPlantillas f ON a.PlantillaId = f.Plantilla_PlantillaId";


            if (filtros.PeriodoId != 0)
            {
                sQuery += $@"WHERE (SELECT COUNT(*) FROM PeriodoPlantillas WHERE Plantilla_PlantillaId = a.PlantillaId AND Periodo_PeriodoId = {filtros.PeriodoId} ) > 0 ";

            }
            sQuery += " ORDER BY d.Orden,c.Orden,b.Orden, e.Orden";
            //var vParameters = new
            //{
            //    Correo = sCorreo,
            //    EmpresaId = iEmpresaId,
            //    Domain = sDomain,
            //    ceros = "00000"
            //};
            vModel = sqlConnection.Query<AuxPlantillas>(sQuery)
                .Where(x => (filtros.LeyId == 0 || x.LeyId == filtros.LeyId)
               && (filtros.ArticuloId == 0 || x.ArticuloId == filtros.ArticuloId)
               && (filtros.FraccionId == 0 || x.FraccionId == filtros.FraccionId)
               && ((filtros.NombreCorto == null || filtros.NombreCorto.Length == 0) || searchstringsNombreCorto.Any(y => x.NombreCorto.ToLower().Contains(y.ToLower())))
               && ((filtros.NombreLargo == null || filtros.NombreLargo.Length == 0) || searchstringsNombreLargo.Any(y => x.NombreLargo.ToLower().Contains(y.ToLower())))
               && ((filtros.NombreTabla == null || filtros.NombreTabla.Length == 0) || searchstringsNombreTabla.Any(y => x.NombreTabla.ToLower().Contains(y.ToLower())))
               && (filtros.ActivoNull == null || filtros.ActivoNull == x.Activo)
               && (filtros.EstatusNull == null || filtros.EstatusNull == x.Publicado)
               && (filtros.PeriodoDesde == null || x.PeriodoDesde >= filtros.PeriodoDesde)
               && (filtros.PeriodoHasta == null || x.PeriodoHasta <= filtros.PeriodoHasta)
               && (x.Activo)
                //&& (filtros.PeriodoId == 0 || x.Periodos.Where(y => y.PeriodoId == filtros.PeriodoId).Count() > 0)
                //&& ((UnidadAdministrativaId == null || UnidadAdministrativaId == 0) || plantillasUnidadAdministrativa.Contains(x.PlantillaId))
                && (plantillasUnidadAdministrativa.Contains(x.PlantillaId))
               && (!entroLeyes || mPlantillaFraccions.Contains(x.PlantillaId))).ToPagedList(iPagina, PerPage);
            sqlConnection.Close();

            //var query = db.Plantillas.Where(x =>
            // ((filtros.NombreCorto == null || filtros.NombreCorto.Length == 0) || searchstringsNombreCorto.Any(y => x.NombreCorto.ToLower().Contains(y.ToLower())))
            // && ((filtros.NombreLargo == null || filtros.NombreLargo.Length == 0) || searchstringsNombreLargo.Any(y => x.NombreLargo.ToLower().Contains(y.ToLower())))
            // && ((filtros.NombreTabla == null || filtros.NombreTabla.Length == 0) || searchstringsNombreTabla.Any(y => x.NombreTabla.ToLower().Contains(y.ToLower())))
            // && (filtros.ActivoNull == null || filtros.ActivoNull == x.Activo)
            //  && (filtros.EstatusNull == null || filtros.EstatusNull == x.Publicado)
            //  && ((UnidadAdministrativaId == null || UnidadAdministrativaId == 0) || plantillasUnidadAdministrativa.Contains(x.PlantillaId))
            //  && (!entroLeyes || mPlantillaFraccions.Contains(x.PlantillaId))
            // );

            //switch (sOrder)
            //{
            //    case "NombreCorto":
            //        vModel = query.OrderBy(x => x.Plantillas.NombreCorto).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "NombreCorto_desc":
            //        vModel = query.OrderByDescending(x => x.Plantillas.NombreCorto).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "NombreLargo":
            //        vModel = query.OrderBy(x => x.Plantillas.NombreLargo).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "NombreLargo_desc":
            //        vModel = query.OrderByDescending(x => x.Plantillas.NombreLargo).ToPagedList(iPagina, PerPage);
            //        break;
            //    //case "NombreTabla":
            //    //    vModel = query.OrderBy(x => x.NombreTabla).ToPagedList(iPagina, PerPage);
            //    //    break;
            //    //case "NombreTabla_desc":
            //    //    vModel = query.OrderByDescending(x => x.NombreTabla).ToPagedList(iPagina, PerPage);
            //        // break;
            //    //case "TipoEstatus":
            //    //    vModel = query.OrderBy(x => x.TipoEstatus.Nombre).ToPagedList(iPagina, PerPage);
            //    //    break;
            //    //case "TipoEstatus_desc":
            //    //    vModel = query.OrderByDescending(x => x.TipoEstatus.Nombre).ToPagedList(iPagina, PerPage);
            //    //    break;
            //    case "Orden":
            //        vModel = query.OrderBy(x => x.Plantillas.Orden).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "Orden_desc":
            //        vModel = query.OrderByDescending(x => x.Plantillas.Orden).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "Activo":
            //        vModel = query.OrderBy(x => x.Plantillas.Activo).ToPagedList(iPagina, PerPage);
            //        break;
            //    case "Activo_desc":
            //        vModel = query.OrderByDescending(x => x.Plantillas.Activo).ToPagedList(iPagina, PerPage);
            //        break;
            //    default:
            //        vModel = query.OrderBy(x => x.Fracciones.Articulos.Nombre).ToPagedList(iPagina, PerPage);
            //        break;
            //}
            if (Request.IsAjaxRequest())
            {
                return PartialView("_listaPlantilla", vModel);
            }

            ViewBag.LeyIds = filtros.LeyId;
            ViewBag.ArticuloIds = filtros.ArticuloId;
            ViewBag.FraccionIds = filtros.FraccionId;
            ViewBag.LeyId = db.Leyes.Where(x => x.Activo).OrderBy(x => x.Orden).ToList();
            if (filtros.LeyId != 0)
                ViewBag.ArticuloId = this.GetArticulosByLeyIdController(filtros.LeyId); //db.Articulos.Where(x => x.LeyId == filtros.LeyId).OrderBy(x => x.Orden).ToList();
            else
                ViewBag.ArticuloId = new List<Articulo>();

            if (filtros.ArticuloId != 0)
                ViewBag.FraccionId = this.GetFraccionByArticulosIdController(filtros.ArticuloId);// db.Fracciones.Where(x => x.ArticuloId == filtros.ArticuloId).OrderBy(x => x.Orden).ToList();
            else
                ViewBag.FraccionId = new List<Articulo>();

            ViewBag.LstPeriodo = db.Periodos.Where(x => x.Activo).OrderBy(x => x.Orden).Select(x => new ListaSeleccion { Text = x.NombrePeriodo, Value = x.PeriodoId }).ToList();

            return View(vModel);
        }


        // GET: Plantillas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Plantillas");
            }
            Plantilla plantilla = db.Plantillas.Find(id);
            if (plantilla == null)
            {
                return HttpNotFound();
            }
            //if (plantilla.TipoEstatus == null)
            //    plantilla.TipoEstatus = db.TipoEstatus.Where(x => x.TipoEstatusId == plantilla.TipoEstatusId).FirstOrDefault();
            return View(plantilla);
        }

        // GET: Plantillas/Create
        public ActionResult Create()
        {
            //ViewBag.TipoEstatusId = new SelectList(db.TipoEstatus, "TipoEstatusId", "Nombre");
            ViewBag.FraccionId = new SelectList(db.Fracciones.Where(x => x.Activo).Select(x => new { FraccionId = x.FraccionId, Nombre = x.Nombre + " con el artículo: " + x.Articulos.Nombre + ", Ley: " + x.Articulos.Leyes.Nombre + " - " + x.Articulos.Leyes.TipoLeyes.Nombre + "." }), "FraccionId", "Nombre");
            ViewBag.OrganismoID = new SelectList(db.Organismos.Where(x => x.Activo), "OrganismoID", "NombreOrganismo");

            var AllPeriodos = db.Periodos.Where(x => x.Activo).ToList();
            ViewBag.AllPeriodos = AllPeriodos.Select(o => new SelectListItem
            {
                Text = o.NombrePeriodo,
                Value = o.PeriodoId.ToString()
            });
            //var AllTags = db.GrupoTags.Where(x => x.Activo).ToList();
            //ViewBag.AllTags = AllTags.Select(o => new SelectListItem
            //{
            //    Text = o.Nombre,
            //    Value = o.GrupoTagId.ToString()
            //});
            //ViewBag.LeyId = db.Leyes.Where(x => x.Activo).OrderBy(x => x.Orden).ToList();
            //ViewBag.ArticuloId = new List<Articulo>();
            //ViewBag.FraccionId = new List<Fraccion>();
            //ViewBag.LeyIds = 0;
            //ViewBag.FraccionIds = 0;
            //ViewBag.ArticuloIds = 0;
            //ViewBag.FrecuenciaConservacion = GetFrecuenciaConservacion();
            //ViewBag.FrecuenciaConservacionIds = 0;

            return View();
        }



        // POST: Plantillas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PlantillaViewModel PlantillaViewModel)
        {
            PlantillaViewModel.Plantilla.NombreTabla = GenerarNombreTabla(PlantillaViewModel.Plantilla.NombreCorto);
            //PlantillaViewModel.Plantilla.PlantillaFraccion.FraccionId = 0;
            //PlantillaViewModel.Plantilla.FrecuenciaConservacion = 0;

            if (PlantillaViewModel.Plantilla.Frecuencia == FrecuenciaActualizacion.Ninguno)
            {
                ModelState.AddModelError("Frecuencia", "Seleccione una Frecuencia.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //Periodo
                    var updatedJobPeriodos = new HashSet<int>(PlantillaViewModel.SelectedPeriodo);
                    foreach (Periodo periodos in db.Periodos.Where(x => x.Activo))
                    {
                        if (!updatedJobPeriodos.Contains(periodos.PeriodoId))
                        {
                            PlantillaViewModel.Plantilla.Periodos.Remove(periodos);
                        }
                        else
                        {
                            PlantillaViewModel.Plantilla.Periodos.Add(periodos);
                        }
                    }
                    //GrupoTags
                    //var updatedJobTags = new HashSet<int>(PlantillaViewModel.SelectedTag);
                    //foreach (GrupoTag grupoTag in db.GrupoTags.Where(x => x.Activo))
                    //{
                    //    if (!updatedJobTags.Contains(grupoTag.GrupoTagId))
                    //    {
                    //        PlantillaViewModel.Plantilla.Tags.Remove(grupoTag);
                    //    }
                    //    else
                    //    {
                    //        PlantillaViewModel.Plantilla.Tags.Add(grupoTag);
                    //    }
                    //}

                    var result = CrearTablaFisica(PlantillaViewModel.Plantilla);
                    if (!result.Result)
                    {
                        return ReturErrorCreate(PlantillaViewModel.Plantilla, null, "Ocurrio un error al momento de guardar la tabla fisica");
                    }
                    PlantillaViewModel.Plantilla.Activo = true;
                    db.Plantillas.Add(PlantillaViewModel.Plantilla);
                    db.SaveChanges();

                    //if (PlantillaViewModel.Plantilla.PlantillaFraccion != null)
                    //{
                    //    //plantilla.PlantillaFraccion.PlantillaId = plantilla.PlantillaId;
                    //    var plantillasFraccion = db.PlantillaFraccions.Where(x => x.PlantillaId == PlantillaViewModel.Plantilla.PlantillaId).FirstOrDefault();
                    //    if (plantillasFraccion != null)
                    //    {
                    //        plantillasFraccion.FraccionId = PlantillaViewModel.Plantilla.PlantillaFraccion.FraccionId;
                    //        db.Entry(plantillasFraccion).State = EntityState.Modified;
                    //        db.SaveChanges();
                    //    }
                    //    else
                    //    {
                    //        plantillasFraccion = new PlantillaFraccion();
                    //        plantillasFraccion.FraccionId = PlantillaViewModel.Plantilla.PlantillaFraccion.FraccionId;
                    //        plantillasFraccion.PlantillaId = PlantillaViewModel.Plantilla.PlantillaId;
                    //        db.PlantillaFraccions.Add(plantillasFraccion);
                    //        db.SaveChanges();

                    //    }

                    //}
                    //if (PlantillaViewModel.Plantilla.PlantillaFraccion != null)
                    //{
                    //    var plantillasFraccion = new PlantillaFraccion();
                    //    plantillasFraccion.FraccionId = PlantillaViewModel.Plantilla.PlantillaFraccion.FraccionId;
                    //    plantillasFraccion.PlantillaId = PlantillaViewModel.Plantilla.PlantillaId;
                    //    db.PlantillaFraccions.Add(plantillasFraccion);
                    //    db.SaveChanges();
                    //}
                    return RedirectToAction("ConfigurarCampos", new { id = PlantillaViewModel.Plantilla.PlantillaId });

                }
                catch (Exception ex)
                {
                    return ReturErrorCreate(PlantillaViewModel.Plantilla, null, "Ocurrio un error al momento de guardar " + " - " + ex.Message);
                }



            }
            //ViewBag.TipoEstatusId = new SelectList(db.TipoEstatus, "TipoEstatusId", "Nombre", plantilla.TipoEstatusId);
            ViewBag.LeyId = db.Leyes.Where(x => x.Activo).OrderBy(x => x.Orden).ToList();
            ViewBag.FraccionId = new SelectList(db.Fracciones.Where(x => x.Activo).Select(x => new { FraccionId = x.FraccionId, Nombre = x.Nombre + " con el artículo: " + x.Articulos.Nombre + ", Ley: " + x.Articulos.Leyes.Nombre + " - " + x.Articulos.Leyes.TipoLeyes.Nombre + "." }), "FraccionId", "Nombre");
            ViewBag.OrganismoID = new SelectList(db.Organismos.Where(x => x.Activo), "OrganismoID", "NombreOrganismo");
            ViewBag.FrecuenciaConservacion = GetFrecuenciaConservacion();
            ViewBag.FrecuenciaConservacionIds = 0;
            int LeyIds = 0;
            int FraccionIds = 0;
            int ArticuloIds = 0;

            ViewBag.LeyId = db.Leyes.Where(x => x.Activo).ToList();
            ViewBag.LeyIds = 0;
            ViewBag.FraccionIds = 0;
            ViewBag.ArticuloIds = 0;
            if (PlantillaViewModel.Plantilla != null && PlantillaViewModel.Plantilla.PlantillaFraccion != null)
            {
                FraccionIds = PlantillaViewModel.Plantilla.PlantillaFraccion.FraccionId;
                ViewBag.FraccionIds = FraccionIds;

            }

            if (FraccionIds != 0)
            {
                var Fracciones = db.Fracciones.Where(x => x.FraccionId == FraccionIds && x.Activo).FirstOrDefault();
                ArticuloIds = Fracciones != null ? Fracciones.ArticuloId : 0;
                ViewBag.ArticuloIds = ArticuloIds;
            }

            if (ArticuloIds != 0)
            {
                var Articulos = db.Articulos.Where(x => x.ArticuloId == ArticuloIds && x.Activo).FirstOrDefault();
                LeyIds = Articulos != null ? Articulos.LeyId : 0;
                ViewBag.LeyIds = LeyIds;


            }

            if (LeyIds != 0)
                ViewBag.ArticuloId = db.Articulos.Where(x => x.LeyId == LeyIds && x.Activo).ToList();
            else
                ViewBag.ArticuloId = new List<Articulo>();

            if (ArticuloIds != 0)
                ViewBag.FraccionId = db.Fracciones.Where(x => x.ArticuloId == ArticuloIds && x.Activo).ToList();
            else
                ViewBag.FraccionId = new List<Fraccion>();
            //Periodo
            var AllPeriodos = db.Periodos.Where(x => x.Activo).ToList();
            ViewBag.AllPeriodos = AllPeriodos.Select(o => new SelectListItem
            {
                Text = o.NombrePeriodo,
                Value = o.PeriodoId.ToString()
            });
            //Grupo Tags
            var AllTags = db.GrupoTags.Where(x => x.Activo).ToList();
            ViewBag.AllTags = AllTags.Select(o => new SelectListItem
            {
                Text = o.Nombre,
                Value = o.GrupoTagId.ToString()
            });
            return View(PlantillaViewModel);
        }
        private ActionResult ReturErrorCreate(Plantilla model, List<CampoViewModel> lstCamposTablero, string sMessageError)
        {
            ViewBag.HasRows = this.CheckIfImport(model);
            // model.lstCamposTableroDinamico = lstCamposTablero;
            ViewBag.Error = sMessageError;


            return View(model);
        }

        // GET: Plantillas/Edit/5
        public ActionResult Edit(int? id)
        {

            if (id == null)
            {
                return RedirectToAction("Index", "Plantillas");
            }

            var plantilla = new PlantillaViewModel
            {
                Plantilla = db.Plantillas.Include(i => i.Periodos).First(i => i.PlantillaId == id),
            };
            if (plantilla.Plantilla == null)
            {
                return HttpNotFound();
            }
            //Plantilla plantilla = db.Plantillas.Find(id);
            //if (plantilla == null)
            //{
            //    return HttpNotFound();
            //}
            //plantilla.Plantilla.PlantillaFraccion = db.PlantillaFraccions.Where(x => x.PlantillaId == plantilla.Plantilla.PlantillaId).FirstOrDefault();

            //ViewBag.FraccionId = new SelectList(db.Fracciones.Where(x => x.Activo).Select(x => new { FraccionId = x.FraccionId, Nombre = x.Nombre + " con el artículo: " + x.Articulos.Nombre + ", Ley: " + x.Articulos.Leyes.Nombre + " - " + x.Articulos.Leyes.TipoLeyes.Nombre + "." }), "FraccionId", "Nombre",plantilla.PlantillaFraccion != null ? plantilla.PlantillaFraccion.FraccionId : 0);
            //int LeyIds = 0;
            //int FraccionIds = 0;
            //int ArticuloIds = 0;

            //ViewBag.LeyId = db.Leyes.Where(x => x.Activo).ToList();
            //ViewBag.LeyIds = 0;
            //ViewBag.FraccionIds = 0;
            //ViewBag.ArticuloIds = 0;
            
            //if (plantilla != null && plantilla.Plantilla.PlantillaFraccion != null)
            //{
            //    FraccionIds = plantilla.Plantilla.PlantillaFraccion.FraccionId;
            //    ViewBag.FraccionIds = FraccionIds;
            //}
            //if (FraccionIds != 0)
            //{
            //    var Fracciones = db.Fracciones.Where(x => x.FraccionId == FraccionIds && x.Activo).FirstOrDefault();
            //    ArticuloIds = Fracciones != null ? Fracciones.ArticuloId : 0;
            //    ViewBag.ArticuloIds = ArticuloIds;
            //}

            //if (ArticuloIds != 0)
            //{
            //    var Articulos = db.Articulos.Where(x => x.ArticuloId == ArticuloIds && x.Activo).FirstOrDefault();
            //    LeyIds = Articulos != null ? Articulos.LeyId : 0;
            //    ViewBag.LeyIds = LeyIds;


            //}

            //if (LeyIds != 0)
            //    ViewBag.ArticuloId = db.Articulos.Where(x => x.LeyId == LeyIds && x.Activo).ToList();
            //else
            //    ViewBag.ArticuloId = new List<Articulo>();

            //if (ArticuloIds != 0)
            //    ViewBag.FraccionId = db.Fracciones.Where(x => x.ArticuloId == ArticuloIds && x.Activo).ToList();
            //else
            //    ViewBag.FraccionId = new List<Fraccion>();

            //Periodos
            var AllPeriodos = db.Periodos.Where(x => x.Activo).ToList();
            ViewBag.AllPeriodos = AllPeriodos.Select(o => new SelectListItem
            {
                Text = o.NombrePeriodo,
                Value = o.PeriodoId.ToString()
            });

            //Grupo Tags
            //var AllTags = db.GrupoTags.Where(x => x.Activo).ToList();
            //ViewBag.AllTags = AllTags.Select(o => new SelectListItem
            //{
            //    Text = o.Nombre,
            //    Value = o.GrupoTagId.ToString()
            //});

            //ViewBag.FrecuenciaConservacion = GetFrecuenciaConservacion();
            //ViewBag.FrecuenciaConservacionId = plantilla.Plantilla.FrecuenciaConservacion;

            //ViewBag.LeyId = db.Leyes.Where(x => x.Activo).ToList();
            //ViewBag.ArticuloId = new List<Articulo>();

            //ViewBag.FraccionId = new List<Fraccion>();

            //ViewBag.OrganismoID = new SelectList(db.Organismos.Where(x => x.Activo), "OrganismoID", "NombreOrganismo",plantilla.PlantillaFraccion != null ? plantilla.PlantillaFraccion.OrganismoID : 0);
            //ViewBag.TipoEstatusId = new SelectList(db.TipoEstatus, "TipoEstatusId", "Nombre", plantilla.TipoEstatusId);
            return View(plantilla);
        }

        public ActionResult GetArticulosByLeyId(int iId = 0)
        {

            var OrganismoID = HMTLHelperExtensions.GetOrganissmoId(User.Identity.Name, "Enlace");
            var plantillasOrganissmo = new List<int>();
            if (OrganismoID != null && OrganismoID != 0)
            {
                plantillasOrganissmo = db.PlantillaOrganismos.Where(x => x.OrganismoID == OrganismoID).Select(x => x.PlantillaId).ToList();
            }
            var UnidadAdministrativaId = HMTLHelperExtensions.GetUnidadId(User.Identity.Name);
            var plantillasUnidadAdministrativa = new List<int>();
            if (UnidadAdministrativaId != null && UnidadAdministrativaId != 0)
            {
                plantillasUnidadAdministrativa = db.PlantillaUnidadAdministrativa.Where(x => x.UnidadAdministrativaId == UnidadAdministrativaId).Select(x => x.PlantillaId).ToList();
            }
            var Articulos = db.PlantillaFraccions.Where(x => x.Fracciones.Articulos.LeyId == iId && x.Fracciones.Articulos.Leyes.Activo
            && (OrganismoID == null || plantillasOrganissmo.Contains((int)x.PlantillaId))
            && (UnidadAdministrativaId == null || plantillasUnidadAdministrativa.Contains((int)x.PlantillaId))
            ).GroupBy(x => x.Fracciones.Articulos).Select(x => x.Key).OrderBy(x => x.Orden).ToList();
            //svar Articulos = db.Articulos.Where(x => x.LeyId == iId && x.Activo).OrderBy(x => x.Orden).ToList();
            return Json(new { data = Articulos, Encontro = Articulos != null }, JsonRequestBehavior.AllowGet);


        }
        public List<Articulo> GetArticulosByLeyIdController(int iId = 0)
        {
            var OrganismoID = HMTLHelperExtensions.GetOrganissmoId(User.Identity.Name, "Enlace");
            var plantillasOrganissmo = new List<int>();
            if (OrganismoID != null && OrganismoID != 0)
            {
                plantillasOrganissmo = db.PlantillaOrganismos.Where(x => x.OrganismoID == OrganismoID).Select(x => x.PlantillaId).ToList();
            }
            var UnidadAdministrativaId = HMTLHelperExtensions.GetUnidadId(User.Identity.Name);
            var plantillasUnidadAdministrativa = new List<int>();
            if (UnidadAdministrativaId != null && UnidadAdministrativaId != 0)
            {
                plantillasUnidadAdministrativa = db.PlantillaUnidadAdministrativa.Where(x => x.UnidadAdministrativaId == UnidadAdministrativaId).Select(x => x.PlantillaId).ToList();
            }
            var Articulos = db.PlantillaFraccions.Where(x => x.Fracciones.Articulos.LeyId == iId && x.Fracciones.Articulos.Leyes.Activo
            && (OrganismoID == null || plantillasOrganissmo.Contains((int)x.PlantillaId))
            && (UnidadAdministrativaId == null || plantillasUnidadAdministrativa.Contains((int)x.PlantillaId))
            ).GroupBy(x => x.Fracciones.Articulos).Select(x => x.Key).OrderBy(x => x.Orden).ToList();
            //svar Articulos = db.Articulos.Where(x => x.LeyId == iId && x.Activo).OrderBy(x => x.Orden).ToList();
            return Articulos;


        }
        public ActionResult GetFraccionByArticulosId(int iId = 0)
        {
            var OrganismoID = HMTLHelperExtensions.GetOrganissmoId(User.Identity.Name, "Enlace");
            var plantillasOrganissmo = new List<int>();
            if (OrganismoID != null && OrganismoID != 0)
            {
                plantillasOrganissmo = db.PlantillaOrganismos.Where(x => x.OrganismoID == OrganismoID).Select(x => x.PlantillaId).ToList();
            }
            var UnidadAdministrativaId = HMTLHelperExtensions.GetUnidadId(User.Identity.Name);
            var plantillasUnidadAdministrativa = new List<int>();
            if (UnidadAdministrativaId != null && UnidadAdministrativaId != 0)
            {
                plantillasUnidadAdministrativa = db.PlantillaUnidadAdministrativa.Where(x => x.UnidadAdministrativaId == UnidadAdministrativaId).Select(x => x.PlantillaId).ToList();
            }
            if (OrganismoID == null && UnidadAdministrativaId == null)
            {
                var Fracciones_ = db.Fracciones.Where(x => x.Activo && x.ArticuloId == iId).OrderBy(x => x.Orden).ToList();
                //var Fracciones = db.Fracciones.Where(x => x.ArticuloId == iId && x.Activo).OrderBy(x => x.Orden).ToList();
                return Json(new { data = Fracciones_, Encontro = Fracciones_ != null }, JsonRequestBehavior.AllowGet);
            }

            var Fracciones = db.PlantillaFraccions.Where(x => x.Fracciones.ArticuloId == iId && x.Fracciones.Articulos.Activo
             && (OrganismoID == null || plantillasOrganissmo.Contains((int)x.PlantillaId))
             && (UnidadAdministrativaId == null || plantillasUnidadAdministrativa.Contains((int)x.PlantillaId))
             ).GroupBy(x => x.Fracciones).Select(x => x.Key).OrderBy(x => x.Orden).ToList();
            //var Fracciones = db.Fracciones.Where(x => x.ArticuloId == iId && x.Activo).OrderBy(x => x.Orden).ToList();
            return Json(new { data = Fracciones, Encontro = Fracciones != null }, JsonRequestBehavior.AllowGet);

        }
        public List<Fraccion> GetFraccionByArticulosIdController(int iId = 0)
        {
            var OrganismoID = HMTLHelperExtensions.GetOrganissmoId(User.Identity.Name, "Enlace");
            var plantillasOrganissmo = new List<int>();
            if (OrganismoID != null && OrganismoID != 0)
            {
                plantillasOrganissmo = db.PlantillaOrganismos.Where(x => x.OrganismoID == OrganismoID).Select(x => x.PlantillaId).ToList();
            }
            var UnidadAdministrativaId = HMTLHelperExtensions.GetUnidadId(User.Identity.Name);
            var plantillasUnidadAdministrativa = new List<int>();
            if (UnidadAdministrativaId != null && UnidadAdministrativaId != 0)
            {
                plantillasUnidadAdministrativa = db.PlantillaUnidadAdministrativa.Where(x => x.UnidadAdministrativaId == UnidadAdministrativaId).Select(x => x.PlantillaId).ToList();
            }
            var Fracciones = db.PlantillaFraccions.Where(x => x.Fracciones.ArticuloId == iId && x.Fracciones.Articulos.Activo
             && (OrganismoID == null || plantillasOrganissmo.Contains((int)x.PlantillaId))
             && (UnidadAdministrativaId == null || plantillasUnidadAdministrativa.Contains((int)x.PlantillaId))
             ).GroupBy(x => x.Fracciones).Select(x => x.Key).OrderBy(x => x.Orden).ToList();
            //var Fracciones = db.Fracciones.Where(x => x.ArticuloId == iId && x.Activo).OrderBy(x => x.Orden).ToList();
            return Fracciones;

        }

        // POST: Plantillas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PlantillaViewModel PlantillaViewModel)
        {
            //PlantillaViewModel.Plantilla.FrecuenciaConservacion = 0;
            if (ModelState.IsValid)
            {
                //var mPlantilla = db.Plantillas
                //    .Include(i => i.Periodos).First(i => i.PlantillaId == PlantillaViewModel.Plantilla.PlantillaId);
                //if (mPlantilla != null)
                //{
                var modelPlantilla = db.Plantillas.Include(i => i.Periodos).First(i => i.PlantillaId == PlantillaViewModel.Plantilla.PlantillaId);
                if (TryUpdateModel(modelPlantilla, "Plantilla", new string[] { "IdPlantillaPNT", "NombreCorto", "NombreLargo", "Ayuda", "Orden", "Publicado", "IsPreved", "Activo", "Frecuencia", "Porcentage" }))
                {
                    //GrupoTags
                    //var newGrupoTags = db.GrupoTags.Where(m => PlantillaViewModel.SelectedTag.Contains(m.GrupoTagId)).ToList();
                    //var updatedGrupoTag = new HashSet<int>(PlantillaViewModel.SelectedTag);

                    //foreach (GrupoTag grupoTag in db.GrupoTags)
                    //{
                    //    if (!updatedGrupoTag.Contains(grupoTag.GrupoTagId))
                    //    {
                    //        modelPlantilla.Tags.Remove(grupoTag);
                    //    }
                    //    else
                    //    {
                    //        modelPlantilla.Tags.Add(grupoTag);
                    //    }
                    //}
                    //Periodo
                    var newPeriodos = db.Periodos.Where(m => PlantillaViewModel.SelectedPeriodo.Contains(m.PeriodoId) && m.Activo).ToList();
                    var updatedPeriodo = new HashSet<int>(PlantillaViewModel.SelectedPeriodo);

                    foreach (Periodo periodo in db.Periodos)
                    {
                        if (!updatedPeriodo.Contains(periodo.PeriodoId))
                        {
                            modelPlantilla.Periodos.Remove(periodo);
                        }
                        else
                        {
                            modelPlantilla.Periodos.Add(periodo);
                        }
                    }
                    //modelPlantilla.Porcentage = Convert.ToDecimal(0.005);

                    db.Entry(modelPlantilla).State = EntityState.Modified;
                    db.SaveChanges();
                }

                //}


                //if (PlantillaViewModel.Plantilla.PlantillaFraccion != null)
                //{
                //    //plantilla.PlantillaFraccion.PlantillaId = plantilla.PlantillaId;
                //    var plantillasFraccion = db.PlantillaFraccions.Where(x => x.PlantillaId == PlantillaViewModel.Plantilla.PlantillaId).FirstOrDefault();
                //    if (plantillasFraccion != null)
                //    {
                //        plantillasFraccion.FraccionId = PlantillaViewModel.Plantilla.PlantillaFraccion.FraccionId;
                //        db.Entry(plantillasFraccion).State = EntityState.Modified;
                //        db.SaveChanges();
                //    }
                //    else
                //    {
                //        plantillasFraccion = new PlantillaFraccion();
                //        plantillasFraccion.FraccionId = PlantillaViewModel.Plantilla.PlantillaFraccion.FraccionId;
                //        plantillasFraccion.PlantillaId = PlantillaViewModel.Plantilla.PlantillaId;
                //        db.PlantillaFraccions.Add(plantillasFraccion);
                //        db.SaveChanges();

                //    }

                //}

                return RedirectToAction("Index");
            }
            //ViewBag.FraccionId = new SelectList(db.Fracciones.Where(x => x.Activo).Select(x => new { FraccionId = x.FraccionId, Nombre = x.Nombre + " con el artículo: " + x.Articulos.Nombre + ", Ley: " + x.Articulos.Leyes.Nombre + " - " + x.Articulos.Leyes.TipoLeyes.Nombre + "." }), "FraccionId", "Nombre");
            int LeyIds = 0;
            int FraccionIds = 0;
            int ArticuloIds = 0;

            ViewBag.LeyId = db.Leyes.Where(x => x.Activo).ToList();
            ViewBag.LeyIds = 0;
            ViewBag.FraccionIds = 0;
            ViewBag.ArticuloIds = 0;
            if (PlantillaViewModel.Plantilla != null && PlantillaViewModel.Plantilla.PlantillaFraccion != null)
            {
                FraccionIds = PlantillaViewModel.Plantilla.PlantillaFraccion.FraccionId;
                ViewBag.FraccionIds = FraccionIds;

            }

            if (FraccionIds != 0)
            {
                var Fracciones = db.Fracciones.Where(x => x.FraccionId == FraccionIds && x.Activo).FirstOrDefault();
                ArticuloIds = Fracciones != null ? Fracciones.ArticuloId : 0;
                ViewBag.ArticuloIds = ArticuloIds;
            }

            if (ArticuloIds != 0)
            {
                var Articulos = db.Articulos.Where(x => x.ArticuloId == ArticuloIds && x.Activo).FirstOrDefault();
                LeyIds = Articulos != null ? Articulos.LeyId : 0;
                ViewBag.LeyIds = LeyIds;


            }

            if (LeyIds != 0)
                ViewBag.ArticuloId = db.Articulos.Where(x => x.LeyId == LeyIds && x.Activo).ToList();
            else
                ViewBag.ArticuloId = new List<Articulo>();

            if (ArticuloIds != 0)
                ViewBag.FraccionId = db.Fracciones.Where(x => x.ArticuloId == ArticuloIds && x.Activo).ToList();
            else
                ViewBag.FraccionId = new List<Fraccion>();

            //Periodo
            var AllPeriodos = db.Periodos.Where(x => x.Activo).ToList();
            ViewBag.AllPeriodos = AllPeriodos.Select(o => new SelectListItem
            {
                Text = o.NombrePeriodo,
                Value = o.PeriodoId.ToString()
            });
            //Grupo Tags
            var AllTags = db.GrupoTags.Where(x => x.Activo).ToList();
            ViewBag.AllTags = AllTags.Select(o => new SelectListItem
            {
                Text = o.Nombre,
                Value = o.GrupoTagId.ToString()
            });
            ViewBag.FrecuenciaConservacion = GetFrecuenciaConservacion();
            ViewBag.FrecuenciaConservacionId = PlantillaViewModel.Plantilla.FrecuenciaConservacion;
            //ViewBag.OrganismoID = new SelectList(db.Organismos.Where(x => x.Activo), "OrganismoID", "NombreOrganismo");
            //ViewBag.TipoEstatusId = new SelectList(db.TipoEstatus, "TipoEstatusId", "Nombre", plantilla.TipoEstatusId);
            return View(PlantillaViewModel);
        }

        #region Asignacion a Unidades Administrativas
        public ActionResult AsignacionUnidadAdministrativa(int? id = 0)
        {
            var OrganismoId = GetOrganismoEnlace();
            if (id == 0 && OrganismoId == 0)
            {
                return RedirectToAction("Index", "Plantillas");
            }
            List<DependenciasVModel> vDependencias = db.UnidadesAdministrativas.Where(x => x.Activo && x.OrganismoId == OrganismoId).Select(s => new DependenciasVModel
            {
                Id = s.UnidadAdministrativaId,
                Nombre = s.NombreUnidad
            }).ToList();

            ViewBag.lstUnidadAdministrativaPlantillas = db.PlantillaUnidadAdministrativa.Where(x => x.PlantillaId == id).ToList();
            ViewBag.PlantillaId = id;
            ViewBag.OrganismoId = OrganismoId;
            var plantilla = db.Plantillas.Where(x => x.PlantillaId == id).FirstOrDefault();
            ViewBag.PlantillaNombre = plantilla != null ? plantilla.NombreLargo : "";
            return View("AsignarUnidadAdministrativa", vDependencias);
        }


        //cuano modifiquen alggo se les quitara todas las plantillas que no esten relacionadas porque borra todoas las que no coinciden con la lista.
        [HttpPost]
        public ActionResult AsignacionUnidadAdministrativa(List<int> lstUnidadesAdministrativas, int? idPlantilla = 0)
        {
            var OrganismoId = GetOrganismoEnlace();
            lstUnidadesAdministrativas = lstUnidadesAdministrativas ?? new List<int>();
            DateTime dtNow = DateTime.Now;
            if (lstUnidadesAdministrativas.Any() && idPlantilla != 0)
            {

                foreach (var item in lstUnidadesAdministrativas)
                {
                    var exite = db.PlantillaUnidadAdministrativa.Where(x => x.PlantillaId == idPlantilla && x.UnidadAdministrativaId == item).FirstOrDefault();
                    if (exite == null)
                    {
                        try
                        {
                            var vModel = new PlantillaUnidadAdministrativa();
                            vModel.PlantillaId = Convert.ToInt32(idPlantilla);
                            vModel.UnidadAdministrativaId = item;
                            db.PlantillaUnidadAdministrativa.Add(vModel);
                            db.SaveChanges();
                        }
                        catch
                        {

                        }
                    }


                }
            }
            var idEliminarPlantillaUnidadAdministrativa = db.PlantillaUnidadAdministrativa.Where(x => x.PlantillaId == idPlantilla && !lstUnidadesAdministrativas.Contains(x.UnidadAdministrativaId)).ToList();
            if (idEliminarPlantillaUnidadAdministrativa.Any())
            {
                db.PlantillaUnidadAdministrativa.RemoveRange(idEliminarPlantillaUnidadAdministrativa);
                db.SaveChanges();
            }
            TempData["Mensaje"] = "Se guardo exitosamente";
            return RedirectToAction("AsignacionUnidadAdministrativa", new { id = idPlantilla });
        }
        #endregion

        public ActionResult Asignacion(int? id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Index", "Plantillas");
            }
            List<DependenciasVModel> vDependencias = db.Organismos.Where(x => x.Activo).Select(s => new DependenciasVModel
            {
                Id = s.OrganismoID,
                Nombre = s.NombreOrganismo
            }).ToList();

            var plantillaDependencia = db.PlantillaOrganismos.Where(x => x.PlantillaId == id).ToList();
            ViewBag.lstDependenciasPlantillas = plantillaDependencia;
            ViewBag.PlantillaId = id;
            var plantilla = db.Plantillas.Where(x => x.PlantillaId == id).FirstOrDefault();
            ViewBag.PlantillaNombre = plantilla != null ? plantilla.NombreLargo : "";
            return View("AsignarDependencias", vDependencias);
        }

        [HttpPost]
        public ActionResult Asignacion(List<int> lstDependencias, int? idPlantilla = 0)
        {
            lstDependencias = lstDependencias ?? new List<int>();
            DateTime dtNow = DateTime.Now;
            if (lstDependencias.Any() && idPlantilla != 0)
            {

                foreach (var item in lstDependencias)
                {
                    var exite = db.PlantillaOrganismos.Where(x => x.PlantillaId == idPlantilla && x.OrganismoID == item).FirstOrDefault();
                    if (exite == null)
                    {
                        try
                        {
                            var vModel = new PlantillaOrganismos();
                            vModel.PlantillaId = Convert.ToInt32(idPlantilla);
                            vModel.OrganismoID = item;
                            db.PlantillaOrganismos.Add(vModel);
                            db.SaveChanges();
                        }
                        catch
                        {

                        }
                    }


                }
            }
            var idEliminarPlantillaOrganismo = db.PlantillaOrganismos.Where(x => x.PlantillaId == idPlantilla && !lstDependencias.Contains(x.OrganismoID)).ToList();
            if (idEliminarPlantillaOrganismo.Any())
            {
                db.PlantillaOrganismos.RemoveRange(idEliminarPlantillaOrganismo);
                db.SaveChanges();
            }

            /*var vEliminarProyectos = ProyectosB.GetList(filtros, lstClavesEscuelas);

            if (vEliminarProyectos != null && vEliminarProyectos.Any())
            {
                foreach (var item in vEliminarProyectos)
                {
                    item.Activo = false;
                    ProyectosB.Update(item);
                    string sDescripcionBitacora = $"Se desasigno el proyecto educativo {item.tblProtectoEducativo.ProyectoEducativo} al centro de trabajo {item.ClaveEscuela}, ciclo escolar: {item.tblCicloEscolar.CicloEscolar}";
                    HMTLHelperExtensions.InsertBitacora(sDescripcionBitacora, dtNow, iLoggedUserId, iCatalogoBitacoraId, item.ProyectoId);
                }
            }
            */
            TempData["Mensaje"] = "Se guardo exitosamente";
            return RedirectToAction("Asignacion", new { id = idPlantilla });
        }

        // GET: Plantillas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Plantilla plantilla = db.Plantillas.Find(id);
            if (plantilla == null)
            {
                return HttpNotFound();
            }
            //if (plantilla.TipoEstatus == null)
            //    plantilla.TipoEstatus = db.TipoEstatus.Where(x => x.TipoEstatusId == plantilla.TipoEstatusId).FirstOrDefault();
            return View(plantilla);
        }

        // POST: Plantillas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Plantilla plantilla = db.Plantillas.Find(id);
            db.Plantillas.Remove(plantilla);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        #region Pruebas para agregar Campos PNT
        public ActionResult PNTCaptura(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Plantillas");
            }
            this.VerifyNecessaryColumn((int)id);
            //var campo = new CampoViewModel();
            ViewBag.Plantilla_name = db.Plantillas.Where(x => x.PlantillaId == id).FirstOrDefault().NombreCorto;

            var campos = db.Campos.Where(x => x.PlantillaId == id && x.Activo).OrderBy(x => x.Orden).ToList();// generarViewModel();
            ViewData["error"] = TempData["error"];
            ViewBag.PlantillaId = id;
            //ViewBag.GrupoExtension = db.GrupoExtesiones.Select(x => new ListaSeleccion { Text = x.Nombre, Value = x.GrupoExtensionId }).ToList();
            //ViewBag.listaCatalogos = db.Catalogoes.Select(x => new ListaSeleccion { Text = x.Nombre, Value = x.CatalogoId }).ToList();

            return View(campos);
        }

        [HttpPost]
        public ActionResult PNTAgregarCaptura(int idPlantillaPNT, List<PNTListaCampos> PNTCampos)
        {
            if (idPlantillaPNT == 0 && !PNTCampos.Any())
            {
                return Json(new { Hecho = false, Mensaje = "Al menos debe de tener un valor en los datos." }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                if (PNTCampos.Any())
                {
                    foreach (var item in PNTCampos)
                    {
                        using (var db = new ApplicationDbContext())
                        {
                            var campo = new Campo() { CampoId = item.CampoId, IdCampoPNT = item.IdCampoPNT, IdTipoCampoPNT = item.IdTipoCampoPNT };
                            db.Campos.Attach(campo);
                            db.Entry(item).Property(x => x.IdCampoPNT).IsModified = true;
                            db.Entry(item).Property(x => x.IdTipoCampoPNT).IsModified = true;
                            db.SaveChanges();
                        }
                    }
                }
                return Json(new { Hecho = true, Mensaje = "se guardo exitosamente" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion




        public ActionResult ConfigurarCampos(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Plantillas");
            }
            this.VerifyNecessaryColumn((int)id);
            //var campo = new CampoViewModel();
            ViewBag.Plantilla_name = db.Plantillas.Where(x => x.PlantillaId == id).FirstOrDefault().NombreCorto;

            var campos = db.Campos.Where(x => x.PlantillaId == id).OrderBy(x => x.Orden).ToList();// generarViewModel();
            ViewData["error"] = TempData["error"];
            ViewBag.PlantillaId = id;
            ViewBag.GrupoExtension = db.GrupoExtesiones.Select(x => new ListaSeleccion { Text = x.Nombre, Value = x.GrupoExtensionId }).ToList();
            ViewBag.listaCatalogos = db.Catalogoes.Select(x => new ListaSeleccion { Text = x.Nombre, Value = x.CatalogoId }).ToList();

            return View(campos);
        }

        public List<CampoViewModel> generarViewModel(List<Campo> campos)
        {
            List<CampoViewModel> lstCampoViewModel = new List<CampoViewModel>();

            try
            {

                if (campos.Count > 0)
                {
                    foreach (var item in campos)
                    {
                        CampoViewModel CampoViewModel = new CampoViewModel();
                        CampoViewModel.Activo = item.Activo;
                        CampoViewModel.Ayuda = item.Ayuda;
                        CampoViewModel.CampoId = item.CampoId;
                        CampoViewModel.CatalogoId = item.CatalogoId;
                        //var conDecimales = item.TipoCampo == TipoCampo.Decimal && item.Porcentaje != null ? item.Porcentaje.ConDecimales : false;

                        //CampoViewModel.ConDecimales = conDecimales;
                        CampoViewModel.Etiqueta = item.Etiqueta;
                        //var GrupoExtensionId = item.TipoCampo == TipoCampo.ArchivoAdjunto && item.ArchivoAdjunto != null ? item.ArchivoAdjunto.GrupoExtension.GrupoExtensionId : 0;
                        //CampoViewModel.GrupoExtensionId = GrupoExtensionId;
                        CampoViewModel.Longitud = item.Longitud;
                        CampoViewModel.Nombre = item.Nombre;
                        CampoViewModel.PlantillaId = item.PlantillaId;
                        CampoViewModel.Requerido = item.Requerido;
                        // var Size = item.TipoCampo == TipoCampo.ArchivoAdjunto && item.ArchivoAdjunto != null ? item.ArchivoAdjunto.Size : 0;
                        //CampoViewModel.Size = Size;
                        CampoViewModel.TipoCampo = item.TipoCampo;
                        //var TipoFecha = item.TipoCampo == TipoCampo.Fecha && item.Fecha != null ? item.Fecha.TipoFecha : 0;
                        //CampoViewModel.TipoFecha = TipoFecha;
                        CampoViewModel.Orden = item.Orden;
                        CampoViewModel._ConDecimales = item._ConDecimales;
                        CampoViewModel._GrupoExtensionId = item._GrupoExtensionId;
                        CampoViewModel._Size = item._Size;
                        CampoViewModel._TipoFecha = item._TipoFecha;
                        lstCampoViewModel.Add(CampoViewModel);

                    }
                }
            }
            catch (Exception ex)
            {

            }

            return lstCampoViewModel;

        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
        public bool PublicarOcultar(int id)
        {
            try
            {
                Plantilla plantilla = db.Plantillas.Find(id);
                plantilla.Publicado = !plantilla.Publicado;
                db.Entry(plantilla).State = EntityState.Modified;
                db.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public ActionResult Inactivar(int id)
        {
            try
            {
                Plantilla plantilla = db.Plantillas.Find(id);
                plantilla.Activo = !plantilla.Activo;
                plantilla.Publicado = false;
                db.Entry(plantilla).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { Activo = plantilla.Activo, Encontro = true }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Activo = false, Encontro = false }, JsonRequestBehavior.AllowGet);
            }
        }

        //Tabla dinamica
        public ResultGuardarTablaFisica CrearTablaFisica(Plantilla model)
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
                    [OrganismoCapturoID]  [int] NOT NULL DEFAULT '',
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

        public ResultGuardarTablaFisica CrearTablaFisicaHistory(Plantilla model)
        {
            var tabla = $"{model.NombreTabla}_History";
            string sDropTableScript = $@"IF OBJECT_ID('{tabla}', 'U') IS NOT NULL 
                                            DROP TABLE {tabla}_History; ";

            string sHistoryTablaScript = $@"CREATE TABLE {tabla}(
	                [TablaFisicaId] [int] IDENTITY(1,1) NOT NULL,
                    [OrganismoID] [int] NOT NULL,
                    [UsuarioId] [nvarchar](128) NOT NULL,
                    [Activo] BIT DEFAULT 1,
                    [FechaCreacion] DATE DEFAULT '',
                    [FechaBaja] DATE DEFAULT '',
                    [OrganismoCapturoID]  [int] NOT NULL DEFAULT 0,
                    [PlantillaHistoryId] [int] NOT NULL,
                 CONSTRAINT [PK_dbo.{tabla}] PRIMARY KEY CLUSTERED 
                (
	                [TablaFisicaId] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY];";

            ResultGuardarTablaFisica bResult = new ResultGuardarTablaFisica
            {
                Result = true
            };
            try
            {

                db.Database.ExecuteSqlCommand(sDropTableScript + sHistoryTablaScript);
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

        public bool CheckIfImport(Plantilla model)
        {
            string sQuery = $@"IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{model.NombreTabla}')
	                            SELECT cast(1 as bit)
                            ELSE
	                            SELECT cast(0 as bit)";


            var bResult = db.Database.SqlQuery<bool>(sQuery).FirstOrDefault();

            if (!bResult)
                return bResult;
            sQuery = $@"IF EXISTS(SELECT TOP 1 * FROM {model.NombreTabla})
	                            SELECT cast(1 as bit)
                            ELSE
	                            SELECT cast(0 as bit)";
            bResult = db.Database.SqlQuery<bool>(sQuery).FirstOrDefault();

            return bResult;
        }

        [HttpPost]
        public ActionResult AgregarCampo(LstCampos tblCampoDinamico, List<CampoViewModel> lstCamposTabla)
        {
            lstCamposTabla = lstCamposTabla ?? new List<CampoViewModel>();
            CampoViewModel campos = new CampoViewModel();

            //if (tblCampoDinamico.TipoCampo != TiposCampoTablero.Fecha && tblCampoDinamico.EsFiltro && tblCampoDinamico.TipoFiltro == TiposFiltroCampo.RangoFechas)
            //{
            //    ViewBag.Error = "Solamente puede aplicar el tipo de filtro de rango de fechas a los campos de tipo fecha";
            //}
            //else 
            if (lstCamposTabla.Any(r => r.Nombre == tblCampoDinamico.AddNombre))
            {
                ViewBag.Error = "Ya existe un campo con el mismo nombre.";
            }
            else
            {
                campos.Activo = true;
                campos.Ayuda = tblCampoDinamico.AddAyuda;
                campos.CatalogoId = tblCampoDinamico.AddCatalogoId;
                campos.ConDecimales = tblCampoDinamico.AddConDecimales;
                campos.Etiqueta = tblCampoDinamico.AddEtiqueta;
                campos.GrupoExtensionId = tblCampoDinamico.AddGrupoExtensionId;
                campos.Longitud = tblCampoDinamico.AddLongitud;
                campos.Nombre = tblCampoDinamico.AddNombre;
                campos.Orden = lstCamposTabla.Count + 1;
                campos.Requerido = tblCampoDinamico.AddRequerido;
                campos.Size = tblCampoDinamico.AddSize;
                campos.TipoCampo = (TipoCampo)tblCampoDinamico.AddTipoCampo;
                campos.TipoFecha = (TipoFecha)tblCampoDinamico.AddTipoFecha;
                campos._TipoFecha = (TipoFecha)tblCampoDinamico.AddTipoFecha;
                campos._ConDecimales = tblCampoDinamico.AddConDecimales;
                campos._GrupoExtensionId = tblCampoDinamico.AddGrupoExtensionId;
                campos._Size = tblCampoDinamico.AddSize;
                lstCamposTabla.Add(campos);
            }
            lstCamposTabla = lstCamposTabla.Where(r => r.Activo).OrderBy(r => r.Orden).ToList();

            return PartialView("_ConfigurarCampos", lstCamposTabla);
        }



        public ActionResult EliminarCampoTablero(List<CampoViewModel> lstCamposTabla, int iEliminar = 0, int plantillaId = 0)
        {
            var vEliminar = lstCamposTabla[iEliminar - 1];
            var plantillas = db.Plantillas.Where(x => x.PlantillaId == plantillaId).FirstOrDefault();
            bool bTieneRelacion = this.TieneRelacion(vEliminar is null ? 0 : vEliminar.CampoId, plantillas is null ? "" : plantillas.NombreTabla, vEliminar is null ? "" : vEliminar.Nombre);
            if (bTieneRelacion)
            {
                TempData["MessageCampoRelacion"] = "El campo que esta intentando eliminar, esta relacionado  con información importante, por favor quite las relaciones existentes antes de eliminarlo.";

                return PartialView("_ConfigurarCampos", lstCamposTabla);
            }

            lstCamposTabla.RemoveAt(iEliminar - 1);
            lstCamposTabla = lstCamposTabla ?? new List<CampoViewModel>();
            lstCamposTabla = lstCamposTabla.Where(r => r.Activo).OrderBy(r => r.Orden).ToList();

            return PartialView("_ConfigurarCampos", lstCamposTabla);
        }

        public bool TieneRelacion(long iId, string nombreTabla = "", string nombreCampo = "")
        {
            bool bResult = false;
            if (nombreTabla == "" || nombreCampo == "")
            {
                return bResult;
            }
            string sQuery = $@"IF EXISTS(SELECT * FROM Campoes WHERE (CampoId = @CampoId) AND Activo = 1)
		                                OR EXISTS(SELECT * FROM  {nombreTabla} WHERE {nombreCampo} IS NOT NULL)
	                                SELECT 'true'
                                ELSE
	                                SELECT 'false'";
            try
            {
                SqlConnection sqlConnections = new SqlConnection(coneccion);
                sqlConnections.Open();
                bResult = sqlConnections.QueryFirst<bool>(sQuery, new { CampoId = iId });
                sqlConnections.Close();
                return bResult;
            }
            catch (Exception ex)
            {
            }


            return bResult;
        }

        [HttpPost]
        public ActionResult CreateCampos(int iId, List<CampoViewModel> lstCamposTabla)
        {
            DateTime dtNow = DateTime.Now;
            List<Campo> lstCampos = new List<Campo>();
            Campo campo = new Campo();
            var plantillas = db.Plantillas.Where(x => x.PlantillaId == iId).FirstOrDefault();
            try
            {

                if (lstCamposTabla != null && lstCamposTabla.Any())
                {
                    //convertimos la lista
                    lstCampos = this.generarCampo(lstCamposTabla, iId);

                    //agregamos los campos nuevos
                    var vCamposNuevos = lstCampos != null ? lstCampos.Where(r => r.CampoId == 0).ToList() : null;
                    if (vCamposNuevos != null && vCamposNuevos.Count > 0)
                    {
                        foreach (var item in vCamposNuevos)
                        {

                            //item._ConDecimales = item?.Porcentaje?.ConDecimales;
                            //item._GrupoExtensionId = item?.ArchivoAdjunto?.GrupoExtension?.GrupoExtensionId;
                            //item._Size = item?.ArchivoAdjunto?.Size;
                            //item._TipoFecha = item?.Fecha?.TipoFecha;
                            var sResponse = this.CreateCampos(item);



                        }
                    }
                    //eliminamos los campos que ya no se usan
                    var vNoEliminarCamposIds = lstCampos.Select(r => r.CampoId).ToList();
                    var vEliminarCampos = this.GetListByNoIdsTableroDinamicoId(vNoEliminarCamposIds, iId);
                    if (vEliminarCampos != null && vEliminarCampos.Any())
                    {
                        foreach (var item in vEliminarCampos)
                        {
                            item.Activo = false;
                            var sResponse = this.UpdateCampos(item);
                            //if (sResponse.Length == 0)
                            //sResponse = HMTLHelperExtensions.InsertBitacora($"Se elimino el campo '{item.Nombre}' del tablero dinámico", dtNow, iLoggedUserId, CatalagoBitacoraId, model.TableroDinamicoId);
                        }
                    }

                    //modificamos el orden de los campos
                    var camposModificables = lstCampos != null ? lstCampos.Where(r => r.CampoId != 0).ToList() : null;
                    if (camposModificables != null && camposModificables.Count > 0)
                    {
                        foreach (var item in camposModificables)
                        {
                            var respuesta = UpdateCampos(item, false);
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
            var vResult = this.CrearTablaFisica(lstCampos, plantillas is null ? "" : plantillas.NombreTabla);

            TempData["Mensaje"] = "Se guardaron los campos exitosamente";

            return RedirectToAction("ConfigurarCampos", new { id = iId });
        }

        public ResultGuardarTablaFisica CrearTablaFisica(List<Campo> lstCamposTablero, string nombreTabla)
        {
            string sCampos = "";
            foreach (var item in lstCamposTablero)
            {
                string sTipoCampo = item.TipoCampo == TipoCampo.Fecha ? "[datetime]"
                    : item.TipoCampo == TipoCampo.Decimal ? "[decimal](18, 2)"
                    : item.TipoCampo == TipoCampo.Texto ? "[nvarchar](" + item.Longitud + ")"
                    : item.TipoCampo == TipoCampo.Numerico ? "[bigint]"
                    : "[nvarchar](" + item.Longitud + ")";

                sCampos += $"[{item.Nombre}] {sTipoCampo} NULL,";
            }

            string sDropTableScript = $@"IF OBJECT_ID('{nombreTabla}', 'U') IS NOT NULL 
                                            DROP TABLE {nombreTabla}; ";

            string sTablaScript = $@"CREATE TABLE {nombreTabla}(
	                [TablaFisicaId] [int] IDENTITY(1,1) NOT NULL,
	                {sCampos}
                 CONSTRAINT [PK_dbo.{nombreTabla}] PRIMARY KEY CLUSTERED 
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
                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                sqlConnection.Query(sDropTableScript + sTablaScript);
                sqlConnection.Close();
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


        public List<Campo> generarCampo(List<CampoViewModel> CampoViewModel, int plantillaId = 0)
        {
            List<Campo> lstCampo = new List<Campo>();

            try
            {
                if (CampoViewModel.Count > 0)
                {
                    foreach (var item in CampoViewModel)
                    {
                        Campo Campo = new Campo();
                        Campo.Activo = true;
                        var gpoExId = db.GrupoExtesiones.FirstOrDefault(g => g.GrupoExtensionId == item.GrupoExtensionId);
                        //Campo.ArchivoAdjunto = item.TipoCampo == TipoCampo.ArchivoAdjunto ? new CampoArchivoAdjunto { GrupoExtension = gpoExId, Size = item.Size } : null;
                        Campo._GrupoExtensionId = item._GrupoExtensionId;
                        Campo._Size = item._Size;
                        Campo.Ayuda = item.Ayuda;
                        Campo.CatalogoId = item.CatalogoId;
                        Campo.Etiqueta = item.Etiqueta;
                        //Campo.Fecha = item.TipoCampo == TipoCampo.Fecha ? new CampoFecha { TipoFecha = (TipoFecha)item.TipoFecha } : null;
                        Campo._TipoFecha = item._TipoFecha;
                        Campo.Longitud = item.Longitud;
                        Campo.Nombre = item.Nombre;
                        Campo.PlantillaId = plantillaId;
                        //Campo.Porcentaje = item.TipoCampo == TipoCampo.Porcentaje ? new CampoPorcentaje { ConDecimales = item.ConDecimales } : null;
                        Campo._ConDecimales = item._ConDecimales;
                        Campo.Requerido = item.Requerido;
                        Campo.TipoCampo = item.TipoCampo;
                        Campo.CampoId = item.CampoId;
                        Campo.Orden = item.Orden;
                        lstCampo.Add(Campo);



                    }
                }
            }
            catch (Exception ex)
            {

            }

            return lstCampo;

        }

        public List<Campo> GetListByNoIdsTableroDinamicoId(List<int> lstIds, int PlantillaId)
           => db.Campos.Where(r => !lstIds.Contains(r.CampoId) && r.PlantillaId == PlantillaId).ToList();


        public string CambiarOrden(Campo model)
        {
            try
            {
                if (db != null)
                    db.Dispose();

                db = new ApplicationDbContext();
                db.Campos.Attach(model);
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string UpdateCampos(Campo model, bool bandera = true)
        {
            try
            {
                if (db != null && bandera)
                    db.Dispose();


                db = new ApplicationDbContext();
                db.Campos.Attach(model);
                db.Entry(model).State = EntityState.Modified;

                db.SaveChanges();

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public string CreateCampos(Campo model)
        {
            try
            {

                db.Campos.Attach(model);
                db.Entry(model).State = EntityState.Added;
                db.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        //[HttpPost]
        //public ActionResult ConfigurarCampos(int PlantillaId, List<int> TipoCampo, List<string> Nombre, List<bool> ConDecimales, List<string> Etiqueta,
        //                                       List<string> Longitud, List<int> TipoFecha, List<int> GrupoExtensionId, List<int> Size, List<int> CatalogoId,
        //                                       List<string> Ayuda, List<bool> Requerido)
        //{
        //    var errMsg = "";
        //    try
        //    {
        //        List<Campo> camposToSave = new List<Campo>();
        //        Plantilla plantilla = db.Plantillas.FirstOrDefault(p => p.PlantillaId == PlantillaId);
        //        for (int i = 0; i < TipoCampo.Count; i++)
        //        {
        //            int gpoVal = GrupoExtensionId[i];
        //            var gpoExId = db.GrupoExtesiones.FirstOrDefault(g => g.GrupoExtensionId == gpoVal);
        //            string longVal = Longitud[i];
        //            int intLongVal = Convert.ToInt32(longVal);
        //            int catObj = CatalogoId[i];
        //            var objCatalogo = db.Catalogoes.FirstOrDefault(c => c.CatalogoId == catObj);
        //            TipoCampo tCamp = (TipoCampo)TipoCampo[i];
        //            Campo campo = new Campo();
        //            campo.Plantilla = plantilla;
        //            campo.PlantillaId = PlantillaId;
        //            campo.TipoCampo = tCamp;
        //            campo.Nombre = Nombre[i];
        //            campo.Etiqueta = Etiqueta[i];
        //            campo.Longitud = intLongVal;
        //            campo.Requerido = Requerido[i];
        //            campo.Ayuda = Ayuda[i];
        //            campo.Activo = true;
        //            // campo.Fecha = tCamp == Models.TipoCampo.Fecha ? new CampoFecha { TipoFecha = (TipoFecha)TipoFecha[i] } : null;
        //            //campo.ArchivoAdjunto = tCamp == Models.TipoCampo.ArchivoAdjunto ? new CampoArchivoAdjunto { GrupoExtension = gpoExId, Size = Size[i] } : null;
        //            //campo.Porcentaje = tCamp == Models.TipoCampo.Porcentaje ? new CampoPorcentaje { ConDecimales = ConDecimales[i] } : null;
        //            campo.CatalogoId = CatalogoId[i];


        //            camposToSave.Add(campo);
        //        }

        //        foreach (var campo in camposToSave)
        //        {
        //            db.Campos.Add(campo);
        //        }

        //        db.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["error"] = errMsg = "Ocurrió un error al momento de guardar campos.";
        //    }

        //    if (!string.IsNullOrWhiteSpace(errMsg))
        //    {

        //        return RedirectToAction("ConfigurarCampos", new { id = PlantillaId });
        //        //return View(PlantillaId);
        //    }

        //    return RedirectToAction("Index");
        //}


        #region NuevosAgregarCapos
        //nuevoCreateAjax
        public string GenerarNombreCampos(string nombre = "", int plantillaId = 0, int CampoId = 0)
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

            if (plantillaId != 0)
            {

                var r = new Random();
                var nombreCampos = db.Campos.Where(x => x.PlantillaId == plantillaId).ToList();
                while (nombreCampos.Where(x => x.Nombre == palabaSinTildes && x.CampoId != CampoId).Count() > 0)
                {
                    palabaSinTildes += $"{ r.Next(1, 89712399) }";
                }
            }

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
        public List<Campo> GetCampos(int iIdA = 0)
        {
            return db.Campos.Where(x => x.PlantillaId == iIdA).OrderBy(x => x.Orden).ToList();
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


        public ActionResult AjaxAgregarCampo(LstCampos tblCampoDinamico, int iIdA = 0)
        {

            if (iIdA == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener la Plantilla." }, JsonRequestBehavior.AllowGet);
            }
            var campos = new Campo();
            var CCampos = db.Campos.Where(x => x.PlantillaId == iIdA).Count();

            campos.Activo = true;
            campos.Ayuda = tblCampoDinamico.AddAyuda;
            campos.CatalogoId = tblCampoDinamico.AddCatalogoId;
            campos._ConDecimales = tblCampoDinamico.AddConDecimales;
            campos.Etiqueta = tblCampoDinamico.AddEtiqueta;
            campos._GrupoExtensionId = tblCampoDinamico.AddGrupoExtensionId;
            campos.Longitud = tblCampoDinamico.AddLongitud; string output =
            campos.Nombre = GenerarNombreCampos(tblCampoDinamico.AddNombre, iIdA);
            campos.Orden = CCampos + 1;
            campos.Requerido = tblCampoDinamico.AddRequerido;
            campos._Size = tblCampoDinamico.AddSize;
            campos.TipoCampo = (TipoCampo)tblCampoDinamico.AddTipoCampo;
            campos._TipoFecha = (TipoFecha)tblCampoDinamico.AddTipoFecha;
            campos._TipoFecha = (TipoFecha)tblCampoDinamico.AddTipoFecha;
            campos._ConDecimales = tblCampoDinamico.AddConDecimales;
            campos._GrupoExtensionId = tblCampoDinamico.AddGrupoExtensionId;
            campos._Size = tblCampoDinamico.AddSize;
            campos.relevantes = tblCampoDinamico.AddRelevante;
            campos.IdCampoPNT = tblCampoDinamico.AddCampoPNT;
            campos.IdTipoCampoPNT = tblCampoDinamico.AddTipoCampoPNT;
            if(tblCampoDinamico.AddOrdenSeleccion.HasValue)
                campos.OrdenSeleccionPublico = (OrdenSeleccionPublico)tblCampoDinamico.AddOrdenSeleccion.Value;
            campos.PlantillaId = iIdA;

            try
            {
                var validarExiste = db.Campos.Where(x => x.PlantillaId == iIdA && x.Nombre == campos.Nombre).Any();
                if (validarExiste)
                {
                    return Json(new { Hecho = false, Mensaje = "Ya existe un campo con el nombre <b>'" + campos.Nombre + "'</b>" }, JsonRequestBehavior.AllowGet); ;
                }
                db.Campos.Add(campos);
                db.SaveChanges();
                var PlantillaNombre = GetTableName(iIdA);
                string Respuesta = this.AddColumToTable(PlantillaNombre, campos.Nombre, this.GetAtributosInput(campos));
                string viewContent = ConvertViewToString("_ListaCampos", GetCampos(iIdA));
                //bitacora
                this.CreateBitacora(null, campos, campos.CampoId);

                return Json(new { Hecho = true, Mensaje = "se guardo exitosamente", Partial = viewContent }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult AjaxCambiarOrden(int iCampoId = 0, int iOrden = 0, int iPlantillaId = 0)
        {

            if (iCampoId == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el campo." }, JsonRequestBehavior.AllowGet);
            }

            if (iPlantillaId == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener la Plantilla." }, JsonRequestBehavior.AllowGet);
            }


            try
            {
                var campo = db.Campos.Where(x => x.CampoId == iCampoId).FirstOrDefault();
                var oldModel = (Campo)campo.Clone();
                var ExisteOrden = db.Campos.Where(x => x.PlantillaId == iPlantillaId && x.Orden == iOrden).Any();

                if (ExisteOrden)
                {
                    var ListaCampos = db.Campos.Where(x => x.PlantillaId == iPlantillaId && x.Orden >= iOrden).ToList();
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
                    //Bitacora
                    this.CreateBitacora(oldModel, campo, campo.CampoId);
                }

                string viewContent = ConvertViewToString("_ListaCampos", GetCampos(iPlantillaId));
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
        public List<string> GetColumnTableHistory(string TableName)
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


        public void VerifyNecessaryColumn(int PlantilldaId = 0)
        {
            try
            {
                var columnas = new List<Campo>();


                var mPlantillas = db.Plantillas.Where(x => x.PlantillaId == PlantilldaId).FirstOrDefault();
                if (mPlantillas != null && (mPlantillas.NombreTabla == null || mPlantillas.NombreTabla.Length == 0))
                {
                    mPlantillas.NombreTabla = GenerarNombreTabla(mPlantillas.NombreCorto);
                    db.Entry(mPlantillas).State = EntityState.Modified;
                    db.SaveChanges();
                }
                if (mPlantillas != null && (mPlantillas.NombreTablaHistory == null || mPlantillas.NombreTablaHistory.Length == 0))
                {
                    mPlantillas.NombreTabla = GenerarNombreTabla(mPlantillas.NombreCorto);
                    mPlantillas.NombreTablaHistory = mPlantillas.NombreTabla + "_History";
                    db.Entry(mPlantillas).State = EntityState.Modified;
                    db.SaveChanges();
                }
                if (!CheckIfExistTable(mPlantillas.NombreTabla))
                {
                    var result = CrearTablaFisica(mPlantillas);
                }
                if (!CheckIfExistTable(mPlantillas.NombreTablaHistory))
                {
                    var result = CrearTablaFisicaHistory(mPlantillas);
                }

                if (mPlantillas != null && mPlantillas.NombreTabla.Length > 0)
                {
                    var LstColumns = this.GetColumnTable(mPlantillas.NombreTabla);

                    var LstColumnsHistory = this.GetColumnTable(mPlantillas.NombreTablaHistory);

                    //normal
                    if (LstColumns.Count > 0)
                    {
                        columnas = db.Campos.Where(x => x.PlantillaId == mPlantillas.PlantillaId).ToList();

                        if (columnas.Count > 0)
                        {
                            foreach (var item in columnas)
                            {
                                if (!LstColumns.Contains(item.Nombre))
                                {
                                    if (item.Nombre.Contains(" "))
                                    {
                                        var Respuesta2 = HMTLHelperExtensions.modificarCampoErroneoPlantillas(item.CampoId);
                                        var mColumna = db.Campos.Where(x => x.CampoId == item.CampoId).FirstOrDefault();
                                        if (mColumna != null)
                                        {
                                            string Respuesta = this.AddColumToTable(mPlantillas.NombreTabla, mColumna.Nombre, this.GetAtributosInput(mColumna));
                                        }
                                    }
                                    else
                                    {
                                        string Respuesta = this.AddColumToTable(mPlantillas.NombreTabla, item.Nombre, this.GetAtributosInput(item));
                                    }
                                }
                            }
                        }
                        if (!LstColumns.Contains("OrganismoID"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTabla, "OrganismoID", "[int] NOT NULL DEFAULT ''");
                        }
                        if (!LstColumns.Contains("UsuarioId"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTabla, "UsuarioId", "[nvarchar] (128) NOT NULL DEFAULT ''");
                        }
                        if (!LstColumns.Contains("Activo"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTabla, "Activo", "BIT DEFAULT 1");
                        }
                        if (!LstColumns.Contains("FechaCreacion"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTabla, "FechaCreacion", "DATE DEFAULT ''");
                        }
                        if (!LstColumns.Contains("FechaBaja"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTabla, "FechaBaja", "DATE DEFAULT ''");
                        }
                        if (!LstColumns.Contains("OrganismoCapturoID"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTabla, "OrganismoCapturoID", "[int] NOT NULL DEFAULT ''");
                        }
                        if (!LstColumns.Contains("sysFrecuencia"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTabla, "sysFrecuencia", "[int] NOT NULL DEFAULT ''");
                        }
                        if (!LstColumns.Contains("sysNumFrecuencia"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTabla, "sysNumFrecuencia", "[int] NOT NULL DEFAULT ''");
                        }
                        if (!LstColumns.Contains("PeriodoId"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTabla, "PeriodoId", "[int] NOT NULL DEFAULT ''");
                        }
                        if (!LstColumns.Contains("Chosen"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTabla, "Chosen", "BIT DEFAULT 0");
                        }

                    }

                    //history
                    if (LstColumnsHistory.Count > 0)
                    {
                        columnas = db.Campos.Where(x => x.PlantillaId == mPlantillas.PlantillaId).ToList();

                        if (columnas.Count > 0)
                        {
                            foreach (var item in columnas)
                            {
                                if (!LstColumnsHistory.Contains(item.Nombre))
                                {
                                    if (item.Nombre.Contains(" "))
                                    {
                                        var Respuesta2 = HMTLHelperExtensions.modificarCampoErroneoPlantillas(item.CampoId);
                                        var mColumna = db.Campos.Where(x => x.CampoId == item.CampoId).FirstOrDefault();
                                        if (mColumna != null)
                                        {
                                            string Respuesta = this.AddColumToTable(mPlantillas.NombreTablaHistory, mColumna.Nombre, this.GetAtributosInput(mColumna));
                                        }
                                    }
                                    else
                                    {
                                        string Respuesta = this.AddColumToTable(mPlantillas.NombreTablaHistory, item.Nombre, this.GetAtributosInput(item));
                                    }
                                }
                            }
                        }
                        if (!LstColumnsHistory.Contains("OrganismoID"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTablaHistory, "OrganismoID", "[int] NOT NULL DEFAULT ''");
                        }
                        if (!LstColumnsHistory.Contains("UsuarioId"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTablaHistory, "UsuarioId", "[nvarchar] (128) NOT NULL DEFAULT ''");
                        }
                        if (!LstColumnsHistory.Contains("Activo"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTablaHistory, "Activo", "BIT DEFAULT 1");
                        }
                        if (!LstColumnsHistory.Contains("FechaCreacion"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTablaHistory, "FechaCreacion", "DATE DEFAULT ''");
                        }
                        if (!LstColumnsHistory.Contains("FechaBaja"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTablaHistory, "FechaBaja", "DATE DEFAULT ''");
                        }
                        if (!LstColumnsHistory.Contains("OrganismoCapturoID"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTablaHistory, "OrganismoCapturoID", "[int] NOT NULL DEFAULT ''");
                        }
                        if (!LstColumnsHistory.Contains("sysFrecuencia"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTablaHistory, "sysFrecuencia", "[int] NOT NULL DEFAULT ''");
                        }
                        if (!LstColumnsHistory.Contains("sysNumFrecuencia"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTablaHistory, "sysNumFrecuencia", "[int] NOT NULL DEFAULT ''");
                        }
                        if (!LstColumnsHistory.Contains("PeriodoId"))
                        {
                            this.AddColumToTable(mPlantillas.NombreTablaHistory, "PeriodoId", "[int] NOT NULL DEFAULT ''");
                        }

                    }
                }

                if (columnas.Count > 0)
                {
                    this.CheckIndexes(mPlantillas.NombreTabla, columnas);
                    this.CheckIndexes(mPlantillas.NombreTablaHistory, columnas);
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

        public string GetTableName(int iPlantillaId)
            => db.Plantillas.Where(x => x.PlantillaId == iPlantillaId).Select(x => x.NombreTabla).FirstOrDefault();
        public string GetPlantillaNombreLargo(int iPlantillaId)
           => db.Plantillas.Where(x => x.PlantillaId == iPlantillaId).Select(x => x.NombreLargo).FirstOrDefault();

        public string GetTableEjercicio(int iPlantillaId)
        {
            string sPeriodos = "";
            try
            {

                var plantillas = db.Plantillas.Where(x => x.PlantillaId == iPlantillaId).FirstOrDefault();
                if (plantillas != null)
                {
                    var mPeriodos = plantillas.Periodos.FirstOrDefault();
                    sPeriodos = mPeriodos.NombrePeriodo;
                }
            }
            catch (Exception ex)
            {

            }
            return sPeriodos;

        }

        public string GetPeriodoById(int periodoId)
        {
            string sPeriodo = "";
            try
            {

                var periodo = db.Periodos.Where(x => x.PeriodoId == periodoId).FirstOrDefault();
                if (periodo != null)
                {
                    sPeriodo = periodo.NombrePeriodo;
                }
            }
            catch (Exception ex)
            {

            }
            return sPeriodo;

        }
        //=> db.Plantillas.Where(x => x.PlantillaId == iPlantillaId).Select(x => x.Periodos.Select(y => y.NombrePeriodo).FirstOrDefault()).FirstOrDefault();

        public string ManageTable(int iPlantillaId)
        {
            string respuesta = "";

            var Plantilla = db.Plantillas.Where(x => x.PlantillaId == iPlantillaId).FirstOrDefault();

            if (Plantilla != null)
            {
                var Campos = db.Campos.Where(x => x.PlantillaId == iPlantillaId && x.Activo).OrderBy(x => x.Orden).ToList();
                if (Campos.Count > 0)
                {
                    foreach (var item in Campos)
                    {

                    }
                }
            }


            return respuesta;

        }

        public string GetAtributosInput(Campo campo)
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
        public string GetValueByCampo(Campo campo, string valor, bool bLike = false)
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



        public ActionResult CreateDynamic(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("IndexPlantillas", "Plantillas");
            }
            this.VerifyNecessaryColumn((int)Id);
            List<Campo> campos = GetCamposByColumn(Id);
            //validamos si se encuentra algun tipo tabla y validamos sus  datos
            //foreach (var item in campos.Where(x=>x.TipoCampo == TipoCampo.Catalogo).ToList())
            //{
            //    if (HMTLHelperExtensions.GetTipoTabla(item.CatalogoId))
            //    {
            //        this.VerifyNecessaryColumnCatalogo(item.CatalogoId);
            //    }
            //}
            ViewBag.PlantillaId = Id;
            ViewBag.Validation = HMTLHelperExtensions.GetValidationJquery(campos);
            ViewBag.NombrePlantilla = HMTLHelperExtensions.GetLeyArtiucloFraccionFromPlantilla(Id);
            SetFrecuencia((int)Id);
            //periodos
            //var AllPeriodos = db.Periodos.Where(x => x.Activo).ToList();
            //ViewBag.AllPeriodos = AllPeriodos.Select(o => new SelectListItem
            //{
            //    Text = o.NombrePeriodo,
            //    Value = o.PeriodoId.ToString()
            //});

            return View(campos);
        }


        public void SetFrecuencia(int PlantillaId = 0)
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            ViewBag.AllPeriodos = new List<SelectListItem>();
            ViewBag.AllFrecuencias = new List<SelectListItem>();
            ViewBag.AllSubFrecuencia = new List<SelectListItem>();
            try
            {
                ApplicationDbContext dbs = new ApplicationDbContext();
                var mPlantillas = dbs.Plantillas.Where(x => x.PlantillaId == PlantillaId).FirstOrDefault();
                if (mPlantillas != null)
                {
                    var AllPeriodos = mPlantillas.Periodos.Where(x=>x.Activo 
                    && (mPlantillas.Periodos == null || mPlantillas.Periodos.Where(y=> y.PeriodoId == x.PeriodoId).Any())
                    );
                    ViewBag.AllPeriodos = AllPeriodos.OrderBy(x => x.Orden).Select(o => new SelectListItem
                    {
                        Text = o.NombrePeriodo,
                        Value = o.PeriodoId.ToString()
                    });
                    var EnumFrecuencia = mPlantillas.Frecuencia;
                    lista.Add(new SelectListItem { Value = ((int)EnumFrecuencia).ToString(), Text = EnumFrecuencia.GetDisplayName() });
                    //ViewBag.AllFrecuencias = new  SelectList( new SelectListItem
                    //{
                    //    Text = AllFrecuencias.GetDisplayName(),
                    //    Value = AllFrecuencias.ToString()
                    //});
                    ViewBag.AllFrecuencias = lista;
                    ViewBag.AllSubFrecuencia = GetFrecuenciasModel(EnumFrecuencia);

                }

            }
            catch (Exception ex)
            {

            }




        }
        [HttpPost]
        public ActionResult CreateDynamic(int iPlantilldaId, FormCollection Form)
        {

            try
            {
                //var files = Request.Files.Count;
                //this.VerifyNecessaryColumn(iPlantilldaId);
                var Respuesta = InserDynamicScript(this.GetTableName(iPlantilldaId), GetCamposByColumn(iPlantilldaId, true), Form, this.GetPlantillaNombreLargo(iPlantilldaId), iPlantilldaId);
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

        //INSET QUERY
        public string InserDynamicScript(string Table, List<Campo> Campos, FormCollection form, string nombreLargo = "", int iPlantilldaId = 0)
        {

            string Respuesta = "";
            //bitacora
            List<cambiosCampos> cambioCampos = new List<cambiosCampos>();
            List<PrepararTablas> prepararTablas = new List<PrepararTablas>();
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
                    Inseert += ",Activo,OrganismoID,UsuarioId,OrganismoCapturoID,FechaCreacion,PeriodoId,sysFrecuencia,sysNumFrecuencia";
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

                            //Sub Tablas
                            if (item.TipoCampo == TipoCampo.Catalogo)
                            {
                                if (HMTLHelperExtensions.GetTipoTabla(item.CatalogoId))
                                {
                                    if (valor != "0" && valor != null)
                                    {
                                        prepararTablas.Add(new PrepararTablas() { CatalogoId = item.CatalogoId, Valor = Convert.ToInt32(valor) });
                                        valor = "1";
                                    }
                                }

                            }

                            if (item.TipoCampo == TipoCampo.ArchivoAdjunto)
                            {
                                var file = Request.Files[$"{item.Nombre}"];
                                if (file != null)
                                {
                                    var periodo = this.GetPeriodoById(Convert.ToInt32(form["PeriodoId"]));
                                    var respuestaFile = this.GuardarArchivos(file, Table, periodo);
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

                        }
                        else
                        {
                            valor = "NULL";
                            if (item.Requerido)
                            {
                                valor = HMTLHelperExtensions.GetDefaultValue(item);
                            }

                        }

                        Inseert += GetValueByCampo(item, valor);
                        first = false;

                        //bitacora
                        cambioCampos.Add(new cambiosCampos
                        {
                            nombre_campo = item.Etiqueta,
                            es_modificado = false,
                            campo_nuevo = valor
                        });
                    }
                    //Inseert += $@",1,{usuario.OrganismoID},'{usuario.Id}'";
                    Inseert += $@",1,{usuario.OrganismoID ?? 0},'{usuario.Id}',{usuario.OrganismoID ?? 0},'{ DateTime.Now.ToString("dd/MM/yyyy") }',{form["PeriodoId"]},{form["sysFrecuencia"]},{form["sysNumFrecuencia"]}";
                    Inseert += ")";

                }

                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                var inserrtedId = sqlConnection.Query<int>(Inseert).FirstOrDefault();
                sqlConnection.Close();
                //Bitacora
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, Table, $"Plantilla: {nombreLargo}", inserrtedId, usuario?.Id);
                res = HMTLHelperExtensions.FechaActualizacion(iPlantilldaId);

                //revisamos si todo esta bien para guardar los datos en la tablas
                if (prepararTablas.Count > 0)
                {
                    foreach (var item in prepararTablas)
                    {
                        var catRespuesta = this.InsertDynamicTablaScript(form, usuario, inserrtedId, item.CatalogoId, item.Valor);
                        if (!catRespuesta.Result)
                        {
                            catRespuesta = this.rollBackPlantilla(Table, nombreLargo, inserrtedId, usuario, cambioCampos);
                            if (!catRespuesta.Result)
                                return catRespuesta.Valor;

                            return catRespuesta.Valor;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Respuesta = ex.Message;
            }
            return Respuesta;


        }

        public ResultGuardarTablaFisica rollBackPlantilla(string Table, string nombreLargo, int inserrtedId, ApplicationUser usuario, List<cambiosCampos> cambioCampos)
        {
            ResultGuardarTablaFisica respueesta = new ResultGuardarTablaFisica();
            respueesta.Result = true;
            try
            {
                //ncesitamos borrar la informaciónn que se guardo anteriormente
                var sQueryEliminar = $"DELETE TOP(1) FROM {Table} WHERE TablaFisicaId = {inserrtedId}; ";
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
                var res = HMTLHelperExtensions.Bitacora(camposElimiandos, Table, $"Plantilla: {nombreLargo}", inserrtedId, usuario?.Id, 3);
            }
            catch (Exception ex)
            {
                respueesta.Result = false;
                respueesta.Valor = ex.Message;
            }
            return respueesta;
        }

        //public ResultGuardarTablaFisica rollBackCatalogo(List<PrepararTablas> prepararTablas, int inserrtedId, ApplicationUser usuario, List<cambiosCampos> cambioCampos)
        //{
        //    ResultGuardarTablaFisica respueesta = new ResultGuardarTablaFisica();
        //    respueesta.Result = true;
        //    try
        //    {
        //        foreach (var item in prepararTablas)
        //        {
        //            var catalogo = db.Catalogoes.Where(x => x.CatalogoId == item.CatalogoId).FirstOrDefault();
        //            if(catalogo != null)
        //            {
        //                //ncesitamos borrar la informaciónn que se guardo anteriormente
        //                var sQueryEliminar = $"DELETE FROM {catalogo.NombreTabla} WHERE Idregistro = {inserrtedId}; ";
        //                SqlConnection sqlConnection = new SqlConnection(coneccion);
        //                sqlConnection.Open();
        //                sqlConnection.Query(sQueryEliminar);
        //                sqlConnection.Close();

        //                //volteamos los datos para informarle que no se eliminaron
        //                var camposElimiandos = new List<cambiosCampos>();
        //                foreach (var campo in cambioCampos)
        //                {
        //                    camposElimiandos.Add(new cambiosCampos() { campo_anterior = campo.campo_nuevo, campo_nuevo = null, es_modificado = true, nombre_campo = campo.nombre_campo });
        //                }
        //                //Bitacora
        //                var res = HMTLHelperExtensions.Bitacora(camposElimiandos, Table, $"Plantilla: {nombreLargo}", inserrtedId, usuario?.Id, 3);
        //            }


        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        respueesta.Result = false;
        //        respueesta.Valor = ex.Message;
        //    }
        //    return respueesta;
        //}

        #region Catalogos 
        public void VerifyNecessaryColumnCatalogo(int iId = 0)
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
                    [Chosen] BIT DEFAULT 0,
                 CONSTRAINT [PK_dbo.{model.NombreTabla}] PRIMARY KEY CLUSTERED 
                (
	                [TablaFisicaId] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY];";

           
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

                db.Database.ExecuteSqlCommand(sDropTableScript + sTablaScript );
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

                if (principal != null)
                {
                    var nombreCampo = principal.Nombre;
                    if (principal.TipoCampo == TipoCampo.Catalogo)
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

        public ResultGuardarTablaFisica InsertDynamicTablaScript(FormCollection form, ApplicationUser usuario, int RegistroId = 0, int CatalogoId = 0, int registros = 0)
        {
            ResultGuardarTablaFisica respuesta = new ResultGuardarTablaFisica();
            respuesta.Result = true;
            ApplicationDbContext db = new ApplicationDbContext();
            //bitacora
            List<cambiosCampos> cambioCampos = new List<cambiosCampos>();
            try
            {
                var Insert = "";
                //aqui seleccionamos el valor que necesitamos guardar
                var mCatalogo = db.Catalogoes.Where(x => x.CatalogoId == CatalogoId).FirstOrDefault();
                var getCampoCatalogo = db.CampoCatalogo.Where(x => x.CatalogoId == CatalogoId).ToList();
                if (getCampoCatalogo.Count > 0)
                {
                    for (int i = 1; i <= registros; i++)
                    {
                        var first = true;
                        Insert = $@"INSERT INTO { mCatalogo.NombreTabla }";
                        Insert += "(";

                        foreach (var item in getCampoCatalogo)
                        {

                            if (!first)
                            {
                                Insert += ",";
                            }
                            Insert += $@"{item.Nombre}";
                            first = false;
                        }
                        //campos estaticos
                        Insert += ",Activo,OrganismoID,UsuarioId,OrganismoCapturoID,FechaCreacion,Idregistro";
                        Insert += ") OUTPUT INSERTED.TablaFisicaId ";
                        Insert += "VALUES (";
                        first = true;
                        //agregamos los datos a guardar en esesas tablas
                        foreach (var item in getCampoCatalogo)
                        {
                            string valor = "";
                            //Generamos el nombre del campo que debe de tener
                            string sNombre = $"field{CatalogoId}_{item.Nombre}_{i}";
                            if (!first)
                            {
                                Insert += ",";
                            }

                            if (item.Activo)
                            {
                                valor = form[$@"{sNombre}"];
                                //if (item.Etiqueta == "ID")
                                //{
                                //    valorCatalogoId = valor;
                                //}
                                //Archivo Adjunto
                                if (item.TipoCampo == TipoCampo.ArchivoAdjunto)
                                {
                                    var file = Request.Files[$"{item.Nombre}"];
                                    if (file != null)
                                    {
                                        var periodo = this.GetPeriodoById(Convert.ToInt32(form["PeriodoId"]));
                                        var respuestaFile = this.GuardarArchivos(file, mCatalogo.NombreTabla, periodo);
                                        if (respuestaFile.Result)
                                        {
                                            valor = respuestaFile.Valor;
                                        }
                                        else
                                        {
                                            return respuestaFile;
                                        }
                                    }

                                }
                                //Casilla de verificacón
                                if (item.TipoCampo == TipoCampo.CasillaVerificacion)
                                {
                                    valor = valor == null ? "0" : valor;
                                }
                                //si esta vacio y es alugno de estos lo dejamos como nulo
                                if ((valor == null || valor == "") &&
                                    (item.TipoCampo == TipoCampo.Decimal ||
                                    item.TipoCampo == TipoCampo.Dinero ||
                                    item.TipoCampo == TipoCampo.Numerico ||
                                    item.TipoCampo == TipoCampo.Porcentaje))
                                {
                                    valor = "NULL";
                                }
                                //remplazamos las masccaras y asi
                                if ((valor != null) && (item.TipoCampo == TipoCampo.Decimal || item.TipoCampo == TipoCampo.Dinero || item.TipoCampo == TipoCampo.Numerico || item.TipoCampo == TipoCampo.Porcentaje))
                                {
                                    valor = valor.Replace("(", "").Replace(")", "").Replace(",", "");
                                }

                            }
                            else
                            {
                                valor = "NULL";
                                if (item.Requerido)
                                {
                                    valor = HMTLHelperExtensions.GetDefaultValue(item);
                                }

                            }
                            Insert += GetValueByCampo(item, valor);
                            first = false;

                            //bitacora
                            cambioCampos.Add(new cambiosCampos
                            {
                                nombre_campo = item.Etiqueta,
                                es_modificado = false,
                                campo_nuevo = valor
                            });
                        }
                        //Inseert += $@",1,{usuario.OrganismoID},'{usuario.Id}'";
                        Insert += $@",1,{usuario.OrganismoID ?? 0},'{usuario.Id}',{usuario.OrganismoID ?? 0},'{ DateTime.Now.ToString("dd/MM/yyyy") }',{RegistroId}";
                        Insert += ") ";

                        //guardamos informacion
                        SqlConnection sqlConnection = new SqlConnection(coneccion);
                        sqlConnection.Open();
                        var inserrtedId = sqlConnection.Query<int>(Insert).FirstOrDefault();
                        sqlConnection.Close();

                        //Bitacora
                        var res = HMTLHelperExtensions.Bitacora(cambioCampos, mCatalogo.NombreTabla, $"Tabla: {mCatalogo.Nombre}", inserrtedId, usuario?.Id);

                    }
                    //respuesta.Valor = valorCatalogoId;
                }

            }
            catch (Exception ex)
            {
                respuesta.Result = false;
                respuesta.Valor = ex.Message;
            }
            return respuesta;
        }

        //form lo podria utlizar en un futuro y  allfields tambien podria utlizarlo en un futuro
        public IPagedList<dynamic> GetSelectFromTableCatalogoAsync(int CatalogoId, int Idregistro = 0, FormCollection form = null, List<Campo> Campos = null, int PerPage = 10, int iPagina = 1, bool allFields = true)
        {

            ViewBag.sPrincipal = "";
            IPagedList<dynamic> results = null;
            List<AuxTitlePlantillas> Titles = new List<AuxTitlePlantillas>();
            //agreamos los titulos de la tabla

            try
            {

                var catalogo = db.Catalogoes.Where(x => x.CatalogoId == CatalogoId).FirstOrDefault();
                string sQuery = "";

                sQuery = $@"SELECT ";
                //en este caso iria el relevante
                var Relevantes = allFields ? db.CampoCatalogo.Where(x => x.CatalogoId == CatalogoId && x.Activo).ToList() : db.CampoCatalogo.Where(x => x.CatalogoId == CatalogoId && x.Activo).ToList();
                foreach (var relevante in Relevantes)
                {
                    sQuery += $@"{relevante.Nombre},";
                    Titles.Add(new AuxTitlePlantillas { LbNombre = relevante.Etiqueta });

                }

                sQuery += $@"'{catalogo.NombreTabla}' as NombreTabla, {CatalogoId} as CatalogoId, ";
                sQuery += $@"TablaFisicaId, Activo";//sQuery += $@"TablaFisicaId, Activo, FechaCreacion,UsuarioId";
                Titles.Add(new AuxTitlePlantillas { LbNombre = "Activo" });
                //Titles.Add(new AuxTitlePlantillas { LbNombre = "Fecha Creación" });
                //Titles.Add(new AuxTitlePlantillas { LbNombre = "Usuario" });
                ViewBag.Titles = Titles;
                ViewBag.Relevantes = Relevantes;
                ViewBag.NombreTabla = catalogo.NombreTabla;
                ViewBag.nombreCatalogo = catalogo.Nombre;

                sQuery += $@" FROM {catalogo.NombreTabla}";

                //toda la seccion de form si en algun momento lo veremos
                var SQueryWhereForRows = $" WHERE Idregistro = {Idregistro}";
                //sQuery += $@" WHERE OrganismoID={OrganismoID} ";
                //SQueryWhereForRows = $@" WHERE OrganismoID={OrganismoID} ";
                //where Secction
                //if (Campos != null && Campos.Count > 0)
                //{
                //    foreach (var item in Campos.Where(x => x.TipoCampo != TipoCampo.ArchivoAdjunto).ToList())
                //    {
                //        string valor = form[$@"{item.Nombre}"];

                //        if (valor != null && valor != "")
                //        {
                //            var sLikeIgual = "=";

                //            if (item.TipoCampo == TipoCampo.Texto || item.TipoCampo == TipoCampo.AreaTexto || item.TipoCampo == TipoCampo.Alfanumerico || item.TipoCampo == TipoCampo.email
                //                || item.TipoCampo == TipoCampo.Hipervinculo || item.TipoCampo == TipoCampo.Telefono)
                //            {
                //                sLikeIgual = "LIKE";
                //            }

                //            if (item.TipoCampo == TipoCampo.Decimal || item.TipoCampo == TipoCampo.Dinero || item.TipoCampo == TipoCampo.Numerico || item.TipoCampo == TipoCampo.Porcentaje)
                //            {
                //                valor = valor.Replace("(", "").Replace(")", "").Replace(",", "");
                //            }

                //            //poneemos el where
                //            if (item._TipoFecha == TipoFecha.FechaHasta)
                //            {

                //            }
                //            else if (item._TipoFecha == TipoFecha.FechaDesde)
                //            {

                //            }
                //            else
                //            {

                //                sQuery += $@" AND {item.Nombre} {sLikeIgual} {GetValueByCampo(item, valor, (sLikeIgual == "LIKE"))}";
                //                SQueryWhereForRows += $@" AND {item.Nombre} {sLikeIgual} {GetValueByCampo(item, valor, (sLikeIgual == "LIKE"))}";


                //            }

                //        }
                //    }
                //}

                sQuery += SQueryWhereForRows;

                sQuery += $" ORDER BY FechaCreacion DESC";

                sQuery += $@" offset ({iPagina} - 1) * {PerPage} rows
                            fetch next {PerPage} rows only;";

                SqlParameter param = new SqlParameter()
                {
                    ParameterName = "@sql",
                    Value = sQuery
                };
                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                SqlCommand command = new SqlCommand("ExecuteSql", sqlConnection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(param);
                command.CommandTimeout = 3000000;

                results = HMTLHelperExtensions.DynamicListFromSql(command).ToPagedList(1, PerPage);

                var sQueryCantidad = $"SELECT COUNT(TablaFisicaId) FROM {catalogo.NombreTabla} " + SQueryWhereForRows;
                var Rows = db.Database.SqlQuery<int>(sQueryCantidad).FirstOrDefault();
                var pageCount = 0;
                if (Rows > 0)
                {
                    var countRows = (double)Rows / PerPage;
                    pageCount = (int)Math.Ceiling(countRows);
                }

                ViewBag.PageCount = pageCount;
                ViewBag.TotalCount = Rows;
                ViewBag.TotalCountString = Rows.ToString("#,##0");
                ViewBag.iPagina = iPagina;

            }
            catch (Exception ex)
            {

            }


            return results;
        }

        //public ResultGuardarTablaFisica GetSelectFromTableCatalogoCount(int CatalogoId, int Idregistro = 0, FormCollection form = null, List<Campo> Campos = null, int PerPage = 10, int iPagina = 1, bool allFields = true)
        //{

        //    ResultGuardarTablaFisica results = new ResultGuardarTablaFisica();
        //    results.Result = true;
        //    //agreamos los titulos de la tabla

        //    try
        //    {

        //        var catalogo = db.Catalogoes.Where(x => x.CatalogoId == CatalogoId).FirstOrDefault();
        //        string sQuery = "";

        //        sQuery = $@"SELECT count(TablaFisicaId) FROM {catalogo.NombreTabla}";
        //        var SQueryWhereForRows = $" WHERE Idregistro = {Idregistro}";

        //        SqlParameter param = new SqlParameter()
        //        {
        //            ParameterName = "@sql",
        //            Value = sQuery
        //        };
        //        SqlConnection sqlConnection = new SqlConnection(coneccion);
        //        sqlConnection.Open();
        //        SqlCommand command = new SqlCommand("ExecuteSql", sqlConnection);
        //        command.CommandType = CommandType.StoredProcedure;
        //        command.Parameters.Add(param);
        //        command.CommandTimeout = 3000000;

        //        results.Valor = HMTLHelperExtensions.DynamicListFromSql(command).ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        results.Result = false;
        //        results.Valor = ex.Message;
        //    }


        //    return results;
        //}


        [HttpPost]
        public ActionResult BuscarDatosTabla(int CatalogoId = 0, int Idregistro = 0)
        {
            try
            {
                if (CatalogoId == 0 || Idregistro == 0)
                {
                    return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el Catalogo o la relación con el registro." }, JsonRequestBehavior.AllowGet);
                }
                Catalogo catalogo = db.Catalogoes.Where(x => x.CatalogoId == CatalogoId && x.Activo).FirstOrDefault();

                if (catalogo == null)
                {
                    return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el Catalogo, el catalogo ya no existe o esta desabilitado." }, JsonRequestBehavior.AllowGet);
                }

                var ListaDatos = GetSelectFromTableCatalogoAsync(CatalogoId, Idregistro);
                string viewContent = ConvertViewToString("_CamposTablaList", ListaDatos);
                return Json(new { Hecho = true, Mensaje = catalogo.Nombre, Partial = viewContent }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult CheckIfHaveRows(int CatalogoId = 0, int Idregistro = 0)
        {
            try
            {
                if (CatalogoId == 0 || Idregistro == 0)
                {
                    return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el Catalogo o la relación con el registro, para la verificaciones de la sub Tabla." }, JsonRequestBehavior.AllowGet);
                }
                Catalogo catalogo = db.Catalogoes.Where(x => x.CatalogoId == CatalogoId && x.Activo).FirstOrDefault();

                if (catalogo == null)
                {
                    return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el Catalogo, el catalogo ya no existe o esta desabilitado, al momento de verificcar la sub Tabla." }, JsonRequestBehavior.AllowGet);
                }

                var total = HMTLHelperExtensions.getTotalRowsPlantilla(catalogo.NombreTabla, Idregistro);

                return Json(new { Hecho = true, Mensaje = total }, JsonRequestBehavior.AllowGet);
                //if (!resultado.Result)
                //{
                //    return Json(new { Hecho = false, Mensaje = resultado.Valor }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json(new { Hecho = true, Mensaje = resultado.Valor }, JsonRequestBehavior.AllowGet);
                //}
            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion


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
                string directorio_sub = $@"Archivos\Plantillas\{nombreCarpeta}\{EjercicioTxt}\{+DateTime.Now.Month}";
                string Directorio = $@"{Server.MapPath("~/")}{directorio_sub}";

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
                    respuesta.Valor = $@"\{directorio_sub}\{nombreArchAnexo}";
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

        public ActionResult DetailDynamic(int? Id, int? iPlantilldaId = 0, string TablaNombre = "")
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




            List<Campo> LstCampos = this.GetValuesFromTable((int)iPlantilldaId, (int)Id, TablaNombre, usuario.OrganismoID ?? 0);
            ViewBag.Validation = HMTLHelperExtensions.GetValidationJquery(LstCampos);
            ViewBag.TablaFisicaId = Id;
            ViewBag.TablaNombre = TablaNombre;
            ViewBag.PlantillaId = iPlantilldaId;
            return PartialView("_CamposDetails", LstCampos);
        }

        //Edit Dynamic
        public ActionResult EditDynamic(int? Id, int? iPlantilldaId = 0, string TablaNombre = "")
        {
            if (Id == null)
            {
                return RedirectToAction("IndexPlantillas", "Plantillas");
            }
            if (TablaNombre == null || TablaNombre.Length == 0)
            {
                return RedirectToAction("IndexPlantillas", "Plantillas");
            }
            var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();

            if (usuario == null)
            {
                return RedirectToAction("IndexPlantillas", "Plantillas");
            }
            this.VerifyNecessaryColumn((int)iPlantilldaId);
            List<Campo> LstCampos = this.GetValuesFromTable((int)iPlantilldaId, (int)Id, TablaNombre, usuario.OrganismoID ?? 0);

            //List<Campo> campos = GetCamposByColumn(Id);
            //foreach (var item in LstCampos.Where(x => x.TipoCampo == TipoCampo.Catalogo).ToList())
            //{
            //    if (HMTLHelperExtensions.GetTipoTabla(item.CatalogoId))
            //    {
            //        this.VerifyNecessaryColumnCatalogo(item.CatalogoId);
            //    }
            //}
            ViewBag.PlantillaId = iPlantilldaId;
            ViewBag.Validation = HMTLHelperExtensions.GetValidationJquery(LstCampos);
            ViewBag.NombrePlantilla = HMTLHelperExtensions.GetLeyArtiucloFraccionFromPlantilla(iPlantilldaId);
            return View(LstCampos);
        }



        [HttpPost]
        public ActionResult EditDynamic(int iPlantilldaId, FormCollection Form, int TablaFisicaId = 0)
        {

            try
            {
                List<Campo> campos = GetCamposByColumn(iPlantilldaId);
                foreach (var item in campos.Where(x => x.TipoCampo == TipoCampo.Catalogo).ToList())
                {
                    if (HMTLHelperExtensions.GetTipoTabla(item.CatalogoId))
                    {
                        if (item.Requerido)
                        {
                            string nombreTabla = HMTLHelperExtensions.GetNombreTabla(item.CatalogoId);
                            int TotalRows = HMTLHelperExtensions.getTotalRowsPlantilla(nombreTabla, TablaFisicaId);
                            if (TotalRows == 0)
                            {
                                return Json(new { Hecho = false, Mensaje = $"El campo {item.Etiqueta} es requerido y necesita tener registros." }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                var Respuesta = EditDynamicScript(this.GetTableName(iPlantilldaId), GetCamposByColumn(iPlantilldaId), Form, TablaFisicaId, iPlantilldaId);
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


        //Edit QUERY
        public string EditDynamicScript(string Table, List<Campo> Campos, FormCollection form, int TablaFisicaId = 0, int plnatillaId = 0)
        {

            string Respuesta = "";
            bool Update = false;
            //bitacora

            try
            {
                //bitacora
                List<cambiosCampos> cambioCampos = new List<cambiosCampos>();
                List<Campo> CamposAnteriores = new List<Campo>();
                var nombreLargo = this.GetPlantillaNombreLargo(plnatillaId);

                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var Inseert = "";

                if (Campos.Count > 0)
                {
                    //Verificamos los valores anteriores!
                    CamposAnteriores = this.GetValuesFromTableById(plnatillaId, TablaFisicaId);
                    var first = true;
                    // var boolFile = true;
                    Inseert = $@"UPDATE { Table } ";
                    Inseert += "SET ";


                    foreach (var item in Campos)
                    {
                        string valor = form[$@"{item.Nombre}"];
                        var campoAnterior = CamposAnteriores.Where(x => x.CampoId == item.CampoId).FirstOrDefault();
                        //Verificamos los datos
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
                        if ((valor == "" || valor == null) &&
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

                        //inicio de editar
                        var valorFormatted = GetValueByCampo(item, valor);
                        if (campoAnterior.TipoCampo == TipoCampo.Fecha)
                        {
                            try { DateTime oDate = Convert.ToDateTime(campoAnterior.Valor); campoAnterior.Valor = oDate.ToString("dd/MM/yyyy"); } catch (Exception ex) { }
                        }
                        if (campoAnterior.TipoCampo == TipoCampo.CasillaVerificacion)
                        {
                            try { campoAnterior.Valor = campoAnterior.Valor == "false" || campoAnterior.Valor == "False" ? "0" : campoAnterior.Valor == "true" || campoAnterior.Valor == "True" ? "1" : campoAnterior.Valor; } catch (Exception ex) { }
                        }
                        if (campoAnterior.TipoCampo == TipoCampo.ArchivoAdjunto && (valor == "" || valor == null))
                        {
                            valorFormatted = GetValueByCampo(item, campoAnterior.Valor);
                            valor = campoAnterior.Valor;
                        }
                        var valorAnterior = GetValueByCampo(item, campoAnterior.Valor);  //campoAnterior.Valor == "" || campoAnterior.Valor == null ? "NULL" : campoAnterior.Valor;
                        var modificado = false;
                        if (valorFormatted != valorAnterior)
                        {
                            if (!first)
                            {
                                Inseert += ",";
                            }
                            Inseert += item.TipoCampo == TipoCampo.ArchivoAdjunto && valor == null ? "" : $@"{item.Nombre}=";
                            Inseert += item.TipoCampo == TipoCampo.ArchivoAdjunto && valor == null ? "" : valorFormatted;

                            if (item.TipoCampo == TipoCampo.ArchivoAdjunto && valor == null && !first)
                            {
                                Inseert = Inseert.Substring(0, (Inseert.Length - 1));
                            }
                            modificado = true;
                            first = false;
                            Update = true;

                        }
                        //bitacora
                        cambioCampos.Add(new cambiosCampos
                        {
                            nombre_campo = item.Etiqueta,
                            es_modificado = modificado,
                            campo_nuevo = HMTLHelperExtensions.FormatDataBitacora(item.TipoCampo, item._ConDecimales, item.CatalogoId, valor),
                            campo_anterior = HMTLHelperExtensions.FormatDataBitacora(item.TipoCampo, item._ConDecimales, item.CatalogoId, campoAnterior.Valor),
                            link = item.TipoCampo == TipoCampo.Hipervinculo,
                            file = item.TipoCampo == TipoCampo.ArchivoAdjunto
                        });

                    }
                    Inseert += $@" WHERE TablaFisicaId={TablaFisicaId}";
                    if (Update)
                    {
                        SqlConnection sqlConnection = new SqlConnection(coneccion);
                        sqlConnection.Open();
                        sqlConnection.Query(Inseert);
                        sqlConnection.Close();

                        //Bitacora
                        var res = HMTLHelperExtensions.Bitacora(cambioCampos, Table, $"Plantilla: {nombreLargo}", TablaFisicaId, usuario?.Id, 2);
                        res = HMTLHelperExtensions.FechaActualizacion(plnatillaId);
                    }

                }


            }
            catch (Exception ex)
            {
                Respuesta = ex.Message;
            }

            return Respuesta;

        }

        public List<Campo> GetValuesFromTable(int Id, int TablaFisicaId, string TablaNombre, int OrganismoID,bool isCatalogo = false)
        {

            List<Campo> LstCampos = new List<Campo>();
            try
            {
                //var urls= Url.Action("Action", "Controller");
                List<Campo> Campos = isCatalogo ?  GetCamposByColumnCatalogo(Id) : GetCamposByColumn(Id);
                string sQuery = "";
                string sInnerJoin = "";
                bool first = true;
                if (Campos.Count > 0)
                {
                    var top = isCatalogo ? "" : "TOP 1";
                    sQuery = $@"SELECT {top} TablaFisicaId,";
                    foreach (var item in Campos)
                    {

                        //if (!first)
                        //{
                        //    sQuery += ",";
                        //}
                        if (item.TipoCampo == TipoCampo.Catalogo)
                        {
                            if (HMTLHelperExtensions.GetTipoTabla(item.CatalogoId))
                            {
                                sQuery += $@"{(!first ? "," : "")} {item.CatalogoId} as {item.Nombre}";
                            }
                            else
                            {
                                sQuery += $@"{(!first ? "," : "")} {item.Nombre}";
                            }
                        }
                        else
                        {
                            sQuery += $@"{(!first ? "," : "")} {item.Nombre}";
                        }

                        first = false;
                    }
                    var tablaFisica = isCatalogo ? "Idregistro" : "TablaFisicaId";
                    sQuery += $@" FROM {TablaNombre} WHERE OrganismoID={OrganismoID} AND {tablaFisica}={TablaFisicaId}  ORDER BY TablaFisicaId DESC";

                }

                SqlConnection con = new SqlConnection(coneccion);
                con.Open();
                SqlCommand cmd = new SqlCommand(sQuery, con);
                DataTable myTable = new DataTable();
                myTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in myTable.Rows)
                {
                    var tablaFisicaid = "";
                    foreach (var item in Campos)
                    {
                        item.Valor = row[$"{item.Nombre}"].ToString();
                        try
                        {
                            if (item.TipoCampo == TipoCampo.Fecha && !string.IsNullOrEmpty(item.Valor))
                            {
                                item.Valor = item.Valor.Split(' ')[0].ToString();
                            }
                        }
                        catch (Exception)
                        {

                        }
                        item.TablaFisicaId = TablaFisicaId;
                        item.CatalogoTablaFisicaId = Convert.ToInt32(row[$"TablaFisicaId"]);
                        item.ValorUrl = isCatalogo ?  Url.Action("GetAttachment", "Catalogos", new { url = item.Valor }) : Url.Action("GetAttachment", "Plantillas", new { url = item.Valor });
                        var  campo = item.Clone() as Campo;
                        LstCampos.Add(campo);

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

        public List<Campo> GetValuesFromTableById(int plantillaId, int TablaFisicaId)
        {

            List<Campo> LstCampos = new List<Campo>();
            try
            {
                //var urls= Url.Action("Action", "Controller");
                List<Campo> Campos = GetCamposByColumn(plantillaId);
                var TablaNombre = GetTableName(plantillaId);
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
                    sQuery += $@" FROM {TablaNombre} WHERE TablaFisicaId={TablaFisicaId}  ORDER BY TablaFisicaId DESC";

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
                        //item.ValorUrl = Url.Action("GetAttachment", "Plantillas", new { url = item.Valor });
                        LstCampos.Add(item);

                    }
                }
                con.Close();
            }
            catch (Exception ex)
            {

            }


            return LstCampos;
        }



        public List<Campo> GetCamposByColumn(int? iPlantillaId = 0, bool procesar = false)
        {
            List<Campo> ListaCampos = new List<Campo>();
            try
            {
                var plantillaNombre = db.Plantillas.Where(x => x.PlantillaId == iPlantillaId).Select(x => x.NombreTabla).FirstOrDefault();
                var LstCampos = GetColumnTable(plantillaNombre);
                if (procesar)
                {
                    ListaCampos = db.Campos.Where(x => x.PlantillaId == iPlantillaId && LstCampos.Contains(x.Nombre.Trim())).OrderBy(x => x.Orden).ToList();
                }
                else
                {
                    ListaCampos = db.Campos.Where(x => x.PlantillaId == iPlantillaId && LstCampos.Contains(x.Nombre.Trim()) && x.Activo).OrderBy(x => x.Orden).ToList();
                }
            }
            catch (Exception ex)
            {

            }


            return ListaCampos;
        }

        public List<Campo> GetCamposByColumnCatalogo(int? CatalogoId = 0, bool procesar = false)
        {
            List<Campo> ListaCampos = new List<Campo>();
            try
            {
                var nombreTabla = db.Catalogoes.Where(x => x.CatalogoId == CatalogoId).Select(x => x.NombreTabla).FirstOrDefault();
                var LstCampos = GetColumnTable(nombreTabla);
                if (procesar)
                {
                    ListaCampos = db.CampoCatalogo.Where(x => x.CatalogoId == CatalogoId && LstCampos.Contains(x.Nombre.Trim())).OrderBy(x => x.Orden)
                        .Select(x=> new Campo()
                        {
                            Activo = x.Activo,
                            Ayuda = x.Ayuda,
                            CampoId = x.CatalogoId,
                            CatalogoId=x.CampoCatalogoId,
                            Etiqueta = x.Etiqueta,
                            Nombre = x.Nombre,
                            TipoCampo = x.TipoCampo

                        }).ToList();
                }
                else
                {
                    var lista = db.CampoCatalogo.Where(x => x.CatalogoId == CatalogoId && LstCampos.Contains(x.Nombre.Trim()) && x.Activo).OrderBy(x => x.Orden).ToList();
                    ListaCampos =  lista.Select(x => new Campo()
                    {
                        Activo = x.Activo,
                        Ayuda = x.Ayuda,
                        CampoId = x.CatalogoId,
                        CatalogoId = x.CampoCatalogoId,
                        Etiqueta = x.Etiqueta,
                        Nombre = x.Nombre,
                        TipoCampo = x.TipoCampo

                    }).ToList();
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


        /////Datos de select nuevos
        ///



        public string getNombreUsuario(string sId = "")
        {
            string Nombre = "";
            try
            {
                var usuario = db.Users.Where(x => x.Id == sId).FirstOrDefault();
                Nombre = usuario != null ? usuario.NombreCompleto : "";

            }
            catch (Exception ex)
            {

            }

            return Nombre;

        }

        public void GenerarFiltros(int Id = 0)
        {
            try
            {
                using (ApplicationDbContext context = new ApplicationDbContext())
                {
                    var mCamposCatalogos = context.Campos.Where(x => x.PlantillaId == Id && x.Activo).ToList();
                    ViewBag.Filtros = mCamposCatalogos;
                }
            }
            catch (Exception ex)
            {
            }
        }


        //public List<VMCampos> GetSelectFromTable(int Id, int OrganismoID)
        //{
        //    ViewBag.sPrincipal = "";
        //    List<VMCampos> LstVMCampos = new List<VMCampos>();
        //    try
        //    {
        //        //var urls = Url.Action("Action", "Controller");
        //        string NombreTabla = db.Plantillas.Where(x => x.PlantillaId == Id).Select(x => x.NombreTabla).FirstOrDefault();
        //        var mCampos = db.Campos.Where(x => x.PlantillaId == Id).FirstOrDefault();
        //        //List<CampoCatalogo> Campos = GetCamposByColumn(Id);
        //        string sQuery = "";
        //        bool first = true;

        //        sQuery = $@"SELECT TablaFisicaId, Activo, FechaCreacion,UsuarioId";
        //        sQuery += $@" FROM {NombreTabla} WHERE OrganismoID={OrganismoID} ORDER BY TablaFisicaId DESC";

        //        LstVMCampos = db.Database.SqlQuery<VMCampos>(sQuery).ToList();

        //        //SqlConnection con = new SqlConnection(coneccion);
        //        //con.Open();
        //        //SqlCommand cmd = new SqlCommand(sQuery, con);
        //        //DataTable myTable = new DataTable();
        //        //myTable.Load(cmd.ExecuteReader());

        //        foreach (var item in LstVMCampos)
        //        {

        //            item.TablaNombre = NombreTabla;
        //            item.CatalogoId = Id;
        //            try
        //            {
        //                item.NombreUsuario = this.getNombreUsuario(item.UsuarioId);
        //            }
        //            catch (Exception) { }
        //            try
        //            {
        //                item.SFechaCreacion = HMTLHelperExtensions.GetFormatForSelect(TipoCampo.Fecha, item.FechaCreacion.ToString());
        //            }
        //            catch (Exception) { }


        //        }
        //        //con.Close();
        //    }
        //    catch (Exception ex)
        //    {

        //    }


        //    return LstVMCampos;
        //}

        //public IPagedList<dynamic> GetSelectFromTableAsync(int Id, int OrganismoID, FormCollection form, List<Campo> Campos = null, int PerPage = 10, int iPagina = 10, bool allFields = false)
        //{

        //    ViewBag.sPrincipal = "";
        //    List<VMCampos> LstVMCampos = new List<VMCampos>();
        //    IPagedList<dynamic> results = null;
        //    List<AuxTitlePlantillas> Titles = new List<AuxTitlePlantillas>();
        //    //agreamos los titulos de la tabla

        //    try
        //    {
        //        //var urls = Url.Action("Action", "Controller");
        //        string NombreTabla = db.Plantillas.Where(x => x.PlantillaId == Id).Select(x => x.NombreTabla).FirstOrDefault();
        //        //var mCamposCatalogos = db.Campos.Where(x => x.CatalogoId == Id && x.Principal).FirstOrDefault();
        //        //List<CampoCatalogo> Campos = GetCamposByColumn(Id);
        //        string sQuery = "";
        //        //bool first = true;
        //        //string sPrincipal = mCamposCatalogos?.Nombre;
        //        //ViewBag.sPrincipal = mCamposCatalogos?.Etiqueta;
        //        //var InnerA = "";
        //        //var InnerB = $"{sPrincipal}";
        //        //var nombreInnerBTable = "";
        //        //if (mCamposCatalogos.TipoCampo == TipoCampo.Catalogo)
        //        //{
        //        //    sPrincipal = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(mCamposCatalogos.iCatalogoId, true);
        //        //    InnerA = "a.";
        //        //    InnerB = $"b.{sPrincipal}";
        //        //    nombreInnerBTable = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(mCamposCatalogos.iCatalogoId);
        //        //}




        //        sQuery = $@"SELECT ";
        //        var Relevantes = allFields ? db.Campos.Where(x => x.PlantillaId == Id && x.Activo).ToList() : db.Campos.Where(x => x.PlantillaId == Id && x.relevantes).ToList();
        //        foreach (var relevante in Relevantes)
        //        {
        //            sQuery += $@"{relevante.Nombre},";
        //            Titles.Add(new AuxTitlePlantillas { LbNombre = relevante.Etiqueta });

        //        }

        //        sQuery += $@"'{NombreTabla}' as NombreTabla, {Id} as PlantillaId, ";
        //        sQuery += $@"TablaFisicaId, Activo";//sQuery += $@"TablaFisicaId, Activo, FechaCreacion,UsuarioId";
        //        Titles.Add(new AuxTitlePlantillas { LbNombre = "Activo" });
        //        //Titles.Add(new AuxTitlePlantillas { LbNombre = "Fecha Creación" });
        //        //Titles.Add(new AuxTitlePlantillas { LbNombre = "Usuario" });
        //        ViewBag.Titles = Titles;
        //        ViewBag.Relevantes = Relevantes;
        //        ViewBag.NombreTabla = NombreTabla;




        //        //sQuery = $@"SELECT {InnerA}TablaFisicaId as aTablaFisicaId, {InnerA}Activo as aActivo, {InnerA}FechaCreacion as aFechaCreacion, {InnerA}";
        //        //if (sPrincipal != null && sPrincipal.Length > 0)
        //        //{

        //        //    sQuery += $",{InnerB}";
        //        //}
        //        sQuery += $@" FROM {NombreTabla} a";
        //        //esta es el codigo de arriba
        //        //sQuery += $@" FROM {NombreTabla}{(mCamposCatalogos.TipoCampo == TipoCampo.Catalogo ? " a" : "")} ";

        //        //sQuery += $@" {(mCamposCatalogos.TipoCampo == TipoCampo.Catalogo ? (" LEFT JOIN " + nombreInnerBTable + $" b ON a.{mCamposCatalogos?.Nombre} = b.TablaFisicaId ") : "")} ";
        //        var SQueryWhereForRows = "";
        //        sQuery += $@" WHERE OrganismoID={OrganismoID} ";
        //        SQueryWhereForRows = $@" WHERE OrganismoID={OrganismoID} ";
        //        //where Secction
        //        if (Campos != null && Campos.Count > 0)
        //        {
        //            foreach (var item in Campos.Where(x => x.TipoCampo != TipoCampo.ArchivoAdjunto).ToList())
        //            {
        //                string valor = form[$@"{item.Nombre}"];

        //                if (valor != null && valor != "")
        //                {
        //                    var sLikeIgual = "=";

        //                    if (item.TipoCampo == TipoCampo.Texto || item.TipoCampo == TipoCampo.AreaTexto || item.TipoCampo == TipoCampo.Alfanumerico || item.TipoCampo == TipoCampo.email
        //                        || item.TipoCampo == TipoCampo.Hipervinculo || item.TipoCampo == TipoCampo.Telefono)
        //                    {
        //                        sLikeIgual = "LIKE";
        //                    }

        //                    if (item.TipoCampo == TipoCampo.Decimal || item.TipoCampo == TipoCampo.Dinero || item.TipoCampo == TipoCampo.Numerico || item.TipoCampo == TipoCampo.Porcentaje)
        //                    {
        //                        valor = valor.Replace("(", "").Replace(")", "").Replace(",", "");
        //                    }

        //                    //poneemos el where
        //                    if (item._TipoFecha == TipoFecha.FechaHasta)
        //                    {

        //                    }
        //                    else if (item._TipoFecha == TipoFecha.FechaDesde)
        //                    {

        //                    }
        //                    else
        //                    {

        //                        sQuery += $@" AND {item.Nombre} {sLikeIgual} {GetValueByCampo(item, valor, (sLikeIgual == "LIKE"))}";
        //                        SQueryWhereForRows += $@" AND {item.Nombre} {sLikeIgual} {GetValueByCampo(item, valor, (sLikeIgual == "LIKE"))}";


        //                    }

        //                }
        //            }
        //        }

        //        sQuery += $" ORDER BY FechaCreacion";

        //        sQuery += $@" offset ({iPagina} - 1) * {PerPage} rows
        //                    fetch next {PerPage} rows only;";
        //        //LstVMCampos = db.Database.SqlQuery<VMCampos>(sQuery).ToList();
        //        //LstVMCampos.Add(new VMCampos { Activo = true, CatalogoId = 1 });

        //        //TypeBuilder builder = HMTLHelperExtensions.CreateTypeBuilder(
        //        //    "MyDynamicAssembly", "MyModule", "MyType");
        //        //HMTLHelperExtensions.CreateAutoImplementedProperty(builder, "TablaFisicaId", typeof(int));
        //        //HMTLHelperExtensions.CreateAutoImplementedProperty(builder, "Activo", typeof(bool));
        //        //HMTLHelperExtensions.CreateAutoImplementedProperty(builder, "FechaCreacion", typeof(DateTime));
        //        //HMTLHelperExtensions.CreateAutoImplementedProperty(builder, "Noce", typeof(string));
        //        //Type resultType = builder.CreateType();

        //        //var nuevo = db.Database.SqlQuery<dynamic>(sQuery).ToPagedList(iPagina, PerPage);
        //        //var datos1 = "";
        //        //foreach (var item in nuevo)
        //        //{
        //        //    var dynamic = item.ToDynamic();
        //        //    datos1 += Convert.ToString(1);
        //        //    var dynamic1 = HMTLHelperExtensions.ToDynamic(item);
        //        //    Console.WriteLine("{0,10}", 1.ToString());
        //        //    //datos1 += Convert.ToString(dynamic.FechaCreacion);
        //        //    //datos1 += Convert.ToString(dynamic.TablaFisicaId);

        //        //    //Console.WriteLine("{0,10} {1,4} {2,10}", item.Activo, item.FechaCreacion, item.TablaFisicaId);
        //        //}

        //        //using (DbContext context = new DbContext(coneccion))
        //        //{
        //        //    TypeBuilder builder = HMTLHelperExtensions.CreateTypeBuilder(
        //        //        "MyDynamicAssembly", "MyModule", "MyType");
        //        //    HMTLHelperExtensions.CreateAutoImplementedProperty(builder, "Activo", typeof(bool));
        //        //    HMTLHelperExtensions.CreateAutoImplementedProperty(builder, "FechaCreacion", typeof(DateTime));
        //        //    HMTLHelperExtensions.CreateAutoImplementedProperty(builder, "TablaFisicaId", typeof(int));

        //        //    Type resultType = builder.CreateType();

        //        //    dynamic queryResult = context.Database.SqlQuery(
        //        //        resultType, sQuery);

        //        //    //Console.WriteLine("{0,20} {1,4} {2,10}", "Name", "Type", "ID");
        //        //    var datos = "";
        //        //    foreach (dynamic item in queryResult)
        //        //    {
        //        //        datos += Convert.ToString(item.Activo);
        //        //        datos += Convert.ToString(item.FechaCreacion);
        //        //        datos += Convert.ToString(item.TablaFisicaId);

        //        //        //Console.WriteLine("{0,10} {1,4} {2,10}", item.Activo, item.FechaCreacion, item.TablaFisicaId);
        //        //    }
        //        //}




        //        //var tablaid = Convert.ToInt32(results.FirstOrDefault().TablaFisicaId);
        //        //foreach (var item in results as Dictionary<string, object>)
        //        //{
        //        //    var texto = item.Key;

        //        //}
        //        //var nuevo2 = DbSet<TEntity>.SqlQuery(sQuery).ToList();


        //        ///Datos de procedure

        //        //var nuevoDato =  db.Database.SqlQuery<List<string,string>>("EXEC ExecuteSql  {0}", sQuery).ToList();
        //        //var row = new ExpandoObject() as IDictionary<string, object>;
        //        //while (nuevoDato.FirstOrDefault().Read())
        //        //{
        //        //   ;
        //        //    for (var fieldCount = 0; fieldCount < nuevoDato.FirstOrDefault().FieldCount; fieldCount++)
        //        //    {
        //        //        row.Add(nuevoDato.FirstOrDefault().GetName(fieldCount), nuevoDato.FirstOrDefault()[fieldCount]);
        //        //    }
        //        //}

        //        SqlParameter param = new SqlParameter()
        //        {
        //            ParameterName = "@sql",
        //            Value = sQuery
        //        };

        //        //SqlCommand cmd = new SqlCommand("getCCM", con);
        //        //cmd.CommandType = CommandType.StoredProcedure;

        //        //una vez terminado empezamos a insertar todos los campos
        //        SqlConnection sqlConnection = new SqlConnection(coneccion);
        //        //sqlConnection.Open();
        //        //sqlConnection.Query(sQuery);
        //        //sqlConnection.Close();
        //        sqlConnection.Open();
        //        SqlCommand command = new SqlCommand("ExecuteSql", sqlConnection);
        //        command.CommandType = CommandType.StoredProcedure;
        //        command.Parameters.Add(param);
        //        // Setting command timeout to 3000000 second
        //        command.CommandTimeout = 3000000;
        //        //var reader = command.ExecuteReader();
        //        //var dataReader = command.ExecuteReader();

        //        results = HMTLHelperExtensions.DynamicListFromSql(command).ToPagedList(1, PerPage);



        //        ///Fin datos del procedure

        //        //results = HMTLHelperExtensions.DynamicListFromSql(db, sQuery, new Dictionary<string, object>()).ToPagedList(1, PerPage);

        //        var sQueryCantidad = $"SELECT COUNT(TablaFisicaId) FROM {NombreTabla} " + SQueryWhereForRows;
        //        var Rows = db.Database.SqlQuery<int>(sQueryCantidad).FirstOrDefault();
        //        var pageCount = 0;
        //        if (Rows > 0)
        //        {
        //            var countRows = (double)Rows / PerPage;
        //            pageCount = (int)Math.Ceiling(countRows);
        //        }

        //        ViewBag.PageCount = pageCount;
        //        ViewBag.TotalCount = Rows;
        //        ViewBag.TotalCountString = Rows.ToString("#,##0");
        //        ViewBag.iPagina = iPagina;


        //        //foreach (var item in nuevo)
        //        //{
        //        //    var items = item;
        //        //    LstVMCampos.Add(new VMCampos { Activo = true, CatalogoId = 1 });
        //        //}

        //        //foreach (var item in LstVMCampos)
        //        //{
        //        //    item.TablaNombre = NombreTabla;
        //        //    item.CatalogoId = Id;
        //        //    try
        //        //    {
        //        //        item.NombreUsuario = this.getNombreUsuario(item.UsuarioId);
        //        //    }
        //        //    catch (Exception) { }
        //        //    try
        //        //    {
        //        //        item.SFechaCreacion = HMTLHelperExtensions.GetFormatForSelect(TipoCampo.Fecha, item.FechaCreacion.ToString());
        //        //    }
        //        //    catch (Exception) { }
        //        //}
        //    }
        //    catch (Exception ex)
        //    {

        //    }


        //    return results;
        //}

        public vmResultadoDatos2 GetSelectFromTableAsync(int iPlantillaId, int iOrganismoId, FormCollection form, List<Campo> Campos = null, int iPorPagina = 10, int iPagina = 10, bool bRelevante = false, int iPeriodo = 0)
        {

            var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
            vmResultadoDatos2 resultado = new vmResultadoDatos2();
            List<AuxTitlePlantillas> Titulos = new List<AuxTitlePlantillas>();
            try
            {
                var sYear = "''";
                if (iPagina == 0)
                {
                    iPagina = 1;
                }
                if (iPeriodo == 0)
                {
                    sYear = $@"'{DateTime.Now.Year.ToString()}'";
                }
                vmPlantillaCampos campos = HMTLHelperExtensions.GetCamposForDatos(iPlantillaId, !bRelevante);
                if (campos != null && campos.campos.Count > 0)
                {
                    string NombreTabla = campos.NombreTabla;
                    //paginador
                    string Query = $@"SELECT 
                             {iPlantillaId} as PlantillaId,
                             '{NombreTabla}' as NombreTabla,
                             count(a.TablaFisicaId) totalRegistros,
                             ((count(a.TablaFisicaId) + {iPorPagina} - 1) / {iPorPagina}) totalPaginas,
                             {iPagina} paginaActual, ";

                    //Datos
                    Query += $@"(SELECT h.TablaFisicaId, h.Activo
								 ,((";
                    //campos
                    Query += $@"SELECT NombreCampo, Valor, CASE ";
                    string QuerCatalogo = "";
                    //PAARA LOS CASES DE CAMPOS
                    string SubQueryCaseEsTabla = " CASE WHEN ";
                    bool hayTabla = false;
                    bool bSubQueryCaseEsTabla = false;
                    //PARA EL FORM DEL CAMPO
                    string SubQueryCampo = $@" FROM (SELECT TOP 1 CAST({NombreTabla}.TablaFisicaId AS nvarchar) as TablaFisicaId, CAST({NombreTabla}.Activo AS nvarchar) as Activo ";
                    //bool bSubQueryCampo = false;
                    string sUpivot = $@" UNPIVOT(Valor FOR NombreCampo IN( ";
                    bool firstUpivt = true;
                    List<string> lstInnerJoin = new List<string>();
                    //UNPIVOT(Valor FOR NombreCampo IN(a.TablaFisicaId, a.Ejercicio, a.Fecha_Inicio_Periodo_Informa)) AS unp  FOR JSON PATH
                    foreach (var item in campos.campos.Where(x => x.Nombre != "TablaFisicaId").ToList())
                    {
                        Titulos.Add(new AuxTitlePlantillas { LbNombre = item.Etiqueta });
                        Query += $@" WHEN NombreCampo = '{item.Nombre}' THEN {item.TipoCampo.GetHashCode()} ";
                        sUpivot += $@"{(!firstUpivt ? "," : "")} a.{item.Nombre}";
                        firstUpivt = false;
                        if (item.TipoCampo == Transparencia.Models.TipoCampo.Catalogo)
                        {
                            if (!item.TablaCatalogo)
                            {
                                string NombreTablaCatalogo = item.NombreTablaCatalogo;


                                //SubQueryCampo += $@", CAST({NombreTablaCatalogo}.{item.NombreCatalogo} AS nvarchar) as {item.Nombre} ";
                                SubQueryCampo += $@", CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTablaCatalogo}.{item.NombreCatalogo} AS nvarchar(MAX)),{item.TipoCampoCatalogo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX)) as {item.Nombre}  ";
                                if (!lstInnerJoin.Any(x => x == NombreTablaCatalogo))
                                {
                                    QuerCatalogo += $" LEFT JOIN {NombreTablaCatalogo} ON {NombreTabla}.{item.Nombre} = {NombreTablaCatalogo}.TablaFisicaId ";
                                    lstInnerJoin.Add(NombreTablaCatalogo);
                                }

                            }
                            else
                            {
                                //SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTabla}.{item.Nombre} AS nvarchar),{item.TipoCampo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as nvarchar)  as {item.Nombre} ";
                                SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({item.CatalogoId} AS  nvarchar(MAX)),100,{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX))  as {item.Nombre} ";
                                SubQueryCaseEsTabla += $@" {(bSubQueryCaseEsTabla ? "OR" : "")} NombreCampo = '{item.Nombre}' ";
                                bSubQueryCaseEsTabla = true;
                                hayTabla = true;
                            }
                        }
                        else
                        {
                            SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTabla}.{item.Nombre} AS  nvarchar(MAX)),{item.TipoCampo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX))  as {item.Nombre} ";
                        }
                    }
                    Query += " END AS TipoCampo, ";
                    SubQueryCaseEsTabla += $@" THEN 1 ELSE 0 END AS EsTabla ";
                    Query += hayTabla ? SubQueryCaseEsTabla : " 0 AS EsTabla ";
                    SubQueryCampo += $@" FROM {NombreTabla} {QuerCatalogo} WHERE {NombreTabla}.TablaFisicaId = h.TablaFisicaId) a";
                    Query += SubQueryCampo;

                    sUpivot += $@")) AS unp  FOR JSON PATH ) ) AS campos";
                    Query += sUpivot;

                    //Where Section
                    string sWhere = "";
                    if (Campos != null && campos.campos.Count > 0)
                    {
                        foreach (var item in Campos.Where(x => x.TipoCampo != TipoCampo.ArchivoAdjunto).ToList())
                        {
                            string valor = form[$@"{item.Nombre}"];

                            if (valor != null && valor != "")
                            {
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

                                }
                                else if (item._TipoFecha == TipoFecha.FechaDesde)
                                {

                                }
                                else
                                {

                                    sWhere += $@" AND {item.Nombre} {sLikeIgual} {GetValueByCampo(item, valor, (sLikeIgual == "LIKE"))}";
                                    //SQueryWhereForRows += $@" AND {item.Nombre} {sLikeIgual} {GetValueByCampo(item, valor, (sLikeIgual == "LIKE"))}";


                                }

                            }
                        }
                    }
                    var PeriodoId = form["PeriodoId"];
                    var sysFrecuencia = form["sysFrecuencia"];
                    var sysNumFrecuencia = form["sysNumFrecuencia"];
                    //if (PeriodoId  != null && PeriodoId.Length > 0)
                    //{
                    //    iPeriodo = Convert.ToInt32(PeriodoId);
                    //}
                    //else
                    //{
                    //    iPeriodo = db.Periodos.Where(x => x.Activo).OrderBy(x => x.Orden).First().PeriodoId;
                    //}
                    if ( PeriodoId.Length > 0)
                    {
                        iPeriodo = Convert.ToInt32(PeriodoId);
                    }

                    var Enlace = HMTLHelperExtensions.GetRoles(User.Identity.Name, "Enlace");


                    //h.Activo=1 AND 
                    Query += $@" FROM {NombreTabla} h
                                 LEFT JOIN Periodoes p ON h.PeriodoId = p.PeriodoId
                                 WHERE h.OrganismoID={iOrganismoId} AND h.Activo=1
                                 AND
                                 ({iPeriodo} = 0 OR p.PeriodoId = {iPeriodo} )
                                 {(!Enlace ? $" AND (UsuarioId = '{usuario.Id}')  " : "")}
                                {(sysFrecuencia != null && sysFrecuencia.Length > 0 && sysFrecuencia != "0" ? $" AND ( h.sysFrecuencia = {sysFrecuencia} ) " : "")}
                                {(sysNumFrecuencia != null && sysNumFrecuencia.Length > 0 && sysNumFrecuencia != "0"  ? $" AND ( h.sysNumFrecuencia = {sysNumFrecuencia} ) " : "")}
                                {sWhere} 
                                 ORDER BY h.FechaCreacion
		                                OFFSET (({iPagina} * {iPorPagina}) - {iPorPagina}) ROWS FETCH NEXT {iPorPagina} ROWS ONLY
                                 FOR JSON PATH ) as datos ";

                    //endpaginador
                    var chosen = this.onlyChosen ? " a.Chosen = 1 " : "a.Chosen IS NULL OR a.Chosen = 0";
                    Query += $@"FROM {NombreTabla} a 
	                            LEFT JOIN Periodoes p ON a.PeriodoId = p.PeriodoId
                          WHERE a.Activo=1 AND a.OrganismoID={iOrganismoId} 
                          AND {chosen}
                          AND
                            ({iPeriodo} = 0 OR p.PeriodoId = {iPeriodo} )
                          {(!Enlace ? $" AND (UsuarioId = '{usuario.Id}')  " : "")}
                          {(sysFrecuencia != null && sysFrecuencia.Length > 0 && sysFrecuencia != "0" ? $" AND ( a.sysFrecuencia = {sysFrecuencia} ) " : "")}
                          {(sysNumFrecuencia != null && sysNumFrecuencia.Length > 0 && sysNumFrecuencia != "0" ? $" AND ( a.sysNumFrecuencia = {sysNumFrecuencia} ) " : "")}
                          {sWhere} ";

                    




                    //consultamos los datos
                    SqlMapper.AddTypeHandler(new vmRowDatosHandler());
                    SqlMapper.AddTypeHandler(new vmcampoDatosHandler());
                    using (var idb = db.Database.Connection)
                    {
                        resultado = idb.Query<vmResultadoDatos2>(
                              Query
                              ).FirstOrDefault();

                        SqlMapper.ResetTypeHandlers();
                    }
                   
                }
            }
            catch (Exception ex)
            {
                string var = ex.Message;

            }
            ViewBag.Titles = Titulos;
            return resultado;
        }

        public ActionResult IndexDatosPlantillas(FormCollection form, bool allFields = false, int id = 0, int PerPage = 10, int iPagina = 1)
        {

            if (id == 0)
            {
                return RedirectToAction("Index", "Plantillas");
            }
            var OrganismoId = GetOrganismoEnlace();
            
            var lista = GetSelectFromTableAsync(id, (int)OrganismoId, form, GetCamposByColumn(id), PerPage, iPagina, allFields);
           
            if (Request.IsAjaxRequest())
            {
                return PartialView("_listaDatosPlantillas", lista);
            }
            GenerarFiltros(id);
            SetFrecuencia(id);
            ViewBag.iId = id;
            ViewBag.NombrePlantilla = HMTLHelperExtensions.GetLeyArtiucloFraccionFromPlantilla(id);
            return View(lista);
        }

        [HttpPost]
        public ActionResult IndexDatosPlantillasPOST(FormCollection form, bool allFields = false, int iId = 0, int PerPage = 10, int iPagina = 1)
        {
            var OrganismoId = GetOrganismoEnlace();
            
            var lista = GetSelectFromTableAsync(iId, (int)OrganismoId, form, GetCamposByColumn(iId), PerPage, iPagina, allFields);


            if (Request.IsAjaxRequest())
            {
                return PartialView("_listaDatosPlantillas", lista);
            }
            GenerarFiltros(iId);
            SetFrecuencia(iId);
            ViewBag.iId = iId;
            ViewBag.NombrePlantilla = HMTLHelperExtensions.GetLeyArtiucloFraccionFromPlantilla(iId);
            return View(lista);
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

        public void CheckIndexes(string NombreTabla, List<Campo> campos)
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
            }
            catch (Exception ex)
            {
            }

        }


        public bool? CambiarStatusCampo(int id = 0)
        {
            var activo = false;
            if (id == 0)
            {
                return null;
            }
            var Model = db.Campos.Where(X => X.CampoId == id).FirstOrDefault();
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


        #endregion


        #region CamposUpdate
        //updateCatalogos
        [HttpPost]
        public ActionResult GetCampoById(int iId = 0)
        {
            if (iId == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el campo." }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var mCampo = db.Campos.Where(x => x.CampoId == iId).FirstOrDefault();

                if (mCampo != null)
                {
                    var json = Json(new { Hecho = true, Campo = mCampo }, JsonRequestBehavior.AllowGet);
                    //configuramos mas length para que no nos den error de longitudx
                    json.MaxJsonLength = 500000000;
                    return json;
                    //return Json(new { Hecho = true, Campo = mCampo }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Hecho = false, Mensaje = $"No se encontro el campo, intentelo de nuevo." }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = $"Ocurrio un error,Error: '{ex.Message}' " }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult AjaxModificarCampo(LstCampos tblCampoDinamico, int iIdA = 0, bool yaLoValido = false)
        {

            if (iIdA == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener el Catalogo." }, JsonRequestBehavior.AllowGet);
            }
            var mCampoNuevo = new Campo();
            var mCampoViejo = db.Campos.Where(x => x.CampoId == tblCampoDinamico.CampoId).FirstOrDefault();
            var oldModel = (Campo)mCampoViejo.Clone();
            if (mCampoViejo == null)
            {
                return Json(new { Hecho = false, Mensaje = "No podimos encontrar el campo intentelo de nuievo." }, JsonRequestBehavior.AllowGet);
            }
            //verificamos el campo viejo
            mCampoViejo.Nombre = GenerarNombreCampos(mCampoViejo.Nombre, iIdA, tblCampoDinamico.CampoId);

            mCampoNuevo.Ayuda = tblCampoDinamico.AddAyuda;
            mCampoNuevo._ConDecimales = tblCampoDinamico.AddConDecimales;
            mCampoNuevo.Etiqueta = tblCampoDinamico.AddEtiqueta;
            mCampoNuevo._GrupoExtensionId = tblCampoDinamico.AddGrupoExtensionId;
            mCampoNuevo.Longitud = tblCampoDinamico.AddLongitud;
            mCampoNuevo.Nombre = tblCampoDinamico.AddNombre = GenerarNombreCampos(tblCampoDinamico.AddNombre, iIdA, tblCampoDinamico.CampoId);
            mCampoNuevo.Requerido = tblCampoDinamico.AddRequerido;
            mCampoNuevo._Size = tblCampoDinamico.AddSize;
            mCampoNuevo.TipoCampo = (TipoCampo)tblCampoDinamico.AddTipoCampo;
            mCampoNuevo._TipoFecha = (TipoFecha)tblCampoDinamico.AddTipoFecha;
            mCampoNuevo._TipoFecha = (TipoFecha)tblCampoDinamico.AddTipoFecha;
            mCampoNuevo._ConDecimales = tblCampoDinamico.AddConDecimales;
            mCampoNuevo._GrupoExtensionId = tblCampoDinamico.AddGrupoExtensionId;
            mCampoNuevo._Size = tblCampoDinamico.AddSize;
            mCampoNuevo.CatalogoId = tblCampoDinamico.AddCatalogoId;
            mCampoNuevo.relevantes = tblCampoDinamico.AddRelevante;
            mCampoNuevo.IdCampoPNT = tblCampoDinamico.AddCampoPNT;
            mCampoNuevo.IdTipoCampoPNT = tblCampoDinamico.AddTipoCampoPNT;
            if(tblCampoDinamico.AddOrdenSeleccion.HasValue)
                mCampoNuevo.OrdenSeleccionPublico = (OrdenSeleccionPublico)tblCampoDinamico.AddOrdenSeleccion.Value;
            try
            {
                var Respuesta = HMTLHelperExtensions.GetInfoOfUpdate(mCampoViejo, mCampoNuevo);
                if (Respuesta.Length > 0)
                {
                    if (yaLoValido)
                    {
                        Respuesta = HMTLHelperExtensions.RemoveInformationFromTable(mCampoViejo, mCampoNuevo);
                        if (Respuesta.Length > 0)
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
                mCampoViejo.CatalogoId = tblCampoDinamico.AddCatalogoId;
                mCampoViejo.relevantes = tblCampoDinamico.AddRelevante;
                mCampoViejo.IdCampoPNT = tblCampoDinamico.AddCampoPNT;
                mCampoViejo.IdTipoCampoPNT = tblCampoDinamico.AddTipoCampoPNT;
                if(tblCampoDinamico.AddOrdenSeleccion.HasValue)
                    mCampoViejo.OrdenSeleccionPublico = (OrdenSeleccionPublico)tblCampoDinamico.AddOrdenSeleccion.Value;


                db.Entry(mCampoViejo).State = EntityState.Modified;
                db.SaveChanges();

                //Bitacora
                this.CreateBitacora(oldModel, mCampoViejo, mCampoViejo.CampoId);


                string viewContent = ConvertViewToString("_ListaCampos", GetCampos(iIdA));
                return Json(new { Hecho = true, Respuesta = Respuesta, Mensaje = "se guardo exitosamente", Partial = viewContent }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult AjaxEliminarCampo(int iCampoId = 0, bool yaLoValido = false)
        {

            if (iCampoId == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener la Plantilla." }, JsonRequestBehavior.AllowGet);
            }


            try
            {

                var campo = db.Campos.Where(x => x.CampoId == iCampoId).FirstOrDefault();
                var Registros = HMTLHelperExtensions.GetInfoOfRemove(campo);
                //verifiicamos si tiene error
                if (Registros.Length > 0 && !yaLoValido)
                {

                    return Json(new { Hecho = true, Respuesta = Registros }, JsonRequestBehavior.AllowGet);

                }
                if (campo != null)
                {
                    db.Campos.Remove(campo);
                    db.SaveChanges();
                    //Bitacora
                    this.CreateBitacora(campo, null, 0);

                    var PlantillaNombre = db.Plantillas.Where(x => x.PlantillaId == campo.PlantillaId).Select(x => x.NombreTabla).FirstOrDefault();
                    string Respuesta = this.RemoveColumToTable(PlantillaNombre, campo.Nombre);
                }
                return Json(new { Hecho = true, Mensaje = "Se elimino el registro", Respuesta = Registros }, JsonRequestBehavior.AllowGet);

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
                var mCampo = db.Campos.Where(x => x.CampoId == iId).FirstOrDefault();

                if (mCampo != null)
                {
                    if (mCampo.TipoCampo == TipoCampo.Catalogo)
                    {
                        var mCatalago = db.Catalogoes.Where(x => x.CatalogoId == mCampo.CatalogoId).FirstOrDefault();
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


                    string viewContent = ConvertViewToString("_ConfigurarCamposDetails", mCampo);
                    return Json(new { Hecho = true, Partial = viewContent }, JsonRequestBehavior.AllowGet);

                }

                return Json(new { Hecho = false, Mensaje = $"No se encontro el campo, intentelo de nuevo." }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = $"Ocurrio un error,Error: '{ex.Message}' " }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult GeListDownload(int PlantillaId = 0)
        {
            ViewBag.iPlantillaId = PlantillaId;
            if (PlantillaId == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Ocurrio un error al momento de obtener la plantilla." }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var mPlantilla = db.Plantillas.Where(x => x.PlantillaId == PlantillaId).FirstOrDefault();

                if (mPlantilla != null && mPlantilla.NombreTabla.Length > 0)
                {
                    var totalRegistros = HMTLHelperExtensions.getTotalRowsPlantilla(mPlantilla.NombreTabla, usuario.OrganismoID.HasValue ? usuario.OrganismoID.Value : 0, usuario.Id);
                    var Listado = HMTLHelperExtensions.getListadoToDownload(totalRegistros);

                    string viewContent = ConvertViewToString("_ListaDownload", Listado);
                    return Json(new { Hecho = true, Partial = viewContent }, JsonRequestBehavior.AllowGet);

                }

                return Json(new { Hecho = false, Mensaje = $"No se encontro la plantilla, intentelo de nuevo." }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = $"Ocurrio un error,Error: '{ex.Message}' " }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult ExcelDatos(int iPlantillaId = 0, string addRango = "", int Tipo = 0)
        {
            var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();

            string[] Rangos = addRango != "" ? addRango.Split(',') : new string[0];


            var mPlantilla = db.Plantillas.Where(x => x.PlantillaId == iPlantillaId).FirstOrDefault();
            var NombreTabla = mPlantilla != null ? mPlantilla.NombreTabla : "";
            var lstCampos = db.Campos.Where(x => x.PlantillaId == iPlantillaId && x.Activo).ToList();

            //return GenerarPlantillaExcelInformacion(lstCampos, mPlantilla, NombreTabla, Rangos, Tipo);
            return GenerarPlantillaExcelInformacion(iPlantillaId, usuario.OrganismoID.HasValue ? usuario.OrganismoID.Value : 0, Rangos, Tipo, usuario.Id);
        }


        //public ActionResult GenerarPlantillaExcelInformacionPublico(int iPlantillaId, int iOrganismoId,int periodo)
        //{

        //    vmPlantillaCampos campos = HMTLHelperExtensions.GetCamposForExcel(iPlantillaId);
        //    List<vmExcelTabs> listaTabs = new List<vmExcelTabs>();
        //    int Row = 1;
        //    var FontSize = 11;
        //    // Generate the workbook...
        //    var workbook = new XLWorkbook();
        //    string nombreDoc = "";
        //    if (campos != null)
        //    {
        //        nombreDoc = "Reporte de Formatos";
        //        var workbooksheets = workbook.AddWorksheet(nombreDoc);
        //        workbooksheets.Cell($"A{Row}").SetValue(campos.IdPlantillaPNT.ToString());
        //        workbooksheets.Row(Row).Height = 0;
        //        Row++;

        //        //TITULOS
        //        workbooksheets.Range($"A{Row}:C{Row}")
        //                                 .Merge()
        //                                 .SetValue("TITULO")
        //                                 .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
        //                                 .Font.SetFontSize(FontSize)
        //                                 .Font.SetBold(true);

        //        workbooksheets.Range($"D{Row}:F{Row}")
        //                                .Merge()
        //                                .SetValue("NOMBRE CORTO")
        //                                .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
        //                                .Font.SetFontSize(FontSize);

        //        workbooksheets.Range($"G{Row}:I{Row}")
        //                               .Merge()
        //                               .SetValue("DESCRIPCIÓN")
        //                               .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
        //                               .Font.SetFontSize(FontSize);
        //        //Estilo
        //        workbooksheets.Range($"A{Row}:I{Row}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
        //        workbooksheets.Range($"A{Row}:I{Row}").Style
        //                  .Font.SetFontColor(XLColor.White)
        //                  .Border.SetBottomBorder(XLBorderStyleValues.Thin)
        //                  .Border.SetTopBorder(XLBorderStyleValues.Thin)
        //                  .Border.SetLeftBorder(XLBorderStyleValues.Thin)
        //                  .Border.SetRightBorder(XLBorderStyleValues.Thin)
        //                  .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
        //                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        //        Row++;
        //        workbooksheets.Range($"A{Row}:C{Row}")
        //                           .Merge()
        //                           .SetValue(campos.NombreLargo)
        //                           .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
        //                           .Font.SetFontSize(FontSize);

        //        workbooksheets.Range($"D{Row}:F{Row}")
        //                           .Merge()
        //                           .SetValue(campos.NombreCorto)
        //                           .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
        //                           .Font.SetFontSize(FontSize);

        //        workbooksheets.Range($"G{Row}:I{Row}")
        //                               .Merge()
        //                               .SetValue(campos.Ayuda)
        //                               .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
        //                               .Font.SetFontSize(FontSize);
        //        workbooksheets.Range($"A{Row}:I{Row}").Style
        //            .Font.SetFontColor(XLColor.Black)
        //            .Border.SetBottomBorder(XLBorderStyleValues.Thin)
        //            .Border.SetTopBorder(XLBorderStyleValues.Thin)
        //            .Border.SetLeftBorder(XLBorderStyleValues.Thin)
        //            .Border.SetRightBorder(XLBorderStyleValues.Thin)
        //            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
        //            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
        //        workbooksheets.Range($"A{Row}:I{Row}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
        //        Row++;
        //        //iniciamos column para usarlo en las configuraciones
        //        var column = 1;

        //        //Ponemos los tipo de Campos PNT
        //        if (campos.campos.Count > 0)
        //        {
        //            foreach (var item in campos.campos)
        //            {

        //                workbooksheets.Range(Row, column, Row, column)
        //                  .Merge()
        //                  .SetValue(item.IdTipoCampoPNT.ToString());
        //                column++;

        //            }
        //        }
        //        //ajustamos la row en 0 para que no se vea
        //        workbooksheets.Row(Row).Height = 0;
        //        Row++;
        //        column = 1;
        //        //Ponemos el id de los campos PNT
        //        if (campos.campos.Count > 0)
        //        {
        //            foreach (var item in campos.campos)
        //            {

        //                workbooksheets.Range(Row, column, Row, column)
        //                  .Merge()
        //                  .SetValue(item.IdCampoPNT.ToString());
        //                column++;

        //            }
        //        }
        //        //ajustamos la row en 0 para que no se vea
        //        workbooksheets.Row(Row).Height = 0;
        //        Row++;
        //        column = 1;
        //        //Row sin texto gray dark
        //        workbooksheets.Range(Row, 1, Row, campos.campos.Count)
        //                              .Merge()
        //                              .SetValue("")
        //                              .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
        //                              .Font.SetFontSize(FontSize)
        //                              .Font.SetBold(true);
        //        workbooksheets.Range(Row, 1, Row, campos.campos.Count).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
        //        Row++;
        //        //Ponemos los nombre de los campos
        //        if (campos.campos.Count > 0)
        //        {
        //            foreach (var item in campos.campos)
        //            {

        //                workbooksheets.Range(Row, column, Row, column)
        //                  .Merge()
        //                  .SetValue(item.Etiqueta);
        //                column++;
        //                workbooksheets.Columns(column, column).Width = 30;
        //            }
        //        }
        //        workbooksheets.Range(Row, 1, Row, (column - 1)).Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
        //        workbooksheets.Range(Row, 1, Row, (column - 1)).Style.Alignment.WrapText = true;
        //        //STILO DE LA TABLA
        //        workbooksheets.Range(Row, 1, Row, (column - 1)).Style
        //                     .Font.SetFontSize(FontSize)
        //                     .Font.SetFontColor(XLColor.Black)
        //                     //.Fill.SetBackgroundColor(XLColor.Cream)
        //                     .Border.SetBottomBorder(XLBorderStyleValues.Thin)
        //                     .Border.SetTopBorder(XLBorderStyleValues.Thin)
        //                     .Border.SetLeftBorder(XLBorderStyleValues.Thin)
        //                     .Border.SetRightBorder(XLBorderStyleValues.Thin)
        //                     .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
        //                     .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);


        //        ///////creamos los tab de los catalogos o tablas///////
        //        var ContadorSheet = 1;

        //        foreach (var item in campos.campos.Where(x => x.TipoCampo == TipoCampo.Catalogo).ToList())
        //        {
        //            var RowCatalogo = 0;
        //            //var NombreTabla = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(item.CatalogoId);
        //            //var NombreColumna = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(item.CatalogoId, true);
        //            List<RespuestaQuery> LstCatalogo = HMTLHelperExtensions.GetListCatalogos(item.NombreTablaCatalogo, item.NombreCampoCatalago);
        //            //var mCatalogo = db.Catalogoes.Where(x => x.CatalogoId == item.CatalogoId).FirstOrDefault();
        //            //var NombreCatalogo = mCatalogo != null ? mCatalogo.Nombre : "No especificado";
        //            //Empezamos los demas hojas
        //            var sCATSheet = $"CAT{ContadorSheet}";
        //            var ws = workbook.Worksheets.Add(sCATSheet);
        //            //var ws1 = workbook.Worksheets.Where(x => x.Name == "CAT{ContadorSheet}").FirstOrDefault();





        //            //subtitulos de los catalogos
        //            RowCatalogo++;
        //            if (item.TablaCatalogo)
        //            {

        //                //var campos = db.CampoCatalogo.Where(x => x.CatalogoId == item.CatalogoId && x.Activo).ToList();
        //                if (item.camposTabla.Count > 0)
        //                {
        //                    int iColum = 1;
        //                    int iCountCampos = item.camposTabla.Count;
        //                    ws.Range(RowCatalogo, iColum, RowCatalogo, iCountCampos)
        //                                                   .Merge()
        //                                                   .SetValue($"CATALOGO: {item.NombreCatalogo}")
        //                                                   .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
        //                                                   .Font.SetFontSize(FontSize)
        //                                                   .Font.SetBold(true);
        //                    ws.Range(RowCatalogo, iColum, RowCatalogo, iCountCampos).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
        //                    ws.Range(RowCatalogo, iColum, RowCatalogo, iCountCampos).Style
        //                              .Font.SetFontColor(XLColor.White)
        //                              .Border.SetBottomBorder(XLBorderStyleValues.Thin)
        //                              .Border.SetTopBorder(XLBorderStyleValues.Thin)
        //                              .Border.SetLeftBorder(XLBorderStyleValues.Thin)
        //                              .Border.SetRightBorder(XLBorderStyleValues.Thin)
        //                              .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
        //                              .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        //                    RowCatalogo = 4;
        //                    iColum--;

        //                    foreach (var camposCatalogo in item.camposTabla)
        //                    {
        //                        iColum++;
        //                        ws.Range(2, iColum, 2, iColum)
        //                        .Merge()
        //                        .SetValue(camposCatalogo.IdTipoCampoPNT.ToString());
        //                        ws.Range(3, iColum, 3, iColum)
        //                          .Merge()
        //                          .SetValue(camposCatalogo.IdCampoPNT.ToString());

        //                        ws.Range(RowCatalogo, iColum, RowCatalogo, iColum)
        //                                  .Merge()
        //                                  .SetValue($"{camposCatalogo.Etiqueta}")
        //                                  .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
        //                                  .Font.SetFontSize(FontSize)
        //                                  .Font.SetBold(true);


        //                    }


        //                    ws.Range(RowCatalogo, 1, RowCatalogo, iColum).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
        //                    ws.Range(RowCatalogo, 1, RowCatalogo, iColum).Style
        //                              .Font.SetFontColor(XLColor.White)
        //                              .Border.SetBottomBorder(XLBorderStyleValues.Thin)
        //                              .Border.SetTopBorder(XLBorderStyleValues.Thin)
        //                              .Border.SetLeftBorder(XLBorderStyleValues.Thin)
        //                              .Border.SetRightBorder(XLBorderStyleValues.Thin)
        //                              .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
        //                              .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        //                    ws.Columns(1, iColum).Width = 30;
        //                    //Lista de catalaogos
        //                    listaTabs.Add(new vmExcelTabs { catalogoId = item.CatalogoId, NombreWorksheets = sCATSheet, Columns = iColum, NumberWorksheets = ContadorSheet + 1, Row = 4 });

        //                    ws.Row(2).Height = 0;
        //                    ws.Row(3).Height = 0;

        //                }


        //            }
        //            else
        //            {
        //                ws.Range($"A{RowCatalogo}:B{RowCatalogo}")
        //                                                .Merge()
        //                                                .SetValue($"CATALOGO: {item.NombreCatalogo}")
        //                                                .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
        //                                                .Font.SetFontSize(FontSize)
        //                                                .Font.SetBold(true);
        //                ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
        //                ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style
        //                          .Font.SetFontColor(XLColor.White)
        //                          .Border.SetBottomBorder(XLBorderStyleValues.Thin)
        //                          .Border.SetTopBorder(XLBorderStyleValues.Thin)
        //                          .Border.SetLeftBorder(XLBorderStyleValues.Thin)
        //                          .Border.SetRightBorder(XLBorderStyleValues.Thin)
        //                          .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
        //                          .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        //                ws.Range($"A{RowCatalogo}")
        //                                   .Merge()
        //                                   .SetValue($"No.")
        //                                   .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
        //                                   .Font.SetFontSize(FontSize)
        //                                   .Font.SetBold(true);
        //                ws.Range($"B{RowCatalogo}")
        //                                  .Merge()
        //                                  .SetValue($"Valor")
        //                                  .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
        //                                  .Font.SetFontSize(FontSize)
        //                                  .Font.SetBold(true);


        //                ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
        //                ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style
        //                          .Font.SetFontColor(XLColor.White)
        //                          .Border.SetBottomBorder(XLBorderStyleValues.Thin)
        //                          .Border.SetTopBorder(XLBorderStyleValues.Thin)
        //                          .Border.SetLeftBorder(XLBorderStyleValues.Thin)
        //                          .Border.SetRightBorder(XLBorderStyleValues.Thin)
        //                          .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
        //                          .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        //                //Campos valores
        //                RowCatalogo++;
        //                var contador = 1;
        //                foreach (var campoCatalogo in LstCatalogo)
        //                {

        //                    ws.Range(RowCatalogo, 1, RowCatalogo, 1)
        //                     .Merge()
        //                     .SetValue(contador);
        //                    column++;
        //                    ws.Columns(column, column).Width = 30;

        //                    ws.Range(RowCatalogo, 2, RowCatalogo, 2)
        //                     .Merge()
        //                     .SetValue(campoCatalogo.Valor);
        //                    column++;
        //                    ws.Columns(column, column).Width = 30;
        //                    RowCatalogo++;
        //                    contador++;


        //                }
        //                RowCatalogo = (RowCatalogo - 1);
        //                if (RowCatalogo >= 2)
        //                {
        //                    ws.Range(2, 1, RowCatalogo, 2).Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
        //                    ws.Range(2, 1, RowCatalogo, 2).Style.Alignment.WrapText = true;
        //                    //STILO DE LA TABLA
        //                    ws.Range(2, 1, RowCatalogo, 2).Style
        //                                 .Font.SetFontSize(FontSize)
        //                                 .Font.SetFontColor(XLColor.Black)
        //                                 //.Fill.SetBackgroundColor(XLColor.Cream)
        //                                 .Border.SetBottomBorder(XLBorderStyleValues.Thin)
        //                                 .Border.SetTopBorder(XLBorderStyleValues.Thin)
        //                                 .Border.SetLeftBorder(XLBorderStyleValues.Thin)
        //                                 .Border.SetRightBorder(XLBorderStyleValues.Thin)
        //                                 .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
        //                                 .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        //                }
        //                ws.Columns(1, 2).Width = 30;
        //            }
        //            ContadorSheet++;
        //        }
        //        // poner los datos que necesitamos


        //        //Llenado
        //        column = 1;
        //        var inicio = Rangos.Count() > 0 ? Convert.ToInt32(Rangos[0]) : 0;
        //        var fin = Rangos.Count() > 0 ? Convert.ToInt32(Rangos[1]) : 0;
        //        var datos = this.GetDatosSelectDonwload(campos, iOrganismoId, 0, inicio, fin);
        //        var datosCatalogos = this.GetDatosSelectDonwloadTabla(listaTabs, iOrganismoId);

        //        foreach (var item in datos)
        //        {
        //            Row++;
        //            column = 1;
        //            foreach (var valores in item.campos)
        //            {
        //                string valorCatalgoId = "0";

        //                if (valores.EsTabla)
        //                {
        //                    var iCatalagoId = Convert.ToInt32(valores.Valor);
        //                    var Tab = listaTabs.Where(x => x.catalogoId == iCatalagoId).FirstOrDefault();
        //                    var valorTabla = datosCatalogos.Where(x => x.catalogoId == iCatalagoId && x.Idregistro == item.TablaFisicaId).ToList();
        //                    if (Tab != null)
        //                    {
        //                        var ws2 = workbook.Worksheet(Tab.NumberWorksheets);
        //                        //buscamos en los datos
        //                        foreach (var itemCatalago in valorTabla)
        //                        {

        //                            Tab.Row = Tab.Row + 1;
        //                            var iColumnCatalago = 1;
        //                            //buscamos en los campos de datos
        //                            foreach (var valoresCatalago in itemCatalago.campos)
        //                            {
        //                                if (valoresCatalago.NombreCampo.ToLower().Trim() == "id")
        //                                {
        //                                    valorCatalgoId = valoresCatalago.Valor;
        //                                }
        //                                ws2.Cell(Tab.Row, iColumnCatalago).SetValue(valoresCatalago.Valor).GetFormattedString();
        //                                iColumnCatalago++;
        //                            }
        //                        }


        //                    }
        //                    workbooksheets.Cell(Row, column).SetValue(valorCatalgoId).GetFormattedString();


        //                }
        //                else
        //                {
        //                    workbooksheets.Cell(Row, column).SetValue(valores.Valor).GetFormattedString();

        //                }
        //                column++;

        //            }modulo
        //        }
        //        //STILO DE LA TABLA
        //        workbooksheets.Range(5, 1, Row, (column - 1)).Style
        //                     .Font.SetFontSize(FontSize)
        //                     .Font.SetFontColor(XLColor.Black)
        //                     //.Fill.SetBackgroundColor(XLColor.Cream)
        //                     .Border.SetBottomBorder(XLBorderStyleValues.Thin)
        //                     .Border.SetTopBorder(XLBorderStyleValues.Thin)
        //                     .Border.SetLeftBorder(XLBorderStyleValues.Thin)
        //                     .Border.SetRightBorder(XLBorderStyleValues.Thin)
        //                     .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
        //                     .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        //    }
        //    return new ExcelResult(workbook, nombreDoc.ToString());
        //}

        public ActionResult GenerarPlantillaExcelInformacion(int iPlantillaId, int iOrganismoId, string[] Rangos, int Tipo,string sUsuarioId)
        {
            var Enlace = HMTLHelperExtensions.GetRoles(User.Identity.Name, "Enlace");
            vmPlantillaCampos campos = HMTLHelperExtensions.GetCamposForExcel(iPlantillaId);
            List<vmExcelTabs> listaTabs = new List<vmExcelTabs>();
            int Row = 1;
            var FontSize = 11;
            // Generate the workbook...
            var workbook = new XLWorkbook();
            string nombreDoc = "";
            if (campos != null)
            {
                nombreDoc = "Reporte de Formatos";
                var workbooksheets = workbook.AddWorksheet(nombreDoc);
                workbooksheets.Cell($"A{Row}").SetValue(campos.IdPlantillaPNT.ToString());
                workbooksheets.Row(Row).Height = 0;
                Row++;

                //TITULOS
                workbooksheets.Range($"A{Row}:C{Row}")
                                         .Merge()
                                         .SetValue("TITULO")
                                         .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                         .Font.SetFontSize(FontSize)
                                         .Font.SetBold(true);

                workbooksheets.Range($"D{Row}:F{Row}")
                                        .Merge()
                                        .SetValue("NOMBRE CORTO")
                                        .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                        .Font.SetFontSize(FontSize);

                workbooksheets.Range($"G{Row}:I{Row}")
                                       .Merge()
                                       .SetValue("DESCRIPCIÓN")
                                       .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                       .Font.SetFontSize(FontSize);
                //Estilo
                workbooksheets.Range($"A{Row}:I{Row}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                workbooksheets.Range($"A{Row}:I{Row}").Style
                          .Font.SetFontColor(XLColor.White)
                          .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                          .Border.SetTopBorder(XLBorderStyleValues.Thin)
                          .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                          .Border.SetRightBorder(XLBorderStyleValues.Thin)
                          .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                          .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                Row++;
                workbooksheets.Range($"A{Row}:C{Row}")
                                   .Merge()
                                   .SetValue(campos.NombreLargo)
                                   .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                                   .Font.SetFontSize(FontSize);

                workbooksheets.Range($"D{Row}:F{Row}")
                                   .Merge()
                                   .SetValue(campos.NombreCorto)
                                   .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                                   .Font.SetFontSize(FontSize);

                workbooksheets.Range($"G{Row}:I{Row}")
                                       .Merge()
                                       .SetValue(campos.Ayuda)
                                       .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                                       .Font.SetFontSize(FontSize);
                workbooksheets.Range($"A{Row}:I{Row}").Style
                    .Font.SetFontColor(XLColor.Black)
                    .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                    .Border.SetTopBorder(XLBorderStyleValues.Thin)
                    .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                    .Border.SetRightBorder(XLBorderStyleValues.Thin)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                workbooksheets.Range($"A{Row}:I{Row}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
                Row++;
                //iniciamos column para usarlo en las configuraciones
                var column = 1;

                //Ponemos los tipo de Campos PNT
                if (campos.campos.Count > 0)
                {
                    foreach (var item in campos.campos)
                    {

                        workbooksheets.Range(Row, column, Row, column)
                          .Merge()
                          .SetValue(item.IdTipoCampoPNT.ToString());
                        column++;

                    }
                }
                //ajustamos la row en 0 para que no se vea
                workbooksheets.Row(Row).Height = 0;
                Row++;
                column = 1;
                //Ponemos el id de los campos PNT
                if (campos.campos.Count > 0)
                {
                    foreach (var item in campos.campos)
                    {

                        workbooksheets.Range(Row, column, Row, column)
                          .Merge()
                          .SetValue(item.IdCampoPNT.ToString());
                        column++;

                    }
                }
                //ajustamos la row en 0 para que no se vea
                workbooksheets.Row(Row).Height = 0;
                Row++;
                column = 1;
                //Row sin texto gray dark
                workbooksheets.Range(Row, 1, Row, campos.campos.Count)
                                      .Merge()
                                      .SetValue("")
                                      .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                      .Font.SetFontSize(FontSize)
                                      .Font.SetBold(true);
                workbooksheets.Range(Row, 1, Row, campos.campos.Count).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                Row++;
                //Ponemos los nombre de los campos
                if (campos.campos.Count > 0)
                {
                    foreach (var item in campos.campos)
                    {

                        workbooksheets.Range(Row, column, Row, column)
                          .Merge()
                          .SetValue(item.Etiqueta);
                        column++;
                        workbooksheets.Columns(column, column).Width = 30;
                    }
                }
                workbooksheets.Range(Row, 1, Row, (column - 1)).Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
                workbooksheets.Range(Row, 1, Row, (column - 1)).Style.Alignment.WrapText = true;
                //STILO DE LA TABLA
                workbooksheets.Range(Row, 1, Row, (column - 1)).Style
                             .Font.SetFontSize(FontSize)
                             .Font.SetFontColor(XLColor.Black)
                             //.Fill.SetBackgroundColor(XLColor.Cream)
                             .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                             .Border.SetTopBorder(XLBorderStyleValues.Thin)
                             .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                             .Border.SetRightBorder(XLBorderStyleValues.Thin)
                             .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                             .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);


                ///////creamos los tab de los catalogos o tablas///////
                var ContadorSheet = 1;

                foreach (var item in campos.campos.Where(x => x.TipoCampo == TipoCampo.Catalogo).ToList())
                {
                    var RowCatalogo = 0;
                    //var NombreTabla = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(item.CatalogoId);
                    //var NombreColumna = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(item.CatalogoId, true);
                    List<RespuestaQuery> LstCatalogo = HMTLHelperExtensions.GetListCatalogos(item.NombreTablaCatalogo, item.NombreCampoCatalago);
                    //var mCatalogo = db.Catalogoes.Where(x => x.CatalogoId == item.CatalogoId).FirstOrDefault();
                    //var NombreCatalogo = mCatalogo != null ? mCatalogo.Nombre : "No especificado";
                    //Empezamos los demas hojas
                    var sCATSheet = $"CAT{ContadorSheet}";
                    var ws = workbook.Worksheets.Add(sCATSheet);
                    //var ws1 = workbook.Worksheets.Where(x => x.Name == "CAT{ContadorSheet}").FirstOrDefault();





                    //subtitulos de los catalogos
                    RowCatalogo++;
                    if (item.TablaCatalogo)
                    {

                        //var campos = db.CampoCatalogo.Where(x => x.CatalogoId == item.CatalogoId && x.Activo).ToList();
                        if (item.camposTabla.Count > 0)
                        {
                            int iColum = 1;
                            int iCountCampos = item.camposTabla.Count;
                            ws.Range(RowCatalogo, iColum, RowCatalogo, iCountCampos)
                                                           .Merge()
                                                           .SetValue($"CATALOGO: {item.NombreCatalogo}")
                                                           .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                                           .Font.SetFontSize(FontSize)
                                                           .Font.SetBold(true);
                            ws.Range(RowCatalogo, iColum, RowCatalogo, iCountCampos).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                            ws.Range(RowCatalogo, iColum, RowCatalogo, iCountCampos).Style
                                      .Font.SetFontColor(XLColor.White)
                                      .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                      .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                      .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                      .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                      .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                      .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            RowCatalogo = 4;
                            iColum--;

                            foreach (var camposCatalogo in item.camposTabla)
                            {
                                iColum++;
                                ws.Range(2, iColum, 2, iColum)
                                .Merge()
                                .SetValue(camposCatalogo.IdTipoCampoPNT.ToString());
                                ws.Range(3, iColum, 3, iColum)
                                  .Merge()
                                  .SetValue(camposCatalogo.IdCampoPNT.ToString());

                                ws.Range(RowCatalogo, iColum, RowCatalogo, iColum)
                                          .Merge()
                                          .SetValue($"{camposCatalogo.Etiqueta}")
                                          .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                          .Font.SetFontSize(FontSize)
                                          .Font.SetBold(true);


                            }


                            ws.Range(RowCatalogo, 1, RowCatalogo, iColum).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                            ws.Range(RowCatalogo, 1, RowCatalogo, iColum).Style
                                      .Font.SetFontColor(XLColor.White)
                                      .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                      .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                      .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                      .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                      .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                      .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            ws.Columns(1, iColum).Width = 30;
                            //Lista de catalaogos
                            listaTabs.Add(new vmExcelTabs { catalogoId = item.CatalogoId, NombreWorksheets = sCATSheet, Columns = iColum, NumberWorksheets = ContadorSheet + 1, Row = 4 });

                            ws.Row(2).Height = 0;
                            ws.Row(3).Height = 0;

                        }


                    }
                    else
                    {
                        ws.Range($"A{RowCatalogo}:B{RowCatalogo}")
                                                        .Merge()
                                                        .SetValue($"CATALOGO: {item.NombreCatalogo}")
                                                        .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                                        .Font.SetFontSize(FontSize)
                                                        .Font.SetBold(true);
                        ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                        ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style
                                  .Font.SetFontColor(XLColor.White)
                                  .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                  .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                  .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                  .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                  .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        ws.Range($"A{RowCatalogo}")
                                           .Merge()
                                           .SetValue($"No.")
                                           .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                           .Font.SetFontSize(FontSize)
                                           .Font.SetBold(true);
                        ws.Range($"B{RowCatalogo}")
                                          .Merge()
                                          .SetValue($"Valor")
                                          .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                          .Font.SetFontSize(FontSize)
                                          .Font.SetBold(true);


                        ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                        ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style
                                  .Font.SetFontColor(XLColor.White)
                                  .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                  .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                  .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                  .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                  .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        //Campos valores
                        RowCatalogo++;
                        var contador = 1;
                        foreach (var campoCatalogo in LstCatalogo)
                        {

                            ws.Range(RowCatalogo, 1, RowCatalogo, 1)
                             .Merge()
                             .SetValue(contador);
                            column++;
                            ws.Columns(column, column).Width = 30;

                            ws.Range(RowCatalogo, 2, RowCatalogo, 2)
                             .Merge()
                             .SetValue(campoCatalogo.Valor);
                            column++;
                            ws.Columns(column, column).Width = 30;
                            RowCatalogo++;
                            contador++;


                        }
                        RowCatalogo = (RowCatalogo - 1);
                        if (RowCatalogo >= 2)
                        {
                            ws.Range(2, 1, RowCatalogo, 2).Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
                            ws.Range(2, 1, RowCatalogo, 2).Style.Alignment.WrapText = true;
                            //STILO DE LA TABLA
                            ws.Range(2, 1, RowCatalogo, 2).Style
                                         .Font.SetFontSize(FontSize)
                                         .Font.SetFontColor(XLColor.Black)
                                         //.Fill.SetBackgroundColor(XLColor.Cream)
                                         .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                         .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                         .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                         .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                         .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                         .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        }
                        ws.Columns(1, 2).Width = 30;
                    }
                    ContadorSheet++;
                }
                // poner los datos que necesitamos


                //Llenado
                column = 1;
                var inicio = Rangos.Count() > 0 ? Convert.ToInt32(Rangos[0]) : 0;
                var fin = 0;
                if (Rangos.Count() > 1)
                {
                    fin = Rangos.Count() > 0 ? Convert.ToInt32(Rangos[1]) : 0;
                }

                List<vmRowDatos> datos = new List<vmRowDatos>();
                if (Enlace)
                    datos = this.GetDatosSelectDonwload(campos, iOrganismoId, 0, inicio, fin);
                else
                    datos = this.GetDatosSelectDonwload(campos, iOrganismoId, 0, inicio, fin, sUsuarioId);

                var datosCatalogos = this.GetDatosSelectDonwloadTabla(listaTabs, iOrganismoId);

                foreach (var item in datos)
                {
                    Row++;
                    column = 1;
                    foreach (var valores in item.campos)
                    {
                        string valorCatalgoId = "0";

                        if (valores.EsTabla)
                        {
                            var iCatalagoId = Convert.ToInt32(valores.Valor);
                            var Tab = listaTabs.Where(x => x.catalogoId == iCatalagoId).FirstOrDefault();
                            var valorTabla = datosCatalogos.Where(x => x.catalogoId == iCatalagoId && x.Idregistro == item.TablaFisicaId).ToList();
                            if (Tab != null)
                            {
                                var ws2 = workbook.Worksheet(Tab.NumberWorksheets);
                                //buscamos en los datos
                                foreach (var itemCatalago in valorTabla)
                                {

                                    Tab.Row = Tab.Row + 1;
                                    var iColumnCatalago = 1;
                                    //buscamos en los campos de datos
                                    foreach (var valoresCatalago in itemCatalago.campos)
                                    {
                                        if (valoresCatalago.NombreCampo.ToLower().Trim() == "id")
                                        {
                                            valorCatalgoId = valoresCatalago.Valor;
                                        }
                                        ws2.Cell(Tab.Row, iColumnCatalago).SetValue(valoresCatalago.Valor).GetFormattedString();
                                        iColumnCatalago++;
                                    }
                                }


                            }
                            workbooksheets.Cell(Row, column).SetValue(valorCatalgoId).GetFormattedString();


                        }
                        else
                        {
                            workbooksheets.Cell(Row, column).SetValue(valores.Valor).GetFormattedString();

                        }
                        column++;

                    }
                }
                //STILO DE LA TABLA
                workbooksheets.Range(5, 1, Row, (column - 1)).Style
                             .Font.SetFontSize(FontSize)
                             .Font.SetFontColor(XLColor.Black)
                             //.Fill.SetBackgroundColor(XLColor.Cream)
                             .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                             .Border.SetTopBorder(XLBorderStyleValues.Thin)
                             .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                             .Border.SetRightBorder(XLBorderStyleValues.Thin)
                             .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                             .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }
            return new ExcelResult(workbook, campos.NombreCorto.Trim());
        }
        public ActionResult GenerarPlantillaExcelInformacion(List<Campo> LstCampos, Plantilla mPlantilla, string NombrePlantilla, string[] Rangos, int Tipo)
        {
            int Row = 1;
            // Generate the workbook...
            var workbook = new XLWorkbook();
            string nombreDoc = "Reporte de Formatos"; //mPlantilla != null ? mPlantilla.NombreCorto : "NoEspecificado";
            var workbooksheets = workbook.AddWorksheet(nombreDoc);//mPlantilla.NombreCorto);
            List<vmExcelTabs> listaTabs = new List<vmExcelTabs>();
            var FontSize = 11;



            //TITULOS
            workbooksheets.Range($"A{Row}:C{Row}")
                                     .Merge()
                                     .SetValue("TITULO")
                                     .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                     .Font.SetFontSize(FontSize)
                                     .Font.SetBold(true);

            workbooksheets.Range($"D{Row}:F{Row}")
                                    .Merge()
                                    .SetValue("NOMBRE CORTO")
                                    .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                    .Font.SetFontSize(FontSize);

            workbooksheets.Range($"G{Row}:I{Row}")
                                   .Merge()
                                   .SetValue("DESCRIPCIÓN")
                                   .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                   .Font.SetFontSize(FontSize);
            //Estilo
            workbooksheets.Range($"A{Row}:I{Row}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
            workbooksheets.Range($"A{Row}:I{Row}").Style
                      .Font.SetFontColor(XLColor.White)
                      .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                      .Border.SetTopBorder(XLBorderStyleValues.Thin)
                      .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                      .Border.SetRightBorder(XLBorderStyleValues.Thin)
                      .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                      .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            Row++;
            if (mPlantilla != null)
            {
                workbooksheets.Range($"A{Row}:C{Row}")
                                   .Merge()
                                   .SetValue(mPlantilla.NombreLargo)
                                   .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                   .Font.SetFontSize(FontSize);

                workbooksheets.Range($"D{Row}:F{Row}")
                                   .Merge()
                                   .SetValue(mPlantilla.NombreCorto)
                                   .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                   .Font.SetFontSize(FontSize);

                workbooksheets.Range($"G{Row}:I{Row}")
                                       .Merge()
                                       .SetValue(mPlantilla.Ayuda)
                                       .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                       .Font.SetFontSize(FontSize);

            }
            workbooksheets.Range($"A{Row}:I{Row}").Style
                     .Font.SetFontColor(XLColor.Black)
                     .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                     .Border.SetTopBorder(XLBorderStyleValues.Thin)
                     .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                     .Border.SetRightBorder(XLBorderStyleValues.Thin)
                     .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                     .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            workbooksheets.Range($"A{Row}:I{Row}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
            Row++;
            //creo que si es con numros
            workbooksheets.Range(Row, 1, Row, LstCampos.Count)
                                  .Merge()
                                  .SetValue("")
                                  .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                  .Font.SetFontSize(FontSize)
                                  .Font.SetBold(true);
            workbooksheets.Range(Row, 1, Row, LstCampos.Count).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
            Row++;
            var column = 1;
            var ContadorSheet = 0;
            if (LstCampos.Count > 0)
            {
                foreach (var item in LstCampos)
                {

                    if (item.TipoCampo == TipoCampo.Catalogo && HMTLHelperExtensions.GetTipoTabla(item.CatalogoId))
                    {
                        ContadorSheet++;
                        var RowCatalogo = 1;
                        var ws = workbook.Worksheets.Add($"CAT{ContadorSheet}");
                        listaTabs.Add(new vmExcelTabs { catalogoId = item.CatalogoId, NombreWorksheets = "CAT{ContadorSheet}" });
                        //hacemos la informacion
                        var campos = db.CampoCatalogo.Where(x => x.CatalogoId == item.CatalogoId && x.Activo).ToList();
                        var mCatalogo = db.Catalogoes.Where(x => x.CatalogoId == item.CatalogoId).FirstOrDefault();
                        var NombreCatalogo = mCatalogo != null ? mCatalogo.Nombre : "No especificado";
                        if (campos.Count > 0)
                        {
                            int iColum = 1;
                            ws.Range(RowCatalogo, iColum, RowCatalogo, campos.Count)
                                                           .Merge()
                                                           .SetValue($"CATALOGO: {NombreCatalogo}")
                                                           .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                                           .Font.SetFontSize(FontSize)
                                                           .Font.SetBold(true);
                            ws.Range(RowCatalogo, iColum, RowCatalogo, campos.Count).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                            ws.Range(RowCatalogo, iColum, RowCatalogo, campos.Count).Style
                                      .Font.SetFontColor(XLColor.White)
                                      .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                      .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                      .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                      .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                      .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                      .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            RowCatalogo++;
                            iColum--;

                            foreach (var camposCatalogo in campos)
                            {
                                iColum++;
                                ws.Range(RowCatalogo, iColum, RowCatalogo, iColum)
                                          .Merge()
                                          .SetValue($"{camposCatalogo.Etiqueta}")
                                          .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                          .Font.SetFontSize(FontSize)
                                          .Font.SetBold(true);


                            }
                            ws.Range(RowCatalogo, 1, RowCatalogo, iColum).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                            ws.Range(RowCatalogo, 1, RowCatalogo, iColum).Style
                                      .Font.SetFontColor(XLColor.White)
                                      .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                      .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                      .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                      .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                      .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                      .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            ws.Columns(1, iColum).Width = 30;

                        }
                    }
                    workbooksheets.Range(Row, column, Row, column)
                      .Merge()
                      .SetValue(item.Etiqueta);
                    column++;
                    workbooksheets.Columns(column, column).Width = 30;

                }
            }
            workbooksheets.Range(Row, 1, Row, (column - 1)).Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
            workbooksheets.Range(Row, 1, Row, (column - 1)).Style.Alignment.WrapText = true;


            //Llenado

            column = 1;
            var inicio = Rangos.Count() > 0 ? Convert.ToInt32(Rangos[0]) : 0;
            var fin = Rangos.Count() > 0 ? Convert.ToInt32(Rangos[1]) : 0;
            var Datos = GetDatosSelectDonwload(NombrePlantilla, LstCampos, inicio, fin);

            foreach (var item in Datos)
            {
                Row++;
                column = 1;
                foreach (var dato in item)
                {
                    var mCampo = LstCampos.Where(x => x.Nombre == dato.Key).FirstOrDefault();

                    if (mCampo != null)
                    {
                        if (HMTLHelperExtensions.GetTipoTabla(mCampo.CatalogoId))
                        {

                        }
                        else
                        {
                            string valor = HMTLHelperExtensions.FormatDataFromRazor(mCampo.TipoCampo, mCampo._ConDecimales, mCampo.CatalogoId, dato.Value, true, Tipo == 2 ? true : false);
                            workbooksheets.Cell(Row, column)
                             //.Merge()
                             //.SetValue(Convert.ToString(dato.Value));
                             .SetValue(valor).GetFormattedString();
                            column++;
                            //workbooksheets.Columns(column, column).Width = 30;

                        }


                    }

                }


            }

            //STILO DE LA TABLA
            workbooksheets.Range(5, 1, Row, (column - 1)).Style
                         .Font.SetFontSize(FontSize)
                         .Font.SetFontColor(XLColor.Black)
                         //.Fill.SetBackgroundColor(XLColor.Cream)
                         .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                         .Border.SetTopBorder(XLBorderStyleValues.Thin)
                         .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                         .Border.SetRightBorder(XLBorderStyleValues.Thin)
                         .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                         .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);



            //var ContadorSheet = 1;
            //foreach (var item in LstCampos.Where(x => x.TipoCampo == TipoCampo.Catalogo).ToList())
            //{
            //    var RowCatalogo = 1;
            //    var NombreTabla = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(item.CatalogoId);
            //    var NombreColumna = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(item.CatalogoId, true);
            //    List<RespuestaQuery> LstCatalogo = HMTLHelperExtensions.GetListCatalogos(NombreTabla, NombreColumna);
            //    var mCatalogo = db.Catalogoes.Where(x => x.CatalogoId == item.CatalogoId).FirstOrDefault();
            //    var NombreCatalogo = mCatalogo != null ? mCatalogo.Nombre : "No especificado";
            //    //Empezamos los demas hojas
            //    var ws = workbook.Worksheets.Add($"CAT{ContadorSheet}");
            //    ws.Range($"A{RowCatalogo}:B{RowCatalogo}")
            //                       .Merge()
            //                       .SetValue($"CATALOGO: {NombreCatalogo}")
            //                       .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            //                       .Font.SetFontSize(FontSize)
            //                       .Font.SetBold(true);



            //    ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
            //    ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style
            //              .Font.SetFontColor(XLColor.White)
            //              .Border.SetBottomBorder(XLBorderStyleValues.Thin)
            //              .Border.SetTopBorder(XLBorderStyleValues.Thin)
            //              .Border.SetLeftBorder(XLBorderStyleValues.Thin)
            //              .Border.SetRightBorder(XLBorderStyleValues.Thin)
            //              .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            //              .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            //    //subtitulos de los catalogos
            //    RowCatalogo++;
            //    ws.Range($"A{RowCatalogo}")
            //                       .Merge()
            //                       .SetValue($"No.")
            //                       .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            //                       .Font.SetFontSize(FontSize)
            //                       .Font.SetBold(true);
            //    ws.Range($"B{RowCatalogo}")
            //                      .Merge()
            //                      .SetValue($"Valor")
            //                      .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            //                      .Font.SetFontSize(FontSize)
            //                      .Font.SetBold(true);


            //    ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
            //    ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style
            //              .Font.SetFontColor(XLColor.White)
            //              .Border.SetBottomBorder(XLBorderStyleValues.Thin)
            //              .Border.SetTopBorder(XLBorderStyleValues.Thin)
            //              .Border.SetLeftBorder(XLBorderStyleValues.Thin)
            //              .Border.SetRightBorder(XLBorderStyleValues.Thin)
            //              .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            //              .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);



            //    //Campos valores
            //    RowCatalogo++;
            //    var contador = 1;
            //    foreach (var campoCatalogo in LstCatalogo)
            //    {

            //        ws.Range(RowCatalogo, 1, RowCatalogo, 1)
            //         .Merge()
            //         .SetValue(contador);
            //        column++;
            //        ws.Columns(column, column).Width = 30;

            //        ws.Range(RowCatalogo, 2, RowCatalogo, 2)
            //         .Merge()
            //         .SetValue(campoCatalogo.Valor);
            //        column++;
            //        ws.Columns(column, column).Width = 30;
            //        RowCatalogo++;
            //        contador++;


            //    }
            //    RowCatalogo = (RowCatalogo - 1);
            //    if (RowCatalogo > 2)
            //    {
            //        ws.Range(3, 1, RowCatalogo, 2).Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
            //        ws.Range(3, 1, RowCatalogo, 2).Style.Alignment.WrapText = true;
            //        //STILO DE LA TABLA
            //        ws.Range(3, 1, RowCatalogo, 2).Style
            //                     .Font.SetFontSize(FontSize)
            //                     .Font.SetFontColor(XLColor.Black)
            //                     //.Fill.SetBackgroundColor(XLColor.Cream)
            //                     .Border.SetBottomBorder(XLBorderStyleValues.Thin)
            //                     .Border.SetTopBorder(XLBorderStyleValues.Thin)
            //                     .Border.SetLeftBorder(XLBorderStyleValues.Thin)
            //                     .Border.SetRightBorder(XLBorderStyleValues.Thin)
            //                     .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            //                     .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            //    }

            //    ContadorSheet++;
            //}


            //Response.Cookies.Add(new HttpCookie(CookieName, "1") { Expires = DateTime.Now.AddSeconds(1) });
            //ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("dlc", CookieName));
            return new ExcelResult(workbook, mPlantilla.NombreCorto.Trim());
        }

        public List<vmRowDatos> GetDatosSelectDonwload(vmPlantillaCampos campos, int iOrgaismoId, int iPeriodo)
        {
            List<vmRowDatos> resultado = new List<vmRowDatos>();
            List<AuxTitlePlantillas> Titulos = new List<AuxTitlePlantillas>();
            List<AuxCatalagoAmbiguos> LstCatalogosAmbiguos = new List<AuxCatalagoAmbiguos>();
            try
            {
                if (campos != null && campos.campos.Count > 0)
                {
                    string NombreTabla = campos.NombreTabla;
                    //paginador
                    string Query = $@"(SELECT h.TablaFisicaId,((SELECT NombreCampo, Valor, CASE ";
                    string QuerCatalogo = "";
                    //PARA TABLAS
                    string sCaseTabla = " CASE WHEN ";
                    bool hayTabla = false;
                    bool bSubQueryCaseEsTabla = false;
                    //PARA EL FORM DEL CAMPO
                    string SubQueryCampo = $@" FROM (SELECT TOP 1 CAST({NombreTabla}.TablaFisicaId AS nvarchar) as TablaFisicaId ";
                    //bool bSubQueryCampo = false;
                    string sUpivot = $@" UNPIVOT(Valor FOR NombreCampo IN( ";
                    bool firstUpivt = true;
                    //UNPIVOT(Valor FOR NombreCampo IN(a.TablaFisicaId, a.Ejercicio, a.Fecha_Inicio_Periodo_Informa)) AS unp  FOR JSON PATH
                    foreach (var item in campos.campos.Where(x => x.Nombre != "TablaFisicaId").ToList())
                    {
                        Titulos.Add(new AuxTitlePlantillas { LbNombre = item.Etiqueta });
                        Query += $@" WHEN NombreCampo = '{item.Nombre}' THEN {item.TipoCampo.GetHashCode()} ";
                        sUpivot += $@"{(!firstUpivt ? "," : "")} a.{item.Nombre}";
                        firstUpivt = false;
                        if (item.TipoCampo == Transparencia.Models.TipoCampo.Catalogo)
                        {
                            if (!item.TablaCatalogo)
                            {
                                AuxCatalagoAmbiguos catalogoAmbiguos = new AuxCatalagoAmbiguos();
                                string NombreTablaCatalogo = item.NombreTablaCatalogo;
                                catalogoAmbiguos.NombreOriginal = NombreTablaCatalogo;
                                catalogoAmbiguos.NombreGenerico = NombreTablaCatalogo;
                                if (LstCatalogosAmbiguos.Where(x => x.NombreOriginal == NombreTablaCatalogo).Count() > 0)
                                {
                                    var r = new Random();
                                    catalogoAmbiguos.NombreOriginal = NombreTablaCatalogo;
                                    catalogoAmbiguos.NombreGenerico = $"{NombreTablaCatalogo}_{r.Next(1, 788988799)}";
                                    while (LstCatalogosAmbiguos.Where(x => x.NombreOriginal == NombreTablaCatalogo && x.NombreGenerico == catalogoAmbiguos.NombreGenerico).Count() > 0)
                                    {
                                        //seguimos cambiando
                                        catalogoAmbiguos.NombreGenerico = $"{NombreTablaCatalogo}_{r.Next(1, 788988799)}";
                                    }
                                    LstCatalogosAmbiguos.Add(catalogoAmbiguos);
                                }
                                else
                                {
                                    LstCatalogosAmbiguos.Add(new AuxCatalagoAmbiguos() { NombreOriginal = NombreTablaCatalogo, NombreGenerico = NombreTablaCatalogo });

                                }

                                //SubQueryCampo += $@", CAST({NombreTablaCatalogo}.{item.NombreCatalogo} AS nvarchar) as {item.Nombre} ";
                                SubQueryCampo += $@", CAST((dbo.fun_formato_valor_dinamico(CAST({catalogoAmbiguos.NombreGenerico}.{item.NombreCampoCatalago} AS  nvarchar(MAX)),{item.TipoCampoCatalogo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX)) as {item.Nombre}  ";
                                QuerCatalogo += $" LEFT JOIN {NombreTablaCatalogo} {catalogoAmbiguos.NombreGenerico} ON {NombreTabla}.{item.Nombre} = {catalogoAmbiguos.NombreGenerico}.TablaFisicaId ";
                            }
                            else
                            {

                                //SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTabla}.{item.Nombre} AS nvarchar),{item.TipoCampo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as nvarchar)  as {item.Nombre} ";
                                SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({item.CatalogoId} AS  nvarchar(MAX)),100,{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX))  as {item.Nombre} ";
                                sCaseTabla += $@" {(bSubQueryCaseEsTabla ? "OR" : "")} NombreCampo = '{item.Nombre}' ";
                                bSubQueryCaseEsTabla = true;
                                hayTabla = true;
                            }
                        }
                        else
                        {
                            SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTabla}.{item.Nombre} AS  nvarchar(MAX)),{item.TipoCampo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX))  as {item.Nombre} ";
                        }
                    }
                    Query += " END AS TipoCampo, ";
                    sCaseTabla += $@" THEN 1 ELSE 0 END AS EsTabla ";
                    Query += hayTabla ? sCaseTabla : " 0 AS EsTabla ";
                    //Agregamos los campos y el from
                    SubQueryCampo += $@" FROM {NombreTabla} {QuerCatalogo} WHERE {NombreTabla}.TablaFisicaId = h.TablaFisicaId) a";
                    Query += SubQueryCampo;
                    sUpivot += $@")) AS unp  FOR JSON PATH ) ) AS campos";
                    Query += sUpivot;
                    //Where Section
                    string sWhere = "";

                    Query += $@" FROM {NombreTabla} h
                                 LEFT JOIN Periodoes p ON h.PeriodoId = p.PeriodoId
                                 WHERE h.Activo=1 AND OrganismoID = {iOrgaismoId}
                    AND
                      ({iPeriodo} = 0 OR p.PeriodoId = { iPeriodo} ) { sWhere}
                    )";

                    //consultamos los datos Aqui esta
                    SqlMapper.AddTypeHandler(new vmRowDatosHandler());
                    SqlMapper.AddTypeHandler(new vmcampoDatosHandler());
                    using (var idb = db.Database.Connection)
                    {

                        resultado = idb.Query<vmRowDatos>(
                              Query
                              ).ToList();

                        SqlMapper.ResetTypeHandlers();
                    }
                    ViewBag.Titles = Titulos;
                    ViewBag.LstAmbiguos = LstCatalogosAmbiguos;
                }
            }
            catch (Exception ex)
            {
                string var = ex.Message;
            }
            return resultado;
        }
        public List<dynamic> GetDatosSelectDonwload(string NombreTabla, List<Campo> Campos = null, int Inicio = 1, int Fin = 1000)
        {

            List<VMCampos> LstVMCampos = new List<VMCampos>();
            List<dynamic> results = null;

            try
            {
                string sQuery = "";
                sQuery = $@"SELECT ";
                foreach (var relevante in Campos)
                {
                    sQuery += $@"{relevante.Nombre},";

                }
                sQuery += $@"TablaFisicaId, Activo";
                sQuery += $@" FROM ";

                sQuery += $@" (SELECT ROW_NUMBER() OVER(ORDER BY(select NULL as noorder)) AS RowNum, * FROM {NombreTabla}) as a";
                sQuery += $@" WHERE RowNum BETWEEN {Inicio} AND {Fin}";
                sQuery += $" ORDER BY FechaCreacion";

                //sQuery += $@" LIMIT {Inicio}, {Fin} ";

                results = HMTLHelperExtensions.DynamicListFromSql(db, sQuery, new Dictionary<string, object>()).ToList();


            }
            catch (Exception ex)
            {

            }


            return results;
        }

        public List<vmRowDatos> GetDatosSelectDonwload(vmPlantillaCampos campos, int iOrgaismoId = 0, int iPeriodo = 0, int Inicio = 1, int Fin = 1000, string sUsuarioId="")
        {
            List<vmRowDatos> resultado = new List<vmRowDatos>();
            List<AuxTitlePlantillas> Titulos = new List<AuxTitlePlantillas>();
            List<AuxCatalagoAmbiguos> LstCatalogosAmbiguos = new List<AuxCatalagoAmbiguos>();
            try
            {

                //var sYear = "''";
                //if (iPeriodo == 0)
                //{
                //    sYear = $@"'{DateTime.Now.Year.ToString()}'";
                //}
                //vmPlantillaCampos campos = HMTLHelperExtensions.GetCamposForDatos(iPlantillaId);
                if (campos != null && campos.campos.Count > 0)
                {
                    string NombreTabla = campos.NombreTabla;
                    //paginador
                    string Query = $@"(SELECT h.TablaFisicaId,((SELECT NombreCampo, Valor, CASE ";
                    string QuerCatalogo = "";
                    //PARA TABLAS
                    string sCaseTabla = " CASE WHEN ";
                    bool hayTabla = false;
                    bool bSubQueryCaseEsTabla = false;
                    //PARA EL FORM DEL CAMPO
                    string SubQueryCampo = $@" FROM (SELECT TOP 1 CAST({NombreTabla}.TablaFisicaId AS nvarchar) as TablaFisicaId ";
                    //bool bSubQueryCampo = false;
                    string sUpivot = $@" UNPIVOT(Valor FOR NombreCampo IN( ";
                    bool firstUpivt = true;
                    //UNPIVOT(Valor FOR NombreCampo IN(a.TablaFisicaId, a.Ejercicio, a.Fecha_Inicio_Periodo_Informa)) AS unp  FOR JSON PATH
                    foreach (var item in campos.campos.Where(x => x.Nombre != "TablaFisicaId").ToList())
                    {
                        Titulos.Add(new AuxTitlePlantillas { LbNombre = item.Etiqueta });
                        Query += $@" WHEN NombreCampo = '{item.Nombre}' THEN {item.TipoCampo.GetHashCode()} ";
                        sUpivot += $@"{(!firstUpivt ? "," : "")} a.{item.Nombre}";
                        firstUpivt = false;
                        if (item.TipoCampo == Transparencia.Models.TipoCampo.Catalogo)
                        {
                            if (!item.TablaCatalogo)
                            {
                                AuxCatalagoAmbiguos catalogoAmbiguos = new AuxCatalagoAmbiguos();
                                string NombreTablaCatalogo = item.NombreTablaCatalogo;
                                catalogoAmbiguos.NombreOriginal = NombreTablaCatalogo;
                                catalogoAmbiguos.NombreGenerico = NombreTablaCatalogo;
                                if (LstCatalogosAmbiguos.Where(x => x.NombreOriginal == NombreTablaCatalogo).Count() > 0)
                                {
                                    var r = new Random();
                                    catalogoAmbiguos.NombreOriginal = NombreTablaCatalogo;
                                    catalogoAmbiguos.NombreGenerico = $"{NombreTablaCatalogo}_{r.Next(1, 788988799)}";
                                    while (LstCatalogosAmbiguos.Where(x => x.NombreOriginal == NombreTablaCatalogo && x.NombreGenerico == catalogoAmbiguos.NombreGenerico).Count() > 0)
                                    {
                                        //seguimos cambiando
                                        catalogoAmbiguos.NombreGenerico = $"{NombreTablaCatalogo}_{r.Next(1, 788988799)}";
                                    }
                                    LstCatalogosAmbiguos.Add(catalogoAmbiguos);
                                }
                                else
                                {
                                    LstCatalogosAmbiguos.Add(new AuxCatalagoAmbiguos() { NombreOriginal = NombreTablaCatalogo, NombreGenerico = NombreTablaCatalogo });

                                }

                                //SubQueryCampo += $@", CAST({NombreTablaCatalogo}.{item.NombreCatalogo} AS nvarchar) as {item.Nombre} ";
                                SubQueryCampo += $@", CAST((dbo.fun_formato_valor_dinamico(CAST({catalogoAmbiguos.NombreGenerico}.{item.NombreCampoCatalago} AS  nvarchar(MAX)),{item.TipoCampoCatalogo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX)) as {item.Nombre}  ";
                                QuerCatalogo += $" LEFT JOIN {NombreTablaCatalogo} {catalogoAmbiguos.NombreGenerico} ON {NombreTabla}.{item.Nombre} = {catalogoAmbiguos.NombreGenerico}.TablaFisicaId ";
                            }
                            else
                            {

                                //SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTabla}.{item.Nombre} AS nvarchar),{item.TipoCampo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as nvarchar)  as {item.Nombre} ";
                                SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({item.CatalogoId} AS  nvarchar(MAX)),100,{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX))  as {item.Nombre} ";
                                sCaseTabla += $@" {(bSubQueryCaseEsTabla ? "OR" : "")} NombreCampo = '{item.Nombre}' ";
                                bSubQueryCaseEsTabla = true;
                                hayTabla = true;
                            }
                        }
                        else
                        {
                            SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTabla}.{item.Nombre} AS  nvarchar(MAX)),{item.TipoCampo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX))  as {item.Nombre} ";
                        }
                    }
                    Query += " END AS TipoCampo, ";
                    sCaseTabla += $@" THEN 1 ELSE 0 END AS EsTabla ";
                    Query += hayTabla ? sCaseTabla : " 0 AS EsTabla ";
                    //Agregamos los campos y el from
                    SubQueryCampo += $@" FROM {NombreTabla} {QuerCatalogo} WHERE {NombreTabla}.TablaFisicaId = h.TablaFisicaId) a";
                    Query += SubQueryCampo;

                    sUpivot += $@")) AS unp  FOR JSON PATH ) ) AS campos";
                    Query += sUpivot;

                    //Where Section
                    string sWhere = "";
                    //if (Campos != null && campos.campos.Count > 0)
                    //{
                    //    foreach (var item in Campos.Where(x => x.TipoCampo != TipoCampo.ArchivoAdjunto).ToList())
                    //    {
                    //        string valor = form[$@"{item.Nombre}"];

                    //        if (valor != null && valor != "")
                    //        {
                    //            var sLikeIgual = "=";

                    //            if (item.TipoCampo == TipoCampo.Texto || item.TipoCampo == TipoCampo.AreaTexto || item.TipoCampo == TipoCampo.Alfanumerico || item.TipoCampo == TipoCampo.email
                    //                || item.TipoCampo == TipoCampo.Hipervinculo || item.TipoCampo == TipoCampo.Telefono)
                    //            {
                    //                sLikeIgual = "LIKE";
                    //            }

                    //            if (item.TipoCampo == TipoCampo.Decimal || item.TipoCampo == TipoCampo.Dinero || item.TipoCampo == TipoCampo.Numerico || item.TipoCampo == TipoCampo.Porcentaje)
                    //            {
                    //                valor = valor.Replace("(", "").Replace(")", "").Replace(",", "");
                    //            }

                    //            //poneemos el where
                    //            if (item._TipoFecha == TipoFecha.FechaHasta)
                    //            {

                    //            }
                    //            else if (item._TipoFecha == TipoFecha.FechaDesde)
                    //            {

                    //            }
                    //            else
                    //            {

                    //                sWhere += $@" AND {item.Nombre} {sLikeIgual} {GetValueByCampo(item, valor, (sLikeIgual == "LIKE"))}";
                    //                //SQueryWhereForRows += $@" AND {item.Nombre} {sLikeIgual} {GetValueByCampo(item, valor, (sLikeIgual == "LIKE"))}";


                    //            }

                    //        }
                    //    }
                    //}


                    Query += $@" FROM {NombreTabla} h
                                 LEFT JOIN Periodoes p ON h.PeriodoId = p.PeriodoId
                                 WHERE h.Activo=1 AND OrganismoID = {iOrgaismoId}
                    AND
                      ({iPeriodo} = 0 OR p.PeriodoId = { iPeriodo} )";

                    if(sUsuarioId.Length > 0)
                    {
                        Query += $" AND (UsuarioId='{sUsuarioId}')";
                    }
                    Query += $" { sWhere} )";






                    //endpaginador
                    //Query += $@"FROM {NombreTabla} a 
                    //         LEFT JOIN Periodoes p ON a.PeriodoId = p.PeriodoId
                    //      WHERE a.Activo=1 AND a.OrganismoID={iOrganismoId} 
                    //AND
                    //  ({ iPeriodo} = 0 OR p.PeriodoId = { iPeriodo} ) { sWhere}
                    //";


                    //consultamos los datos Aqui esta
                    SqlMapper.AddTypeHandler(new vmRowDatosHandler());
                    SqlMapper.AddTypeHandler(new vmcampoDatosHandler());
                    using (var idb = db.Database.Connection)
                    {

                        resultado = idb.Query<vmRowDatos>(
                              Query
                              ).ToList();

                        SqlMapper.ResetTypeHandlers();
                    }
                    ViewBag.Titles = Titulos;
                    ViewBag.LstAmbiguos = LstCatalogosAmbiguos;
                }
            }
            catch (Exception ex)
            {
                string var = ex.Message;
            }
            return resultado;
        }

        public List<vmRowDatos> GetDatosSelectDonwloadTabla(List<vmExcelTabs> LstTab, int iOrganismoId = 0)
        {

            List<vmRowDatos> resultado = new List<vmRowDatos>();
            try
            {


                //var sYear = "''";
                //if (iPeriodo == 0)
                //{
                //    sYear = $@"'{DateTime.Now.Year.ToString()}'";
                //}
                //vmPlantillaCampos campos = HMTLHelperExtensions.GetCamposForDatos(iPlantillaId);
                string Query = "(";
                int contadorUnion = 1;
                foreach (var Tab in LstTab)
                {
                    var catalagoCampos = HMTLHelperExtensions.GetCamposCatalagoForExcel(Tab.catalogoId);

                    //emppezamos a consultar
                    if (catalagoCampos != null && catalagoCampos.catalagoCampos.Count > 0)
                    {
                        string NombreTabla = catalagoCampos.NombreTabla;
                        //paginador
                        Query += $@"{(contadorUnion > 1 ? " UNION " : "")}(SELECT h.TablaFisicaId,h.Idregistro,{Tab.catalogoId} as catalogoId,((SELECT NombreCampo, Valor, CASE ";
                        string QuerCatalogo = "";
                        //PARA TABLAS
                        string sCaseTabla = " CASE WHEN ";
                        bool hayTabla = false;
                        bool bSubQueryCaseEsTabla = false;
                        //PARA EL FORM DEL CAMPO
                        string SubQueryCampo = $@" FROM (SELECT TOP 1 CAST({NombreTabla}.TablaFisicaId AS nvarchar) as TablaFisicaId ";
                        //bool bSubQueryCampo = false;
                        string sUpivot = $@" UNPIVOT(Valor FOR NombreCampo IN( ";
                        bool firstUpivt = true;
                        //UNPIVOT(Valor FOR NombreCampo IN(a.TablaFisicaId, a.Ejercicio, a.Fecha_Inicio_Periodo_Informa)) AS unp  FOR JSON PATH
                        foreach (var item in catalagoCampos.catalagoCampos.Where(x => x.Nombre != "TablaFisicaId").ToList())
                        {
                            Query += $@" WHEN NombreCampo = '{item.Nombre}' THEN {item.TipoCampo.GetHashCode()} ";
                            sUpivot += $@"{(!firstUpivt ? "," : "")} a.{item.Nombre}";
                            firstUpivt = false;
                            //if (item.TipoCampo == Transparencia.Models.TipoCampo.Catalogo)
                            //{
                            //    if (!item.TablaCatalogo)
                            //    {
                            //        string NombreTablaCatalogo = item.NombreTablaCatalogo;
                            //        //SubQueryCampo += $@", CAST({NombreTablaCatalogo}.{item.NombreCatalogo} AS nvarchar) as {item.Nombre} ";
                            //        SubQueryCampo += $@", CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTablaCatalogo}.{item.NombreCampoCatalago} AS nvarchar),{item.TipoCampoCatalogo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as nvarchar) as {item.Nombre}  ";
                            //        QuerCatalogo += $" LEFT JOIN {NombreTablaCatalogo} ON {NombreTabla}.{item.Nombre} = {NombreTablaCatalogo}.TablaFisicaId ";
                            //    }
                            //    else
                            //    {
                            //        //SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTabla}.{item.Nombre} AS nvarchar),{item.TipoCampo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as nvarchar)  as {item.Nombre} ";
                            //        SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({item.CatalogoId} AS nvarchar),100,{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as nvarchar)  as {item.Nombre} ";
                            //        sCaseTabla += $@" {(bSubQueryCaseEsTabla ? "OR" : "")} NombreCampo = '{item.Nombre}' ";
                            //        bSubQueryCaseEsTabla = true;
                            //        hayTabla = true;
                            //    }
                            //}
                            //else
                            //{
                            SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTabla}.{item.Nombre} AS  nvarchar(MAX)),{item.TipoCampo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX))  as {item.Nombre} ";
                            //}
                        }
                        Query += " END AS TipoCampo, ";
                        sCaseTabla += $@" THEN 1 ELSE 0 END AS EsTabla ";
                        Query += hayTabla ? sCaseTabla : " 0 AS EsTabla ";
                        //Agregamos los campos y el from
                        SubQueryCampo += $@" FROM {NombreTabla} {QuerCatalogo} WHERE {NombreTabla}.TablaFisicaId = h.TablaFisicaId) a";
                        Query += SubQueryCampo;

                        sUpivot += $@")) AS unp  FOR JSON PATH ) ) AS campos";
                        Query += sUpivot;

                        //Where Section
                        string sWhere = "";

                        Query += $@" FROM {NombreTabla} h
                                 WHERE h.Activo=1 AND OrganismoId={iOrganismoId})";

                        //endpaginador
                        //Query += $@"FROM {NombreTabla} a 
                        //         LEFT JOIN Periodoes p ON a.PeriodoId = p.PeriodoId
                        //      WHERE a.Activo=1 AND a.OrganismoID={iOrganismoId} 
                        //AND
                        //  ({ iPeriodo} = 0 OR p.PeriodoId = { iPeriodo} ) { sWhere}
                        //";
                    }
                    contadorUnion++;
                }
                Query += ")";


                //consultamos los datos
                SqlMapper.AddTypeHandler(new vmRowDatosHandler());
                SqlMapper.AddTypeHandler(new vmcampoDatosHandler());
                ApplicationDbContext dbConsulta = new ApplicationDbContext();
                using (var idb = dbConsulta.Database.Connection)
                {

                    resultado = idb.Query<vmRowDatos>(
                          Query
                          ).ToList();

                    SqlMapper.ResetTypeHandlers();
                }
                dbConsulta.Dispose();



            }
            catch (Exception ex)
            {
                string var = ex.Message;
            }
            return resultado;
        }

        public List<SelectListItem> GetFrecuenciaConservacion()
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            try
            {
                lista.Add(new SelectListItem { Value = "1", Text = "Información vigente" });
                lista.Add(new SelectListItem { Value = "2", Text = "Información vigente y la generada en el ejercicio en curso" });
                lista.Add(new SelectListItem { Value = "3", Text = "Información del ejercicio en curso y la correspondiente al ejercicio anterior" });
                lista.Add(new SelectListItem { Value = "4", Text = "Información del ejercicio en curso y la correspondiente a los últimos seis ejercicios anteriores" });
                lista.Add(new SelectListItem { Value = "5", Text = "Información del ejercicio en curso y la correspondiente a los dos ejercicios anteriores" });
                lista.Add(new SelectListItem { Value = "6", Text = "Información generada en el ejercicio en curso y la correspondiente a los tres ejercicios anteriores"});
                lista.Add(new SelectListItem { Value = "7", Text = "Información de seis ejercicio anteriores" });
                lista.Add(new SelectListItem { Value = "8", Text = "Información vigente y la correspondiente al plan anterior" });
                lista.Add(new SelectListItem { Value = "9", Text = "Información del ejercicio en curso y la correspondiente al ejercicio inmediato anterior" });
                lista.Add(new SelectListItem { Value = "10", Text = "Periodo de doce meses" });

                lista.Add(new SelectListItem { Value = "11", Text = "Información vigente y las modificaciones que se hayan realizado desde la creación del sujeto obligado" });
                lista.Add(new SelectListItem { Value = "12", Text = "Debiendo mantenerse publicada por un periodo de diez años a partir de la publicación de los presentes lineamientos" });
                lista.Add(new SelectListItem { Value = "13", Text = "Se mantendrá un histórico de diez años" });
                lista.Add(new SelectListItem { Value = "14", Text = "El primero de junio y el primero de diciembre de cada año" });
                lista.Add(new SelectListItem { Value = "15", Text = "El primero de junio y el primero de diciembre de cada año, en conjunto con el informe de la fracción VI" });
                lista.Add(new SelectListItem { Value = "16", Text = "Serán publicadas a más tardar 24 horas después de efectuadas" });
                lista.Add(new SelectListItem { Value = "17", Text = "15 días hábiles posteriores de que el despacho externo" });

                lista = lista.OrderBy(x => x.Text).ToList<SelectListItem>();


            }
            catch (Exception ex)
            {
                
            }
            return lista;

        }

        public ActionResult GetFrecuencias(FrecuenciaActualizacion vTipoFrecuencia = 0)
        {
            List<SelectListItem> lista = new List<SelectListItem>();
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
                        lista.Add(new SelectListItem { Value = "12", Text = "Diciembre" });
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


                return Json(new { Hecho = true, data = lista }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = $"Ocurrio un error,Error: '{ex.Message}' " }, JsonRequestBehavior.AllowGet);
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
                        lista.Add(new SelectListItem { Value = "12", Text = "Diciembre" });
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

        #endregion

        #region Excel

        public ActionResult FormatoExcelPlantilla(int iId, string CookieName)
        {

            //var mPlantilla = db.Plantillas.Where(x => x.PlantillaId == iId).FirstOrDefault();
            //var NombrePlantilla = mPlantilla != null ? mPlantilla.NombreCorto : "";
            //var lstCampos = db.Campos.Where(x => x.PlantillaId == iId && x.Activo).ToList();

            return GenerarPlantillaExcel(iId, CookieName);
        }
        //Generar1
        public ActionResult GenerarPlantillaExcel(int iPlantillaId, string CookieName)
        {

            vmPlantillaCampos campos = HMTLHelperExtensions.GetCamposForExcel(iPlantillaId);
            int Row = 1;
            var FontSize = 11;
            // Generate the workbook...
            var workbook = new XLWorkbook();
            string nombreDoc = "Reporte de Formatos";
            if (campos != null)
            {
                //nombreDoc = campos.NombreCorto;
                //if(nombreDoc.Length > 31)
                //{
                //    nombreDoc = nombreDoc.Substring(0,30);
                //}
                var workbooksheets = workbook.AddWorksheet(nombreDoc);
                workbooksheets.Cell($"A{Row}").SetValue(campos.IdPlantillaPNT.ToString());
                workbooksheets.Row(Row).Height = 0;
                Row++;

                //TITULOS
                workbooksheets.Range($"A{Row}:C{Row}")
                                         .Merge()
                                         .SetValue("TITULO")
                                         .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                         .Font.SetFontSize(FontSize)
                                         .Font.SetBold(true);

                workbooksheets.Range($"D{Row}:F{Row}")
                                        .Merge()
                                        .SetValue("NOMBRE CORTO")
                                        .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                        .Font.SetFontSize(FontSize);

                workbooksheets.Range($"G{Row}:I{Row}")
                                       .Merge()
                                       .SetValue("DESCRIPCIÓN")
                                       .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                       .Font.SetFontSize(FontSize);
                //Estilo
                workbooksheets.Range($"A{Row}:I{Row}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                workbooksheets.Range($"A{Row}:I{Row}").Style
                          .Font.SetFontColor(XLColor.White)
                          .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                          .Border.SetTopBorder(XLBorderStyleValues.Thin)
                          .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                          .Border.SetRightBorder(XLBorderStyleValues.Thin)
                          .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                          .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                Row++;
                workbooksheets.Range($"A{Row}:C{Row}")
                                   .Merge()
                                   .SetValue(campos.NombreLargo)
                                   .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                                   .Font.SetFontSize(FontSize);

                workbooksheets.Range($"D{Row}:F{Row}")
                                   .Merge()
                                   .SetValue(campos.NombreCorto)
                                   .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                                   .Font.SetFontSize(FontSize);

                workbooksheets.Range($"G{Row}:I{Row}")
                                       .Merge()
                                       .SetValue(campos.Ayuda)
                                       .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                                       .Font.SetFontSize(FontSize);
                workbooksheets.Range($"A{Row}:I{Row}").Style
                    .Font.SetFontColor(XLColor.Black)
                    .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                    .Border.SetTopBorder(XLBorderStyleValues.Thin)
                    .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                    .Border.SetRightBorder(XLBorderStyleValues.Thin)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                workbooksheets.Range($"A{Row}:I{Row}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
                Row++;
                //iniciamos column para usarlo en las configuraciones
                var column = 1;

                //Ponemos los tipo de Campos PNT
                if (campos.campos != null  && campos.campos.Count > 0)
                {
                    foreach (var item in campos.campos)
                    {

                        workbooksheets.Range(Row, column, Row, column)
                          .Merge()
                          .SetValue(item.IdTipoCampoPNT.ToString());
                        column++;

                    }
                }
                //ajustamos la row en 0 para que no se vea
                workbooksheets.Row(Row).Height = 0;
                Row++;
                column = 1;
                //Ponemos el id de los campos PNT
                if (campos.campos != null && campos.campos.Count > 0)
                {
                    foreach (var item in campos.campos)
                    {

                        workbooksheets.Range(Row, column, Row, column)
                          .Merge()
                          .SetValue(item.IdCampoPNT.ToString());
                        column++;

                    }
                }
                //ajustamos la row en 0 para que no se vea
                workbooksheets.Row(Row).Height = 0;
                Row++;
                column = 1;
                //Row sin texto gray dark
                workbooksheets.Range(Row, 1, Row, campos.campos != null ? campos.campos.Count : 1)
                                      .Merge()
                                      .SetValue("")
                                      .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                      .Font.SetFontSize(FontSize)
                                      .Font.SetBold(true);
                workbooksheets.Range(Row, 1, Row, campos.campos != null ? campos.campos.Count : 1).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                Row++;
                //nos ayuda ContadorSheet a contar las tab que llevamos
                var ContadorSheet = 1;
                //Ponemos los nombre de los campos
                if (campos.campos != null && campos.campos.Count > 0)
                {
                    foreach (var item in campos.campos)
                    {
                        var catalogoName = "";
                        if (item.TipoCampo == TipoCampo.Catalogo)
                        {

                            if (item.TablaCatalogo)
                            {
                                catalogoName = $" (TABLA_{ContadorSheet})";
                                ContadorSheet++;
                            }
                            else
                            {
                                catalogoName = $" (CATALOGO_{ContadorSheet})";
                                ContadorSheet++;
                            }

                        }

                        workbooksheets.Range(Row, column, Row, column)
                          .Merge()
                          .SetValue(item.Etiqueta + catalogoName);
                        column++;
                        workbooksheets.Columns(column, column).Width = 30;
                        workbooksheets.Cell(Row, column).Style.Alignment.SetWrapText();
                        workbooksheets.Row(Row).Height = 60;
                        //IxLworkbooksheet

                    }
                }
                workbooksheets.Range(Row, 1, Row, (column - 1)).Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
                workbooksheets.Range(Row, 1, Row, (column - 1)).Style.Alignment.WrapText = true;
                //STILO DE LA TABLA
                workbooksheets.Range(Row, 1, Row, (column - 1)).Style
                             .Font.SetFontSize(FontSize)
                             .Font.SetFontColor(XLColor.Black)
                             //.Fill.SetBackgroundColor(XLColor.Cream)
                             .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                             .Border.SetTopBorder(XLBorderStyleValues.Thin)
                             .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                             .Border.SetRightBorder(XLBorderStyleValues.Thin)
                             .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                             .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);


                ///////creamos los tab de los catalogos o tablas///////
                ContadorSheet = 1;
                if(campos.campos != null)
                {
                    foreach (var item in campos.campos.Where(x => x.TipoCampo == TipoCampo.Catalogo).ToList())
                    {
                        var RowCatalogo = 0;
                        var RowCatalogoPNT = 0;
                        //var NombreTabla = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(item.CatalogoId);
                        //var NombreColumna = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(item.CatalogoId, true);
                        List<RespuestaQuery> LstCatalogo = HMTLHelperExtensions.GetListCatalogos(item.NombreTablaCatalogo, item.NombreCampoCatalago);
                        //var mCatalogo = db.Catalogoes.Where(x => x.CatalogoId == item.CatalogoId).FirstOrDefault();
                        //var NombreCatalogo = mCatalogo != null ? mCatalogo.Nombre : "No especificado";
                        //Empezamos los demas hojas
                        //var ws = workbook.Worksheets.Add($"CAT{ContadorSheet}");
                        //var ws1 = workbook.Worksheets.Where(x => x.Name == "CAT{ContadorSheet}").FirstOrDefault();





                        //subtitulos de los catalogos
                        RowCatalogo++;
                        if (item.TablaCatalogo)
                        {
                            int CatalogoHidden = 1;
                            string sTablaNoimbre = $"TABLA_{ContadorSheet}";
                            var ws = workbook.Worksheets.Add($"TABLA_{ContadorSheet}");

                            //var campos = db.CampoCatalogo.Where(x => x.CatalogoId == item.CatalogoId && x.Activo).ToList();
                            if (item.camposTabla.Count > 0)
                            {
                                int iColum = 1;
                                int iCountCampos = item.camposTabla.Count;
                                //ws.Range(RowCatalogo, iColum, RowCatalogo, iCountCampos)
                                //                               .Merge()
                                //                               .SetValue($"Tabla {item.NombreCatalogo}")
                                //                               .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                //                               .Font.SetFontSize(FontSize)
                                //                               .Font.SetBold(true);
                                //ws.Range(RowCatalogo, iColum, RowCatalogo, iCountCampos).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                                //ws.Range(RowCatalogo, iColum, RowCatalogo, iCountCampos).Style
                                //          .Font.SetFontColor(XLColor.White)
                                //          .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                //          .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                //          .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                //          .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                //          .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                //          .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                RowCatalogo = 3;
                                iColum--;

                                foreach (var camposCatalogo in item.camposTabla)
                                {
                                    iColum++;
                                    ws.Range(1, iColum, 1, iColum)
                                      .Merge()
                                      .SetValue(camposCatalogo.IdTipoCampoPNT.ToString());
                                    ws.Range(2, iColum, 2, iColum)
                                      .Merge()
                                      .SetValue(camposCatalogo.IdCampoPNT.ToString());


                                    ws.Range(RowCatalogo, iColum, RowCatalogo, iColum)
                                              .Merge()
                                              .SetValue($"{camposCatalogo.Etiqueta}")
                                              .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                              .Font.SetFontSize(FontSize)
                                              .Font.SetBold(true);


                                    //////////////////////////////////////////////////////////////// catalogos de tablas ////////////////////////////////////////////////
                                    if (camposCatalogo.TipoCampo == TipoCampo.Catalogo)
                                    {
                                        int RowCatalogo_tabla_catalogo = 0;
                                        RowCatalogo_tabla_catalogo++;

                                        var ws_catalgogo = workbook.Worksheets.Add($"CATALOGO_{sTablaNoimbre}_{CatalogoHidden}");
                                        ws_catalgogo.Range($"A{RowCatalogo_tabla_catalogo}:B{RowCatalogo_tabla_catalogo}")
                                                            .Merge()
                                                            .SetValue($"CATALOGO: {item.NombreCatalogo}")
                                                            .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                                            .Font.SetFontSize(FontSize)
                                                            .Font.SetBold(true);
                                        ws_catalgogo.Range($"A{RowCatalogo_tabla_catalogo}:B{RowCatalogo_tabla_catalogo}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                                        ws_catalgogo.Range($"A{RowCatalogo_tabla_catalogo}:B{RowCatalogo_tabla_catalogo}").Style
                                                  .Font.SetFontColor(XLColor.White)
                                                  .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                                  .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                                  .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                                  .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                                  .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                        ws_catalgogo.Range($"A{RowCatalogo_tabla_catalogo}")
                                                           .Merge()
                                                           .SetValue($"No.")
                                                           .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                                           .Font.SetFontSize(FontSize)
                                                           .Font.SetBold(true);
                                        ws_catalgogo.Range($"B{RowCatalogo_tabla_catalogo}")
                                                          .Merge()
                                                          .SetValue($"Valor")
                                                          .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                                          .Font.SetFontSize(FontSize)
                                                          .Font.SetBold(true);


                                        ws_catalgogo.Range($"A{RowCatalogo_tabla_catalogo}:B{RowCatalogo_tabla_catalogo}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                                        ws_catalgogo.Range($"A{RowCatalogo_tabla_catalogo}:B{RowCatalogo_tabla_catalogo}").Style
                                                  .Font.SetFontColor(XLColor.White)
                                                  .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                                  .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                                  .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                                  .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                                  .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                        //Campos valores
                                        RowCatalogo_tabla_catalogo++;
                                        var contador = 1;
                                        var LstCatalogo_tabla = HMTLHelperExtensions.GetListCatalogos(camposCatalogo.iCatalogoId);
                                        foreach (var campoCatalogo in LstCatalogo_tabla)
                                        {

                                            ws_catalgogo.Range(RowCatalogo_tabla_catalogo, 1, RowCatalogo_tabla_catalogo, 1)
                                             .Merge()
                                             .SetValue(contador);
                                            column++;
                                            ws_catalgogo.Columns(column, column).Width = 30;

                                            ws_catalgogo.Range(RowCatalogo_tabla_catalogo, 2, RowCatalogo_tabla_catalogo, 2)
                                             .Merge()
                                             .SetValue(campoCatalogo.Valor);
                                            column++;
                                            ws_catalgogo.Columns(column, column).Width = 30;
                                            RowCatalogo_tabla_catalogo++;
                                            contador++;


                                        }
                                        RowCatalogo_tabla_catalogo = (RowCatalogo_tabla_catalogo - 1);
                                        if (RowCatalogo_tabla_catalogo >= 2)
                                        {
                                            ws_catalgogo.Range(2, 1, RowCatalogo_tabla_catalogo, 2).Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
                                            ws_catalgogo.Range(2, 1, RowCatalogo_tabla_catalogo, 2).Style.Alignment.WrapText = true;
                                            //STILO DE LA TABLA
                                            ws_catalgogo.Range(2, 1, RowCatalogo_tabla_catalogo, 2).Style
                                                         .Font.SetFontSize(FontSize)
                                                         .Font.SetFontColor(XLColor.Black)
                                                         //.Fill.SetBackgroundColor(XLColor.Cream)
                                                         .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                                         .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                                         .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                                         .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                                         .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                                         .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                        }
                                        ws_catalgogo.Columns(1, 2).Width = 30;
                                        CatalogoHidden++;

                                    }
                                }
                                ws.Range(RowCatalogo, 1, RowCatalogo, iColum).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                                ws.Range(RowCatalogo, 1, RowCatalogo, iColum).Style
                                          .Font.SetFontColor(XLColor.White)
                                          .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                          .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                          .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                          .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                          .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                          .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                                ws.Columns(1, iColum).Width = 30;
                                //rows invisibles


                                ws.Row(1).Height = 0;
                                ws.Row(2).Height = 0;


                            }


                        }
                        else
                        {
                            var ws = workbook.Worksheets.Add($"CATALOGO_{ContadorSheet}");

                            ws.Range($"A{RowCatalogo}:B{RowCatalogo}")
                                                            .Merge()
                                                            .SetValue($"CATALOGO: {item.NombreCatalogo}")
                                                            .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                                            .Font.SetFontSize(FontSize)
                                                            .Font.SetBold(true);
                            ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                            ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style
                                      .Font.SetFontColor(XLColor.White)
                                      .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                      .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                      .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                      .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                      .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                      .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            ws.Range($"A{RowCatalogo}")
                                               .Merge()
                                               .SetValue($"No.")
                                               .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                               .Font.SetFontSize(FontSize)
                                               .Font.SetBold(true);
                            ws.Range($"B{RowCatalogo}")
                                              .Merge()
                                              .SetValue($"Valor")
                                              .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                              .Font.SetFontSize(FontSize)
                                              .Font.SetBold(true);


                            ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                            ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style
                                      .Font.SetFontColor(XLColor.White)
                                      .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                      .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                      .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                      .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                      .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                      .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            //Campos valores
                            RowCatalogo++;
                            var contador = 1;
                            foreach (var campoCatalogo in LstCatalogo)
                            {

                                ws.Range(RowCatalogo, 1, RowCatalogo, 1)
                                 .Merge()
                                 .SetValue(contador);
                                column++;
                                ws.Columns(column, column).Width = 30;

                                ws.Range(RowCatalogo, 2, RowCatalogo, 2)
                                 .Merge()
                                 .SetValue(campoCatalogo.Valor);
                                column++;
                                ws.Columns(column, column).Width = 30;
                                RowCatalogo++;
                                contador++;


                            }
                            RowCatalogo = (RowCatalogo - 1);
                            if (RowCatalogo >= 2)
                            {
                                ws.Range(2, 1, RowCatalogo, 2).Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
                                ws.Range(2, 1, RowCatalogo, 2).Style.Alignment.WrapText = true;
                                //STILO DE LA TABLA
                                ws.Range(2, 1, RowCatalogo, 2).Style
                                             .Font.SetFontSize(FontSize)
                                             .Font.SetFontColor(XLColor.Black)
                                             //.Fill.SetBackgroundColor(XLColor.Cream)
                                             .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                             .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                             .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                             .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                             .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                             .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                            }
                            ws.Columns(1, 2).Width = 30;
                        }
                        ContadorSheet++;
                    }
                }
      
                // poner los datos que necesitamos


                //Response.Cookies.Add(new HttpCookie(CookieName, "1") { Expires = DateTime.Now.AddSeconds(1) });

                ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("dlc", CookieName));

            }

            return new ExcelResult(workbook, !string.IsNullOrEmpty(campos.NombreCorto) ? campos.NombreCorto.Trim() : "Excel_no_enconrado");
        }
     
        public ActionResult GenerarPlantillaExcel(List<Campo> LstCampos, Plantilla mPlantilla, string CookieName)
        {
            int Row = 1;
            // Generate the workbook...
            var workbook = new XLWorkbook();
            string nombreDoc = "Reporte de Formatos"; //mPlantilla != null ? mPlantilla.NombreCorto : "NoEspecificado";
            var workbooksheets = workbook.AddWorksheet(nombreDoc);

            var FontSize = 11;
            //IdPlantillaPNT
            workbooksheets.Cell($"A{Row}")
                                    .SetValue(mPlantilla?.IdPlantillaPNT?.ToString());
            workbooksheets.Row(Row).Height = 0;
            Row++;
            //TITULOS
            workbooksheets.Range($"A{Row}:C{Row}")
                                     .Merge()
                                     .SetValue("TITULO")
                                     .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                     .Font.SetFontSize(FontSize)
                                     .Font.SetBold(true);

            workbooksheets.Range($"D{Row}:F{Row}")
                                    .Merge()
                                    .SetValue("NOMBRE CORTO")
                                    .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                    .Font.SetFontSize(FontSize);

            workbooksheets.Range($"G{Row}:I{Row}")
                                   .Merge()
                                   .SetValue("DESCRIPCIÓN")
                                   .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                   .Font.SetFontSize(FontSize);
            //Estilo
            workbooksheets.Range($"A{Row}:I{Row}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
            workbooksheets.Range($"A{Row}:I{Row}").Style
                      .Font.SetFontColor(XLColor.White)
                      .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                      .Border.SetTopBorder(XLBorderStyleValues.Thin)
                      .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                      .Border.SetRightBorder(XLBorderStyleValues.Thin)
                      .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                      .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            Row++;
            if (mPlantilla != null)
            {
                workbooksheets.Range($"A{Row}:C{Row}")
                                   .Merge()
                                   .SetValue(mPlantilla.NombreLargo)
                                   .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                                   .Font.SetFontSize(FontSize);

                workbooksheets.Range($"D{Row}:F{Row}")
                                   .Merge()
                                   .SetValue(mPlantilla.NombreCorto)
                                   .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                                   .Font.SetFontSize(FontSize);

                workbooksheets.Range($"G{Row}:I{Row}")
                                       .Merge()
                                       .SetValue(mPlantilla.Ayuda)
                                       .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                                       .Font.SetFontSize(FontSize);

            }
            workbooksheets.Range($"A{Row}:I{Row}").Style
                     .Font.SetFontColor(XLColor.Black)
                     .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                     .Border.SetTopBorder(XLBorderStyleValues.Thin)
                     .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                     .Border.SetRightBorder(XLBorderStyleValues.Thin)
                     .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                     .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            workbooksheets.Range($"A{Row}:I{Row}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
            Row++;

            //iniciamos column para usarlo en las configuraciones
            var column = 1;

            //Ponemos los tipo de Campos PNT
            if (LstCampos.Count > 0)
            {
                foreach (var item in LstCampos)
                {

                    workbooksheets.Range(Row, column, Row, column)
                      .Merge()
                      .SetValue(item?.IdTipoCampoPNT?.ToString());
                    column++;

                }
            }
            //ajustamos la row en 0 para que no se vea
            workbooksheets.Row(Row).Height = 0;
            Row++;
            column = 1;
            //Ponemos el id de los campos PNT
            if (LstCampos.Count > 0)
            {
                foreach (var item in LstCampos)
                {

                    workbooksheets.Range(Row, column, Row, column)
                      .Merge()
                      .SetValue(item?.IdCampoPNT?.ToString());
                    column++;

                }
            }
            //ajustamos la row en 0 para que no se vea
            workbooksheets.Row(Row).Height = 0;
            Row++;
            column = 1;
            //Row sin texto gray dark
            workbooksheets.Range(Row, 1, Row, LstCampos.Count)
                                  .Merge()
                                  .SetValue("")
                                  .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                  .Font.SetFontSize(FontSize)
                                  .Font.SetBold(true);
            workbooksheets.Range(Row, 1, Row, LstCampos.Count).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
            Row++;

            //Ponemos los nombre de los campos
            if (LstCampos.Count > 0)
            {
                foreach (var item in LstCampos)
                {

                    workbooksheets.Range(Row, column, Row, column)
                      .Merge()
                      .SetValue(item.Etiqueta);
                    column++;
                    workbooksheets.Columns(column, column).Width = 30;
                }
            }
            workbooksheets.Range(Row, 1, Row, (column - 1)).Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
            workbooksheets.Range(Row, 1, Row, (column - 1)).Style.Alignment.WrapText = true;


            //STILO DE LA TABLA
            workbooksheets.Range(Row, 1, Row, (column - 1)).Style
                         .Font.SetFontSize(FontSize)
                         .Font.SetFontColor(XLColor.Black)
                         //.Fill.SetBackgroundColor(XLColor.Cream)
                         .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                         .Border.SetTopBorder(XLBorderStyleValues.Thin)
                         .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                         .Border.SetRightBorder(XLBorderStyleValues.Thin)
                         .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                         .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);



            var ContadorSheet = 1;
            foreach (var item in LstCampos.Where(x => x.TipoCampo == TipoCampo.Catalogo).ToList())
            {
                var RowCatalogo = 0;
                var NombreTabla = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(item.CatalogoId);
                var NombreColumna = HMTLHelperExtensions.GetNombreTablaOCampoCatalago(item.CatalogoId, true);
                List<RespuestaQuery> LstCatalogo = HMTLHelperExtensions.GetListCatalogos(NombreTabla, NombreColumna);
                var mCatalogo = db.Catalogoes.Where(x => x.CatalogoId == item.CatalogoId).FirstOrDefault();
                var NombreCatalogo = mCatalogo != null ? mCatalogo.Nombre : "No especificado";
                //Empezamos los demas hojas
                var ws = workbook.Worksheets.Add($"CAT{ContadorSheet}");
                //var ws1 = workbook.Worksheets.Where(x => x.Name == "CAT{ContadorSheet}").FirstOrDefault();





                //subtitulos de los catalogos
                RowCatalogo++;
                if (HMTLHelperExtensions.GetTipoTabla(item.CatalogoId))
                {
                    var campos = db.CampoCatalogo.Where(x => x.CatalogoId == item.CatalogoId && x.Activo).ToList();
                    if (campos.Count > 0)
                    {
                        int iColum = 1;
                        ws.Range(RowCatalogo, iColum, RowCatalogo, campos.Count)
                                                       .Merge()
                                                       .SetValue($"CATALOGO: {NombreCatalogo}")
                                                       .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                                       .Font.SetFontSize(FontSize)
                                                       .Font.SetBold(true);
                        ws.Range(RowCatalogo, iColum, RowCatalogo, campos.Count).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                        ws.Range(RowCatalogo, iColum, RowCatalogo, campos.Count).Style
                                  .Font.SetFontColor(XLColor.White)
                                  .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                  .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                  .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                  .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                  .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        RowCatalogo++;
                        iColum--;

                        foreach (var camposCatalogo in campos)
                        {
                            iColum++;
                            ws.Range(RowCatalogo, iColum, RowCatalogo, iColum)
                                      .Merge()
                                      .SetValue($"{camposCatalogo.Etiqueta}")
                                      .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                      .Font.SetFontSize(FontSize)
                                      .Font.SetBold(true);


                        }
                        ws.Range(RowCatalogo, 1, RowCatalogo, iColum).Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                        ws.Range(RowCatalogo, 1, RowCatalogo, iColum).Style
                                  .Font.SetFontColor(XLColor.White)
                                  .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                  .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                  .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                  .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                  .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                  .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        ws.Columns(1, iColum).Width = 30;

                    }


                }
                else
                {
                    ws.Range($"A{RowCatalogo}:B{RowCatalogo}")
                                                    .Merge()
                                                    .SetValue($"CATALOGO: {NombreCatalogo}")
                                                    .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                                    .Font.SetFontSize(FontSize)
                                                    .Font.SetBold(true);
                    ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                    ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style
                              .Font.SetFontColor(XLColor.White)
                              .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                              .Border.SetTopBorder(XLBorderStyleValues.Thin)
                              .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                              .Border.SetRightBorder(XLBorderStyleValues.Thin)
                              .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                              .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    ws.Range($"A{RowCatalogo}")
                                       .Merge()
                                       .SetValue($"No.")
                                       .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                       .Font.SetFontSize(FontSize)
                                       .Font.SetBold(true);
                    ws.Range($"B{RowCatalogo}")
                                      .Merge()
                                      .SetValue($"Valor")
                                      .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                      .Font.SetFontSize(FontSize)
                                      .Font.SetBold(true);


                    ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style.Fill.SetBackgroundColor(XLColor.FromArgb(44, 39, 38));
                    ws.Range($"A{RowCatalogo}:B{RowCatalogo}").Style
                              .Font.SetFontColor(XLColor.White)
                              .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                              .Border.SetTopBorder(XLBorderStyleValues.Thin)
                              .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                              .Border.SetRightBorder(XLBorderStyleValues.Thin)
                              .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                              .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    //Campos valores
                    RowCatalogo++;
                    var contador = 1;
                    foreach (var campoCatalogo in LstCatalogo)
                    {

                        ws.Range(RowCatalogo, 1, RowCatalogo, 1)
                         .Merge()
                         .SetValue(contador);
                        column++;
                        ws.Columns(column, column).Width = 30;

                        ws.Range(RowCatalogo, 2, RowCatalogo, 2)
                         .Merge()
                         .SetValue(campoCatalogo.Valor);
                        column++;
                        ws.Columns(column, column).Width = 30;
                        RowCatalogo++;
                        contador++;


                    }
                    RowCatalogo = (RowCatalogo - 1);
                    if (RowCatalogo >= 2)
                    {
                        ws.Range(2, 1, RowCatalogo, 2).Style.Fill.SetBackgroundColor(XLColor.FromArgb(214, 214, 214));
                        ws.Range(2, 1, RowCatalogo, 2).Style.Alignment.WrapText = true;
                        //STILO DE LA TABLA
                        ws.Range(2, 1, RowCatalogo, 2).Style
                                     .Font.SetFontSize(FontSize)
                                     .Font.SetFontColor(XLColor.Black)
                                     //.Fill.SetBackgroundColor(XLColor.Cream)
                                     .Border.SetBottomBorder(XLBorderStyleValues.Thin)
                                     .Border.SetTopBorder(XLBorderStyleValues.Thin)
                                     .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                                     .Border.SetRightBorder(XLBorderStyleValues.Thin)
                                     .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                     .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    }
                    ws.Columns(1, 2).Width = 30;
                }
                ContadorSheet++;
            }

            //Response.Cookies.Add(new HttpCookie(CookieName, "1") { Expires = DateTime.Now.AddSeconds(1) });

            ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("dlc", CookieName));
            return new ExcelResult(workbook, mPlantilla.NombreCorto.Trim());
        }



        #region formatos
        public string TituloFromato(string value)
        {
            try
            {
                string stringNuevo = "";
                var excepciones = new[] { "de", "del", "la", "las", "al", "en", "el", "con", "lo", "los", "que", "se", "les", "y", "o", "u", "a", "e", "y/o", "s.a", "s.a.", "c.v.", "c.v", "a.c.", "a.c" };
                value = value.Replace("_", " ");
                var ListString = value.Split(' ');
                var firstChar = true;
                var firstWord = true;
                foreach (var item in ListString)
                {

                    stringNuevo += $@"{(!firstChar ? " " : "")}";
                    firstChar = false;
                    if (!IsRoman(item) && !IsInciso(item) && firstWord)
                    {
                        stringNuevo += item.Substring(0, 1).ToUpper() + item.Substring(1).ToLower();
                        firstWord = false;
                    }
                    else if (IsRoman(item) || IsInciso(item))
                    {
                        stringNuevo += item.ToUpper();
                    }
                    else if (excepciones.Where(x => x.Contains(item.ToLower())).Count() > 0 && !firstWord)
                    {
                        stringNuevo += item.ToLower();
                    }
                    else
                    {
                        stringNuevo += item.Substring(0, 1).ToUpper() + item.Substring(1).ToLower();
                    }
                }
                return stringNuevo;
            }
            catch (Exception ex)
            {
                
                return ex.Message;
                return value;
            }

        }


        public bool IsInciso(string value)
        {
            Regex r = new Regex("^[A-Z]+[)]$");
            value = value.Replace(".", "");
            return r.IsMatch(value);
        }

        public bool IsRoman(string value)
        {
            Regex r = new Regex("^M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$");
            value = value.Replace(".", "");
            return r.IsMatch(value);
        }
        #endregion

        #region NuevoImportarExcel

        //Generar2
        [HttpPost]
        public ActionResult ProcesarExcel(HttpPostedFileBase ExcelFile, int iId, bool bReemplazar, int PeriodoId, int sysFrecuencia, int sysNumFrecuencia=0)
        { 
            try
            {
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var OrganismoId = usuario.OrganismoID ?? 0;
                //error si los datos vienen vacios.
                if (iId == 0 || PeriodoId == 0 || sysFrecuencia == 0)
                    return Json(new { Hecho = false, Mensaje = $"Error: 'Los valores como plantilla, Periodo, Frecuecia no pueden estar vacios.' " }, JsonRequestBehavior.AllowGet);

                //inicializamos el excel
                IExcelDataReader reader = null;
                //Verificamos la extension y leemos dependiendo del archivo
                if (Path.GetExtension(ExcelFile.FileName).Equals(".xls") || Path.GetExtension(ExcelFile.FileName).Equals(".XLS"))
                    reader = ExcelReaderFactory.CreateBinaryReader(ExcelFile.InputStream);
                else if (Path.GetExtension(ExcelFile.FileName).Equals(".xlsx") || Path.GetExtension(ExcelFile.FileName).Equals(".XLSX"))
                    reader = ExcelReaderFactory.CreateOpenXmlReader(ExcelFile.InputStream);
                if (reader != null)
                {
                    //comenzamos a procesar y convertir los datos a data Set
                    // Se cconfigura pra leer el excel y los datos de las pestañas
                    var confPrincipal = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true,
                            FilterRow = rowReader => rowReader.Depth >= 1
                        }
                    };
                    var confTablas = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true,
                            FilterRow = rowReader => rowReader.Depth >= 3
                        }
                    };
                    var dataSetPrincipal = reader.AsDataSet(confPrincipal);
                    var dataSetTablas = reader.AsDataSet(confTablas);
                    DataTable csvData = new DataTable();

                    //comenzamos con validar la información
                    //primero validamos el excel 
                    var campos = this._GetCampos(iId);
                    var Errores = this._validarExcel(campos.campos, dataSetPrincipal, dataSetTablas, PeriodoId);
                    if (Errores.Count > 0)
                    {
                        //ViewBag.ColumnsNames = ColumnsNames;
                        string viewContent = ConvertViewToString("_RowsExcelImport", Errores);
                        //return PartialView("_RowsExcelImport", CamposConErrores);
                        var json = Json(new { Hecho = true, ConErrores = true, Partial = viewContent }, JsonRequestBehavior.AllowGet);
                        //configuramos mas length para que no nos den error de longitud
                        json.MaxJsonLength = 500000000;
                        return json;
                    }
                    //despues de validad y no hay cambios pues entonces proedemos a remplazar si es el caso
                    if (bReemplazar)
                    {
                        var reemplazar = this.RemplazarInformacion(campos.NombreTabla, OrganismoId, PeriodoId, sysFrecuencia, sysNumFrecuencia, usuario.Id);
                        if (reemplazar.Length > 0)
                            return Json(new { Hecho = false, Mensaje = $"Error: '{reemplazar}' " }, JsonRequestBehavior.AllowGet);
                    }

                    //insertamos la informacion
                    var respuesta = this._insert(campos, dataSetPrincipal.Tables[0], OrganismoId, PeriodoId, sysFrecuencia, sysNumFrecuencia, usuario.Id);
                    //revisamos si tieneserroees

                    if (respuesta.sError.Length > 0)
                    {
                        return Json(new { Hecho = false, Mensaje = $"Error: '{respuesta.sError}' " }, JsonRequestBehavior.AllowGet);
                    }

                    //entonces nos enfocamos en insertar lo datos de tablas
                    string sError = "";
                    var dsTablas = this.GetDSDeTabla(respuesta.dtTable, dataSetTablas, campos.campos);
                    if (dsTablas.sError.Length <= 0)
                    {
                        int iContador = 0;
                        foreach (var item in campos.campos.Where(x => x.TipoCampo == TipoCampo.Catalogo && x.TablaCatalogo))
                        {
                            if (dsTablas.dsTable.Tables.Count > iContador)
                            {
                                if (dsTablas.dsTable.Tables[iContador].TableName == item.NombreTablaCatalogo)
                                {
                                    var respuestaTable = this._insertTabla(item.camposTabla, dsTablas.dsTable.Tables[iContador], item.NombreTablaCatalogo,OrganismoId,usuario.Id);
                                    if (respuestaTable.sError.Length > 0)
                                        sError += $"{(sError.Length > 0 ? "," : "")}[{dsTablas.dsTable.Tables[iContador].TableName}]: "+ respuestaTable.sError;
                                    iContador++;
                                }
                            }
                        }
                        if(sError.Length > 0)
                        {
                            sError += "Se encontraron observaciones en las siguientes Tablas: "+sError;
                            return Json(new { Hecho = false, Mensaje = $"Error: '{sError}' " }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { Hecho = false, Mensaje = $"Error: '{dsTablas.sError}' " }, JsonRequestBehavior.AllowGet);
                    }

                    //si llega hasta aqui significa que todo salio bien
                    return Json(new { Hecho = true, Mensaje = "" }, JsonRequestBehavior.AllowGet);



                }

            }
            catch(Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = $"Error: '{ex.Message}' " }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Hecho = false, Mensaje = $"Error: '' " }, JsonRequestBehavior.AllowGet);
        }

        private List<AuxErroresEcxel> _validarExcel(List<vmCampoForDatos> campos, DataSet dataSetPrincipal, DataSet dataSetTablas,int PeriodoId = 0)
        {
            var listErrors = new HashSet<string>();
            var CamposConErrores = new List<AuxErroresEcxel>();
            var temporalCampo = new Campo();
            var temporalLstCampo = new List<Campo>();
            int NoRow = 0;
            bool bError = false;

            

            try
            {
                var Periodo = db.Periodos.Where(x => x.PeriodoId == PeriodoId).FirstOrDefault().NombrePeriodo;
                DataRowCollection row = dataSetPrincipal.Tables[0].Rows;
                //DataColumnCollection col = dataSet.Tables["Sheet1"].Columns; // otra manera
                //List<object> allRowsList = new List<object>(); //despues lo podemos usar.

                //inicializamos variables
                bool FormatoDiferente = false;
                //  var OtherFormatExcel = ws1.Range(5, 1, 5, 1).Search("ID").Count() > 0 || ws1.Range(7, 1, 7, 1).IsEmpty() ? true : false;
                for (int i = 0; i < row.Count; i++)
                {
                    temporalLstCampo = new List<Campo>();
                    //hacemos validacion pero queda pendiente revisar el i == 4
                    if (
                        (i == 3 && row[i].ItemArray[0].ToString() == "ID")
                        ||
                         (i == 5 && row[i].ItemArray[0].ToString() == "")
                        )
                    {
                        i++;
                        FormatoDiferente = true;
                    }
                    if (i >= 6)
                    {
                        NoRow = i;
                        //verificamos si tiene Id o si empieza del 7
                        //Aqui empieza cada rows
                        int iColumn = FormatoDiferente ? 1 : 0;
                        int iCatalogoPosition = 0;
                        for (int x = 0; x < campos.Count; x++)
                        {
                           
                            //creamos la información
                            //analizamos los errores!
                            temporalCampo = new Campo();
                            
                            //contamos las posiciones de los catalogos
                            var tipoCampo = campos[x].TipoCampo;
                            iCatalogoPosition = tipoCampo == TipoCampo.Catalogo ? iCatalogoPosition + 1 : iCatalogoPosition;

                            //Validamos
                            string error = this._GetValidarCampo(campos[x], row[i].ItemArray[iColumn].ToString(), dataSetTablas, iCatalogoPosition, FormatoDiferente);
                            if (x == 0 && error.Length <= 0)
                            {
                                error = row[i].ItemArray[iColumn].ToString() != Periodo ? "La columna de Ejercicio no coincide con el periodo seleccionado. " : "";
                            }
                            //verificamos si hay un error para mostrar
                            temporalCampo.ExcelValidacion = false;
                            if (error.Length > 0)
                            {
                                bError = true;
                                temporalCampo.ExcelValidacion = true;
                            }

                            temporalCampo.TipoCampo = campos[x].TipoCampo;
                            temporalCampo.Etiqueta = campos[x].Etiqueta;
                            temporalCampo.ExcelValor = row[i].ItemArray[iColumn].ToString();
                            
                            temporalCampo.ExcelErrorTxt = error;
                            temporalLstCampo.Add(temporalCampo);

                            if (tipoCampo == TipoCampo.Catalogo)
                            {

                                if (!campos[x].TablaCatalogo)
                                {
                                    var dato = ListaCatalogos.Where(cat => cat.Key == campos[x].CatalogoId && cat.Value.Value.ToLower() == row[i].ItemArray[iColumn].ToString().ToLower()).FirstOrDefault();
                                    dataSetPrincipal.Tables[0].Rows[i].SetField(iColumn, dato.Value.Key);
                                }
                                else
                                {
                                    //////Hacemos los mismos con los catalogos de las tablas
                                    //for (int xi = 0; xi < campos[x].camposTabla.Count; xi++)
                                    //{
                                    //    if (campos[x].camposTabla[xi].TipoCampo == TipoCampo.Catalogo)
                                    //    {
                                    //        var dato = ListaCatalogos.Where(cat => cat.Key == campos[x].camposTabla[xi].iCatalogoId && cat.Value.Value.ToLower() == row[i].ItemArray[iColumn].ToString().ToLower()).FirstOrDefault();
                                    //        dataSetPrincipal.Tables[0].Rows[irowCatalogo].SetField(iColumn, dato.Value.Key);
                                    //    }
                                    //}

                                    //cuatos catalogos tiene
                                    var totalCatalgoos = campos[x].camposTabla.Where(y => y.TipoCampo == TipoCampo.Catalogo).ToList();
                                    if (totalCatalgoos != null && totalCatalgoos.Count > 0)
                                    {
                                        iCatalogoPosition += totalCatalgoos.Count;
                                    }

                                }

                            }

                            //aumentamos el campo
                            iColumn++;
                        }

                        if (bError)
                        {
                            var mAuxErroresEcxel = new AuxErroresEcxel();
                            mAuxErroresEcxel.Campo = temporalLstCampo;
                            mAuxErroresEcxel.NoRenglon = i+2;
                            CamposConErrores.Add(mAuxErroresEcxel);
                            break;   
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var mAuxErroresEcxel = new AuxErroresEcxel();
                mAuxErroresEcxel.Campo = temporalLstCampo;
                mAuxErroresEcxel.NoRenglon = NoRow+2;
                CamposConErrores.Add(mAuxErroresEcxel);
            }
            return CamposConErrores;
        }

        private string _GetValidarCampo(vmCampoForDatos campo, string sValor, DataSet dataset, int iCatalogoPosition,bool FormatoDiferente = false)
        {
            var listErrors = new HashSet<string>();
            string sError = "";
            string TipoCampoName = campo.Etiqueta;
            try
            {
                Regex r = new Regex("^[a-zA-Z0-9\x20]*$");
                if (sValor.Length <= campo.Longitud ||
                    (campo.TipoCampo == TipoCampo.Fecha
                    || campo.TipoCampo == TipoCampo.Catalogo
                    || campo.TipoCampo == TipoCampo.ArchivoAdjunto
                    || campo.TipoCampo == TipoCampo.CasillaVerificacion
                    || campo.TipoCampo == TipoCampo.Hora))
                {
                    if (campo.Requerido)
                    {
                        if (campo.TipoCampo != TipoCampo.ArchivoAdjunto && campo.TipoCampo != TipoCampo.Catalogo)
                        {
                            if (sValor.Length == 0)
                            {
                                return $@"La columna { TipoCampoName }  es requerido y no cuenta con un valor.";
                            }
                        }
                        else if (campo.TipoCampo == TipoCampo.Catalogo)
                        {
                            if (sValor == "0" || sValor == "" || sValor == null)
                            {
                                return $@"La columna { TipoCampoName }  es requerido y no cuenta con un valor.";
                            }

                        }
                    }

                    if (sValor.Length > 0)
                    {
                        switch (campo.TipoCampo)
                        {
                            case TipoCampo.Alfanumerico:
                                //^[a-zA-Z0-9ñÑáÁéÉíÍóÓÓúÚüÜ“”,;=%$\n!¡¿?'""|().\/\/_@.#&+-‐	\\.\x20]*$
                                r = new Regex(@"^[a-zA-Z0-9ñÑáÁéÉíÍóÓÓúÚüÜ“”,;º’=%$\n!¡¿?'""|().\/\/_@.#&+-‐\\.\x20]*$");
                                if (!r.IsMatch(sValor.Trim()))
                                {
                                    sError =  $@"La columna {  TipoCampoName } solo admite letras y numeros.";
                                }
                                break;
                            case TipoCampo.CasillaVerificacion:
                                if (sValor.ToLower() != "true" && sValor.ToLower() != "false")
                                {
                                    sError = $@"La columna { TipoCampoName } solo admite las palabra: 'true' para indicar que es verdadero y 'false' para falso.";

                                }
                                break;
                            case TipoCampo.Catalogo:

                                if (sValor != "0")
                                {
                                    if (campo.TablaCatalogo)
                                    {
                                        var nombreColumna = dataset.Tables[iCatalogoPosition].Columns[0].ColumnName;
                                        var sheetExcelCatalogo = dataset.Tables[iCatalogoPosition].Select($"[{ nombreColumna }] = {sValor}");

                                        for (int i = 0; i < sheetExcelCatalogo.Length; i++)
                                        {
                                            var iDiff = false;
                                            var sumar = 0;
                                            if (FormatoDiferente && campo.camposTabla.Count < sheetExcelCatalogo[i].ItemArray.Length)
                                            {
                                                iDiff = true;
                                                dataset.Tables[iCatalogoPosition].Columns.RemoveAt(1);
                                            }

                                            for (int x = 0; x < campo.camposTabla.Count; x++)
                                            {

                                                sError = this._GetValidarCampo(campo.camposTabla[x], sheetExcelCatalogo[i].ItemArray[x].ToString());
                                                    if (sError != "")
                                                        return $"Pestaña: '{ dataset.Tables[iCatalogoPosition].TableName }', Valor: '{sheetExcelCatalogo[i].ItemArray[x]}', Linea: {i + 1}: " + sError;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!this._ExisteValorEnCatalogo(campo.CatalogoId, campo.NombreTablaCatalogo, campo.NombreCampoCatalago, sValor.ToLower()))
                                            sError = $"No se pudo encontrar el valor del catalogo '{sValor}' este valor no existe en la base de datos.";
                                    }
                                }
                                break;
                            case TipoCampo.Decimal:
                            case TipoCampo.Dinero:
                            case TipoCampo.Porcentaje:
                                if (campo.TipoCampo == TipoCampo.Porcentaje && campo._ConDecimales == false)
                                {
                                    long iiValue = 0;
                                    bool bIisInt = long.TryParse(sValor, out iiValue);
                                    sError = !bIisInt ? $@"La columna { TipoCampoName } es de tipo porcentaje  y en el formato de excel no cumple con el formato requerido" : "";
                                   

                                }
                                else
                                {
                                    if (sValor.Contains("-"))
                                    {
                                        decimal dValue = 0;
                                        bool bIsNumber = decimal.TryParse(sValor, out dValue);
                                        sError = !bIsNumber ? $@"La columna { TipoCampoName } es de tipo porcentaje  y en el formato de excel no cumple con el formato requerido" : "";
                                       
                                    }
                                    else
                                    {
                                        decimal dValue = 0;
                                        bool bIsNumber = decimal.TryParse(sValor, out dValue);
                                        sError = !bIsNumber ? $@"La columna { TipoCampoName } es de tipo porcentaje  y en el formato de excel no cumple con el formato requerido" : "";
                                        
                                    }


                                }


                                break;
                            case TipoCampo.email:
                                string PatternEmail = @"^(([^<>()[\]\\.,;:\s@""]+(\.[^<>()[\]\\.,;:\s@""]+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";
                                r = new Regex(PatternEmail, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                                //r = new Regex("^[A-z0-9_.@]*$");
                                if (!r.IsMatch(sValor.Trim()))
                                {
                                    sError = $@"La columna { TipoCampoName } es de tipo email  y en el formato de excel no cumple con el formato requerido";
                                }
                                break;
                            case TipoCampo.Fecha:
                                try
                                {
                                    sValor = sValor.Replace(" 12:00:00 AM", "");
                                    sValor = sValor.Replace(" 12:00:00 a. m.", "");
                                    string[] validDateTimeFormats = { "dd/MM/yyyy", "d/m/yyyy", "d/m/yy", "dd/m/yyyy", "dd/m/yy", "d/mm/yyyy", "d/mm/yy" };

                                    DateTime dtValue = new DateTime();
                                    bool bIsDate = DateTime.TryParseExact(sValor, validDateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtValue);
                                    sError = !bIsDate ? $@"La columna { TipoCampoName } es de tipo fecha  y en el formato de excel no cumple con el formato requerido" : "";
                                   

                                }
                                catch (Exception ex)
                                {
                                    sError = $@"La columna { TipoCampoName } es de tipo fecha y en el formato de excel no cumple con el formato requerido";
                                    
                                }
                                //DateTime dtValue = new DateTime();
                                //bool bIsDate = DateTime.TryParse(sValue, out dtValue);
                                //tBerror = !bIsDate ? true : tBerror;
                                break;
                            case TipoCampo.Hipervinculo:
                                //string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
                                //string Pattern = @"https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,}";
                                string Pattern = @"(http|https):\/\/";
                                r = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                                if (!r.IsMatch(sValor.Trim()))
                                {
                                    sError = $@"La columna { TipoCampoName } es de tipo hipervínculo  y en el formato de excel no cumple con el formato requerido";
                                }
                                break;
                            case TipoCampo.Hora:
                                r = new Regex(@"^(?:(?:0?[1-9]|1[0-2]):[0-5][0-9]\s?(?:[AP][Mm]?|[ap][m]?)?|(?:00?|1[3-9]|2[0-3]):[0-5][0-9])$");
                                if (!r.IsMatch(sValor.Trim()))
                                {
                                    try
                                    {
                                        DateTime vDateTime = new DateTime();
                                        bool itsDate = DateTime.TryParse(sValor, out vDateTime);
                                        if (itsDate)
                                        {
                                            string displayTime = vDateTime.ToString("hh:mm tt");
                                            if (r.IsMatch(displayTime))
                                            {
                                                sValor = displayTime;
                                            }

                                        }


                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                                break;
                            case TipoCampo.Numerico:
                                long iValue = 0;
                                bool bIsInt = long.TryParse(sValor, out iValue);
                                sError = !bIsInt ? $@"La columna { TipoCampoName } es de tipo numerico  y en el formato de excel no cumple con el formato requerido" : "";
                                

                                break;
                            case TipoCampo.Telefono:

                                break;
                            case TipoCampo.Texto:
                                r = new Regex("^[a-zA-Z\x20]*$");
                                if (!r.IsMatch(sValor.Trim()))
                                {
                                    sError = $@"La columna { TipoCampoName } solo admite letras.";
                                }
                                break;
                            default:

                                break;

                        }
                    }

                }
                else
                {
                    sError = $@"El valor cuenta con una longitud de {sValor.Length} y la longitud que permite es de {campo.Longitud}";

                }

            }
            catch (Exception ex)
            {
                sError =  $"Error del sistema (Modulo 3) : " + ex.Message.ToString();
            }
            return sError;
        }
        private string _GetValidarCampo(camposTabla campo, string sValor)
        {
            string sError = "";
            string TipoCampoName = campo.Etiqueta;
            try
            {

                Regex r = new Regex("^[a-zA-Z0-9\x20]*$");
                if (sValor.Length <= campo.Longitud ||
                    (campo.TipoCampo == TipoCampo.Fecha
                    || campo.TipoCampo == TipoCampo.Catalogo
                    || campo.TipoCampo == TipoCampo.ArchivoAdjunto
                    || campo.TipoCampo == TipoCampo.CasillaVerificacion
                    || campo.TipoCampo == TipoCampo.Hora))
                {
                    if (campo.Requerido)
                    {
                        if (campo.TipoCampo != TipoCampo.ArchivoAdjunto && campo.TipoCampo != TipoCampo.Catalogo)
                        {
                            if (sValor.Length == 0)
                            {
                                return $@"La columna { TipoCampoName }  es requerido y no cuenta con un valor.";
                            }
                        }
                        else if (campo.TipoCampo == TipoCampo.Catalogo)
                        {
                            if (sValor == "0" || sValor == "" || sValor == null)
                            {
                                return $@"La columna { TipoCampoName }  es requerido y no cuenta con un valor.";
                            }

                        }
                    }

                    if (sValor.Length > 0)
                    {
                        switch (campo.TipoCampo)
                        {
                            case TipoCampo.Alfanumerico:
                                //^[a-zA-Z0-9ñÑáÁéÉíÍóÓÓúÚüÜ“”,;=%$\n!¡¿?'""|().\/\/_@.#&+-‐	\\.\x20]*$
                                r = new Regex(@"^[a-zA-Z0-9ñÑáÁéÉíÍóÓÓúÚüÜ“”,;º’=%$\n!¡¿?'""|\*\(\).\/\/_@.#&+-‐\\.\x20]*$");
                                if (!r.IsMatch(sValor.Trim()))
                                {
                                    return $@"La columna  {  TipoCampoName } solo admite letras y numeros.";
                                }
                                break;
                            case TipoCampo.CasillaVerificacion:
                                if (sValor.ToLower() != "true" && sValor.ToLower() != "false")
                                {
                                    return $@"La columna  { TipoCampoName } es de tipo casilla de verificación y  solo admite las palabra: 'true' para verdadero y 'false' para falso.";
                                }
                                break;
                            case TipoCampo.Catalogo:

                                if (sValor != "0")
                                {
                                   if (!this._ExisteValorEnCatalogo(campo.iCatalogoId, campo.NombreTablaCatalogo, campo.NombreCampoCatalago, sValor.ToLower()))
                                            sError = $"No se pudo encontrar el valor del catalogo '{sValor}' este valor no existe en la base de datos.";
                                }

                                break;
                            case TipoCampo.Decimal:
                            case TipoCampo.Dinero:
                            case TipoCampo.Porcentaje:
                                if (campo.TipoCampo == TipoCampo.Porcentaje && campo._ConDecimales == false)
                                {
                                    long iiValue = 0;
                                    bool bIisInt = long.TryParse(sValor, out iiValue);
                                    sError = !bIisInt ? $@"La columna { TipoCampoName } es de tipo porcentaje y en el formato de excel no cumple con el formato requerido" : "";


                                }
                                else
                                {
                                    if (sValor.Contains("-"))
                                    {
                                        decimal dValue = 0;
                                        bool bIsNumber = decimal.TryParse(sValor, out dValue);
                                        sError = !bIsNumber ? $@"La columna { TipoCampoName } es de tipo {{ porcentaje,Decimal o Dinero }}  y en el formato de excel no cumple con el formato requerido" : "";
                                    }
                                    else
                                    {
                                        decimal dValue = 0;
                                        bool bIsNumber = decimal.TryParse(sValor, out dValue);
                                        sError = !bIsNumber ? $@"La columna { TipoCampoName } es de tipo {{ porcentaje,Decimal o Dinero }}  y en el formato de excel no cumple con el formato requerido" : "";
                                    }


                                }


                                break;
                            case TipoCampo.email:
                                string PatternEmail = @"^(([^<>()[\]\\.,;:\s@""]+(\.[^<>()[\]\\.,;:\s@""]+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";
                                r = new Regex(PatternEmail, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                                //r = new Regex("^[A-z0-9_.@]*$");
                                if (!r.IsMatch(sValor.Trim()))
                                {
                                    return $@"La columna { TipoCampoName } es de tipo email  y en el formato de excel no cumple con el formato requerido";
                                }
                                break;
                            case TipoCampo.Fecha:
                                try
                                {
                                    sValor = sValor.Replace(" 12:00:00 AM", "");
                                    sValor = sValor.Replace(" 12:00:00 a. m.", "");
                                    string[] validDateTimeFormats = { "dd/MM/yyyy", "d/m/yyyy", "d/m/yy", "dd/m/yyyy", "dd/m/yy", "d/mm/yyyy", "d/mm/yy" };

                                    DateTime dtValue = new DateTime();
                                    bool bIsDate = DateTime.TryParseExact(sValor, validDateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtValue);
                                    sError = !bIsDate ? $@"La columna { TipoCampoName } es de tipo fecha  y en el formato de excel no cumple con el formato requerido" : "";


                                    //sValor = sValor.Replace(" 12:00:00 AM", "").Replace(" 0:00:00", "");
                                    //string[] validDateTimeFormats = { "dd/MM/yyyy", "dd/MM/yy", "d/m/yyyy", "d/m/yy", "dd/m/yyyy", "dd/m/yy", "d/mm/yyyy", "d/mm/yy" };

                                    //DateTime dtValue = new DateTime();
                                    //bool bIsDate = DateTime.TryParseExact(sValor, validDateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtValue);
                                    //sError = !bIsDate ? $@"La columna { TipoCampoName } es de tipo fecha y en el formato de excel no cumple con el formato requerido" : "";

                                }
                                catch (Exception ex)
                                {
                                    sError = $@"La columna { TipoCampoName } es de tipo fecha  y en el formato de excel no cumple con el formato requerido";
                                }
                                break;
                            case TipoCampo.Hipervinculo:
                                string Pattern = @"(http|https):\/\/";
                                r = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                                if (!r.IsMatch(sValor.Trim()))
                                {
                                    return $@"La columna { TipoCampoName } es de tipo hipervínculo y en el formato de excel no cumple con el formato requerido";
                                }
                                break;
                            case TipoCampo.Hora:
                                r = new Regex(@"^(?:(?:0?[1-9]|1[0-2]):[0-5][0-9]\s?(?:[AP][Mm]?|[ap][m]?)?|(?:00?|1[3-9]|2[0-3]):[0-5][0-9])$");
                                if (!r.IsMatch(sValor.Trim()))
                                {
                                    try
                                    {
                                        DateTime vDateTime = new DateTime();
                                        bool itsDate = DateTime.TryParse(sValor, out vDateTime);
                                        if (itsDate)
                                        {
                                            string displayTime = vDateTime.ToString("hh:mm tt");
                                            if (r.IsMatch(displayTime))
                                            {
                                                sValor = displayTime;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                                break;
                            case TipoCampo.Numerico:
                                long iValue = 0;
                                bool bIsInt = long.TryParse(sValor, out iValue);
                                sError = !bIsInt ? $@"La columna { TipoCampoName } es de tipo numerico y en el formato de excel no cumple con el formato requerido" : "";
                                break;
                            case TipoCampo.Texto:
                                r = new Regex("^[a-zA-Z\x20]*$");
                                if (!r.IsMatch(sValor.Trim()))
                                {
                                    return $@"La columna { TipoCampoName } solo admite letras.";
                                }
                                break;
                            default:

                                break;
                        }
                    }
                }
                else
                {
                    return $@"La columna {TipoCampoName} cuenta con una longitud de {sValor.Length} y la longitud que permite es de {campo.Longitud}";
                }

            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }

            return sError;
        }
        public bool _ExisteValorEnCatalogo(int iCatalogoId, string NombreTabla, string NombreColumna, string sValor)
        {
            var bResult = false;
            try
            {
                if (!ListaCatalogos.Where(x => x.Key == iCatalogoId).Any())
                {
                    ApplicationDbContext db = new ApplicationDbContext();
                    string sQuery = $@"SELECT TablaFisicaId AS [Key], LOWER({NombreColumna}) AS [Value] FROM {NombreTabla} WHERE Activo=1";
                    var resultado = db.Database.SqlQuery<vmKeyValue>(sQuery).ToArray();
                    for (int i = 0; i < resultado.Length; i++)
                    {
                        ListaCatalogos.Add(new KeyValuePair<int, KeyValuePair<int, string>>(iCatalogoId, new KeyValuePair<int, string>(resultado[i].Key, resultado[i].Value)));
                    }
                }
                if (ListaCatalogos.Where(x => x.Key == iCatalogoId && x.Value.Value == sValor).Any())
                {
                    bResult = true;
                }
                else
                {
                    bResult = false;
                }
            }
            catch (Exception ex)
            {

            }
            return bResult;
        }
        public vmPlantillaCampos _GetCampos(int iPlantillaId = 0)
        {
            vmPlantillaCampos result = new vmPlantillaCampos();
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                var sQuery = $@"spr_portal_consulta_campos_excel @PlantillaId";
                SqlMapper.AddTypeHandler(new vmCampoForDatosHandler());
                SqlMapper.AddTypeHandler(new camposTablaHandler());


                using (IDbConnection idb = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    result = idb.Query<vmPlantillaCampos>(
                        sQuery,
                        new { PlantillaId = iPlantillaId })
                        .FirstOrDefault();
                    SqlMapper.ResetTypeHandlers();
                }
            }
            catch (Exception ex)
            {
                result.NombreTabla = ex.Message;
            }
            return result;
        }
        public vmErroresInsert _insert(vmPlantillaCampos mPlantillas, DataTable dtTabla,int OrganismoId,int PeriodoId,int sysFrecuencia, int sysNumFrecuencia,string usuarioId)
        {
            var culture = CultureInfo.GetCultureInfo("es-MX");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            vmErroresInsert respuesta = new vmErroresInsert();
            string UbicacionError = "";
            try
            {
                bool FormatoDiferente = false;
                if (
                       (dtTabla.Rows.Count >= 3 && dtTabla.Rows[3].ItemArray[0].ToString() == "ID")
                       ||
                        (dtTabla.Rows.Count >= 6 && dtTabla.Rows[5].ItemArray[0].ToString() == "")
                       )
                {
                    FormatoDiferente = true;
                }


                respuesta.sError = "Sin conexion a internet";
                List<vmINFORMATION_SCHEMA_COLUMNS> CamposTabla = this._GetCamposTabla(mPlantillas.NombreTabla);
                if (CamposTabla == null || (CamposTabla != null && CamposTabla.Count == 0))
                {
                    respuesta.sError = "Error del sistema (Modulo 6): no se puede accesar a la información del formato, contacte a un administrador del sistema.";
                }
                using (SqlConnection connection = new SqlConnection(conString.ConnectionString))
                {
                    var bulk = new BulkOperation(connection);
                    bulk.DestinationTableName = mPlantillas.NombreTabla;

                    //removemos las columnas excedenntes.
                    if (FormatoDiferente)
                    {
                        dtTabla.Columns.RemoveAt(0);
                    }
                    
                    while (dtTabla.Columns.Count > mPlantillas.campos.Count)
                    {
                        dtTabla.Columns.RemoveAt(dtTabla.Columns.Count - 1);
                    }


                    //añdimos las columnas faltantes
                    DataColumn newColumn = new DataColumn("OrganismoID", typeof(int));
                    newColumn.DefaultValue = OrganismoId;
                    dtTabla.Columns.Add(newColumn);
                    newColumn = new System.Data.DataColumn("PeriodoId", typeof(int));
                    newColumn.DefaultValue = PeriodoId;
                    dtTabla.Columns.Add(newColumn);
                    newColumn = new System.Data.DataColumn("sysFrecuencia", typeof(int));
                    newColumn.DefaultValue = sysFrecuencia;
                    dtTabla.Columns.Add(newColumn);
                    newColumn = new System.Data.DataColumn("sysNumFrecuencia", typeof(int));
                    newColumn.DefaultValue = sysNumFrecuencia;
                    dtTabla.Columns.Add(newColumn);
                    newColumn = new System.Data.DataColumn("FechaCreacion", typeof(DateTime));
                    newColumn.DefaultValue = DateTime.Now;
                    dtTabla.Columns.Add(newColumn);
                    newColumn = new System.Data.DataColumn("TablaFisicaId", typeof(int));
                    newColumn.DefaultValue = 0;
                    dtTabla.Columns.Add(newColumn);
                    newColumn = new System.Data.DataColumn("UsuarioId", typeof(string));
                    newColumn.DefaultValue = usuarioId;
                    dtTabla.Columns.Add(newColumn);



                    DataTable dtCloned = dtTabla.Select().Skip(6).CopyToDataTable();

                    //Mapping Table column  
                    var columnas = 0;

                    for (int i = 0; i < mPlantillas.campos.Count; i++)
                    {
                        UbicacionError = $" Columna (No. {i + 1} ) {mPlantillas.campos[i].Etiqueta } ";
                        //if (!mPlantillas.campos[i].Requerido)
                        //    dtCloned.Columns[i].DefaultValue = getDefaultValue(mPlantillas.campos[i].TipoCampo);
                        dtCloned = _ConvertColumnType(dtCloned, dtCloned.Columns[columnas].ColumnName, _getTypeForDt(mPlantillas.campos[i].TipoCampo, mPlantillas.campos[i]._ConDecimales), columnas, mPlantillas.campos[i].TipoCampo);
                        //bulk.ColumnMappings.Add(i, dtCloned.Columns[i].Ordinal);
                        bulk.ColumnMappings.Add(i, CamposTabla.Where(x => x.COLUMN_NAME.Trim().ToLower() == mPlantillas.campos[i].Nombre.Trim().ToLower()).FirstOrDefault().COLUMN_NAME);

                        columnas++;

                    }
                    //bulk.ColumnMappings.Add(columnas, "OrganismoID");
                    bulk.ColumnMappings.Add(columnas, CamposTabla.Where(x => x.COLUMN_NAME.Trim().ToLower() == "organismoid").FirstOrDefault().COLUMN_NAME);
                    columnas++;
                    //bulk.ColumnMappings.Add(columnas, "PeriodoId");
                    bulk.ColumnMappings.Add(columnas, CamposTabla.Where(x => x.COLUMN_NAME.Trim().ToLower() == "periodoid").FirstOrDefault().COLUMN_NAME);
                    columnas++;
                    //bulk.ColumnMappings.Add(columnas, "sysFrecuencia");
                    bulk.ColumnMappings.Add(columnas, CamposTabla.Where(x => x.COLUMN_NAME.Trim().ToLower() == "sysfrecuencia").FirstOrDefault().COLUMN_NAME);
                    columnas++;
                    //bulk.ColumnMappings.Add(columnas, "sysNumFrecuencia");
                    bulk.ColumnMappings.Add(columnas, CamposTabla.Where(x => x.COLUMN_NAME.Trim().ToLower() == "sysnumfrecuencia").FirstOrDefault().COLUMN_NAME);
                    columnas++;
                    //bulk.ColumnMappings.Add(columnas, "FechaCreacion");
                    bulk.ColumnMappings.Add(columnas, CamposTabla.Where(x => x.COLUMN_NAME.Trim().ToLower() == "fechacreacion").FirstOrDefault().COLUMN_NAME);
                    columnas++;
                    //bulk.ColumnMappings.Add("TablaFisicaId", ColumnMappingDirectionType.Output);
                    bulk.ColumnMappings.Add(CamposTabla.Where(x => x.COLUMN_NAME.Trim().ToLower() == "tablafisicaid").FirstOrDefault().COLUMN_NAME, ColumnMappingDirectionType.Output);
                    columnas++;
                    //bulk.ColumnMappings.Add(columnas, "UsuarioId");
                    bulk.ColumnMappings.Add(columnas, CamposTabla.Where(x => x.COLUMN_NAME.Trim().ToLower() == "usuarioid").FirstOrDefault().ORDINAL_POSITION - 1);

                    //ejecutamos la accion
                    connection.Open();
                    //objbulk.WriteToServer(dtCloned);
                    bulk.BulkInsert(dtCloned);
                    connection.Close();
                    connection.Dispose();
                    respuesta.dtTable = dtCloned;
                    respuesta.sError = "";
                }
            }
            catch (Exception ex)
            {
                respuesta.sError = "Error del sistema (Modulo 7): " + UbicacionError + " ," + ex.Message;
                if (ex.Message == "El origen no contiene DataRows.")
                {
                    respuesta.sError = "Error del sistema (Modulo 7): El archivo no contiene datos.";
                }
            }
            return respuesta;
        }
        public vmErroresInsert _insertTabla(List<camposTabla> vmCampo, DataTable dtTabla, string NombreTablaCatalogo,int OrganismoId,string usuarioId)
        {
            vmErroresInsert respuesta = new vmErroresInsert();
            try
            {
                 //bool FormatoDiferente = false;
                
                //if (
                //       (dtTabla.Rows.Count >= 3 && dtTabla.Rows[3].ItemArray[0].ToString() == "ID")
                //       ||
                //        (dtTabla.Rows.Count >= 5 &&  dtTabla.Rows[5].ItemArray[0].ToString() == "")
                //       )
                //{
                //    FormatoDiferente = true;
                //}

                respuesta.sError = "Sin conexion a internet";
                List<vmINFORMATION_SCHEMA_COLUMNS> CamposTabla = this._GetCamposTabla(NombreTablaCatalogo);
                if (CamposTabla == null || (CamposTabla != null && CamposTabla.Count == 0))
                {
                    respuesta.sError = "Error del sistema (Modulo 8): no se puede accesar a la información del formato de la tabla, contacte a un administrador del sistema.";
                }
                using (SqlConnection connection = new SqlConnection(conString.ConnectionString))
                {
                    var bulk = new BulkOperation(connection);
                    bulk.DestinationTableName = NombreTablaCatalogo;
                    //ponemos los campos
                    DataColumn newColumn = new DataColumn("OrganismoID", typeof(int));
                    newColumn.DefaultValue =OrganismoId;
                    dtTabla.Columns.Add(newColumn);
                    newColumn = new DataColumn("FechaCreacion", typeof(DateTime));
                    newColumn.DefaultValue = DateTime.Now;
                    dtTabla.Columns.Add(newColumn);
                    newColumn = new DataColumn("UsuarioId", typeof(string));
                    newColumn.DefaultValue = usuarioId;
                    dtTabla.Columns.Add(newColumn);

                    //Mapping Table column
                    var columnas = 0;//FormatoDiferente ? 1 : 0;

                    //foreach (var item in vmCampo)
                    //{
                    //    dtTabla = ConvertColumnType(dtTabla, dtTabla.Columns[columnas].ColumnName, getTypeForDt(item.TipoCampo, item._ConDecimales));
                    //    bulk.ColumnMappings.Add(columnas,item.Nombre);
                    //    columnas++;
                    //}
                    for (int i = 0; i < vmCampo.Count; i++)
                    {
                        //var requerdido = vmCampo[i].Requerido;
                        //if (!requerdido)
                        //    dtTabla.Columns[i].AllowDBNull = true;
                        dtTabla = _ConvertColumnType(dtTabla, dtTabla.Columns[columnas].ColumnName, _getTypeForDt(vmCampo[i].TipoCampo, vmCampo[i]._ConDecimales), columnas, vmCampo[i].TipoCampo, vmCampo[i].iCatalogoId,true);
                        //bulk.ColumnMappings.Add(i, vmCampo[i].Nombre);
                        bulk.ColumnMappings.Add(i, CamposTabla.Where(x => x.COLUMN_NAME.Trim().ToLower() == vmCampo[i].Nombre.Trim().ToLower()).FirstOrDefault().COLUMN_NAME);
                        columnas++;

                    }
                    dtTabla = _ConvertColumnType(dtTabla, dtTabla.Columns[columnas].ColumnName, typeof(int), columnas, TipoCampo.Numerico);
                    //bulk.ColumnMappings.Add(columnas, "Idregistro");
                    bulk.ColumnMappings.Add(columnas, CamposTabla.Where(x => x.COLUMN_NAME.Trim().ToLower() == "idregistro").FirstOrDefault().COLUMN_NAME);
                    columnas++;
                    //bulk.ColumnMappings.Add(columnas, "OrganismoID");
                    bulk.ColumnMappings.Add(columnas, CamposTabla.Where(x => x.COLUMN_NAME.Trim().ToLower() == "organismoid").FirstOrDefault().COLUMN_NAME);
                    columnas++;
                    //bulk.ColumnMappings.Add(columnas, "FechaCreacion");
                    bulk.ColumnMappings.Add(columnas, CamposTabla.Where(x => x.COLUMN_NAME.Trim().ToLower() == "fechacreacion").FirstOrDefault().COLUMN_NAME);
                    columnas++;
                    //bulk.ColumnMappings.Add(columnas, "UsuarioId");
                    bulk.ColumnMappings.Add(columnas, CamposTabla.Where(x => x.COLUMN_NAME.Trim().ToLower() == "usuarioid").FirstOrDefault().COLUMN_NAME);
                    columnas++;

                    //ejecutamos la accion
                    connection.Open();
                    //objbulk.WriteToServer(dtCloned);
                    bulk.BulkInsert(dtTabla);
                    connection.Close();
                    connection.Dispose();
                    respuesta.dtTable = dtTabla;
                    respuesta.sError = "";
                }


            }
            catch (Exception ex)
            {
                respuesta.sError = "Error del sistema (Modulo 1): " + ex.Message;
            }
            return respuesta;
        }
        public vmDsTablas GetDSDeTabla(DataTable dtPrincipal, DataSet dtTablas, List<vmCampoForDatos> vmCampos)
        {
            DataSet DsTablas = new DataSet();
            vmDsTablas respuesta = new vmDsTablas();
            try
            {
                respuesta.sError = "Sin conexion a internet";
                for (int i = 0; i < dtPrincipal.Rows.Count; i++)
                {
                    var iCamposCount = vmCampos.Count;
                    int iCatalogoCount = 0;
                    for (int x = 0; x < iCamposCount; x++)
                    {
                        if (vmCampos[x].TipoCampo == TipoCampo.Catalogo)
                        {
                            iCatalogoCount++;
                            if (vmCampos[x].TablaCatalogo)
                            {
                                
                                var nombreColumna = dtTablas.Tables[iCatalogoCount].Columns[0].ColumnName;
                                var sheetExcelCatalogo = dtTablas.Tables[iCatalogoCount].Select($"[{nombreColumna}] = {dtPrincipal.Rows[i].ItemArray[x] }");
                                var CatalgosInTabla = vmCampos[x].camposTabla.Where(c => c.TipoCampo == TipoCampo.Catalogo && c.Activo).ToList().Count;
                                iCatalogoCount += CatalgosInTabla > 0 ? CatalgosInTabla : 0;
                                if (sheetExcelCatalogo.Length > 0)
                                {
                                    var iIdRegistro = dtPrincipal.Rows[i].ItemArray[iCamposCount + 5]; ;
                                    var NombreTabla = vmCampos[x].NombreTablaCatalogo;

                                    //lo agregamos al dt
                                    //DsTablas.Tables[NombreTabla].Columns.Add(newColumn);
                                    //Verificamos si existe la tabla
                                    if (!DsTablas.Tables.Contains(NombreTabla))
                                    {
                                        DataTable dtProvicional = sheetExcelCatalogo.CopyToDataTable();

                                        while (dtProvicional.Columns.Count > vmCampos[x].camposTabla.Count)
                                        {
                                            dtProvicional.Columns.RemoveAt(dtProvicional.Columns.Count - 1);
                                        }

                                        dtProvicional.TableName = NombreTabla;
                                        DataColumn newColumn = new DataColumn("Idregistro", typeof(int));
                                        newColumn.DefaultValue = iIdRegistro;
                                        dtProvicional.Columns.Add(newColumn);
                                        //agregamos la tabla con sus registros
                                        DsTablas.Tables.Add(dtProvicional);

                                    }
                                    else
                                    {
                                        int totalRows = DsTablas.Tables[NombreTabla].Rows.Count;
                                        for (int irow = 0; irow < sheetExcelCatalogo.Length; irow++)
                                        {
                                            //sheetExcelCatalogo[1].ItemArray.
                                            //    .SetField("Idregistro", respuesta.dtTable.Rows[i].ItemArray[iCamposCount + 5]);
                                            DsTablas.Tables[NombreTabla].ImportRow(sheetExcelCatalogo[irow]);
                                            DsTablas.Tables[NombreTabla].Rows[totalRows].SetField("Idregistro", iIdRegistro);
                                        }



                                    }


                                }
                            }

                        }
                    }
                }


                respuesta.dsTable = DsTablas;
                respuesta.sError = "";

            }
            catch (Exception ex)
            {
                respuesta.sError = "Error del Sistema (Modulo 4) : " + ex.Message;
            }
            return respuesta;
        }
        public DataTable _ConvertColumnType(DataTable dt, string columnName, Type newType, int iColumn, TipoCampo tipocampo,int CatalogoId = 0,bool Tabla=false)
        {
            using (DataColumn dc = new DataColumn(columnName + "_new", newType))
            {
                // Add the new column which has the new type, and move it to the ordinal of the old column
                int ordinal = dt.Columns[columnName].Ordinal;
                dt.Columns.Add(dc);
                dc.SetOrdinal(ordinal);

                // Get and convert the values of the old column, and insert them into the new
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr.ItemArray[iColumn + 1].ToString() == "")
                    {
                       // var type = dr[columnName].GetType();
                        dr.SetField(columnName, _getDefaultValue(tipocampo));
                    }
                    else if (tipocampo == TipoCampo.Catalogo && Tabla)
                    {
                        var dato = ListaCatalogos.Where(cat => cat.Key == CatalogoId && cat.Value.Value.ToLower() == dr.ItemArray[iColumn + 1].ToString().ToLower()).FirstOrDefault();
                        dr.SetField(columnName, dato.Value.Key.ToString());
                    }
                   
                    dr[dc.ColumnName] = Convert.ChangeType(dr[columnName], newType);
                }


                // Remove the old column
                dt.Columns.Remove(columnName);

                // Give the new column the old column's name
                dc.ColumnName = columnName;
            }
            return dt;
        }
        public object _getDefaultValue(TipoCampo tipocampo, Type newType = null)
        {
            switch (tipocampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.AreaTexto:
                case TipoCampo.Alfanumerico:
                case TipoCampo.Hora:
                case TipoCampo.Hipervinculo:
                case TipoCampo.email:
                case TipoCampo.Telefono:
                case TipoCampo.ArchivoAdjunto:
                    if(newType != null && 
                        (newType.Name == "Double" || newType.Name == "Int16" || newType.Name == "Int32" || newType.Name == "Int64")
                        )
                    {
                        return "0";
                    }
                    return "";
                    break;
                case TipoCampo.Numerico:
                    return "0";
                    break;
                case TipoCampo.Porcentaje:
                    return "0";
                    break;
                case TipoCampo.Dinero:
                case TipoCampo.Decimal:
                    return "0";
                    break;
                case TipoCampo.Fecha:
                    return new DateTime(1900, 01, 01).ToString();
                    break;
                case TipoCampo.Catalogo:
                case TipoCampo.CasillaVerificacion:
                    return "0";
                    break;
                default:
                    return "";
                    break;
            }
        }
        public Type _getTypeForDt(TipoCampo tipocampo, bool? conDecimales)
        {
            switch (tipocampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.AreaTexto:

                case TipoCampo.Alfanumerico:
                case TipoCampo.Hora:
                case TipoCampo.Hipervinculo:
                case TipoCampo.email:
                case TipoCampo.Telefono:
                case TipoCampo.ArchivoAdjunto:
                    return typeof(string);
                    break;
                case TipoCampo.Numerico:
                    //if(bRequerido)
                    //{
                    return typeof(int);
                    //}
                    //else
                    //{
                    //    return typeof(int);
                    //}

                    break;
                case TipoCampo.Porcentaje:
                    if (conDecimales == true)
                        return typeof(double);
                    else
                        return typeof(int);
                    break;
                case TipoCampo.Dinero:
                case TipoCampo.Decimal:
                    return typeof(double);
                    break;
                case TipoCampo.Fecha:
                    return typeof(DateTime);
                    break;
                case TipoCampo.Catalogo:
                case TipoCampo.CasillaVerificacion:
                    return typeof(int);
                    break;
                default:
                    return typeof(string);
                    break;
            }
        }
        public List<vmINFORMATION_SCHEMA_COLUMNS> _GetCamposTabla(string snombreTabla)
        {
            List<vmINFORMATION_SCHEMA_COLUMNS> result = new List<vmINFORMATION_SCHEMA_COLUMNS>();
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                var sQuery = $@"spr_system_get_campos_tabla @nombreTabla";


                using (IDbConnection idb = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    result = idb.Query<vmINFORMATION_SCHEMA_COLUMNS>(
                        sQuery,
                        new { nombreTabla = snombreTabla })
                        .ToList();
                    SqlMapper.ResetTypeHandlers();
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }
        #endregion
        public ActionResult ImportarExcelBeta(int iId = 0)
        {
            var model = db.Plantillas.Where(x => x.PlantillaId == iId).FirstOrDefault();
            if (model == null)
                return View("~/Views/Errors/PageNotFound.cshtml");
            this.VerifyNecessaryColumn((int)iId);
            SetFrecuencia((int)iId);
            ViewBag.NombrePlantilla = TituloFromato(HMTLHelperExtensions.GetLeyArtiucloFraccionFromPlantilla(iId));
            return View("ImportarExcelBeta", model);
        }
        public ActionResult ImportarExcel(int iId = 0)
        {
            var model = db.Plantillas.Where(x => x.PlantillaId == iId).FirstOrDefault();
            if (model == null)
                return View("~/Views/Errors/PageNotFound.cshtml");
            this.VerifyNecessaryColumn((int)iId);
            SetFrecuencia((int)iId);
            ViewBag.NombrePlantilla = TituloFromato(HMTLHelperExtensions.GetLeyArtiucloFraccionFromPlantilla(iId));
            return View("ImportarExcel",model);
        }
       
        //Generar2
        [HttpPost]
        public ActionResult ReadExcel(HttpPostedFileBase ExcelFile, int iId = 0)
        {

            try
            {
                //return Json(new { Hecho = true, Mensaje = $"Error: El documento no tiene registros, por favor seleccione un archivo con datos." }, JsonRequestBehavior.AllowGet);
                var TieneRegistros = false;

                vmPlantillaCampos campos = HMTLHelperExtensions.GetCamposForExcel(iId);
                //var model = db.Plantillas.Where(x => x.PlantillaId == iId).FirstOrDefault();
                if (campos == null)
                    return Json(new { Hecho = false, Mensaje = $"No se encontro la plantilla " }, JsonRequestBehavior.AllowGet);
                var CamposConErrores = new List<AuxErroresEcxel>();
                List<string> ColumnsNames = new List<string>();
                var vRows = new List<string>();
                if (ExcelFile != null)
                {
                    //if(!ExcelFile.ContentType.Contains("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
                    //{
                    //    throw new ArgumentException("Excel file not valid", "error"); 
                    //}
                    var extencion = ExcelFile.ContentType;
                    var splitArchivo = ExcelFile.FileName.Split('.');
                    string directorio_sub = $@"Archivos\Plantillas\archivosSuplentes\";
                    string Directorio = $@"{Server.MapPath("~/")}{directorio_sub}";
                    var workbook = new XLWorkbook();

                    if (extencion == "application/vnd.ms-excel")
                    {
                        bool exists = System.IO.Directory.Exists(Directorio);
                        if (!exists)
                            System.IO.Directory.CreateDirectory(Directorio);
                        var Input = Directorio + "Input.xls";
                        var Output = Directorio + "Output.xlsx";

                        ExcelFile.SaveAs(Input);
                        Workbook wb = new Workbook();
                        wb.LoadFromFile(Input);
                        wb.SaveToFile(Output, ExcelVersion.Version2013);
                        workbook = new XLWorkbook(Output);
                    }
                    else
                    {
                        workbook = new XLWorkbook(ExcelFile.InputStream);
                    }


                    var ws1 = workbook.Worksheet(1);

                    //foreach (var item in rows)
                    //{
                    //    string sValue1 =  .Cell(1).HasFormula ? item.Cell(1).CachedValue.ToString() : rows.Cell(1).GetValue<string>();
                    //}


                    //var vFirstRow = ws1.RowsUsed().FirstOrDefault();

                    //aqui mero
                    //var campos = db.Campos.Where(x => x.PlantillaId == model.PlantillaId && x.Activo).OrderBy(x => x.Orden).ToList();

                    //var initRow = 9;

                    //var rows = worksheet.RangeUsed().RowsUsed().Skip(1);
                    var rowuserd = ws1.RowsUsed();
                    var OtherFormatExcel = ws1.Range(5, 1, 5, 1).Search("ID").Count() > 0 || ws1.Range(7, 1, 7, 1).IsEmpty() ? true : false;

                    foreach (var row in ws1.RowsUsed())
                    {
                        if (row.RowNumber() >= 8)
                        {
                            TieneRegistros = true;


                            List<string> lstErrorsRow = new List<string>();

                            var temporalLstCampo = new List<Campo>();
                            bool bError = false;
                            int iCountCampo = OtherFormatExcel ? 2 : 1;
                            var iCountCatalogo = 1;
                            string sCamposValues = "";
                            string errorTxt = "";

                            foreach (var item in campos.campos)
                            {

                                errorTxt = $@"Es de tipo { item.TipoCampo.GetDisplayName() }  y en el formato de excel no cumple con el formato requerido";
                                var tBerror = false;
                                var temporalCampo = new Campo();

                                string sValue = row.Cell(iCountCampo).HasFormula ? row.Cell(iCountCampo).CachedValue.ToString() : row.Cell(iCountCampo).GetValue<string>();
                                //if(item.TipoCampo == TipoCampo.Telefono)
                                //{

                                //}

                                //if(item.TipoCampo == TipoCampo.Catalogo)
                                //{
                                //    iCountCatalogo++;
                                //}

                                Regex r = new Regex("^[a-zA-Z0-9\x20]*$");
                                if (sValue.Length <= item.Longitud ||
                                    (item.TipoCampo == TipoCampo.Fecha
                                    || item.TipoCampo == TipoCampo.Catalogo
                                    || item.TipoCampo == TipoCampo.ArchivoAdjunto
                                    || item.TipoCampo == TipoCampo.CasillaVerificacion
                                    || item.TipoCampo == TipoCampo.Hora))
                                {
                                    if (item.Requerido)
                                    {
                                        if (item.TipoCampo != TipoCampo.ArchivoAdjunto && item.TipoCampo != TipoCampo.Catalogo)
                                        {
                                            if (sValue.Length == 0)
                                            {
                                                errorTxt = $@"El tipo de campo { item.TipoCampo.GetDisplayName() }  es requerido y no cuenta con un valor.";
                                                tBerror = true;
                                            }
                                            else
                                            {
                                                tBerror = false;
                                            }
                                        }
                                        else if (item.TipoCampo == TipoCampo.Catalogo)
                                        {
                                            if (sValue == "0" || sValue == "" || sValue == null)
                                            {
                                                errorTxt = $@"El tipo de campo { item.TipoCampo.GetDisplayName() }  es requerido y no cuenta con un valor.";
                                                tBerror = true;
                                            }
                                            else
                                            {
                                                tBerror = false;
                                            }

                                        }
                                    }
                                    if(item.TipoCampo == TipoCampo.Catalogo)
                                        iCountCatalogo++;

                                    if (sValue.Length > 0 && !tBerror)
                                    {
                                        switch (item.TipoCampo)
                                        {
                                            case TipoCampo.Alfanumerico:
                                                //^[a-zA-Z0-9ñÑáÁéÉíÍóÓÓúÚüÜ“”,;=%$\n!¡¿?'""|().\/\/_@.#&+-‐	\\.\x20]*$
                                                r = new Regex(@"^[a-zA-Z0-9ñÑáÁéÉíÍóÓÓúÚüÜ“”,;º’=%$\n!¡¿?'""|().\/\/_@.#&+-‐\\.\x20]*$");
                                                if (!r.IsMatch(sValue.Trim()))
                                                {
                                                    errorTxt = $@"El campo  { item.TipoCampo.GetDisplayName() } solo admite letras y numeros.";
                                                    tBerror = true;
                                                }
                                                break;
                                            case TipoCampo.CasillaVerificacion:
                                                if (sValue.ToLower() != "true" && sValue.ToLower() != "false")
                                                {
                                                    errorTxt = $@"El campo  { item.TipoCampo.GetDisplayName() } solo admite las palabra: 'true' para verdadero y 'false' para falso.";
                                                    tBerror = true;
                                                }
                                                break;
                                            case TipoCampo.Catalogo:
                                               
                                                if (sValue != "0")
                                                {
                                                    if (item.TablaCatalogo)//(HMTLHelperExtensions.GetTipoTabla(item.CatalogoId))
                                                    {
                                                        //iCountCatalogo++;
                                                        //traemos los tab del catalogo 
                                                        //traemos los campos del catalogo
                                                        var camposCatalogo = db.CampoCatalogo.Where(x => x.CatalogoId == item.CatalogoId && x.Activo).OrderBy(x => x.Orden).ToList();

                                                        var ws2 = workbook.Worksheet(iCountCatalogo);
                                                        //var rows = ws2.Range(4, 1, ws2.RowsUsed().Count(), 1).Search(sValue).AsList();
                                                        var rows = ws2.Range(4, 1, ws1.RowsUsed().Count(), 1).CellsUsed(cell => cell.GetString() == sValue).AsList();
                                                        var ColumnTwoEmpty = ws2.Range(3, 2, 3, 2).IsEmpty() ? true : false;
                                                        if (rows.Count > 0)
                                                        {
                                                            foreach (var tabla_item in rows)
                                                            {
                                                                var nRow = ws2.Rows(tabla_item.Address.RowNumber.ToString()).FirstOrDefault();
                                                                //verificamoos por cada uno de los valores
                                                                //asdsad

                                                                var respuesta = this.validarRowOfExcel(nRow, camposCatalogo, ColumnTwoEmpty);
                                                                if (respuesta.Result)
                                                                {
                                                                    errorTxt = respuesta.Valor;
                                                                    tBerror = respuesta.Result;
                                                                    break;
                                                                }
                                                                else if (respuesta.HandleError)
                                                                {
                                                                    return Json(new { Hecho = false, Mensaje = $"Error: '{respuesta.Valor}' " }, JsonRequestBehavior.AllowGet);
                                                                }
                                                            }
                                                        }
                                                        //iCountCatalogo += camposCatalogo.Where(x => x.TipoCampo == TipoCampo.Catalogo).Count();
                                                    }
                                                    else
                                                    {
                                                        //iCountCatalogo++;
                                                        var EncontroCat = HMTLHelperExtensions.CehckifCatalogoExist(item, sValue);
                                                        if (!EncontroCat)
                                                        {
                                                            errorTxt = $@"No se pudo encontrar el valor del catalogo '{sValue}' este valor no existe en la base de datos.";
                                                            tBerror = true;
                                                        }
                                                    }
                                                }

                                                break;
                                            case TipoCampo.Decimal:
                                            case TipoCampo.Dinero:
                                            case TipoCampo.Porcentaje:
                                                if (item.TipoCampo == TipoCampo.Porcentaje && item._ConDecimales == false)
                                                {
                                                    long iiValue = 0;
                                                    bool bIisInt = long.TryParse(sValue, out iiValue);
                                                    tBerror = !bIisInt ? true : tBerror;
                                                }
                                                else
                                                {
                                                    if (sValue.Contains("-"))
                                                    {
                                                        decimal dValue = 0;
                                                        bool bIsNumber = decimal.TryParse(sValue, out dValue);
                                                        tBerror = !bIsNumber ? true : tBerror;
                                                    }
                                                    else
                                                    {
                                                        decimal dValue = 0;
                                                        bool bIsNumber = decimal.TryParse(sValue, out dValue);
                                                        tBerror = !bIsNumber ? true : tBerror;
                                                    }


                                                }


                                                break;
                                            case TipoCampo.email:
                                                string PatternEmail = @"^(([^<>()[\]\\.,;:\s@""]+(\.[^<>()[\]\\.,;:\s@""]+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";
                                                r = new Regex(PatternEmail, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                                                //r = new Regex("^[A-z0-9_.@]*$");
                                                if (!r.IsMatch(sValue.Trim()))
                                                {
                                                    tBerror = true;
                                                }
                                                break;
                                            case TipoCampo.Fecha:
                                                try
                                                {
                                                    sValue = sValue.Replace(" 12:00:00 AM", "");
                                                    //string[] validDateTimeFormats = { "MM/dd/yyyy", "dd/MM/yyyy", "MM/dd/yy", "dd/MM/yy", "MM/d/yy", "d/MM/yy", "M/dd/yy", "dd/M/yy", "MM/d/yyyy", "d/MM/yyyy", "M/dd/yyyy", "dd/M/yyyy", "d/M/yyyy", "M/d/yyyy", "d/M/yy", "M/d/yy" };
                                                    string[] validDateTimeFormats = { "dd/MM/yyyy", "d/m/yyyy", "d/m/yy", "dd/m/yyyy", "dd/m/yy", "d/mm/yyyy", "d/mm/yy" };

                                                    DateTime dtValue = new DateTime();
                                                    bool bIsDate = DateTime.TryParseExact(sValue, validDateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtValue);
                                                    tBerror = !bIsDate ? true : tBerror;

                                                }
                                                catch (Exception ex)
                                                {
                                                    tBerror = true;
                                                }
                                                //DateTime dtValue = new DateTime();
                                                //bool bIsDate = DateTime.TryParse(sValue, out dtValue);
                                                //tBerror = !bIsDate ? true : tBerror;
                                                break;
                                            case TipoCampo.Hipervinculo:
                                                //string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
                                                //string Pattern = @"https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,}";
                                                string Pattern = @"(http|https):\/\/";
                                                r = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                                                if (!r.IsMatch(sValue.Trim()))
                                                {
                                                    tBerror = true;
                                                }
                                                break;
                                            case TipoCampo.Hora:
                                                r = new Regex(@"^(?:(?:0?[1-9]|1[0-2]):[0-5][0-9]\s?(?:[AP][Mm]?|[ap][m]?)?|(?:00?|1[3-9]|2[0-3]):[0-5][0-9])$");
                                                if (!r.IsMatch(sValue.Trim()))
                                                {
                                                    tBerror = true;
                                                }
                                                if (tBerror)
                                                {
                                                    try
                                                    {
                                                        DateTime vDateTime = new DateTime();
                                                        bool itsDate = DateTime.TryParse(sValue, out vDateTime);
                                                        if (itsDate)
                                                        {
                                                            string displayTime = vDateTime.ToString("hh:mm tt");
                                                            if (r.IsMatch(displayTime))
                                                            {
                                                                sValue = displayTime;
                                                                tBerror = false;
                                                            }

                                                        }


                                                    }
                                                    catch (Exception ex)
                                                    {
                                                    }

                                                }
                                                break;
                                            case TipoCampo.Numerico:
                                                long iValue = 0;
                                                bool bIsInt = long.TryParse(sValue, out iValue);
                                                tBerror = !bIsInt ? true : tBerror;

                                                break;
                                            case TipoCampo.Telefono:

                                                break;
                                            case TipoCampo.Texto:
                                                r = new Regex("^[a-zA-Z\x20]*$");
                                                if (!r.IsMatch(sValue.Trim()))
                                                {
                                                    errorTxt = $@"El campo { item.TipoCampo.GetDisplayName() } solo admite letras.";
                                                    tBerror = true;
                                                }
                                                break;
                                            default:

                                                break;

                                        }
                                    }

                                }
                                else
                                {
                                    errorTxt = $@"El valor cuenta con una longitud de {sValue.Length} y la longitud que permite es de {item.Longitud}";
                                    tBerror = true;

                                }


                                sCamposValues += iCountCampo == 1 ? "" : "|||";
                                sCamposValues += $"{sValue}";

                                //prueba
                                temporalCampo.TipoCampo = item.TipoCampo;
                                temporalCampo.Etiqueta = item.Etiqueta;
                                temporalCampo.ExcelValor = sValue;
                                temporalCampo.ExcelValidacion = tBerror;
                                temporalCampo.ExcelErrorTxt = errorTxt;
                                temporalLstCampo.Add(temporalCampo);

                                if (tBerror)
                                {
                                    bError = tBerror;
                                    //if (ColumnsNames.Where(x => x == item.Etiqueta).Count() <= 0)
                                    //{
                                    //    ColumnsNames.Add(item.Etiqueta);
                                    //}
                                }

                                iCountCampo++;
                            }


                            if (bError)
                            {
                                var mAuxErroresEcxel = new AuxErroresEcxel();
                                mAuxErroresEcxel.Campo = temporalLstCampo;
                                mAuxErroresEcxel.NoRenglon = row.RowNumber();
                                CamposConErrores.Add(mAuxErroresEcxel);
                                break;
                                //vRows.Add(sCamposValues);
                            }


                            //}
                        }

                    }
                }
                if (TieneRegistros)
                {
                    //ViewBag.ColumnsNames = ColumnsNames;
                    string viewContent = ConvertViewToString("_RowsExcelImport", CamposConErrores);
                    //return PartialView("_RowsExcelImport", CamposConErrores);
                    var conErrores = CamposConErrores.Count > 0 ? true : false;
                    var json = Json(new { Hecho = true, ConErrores = conErrores, Partial = viewContent }, JsonRequestBehavior.AllowGet);
                    //configuramos mas length para que no nos den error de longitud
                    json.MaxJsonLength = 500000000;
                    return json;
                }
                else
                {
                    return Json(new { Hecho = false, Mensaje = $"Error: El documento no tiene registros, por favor seleccione un archivo con datos." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ColumnsNames = new List<string>();
                return Json(new { Hecho = false, Mensaje = $"Error: '{ex.Message}' " }, JsonRequestBehavior.AllowGet);
            }



        }

        public ResultGuardarTablaFisica validarRowOfExcel(IXLRow row, List<CampoCatalogo> campos, bool columnTwoEmpty = false)
        {
            ResultGuardarTablaFisica respuesta = new ResultGuardarTablaFisica();
            try
            {
                string sTabla = $"Problema en la Linea {row.RowNumber()} dentro de la sub Tabla, ";
                //var OtherFormatExcel = row. 2, 3, 2).IsEmpty() ? true : false;
                int iColumnNumber = 1;
                foreach (var item in campos)
                {
                    if (iColumnNumber == 2 && columnTwoEmpty)
                        iColumnNumber++;


                    respuesta.Result = false;
                    string sValue = "";
                    sValue = row.Cell(iColumnNumber).HasFormula ? row.Cell(iColumnNumber).CachedValue.ToString() : row.Cell(iColumnNumber).GetValue<string>();
                    respuesta.Valor = $@"{sTabla} Es de tipo { item.TipoCampo.GetDisplayName() }  y en el formato de excel no cumple con el formato requerido, Valor Actual: '{sValue}'";
                    //if(item.TipoCampo == TipoCampo.Telefono)
                    //{

                    //}
                    Regex r = new Regex("^[a-zA-Z0-9\x20]*$");
                    if (sValue.Length <= item.Longitud ||
                        (item.TipoCampo == TipoCampo.Fecha
                        || item.TipoCampo == TipoCampo.Catalogo
                        || item.TipoCampo == TipoCampo.ArchivoAdjunto
                        || item.TipoCampo == TipoCampo.CasillaVerificacion
                        || item.TipoCampo == TipoCampo.Hora))
                    {
                        if (item.Requerido)
                        {
                            if (item.TipoCampo != TipoCampo.ArchivoAdjunto && item.TipoCampo != TipoCampo.Catalogo)
                            {
                                if (sValue.Length == 0)
                                {
                                    respuesta.Valor = $@"{sTabla} El tipo de campo { item.TipoCampo.GetDisplayName() }  es requerido y no cuenta con un valor.";
                                    respuesta.Result = true;
                                }
                                else
                                {
                                    respuesta.Result = false;
                                }
                            }
                            else if (item.TipoCampo == TipoCampo.Catalogo)
                            {
                                if (sValue == "0" || sValue == "" || sValue == null)
                                {
                                    respuesta.Valor = $@"{sTabla} El tipo de campo { item.TipoCampo.GetDisplayName() }  es requerido y no cuenta con un valor.";
                                    respuesta.Result = true;
                                }
                                else
                                {
                                    respuesta.Result = false;
                                }

                            }
                        }

                        if (sValue.Length > 0 && !respuesta.Result)
                        {
                            switch (item.TipoCampo)
                            {
                                case TipoCampo.Alfanumerico:
                                    //^[a-zA-Z0-9ñÑáÁéÉíÍóÓúÚ\/_@.#&+-\\.\x20]*$
                                    r = new Regex(@"^[a-zA-Z0-9ñÑáÁéÉíÍóÓÓúÚüÜ“”,;º’=%$\n!¡¿?'""|().\/\/_@.#&+-‐\\.\x20]*$");
                                    if (!r.IsMatch(sValue))
                                    {
                                        respuesta.Valor = $@"{sTabla} El campo  { item.TipoCampo.GetDisplayName() } solo admite letras y numeros.";
                                        respuesta.Result = true;
                                    }
                                    break;
                                case TipoCampo.CasillaVerificacion:
                                    if (sValue.ToLower() != "true" && sValue.ToLower() != "false")
                                    {
                                        respuesta.Valor = $@"{sTabla} El campo  { item.TipoCampo.GetDisplayName() } solo admite las palabra: 'true' para verdadero y 'false' para falso.";
                                        respuesta.Result = true;
                                    }
                                    break;
                                case TipoCampo.Catalogo:
                                    var EncontroCat = HMTLHelperExtensions.CehckifCatalogoExist(item, sValue);
                                    if (!EncontroCat)
                                    {
                                        respuesta.Valor = $@"{sTabla} No se pudo encontrar el valor del catalogo '{sValue}' este valor no existe en la base de datos.";
                                        respuesta.Result = true;
                                    }
                                    //else if (HMTLHelperExtensions.GetTipoTabla(item.CatalogoId))
                                    //{
                                    //    //asdsad

                                    //}
                                    break;
                                case TipoCampo.Decimal:
                                case TipoCampo.Dinero:
                                case TipoCampo.Porcentaje:
                                    if (item.TipoCampo == TipoCampo.Porcentaje && item._ConDecimales == false)
                                    {
                                        long iiValue = 0;
                                        bool bIisInt = long.TryParse(sValue, out iiValue);
                                        respuesta.Result = !bIisInt ? true : respuesta.Result;
                                    }
                                    else
                                    {
                                        decimal dValue = 0;
                                        //bool bIsNumber = decimal.TryParse(sValue, out dValue);

                                        bool bIsNumber = decimal.TryParse(sValue, NumberStyles.Any, CultureInfo.InvariantCulture, out dValue);
                                        respuesta.Result = !bIsNumber ? true : respuesta.Result;

                                    }


                                    break;
                                case TipoCampo.email:
                                    string PatternEmail = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";
                                    r = new Regex(PatternEmail, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                                    //r = new Regex("^[A-z0-9_.@]*$");
                                    if (!r.IsMatch(sValue))
                                    {
                                        respuesta.Result = true;
                                    }
                                    break;

                                case TipoCampo.Fecha:
                                    DateTime dtValue = new DateTime();
                                    bool bIsDate = DateTime.TryParse(sValue, out dtValue);
                                    respuesta.Result = !bIsDate ? true : respuesta.Result;
                                    break;
                                case TipoCampo.Hipervinculo:
                                    //string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
                                    //string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
                                    //string Pattern = @"https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,}";
                                    string Pattern = @"(http|https):\/\/";
                                    r = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                                    if (!r.IsMatch(sValue))
                                    {
                                        respuesta.Result = true;
                                    }
                                    break;
                                case TipoCampo.Hora:
                                    r = new Regex(@"^(?:(?:0?[1-9]|1[0-2]):[0-5][0-9]\s?(?:[AP][Mm]?|[ap][m]?)?|(?:00?|1[3-9]|2[0-3]):[0-5][0-9])$");
                                    if (!r.IsMatch(sValue))
                                    {
                                        respuesta.Result = true;
                                    }
                                    if (respuesta.Result)
                                    {
                                        try
                                        {
                                            DateTime vDateTime = new DateTime();
                                            bool itsDate = DateTime.TryParse(sValue, out vDateTime);
                                            if (itsDate)
                                            {
                                                string displayTime = vDateTime.ToString("hh:mm tt");
                                                if (r.IsMatch(displayTime))
                                                {
                                                    sValue = displayTime;
                                                    respuesta.Result = false;
                                                }

                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                        }

                                    }
                                    break;
                                case TipoCampo.Numerico:
                                    long iValue = 0;
                                    bool bIsInt = long.TryParse(sValue, out iValue);
                                    respuesta.Result = !bIsInt ? true : respuesta.Result;

                                    break;
                                case TipoCampo.Telefono:

                                    break;
                                case TipoCampo.Texto:
                                    r = new Regex("^[a-zA-Z\x20]*$");
                                    if (!r.IsMatch(sValue))
                                    {
                                        respuesta.Valor = $@"{sTabla} El campo { item.TipoCampo.GetDisplayName() } solo admite letras.";
                                        respuesta.Result = true;
                                    }
                                    break;
                                default:

                                    break;

                            }
                        }

                    }
                    else
                    {
                        respuesta.Valor = $@"{sTabla} El valor cuenta con una longitud de {sValue.Length} y la longitud que permite es de {item.Longitud}, Valor Actual: '{sValue}'";
                        respuesta.Result = true;

                    }
                    if (respuesta.Result)
                        break;
                    iColumnNumber++;
                }



            }
            catch (Exception ex)
            {
                respuesta.HandleError = true;
                respuesta.Valor = ex.Message;

            }
            return respuesta;
        }

        [HttpPost]
        public ActionResult InsertExcel(HttpPostedFileBase ExcelFile, bool bReemplazar, int iId, int PeriodoId = 0, int sysFrecuencia = 0, int sysNumFrecuencia = 0)
        {
            try
            {
                //bitacora
                List<List<cambiosCampos>> LstcambioCampos = new List<List<cambiosCampos>>();
                List<List<AuxCatalogoTablaExcel>> Lsttablas = new List<List<AuxCatalogoTablaExcel>>();
                string Table = "";
                string nombreLargo = "";

                if (PeriodoId == 0)
                {
                    return Json(new { Hecho = false, Mensaje = "Por favor asegurese de seleccionar Periodo gracias." }, JsonRequestBehavior.AllowGet);
                }
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var OrganismoId = usuario.OrganismoID ?? 0;
                string Inseert = "";
                var model = db.Plantillas.Where(x => x.PlantillaId == iId).FirstOrDefault();
                if (model == null)
                    return Json(new { Hecho = false, Mensaje = $"No se encontro la plantilla " }, JsonRequestBehavior.AllowGet);
                if (bReemplazar)
                {
                    if (RemplazarInformacion(model.NombreTabla, OrganismoId, PeriodoId, sysFrecuencia, sysNumFrecuencia,usuario.Id).Length > 0)
                        return Json(new { Hecho = false, Mensaje = "No se pudo desactivar la infromación de la tabla " }, JsonRequestBehavior.AllowGet);
                }
                if (ExcelFile != null)
                {
                    var extencion = ExcelFile.ContentType;
                    var splitArchivo = ExcelFile.FileName.Split('.');
                    string directorio_sub = $@"Archivos\Plantillas\archivosSuplentes\";
                    string Directorio = $@"{Server.MapPath("~/")}{directorio_sub}";
                    var workbook = new XLWorkbook();

                    if (extencion == "application/vnd.ms-excel")
                    {
                        bool exists = System.IO.Directory.Exists(Directorio);
                        if (!exists)
                            System.IO.Directory.CreateDirectory(Directorio);
                        var Input = Directorio + "Input.xls";
                        var Output = Directorio + "Output.xlsx";

                        ExcelFile.SaveAs(Input);
                        Workbook wb = new Workbook();
                        wb.LoadFromFile(Input);
                        wb.SaveToFile(Output, ExcelVersion.Version2013);
                        workbook = new XLWorkbook(Output);
                    }
                    else
                    {
                        workbook = new XLWorkbook(ExcelFile.InputStream);
                    }

                    var ws1 = workbook.Worksheet(1);
                    //var workbook = new XLWorkbook(ExcelFile.InputStream);
                    //var ws1 = workbook.Worksheet(1);
                    var ListaCampos = GetCamposByColumn(iId, false);
                    var OtherFormatExcel = ws1.Range(5, 1, 5, 1).Search("ID").Count() > 0 || ws1.Range(7, 1, 7, 1).IsEmpty() ? true : false;

                    if (ListaCampos.Count > 0)
                    {
                        var first = true;
                        var firstValuee = true;
                        Table = model.NombreTabla;
                        nombreLargo = model.NombreLargo;
                        Inseert = $@"INSERT INTO { model.NombreTabla }(";
                        //ponemos los campos
                        foreach (var item in ListaCampos)
                        {
                            if (!first)
                            {
                                Inseert += ",";
                            }
                            Inseert += $@"{item.Nombre}";
                            first = false;
                        }
                        //ponemos los campos por default
                        Inseert += ",Activo,OrganismoID,UsuarioId,OrganismoCapturoID,FechaCreacion,PeriodoId,sysFrecuencia,sysNumFrecuencia) OUTPUT INSERTED.TablaFisicaId VALUES(";
                        first = true;
                        var vFirstRow = 8;
                        var iContarRows = 0;
                        //y empezamos a poner los campos 
                        var numero = ws1.RowsUsed().Count();
                        //Bitacora
                        List<cambiosCampos> cambioCampos = new List<cambiosCampos>();
                        List<AuxCatalogoTablaExcel> tablas = new List<AuxCatalogoTablaExcel>();
                        foreach (var row in ws1.RowsUsed())
                        {
                            iContarRows++;
                            if (iContarRows >= 500)
                            {
                                first = true;
                                firstValuee = true;
                                iContarRows = 0;
                                Inseert += $@" INSERT INTO { model.NombreTabla }(";
                                foreach (var item in ListaCampos)
                                {
                                    if (!first)
                                    {
                                        Inseert += ",";
                                    }
                                    Inseert += $@"{item.Nombre}";
                                    first = false;
                                }
                                //ponemos los campos por default
                                Inseert += ",Activo,OrganismoID,UsuarioId,OrganismoCapturoID,FechaCreacion,PeriodoId,sysFrecuencia,sysNumFrecuencia) OUTPUT INSERTED.TablaFisicaId VALUES(";
                                first = true;
                            }

                            if (!firstValuee)
                            {
                                Inseert += ",(";
                                LstcambioCampos.Add(cambioCampos);
                                Lsttablas.Add(tablas);
                                cambioCampos = new List<cambiosCampos>();
                                tablas = new List<AuxCatalogoTablaExcel>();
                            }
                            first = true;
                            if (row.RowNumber() >= vFirstRow)
                            {

                                int iCountCampo = OtherFormatExcel ? 2 : 1;
                                int iCountCatalogo = 2;

                                foreach (var item in ListaCampos)
                                {
                                    string valor = "";
                                    if (!first)
                                    {
                                        Inseert += ",";
                                    }
                                    if (item.Activo)
                                    {
                                        row.Clear(XLClearOptions.AllFormats);
                                        
                                        valor = row.Cell(iCountCampo).HasFormula ? row.Cell(iCountCampo).CachedValue.ToString().Replace("'", "") : row.Cell(iCountCampo).GetValue<string>().Replace("'", "");

                                        if (item.TipoCampo == TipoCampo.Fecha)
                                        {
                                            row.Cell(iCountCampo).Style.NumberFormat.Format = "dd/MM/yyyy";
                                            //var valor1 = row.Cell(iCountCampo).CachedValue;
                                            //var valor2 = row.Cell(iCountCampo).GetString();
                                            valor = row.Cell(iCountCampo).GetFormattedString();
                                            //var valor4 = row.Cell(iCountCampo).Value.ToString();
                                            //var valor5 = row.Cell(iCountCampo).GetString() ;
                                            //var valor6 = row.Cell(iCountCampo).GetValue<string>();
                                            //var valor7 = row.Cell(iCountCampo).GetValue<string>();


                                        }

                                        if (item.TipoCampo == TipoCampo.ArchivoAdjunto)
                                        {
                                            valor = "";

                                        }
                                        if (item.TipoCampo == TipoCampo.CasillaVerificacion)
                                        {
                                            valor = valor.ToLower() == "true" ? "1" : "0";
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
                                        if (item.TipoCampo == TipoCampo.Fecha)
                                        {
                                            try
                                            {
                                                DateTime dtValue = new DateTime();
                                                bool bIsDate = DateTime.TryParse(valor, CultureInfo.CreateSpecificCulture("es-mx") , DateTimeStyles.None, out dtValue);
                                                if (bIsDate)
                                                {
                                                    valor = dtValue.ToString("dd/MM/yyyy");
                                                }

                                                //valor = valor.Replace(" 12:00:00 AM", "");
                                                //DateTime dtValue = new DateTime();
                                                //string[] validDateTimeFormats = { "dd/MM/yyyy", "d/m/yyyy", "d/m/yy", "dd/m/yyyy", "dd/m/yy", "d/mm/yyyy", "d/mm/yy" };

                                                ////bool bIsDate = DateTime.TryParse(valor, out dtValue);
                                                //bool bIsDate = DateTime.TryParseExact(valor, validDateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtValue);
                                                //if (bIsDate)
                                                //{
                                                //    valor = dtValue.ToString("dd/MM/yyyy");
                                                //}
                                            }
                                            catch (Exception ex)
                                            {

                                            }

                                        }
                                        if (item.TipoCampo == TipoCampo.Hora)
                                        {
                                            try
                                            {
                                                var r = new Regex(@"^(?:(?:0?[1-9]|1[0-2]):[0-5][0-9]\s?(?:[AP][Mm]?|[ap][m]?)?|(?:00?|1[3-9]|2[0-3]):[0-5][0-9])$");
                                                if (!r.IsMatch(valor))
                                                {
                                                    DateTime vDateTime = new DateTime();
                                                    bool itsDate = DateTime.TryParse(valor, out vDateTime);
                                                    if (itsDate)
                                                    {
                                                        string displayTime = vDateTime.ToString("hh:mm tt");
                                                        if (r.IsMatch(displayTime))
                                                        {
                                                            valor = displayTime;
                                                        }

                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                            }

                                        }
                                        if (item.TipoCampo == TipoCampo.Catalogo)
                                        {

                                            if (HMTLHelperExtensions.GetTipoTabla(item.CatalogoId))
                                            {
                                                if (valor == null || valor == "0" || valor == "")
                                                    valor = "0";
                                                else
                                                {

                                                    tablas.Add(new AuxCatalogoTablaExcel { CatalogoId = item.CatalogoId, numeroExcel = iCountCatalogo, valor = valor });
                                                    valor = "1";
                                                    iCountCatalogo += db.CampoCatalogo.Where(x => x.CatalogoId == item.CatalogoId && x.Activo && x.TipoCampo == TipoCampo.Catalogo).Count();
                                                }
                                            }
                                            else
                                            {
                                                valor = HMTLHelperExtensions.GetIdFromCatalogo(item, valor).ToString();
                                            }
                                            //contamos las tab del excel
                                            iCountCatalogo++;

                                        }
                                        //Inseert += GetValueByCampo(item, valor);
                                        iCountCampo++;
                                    }
                                    else
                                    {
                                        valor = "NULL";
                                        if (item.Requerido)
                                        {
                                            //valor = HMTLHelperExtensions.GetDefaultValue(item);

                                            valor = "";
                                        }
                                        //else
                                        //{
                                        //    Inseert += valor;
                                        //}
                                    }
                                    Inseert += GetValueByCampo(item, valor);
                                    first = false;

                                    //Bitacora
                                    if (item.Activo)
                                    {
                                        cambioCampos.Add(new cambiosCampos
                                        {
                                            nombre_campo = item.Etiqueta,
                                            es_modificado = false,
                                            campo_nuevo = valor
                                        });
                                    }


                                }
                                Inseert += $@",1,{usuario.OrganismoID ?? 0},'{usuario.Id}',{usuario.OrganismoID ?? 0},'{ DateTime.Now.ToString("dd/MM/yyyy") }',{PeriodoId},{sysFrecuencia},{sysNumFrecuencia})";
                                firstValuee = false;



                            }

                        }
                        //Bitacora
                        LstcambioCampos.Add(cambioCampos);
                        Lsttablas.Add(tablas);
                        //una vez terminado empezamos a insertar todos los campos
                        SqlConnection sqlConnection = new SqlConnection(coneccion);
                        sqlConnection.Open();
                        var LstInserted = sqlConnection.Query<int>(Inseert).ToList();
                        sqlConnection.Close();
                        //Bitacora
                        var res = HMTLHelperExtensions.Bitacora(LstcambioCampos, Table, $"Plantilla: {nombreLargo}", 0, usuario?.Id);

                        res = HMTLHelperExtensions.FechaActualizacion(iId);

                        //Una vez guardado la plantilla guardaremos los datos de los catalogos dinamicos
                        if (LstInserted.Count > 0)
                        {
                            var contInserted = 0;
                            foreach (var item in LstInserted)
                            {
                                try
                                {

                                    foreach (var itemCatalogo in Lsttablas[contInserted])
                                    {
                                        var respuesta = this.InsertExcelForTabla(ExcelFile, itemCatalogo.CatalogoId, item, itemCatalogo.valor, itemCatalogo.numeroExcel);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    return Json(new { Hecho = false, Mensaje = "Se guardaron con exito los datos de la plantilla, pero no pudimos guardar uno o mas datos de las tablas." }, JsonRequestBehavior.AllowGet);
                                }
                                contInserted++;
                            }
                        }


                        return Json(new { Hecho = true, Mensaje = "Se guardaron con exito el registro." }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { Hecho = false, Mensaje = "Error al rcuperar los campos." }, JsonRequestBehavior.AllowGet);

                }
                return Json(new { Hecho = false, Mensaje = "Error al recibir el excel." }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ViewBag.ColumnsNames = new List<string>();
                return Json(new { Hecho = false, Mensaje = $"Ocurrio un error,Error: '{ex.Message}' " }, JsonRequestBehavior.AllowGet);
            }

            //string sError = "";

            //if (ExcelFile != null)
            //{
            //    try
            //    {


            //        var workbook = new XLWorkbook(ExcelFile.InputStream);

            //        var ws1 = workbook.Worksheet(1);
            //        var vErrores = new List<string>();
            //        string sQueryInsert = $@"INSERT INTO [dbo].[{model.NombreTablaBD}]
            //                   ({model.CamposFormatSql})
            //             VALUES";
            //        int iCountInsert = 0;
            //        var lastRow = ws1.RowsUsed().LastOrDefault();
            //        var vFirstRow = ws1.RowsUsed().FirstOrDefault();

            //        if (bReemplazar)
            //        {
            //            TablerosDinamicosB.QueryDataTablero($"DELETE FROM {model.NombreTablaBD}");
            //        }

            //        foreach (var row in ws1.RowsUsed())
            //        {
            //            if (row != vFirstRow)
            //            {
            //                List<string> lstErrorsRow = new List<string>();

            //                int iCountCampo = 1;
            //                string sCamposValues = "";
            //                foreach (var item in model.lstCamposTableroDinamicoActivos)
            //                {
            //                    string sValue = row.Cell(iCountCampo).HasFormula ? row.Cell(iCountCampo).ValueCached.ToString().Replace("'", "") : row.Cell(iCountCampo).GetValue<string>().Replace("'", "");
            //                    sCamposValues += iCountCampo == 1 ? "" : ",";
            //                    DateTime dtField = new DateTime();
            //                    if (item.TipoCampo == TiposCampoTablero.Fecha)
            //                        sValue = DateTime.Parse(sValue).ToString("dd-MM-yyyy");
            //                    sCamposValues += $"'{sValue}'";

            //                    iCountCampo++;
            //                }
            //                sQueryInsert += iCountInsert == 0 ? "" : ",";
            //                sQueryInsert += $@" ( {sCamposValues} )";

            //                iCountInsert++;
            //                if (iCountInsert == 1000 || lastRow == row)
            //                {
            //                    TablerosDinamicosB.QueryDataTablero(sQueryInsert);
            //                    sQueryInsert = $@"INSERT INTO [dbo].[{model.NombreTablaBD}]
            //                   ({model.CamposFormatSql})
            //             VALUES";
            //                    iCountInsert = 0;
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        sError = ex.Message;
            //    }
            //}
            //return Json(sError);
        }
        public ResultGuardarTablaFisica InsertExcelForTabla(HttpPostedFileBase ExcelFile, int CatalogoId, int Idregistro, string sValue, int tabExcel = 1, int PeriodoId = 0)
        {
            ResultGuardarTablaFisica resultado = new ResultGuardarTablaFisica();
            resultado.Result = true;
            try
            {
                //bitacora
                List<List<cambiosCampos>> LstcambioCampos = new List<List<cambiosCampos>>();
                //List<List<AuxCatalogoTablaExcel>> Lsttablas = new List<List<AuxCatalogoTablaExcel>>();
                string Table = "";
                string nombreLargo = "";
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                string Inseert = "";

                var model = db.Catalogoes.Where(x => x.CatalogoId == CatalogoId).FirstOrDefault();
                if (model == null)
                {
                    resultado.Result = false;
                    resultado.Valor = "No podemos encontrar el catalago en nuestra base de datos";
                }

                if (ExcelFile != null)
                {
                    var workbook = new XLWorkbook(ExcelFile.InputStream);
                    var ws1 = workbook.Worksheet(tabExcel);
                    var ListaCampos = db.CampoCatalogo.Where(x => x.CatalogoId == CatalogoId && x.Activo).OrderBy(x => x.Orden).ToList();
                    if (ListaCampos.Count > 0)
                    {
                        var first = true;
                        var firstValuee = true;
                        Table = model.NombreTabla;
                        nombreLargo = model.Nombre;
                        Inseert = $@"INSERT INTO { model.NombreTabla }(";
                        //ponemos los campos
                        foreach (var item in ListaCampos)
                        {
                            if (!first)
                            {
                                Inseert += ",";
                            }
                            Inseert += $@"{item.Nombre}";
                            first = false;
                        }
                        //ponemos los campos por default
                        Inseert += ",Activo,OrganismoID,UsuarioId,OrganismoCapturoID,FechaCreacion,Idregistro) OUTPUT INSERTED.TablaFisicaId VALUES(";
                        first = true;
                        //var vFirstRow = 8;
                        //y empezamos a poner los campos 

                        //Bitacora
                        List<cambiosCampos> cambioCampos = new List<cambiosCampos>();
                        var iContarRows = 0;
                        //necesitamos saber filtrar los datos


                        //var rows = ws1.Range(4, 1, ws1.RowsUsed().Count(), 1).Search(sValue).AsList();

                        var rows = ws1.Range(4, 1, ws1.RowsUsed().Count(), 1).CellsUsed(cell => cell.GetString() == sValue).AsList();

                        if (rows.Count > 0)
                        {
                            foreach (var tabla_item in rows)
                            {
                                iContarRows++;
                                if (iContarRows >= 500)
                                {
                                    first = true;
                                    firstValuee = true;
                                    iContarRows = 0;
                                    Inseert += $@" INSERT INTO { model.NombreTabla }(";
                                    foreach (var item in ListaCampos)
                                    {
                                        if (!first)
                                        {
                                            Inseert += ",";
                                        }
                                        Inseert += $@"{item.Nombre}";
                                        first = false;
                                    }
                                    //ponemos los campos por default
                                    Inseert += ",Activo,OrganismoID,UsuarioId,OrganismoCapturoID,FechaCreacion,PeriodoId,sysFrecuencia,sysNumFrecuencia) OUTPUT INSERTED.TablaFisicaId VALUES(";
                                    first = true;
                                }

                                var row = ws1.Rows(tabla_item.Address.RowNumber.ToString()).FirstOrDefault();
                                //verificamoos por cada uno de los valores
                                if (!firstValuee)
                                {
                                    Inseert += ",(";
                                    LstcambioCampos.Add(cambioCampos);
                                    cambioCampos = new List<cambiosCampos>();
                                }
                                first = true;
                                //verificamos si no tiene el id
                                var ColumnTwoEmpty = ws1.Range(3, 2, 3, 2).IsEmpty() ? true : false;
                                int iCountCampo = 1;


                                foreach (var item in ListaCampos)
                                {
                                    if (iCountCampo == 2 && ColumnTwoEmpty)
                                        iCountCampo++;
                                    string valor = "";
                                    if (!first)
                                    {
                                        Inseert += ",";
                                    }
                                    if (item.Activo)
                                    {
                                        valor = row.Cell(iCountCampo).HasFormula ? row.Cell(iCountCampo).CachedValue.ToString().Replace("'", "") : row.Cell(iCountCampo).GetValue<string>().Replace("'", "");
                                        if (item.TipoCampo == TipoCampo.ArchivoAdjunto)
                                        {
                                            valor = "";

                                        }
                                        if (item.TipoCampo == TipoCampo.CasillaVerificacion)
                                        {
                                            valor = valor.ToLower() == "true" ? "1" : "0";
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
                                        if (item.TipoCampo == TipoCampo.Fecha)
                                        {
                                            try
                                            {
                                                DateTime dtValue = new DateTime();
                                                bool bIsDate = DateTime.TryParse(valor, out dtValue);
                                                if (bIsDate)
                                                {
                                                    valor = dtValue.ToString("dd/MM/yyyy");
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                            }

                                        }
                                        if (item.TipoCampo == TipoCampo.Hora)
                                        {
                                            try
                                            {
                                                var r = new Regex(@"^(?:(?:0?[1-9]|1[0-2]):[0-5][0-9]\s?(?:[AP][Mm]?|[ap][m]?)?|(?:00?|1[3-9]|2[0-3]):[0-5][0-9])$");
                                                if (!r.IsMatch(valor))
                                                {
                                                    DateTime vDateTime = new DateTime();
                                                    bool itsDate = DateTime.TryParse(valor, out vDateTime);
                                                    if (itsDate)
                                                    {
                                                        string displayTime = vDateTime.ToString("hh:mm tt");
                                                        if (r.IsMatch(displayTime))
                                                        {
                                                            valor = displayTime;
                                                        }

                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                            }

                                        }
                                        if (item.TipoCampo == TipoCampo.Catalogo)
                                        {

                                            valor = HMTLHelperExtensions.GetIdFromCatalogo(item, valor).ToString();


                                        }
                                        iCountCampo++;
                                    }
                                    else
                                    {
                                        valor = "NULL";
                                        if (item.Requerido)
                                        {
                                            valor = "";
                                        }
                                    }
                                    Inseert += GetValueByCampo(item, valor);
                                    first = false;

                                    //Bitacora
                                    if (item.Activo)
                                    {
                                        cambioCampos.Add(new cambiosCampos
                                        {
                                            nombre_campo = item.Etiqueta,
                                            es_modificado = false,
                                            campo_nuevo = valor
                                        });
                                    }


                                }
                                Inseert += $@",1,{usuario.OrganismoID ?? 0},'{usuario.Id}',{usuario.OrganismoID ?? 0},'{ DateTime.Now.ToString("dd/MM/yyyy") }',{Idregistro})";
                                firstValuee = false;

                                //Bitacora
                                LstcambioCampos.Add(cambioCampos);
                            }

                        }


                        //una vez terminado empezamos a insertar todos los campos
                        SqlConnection sqlConnection = new SqlConnection(coneccion);
                        sqlConnection.Open();
                        var LstInserted = sqlConnection.Query<int>(Inseert).ToList();
                        sqlConnection.Close();
                        //Bitacora
                        var res = HMTLHelperExtensions.Bitacora(LstcambioCampos, Table, $"Catalogo: {nombreLargo}", 0, usuario?.Id);
                    }

                }

            }
            catch (Exception ex)
            {
                resultado.Result = false;
                resultado.Valor = ex.Message;
            }
            return resultado;
        }


        public string RemplazarInformacion(string NombreTabla, int OrganismoId, int PeriodoId, int sysFrecuencia, int sysNumFrecuencia,string usuarioId)
        {
            string respuesta = "";
            string sQuery = "";
            try
            {
                //using (SqlCommand RetrieveOrderCommand = new SqlCommand())
                //{
                //    RetrieveOrderCommand.CommandTimeout = 150;
                //}
                //sQuery = $@"UPDATE {NombreTabla } SET Activo=0";


                SqlParameter param = new SqlParameter()
                {
                    ParameterName = "@nombreTabla",
                    Value = NombreTabla
                };

                SqlParameter param2 = new SqlParameter()
                {
                    ParameterName = "@OrganismoId",
                    Value = OrganismoId
                };
                SqlParameter param3 = new SqlParameter()
                {
                    ParameterName = "@PeriodoId",
                    Value = PeriodoId
                };
                SqlParameter param4 = new SqlParameter()
                {
                    ParameterName = "@sysFrecuencia",
                    Value = sysFrecuencia
                };
                //SqlParameter param5 = new SqlParameter()
                //{
                //    ParameterName = "@sysNumFrecuencia",
                //    Value = sysNumFrecuencia
                //};
                SqlParameter param6 = new SqlParameter()
                {
                    ParameterName = "@usuarioId",
                    Value = usuarioId
                };



                //SqlCommand cmd = new SqlCommand("getCCM", con);
                //cmd.CommandType = CommandType.StoredProcedure;

                //una vez terminado empezamos a insertar todos los campos
                SqlConnection sqlConnection = new SqlConnection(coneccion);
                //sqlConnection.Open();
                //sqlConnection.Query(sQuery);
                //sqlConnection.Close();
                sqlConnection.Open();
                SqlCommand command = new SqlCommand("reemplazarDatosImportar", sqlConnection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(param);
                command.Parameters.Add(param2);
                command.Parameters.Add(param3);
                command.Parameters.Add(param4);
                //command.Parameters.Add(param5);
                command.Parameters.Add(param6);

                // Setting command timeout to 3000000 second
                command.CommandTimeout = 3000000;
                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                respuesta = ex.Message;
            }
            return respuesta;


        }

        #endregion

        #region ModuloBombon
        public ActionResult AsignarObligacionesPorOrganismos()
        {
            var OrganismoId = GetOrganismoEnlace();
            if (OrganismoId == 0)
            {
                return RedirectToAction("Index", "Plantillas");
            }
            List<DependenciasVModel> vDependencias = db.UnidadesAdministrativas.Where(x => x.Activo && x.OrganismoId == OrganismoId).Select(s => new DependenciasVModel
            {
                Id = s.UnidadAdministrativaId,
                Nombre = s.NombreUnidad
            }).ToList();
            ViewBag.LeyId = db.Leyes.Where(x => x.Activo).OrderBy(x => x.Orden).ToList();
            ViewBag.ArticuloId = new List<Articulo>();
            ViewBag.FraccionId = new List<Articulo>();
            ViewBag.LstPeriodo = db.Periodos.Where(x => x.Activo).OrderBy(x => x.Orden).ToList();

            ViewBag.LstDependencias = vDependencias;

            return View();
        }



        [HttpPost]
        public ActionResult AsignarObligacionesPorOrganismosPost(int UnidadAdministrativaId = 0, int LeyId = 0, int ArticuloId = 0, int FraccionId = 0, int PeriodoId = 0)
        {
            if (UnidadAdministrativaId == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Por favor seleccione una unidad administrativa para continuar." }, JsonRequestBehavior.AllowGet);
            }
            var OrganismoId = GetOrganismoEnlace();
            List<AuxPlantillas> vModel = new List<AuxPlantillas>();
            try
            {
                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                string sQuery = $@"
                           SELECT d.Nombre as LeyNombre,d.LeyId,
			                c.Nombre as ArticuloNombre, c.ArticuloId,
			                b.Nombre as FracionNombre,
			                e.*,
							CASE WHEN  (SELECT COUNT(UnidadAdministrativaId) FROM PlantillaUnidadAdministrativas x WHERE x.PlantillaId = a.PlantillaId AND x.UnidadAdministrativaId={UnidadAdministrativaId} ) > 0 THEN 'True' ELSE 'False' END as Seleccionado
			
		
	                FROM PlantillaFraccions  a
                                            LEFT JOIN Fraccions b ON a.FraccionId = b.FraccionId
                                            LEFT JOIN Articuloes c ON b.ArticuloId = c.ArticuloId
                                            LEFT JOIN Leys d ON c.LeyId = d.LeyId
                                            LEFT JOIN Plantillas e ON a.PlantillaId = e.PlantillaId
											LEFT JOIN PlantillaOrganismos f ON a.PlantillaId = f.PlantillaId

                WHERE f.OrganismoID={OrganismoId} AND e.Publicado=1 ";
                if (LeyId > 0)
                {
                    sQuery += $@" AND d.LeyId = {LeyId}";

                }
                if (ArticuloId > 0)
                {
                    sQuery += $@" AND c.ArticuloId = {ArticuloId}";

                }
                if (FraccionId > 0)
                {
                    sQuery += $@" AND b.FraccionId = {FraccionId}";
                }
                if (PeriodoId > 0)
                {
                    sQuery += $@" AND (SELECT COUNT(*) FROM PeriodoPlantillas WHERE Periodo_PeriodoId = {PeriodoId} AND Plantilla_PlantillaId = a.PlantillaId) > 0 ";
                }
                //sQuery += $@" GROUP BY d.Nombre,d.LeyId, c.Nombre, c.ArticuloId, b.Nombre,e.*,z.*";
                sQuery += " ORDER BY d.Orden,c.Orden,b.Orden, e.Orden";


                vModel = sqlConnection.Query<AuxPlantillas>(sQuery).ToList();

                //return PartialView("_ListaAsignarObligacionesPorOrganismos", vModel);

                string viewContent = ConvertViewToString("_ListaAsignarObligacionesPorOrganismos", vModel);
                return Json(new { Hecho = true, Mensaje = "Se consultaron exitosamente", Partial = viewContent }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            //return PartialView("_ListaAsignarObligacionesPorOrganismos", vModel);
        }
        [HttpPost]
        public ActionResult AsignarPlantillaUnidadAdministrativa(bool sAsignar, int plantillaId = 0, int UnidadAdministrativaId = 0)
        {
            try
            {
                if (plantillaId == 0 || UnidadAdministrativaId == 0)
                {
                    return Json(new { Hecho = false, Mensaje = "No pudimos encontrar la Unidad Administrativa o Plantilla para asignar." }, JsonRequestBehavior.AllowGet);
                }
                if (sAsignar)
                {
                    if (!db.PlantillaUnidadAdministrativa.Where(x => x.PlantillaId == plantillaId && x.UnidadAdministrativaId == UnidadAdministrativaId).Any())
                    {
                        var model = new PlantillaUnidadAdministrativa();
                        model.PlantillaId = plantillaId;
                        model.UnidadAdministrativaId = UnidadAdministrativaId;
                        db.PlantillaUnidadAdministrativa.Add(model);
                        db.SaveChanges();
                        //bitacora
                        this.CreateBitacora(null, model, model.PlantillaUnidadAdministrativaId);
                    }
                }
                else
                {
                    var lstModel = db.PlantillaUnidadAdministrativa.Where(x => x.PlantillaId == plantillaId && x.UnidadAdministrativaId == UnidadAdministrativaId).ToList();
                    db.PlantillaUnidadAdministrativa.RemoveRange(lstModel);
                    db.SaveChanges();
                    foreach (var item in lstModel)
                    {
                        //bitacora
                        this.CreateBitacora(item, null, 0);
                    }

                }



                return Json(new { Hecho = true, Mensaje = "Se consultaron exitosamente" }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            //return PartialView("_ListaAsignarObligacionesPorOrganismos", vModel);
        }
        #endregion

        #region ModuloBombonAdmin
        public ActionResult AsignarObligacionesPorDependencia()
        {
            //var OrganismoId = GetOrganismoEnlace();
            //if (OrganismoId == 0)
            //{
            //    return RedirectToAction("Index", "Plantillas");
            //}
            //Buscamos todas las dependencias
            List<DependenciasVModel> vDependencias = db.Organismos.Where(x => x.Activo).Select(s => new DependenciasVModel
            {
                Id = s.OrganismoID,
                Nombre = s.NombreOrganismo
            }).ToList();

            ViewBag.LeyId = db.Leyes.Where(x => x.Activo).OrderBy(x => x.Orden).ToList();
            ViewBag.ArticuloId = new List<Articulo>();
            ViewBag.FraccionId = new List<Articulo>();
            ViewBag.LstPeriodo = db.Periodos.Where(x => x.Activo).OrderBy(x => x.Orden).ToList();

            ViewBag.LstDependencias = vDependencias;

            return View();
        }



        [HttpPost]
        public ActionResult AsignarObligacionesPorDependenciaPost(int OrganismoId = 0, int LeyId = 0, int ArticuloId = 0, int FraccionId = 0, int PeriodoId = 0)
        {
            if (OrganismoId == 0)
            {
                return Json(new { Hecho = false, Mensaje = "Por favor seleccione una dependencia para continuar." }, JsonRequestBehavior.AllowGet);
            }
            //var OrganismoId = GetOrganismoEnlace();
            List<AuxPlantillas> vModel = new List<AuxPlantillas>();
            try
            {
                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                string sQuery = $@"
                           SELECT a.NombreCorto,
                                  a.NombreLargo,
                                  a.Ayuda,
                                  a.PlantillaId,
							CASE WHEN  (SELECT COUNT(OrganismoID) FROM PlantillaOrganismos x WHERE x.PlantillaId = a.PlantillaId AND x.OrganismoID={OrganismoId} ) > 0 THEN 'True' ELSE 'False' END as Seleccionado
	                FROM Plantillas  a";

                bool Where = false;
                //if (LeyId > 0)
                //{
                //    sQuery += !Where ? " WHERE " : " AND ";
                //    Where = true;
                //    sQuery += $@" d.LeyId = {LeyId}";

                //}
                //if (ArticuloId > 0)
                //{
                //    sQuery += !Where ? " WHERE " : " AND ";
                //    Where = true;
                //    sQuery += $@" c.ArticuloId = {ArticuloId}";

                //}
                //if (FraccionId > 0)
                //{
                //    sQuery += !Where ? " WHERE " : " AND ";
                //    Where = true;
                //    sQuery += $@" b.FraccionId = {FraccionId}";
                //}
                if (PeriodoId > 0)
                {
                    sQuery += !Where ? " WHERE " : " AND ";
                    sQuery += $@" (SELECT COUNT(*) FROM PeriodoPlantillas WHERE Periodo_PeriodoId = {PeriodoId} AND Plantilla_PlantillaId = a.PlantillaId) > 0 ";
                }
                //sQuery += $@" GROUP BY d.Nombre,d.LeyId, c.Nombre, c.ArticuloId, b.Nombre,e.*,z.*";
                sQuery += " ORDER BY a.Orden";


                vModel = sqlConnection.Query<AuxPlantillas>(sQuery).ToList();

                //return PartialView("_ListaAsignarObligacionesPorOrganismos", vModel);

                string viewContent = ConvertViewToString("_ListaAsignarObligacionesPorDependencia", vModel);
                return Json(new { Hecho = true, Mensaje = "Se consultaron exitosamente", Partial = viewContent }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            //return PartialView("_ListaAsignarObligacionesPorOrganismos", vModel);
        }
        [HttpPost]
        public ActionResult AsignarPlantillaDependencia(bool sAsignar, int plantillaId = 0, int OrganismoId = 0)
        {
            try
            {
                if (plantillaId == 0 || OrganismoId == 0)
                {
                    return Json(new { Hecho = false, Mensaje = "No pudimos encontrar la Dependencia o Plantilla para asignar." }, JsonRequestBehavior.AllowGet);
                }
                if (sAsignar)
                {
                    if (!db.PlantillaOrganismos.Where(x => x.PlantillaId == plantillaId && x.OrganismoID == OrganismoId).Any())
                    {
                        var model = new PlantillaOrganismos();
                        model.PlantillaId = plantillaId;
                        model.OrganismoID = OrganismoId;
                        db.PlantillaOrganismos.Add(model);
                        db.SaveChanges();
                        //bitacora
                        this.CreateBitacora(null, model, model.PlantillaOrganismosId);

                    }
                }
                else
                {
                    var lstModel = db.PlantillaOrganismos.Where(x => x.PlantillaId == plantillaId && x.OrganismoID == OrganismoId).ToList();
                    db.PlantillaOrganismos.RemoveRange(lstModel);
                    db.SaveChanges();
                    foreach (var item in lstModel)
                    {
                        //bitacora
                        this.CreateBitacora(item, null, 0);
                    }
                }
                return Json(new { Hecho = true, Mensaje = "Se consultaron exitosamente" }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            //return PartialView("_ListaAsignarObligacionesPorOrganismos", vModel);
        }
        #endregion

        #region ExcelMethod
        //ACTION RESULT PERSONALIZADO
        public class ExcelResult : ActionResult
        {
            private readonly XLWorkbook _workbook;
            private readonly string _fileName;

            public ExcelResult(XLWorkbook workbook, string fileName)
            {
                _workbook = workbook;
                _fileName = fileName;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                var response = context.HttpContext.Response;
                response.Clear();
                response.ContentType = "application/vnd.openxmlformats-officedocument."
                                     + "spreadsheetml.sheet";
                response.AddHeader("content-disposition",
                                   "attachment;filename=\"" + _fileName + ".xlsx\"");

                using (var memoryStream = new MemoryStream())
                {
                    _workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(response.OutputStream);
                }
                response.End();
            }
        }
        #endregion

        #region Clonar
        [HttpPost]
        public ActionResult ClonarPlantilla(int plantillaId = 0)
        {
            try
            {
                if (plantillaId == 0)
                {
                    return Json(new { Hecho = false, Mensaje = "No pudimos encontrar Plantilla para clonar, contacte al administrador." }, JsonRequestBehavior.AllowGet);
                }
                ResultGuardarTablaFisica respuesta = ClonarPlantillaScript(plantillaId);
                if (!respuesta.Result)
                {
                    return Json(new { Hecho = false, Mensaje = respuesta.Valor }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Hecho = true, Mensaje = "Se Clono la plantilla exitosamente" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Hecho = false, Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            //return PartialView("_ListaAsignarObligacionesPorOrganismos", vModel);
        }

        public ResultGuardarTablaFisica ClonarPlantillaScript(int plantillaIdOld = 0)
        {
            ResultGuardarTablaFisica respuesta = new ResultGuardarTablaFisica();
            respuesta.Result = true;
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                //Clonar Plantilla
                var Insert = $@"INSERT INTO Plantillas(NombreCorto,NombreLargo,Ayuda,Orden,Activo,Publicado,Frecuencia,PeriodoDesde,PeriodoHasta,IdPlantillaPNT)
                                OUTPUT Inserted.PlantillaId
                                SELECT NombreCorto, NombreLargo, Ayuda, Orden, Activo, Publicado, Frecuencia, PeriodoDesde, PeriodoHasta, IdPlantillaPNT
                                FROM Plantillas WHERE PlantillaId = {plantillaIdOld}";
                //guardamos informacion
                SqlConnection sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                var plantillaIdNew = sqlConnection.Query<int>(Insert).FirstOrDefault();
                sqlConnection.Close();

                if (plantillaIdNew == 0)
                {
                    respuesta.Result = false;
                    respuesta.Valor = "No se puedo obtener el Id de la plantilla clonada, por favor contacte al administrador.";
                    return respuesta;
                }
                //Clonar campos
                Insert = $@"INSERT INTO Campoes (TipoCampo,Nombre,Etiqueta, Longitud,Requerido,Ayuda,Activo,CatalogoId,PlantillaId,Orden,_TipoFecha,_ConDecimales,_GrupoExtensionId,_Size,relevantes,IdCampoPNT,IdTipoCampoPNT) 
	                            SELECT TipoCampo,Nombre,Etiqueta, Longitud,Requerido,Ayuda,Activo,CatalogoId,{plantillaIdNew} as PlantillaId,Orden,_TipoFecha,_ConDecimales,_GrupoExtensionId,_Size,relevantes,IdCampoPNT,IdTipoCampoPNT
	                            FROM Campoes WHERE PlantillaId = {plantillaIdOld}";
                //guardamos informacion
                sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                sqlConnection.Query(Insert);
                sqlConnection.Close();

                //Clonar plantilla fracciones
                Insert = $@"INSERT INTO PlantillaFraccions (PlantillaId,FraccionId) 
	                        SELECT {plantillaIdNew} as PlantillaId,FraccionId
	                         FROM PlantillaFraccions WHERE PlantillaId = {plantillaIdOld}";
                //guardamos informacion
                sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                sqlConnection.Query(Insert);
                sqlConnection.Close();

                //Clonar plantilla organismos
                Insert = $@"INSERT INTO PlantillaOrganismos (PlantillaId,OrganismoID) 
	                        SELECT {plantillaIdNew} as PlantillaId,OrganismoID
	                        FROM PlantillaOrganismos WHERE PlantillaId = {plantillaIdOld}";
                //guardamos informacion
                sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                sqlConnection.Query(Insert);
                sqlConnection.Close();

                //Clonar plantilla unidades administrativas
                Insert = $@"INSERT INTO PlantillaUnidadAdministrativas (PlantillaId,UnidadAdministrativaId) 
	                        SELECT {plantillaIdNew} as PlantillaId,UnidadAdministrativaId
	                        FROM PlantillaUnidadAdministrativas WHERE PlantillaId = {plantillaIdOld}";
                //guardamos informacion
                sqlConnection = new SqlConnection(coneccion);
                sqlConnection.Open();
                sqlConnection.Query(Insert);
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                respuesta.Result = false;
                respuesta.Valor = ex.Message;
            }
            return respuesta;
        }

        #endregion

        #region Bitacora
        //Bitacora para campoes
        private void CreateBitacora(Campo oldModel, Campo newModel, int id = 0)
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

                if (newModel != null && newModel.CatalogoId != 0)
                {
                    newCatalogo = db.Catalogoes.Where(x => x.CatalogoId == newModel.CatalogoId).FirstOrDefault()?.Nombre;
                }
                if (oldModel != null && oldModel.CatalogoId != 0)
                {
                    oldCatalogo = db.Catalogoes.Where(x => x.CatalogoId == oldModel.CatalogoId).FirstOrDefault()?.Nombre;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Catálogo",
                    es_modificado = oldModel != null && newModel != null && oldModel.CatalogoId != newModel.CatalogoId ? true : false,
                    campo_nuevo = newCatalogo,
                    campo_anterior = oldCatalogo
                });
                //PlantillaId
                string newPlantilla = "";
                string oldPlantilla = "";

                if (newModel != null && newModel.PlantillaId != 0)
                {
                    newPlantilla = db.Plantillas.Where(x => x.PlantillaId == newModel.PlantillaId).FirstOrDefault()?.NombreCorto;
                }
                if (oldModel != null && oldModel.PlantillaId != 0)
                {
                    oldPlantilla = db.Plantillas.Where(x => x.PlantillaId == oldModel.PlantillaId).FirstOrDefault()?.NombreCorto;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Plantilla",
                    es_modificado = oldModel != null && newModel != null && oldModel.PlantillaId != newModel.PlantillaId ? true : false,
                    campo_nuevo = newPlantilla,
                    campo_anterior = oldPlantilla
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

                //Relevante
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Relevante",
                    es_modificado = oldModel != null && newModel != null && oldModel.relevantes != newModel?.relevantes ? true : false,
                    campo_nuevo = newModel != null ? HMTLHelperExtensions.getStringBoolBitacora(newModel.relevantes) : null,
                    campo_anterior = oldModel != null ? HMTLHelperExtensions.getStringBoolBitacora(oldModel.relevantes) : null
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
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "Campoes", "Campos", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }

        //Bitacora para Pla ntillaUnidad administrativa
        private void CreateBitacora(PlantillaUnidadAdministrativa oldModel, PlantillaUnidadAdministrativa newModel, int id = 0)
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

                //PlantillaId
                string newPlantilla = "";
                string oldPlantilla = "";

                if (newModel != null && newModel.PlantillaId != 0)
                {
                    newPlantilla = db.Plantillas.Where(x => x.PlantillaId == newModel.PlantillaId).FirstOrDefault()?.NombreCorto;
                }
                if (oldModel != null && oldModel.PlantillaId != 0)
                {
                    oldPlantilla = db.Plantillas.Where(x => x.PlantillaId == oldModel.PlantillaId).FirstOrDefault()?.NombreCorto;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Plantilla",
                    es_modificado = oldModel != null && newModel != null && oldModel.PlantillaId != newModel.PlantillaId ? true : false,
                    campo_nuevo = newPlantilla,
                    campo_anterior = oldPlantilla
                });
                //UnidadAdministrtivoa
                string newUidadAdministrativa = "";
                string oldUidadAdministrativa = "";

                if (newModel != null && newModel.UnidadAdministrativaId != 0)
                {
                    newUidadAdministrativa = db.UnidadesAdministrativas.Where(x => x.UnidadAdministrativaId == newModel.UnidadAdministrativaId).FirstOrDefault()?.NombreUnidad;
                }
                if (oldModel != null && oldModel.PlantillaId != 0)
                {
                    oldUidadAdministrativa = db.UnidadesAdministrativas.Where(x => x.UnidadAdministrativaId == oldModel.UnidadAdministrativaId).FirstOrDefault()?.NombreUnidad;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Unidad Administrativa",
                    es_modificado = oldModel != null && newModel != null && oldModel.UnidadAdministrativaId != newModel.UnidadAdministrativaId ? true : false,
                    campo_nuevo = newUidadAdministrativa,
                    campo_anterior = oldUidadAdministrativa
                });

                //Bitacora
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "PlantillaUnidadAdministrativas", "Plantillas y Unidad Administrativa", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
        //Bitacora para Plantilla Organismo
        private void CreateBitacora(PlantillaOrganismos oldModel, PlantillaOrganismos newModel, int id = 0)
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

                //PlantillaId
                string newPlantilla = "";
                string oldPlantilla = "";

                if (newModel != null && newModel.PlantillaId != 0)
                {
                    newPlantilla = db.Plantillas.Where(x => x.PlantillaId == newModel.PlantillaId).FirstOrDefault()?.NombreCorto;
                }
                if (oldModel != null && oldModel.PlantillaId != 0)
                {
                    oldPlantilla = db.Plantillas.Where(x => x.PlantillaId == oldModel.PlantillaId).FirstOrDefault()?.NombreCorto;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Plantilla",
                    es_modificado = oldModel != null && newModel != null && oldModel.PlantillaId != newModel.PlantillaId ? true : false,
                    campo_nuevo = newPlantilla,
                    campo_anterior = oldPlantilla
                });
                //Organismo
                string newOrganismo = "";
                string oldOrganismo = "";

                if (newModel != null && newModel.OrganismoID != 0)
                {
                    newOrganismo = db.Organismos.Where(x => x.OrganismoID == newModel.OrganismoID).FirstOrDefault()?.NombreOrganismo;
                }
                if (oldModel != null && oldModel.OrganismoID != 0)
                {
                    oldOrganismo = db.Organismos.Where(x => x.OrganismoID == oldModel.OrganismoID).FirstOrDefault()?.NombreOrganismo;
                }
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Dependencia",
                    es_modificado = oldModel != null && newModel != null && oldModel.OrganismoID != newModel.OrganismoID ? true : false,
                    campo_nuevo = newOrganismo,
                    campo_anterior = oldOrganismo
                });

                //Bitacora
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "PlantillaOrganismos", "Plantillas y Dependencia", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }

        #endregion


        #region chosenSection
        public ActionResult IndexHistory(FiltrosPlantillaHistory filtros, int plantillaId = 0, int PerPage = 10, int iPagina = 1)
        {

            var UnidadAdministrativaId = HMTLHelperExtensions.GetUnidadId(User.Identity.Name);
            if ((UnidadAdministrativaId != null && UnidadAdministrativaId != 0))
            {
                return RedirectToAction("IndexPlantillas", "Plantillas");
            }
            
            if (plantillaId == 0)
            {
                return RedirectToAction("Index", "Plantillas");
            }

            if(filtros.OrganismoID == 0)
                filtros.OrganismoID = GetOrganismoEnlace() ?? 0;
            IPagedList<PlantillaHistory> vModel = null;

            vModel = db.PlantillaHistory.Include(x=>x.Periodo).Include(x=>x.Organismo).Where(x =>
            (filtros.PeriodoId == 0 || x.PeriodoId == filtros.PeriodoId)
            && (filtros.SysFrecuencia == null || x.SysFrecuencia == filtros.SysFrecuencia)
            && (filtros.SysNumFrecuencia == 0 || x.SysNumFrecuencia == filtros.SysNumFrecuencia)
            && (filtros.OrganismoID == 0 || filtros.OrganismoID == x.OrganismoID)
            && x.PlantillaId == plantillaId
            ).OrderByDescending(x=>x.FechaCreacion).ToPagedList(iPagina, PerPage);

            if (Request.IsAjaxRequest())
            {
                return PartialView("Chosen/_listaHistory", vModel);
            }

            ViewBag.LstPeriodo = db.Periodos.Where(x => x.Activo).OrderBy(x => x.Orden).Select(x => new ListaSeleccion { Text = x.NombrePeriodo, Value = x.PeriodoId }).ToList();
            ViewBag.LstDependencias = (from a in db.Organismos
                                       join s in db.PlantillaOrganismos on a.OrganismoID equals s.OrganismoID
                                       where s.PlantillaId == plantillaId
                                       select new
                                       {
                                           Id = a.OrganismoID,
                                           Nombre = a.NombreOrganismo
                                       }).ToList();
            SetFrecuencia((int)plantillaId);
            ViewBag.plantillaId = plantillaId;
            return View("Chosen/IndexHistory",vModel);
        }

        public ActionResult GenerateChosen(int plantillaId = 0)
        {
            var model = db.Plantillas.Where(x => x.PlantillaId == plantillaId).FirstOrDefault();
            if (model == null)
                return View("~/Views/Errors/PageNotFound.cshtml");
            
            this.VerifyNecessaryColumn((int)plantillaId);
            SetFrecuencia((int)plantillaId);
            ViewBag.NombrePlantilla = TituloFromato(HMTLHelperExtensions.GetLeyArtiucloFraccionFromPlantilla(plantillaId));
            ViewBag.LstDependencias = (from a in db.Organismos
                         join s in db.PlantillaOrganismos on a.OrganismoID equals s.OrganismoID
                         where s.PlantillaId == plantillaId
                         select new
                         {
                             Id = a.OrganismoID,
                             Nombre = a.NombreOrganismo
                         }).ToList();
            return View("Chosen/GenerateChosen", model);
        }

        public ActionResult InitChosenPublic(int plantillaId, int sysFrecuencia, int sysNumFrecuencia, int PeriodoId, int organismoId = 0)
        {

            var model = db.Plantillas.Where(x => x.PlantillaId == plantillaId).FirstOrDefault();
            if (model == null || PeriodoId == 0)
                return View("~/Views/Errors/PageNotFound.cshtml");

            var sPeriodo = db.Periodos.FirstOrDefault(x => x.PeriodoId == PeriodoId);
            if (sPeriodo == null)
                return View("~/Views/Errors/PageNotFound.cshtml");

            var vmodel = new vmChosenPuiblic();
            vmodel.plantillaId = plantillaId;
            vmodel.sysFrecuencia = (FrecuenciaActualizacion)sysFrecuencia;
            vmodel.sysNumFrecuencia = sysNumFrecuencia;
            vmodel.PeriodoId = PeriodoId;
            vmodel.organismoId = organismoId;
            vmodel.Descripcion = $"Seleccion aleatorea de {vmodel.sysFrecuencia.GetFrecuencia(vmodel.sysNumFrecuencia)} del periodo {sPeriodo.NombrePeriodo} ";
            vmodel.textPlantilla = TituloFromato(HMTLHelperExtensions.GetLeyArtiucloFraccionFromPlantilla(plantillaId));

            return View("ChosenPublic/index", vmodel);
        }

        [HttpPost]
        public ActionResult GenerateChosenPublic(int plantillaId, int sysFrecuencia, int sysNumFrecuencia, int PeriodoId, int organismoId = 0)
        {
            var vmodel = new vmChosenPuiblic();

            try
            {
                var error = Json(new { Hecho = false, Mensaje = $"No se encontro la plantilla " }, JsonRequestBehavior.AllowGet); ;
                var plantilla = db.Plantillas.Where(x => x.PlantillaId == plantillaId).FirstOrDefault();
                if (plantilla == null)
                {
                    vmodel.Errors = "No se encontro la plantilla ";
                    return View("ChosenPublic/servidoresPublicos", vmodel);
                }

                var sPeriodo = db.Periodos.FirstOrDefault(x => x.PeriodoId == PeriodoId);
                vmodel.plantillaId = plantillaId;
                vmodel.sysFrecuencia = (FrecuenciaActualizacion)sysFrecuencia;
                vmodel.sysNumFrecuencia = sysNumFrecuencia;
                vmodel.PeriodoId = PeriodoId;
                vmodel.organismoId = organismoId;
                vmodel.Descripcion = $"Seleccion aleatorea de {vmodel.sysFrecuencia.GetFrecuencia(vmodel.sysNumFrecuencia)} del periodo {sPeriodo.NombrePeriodo} ";
                vmodel.textPlantilla = TituloFromato(HMTLHelperExtensions.GetLeyArtiucloFraccionFromPlantilla(plantillaId));

                organismoId = organismoId == 0 ? GetOrganismoEnlace() ?? 0 : organismoId;
                var tableName = plantilla.NombreTabla;
                var totalRow = GetTotalFromTable(plantilla.NombreTabla, PeriodoId, organismoId);
                var top = Convert.ToInt32(plantilla.Porcentage);// Convert.ToInt32((plantilla.Porcentage / 100) * totalRow);
                top = top == 0 ? 1 : top;

                if(totalRow == 0)
                {
                    vmodel.Errors = "Error: ocurrio un error al momento de generar el sorteo, no se encuentran registros disponibles en la plantilla.";
                    return View("ChosenPublic/index", vmodel);
                    
                }

                int plantillaHistoryId = 0;
                List<int> ListadoInserted = new List<int>();

                plantillaHistoryId = this.CreatePlantillaHistory(plantillaId, PeriodoId, sysFrecuencia, sysNumFrecuencia, organismoId);
                if (plantillaHistoryId == 0)
                {
                    vmodel.Errors = "Error: ocurrio un error al momento de generar el apartado de historial";
                    return View("ChosenPublic/index", vmodel);
                }

                if (this.CheckChosen(plantilla.NombreTabla, PeriodoId, organismoId, sysNumFrecuencia))
                {
                    restoreChoosen(plantilla.NombreTabla, PeriodoId, organismoId, sysNumFrecuencia);
                   // return View("ChosenPublic/servidoresPublicos", vmodel);
                }

                ListadoInserted = this.SetChosen(tableName, top, sysFrecuencia, sysNumFrecuencia, organismoId, PeriodoId);
                if (ListadoInserted.Count == 0)
                {
                    vmodel.Errors = "Error: ocurrio un error al momento de generar el sorteo.";
                    return View("ChosenPublic/index", vmodel);
                }
                if (this.SetChosenHistory(plantilla, ListadoInserted, plantillaHistoryId))
                {
                    FormCollection form = new FormCollection();
                    vmodel.result = GetSelectFromTableHistoryAsyncPublic(plantillaId, form, GetCamposByColumn(plantillaId), plantillaHistoryId);
                    return View("ChosenPublic/servidoresPublicos", vmodel);
                }


                this.DeletePlantillaHistory(plantillaHistoryId);
                vmodel.Errors = "Error: ocurrio un error al momento de asignar los registros en el apartado de historial";
                return View("ChosenPublic/index", vmodel);
            }
            catch (Exception ex)
            {
                ViewBag.ColumnsNames = new List<string>();
            }

            return View("ChosenPublic/index", vmodel);
        }

        [HttpPost]

        public ActionResult GenerateChosen(int plantillaId, int sysFrecuencia, int sysNumFrecuencia,int PeriodoId, int organismoId = 0)
        {

            try
            {
                var error = Json(new { Hecho = false, Mensaje = $"No se encontro la plantilla " }, JsonRequestBehavior.AllowGet); ;
                var plantilla = db.Plantillas.Where(x => x.PlantillaId == plantillaId).FirstOrDefault();
                if (plantilla == null)
                    return Json(new { Hecho = false, Mensaje = $"No se encontro la plantilla " }, JsonRequestBehavior.AllowGet);
                
                organismoId = organismoId == 0 ? GetOrganismoEnlace() ?? 0 : organismoId;
                var tableName = plantilla.NombreTabla;
                var totalRow = GetTotalFromTable(plantilla.NombreTabla,PeriodoId, organismoId);
                var top = Convert.ToInt32(plantilla.Porcentage); //Convert.ToInt32((plantilla.Porcentage / 100) * totalRow);
                top = top == 0 ? 1 : top;

                if((FrecuenciaActualizacion)sysFrecuencia == FrecuenciaActualizacion.Anual)
                {
                    if (this.CheckChosen(plantilla.NombreTabla, PeriodoId,organismoId))
                    {
                        restoreChoosen(plantilla.NombreTabla, PeriodoId, organismoId);
                    }
                }

                int plantillaHistoryId = 0;
                List<int> ListadoInserted = new List<int>();
                
                plantillaHistoryId = this.CreatePlantillaHistory(plantillaId, PeriodoId, sysFrecuencia, sysNumFrecuencia, organismoId);
                if (plantillaHistoryId == 0)
                {
                    return Json(new { Hecho = false, Mensaje = $"Error: ocurrio un error al momento de generar el apartado de historial " }, JsonRequestBehavior.AllowGet);
                }

                if (this.CheckChosen(plantilla.NombreTabla, PeriodoId, organismoId, sysNumFrecuencia))
                {
                    restoreChoosen(plantilla.NombreTabla, PeriodoId, organismoId, sysNumFrecuencia);
                }

                ListadoInserted = this.SetChosen(tableName, top, sysFrecuencia, sysNumFrecuencia, organismoId, PeriodoId);
                if (ListadoInserted.Count == 0)
                {
                    return Json(new { Hecho = false, Mensaje = $"Error: ocurrio un error al momento de hacer la seleccion, o ya no existen servidores publicos por seleccionar. " }, JsonRequestBehavior.AllowGet);
                }
                if (this.SetChosenHistory(plantilla, ListadoInserted, plantillaHistoryId))
                {
                    return Json(new { Hecho = true, PlantillaHistoryId = plantillaHistoryId, Mensaje = $"Los servidores públicos fueron seleccionados, a continuación se le redireccionará a la lista de los servidores seleccionados para esta frecuencia." }, JsonRequestBehavior.AllowGet);
                }

                //if(ListadoInserted.Count == 0)
                //{
                //    return Json(new { Hecho = false, Mensaje = $"Error: ocurrio un error al momento de hacer la seleccion, o ya no existen servidores publicos por seleccionar. " }, JsonRequestBehavior.AllowGet);
                //}
                //if (this.SetChosenHistory(plantilla,ListadoInserted, plantillaHistoryId))
                //{
                //    return Json(new { Hecho = true,PlantillaHistoryId= plantillaHistoryId, Mensaje = $"Los servidores públicos fueron seleccionados exitosamente, a continuación se le redireccionará a la lista de los servidores seleccionados para esta frecuencia." }, JsonRequestBehavior.AllowGet);
                //}

                this.DeletePlantillaHistory(plantillaHistoryId);
                return Json(new { Hecho = false, Mensaje = $"Error: ocurrio un error al momento de seleccionar los servidores publicos. " }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                ViewBag.ColumnsNames = new List<string>();
                return Json(new { Hecho = false, Mensaje = $"Error: '{ex.Message}' " }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CheckChoseen(int plantillaId, int sysFrecuencia, int sysNumFrecuencia, int PeriodoId, int organismoId = 0)
        {

            try
            {
                var plantilla = db.Plantillas.Where(x => x.PlantillaId == plantillaId).FirstOrDefault();
                if (plantilla == null)
                    return Json(new { Hecho = false, Mensaje = $"No se encontro la plantilla " }, JsonRequestBehavior.AllowGet);

                if (this.CheckChosen(plantilla.NombreTabla, PeriodoId, sysNumFrecuencia))
                {
                    return Json(new { Hecho = false, Mensaje = $"Existen información seleccionada para esta frecuencia, desea generarlo otra vez, este proceso dejará la información anterior en un Historial y volverá a habilitar a los servidores públicos para que puedan ser seleccionados otra vez." }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Hecho = true}, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                ViewBag.ColumnsNames = new List<string>();
                return Json(new { Hecho = false, Mensaje = $"Error: '{ex.Message}' " }, JsonRequestBehavior.AllowGet);
            }
        }

        private int GetTotalFromTable(string Table,int periodoId,int OrganismoID)
        {
            int totalRows = 0;
            try
            {
                totalRows = db.Database.SqlQuery<int>($"SELECT COUNT(TablaFisicaId) FROM {Table} " +
                    $"WHERE PeriodoId = {periodoId}" +
                    $"AND OrganismoID = {OrganismoID}" +
                    $"AND Activo = 1").FirstOrDefault();
            }
            catch (Exception ex)
            {

            }
            
            return totalRows;

        }

        private List<int> SetChosen(string Table,int top, int sysFrecuencia, int sysNumFrecuencia, int organismoId,int periodoId)
        {
            string updateScript = $@"UPDATE {Table} SET {Table}.Chosen = 1
                            ,sysFrecuencia={sysFrecuencia}
                            ,sysNumFrecuencia={sysNumFrecuencia}
                            OUTPUT INSERTED.TablaFisicaId
                            WHERE 
                            Activo=1 
                            AND {Table}.OrganismoID = {organismoId}
                            AND {Table}.PeriodoId = {periodoId}
                            AND {Table}.TablaFisicaId in 
                            (SELECT TOP({top}) TablaFisicaId FROM {Table} 
                                WHERE 
                                Activo=1 
                                AND ({Table}.Chosen=0 OR {Table}.Chosen is null)
                                AND {Table}.OrganismoID = {organismoId}
                                AND {Table}.PeriodoId = {periodoId}
                                ORDER BY newid())";
            try
            {
                return db.Database.SqlQuery<int>(updateScript).ToList();
            }
            catch (Exception ex)
            {
            }

            return new List<int>();

        }

        private bool SetChosenHistory(Plantilla plantilla,List<int> ids,int plantillaHistoryId)
        {
            var campos = db.Campos.Where(x => x.PlantillaId == plantilla.PlantillaId).ToList();
            if (campos == null && campos.Count <= 0)
            {
                return false;
            }
            string query = $@"INSERT INTO {plantilla.NombreTablaHistory} ({String.Join(", ", campos.Select(x => x.Nombre))},Activo,OrganismoID,UsuarioId,OrganismoCapturoID,FechaCreacion,PeriodoId,sysFrecuencia,sysNumFrecuencia,PlantillaHistoryId)
                                SELECT {String.Join(", ", campos.Select(x => x.Nombre))},Activo,OrganismoID,UsuarioId,OrganismoCapturoID,FechaCreacion,PeriodoId,sysFrecuencia,sysNumFrecuencia,PlantillaHistoryId = {plantillaHistoryId}
                                FROM {plantilla.NombreTabla}
                                WHERE TablaFisicaId IN ({String.Join(", ", ids)});";
            try
            {
                db.Database.ExecuteSqlCommand(query);
                return true;
            }
            catch (Exception ex)
            {   
            }

            return false;

        }

        private bool CheckChosen(string Table,int periodo,int OrganismoID, int numFrecuencia=0)
        {
            var withFrecuencia = numFrecuencia > 0 ? $" AND {Table}.Sysnumfrecuencia = {numFrecuencia}" : "";
            string sQuery = $@"IF EXISTS(SELECT TOP(1) * FROM {Table} WHERE {Table}.Chosen = 1 {withFrecuencia} AND Periodoid= {periodo} AND OrganismoID={OrganismoID} AND Activo=1)
	                            SELECT CAST(1 AS BIT)
                            ELSE
	                            SELECT CAST(0 AS BIT)";
            try
            {
                var count = db.Database.SqlQuery<bool>(sQuery).FirstOrDefault();
                return count;
            }
            catch (Exception ex)
            {
            }

            return false;

        }

        private bool restoreChoosen(string Table, int periodo,int OrganismoID, int numFrecuencia = 0)
        {
            var withFrecuencia = numFrecuencia > 0 ? $"{Table}.Sysnumfrecuencia = {numFrecuencia}" : "";
            string sQuery = $@"UPDATE  {Table} SET Chosen = 0
                            WHERE {withFrecuencia} {(numFrecuencia > 0  ? "AND" : "")} Periodoid= {periodo} AND OrganismoID={OrganismoID} AND Activo=1";
            try
            {
                db.Database.ExecuteSqlCommand(sQuery);
                return true;
            }
            catch (Exception ex)
            {
            }

            return false;

        }

        private int CreatePlantillaHistory(int plantillaId,int periodo,int frecuencia, int numFrecuencia, int organismoID)
        {
            try
            {
                PlantillaHistory plantillaHistory = new PlantillaHistory();
                plantillaHistory.PeriodoId = periodo;
                plantillaHistory.SysFrecuencia = (FrecuenciaActualizacion)frecuencia;
                plantillaHistory.SysNumFrecuencia = numFrecuencia;
                plantillaHistory.FechaCreacion = DateTime.Now;
                plantillaHistory.UsuarioId = User.Identity.GetUserId();
                plantillaHistory.PlantillaId = plantillaId;
                plantillaHistory.OrganismoID = organismoID;
                plantillaHistory.Activo = true;
                db.PlantillaHistory.Add(plantillaHistory);
                db.SaveChanges();
                return plantillaHistory.PlantillaHistoryId; 
            }
            catch (Exception ex)
            {
            }

            return 0;

        }

        private bool DeletePlantillaHistory(int plantillaId)
        {
            try
            {
                var model = db.PlantillaHistory.FirstOrDefault(x=>x.PlantillaHistoryId == plantillaId);
                if(model != null)
                {
                    db.PlantillaHistory.Remove(model);
                    db.SaveChanges();
                    return true;

                }
               
            }
            catch (Exception ex)
            {
            }

            return false;

        }

        public ActionResult IndexDatosChosenPlantillas(FormCollection form,int PlantillaHistoryId, bool allFields = false, int plantillaId = 0, int PerPage = 10, int iPagina = 1)
        {
            if (plantillaId == 0)
            {
                return RedirectToAction("Index", "Plantillas");
            }

            var lista = GetSelectFromTableHistoryAsync(plantillaId, form, GetCamposByColumn(plantillaId), PlantillaHistoryId, PerPage, iPagina, allFields);

            if (Request.IsAjaxRequest())
            {
                return PartialView("Chosen/_listaDatosChosenPlantillas", lista);
            }

            GenerarFiltros(plantillaId);
            SetFrecuencia(plantillaId);
            ViewBag.iId = plantillaId;
            ViewBag.PlantillaHistoryId = PlantillaHistoryId;
            ViewBag.NombrePlantilla = "Vista de seleccion";
            return View("Chosen/IndexChosenPlantillas", lista);
        }

        [HttpPost]
        public ActionResult IndexDatosChosenPlantillasPOST(FormCollection form, int PlantillaHistoryId, bool allFields = false, int plantillaId = 0, int PerPage = 10, int iPagina = 1)
        {

            if (plantillaId == 0)
            {
                return RedirectToAction("Index", "Plantillas");
            }

            var lista = GetSelectFromTableHistoryAsync(plantillaId, form, GetCamposByColumn(plantillaId), PlantillaHistoryId, PerPage, iPagina, allFields);

            if (Request.IsAjaxRequest())
            {
                return PartialView("Chosen/_listaDatosChosenPlantillas", lista);
            }

            GenerarFiltros(plantillaId);
            SetFrecuencia(plantillaId);
            ViewBag.iId = plantillaId;
            ViewBag.PlantillaHistoryId = PlantillaHistoryId;
            ViewBag.NombrePlantilla = "Vista de seleccion";
            return View("Chosen/IndexChosenPlantillas", lista);
        }

        public vmResultadoDatos2 GetSelectFromTableHistoryAsync(int iPlantillaId, FormCollection form, List<Campo> Campos = null, int PlantillaHistoryId  = 0, int iPorPagina = 10, int iPagina = 1, bool bRelevante = false)
        {

            var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
            vmResultadoDatos2 resultado = new vmResultadoDatos2();
            List<AuxTitlePlantillas> Titulos = new List<AuxTitlePlantillas>();
            try
            {
                var sYear = "''";
                if (iPagina == 0)
                {
                    iPagina = 1;
                }
                //if (iPeriodo == 0)
                //{
                //    sYear = $@"'{DateTime.Now.Year.ToString()}'";
                //}
                vmPlantillaCampos campos = HMTLHelperExtensions.GetCamposForDatos(iPlantillaId, !bRelevante);
                if (campos != null && campos.campos.Count > 0)
                {
                    string NombreTabla = campos.NombreTablaHistory;
                    //paginador
                    string Query = $@"SELECT 
                             {iPlantillaId} as PlantillaId,
                             '{NombreTabla}' as NombreTabla,
                             count(a.TablaFisicaId) totalRegistros,
                             ((count(a.TablaFisicaId) + {iPorPagina} - 1) / {iPorPagina}) totalPaginas,
                             {iPagina} paginaActual, ";

                    //Datos
                    Query += $@"(SELECT h.TablaFisicaId, h.Activo
								 ,((";
                    //campos
                    Query += $@"SELECT NombreCampo, Valor, CASE ";
                    string QuerCatalogo = "";
                    //PAARA LOS CASES DE CAMPOS
                    string SubQueryCaseEsTabla = " CASE WHEN ";
                    bool hayTabla = false;
                    bool bSubQueryCaseEsTabla = false;
                    //PARA EL FORM DEL CAMPO
                    string SubQueryCampo = $@" FROM (SELECT TOP 1 CAST({NombreTabla}.TablaFisicaId AS nvarchar) as TablaFisicaId, CAST({NombreTabla}.Activo AS nvarchar) as Activo ";
                    //bool bSubQueryCampo = false;
                    string sUpivot = $@" UNPIVOT(Valor FOR NombreCampo IN( ";
                    bool firstUpivt = true;
                    List<string> lstInnerJoin = new List<string>();
                    //UNPIVOT(Valor FOR NombreCampo IN(a.TablaFisicaId, a.Ejercicio, a.Fecha_Inicio_Periodo_Informa)) AS unp  FOR JSON PATH
                    foreach (var item in campos.campos.Where(x => x.Nombre != "TablaFisicaId").ToList())
                    {
                        Titulos.Add(new AuxTitlePlantillas { LbNombre = item.Etiqueta });
                        Query += $@" WHEN NombreCampo = '{item.Nombre}' THEN {item.TipoCampo.GetHashCode()} ";
                        sUpivot += $@"{(!firstUpivt ? "," : "")} a.{item.Nombre}";
                        firstUpivt = false;
                        if (item.TipoCampo == Transparencia.Models.TipoCampo.Catalogo)
                        {
                            if (!item.TablaCatalogo)
                            {
                                string NombreTablaCatalogo = item.NombreTablaCatalogo;


                                //SubQueryCampo += $@", CAST({NombreTablaCatalogo}.{item.NombreCatalogo} AS nvarchar) as {item.Nombre} ";
                                SubQueryCampo += $@", CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTablaCatalogo}.{item.NombreCatalogo} AS nvarchar(MAX)),{item.TipoCampoCatalogo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX)) as {item.Nombre}  ";
                                if (!lstInnerJoin.Any(x => x == NombreTablaCatalogo))
                                {
                                    QuerCatalogo += $" LEFT JOIN {NombreTablaCatalogo} ON {NombreTabla}.{item.Nombre} = {NombreTablaCatalogo}.TablaFisicaId ";
                                    lstInnerJoin.Add(NombreTablaCatalogo);
                                }

                            }
                            else
                            {
                                //SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTabla}.{item.Nombre} AS nvarchar),{item.TipoCampo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as nvarchar)  as {item.Nombre} ";
                                SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({item.CatalogoId} AS  nvarchar(MAX)),100,{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX))  as {item.Nombre} ";
                                SubQueryCaseEsTabla += $@" {(bSubQueryCaseEsTabla ? "OR" : "")} NombreCampo = '{item.Nombre}' ";
                                bSubQueryCaseEsTabla = true;
                                hayTabla = true;
                            }
                        }
                        else
                        {
                            SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTabla}.{item.Nombre} AS  nvarchar(MAX)),{item.TipoCampo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX))  as {item.Nombre} ";
                        }
                    }
                    Query += " END AS TipoCampo, ";
                    SubQueryCaseEsTabla += $@" THEN 1 ELSE 0 END AS EsTabla ";
                    Query += hayTabla ? SubQueryCaseEsTabla : " 0 AS EsTabla ";
                    SubQueryCampo += $@" FROM {NombreTabla} {QuerCatalogo} WHERE {NombreTabla}.TablaFisicaId = h.TablaFisicaId) a";
                    Query += SubQueryCampo;

                    sUpivot += $@")) AS unp  FOR JSON PATH ) ) AS campos";
                    Query += sUpivot;

                    //Where Section
                    string sWhere = "";
                    if (Campos != null && campos.campos.Count > 0)
                    {
                        foreach (var item in Campos.Where(x => x.TipoCampo != TipoCampo.ArchivoAdjunto).ToList())
                        {
                            string valor = form[$@"{item.Nombre}"];

                            if (valor != null && valor != "")
                            {
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

                                }
                                else if (item._TipoFecha == TipoFecha.FechaDesde)
                                {

                                }
                                else
                                {

                                    sWhere += $@" AND {item.Nombre} {sLikeIgual} {GetValueByCampo(item, valor, (sLikeIgual == "LIKE"))}";
                                    //SQueryWhereForRows += $@" AND {item.Nombre} {sLikeIgual} {GetValueByCampo(item, valor, (sLikeIgual == "LIKE"))}";


                                }

                            }
                        }
                    }
                    //var PeriodoId = form["PeriodoId"];
                    //var sysFrecuencia = form["sysFrecuencia"];
                    //var sysNumFrecuencia = form["sysNumFrecuencia"];
                    //if (PeriodoId  != null && PeriodoId.Length > 0)
                    //{
                    //    iPeriodo = Convert.ToInt32(PeriodoId);
                    //}
                    //else
                    //{
                    //    iPeriodo = db.Periodos.Where(x => x.Activo).OrderBy(x => x.Orden).First().PeriodoId;
                    //}
                    //if (PeriodoId.Length > 0)
                    //{
                    //    iPeriodo = Convert.ToInt32(PeriodoId);
                    //}

                    var Enlace = HMTLHelperExtensions.GetRoles(User.Identity.Name, "Enlace");


                    //h.Activo=1 AND 
                    Query += $@" FROM {NombreTabla} h
                                 LEFT JOIN Periodoes p ON h.PeriodoId = p.PeriodoId
                                 WHERE h.PlantillaHistoryId={PlantillaHistoryId} 
                                 AND h.Activo=1
                                {sWhere} 
                                 ORDER BY h.FechaCreacion
		                                OFFSET (({iPagina} * {iPorPagina}) - {iPorPagina}) ROWS FETCH NEXT {iPorPagina} ROWS ONLY
                                 FOR JSON PATH ) as datos ";

                    //endpaginador
                    var chosen = " a.Chosen = 1 ";
                    Query += $@"FROM {NombreTabla} a 
	                            LEFT JOIN Periodoes p ON a.PeriodoId = p.PeriodoId
                          WHERE a.Activo=1 AND a.PlantillaHistoryId={PlantillaHistoryId} 
                          {sWhere} ";

                    //consultamos los datos
                     SqlMapper.AddTypeHandler(new vmRowDatosHandler());
                    SqlMapper.AddTypeHandler(new vmcampoDatosHandler());
                    using (var idb = db.Database.Connection)
                    {
                        resultado = idb.Query<vmResultadoDatos2>(
                              Query
                              ).FirstOrDefault();

                        SqlMapper.ResetTypeHandlers();
                    }

                }
            }
            catch (Exception ex)
            {
                string var = ex.Message;

            }
            ViewBag.Titles = Titulos;
            return resultado;
        }
        public vmResultadoDatos2 GetSelectFromTableHistoryAsyncPublic(int iPlantillaId, FormCollection form, List<Campo> Campos = null, int PlantillaHistoryId = 0, int iPorPagina = 10, int iPagina = 1, bool bRelevante = false)
        {
            var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
            vmResultadoDatos2 resultado = new vmResultadoDatos2();
            List<AuxTitlePlantillas> Titulos = new List<AuxTitlePlantillas>();
            try
            {
                var sYear = "''";
                if (iPagina == 0)
                {
                    iPagina = 1;
                }
                //if (iPeriodo == 0)
                //{
                //    sYear = $@"'{DateTime.Now.Year.ToString()}'";
                //}
                vmPlantillaCampos campos = HMTLHelperExtensions.GetCamposForDatos(iPlantillaId, !bRelevante);
                if (campos != null && campos.campos.Count > 0)
                {
                    string NombreTabla = campos.NombreTablaHistory;
                    //paginador
                    string Query = $@"SELECT 
                             {iPlantillaId} as PlantillaId,
                             '{NombreTabla}' as NombreTabla,
                             count(a.TablaFisicaId) totalRegistros,
                             ((count(a.TablaFisicaId) + {iPorPagina} - 1) / {iPorPagina}) totalPaginas,
                             {iPagina} paginaActual, ";

                    //Datos
                    Query += $@"(SELECT h.TablaFisicaId, h.Activo
								 ,((";
                    //campos
                    Query += $@"SELECT NombreCampo, Valor, CASE ";
                    var QueryOrdenSeleccionPublico = $@"CASE ";
                    string QuerCatalogo = "";
                    //PAARA LOS CASES DE CAMPOS
                    string SubQueryCaseEsTabla = "";
                    bool hayTabla = false;
                    bool bSubQueryCaseEsTabla = false;
                    //PARA EL FORM DEL CAMPO
                    string SubQueryCampo = $@" FROM (SELECT TOP 1 CAST({NombreTabla}.TablaFisicaId AS nvarchar) as TablaFisicaId, CAST({NombreTabla}.Activo AS nvarchar) as Activo ";
                    //bool bSubQueryCampo = false;
                    string sUpivot = $@" UNPIVOT(Valor FOR NombreCampo IN( ";
                    bool firstUpivt = true;
                    List<string> lstInnerJoin = new List<string>();
                    //UNPIVOT(Valor FOR NombreCampo IN(a.TablaFisicaId, a.Ejercicio, a.Fecha_Inicio_Periodo_Informa)) AS unp  FOR JSON PATH
                    foreach (var item in campos.campos.Where(x => x.Nombre != "TablaFisicaId").ToList())
                    {
                        Titulos.Add(new AuxTitlePlantillas { LbNombre = item.Etiqueta });
                        Query += $@" WHEN NombreCampo = '{item.Nombre}' THEN {item.TipoCampo.GetHashCode()} ";
                        QueryOrdenSeleccionPublico += $@" WHEN NombreCampo = '{item.Nombre}' THEN { (item.OrdenSeleccionPublico.HasValue ? (int)item.OrdenSeleccionPublico.Value : 0)} ";
                        sUpivot += $@"{(!firstUpivt ? "," : "")} a.{item.Nombre}";
                        firstUpivt = false;
                        if (item.TipoCampo == Transparencia.Models.TipoCampo.Catalogo)
                        {
                            if (!item.TablaCatalogo)
                            {
                                string NombreTablaCatalogo = item.NombreTablaCatalogo;


                                //SubQueryCampo += $@", CAST({NombreTablaCatalogo}.{item.NombreCatalogo} AS nvarchar) as {item.Nombre} ";
                                SubQueryCampo += $@", CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTablaCatalogo}.{item.NombreCatalogo} AS nvarchar(MAX)),{item.TipoCampoCatalogo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX)) as {item.Nombre}  ";
                                if (!lstInnerJoin.Any(x => x == NombreTablaCatalogo))
                                {
                                    QuerCatalogo += $" LEFT JOIN {NombreTablaCatalogo} ON {NombreTabla}.{item.Nombre} = {NombreTablaCatalogo}.TablaFisicaId ";
                                    lstInnerJoin.Add(NombreTablaCatalogo);
                                }

                            }
                            else
                            {
                                //SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTabla}.{item.Nombre} AS nvarchar),{item.TipoCampo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as nvarchar)  as {item.Nombre} ";
                                SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({item.CatalogoId} AS  nvarchar(MAX)),100,{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX))  as {item.Nombre} ";
                                SubQueryCaseEsTabla += $@" {(bSubQueryCaseEsTabla ? "OR" : "")} NombreCampo = '{item.Nombre}' ";
                                bSubQueryCaseEsTabla = true;
                                hayTabla = true;
                            }
                        }
                        else
                        {
                            SubQueryCampo += $@",CAST((dbo.fun_formato_valor_dinamico(CAST({NombreTabla}.{item.Nombre} AS  nvarchar(MAX)),{item.TipoCampo.GetHashCode()},{(item._ConDecimales.HasValue && item._ConDecimales.Value ? 1 : 0)})) as  nvarchar(MAX))  as {item.Nombre} ";
                        }
                    }
                    QueryOrdenSeleccionPublico += " END AS OrdenSeleccionPublico, ";
                    Query += $" END AS TipoCampo, {QueryOrdenSeleccionPublico} ";
                    SubQueryCaseEsTabla += $@" THEN 1 ELSE 0 END AS EsTabla ";
                    Query += hayTabla ? SubQueryCaseEsTabla : " 0 AS EsTabla ";
                    SubQueryCampo += $@" FROM {NombreTabla} {QuerCatalogo} WHERE {NombreTabla}.TablaFisicaId = h.TablaFisicaId) a";
                    Query += SubQueryCampo;

                    sUpivot += $@")) AS unp  FOR JSON PATH ) ) AS campos";
                    Query += sUpivot;

                    //Where Section
                    string sWhere = "";
                    if (Campos != null && campos.campos.Count > 0)
                    {
                        foreach (var item in Campos.Where(x => x.TipoCampo != TipoCampo.ArchivoAdjunto).ToList())
                        {
                            string valor = form[$@"{item.Nombre}"];

                            if (valor != null && valor != "")
                            {
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

                                }
                                else if (item._TipoFecha == TipoFecha.FechaDesde)
                                {

                                }
                                else
                                {

                                    sWhere += $@" AND {item.Nombre} {sLikeIgual} {GetValueByCampo(item, valor, (sLikeIgual == "LIKE"))}";
                                    //SQueryWhereForRows += $@" AND {item.Nombre} {sLikeIgual} {GetValueByCampo(item, valor, (sLikeIgual == "LIKE"))}";


                                }

                            }
                        }
                    }
                    //var PeriodoId = form["PeriodoId"];
                    //var sysFrecuencia = form["sysFrecuencia"];
                    //var sysNumFrecuencia = form["sysNumFrecuencia"];
                    //if (PeriodoId  != null && PeriodoId.Length > 0)
                    //{
                    //    iPeriodo = Convert.ToInt32(PeriodoId);
                    //}
                    //else
                    //{
                    //    iPeriodo = db.Periodos.Where(x => x.Activo).OrderBy(x => x.Orden).First().PeriodoId;
                    //}
                    //if (PeriodoId.Length > 0)
                    //{
                    //    iPeriodo = Convert.ToInt32(PeriodoId);
                    //}

                    var Enlace = HMTLHelperExtensions.GetRoles(User.Identity.Name, "Enlace");


                    //h.Activo=1 AND 
                    Query += $@" FROM {NombreTabla} h
                                 LEFT JOIN Periodoes p ON h.PeriodoId = p.PeriodoId
                                 WHERE h.PlantillaHistoryId={PlantillaHistoryId} 
                                {sWhere} 
                                 ORDER BY h.FechaCreacion
		                                
                                 FOR JSON PATH ) as datos ";

                    //endpaginador
                    var chosen = " a.Chosen = 1 ";
                    Query += $@"FROM {NombreTabla} a 
	                            LEFT JOIN Periodoes p ON a.PeriodoId = p.PeriodoId
                          WHERE a.PlantillaHistoryId={PlantillaHistoryId} 
                          {sWhere} ";

                    //consultamos los datos
                    SqlMapper.AddTypeHandler(new vmRowDatosHandler());
                    SqlMapper.AddTypeHandler(new vmcampoDatosHandler());
                    using (var idb = db.Database.Connection)
                    {
                        resultado = idb.Query<vmResultadoDatos2>(
                              Query
                              ).FirstOrDefault();

                        SqlMapper.ResetTypeHandlers();
                    }
                    //var datos = new List<vmDatosPublico>();
                    //foreach (var item in resultado.datos)
                    //{
                    //    foreach (var campo in item.campos)
                    //    {
                    //        datos.Add(new vmDatosPublico()
                    //        { 
                    //             Valor = campo.Valor,
                    //        });
                    //    }
                        
                    //}

                }
            }
            catch (Exception ex)
            {
                string var = ex.Message;

            }
            ViewBag.Titles = Titulos;
            return resultado;
        }





        //PDF
        public ActionResult CreatePdfChosen(FormCollection form, int PlantillaHistoryId, bool allFields = false, int id = 0, int PerPage = 1000, int iPagina = 1)
        {
            if (id == 0)
            {
                return RedirectToAction("Index", "Plantillas");
            }



            // Define la URL de la Cabecera 
            //string _headerUrl = Url.Action("HeaderPDF", "Home", null, "https");
            //// Define la URL del Pie de página(OrdenSeleccionPublico)tblCampoDinamico.AddOrdenSeleccion
            //string _footerUrl = Url.Action("FooterPDF", "Home", null, "https");
            var plantillaHistory = db.PlantillaHistory.Include(x=>x.Periodo).Include(x=>x.Plantillas).Where(x => x.PlantillaHistoryId == PlantillaHistoryId).FirstOrDefault();
            var lista = GetSelectFromTableHistoryAsync(id, form, GetCamposByColumn(id), PlantillaHistoryId, PerPage, iPagina, allFields);
            lista.fechaCreacion = plantillaHistory.FechaCreacion;
            lista.IsPreved = plantillaHistory.Plantillas.IsPreved;
            ViewBag.sysNumFrecuenciaStr = plantillaHistory.SysNumFrecuenciaToString;
            ViewBag.sPeriodo = plantillaHistory.Periodo.NombrePeriodo ?? "";
            ViewBag.totalRows = lista.totalRegistros;
            ViewBag.textPlantilla = TituloFromato(HMTLHelperExtensions.GetLeyArtiucloFraccionFromPlantilla(plantillaHistory.PlantillaId));
            string footer = $"--footer-right \"Fecha: {plantillaHistory.FechaCreacionToString} \" " + "--footer-center \"Página: [page] de [toPage]\" --footer-line --footer-font-size \"9\" --footer-spacing 5 --footer-font-name \"calibri light\"";
            return new ViewAsPdf("Chosen/PdfChosen", lista)

            {
                // Establece la Cabecera y el Pie de página 
                //CustomSwitches = "--header-html " + _headerUrl + " --header-spacing 0 " +
                //                 "--footer-html " + _footerUrl + " --footer-spacing 0"
                //,
                PageSize = Rotativa.Options.Size.A4,
                CustomSwitches = footer,
                PageMargins = new Rotativa.Options.Margins(20, 10, 20, 10)
            };
        }

        public ActionResult PDFDynamicRecord(int? tablaFisicaId, int? plantilldaId = 0)
        {
            if (tablaFisicaId == null)
            {
                return RedirectToAction("IndexPlantillas", "Plantillas");
            }
            var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();

            if (usuario == null)
            {
                return RedirectToAction("IndexPlantillas", "Plantillas");
            }
            var plantilla = db.Plantillas.Include(x => x.Periodos).Where(x => x.PlantillaId == plantilldaId).FirstOrDefault();
            if(plantilla == null)
            {
                return RedirectToAction("IndexPlantillas", "Plantillas");
            }

            this.VerifyNecessaryColumn((int)plantilldaId);
            List<Campo> LstCampos = this.GetValuesFromTable((int)plantilldaId, (int)tablaFisicaId, plantilla.NombreTabla, usuario.OrganismoID ?? 0);

            List<vmResultPdfCatalogos> lstCamposTabla = new List<vmResultPdfCatalogos>();
            foreach (var item in LstCampos.Where(x=>x.CatalogoId  > 0))
            {
                var Catalogo = db.Catalogoes.Where(x => x.CatalogoId == item.CatalogoId).FirstOrDefault();
                if (Catalogo.Tabla)
                {
                    item.isTable = true;

                    var campos =  this.GetValuesFromTable((int)item.CatalogoId, (int)item.TablaFisicaId, Catalogo.NombreTabla, usuario.OrganismoID ?? 0,true);
                    lstCamposTabla.Add(new vmResultPdfCatalogos() { nombreTabla = item.Nombre, camposTabla = campos });
                }
            }
            vmResultPdf model = new vmResultPdf();
            model.campos = LstCampos;
            model.Tablas = lstCamposTabla;
            model.isPreved = plantilla.IsPreved;
            ViewBag.PlantillaId = plantilldaId;
            ViewBag.textPlantilla = TituloFromato(HMTLHelperExtensions.GetLeyArtiucloFraccionFromPlantilla(plantilldaId));
         
            return new ViewAsPdf("PdfRowDetail/Pdf", model)
            {
                CustomSwitches = "--page-offset 0 --footer-center Pagina: [page] de [toPage] --footer-font-size 8",
                // Establece la Cabecera y el Pie de página 
                //CustomSwitches = "--header-html " + _headerUrl + " --header-spacing 0 " +
                //                 "--footer-html " + _footerUrl + " --footer-spacing 0"
                //,
                PageSize = Rotativa.Options.Size.A4
              //,FileName = "ReporteSeleccion.pdf" // SI QUEREMOS QUE EL ARCHIVO SE DESCARGUE DIRECTAMENTE
              ,
                PageMargins = new Rotativa.Options.Margins(10, 10, 10, 10)
            };
        }

        private string getSysNumFrecuencia(int sysNumFrecuencia=0)
        {
            switch (sysNumFrecuencia)
            {
                case 1:
                    return "Enero";
                    break;
                case 2:
                    return "Febrero";
                    break;
                case 3:
                    return "Marzo";
                    break;
                case 4:
                    return "Abril";
                    break;
                case 5:
                    return "Mayo";
                    break;
                case 6:
                    return "Junio";
                    break;
                case 7:
                    return "Julio";
                    break;
                case 8:
                    return "Agosto";
                    break;
                case 9:
                    return "Agosto";
                    break;
                case 10:
                    return "Octubre";
                    break;
                case 11:
                    return "Noviembre";
                    break;
                case 12:
                    return "Diciembre";
                    break;
                default:
                    return "Enero";
                    break;
            }
        }



        #endregion

    }

}