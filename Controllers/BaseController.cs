using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Transparencia.Models;

namespace Transparencia.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var chkTimeOut = Session.Timeout;
            //Acciones Permitidas
            List<VMPermisos> LstAccionesPermitidas = getPermisos();


            string actionName = ControllerContext.RouteData.Values["action"].ToString();
            string controllerName = ControllerContext.RouteData.Values["controller"].ToString();
            var Roles = LstAccionesPermitidas.Where(x => x.Accion == actionName && x.Controller == controllerName).FirstOrDefault();

            var usuario = ((Usuario)Session["Application"]);

            if (usuario == null || !usuario.Activo)
            {
                filterContext.Result = RedirectToAction("Login", "Account");
                Dispose();
                TempData["MensajePermiso"] = "Tu cuenta no tiene permisos para realizar esta acción";
                return;
            }


            if (!HMTLHelperExtensions.TienePermiso(User.Identity.Name,Roles?.Role))
            {
                filterContext.Result = RedirectToAction("Index", "Home");
                Dispose();
                TempData["MensajePermiso"] = "Tu cuenta no tiene permisos para realizar esta acción";
                return;
            }
        }
       public List<VMPermisos> getPermisos()
        {
            //Acciones Permitidas
            List<VMPermisos> LstAccionesPermitidas = new List<VMPermisos>();
            VMPermisos VMPermisos = new VMPermisos();
            #region Articulos
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "Articulos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "Articulos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "Articulos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "Articulos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "Articulos", Role = new List<string>() { "Administrador" } });
            #endregion
            #region TipoOrganismos
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "TipoOrganismos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "TipoOrganismos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "TipoOrganismos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "TipoOrganismos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "TipoOrganismos", Role = new List<string>() { "Administrador" } });
            #endregion


            #region Organismos
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "Organismos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "Organismos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "Organismos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "Organismos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "Organismos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "GetOrganismoByTipoOrganismoId", Controller = "Organismos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "AsignarEstructura", Controller = "Organismos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "GetFraccionByArticulosId", Controller = "Organismos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "AsignarEstructura", Controller = "Organismos", Role = new List<string>() { "Administrador" } });
            #endregion

            #region UnidadesAdministrativas
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "UnidadesAdministrativas", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "UnidadesAdministrativas", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "UnidadesAdministrativas", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "UnidadesAdministrativas", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "UnidadesAdministrativas", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "GetUAByOrganismoId", Controller = "UnidadesAdministrativas", Role = new List<string>() { "Administrador" } }); 
            #endregion

            #region Representantes
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "Representantes", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "Representantes", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "Representantes", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "Representantes", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "Representantes", Role = new List<string>() { "Administrador" } });
            #endregion

            #region GrupoExtensiones
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "GrupoExtensiones", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "GrupoExtensiones", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "GrupoExtensiones", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "GrupoExtensiones", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "GrupoExtensiones", Role = new List<string>() { "Administrador" } });
            #endregion

            #region Extensiones
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "Extensiones", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "Extensiones", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "Extensiones", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "Extensiones", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "Extensiones", Role = new List<string>() { "Administrador" } });
            #endregion

            #region GrupoTags
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "GrupoTags", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "GrupoTags", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "GrupoTags", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "GrupoTags", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "GrupoTags", Role = new List<string>() { "Administrador" } });
            #endregion

            #region Tags
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "Tags", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "Tags", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "Tags", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "Tags", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "Tags", Role = new List<string>() { "Administrador" } });
            #endregion

            #region Periodos
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "Periodos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "Periodos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "Periodos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "Periodos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "Periodos", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "ObtenerTodosLosPeriodos", Controller = "Periodos", Role = new List<string>() { "Administrador" } });
            #endregion

            #region Ciudades
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "Ciudades", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "Ciudades", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "Ciudades", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "Ciudades", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "Ciudades", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "ObtenerTodasLasCiudades", Controller = "Ciudades", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "ObtenerCiudadesDeEstado", Controller = "Ciudades", Role = new List<string>() { "Administrador" } });
            #endregion
            #region Estados
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "Estados", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "Estados", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "Estados", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "Estados", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "Estados", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "ObtenerEstadosDePais", Controller = "Estados", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "ObtenerTodosLosEstados", Controller = "Estados", Role = new List<string>() { "Administrador" } });
            #endregion
            #region Paises
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "Paises", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "Paises", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "Paises", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "Paises", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "Paises", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "ObtenerTodosLosPaises", Controller = "Paises", Role = new List<string>() { "Administrador" } });
            #endregion

            #region Importar Masivo
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "Paises", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "Paises", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "Paises", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "ImportacionMasivo", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "Paises", Role = new List<string>() { "Administrador" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "ObtenerTodosLosPaises", Controller = "Paises", Role = new List<string>() { "Administrador" } });
            #endregion
            #region Otra informacion
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "OtraInformacion", Role = new List<string>() { "Administrador", "Enlace" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "OtraInformacion", Role = new List<string>() { "Administrador", "Enlace" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "OtraInformacion", Role = new List<string>() { "Administrador", "Enlace" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "OtraInformacion", Role = new List<string>() { "Administrador", "Enlace" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "OtraInformacion", Role = new List<string>() { "Administrador", "Enlace" } });
            #endregion
            #region Tipo Otra informacion
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Index", Controller = "TipoOtraInformacion", Role = new List<string>() { "Administrador", "Enlace" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "changeStatus", Controller = "TipoOtraInformacion", Role = new List<string>() { "Administrador", "Enlace" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Details", Controller = "TipoOtraInformacion", Role = new List<string>() { "Administrador", "Enlace" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Create", Controller = "TipoOtraInformacion", Role = new List<string>() { "Administrador", "Enlace" } });
            LstAccionesPermitidas.Add(new VMPermisos() { Accion = "Edit", Controller = "TipoOtraInformacion", Role = new List<string>() { "Administrador", "Enlace" } });
            #endregion
            

            return LstAccionesPermitidas;
        }
        public string ConvertViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
