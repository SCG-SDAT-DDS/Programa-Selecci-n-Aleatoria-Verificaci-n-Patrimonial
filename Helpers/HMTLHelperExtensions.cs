using ApiTransparencia.sqlHandlers;
using Dapper;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Transparencia.FiltersClass;
using Transparencia.Helpers;
using Transparencia.Models;

namespace Transparencia
{

    public static class HMTLHelperExtensions
    {

        private static string coneccion = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        public static  string[] stringFormatDate = new string[] { "d/M/yyyy", "d/M/yyyy hh:mm:ss tt", "dd/MM/yyyy", "dd/MM/yyyy hh:mm:ss tt",
        "M/d/yyyy", "M/d/yyyy hh:mm:ss tt", "MM/dd/yyyy", "MM/dd/yyyy hh:mm:ss tt" };

    public static string IsSelected(this HtmlHelper html, string controller = null, string action = null, string cssClass = null)
        {

            if (String.IsNullOrEmpty(cssClass))
                cssClass = "active";

            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            string currentController = (string)html.ViewContext.RouteData.Values["controller"];

            if (String.IsNullOrEmpty(controller))
                controller = currentController;

            if (String.IsNullOrEmpty(action))
                action = currentAction;

            return controller == currentController && action == currentAction ?
                cssClass : String.Empty;
        }

        public static bool GetRoles(string sCorreoElectronico, string rolesname)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var user = db.Users.Where(r => r.UserName == sCorreoElectronico).FirstOrDefault();
            var Roles = user.Roles.ToList().Select(x => x.RoleId);
            var RolesNames = db.Roles.Where(x => Roles.Contains(x.Id)).ToList();

            return RolesNames != null ? RolesNames.Any(x => x.Name == rolesname) : false;
        }

        public static string GetOrganismoName(string sCorreoElectronico)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var user = db.Users.Where(r => r.UserName == sCorreoElectronico).FirstOrDefault();

            return user?.Organismo?.NombreOrganismo;
        }


        public static bool TienePermiso(string sCorreoElectronico, List<string> rolesname, string modulo = "")
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var user = db.Users.Where(r => r.UserName == sCorreoElectronico  && r.Activo).FirstOrDefault();
            var Roles = user.Roles.ToList().Select(x => x.RoleId);
            if (rolesname == null)
            {
                return false;
            }
            return db.Roles.Where(x => Roles.Contains(x.Id) && rolesname.Contains(x.Name)).Any();


        }

        public static int? GetOrganissmoId(string sUserName, string rolesname)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var user = db.Users.Where(r => r.UserName == sUserName).FirstOrDefault();
            var Roles = user.Roles.ToList().Select(x => x.RoleId);
            var RolesNames = db.Roles.Where(x => Roles.Contains(x.Id)).ToList();
            if (RolesNames.Any(x => x.Name == rolesname))
            {
                return user.OrganismoID;
            }
            return user.OrganismoID;
        }


        public static int? GetUnidadId(string sCorreoElectronico)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var user = db.Users.Where(r => r.UserName == sCorreoElectronico).FirstOrDefault();
            var Roles = user.Roles.ToList().Select(x => x.RoleId);
            var RolesNames = db.Roles.Where(x => Roles.Contains(x.Id)).ToList();
            if (RolesNames.Any(x => x.Id == "8d9f54dc-009b-4cc6-9721-51d10377322f"))
            {
                return user.UnidadAdministrativaId == null || user.UnidadAdministrativaId == 0 ? -1 : user.UnidadAdministrativaId;
            }
            return user.UnidadAdministrativaId;
        }
        public static string PageClass(this HtmlHelper html)
        {
            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            return currentAction;
        }

        public static string EnviarCorreo(string sSubject = "", string sMessageBody = "", string sToAddress = "")
        {
            string sResponse = "";
            try
            {
                string sEmailSubject = sSubject;
                string sCorreo = WebConfigurationManager.AppSettings["EmailUsuario"];
                string sContrasena = WebConfigurationManager.AppSettings["EmailContrasena"];
                var vFromAddress = new MailAddress(sCorreo, "Transparencia Sonora");
                DateTime dtHoy = DateTime.Now;
                var vToAddress = new MailAddress(sToAddress);
                MailMessage msg = new MailMessage();
                msg.From = vFromAddress;
                msg.To.Add(vToAddress);
                msg.Subject = sEmailSubject;
                msg.IsBodyHtml = true;
                msg.Body = sMessageBody;

                using (SmtpClient client = new SmtpClient())
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(sCorreo, sContrasena);
                    client.Host = "smtp.gmail.com";
                    client.Port = 587;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Send(msg);
                }
            }
            catch (Exception ex)
            {
                sResponse = ex.Message;
            }
            return sResponse;
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            if (enumValue == null)
                return "";
            var eV = enumValue.GetType().GetMember(enumValue.ToString())
                            .First();

            if (eV.GetCustomAttribute<DisplayAttribute>() == null)
            {
                return eV.Name;
            }

            return eV.GetCustomAttribute<DisplayAttribute>().GetName();
        }

        public static List<SelectListItem> GetTipoCampoDropDown()
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            // lista.Add(new SelectListItem { Value = "-1", Text = "Seleccione..." , Selected = true});
            var tipos = Enum.GetValues(typeof(TipoCampo));

            foreach (var item in tipos)
            {
                var tipoCampo = (TipoCampo)item;
                var valor = (int)tipoCampo;

                var selectListItem = new SelectListItem();
                selectListItem.Value = valor.ToString();
                selectListItem.Text = tipoCampo.GetDisplayName();
                selectListItem.Selected = false;
                lista.Add(selectListItem);
            }

            return lista;
        }

        public static List<SelectListItem> GetOrdenSeccionPublico()
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            // lista.Add(new SelectListItem { Value = "-1", Text = "Seleccione..." , Selected = true});
            var tipos = Enum.GetValues(typeof(OrdenSeleccionPublico));

            foreach (var item in tipos)
            {
                var tipoCampo = (OrdenSeleccionPublico)item;
                var valor = (int)tipoCampo;

                var selectListItem = new SelectListItem();
                selectListItem.Value = valor.ToString();
                selectListItem.Text = tipoCampo.GetDisplayName();
                selectListItem.Selected = false;
                lista.Add(selectListItem);
            }

            return lista;
        }

        public static List<SelectListItem> GetTipoFrecuencia()
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            // lista.Add(new SelectListItem { Value = "-1", Text = "Seleccione..." , Selected = true});
            var tipos = Enum.GetValues(typeof(FrecuenciaActualizacion));

            foreach (var item in tipos)
            {
                var tipoCampo = (FrecuenciaActualizacion)item;
                var valor = (int)tipoCampo;

                var selectListItem = new SelectListItem();
                selectListItem.Value = valor.ToString();
                selectListItem.Text = tipoCampo.GetDisplayName();
                selectListItem.Selected = false;
                lista.Add(selectListItem);
            }

            return lista;
        }

        public static string GetFrecuencia(this FrecuenciaActualizacion tipoFrecuencia, int frecuencia)
        {
            string sFrecuencia = "";
            List<SelectListItem> lista = new List<SelectListItem>();
            switch (tipoFrecuencia)
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
            return lista.Where(x=>x.Value == frecuencia.ToString()).FirstOrDefault().Text ?? "";
        }

        //public static string GetFrecuencia(FrecuenciaActualizacion tipoFrecuencia, int frecuencia)
        //{
        //    string sFrecuencia = "";
        //    List<SelectListItem> lista = new List<SelectListItem>();
        //    switch (tipoFrecuencia)
        //    {
        //        case FrecuenciaActualizacion.Anual:
        //            lista.Add(new SelectListItem { Value = "1", Text = "Enero - Diciembre" });
        //            break;
        //        case FrecuenciaActualizacion.Bimestral:
        //            lista.Add(new SelectListItem { Value = "1", Text = "Enero - Febrero" });
        //            lista.Add(new SelectListItem { Value = "2", Text = "Marzo - Abril" });
        //            lista.Add(new SelectListItem { Value = "3", Text = "Mayo - Junio" });
        //            lista.Add(new SelectListItem { Value = "4", Text = "Julio - Agosto" });
        //            lista.Add(new SelectListItem { Value = "5", Text = "Septiembre - Octubre" });
        //            lista.Add(new SelectListItem { Value = "6", Text = "Noviembre - Diciembre" });
        //            break;
        //        case FrecuenciaActualizacion.Mensual:
        //            lista.Add(new SelectListItem { Value = "1", Text = "Enero" });
        //            lista.Add(new SelectListItem { Value = "2", Text = "Febrero" });
        //            lista.Add(new SelectListItem { Value = "3", Text = "Marzo" });
        //            lista.Add(new SelectListItem { Value = "4", Text = "Abril" });
        //            lista.Add(new SelectListItem { Value = "5", Text = "Mayo" });
        //            lista.Add(new SelectListItem { Value = "6", Text = "Junio" });
        //            lista.Add(new SelectListItem { Value = "7", Text = "Julio" });
        //            lista.Add(new SelectListItem { Value = "8", Text = "Agosto" });
        //            lista.Add(new SelectListItem { Value = "9", Text = "Septiembre" });
        //            lista.Add(new SelectListItem { Value = "10", Text = "Octubre" });
        //            lista.Add(new SelectListItem { Value = "11", Text = "Noviembre" });
        //            lista.Add(new SelectListItem { Value = "12", Text = "Diciembre" });
        //            break;

        //        case FrecuenciaActualizacion.Semestral:
        //            lista.Add(new SelectListItem { Value = "1", Text = "Enero - Junio" });
        //            lista.Add(new SelectListItem { Value = "2", Text = "Julio -Diciembre" });
        //            break;
        //        case FrecuenciaActualizacion.Trimestral:
        //            lista.Add(new SelectListItem { Value = "1", Text = "Enero - Marzo" });
        //            lista.Add(new SelectListItem { Value = "2", Text = "Abril - Junio" });
        //            lista.Add(new SelectListItem { Value = "3", Text = "Julio - Septiembre" });
        //            lista.Add(new SelectListItem { Value = "4", Text = "Octubre - Diciembre" });
        //            break;
        //    }
        //    return lista.Where(x => x.Value == frecuencia.ToString()).FirstOrDefault().Text ?? "";
        //}

        public static List<SelectListItem> GetTipoFechaDropDown()
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            // lista.Add(new SelectListItem { Value = "-1", Text = "Seleccione..." , Selected = true});
            var tipos = Enum.GetValues(typeof(TipoFecha));

            foreach (var item in tipos)
            {
                var tipoFecha = (TipoFecha)item;
                var valor = (int)tipoFecha;

                var selectListItem = new SelectListItem();
                selectListItem.Value = valor.ToString();
                selectListItem.Text = tipoFecha.GetDisplayName();
                selectListItem.Selected = false;
                lista.Add(selectListItem);
            }

            return lista;
        }
        //validationJquery
        public static string GetMaskJquery(Campo mCampos)
        {

            string Mask = "";
            try
            {
                switch (mCampos.TipoCampo)
                {
                    case TipoCampo.Texto:
                        // /[^a-zA-ZáéíóúàèìòùÀÈÌÒÙÁÉÍÓÚñÑüÜ\s]/g; 
                        //=/^[0-9a-zA-ZáéíóúàèìòùÀÈÌÒÙÁÉÍÓÚñÑüÜ\s]+$/g;
                        Mask = $@"$('#{mCampos.Nombre}').on('input', function(){{
                                      var regexp =/[^a-zA-ZáéíóúàèìòùÀÈÌÒÙÁÉÍÓÚñÑüÜ\s]/g; 
                                      if ($(this).val().match(regexp)){{
                                            $(this).val( $(this).val().replace(regexp, ''));
                                      }}
                                  }}); ";
                        break;
                    case TipoCampo.AreaTexto:

                        break;
                    case TipoCampo.Numerico:
                        break;
                    case TipoCampo.Alfanumerico:
                        //r = new Regex(@"^[a-zA-Z0-9ñÑáÁéÉíÍóÓÓúÚüÜ“”,;=%$\n!¡¿?'""|().\/\/_@.#&+-‐\\.\x20]*$");
                        //^[a-zA-Z0-9ñÑáÁéÉíÍóÓÓúÚüÜ“”,;º’=%$\n!¡¿?'""|().\/\/_@.#&+-‐\\.\x20]*$

                        Mask = $@"$('#{mCampos.Nombre}').on('input', function(){{
                                      var regexp =/[^0-9a-zA-ZáéíóúàèìòùÀÈÌÒÙÁÉÍÓÚñÑüÜ“”#º’=$-‐!\/.,;¡?¿'""()\s]/g;  
                                      if ($(this).val().match(regexp)){{
                                            $(this).val( $(this).val().replace(regexp, ''));
                                      }}
                                  }}); ";
                        break;
                    case TipoCampo.Dinero:
                        Mask = $@"$('#{mCampos.Nombre}').mask('#,##0.00', {{reverse: true}});";
                        break;
                    case TipoCampo.Porcentaje:
                        if (mCampos._ConDecimales == true)
                        {
                            Mask = $@"$('#{mCampos.Nombre}').mask('#,##0.00', {{reverse: true}});";
                        }
                        else
                        {
                            Mask = $@"$('#{mCampos.Nombre}').mask('#,##0', {{reverse: true}});";
                        }
                        break;
                    case TipoCampo.Decimal:
                        Mask = $@"$('#{mCampos.Nombre}').mask('#,##0.00', {{reverse: true}});";
                        break;
                    case TipoCampo.Fecha:
                        break;
                    case TipoCampo.Hora:
                        Mask = $@"$('#{mCampos.Nombre}').timepicki();";
                        break;
                    case TipoCampo.Hipervinculo:
                        break;
                    case TipoCampo.email:
                        break;
                    case TipoCampo.Telefono:
                        Mask = $@"$('#{mCampos.Nombre}').mask('(000)0-00-00-00');";
                        break;
                    case TipoCampo.ArchivoAdjunto:
                        break;
                    case TipoCampo.Catalogo:
                        break;
                    case TipoCampo.CasillaVerificacion:
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {

            }




            return Mask;

        }
        public static string GetMaskJquery(CampoCatalogo mCampos, bool Tabla = false)
        {
            string frm = Tabla ? "frmCrearCatalogo" : "frmCrear";
            string Mask = "";
            //verificamos si es tabla para dar el nombre provicional
            mCampos.Nombre = Tabla ? mCampos.NombreProvicional : mCampos.Nombre;
            try
            {
                switch (mCampos.TipoCampo)
                {
                    case TipoCampo.Texto:
                        // /[^a-zA-ZáéíóúàèìòùÀÈÌÒÙÁÉÍÓÚñÑüÜ\s]/g; 
                        //=/^[0-9a-zA-ZáéíóúàèìòùÀÈÌÒÙÁÉÍÓÚñÑüÜ\s]+$/g;
                        Mask = $@"$('#{frm} input[name={mCampos.Nombre}]').on('input', function(){{
                                      var regexp =/[^0-9a-zA-ZáéíóúàèìòùÀÈÌÒÙÁÉÍÓÚñÑüÜ“”#=$-!\/.,;¡?¿'""()\s]/g; 
                                      if ($(this).val().match(regexp)){{
                                            $(this).val( $(this).val().replace(regexp, ''));
                                      }}
                                  }}); ";
                        break;
                    case TipoCampo.AreaTexto:

                        break;
                    case TipoCampo.Numerico:
                        break;
                    case TipoCampo.Alfanumerico:
                        Mask = $@"$('#{frm} input[name={mCampos.Nombre}]').on('input', function(){{
                                       var regexp =/[^0-9a-zA-ZáéíóúàèìòùÀÈÌÒÙÁÉÍÓÚñÑüÜ“”#º’=$-‐!\/.,;¡?¿'""()\s]/g;  
                                      if ($(this).val().match(regexp)){{
                                            $(this).val( $(this).val().replace(regexp, ''));
                                      }}
                                  }}); ";
                        break;
                    case TipoCampo.Dinero:
                        Mask = $"$('#{frm} input[name={mCampos.Nombre}]').mask('#,##0.00', {{reverse: true}});";
                        break;
                    case TipoCampo.Porcentaje:
                        if (mCampos._ConDecimales == true)
                        {
                            Mask = $"$('#{frm} input[name='{mCampos.Nombre}']').mask('#,##0.00', {{reverse: true}});";
                        }
                        else
                        {
                            Mask = $"$('#{frm} input[name={mCampos.Nombre}]').mask('#,##0', {{reverse: true}});";
                        }
                        break;
                    case TipoCampo.Decimal:
                        Mask = $"$('#{frm} input[name={ mCampos.Nombre}]').mask('#,##0.00', {{reverse: true}});";
                        break;
                    case TipoCampo.Fecha:
                        break;
                    case TipoCampo.Hora:
                        Mask = $"$('#{frm} input[name={mCampos.Nombre}]').timepicki();";
                        break;
                    case TipoCampo.Hipervinculo:
                        break;
                    case TipoCampo.email:
                        break;
                    case TipoCampo.Telefono:
                        Mask = $@"$('#{frm} input[name={mCampos.Nombre}]').mask('(000)0-00-00-00');";
                        break;
                    case TipoCampo.ArchivoAdjunto:
                        break;
                    case TipoCampo.Catalogo:
                        break;
                    case TipoCampo.CasillaVerificacion:
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {

            }




            return Mask;

        }


        public static string GetValidationSecundary(int tipoValidacion = 0)
        {
            string respuesta = "";
            try
            {

                switch (tipoValidacion)
                {
                    //Rules
                    case 1:
                        //sysFrecuencia
                        respuesta += ",sysFrecuencia:{notEqual: '0'}";
                        respuesta += ",sysNumFrecuencia:{notEqual: '0'}";
                        respuesta += ",PeriodoId:{notEqual: '0'}";

                        break;
                    //Mehtodos
                    case 2:
                        //sysFrecuencia
                        respuesta += $@"jQuery.validator.addMethod('notEqual', function(value, element, param) {{
                                              return this.optional(element) || value != param;
                                           }}, 'Este campo es obligatorio.');";
                        break;
                }



            }
            catch (Exception ex)
            {

            }

            return respuesta;
        }
        //Mask validation
        public static string GetValidationJquery(List<Campo> mCampos)
        {

            string Validation = "";
            string ValidationCatalogo = "";
            try
            {

                var first = true;

                var sRules = "";
                var sMessage = "";
                var sMask = "";
                var sMethods = "";
                var sTabla = "";


                foreach (var item in mCampos)
                {
                    sMask += GetMaskJquery(item);
                    if (!first)
                    {
                        sRules += ",";
                        sMessage += ",";
                    }

                    sRules += item.Nombre + ":{";
                    sMessage += item.Nombre + ":{";

                    var requiereFirst = true;

                    //hacemos los script para tabla
                    if (item.TipoCampo == TipoCampo.Catalogo)
                    {
                        if (GetTipoTabla(item.CatalogoId))
                        {
                            //nombres
                            string tableName = $"Table{item.CatalogoId}";
                            string deleteField = $"deleteField{ item.CatalogoId}";
                            string selectAll = $"selectAll{ item.CatalogoId}";

                            //methodo para desabilitar botoon eliminar
                            sTabla += $@" $('#{selectAll}').change(function () {{
                                            var $remove = $('#{deleteField}');
                                            var $checkboxes = $('#{tableName}').find('td').find(':checkbox');
                                            var $selectAll = $('#{selectAll}');
                                           
                                            $remove.prop('disabled', !$selectAll.is(':checked'));
                                            $checkboxes.prop('checked', $selectAll.is(':checked'));
                                        }});";
                            //checkbox change
                            sTabla += $@"$( '#{tableName}' ).on( 'click', 'input[type=checkbox]', function() {{
                                               var totalChecked = $('#{tableName} input[type=checkbox]:checked').length;
                                               var $remove = $('#{deleteField}');
                                               if(totalChecked > 0){{
                                                    $remove.prop('disabled', false);
                                               }}else{{
                                                    $remove.prop('disabled', true);
                                               }}
                                       }}); ";

                            sTabla += $@" $('#{deleteField}').click(function () {{
                                             $('#{tableName} input[type=checkbox]:checked:not(\'#selectAll{item.CatalogoId}\')').closest('tr').remove();
                                             $('#{tableName}').trigger('update');
                                             var $selectAll = $('#{selectAll}');
                                             $selectAll.prop('checked', false);
                                         }});";
                            //Saber si tiene o no registros para aponer la leyenda
                            sTabla += $@"$('#{tableName}').on('update', function() {{
                                        var rowCount = $('#{tableName} tbody tr.trFormTable').length;
                                       
                                        if(rowCount > 0){{
                                            $('#{tableName} tbody tr.withoutRowsLabel').remove();
                                            $('#{item.Nombre}').val(rowCount);
                                        }}else if(rowCount == 0){{
                                            var totalColums = $('#{tableName} thead th').length;
                                            $('#{tableName} tbody tr.withoutRowsLabel').remove();
                                            $('#{tableName} > tbody:last-child').append(`
                                            <tr class='withoutRowsLabel'>
                                              <th colspan='${{totalColums}}'  class='text-center text-danger'>Sin registros.</th>
                                            </tr>`);
                                            $('#{item.Nombre}').val(0);
                                        }}
                        }}); ";
                            sTabla += GetFunctionNewTabla(item.CatalogoId);
                        }
                    }

                    if (item.Requerido && item.TipoCampo != TipoCampo.CasillaVerificacion)
                    {
                        if (item.TipoCampo == TipoCampo.Catalogo)
                        {
                            sRules += "notEqual: '0'";
                            //sMessage += "notEqual: 'Este campo es obligatorio.'";
                            sMethods += $@"jQuery.validator.addMethod('notEqual', function(value, element, param) {{
                                              return this.optional(element) || value != param;
                                           }}, 'Este campo es obligatorio.');";
                        }
                        else
                        {
                            sRules += "required: true";
                            sMessage += "required: 'Este campo es obligatorio.'";
                        }

                        //sRules += "required: true";
                        //sMessage += "required: 'Este campo es obligatorio.'";
                        requiereFirst = false;
                    }
                    if (item.Longitud > 0)
                    {
                        if (!requiereFirst)
                        {
                            sRules += ",";
                            if (item.TipoCampo != TipoCampo.Catalogo)
                            {
                                sMessage += ",";
                            }

                        }
                        var longiutd = item.TipoCampo == TipoCampo.Porcentaje ? item.Longitud + 1 : item.Longitud;
                        sRules += "maxlength: " + longiutd;
                        sMessage += "maxlength: 'Por favor no introduzca mas de " + item.Longitud + " caracteres.'";
                        requiereFirst = false;
                    }
                    if (item.TipoCampo == TipoCampo.email)
                    {
                        if (!requiereFirst)
                        {
                            sRules += ",";
                            sMessage += ",";
                        }
                        sRules += "email: true";
                        sMessage += "email: 'Este no es un correo electronico valido.'";
                        requiereFirst = false;
                    }
                    //if (item.TipoCampo == TipoCampo.email)
                    //{
                    //    if (!requiereFirst)
                    //    {
                    //        sRules += ",";
                    //        sMessage += ",";
                    //    }
                    //    sRules += "email: true";
                    //    sMessage += "email: 'Este no es un correo electrónico valido.'";
                    //    requiereFirst = false;
                    //}
                    if (item.TipoCampo == TipoCampo.Numerico)
                    {
                        if (!requiereFirst)
                        {
                            sRules += ",";
                            sMessage += ",";
                        }
                        sRules += "number: true";
                        sMessage += "number: 'Este no es un numero valido.'";
                        requiereFirst = false;
                    }

                    if (item.TipoCampo == TipoCampo.Hipervinculo)
                    {
                        if (!requiereFirst)
                        {
                            sRules += ",";
                            sMessage += ",";
                        }
                        //sRules += "url: true";
                        //sMessage += "url: 'Este no es un Hipervínculo correcto.'";
                        var number = new Random();
                        var nombre = $"notUrl{number.Next(122384, 543987)}";
                        sRules += $"{nombre}: {{ {nombre}: true }}";
                        //sMessage += "notEqual: 'Este campo es obligatorio.'";
                        sMethods += $@"jQuery.validator.addMethod('{nombre}', function(value, element) {{
                                                //var pattern = new RegExp('https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{{2,}}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{{2,}}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{{2,}}|www\.[a-zA-Z0-9]+\.[^\s]{{2,}}');
                                             

                                                if(value.length <= 0){{
                                                    return true;
                                                }}
                                              var pattern = new RegExp('(http|https):\/\/');
                                             // return this.optional(element) || 
                                              return !!pattern.test(value);
                                           }}, 'Este no es un Hipervínculo correcto.');";
                        requiereFirst = false;
                    }




                    first = false;

                    sRules += "}";
                    sMessage += "}";
                }
                //agregamos los campos
                sRules += GetValidationSecundary(1);
                sMethods += GetValidationSecundary(2);
                Validation = "$('#frmCrear').validate({ ignore: '',rules:{" + sRules + "},messages:{ " + sMessage + " } }); " + sMask + "  " + sMethods + " " + sTabla;


            }
            catch (Exception ex)
            {

            }




            return Validation;

        }
        public static string GetValidationJquery(List<CampoCatalogo> mCampos, bool Tabla = false, bool createTabla = false, int catalogoId = 0)
        {

            string Validation = "";
            try
            {

                var first = true;

                var sRules = "";
                var sMessage = "";
                var sMask = "";
                var sMethods = "";
                var sTabla = "";

                foreach (var item in mCampos)
                {
                    //verificamos si es para tabla y ponemos el nombre provicional si es necesario
                    item.Nombre = Tabla ? item.NombreProvicional : item.Nombre;

                    sMask += GetMaskJquery(item, Tabla);
                    if (!first)
                    {
                        sRules += ",";
                        sMessage += ",";
                    }

                    sRules += item.Nombre + ":{";
                    sMessage += item.Nombre + ":{";

                    var requiereFirst = true;



                    if (item.Requerido && item.TipoCampo != TipoCampo.CasillaVerificacion)
                    {
                        if (item.TipoCampo == TipoCampo.Catalogo)
                        {
                            sRules += "notEqual: '0'";
                            //sMessage += "notEqual: 'Este campo es obligatorio.'";
                            sMethods += $@"jQuery.validator.addMethod('notEqual', function(value, element, param) {{
                                              return this.optional(element) || value != param;
                                           }}, 'Este campo es obligatorio.');";
                        }
                        else
                        {
                            sRules += "required: true";
                            sMessage += "required: 'Este campo es obligatorio.'";
                        }

                        //sRules += "required: true";
                        //sMessage += "required: 'Este campo es obligatorio.'";
                        requiereFirst = false;
                    }
                    if (item.Longitud > 0)
                    {
                        if (!requiereFirst)
                        {
                            sRules += ",";
                            sMessage += ",";
                        }
                        var longiutd = item.TipoCampo == TipoCampo.Porcentaje ? item.Longitud + 1 : item.Longitud;
                        sRules += "maxlength: " + longiutd;
                        sMessage += "maxlength: 'Por favor no introduzca mas de " + item.Longitud + " caracteres.'";
                        requiereFirst = false;
                    }
                    if (item.TipoCampo == TipoCampo.email)
                    {
                        if (!requiereFirst)
                        {
                            sRules += ",";
                            sMessage += ",";
                        }
                        sRules += "email: true";
                        sMessage += "email: 'Este no es un correo electronico valido.'";
                        requiereFirst = false;
                    }
                    if (item.TipoCampo == TipoCampo.email)
                    {
                        if (!requiereFirst)
                        {
                            sRules += ",";
                            sMessage += ",";
                        }
                        sRules += "email: true";
                        sMessage += "email: 'Este no es un correo electrónico valido.'";
                        requiereFirst = false;
                    }
                    if (item.TipoCampo == TipoCampo.Numerico)
                    {
                        if (!requiereFirst)
                        {
                            sRules += ",";
                            sMessage += ",";
                        }
                        sRules += "number: true";
                        sMessage += "number: 'Este no es un numero valido.'";
                        requiereFirst = false;
                    }

                    if (item.TipoCampo == TipoCampo.Hipervinculo)
                    {
                        if (!requiereFirst)
                        {
                            sRules += ",";
                            sMessage += ",";
                        }
                        sRules += "url: true";
                        sMessage += "url: 'Este no es un Hipervínculo correcto.'";
                        requiereFirst = false;
                    }
                    first = false;

                    sRules += "}";
                    sMessage += "}";
                }
                string frm = Tabla ? "frmCrearCatalogo" : "frmCrear";
                Validation = "$('#" + frm + "').validate({ ignore: '',rules:{" + sRules + "},messages:{ " + sMessage + " } }); " + sMask + "  " + sMethods + " " + sTabla;
            }
            catch (Exception ex)
            {

            }




            return Validation;

        }

        //Getcampos
        public static string GetCampo(Campo mCampo)
        {
            string campo = "";
            //PopOver
            string icono = "fa fa-question-circle";
            string popoverName = "popover" + mCampo.CampoId.ToString();
            string poppover = " <i class='" + icono + "' ></i>";
            string maxLenght = mCampo.TipoCampo != TipoCampo.Fecha && mCampo.TipoCampo != TipoCampo.Hora ? $"maxlength='{mCampo.Longitud }'" : "";
            string cssFormatoFecha = mCampo.TipoCampo == TipoCampo.Fecha ? "datefield" : "";
            string ReadyOnly = mCampo.TipoCampo == TipoCampo.Fecha || mCampo.TipoCampo == TipoCampo.Hora ? "readonly" : "";
            string sPlaceholder = mCampo.TipoCampo == TipoCampo.Fecha || mCampo.TipoCampo == TipoCampo.Hora ? "Seleccione..." : mCampo.Etiqueta;
            campo = $@" <div for='{mCampo.Nombre}' class='row col-lg-12 mb-2'>
                        <div class='col-md-6'> { mCampo?.Etiqueta} </div>
                        <div class='col-md-6 text-right'><a class='text-info' data-toggle='popover' data-placement='top' data-content='{ mCampo.Ayuda }' aria-describedby='{ popoverName}'> Información {poppover}</a></div>
                      </div>";

            //Campos
            switch (mCampo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.Alfanumerico:
                    campo += $@"
                            <div class='input-group mb-3'>
                                    <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Dinero:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>$</span>
                                    </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                  <div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Porcentaje:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>%</span>
                                    </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >";
                    if (mCampo._ConDecimales == true)
                    {
                        campo += $@"<div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>";
                    }
                    campo += "</div>";
                    break;
                case TipoCampo.Decimal:
                    campo += $@"<div class='input-group mb-3'>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                  <div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Fecha:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'><i class='fa fa-calendar'></i></span>
                                  </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Hora:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>Hora</span>
                                  </div>
                                    <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Hipervinculo:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-link'></i></span>
                                      </div>
                                    <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                    </div>";
                    break;
                case TipoCampo.Telefono:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-phone'></i></span>
                                      </div>
                                    <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.AreaTexto:
                    campo += $@"
                            <div class='input-group mb-3'>
                               <textarea class='form-control'  id='{mCampo.Nombre}'  name='{mCampo.Nombre}' rows='5' {maxLenght}></textarea>
                            </div>";
                    break;
                case TipoCampo.Numerico:
                    campo += $@"
                                <div class='input-group mb-3'>
                                    <input type='number' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' placeholder='0' {maxLenght}>
                                    </div>";
                    break;
                case TipoCampo.email:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-envelope'></i></span>
                                      </div>
                                    <input type='email' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' placeholder='{sPlaceholder}'>
                                </div>";
                    break;

                case TipoCampo.ArchivoAdjunto:
                    var extensiones = GetAcceptFileTypeFile(mCampo._GrupoExtensionId);
                    campo += $@"
                                <div class=""input-group mb-3"">
                                  <div class=""input-group-prepend"">
                                    <span class=""input-group-text"" id=""inputGroupFileAddon01""><i class='fa fa-file'></i></span>
                                  </div>
                                  <div class=""custom-file"">
                                    <input type=""file"" class=""custom-file-input"" id='{mCampo.Nombre}'  name='{mCampo.Nombre}' accept='{extensiones}' aria-describedby='{mCampo.Nombre}'>
                                    <label class=""custom-file-label"" for=""inputGroupFile01"">Seleccione el archivo</label>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Catalogo:

                    if (GetTipoTabla(mCampo.CatalogoId))
                    {
                        //conseguir los campos deeste catalogo

                        string modalName = $"Modal{mCampo.CatalogoId}";
                        string buttonNewField = $"NewField{mCampo.CatalogoId}"; //creo que no lo estamos usando
                        string buttonDeleteField = $"deleteField{mCampo.CatalogoId}";
                        string nombreCatalogo = GetNombreCatalogo(mCampo.CatalogoId);
                        campo += $@"<div class='input-group'>
                                    <div class='card col-lg-12'>
                                      <div class='card-body'>
                                        <a class='text-{ (mCampo.Requerido ? "danger" : "info") } ModalCatalogo' data-toggle='modal' data-target='#{modalName}'> Capturar los valores. </a>
                                      </div>
                                    </div>


                                    <div id='{modalName}' class='modal fade bd-example-modal-lg' role='dialog' aria-labelledby='Información del catalogo'>
                                      <div class='modal-dialog modal-lg'>
                                        <div class='modal-content'>
                                            <div class='modal-header'>
                                                    <h4 class='modal-title' id='{modalName}Title'>Información de {mCampo.Etiqueta}</h4>
                                                    <button type='button' class='close' data-dismiss='modal' aria-label='Close'>
                                                      <span aria-hidden='true'>×</span>
                                                    </button>
                                            </div>
                                            <div class='modal-body'>

                                                    <div class='form-group col-md-12'>
                                                        <div class='alert alert-warning' role='alert'>
                                                            <h4><i class='fa fa-info-circle'></i> Capture, borra o modifique la información necesaria para {mCampo.Etiqueta}.</h4>
                                                        </div>
                                                    </div>";

                        campo += $@"<div class='col-ms-12 mt-1 mb-3'>
                                                            <button type='button' id='{buttonNewField}' class='btn btn-success ButtonNewCatalogo' data-catalogo='{mCampo.CatalogoId}' data-catalago-name='{nombreCatalogo}'><i class='fa fa-plus'></i> Nuevo</button>
                                                            <button type='button' id='{buttonDeleteField}' class='btn btn-danger' disabled=''><i class='fa fa-remove'></i> Borrar</button>
                                                        </div>";

                        //Generamos la tabla de información
                        campo += getTabla(mCampo.CatalogoId);


                        campo += $@"      </div>
                                        </div>
                                      </div>
                                    </div>
                                    <input type='hidden' id='{mCampo.Nombre}' name='{mCampo.Nombre}' value='0' />
                                </div>";
                    }
                    else
                    {
                        campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-clipboard'></i></i></span>
                                      </div>
                                    <select id='{mCampo?.Nombre}' name='{ mCampo.Nombre }' class='form-control'>
                                        <option value='0' selected>Seleccione...</option>
                                        { GetListCatalogo(mCampo.CatalogoId)}
                                    </select>
                                </div>";
                    }

                    break;
                case TipoCampo.CasillaVerificacion:
                    campo += $@"
                                <div class='input-group mb-3'>
                                    <div class='i-checks'>
                                        <input type = 'checkbox' id='{ mCampo.Nombre }'  name='{ mCampo.Nombre }' value = '1' /> 
                                            <label class='form-check-label' for='{ mCampo.Nombre }'> { mCampo?.Etiqueta }</label>
                                    </div>
                                </div>";
                    break;
                default:
                    break;
            }

            campo += $@"<label for='{ mCampo.Nombre }' generated='true' class='error'></label><div class='input-group mb-0'></div>";

            return campo;

        }
        #region NuevosCambios
        public static string getTabla(int CatalogoId = 0)
        {
            string respuesta = "";
            try
            {
                //nombres 
                string tableName = $"Table{CatalogoId}";

                ApplicationDbContext db = new ApplicationDbContext();
                List<string> campoCatalogos = db.CampoCatalogo.Where(x => x.CatalogoId == CatalogoId && x.Activo).Select(x => x.Etiqueta).ToList();
                if (campoCatalogos != null && campoCatalogos.Count > 0)
                {
                    //table-responsive
                    respuesta += $@"<table class='table table-bordered FromTable' id='{tableName}'>
                                         <thead><tr>";
                    respuesta += $@"<th scope='col' style='text-align: center; vertical-align: middle; width: 36px;'>
                                        <label>
                                                <input data-index='0' id='selectAll{CatalogoId}' name='selectAll{CatalogoId}' type='checkbox' value='0'>
                                                <span></span>
                                        </label>
                                   </th>";
                    //agregamos uno por el checkbox
                    int totalColum = 2;
                    foreach (var item in campoCatalogos)
                    {
                        totalColum++;
                        respuesta += $@"<th scope='col'>{item}</th>";
                    }
                    respuesta += $@"<th scope='col'>Acción</th>";
                    respuesta += $@" </thead></tr> 
                                          <tbody>
                                            <tr class='withoutRowsLabel'>
                                              <th colspan='{totalColum}'  class='text-center text-danger'>Sin registros.</th>
                                            </tr>
                                         </tbody>
                                   </table>";
                }
            }
            catch (Exception ex)
            {

            }
            return respuesta;
        }

        public static string GetFunctionNewTabla(int CatalogoId = 0)
        {
            string respuesta = "";
            try
            {
                //nombres 
                string tableName = $"Table{CatalogoId}";
                string functionName = $"function_{CatalogoId}";

                ApplicationDbContext db = new ApplicationDbContext();
                List<string> campoCatalogos = db.CampoCatalogo.Where(x => x.CatalogoId == CatalogoId && x.Activo).Select(x => x.Nombre).ToList();
                if (campoCatalogos != null && campoCatalogos.Count > 0)
                {
                    //primeros hacemos el Array para saber cuantos campos son
                    respuesta += $@"{functionName} = function(){{
                                     var arr = [";
                    var contadorArr = 0;
                    foreach (var item in campoCatalogos)
                    {
                        if (contadorArr > 0)
                            respuesta += ",";
                        respuesta += $"'field{CatalogoId}_{item}'";
                        contadorArr++;
                    }
                    respuesta += "];";

                    //colocamoos el each para ponerlo en la tabla
                    respuesta += $"var rowCount = $('#{tableName} tr').length;";
                    respuesta += "rowCount--;";
                    respuesta += $"var trName=`Tr{CatalogoId}${{rowCount}}`;";

                    //chexkbox
                    respuesta += $@"var htmlAppend = `<tr id='${{trName}}' class='trFormTable'><td scope='col' style='text-align: center; vertical-align: middle; width: 36px;'>
                                        <label>
                                                <input data-index='0' type='checkbox' value='0'>
                                                <span></span>
                                        </label>
                                   </td>`;";

                    //respuesta += $@"$('#{tableName} > tbody:last-child').append({checkbox});";
                    respuesta += $@"jQuery.each(arr, function(i,val) {{
                                        var value = $('#'+val).val();
                                        htmlAppend += `<td data-name='${{val}}' data-value='${{value}}' class='validField'>${{value}}</td>`;
                                    }});";
                    respuesta += $@"htmlAppend += `</td>`;";
                    respuesta += $@"htmlAppend += `<td class='text-center'>
                                    <a class='btn-sm btn-primary text-white bg-warning' onclick='btnEditar(this)' data-catalogo='{CatalogoId}' title='Editar'> 
                                            <i class='fa fa-pencil-square-o' aria-hidden='true'></i>  
                                        </a> 
                                </td></tr>`;";
                    respuesta += $@"$('#{tableName} > tbody:last-child').append(htmlAppend);";
                    //cerramos la function
                    respuesta += $@"}};";
                }
            }
            catch (Exception ex)
            {

            }
            return respuesta;
        }



        #endregion
        public static string GetCampo(CampoCatalogo mCampo, bool Tabla = false)
        { 
            string campo = "";
            //PopOver
            string icono = "fa fa-question-circle";
            string popoverName = "popover" + mCampo.CampoCatalogoId.ToString();
            string poppover = " <i class='" + icono + "' ></i>";
            string maxLenght = mCampo.TipoCampo != TipoCampo.Fecha && mCampo.TipoCampo != TipoCampo.Hora ? $"maxlength='{mCampo.Longitud }'" : "";
            string cssFormatoFecha = mCampo.TipoCampo == TipoCampo.Fecha ? "datefield" : "";
            string ReadyOnly = mCampo.TipoCampo == TipoCampo.Fecha || mCampo.TipoCampo == TipoCampo.Hora ? "readonly" : "";
            string sPlaceholder = mCampo.TipoCampo == TipoCampo.Fecha || mCampo.TipoCampo == TipoCampo.Hora ? "Seleccione..." : mCampo.Etiqueta;
            campo = $@" <div for='{mCampo.Nombre}' class='row col-lg-12 mb-2'>
                        <div class='col-md-6'> { mCampo?.Etiqueta} </div>
                        <div class='col-md-6 text-right'><a class='text-info' data-toggle='popover' data-placement='top' data-content='{ mCampo.Ayuda }' aria-describedby='{ popoverName}'> Información {poppover}</a></div>
                      </div>";
            string sNombre = Tabla ? mCampo.NombreProvicional : mCampo.Nombre;

            //Campos
            switch (mCampo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.Alfanumerico:
                    campo += $@"
                            <div class='input-group mb-3'>
                                    <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Dinero:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>$</span>
                                    </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                  <div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Porcentaje:
                    var opcionDecimales = mCampo._ConDecimales;

                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>%</span>
                                    </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >";
                    if (mCampo._ConDecimales == true)
                    {
                        campo += $@"<div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>";
                    }
                    campo += "</div>";
                    break;
                case TipoCampo.Decimal:
                    campo += $@"<div class='input-group mb-3'>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                  <div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Fecha:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'><i class='fa fa-calendar'></i></span>
                                  </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Hora:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>Hora</span>
                                  </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Hipervinculo:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-link'></i></span>
                                      </div>
                                    <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                    </div>";
                    break;
                case TipoCampo.Telefono:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-phone'></i></span>
                                      </div>
                                    <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.AreaTexto:
                    campo += $@"
                            <div class='input-group mb-3'>
                               <textarea class='form-control'  id='{sNombre}'  name='{sNombre}' rows='5' {maxLenght}></textarea>
                            </div>";
                    break;
                case TipoCampo.Numerico:
                    campo += $@"
                                <div class='input-group mb-3'>
                                    <input type='number' class='form-control' id='{sNombre}'  name='{sNombre}' placeholder='0' {maxLenght}>
                                </div>";
                    break;
                case TipoCampo.email:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-envelope'></i></span>
                                      </div>
                                    <input type='email' class='form-control' id='{sNombre}'  name='{sNombre}' placeholder='{sPlaceholder}'>
                                </div>";
                    break;

                case TipoCampo.ArchivoAdjunto:
                    var extensiones = GetAcceptFileTypeFile(mCampo._GrupoExtensionId);
                    campo += $@"
                                <div class=""input-group mb-3"">
                                  <div class=""input-group-prepend"">
                                    <span class=""input-group-text"" id=""inputGroupFileAddon01""><i class='fa fa-file'></i></span>
                                  </div>
                                  <div class=""custom-file"">
                                    <input type=""file"" class=""custom-file-input"" id='{sNombre}'  name='{sNombre}' accept='{extensiones}' aria-describedby='{sNombre}'>
                                    <label class=""custom-file-label"" for='{mCampo.Nombre}'>Seleccione el archivo</label>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Catalogo:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-clipboard'></i></i></span>
                                      </div>
                                    <select id='{sNombre}' name='{ sNombre }' class='form-control'>
                                        <option value='0' selected>Seleccione...</option>
                                        {GetListCatalogo(mCampo.iCatalogoId)}
                                    </select>
                                </div>";
                    break;
                case TipoCampo.CasillaVerificacion:
                    campo += $@"
                                <div class='input-group mb-3'>
                                    <div class='i-checks'>
                                        <input type = 'checkbox' id='{ sNombre }'  name='{ sNombre }' value = '1' /> 
                                            <label class='form-check-label' for='{ sNombre }'> { mCampo?.Etiqueta }</label>
                                    </div>
                                </div>";
                    break;
                default:
                    break;
            }

            campo += $@"<label for='{ sNombre }' generated='true' class='error'></label><div class='input-group mb-0'></div>";

            return campo;

        }


        public static string GetIconsInitialDiv(TipoCampo eTipoCampo)
        {
            string respuesta = "";
            try
            {
                switch (eTipoCampo)
                {

                    case TipoCampo.Dinero:
                        respuesta = $@"<div class='input-group-prepend'>
                                            <span class='input-group-text'>$</span>
                                        </div>";
                        break;
                    case TipoCampo.Porcentaje:
                        respuesta = $@"<div class='input-group-prepend'>
                                            <span class='input-group-text'>%</span>
                                        </div>";
                        break;
                    case TipoCampo.Fecha:
                        respuesta = $@"<div class='input-group-prepend'>
                                            <span class='input-group-text'><i class='fa fa-calendar'></i></span>
                                        </div>";
                        break;
                    case TipoCampo.Hora:
                        respuesta = $@"<div class='input-group-prepend'>
                                            <span class='input-group-text'>Hora</span>
                                        </div>";
                        break;
                    case TipoCampo.Hipervinculo:
                        respuesta = $@"<div class='input-group-prepend'>
                                            <span class='input-group-text'><i class='fa fa-link'></i></span>
                                        </div>";
                        break;
                    case TipoCampo.Telefono:
                        respuesta = $@"<div class='input-group-prepend'>
                                            <span class='input-group-text'><i class='fa fa-phone'></i></span>
                                        </div>";
                        break;
                    case TipoCampo.email:
                        respuesta = $@"<div class='input-group-prepend'>
                                            <span class='input-group-text'><i class='fa fa-envelope'></i></span>
                                        </div>";
                        break;
                    case TipoCampo.Catalogo:
                    case TipoCampo.CasillaVerificacion:
                        respuesta = $@"<div class='input-group-prepend'>
                                            <span class='input-group-text'><i class='fa fa-clipboard'></i></span>
                                        </div>";
                        break;
                    default:
                        break;
                }


            }
            catch (Exception ex)
            {

            }

            return respuesta;
        }
        public static string GetIconsFinallDiv(TipoCampo eTipoCampo, bool? bconDecimales = false)
        {
            string respuesta = "";
            try
            {
                switch (eTipoCampo)
                {
                    case TipoCampo.Dinero:
                        respuesta += $@"<div class='input-group-append'>
                                            <span class='input-group-text'>.00</span>
                                        </div>";
                        break;
                    case TipoCampo.Porcentaje:
                        if (bconDecimales == true)
                        {
                            respuesta += $@"<div class='input-group-append'>
                                            <span class='input-group-text'>.00</span>
                                        </div>";
                        }
                        break;
                    case TipoCampo.Decimal:
                        respuesta += $@"<div class='input-group-append'>
                                            <span class='input-group-text'>.00</span>
                                        </div>";
                        break;
                    default:
                        break;
                }


            }
            catch (Exception ex)
            {

            }

            return respuesta;
        }

        public static string GetFilters(Campo mCampo)
        {
            string campo = "";
            //PopOver
            string icono = "fa fa-question-circle";
            string popoverName = "popover" + mCampo.CampoId.ToString();
            string poppover = " <i class='" + icono + "' ></i>";
            string maxLenght = mCampo.TipoCampo != TipoCampo.Fecha && mCampo.TipoCampo != TipoCampo.Hora ? $"maxlength='{mCampo.Longitud }'" : "";
            string cssFormatoFecha = mCampo.TipoCampo == TipoCampo.Fecha ? "datefield" : "";
            string ReadyOnly = mCampo.TipoCampo == TipoCampo.Fecha || mCampo.TipoCampo == TipoCampo.Hora ? "readonly" : "";
            string sPlaceholder = mCampo.TipoCampo == TipoCampo.Fecha || mCampo.TipoCampo == TipoCampo.Hora ? "Seleccione..." : mCampo.Etiqueta;
            //campo = $@" <div for='{mCampo.Nombre}' class='row col-lg-12 mb-2'>
            //            <div class='col-md-6'> { mCampo?.Etiqueta} </div>
            //            <div class='col-md-6 text-right'><a class='text-info' data-toggle='popover' data-placement='top' data-content='{ mCampo.Ayuda }' aria-describedby='{ popoverName}'> Información {poppover}</a></div>
            //          </div>";




            //<div class='input-group-prepend'>
            //                          <div class='input-group-text'></div>
            //                        </div>
            var typeInput = "text";
            if (mCampo.TipoCampo == TipoCampo.Numerico)
            {
                typeInput = "number";

            }
            else if (mCampo.TipoCampo == TipoCampo.email)
            {
                typeInput = "email";
            }
            //Campos
            switch (mCampo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.Alfanumerico:
                case TipoCampo.Dinero:
                case TipoCampo.Porcentaje:
                case TipoCampo.Decimal:
                case TipoCampo.Fecha:
                case TipoCampo.Hora:
                case TipoCampo.Telefono:
                case TipoCampo.AreaTexto:
                case TipoCampo.Numerico:
                case TipoCampo.email:
                case TipoCampo.Hipervinculo:
                    campo = $@"  <div class='col-auto col-3'>
                                    <label for='{mCampo?.Nombre}'> { mCampo?.Etiqueta}</label>
                                  <div class='input-group mb-4'>
                                    {GetIconsInitialDiv(mCampo.TipoCampo)}
                                    <input type='{typeInput}' class='form-control  {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly}>
                                    {GetIconsFinallDiv(mCampo.TipoCampo, mCampo._ConDecimales)}
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Catalogo:
                    if (!GetTipoTabla(mCampo.CatalogoId))
                    {
                        campo = $@"<div class='col-auto col-3'>
                                  <label for='{mCampo?.Nombre}' class='mr-sm-4'> { mCampo?.Etiqueta}</label>
                                  <select id='{mCampo?.Nombre}' name='{ mCampo.Nombre }' class='form-control'>
                                    <option value='' selected>Seleccione...</option>
                                   {GetListCatalogo(mCampo.CatalogoId)}
                                  </select>
                                </div>";
                    }

                    break;
                case TipoCampo.CasillaVerificacion:
                    campo = $@"<div class='col-auto col-3'>
                                  <label for='{mCampo?.Nombre}' class='mr-sm-4'> { mCampo?.Etiqueta}</label>
                                  <select id='{mCampo?.Nombre}' name='{ mCampo.Nombre }' class='form-control'>
                                    <option value=''  selected>Seleccione...</option>
                                    <option value='1' >Activo</option>
                                    <option value='0' >Inactivo</option>
                                  </select>
                                </div>";
                    break;
                default:
                    break;
            }

            //campo += $@"<label for='{ mCampo.Nombre }' generated='true' class='error'></label><div class='input-group mb-0'></div>";

            return campo;

        }
        public static string GetFilters(CampoCatalogo mCampo)
        {
            string campo = "";
            //PopOver
            string icono = "fa fa-question-circle";
            string popoverName = "popover" + mCampo.CampoCatalogoId.ToString();
            string poppover = " <i class='" + icono + "' ></i>";
            string maxLenght = mCampo.TipoCampo != TipoCampo.Fecha && mCampo.TipoCampo != TipoCampo.Hora ? $"maxlength='{mCampo.Longitud }'" : "";
            string cssFormatoFecha = mCampo.TipoCampo == TipoCampo.Fecha ? "datefield" : "";
            string ReadyOnly = mCampo.TipoCampo == TipoCampo.Fecha || mCampo.TipoCampo == TipoCampo.Hora ? "readonly" : "";
            string sPlaceholder = mCampo.TipoCampo == TipoCampo.Fecha || mCampo.TipoCampo == TipoCampo.Hora ? "Seleccione..." : mCampo.Etiqueta;
            //campo = $@" <div for='{mCampo.Nombre}' class='row col-lg-12 mb-2'>
            //            <div class='col-md-6'> { mCampo?.Etiqueta} </div>
            //            <div class='col-md-6 text-right'><a class='text-info' data-toggle='popover' data-placement='top' data-content='{ mCampo.Ayuda }' aria-describedby='{ popoverName}'> Información {poppover}</a></div>
            //          </div>";




            //<div class='input-group-prepend'>
            //                          <div class='input-group-text'></div>
            //                        </div>
            var typeInput = "text";
            if (mCampo.TipoCampo == TipoCampo.Numerico)
            {
                typeInput = "number";

            }
            else if (mCampo.TipoCampo == TipoCampo.email)
            {
                typeInput = "email";
            }
            //Campos
            switch (mCampo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.Alfanumerico:
                case TipoCampo.Dinero:
                case TipoCampo.Porcentaje:
                case TipoCampo.Decimal:
                case TipoCampo.Fecha:
                case TipoCampo.Hora:
                case TipoCampo.Telefono:
                case TipoCampo.AreaTexto:
                case TipoCampo.Numerico:
                case TipoCampo.email:
                case TipoCampo.Hipervinculo:
                    campo = $@"  <div class='col-auto col-3'>
                                    <label for='{mCampo?.Nombre}'> { mCampo?.Etiqueta}</label>
                                  <div class='input-group mb-4'>
                                    {GetIconsInitialDiv(mCampo.TipoCampo)}
                                    <input type='{typeInput}' class='form-control  {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly}>
                                    {GetIconsFinallDiv(mCampo.TipoCampo, mCampo._ConDecimales)}
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Catalogo:

                    campo = $@"<div class='col-auto col-3'>
                                  <label for='{mCampo?.Nombre}' class='mr-sm-4'> { mCampo?.Etiqueta}</label>
                                  <select id='{mCampo?.Nombre}' name='{ mCampo.Nombre }' class='form-control'>
                                    <option value='' selected>Seleccione...</option>
                                   {GetListCatalogo(mCampo.iCatalogoId)}
                                  </select>
                                </div>";
                    break;
                case TipoCampo.CasillaVerificacion:
                    campo = $@"<div class='col-auto col-3'>
                                  <label for='{mCampo?.Nombre}' class='mr-sm-4'> { mCampo?.Etiqueta}</label>
                                  <select id='{mCampo?.Nombre}' name='{ mCampo.Nombre }' class='form-control'>
                                    <option value=''  selected>Seleccione...</option>
                                    <option value='1' >Activo</option>
                                    <option value='0' >Inactivo</option>
                                  </select>
                                </div>";
                    break;
                default:
                    break;
            }

            //campo += $@"<label for='{ mCampo.Nombre }' generated='true' class='error'></label><div class='input-group mb-0'></div>";

            return campo;

        }

        //Getcampos Editaredit
        public static string GetCampoEdit(Campo mCampo)
        {
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-Us", false);
            string campo = "";
            //PopOver
            string icono = "fa fa-question-circle";
            string popoverName = "popover" + mCampo.CampoId.ToString();
            string poppover = "<i class='" + icono + "' data-toggle='popover' data-placement='top' data-content='" + mCampo.Ayuda + "' aria-describedby='" + popoverName + "'></i>";
            string maxLenght = mCampo.TipoCampo != TipoCampo.Fecha && mCampo.TipoCampo != TipoCampo.Hora ? $"maxlength='{mCampo.Longitud }'" : "";
            string cssFormatoFecha = mCampo.TipoCampo == TipoCampo.Fecha ? "datefield" : "";
            string ReadyOnly = mCampo.TipoCampo == TipoCampo.Fecha || mCampo.TipoCampo == TipoCampo.Hora ? "readonly" : "";
            string sPlaceholder = mCampo.TipoCampo == TipoCampo.Fecha || mCampo.TipoCampo == TipoCampo.Hora ? "Seleccione..." : mCampo.Etiqueta;
            //Campos
            campo = $@" <div for='{mCampo.Nombre}' class='row col-lg-12 mb-2'>
                        <div class='col-md-6'> { mCampo?.Etiqueta} </div>
                        <div class='col-md-6 text-right'><a class='text-info' data-toggle='popover' data-placement='top' data-content='{ mCampo.Ayuda }' aria-describedby='{ popoverName}'> Información {poppover}</a></div>
                      </div>";
            switch (mCampo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.Alfanumerico:
                    campo += $@"
                            <div class='input-group mb-3'>
                                    <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Dinero:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>$</span>
                                    </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                  <div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Porcentaje:
                    var opcionDecimales = mCampo._ConDecimales;

                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>%</span>
                                    </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >";
                    if (mCampo._ConDecimales == true)
                    {
                        campo += $@"<div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>";
                    }
                    campo += "</div>";
                    break;
                case TipoCampo.Decimal:
                    campo += $@"<div class='input-group mb-3'>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                  <div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Fecha:
                    string valor = mCampo.Valor;
                    try { var dt = mCampo.Valor.ToUpper().ToDate(stringFormatDate); 
                          valor = dt.HasValue? dt.Value.ToString("dd/MM/yyyy") :  mCampo.Valor.Replace(" 12:00:00 a. m.",""); /*Convert.ToDateTime(mCampo.Valor); valor = oDate.ToString("dd/MM/yyyy");*/ 
                    } catch (Exception ex) { }
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'><i class='fa fa-calendar'></i></span>
                                  </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{ valor }' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Hora:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>Hora</span>
                                  </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Hipervinculo:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-link'></i></span>
                                      </div>
                                    <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                    </div>";
                    break;
                case TipoCampo.Telefono:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-phone'></i></span>
                                      </div>
                                    <input type='text' class='form-control {cssFormatoFecha}' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.AreaTexto:
                    campo += $@"
                            <div class='input-group mb-3'>
                               <textarea class='form-control'  id='{mCampo.Nombre}'  name='{mCampo.Nombre}' rows='5' {maxLenght}>{mCampo.Valor}</textarea>
                            </div>";
                    break;
                case TipoCampo.Numerico:
                    campo += $@"
                                <div class='input-group mb-3'>
                                    <input type='number' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' placeholder='0' {maxLenght}>
                                    </div>";
                    break;
                case TipoCampo.email:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-envelope'></i></span>
                                      </div>
                                    <input type='email' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}'>
                                </div>";
                    break;

                case TipoCampo.ArchivoAdjunto:
                    var extensiones = GetAcceptFileTypeFile(mCampo._GrupoExtensionId);
                    campo += $@"
                                <div class=""input-group mb-3"">
                                  <div class=""input-group-prepend"">
                                    <span class=""input-group-text"" id=""inputGroupFileAddon01""><i class='fa fa-file'></i></span>
                                  </div>
                                  <div class=""custom-file"">
                                    <input type=""file"" class=""custom-file-input"" id='{mCampo.Nombre}'  name='{mCampo.Nombre}' accept='{extensiones}' aria-describedby='{mCampo.Nombre}'>
                                    <label class=""custom-file-label"" for=""inputGroupFile01"">Seleccione el archivo</label>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Catalogo:
                    if (GetTipoTabla(mCampo.CatalogoId))
                    {
                        //conseguir los campos deeste catalogo

                        //string modalName = $"Modal{mCampo.CatalogoId}";
                        //string buttonNewField = $"NewField{mCampo.CatalogoId}"; //creo que no lo estamos usando
                        //string buttonDeleteField = $"deleteField{mCampo.CatalogoId}";
                        //string nombreCatalogo = GetNombreTabla(mCampo.CatalogoId); //GetNombreCatalogo(mCampo.);
                        //var total = getTotalRowsPlantilla(nombreCatalogo, Idregistro);
                        campo += $@"<div class='input-group'>
                                    <div class='card col-lg-12'>
                                      <div class='card-body'>
                                        <a class='text-{ (mCampo.Requerido ? "danger" : "info") } modalCatalogoList' data-catalogo='{mCampo.CatalogoId}' data-hidden='{mCampo.Nombre}'> Capturar los valores. </a>
                                      </div>
                                    </div>

                                    <input type='hidden' id='{mCampo.Nombre}' name='{mCampo.Nombre}' value='{mCampo.Valor}' />
                                </div>";
                    }
                    else
                    {
                        campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-clipboard'></i></i></span>
                                      </div>
                                    <select id='{mCampo?.Nombre}' name='{ mCampo.Nombre }' class='form-control'>
                                        <option value='0' selected>Seleccione...</option>
                                        {GetListCatalogo(mCampo.CatalogoId, mCampo.Valor)}
                                    </select>
                                </div>";
                    }


                    break;
                case TipoCampo.CasillaVerificacion:
                    string sChecked = mCampo.Valor == "1" || mCampo.Valor == "True" ? "checked" : "";
                    campo += $@"
                                <div class='input-group mb-3'>
                                    <div class='i-checks'>
                                        <input type = 'checkbox' id='{ mCampo.Nombre }'  name='{ mCampo.Nombre }' value = '1' {sChecked} /> 
                                            <label class='form-check-label' for='{ mCampo.Nombre }'> { mCampo?.Etiqueta }</label>
                                    </div>
                                </div>";
                    break;
                default:
                    break;
            }
            campo += $@"<label for='{ mCampo.Nombre }' generated='true' class='error'></label><div class='input-group mb-0'></div>";

            return campo;

        }
        public static string GetCampoEdit(CampoCatalogo mCampo, bool Tabla = false)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-Us", false);
            string campo = "";
            //PopOver
            string icono = "fa fa-question-circle";
            string popoverName = "popover" + mCampo.CampoCatalogoId.ToString();
            string poppover = "<i class='" + icono + "' data-toggle='popover' data-placement='top' data-content='" + mCampo.Ayuda + "' aria-describedby='" + popoverName + "'></i>";
            string maxLenght = mCampo.TipoCampo != TipoCampo.Fecha && mCampo.TipoCampo != TipoCampo.Hora ? $"maxlength='{mCampo.Longitud }'" : "";
            string cssFormatoFecha = mCampo.TipoCampo == TipoCampo.Fecha ? "datefield" : "";
            string ReadyOnly = mCampo.TipoCampo == TipoCampo.Fecha || mCampo.TipoCampo == TipoCampo.Hora ? "readonly" : "";
            string sPlaceholder = mCampo.TipoCampo == TipoCampo.Fecha || mCampo.TipoCampo == TipoCampo.Hora ? "Seleccione..." : mCampo.Etiqueta;
            //Campos

            campo = $@" <div for='{mCampo.Nombre}' class='row col-lg-12 mb-2'>
                        <div class='col-md-6'> { mCampo?.Etiqueta} </div>
                        <div class='col-md-6 text-right'><a class='text-info' data-toggle='popover' data-placement='top' data-content='{ mCampo.Ayuda }' aria-describedby='{ popoverName}'> Información {poppover}</a></div>
                      </div>";

            string sNombre = Tabla ? mCampo.NombreProvicional : mCampo.Nombre;
            switch (mCampo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.Alfanumerico:
                    campo += $@"
                            <div class='input-group mb-3'>
                                    <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Dinero:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>$</span>
                                    </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                  <div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Porcentaje:
                    var opcionDecimales = mCampo._ConDecimales;

                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>%</span>
                                    </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >";
                    if (mCampo._ConDecimales == true)
                    {
                        campo += $@"<div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>";
                    }
                    campo += "</div>";
                    break;
                case TipoCampo.Decimal:
                    campo += $@"<div class='input-group mb-3'>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                  <div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Fecha:
                    string valor = mCampo.Valor;
                    try { DateTime oDate = Convert.ToDateTime(mCampo.Valor); valor = oDate.ToString("dd/MM/yyyy"); } catch (Exception ex) { }
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'><i class='fa fa-calendar'></i></span>
                                  </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' value='{valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Hora:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>Hora</span>
                                  </div>
                                <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Hipervinculo:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-link'></i></span>
                                      </div>
                                    <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                    </div>";
                    break;
                case TipoCampo.Telefono:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-phone'></i></span>
                                      </div>
                                    <input type='text' class='form-control {cssFormatoFecha}' id='{sNombre}'  name='{sNombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}' {maxLenght} {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.AreaTexto:
                    campo += $@"
                            <div class='input-group mb-3'>
                               <textarea class='form-control'  id='{sNombre}'  name='{sNombre}' rows='5' {maxLenght}> {mCampo.Valor} </textarea>
                            </div>";
                    break;
                case TipoCampo.Numerico:
                    campo += $@"
                                <div class='input-group mb-3'>
                                    <input type='number' class='form-control' id='{sNombre}'  name='{sNombre}' value='{mCampo.Valor}' placeholder='0' {maxLenght}>
                                    </div>";
                    break;
                case TipoCampo.email:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-envelope'></i></span>
                                      </div>
                                    <input type='email' class='form-control' id='{sNombre}'  name='{sNombre}' value='{mCampo.Valor}' placeholder='{sPlaceholder}'>
                                </div>";
                    break;

                case TipoCampo.ArchivoAdjunto:
                    var extensiones = GetAcceptFileTypeFile(mCampo._GrupoExtensionId);
                    campo += $@"
                                <div class=""input-group mb-3"">
                                  <div class=""input-group-prepend"">
                                    <span class=""input-group-text"" id=""inputGroupFileAddon01""><i class='fa fa-file'></i></span>
                                  </div>
                                  <div class=""custom-file"">
                                    <input type=""file"" class=""custom-file-input"" id='{sNombre}'  name='{sNombre}' accept='{extensiones}' aria-describedby='{sNombre}'>
                                    <label class=""custom-file-label"" for=""inputGroupFile01"">Seleccione el archivo</label>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Catalogo:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-clipboard'></i></i></span>
                                      </div>
                                    <select id='{sNombre}' name='{ sNombre }' class='form-control'>
                                        <option value='0' selected>Seleccione...</option>
                                        {GetListCatalogo(mCampo.CatalogoId, mCampo.Valor)}
                                    </select>
                                </div>";
                    break;
                case TipoCampo.CasillaVerificacion:
                    string sChecked = mCampo.Valor == "1" || mCampo.Valor == "True" ? "checked" : "";
                    campo += $@"
                                <div class='input-group mb-3'>
                                    <div class='i-checks'>
                                        <input type = 'checkbox' id='{ sNombre }'  name='{sNombre }' value = '1' {sChecked} /> 
                                            <label class='form-check-label' for='{ sNombre }'> { mCampo?.Etiqueta }</label>
                                    </div>
                                </div>";
                    break;
                default:
                    break;
            }
            campo += $@"<label for='{ sNombre }' generated='true' class='error'></label><div class='input-group mb-0'></div>";

            return campo;

        }


        public static string GetFormatForSelect(TipoCampo tipoCampo, string valor, bool? decimales = false)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-Us", false);
            string respuesta = valor;
            switch (tipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.AreaTexto:
                case TipoCampo.Numerico:
                case TipoCampo.Alfanumerico:
                case TipoCampo.Hora:
                case TipoCampo.email:
                case TipoCampo.Telefono:
                case TipoCampo.ArchivoAdjunto:
                    break;
                case TipoCampo.Dinero:
                    var dinero = Convert.ToDecimal(valor).ToString("#,##0.00");
                    respuesta = $@"$ {  dinero }";
                    break;
                case TipoCampo.Porcentaje:
                    respuesta = $" { Convert.ToDecimal(valor).ToString("#,##0.00") }%";
                    break;
                case TipoCampo.Decimal:

                    if (decimales == true)
                    {
                        respuesta = Convert.ToDecimal(valor).ToString("#,##0.00");
                    }
                    else
                    {
                        respuesta = Convert.ToDecimal(valor).ToString("#,##0");
                    }
                    break;
                case TipoCampo.Fecha:
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX", false);
                    var dt = valor.ToUpper().ToDate(stringFormatDate);
                    respuesta = dt.HasValue ? dt.Value.ToLongDateString() : valor;
                    break;
                case TipoCampo.Hipervinculo:
                    break;
                case TipoCampo.Catalogo:
                    break;
                case TipoCampo.CasillaVerificacion:
                    bool sChecked = valor == "1" || valor == "True" ? true : false;
                    if (sChecked)
                    {
                        respuesta = $@"<span class='label label-success'>Activo</span>";
                    }
                    else
                    {
                        respuesta = $@"<span class='label label-danger'>Desactivado</span>";
                    }
                    break;
                default:
                    break;
            }
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-Us", false);
            return respuesta;
        }
        //GtCampos Details
        public static string GetCampoDetails(Campo mCampo)
        {
            string campo = "";
            //PopOver
            string icono = "fa fa-question-circle";
            string popoverName = "popover" + mCampo.CampoId.ToString();
            string poppover = "<i class='" + icono + "' data-toggle='popover' data-placement='top' data-content='" + mCampo.Ayuda + "' aria-describedby='" + popoverName + "'></i>";

            string ReadyOnly = "readonly";
            string valor = FormattingValue(mCampo.Valor, mCampo.TipoCampo);
            //Campos

            campo = $@" <div for='{mCampo.Nombre}' class='row col-lg-12 mb-2'>
                        <div class='col-md-6'> { mCampo?.Etiqueta} </div>
                        <div class='col-md-6 text-right'><a class='text-info' data-toggle='popover' data-placement='top' data-content='{ mCampo.Ayuda }' aria-describedby='{ popoverName}'> Información {poppover}</a></div>
                      </div>";
            switch (mCampo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.Alfanumerico:
                    campo += $@"
                            <div class='input-group mb-3'>
                                    <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Dinero:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>$</span>
                                    </div>
                                <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                                  <div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Porcentaje:
                    var opcionDecimales = mCampo._ConDecimales;

                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>%</span>
                                    </div>
                                <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >";
                    if (mCampo._ConDecimales == true)
                    {
                        campo += $@"<div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>";
                    }
                    campo += "</div>";
                    break;
                case TipoCampo.Decimal:
                    campo += $@"<div class='input-group mb-3'>
                                <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                                  <div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Fecha:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'><i class='fa fa-calendar'></i></span>
                                  </div>
                                <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Hora:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>Hora</span>
                                  </div>
                                <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Hipervinculo:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-link'></i></span>
                                      </div>
                                        <a href='{mCampo.Valor}' target='_blank' title='{mCampo.Valor}'> Ir al Sitio </a>
                                    </div>";
                    break;
                case TipoCampo.Telefono:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-phone'></i></span>
                                      </div>
                                    <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.AreaTexto:
                    campo += $@"
                            <div class='input-group mb-3'>
                               <textarea class='form-control'  id='{mCampo.Nombre}'  name='{mCampo.Nombre}' rows='5'  {ReadyOnly}> {mCampo.Valor} </textarea>
                            </div>";
                    break;
                case TipoCampo.Numerico:
                    campo += $@"
                                <div class='input-group mb-3'>
                                    <input type='number' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}'  {ReadyOnly}>
                                    </div>";
                    break;
                case TipoCampo.email:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-envelope'></i></span>
                                      </div>
                                    <input type='email' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}'  {ReadyOnly}>
                                </div>";
                    break;

                case TipoCampo.ArchivoAdjunto:
                    campo += $@"
                                <div class='input-group mb-3'>
                                    Descargar archivo: <a href='{ mCampo.ValorUrl }' class='btn btn-info btn-circle'><i class='fa fa-paperclip'></i></a>
                                </div>";
                    break;
                case TipoCampo.Catalogo:
                    try
                    {
                        valor = GetNameCatalogo(int.Parse(mCampo.Valor), mCampo.CatalogoId);

                    }
                    catch (Exception ex)
                    {
                    }

                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-clipboard'></i></i></span>
                                      </div>
                                    <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{valor}' {ReadyOnly}>
                                </div>";
                    break;
                case TipoCampo.CasillaVerificacion:
                    string sChecked = mCampo.Valor == "1" || mCampo.Valor == "True" ? "checked" : "";
                    campo += $@"
                                <div class='input-group mb-3'>
                                    <div class='i-checks'>
                                        <input type = 'checkbox' id='{ mCampo.Nombre }'  name='{ mCampo.Nombre }' value = '1' {sChecked} {ReadyOnly} onclick='javascript: return false;' /> 
                                            <label class='form-check-label' for='{ mCampo.Nombre }'> { mCampo?.Etiqueta }</label>
                                    </div>
                                </div>";
                    break;
                default:
                    break;
            }
            campo += $@"<label for='{ mCampo.Nombre }' generated='true' class='error'></label><div class='input-group mb-0'></div>";

            return campo;

        }
        public static string GetCampoDetailsDForModal(Campo mCampo,bool isPdf=false)
        {
            string campo = "";
            //PopOver
            string icono = "fa fa-question-circle";
            string popoverName = "popover" + mCampo.CampoId.ToString();
            string poppover = "<i class='" + icono + "' data-toggle='popover' data-placement='top' data-content='" + mCampo.Ayuda + "' aria-describedby='" + popoverName + "'></i>";

            string ReadyOnly = "readonly";
            string valor = FormattingValue(mCampo.Valor, mCampo.TipoCampo);
            //Campos

            campo = $@"   <tr><td class='text-right font-weight-bold'>{ mCampo?.Etiqueta}&nbsp;</td>";
            //" <div for='{mCampo.Nombre}' class='row col-lg-12 mb-2'>
            //          < div class='col-md-6'> { mCampo?.Etiqueta} </div>
            //        <div class='col-md-6 text-right'><a class='text-info' data-toggle='popover' data-placement='top' data-content='{ mCampo.Ayuda }' aria-describedby='{ popoverName}'> Información {poppover}</a></div>
            //      </div>";
            switch (mCampo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.Alfanumerico:
                    campo += $@"<td>&nbsp;<p class='p-1'> {mCampo.Valor}</p></td>";
                    //<div class='input-group mb-3'>
                    //        <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                    //    </div>";
                    break;
                case TipoCampo.Dinero:
                    campo += $@"<td>&nbsp;<p class='p-1'> {mCampo.Valor}</p></td>";
                    //campo += $@"
                    //        <div class='input-group mb-3'>
                    //              <div class='input-group-prepend'>
                    //                <span class='input-group-text'>$</span>
                    //                </div>
                    //            <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                    //              <div class='input-group-append'>
                    //                <span class='input-group-text'>.00</span>
                    //              </div>
                    //            </div>";
                    break;
                case TipoCampo.Porcentaje:
                    //var opcionDecimales = mCampo._ConDecimales;

                    //campo += $@"
                    //        <div class='input-group mb-3'>
                    //              <div class='input-group-prepend'>
                    //                <span class='input-group-text'>%</span>
                    //                </div>
                    //            <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >";
                    //if (mCampo._ConDecimales == true)
                    //{
                    //    campo += $@"<div class='input-group-append'>
                    //                <span class='input-group-text'>.00</span>
                    //              </div>";
                    //}
                    //campo += "</div>";
                    campo += $@"<td>&nbsp;<p class='p-1'> {mCampo.Valor}</p></td>";
                    break;
                case TipoCampo.Decimal:
                    //campo += $@"<div class='input-group mb-3'>
                    //            <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                    //              <div class='input-group-append'>
                    //                <span class='input-group-text'>.00</span>
                    //              </div>
                    //            </div>";
                    campo += $@"<td>&nbsp;<p class='p-1'> {mCampo.Valor}</p></td>";
                    break;
                case TipoCampo.Fecha:
                    //campo += $@"
                    //        <div class='input-group mb-3'>
                    //              <div class='input-group-prepend'>
                    //                <span class='input-group-text'><i class='fa fa-calendar'></i></span>
                    //              </div>
                    //            <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                    //            </div>";
                    campo += $@"<td>&nbsp;<p class='p-1'> {mCampo.Valor}</p></td>";
                    break;
                case TipoCampo.Hora:
                    //campo += $@"
                    //        <div class='input-group mb-3'>
                    //              <div class='input-group-prepend'>
                    //                <span class='input-group-text'>Hora</span>
                    //              </div>
                    //            <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                    //            </div>";
                    campo += $@"<td>&nbsp;<p class='p-1'> {mCampo.Valor}</p></td>";
                    break;
                case TipoCampo.Hipervinculo:
                    //campo += $@"
                    //            <div class='input-group mb-3'>
                    //                  <div class='input-group-prepend'>
                    //                    <span class='input-group-text'><i class='fa fa-link'></i></span>
                    //                  </div>
                    //                <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                    //                </div>";
                    campo += $@"<td>&nbsp;<p class='p-1'> <a href='{mCampo.Valor}' target='_blank' title='{mCampo.Valor}'> Ir al Sitio </a> </p></td>";
                    break;
                case TipoCampo.Telefono:
                    //campo += $@"
                    //            <div class='input-group mb-3'>
                    //                  <div class='input-group-prepend'>
                    //                    <span class='input-group-text'><i class='fa fa-phone'></i></span>
                    //                  </div>
                    //                <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                    //            </div>";
                    campo += $@"<td>&nbsp;<p class='p-1'> {mCampo.Valor}</p></td>";
                    break;
                case TipoCampo.AreaTexto:
                    //campo += $@"
                    //        <div class='input-group mb-3'>
                    //           <textarea class='form-control'  id='{mCampo.Nombre}'  name='{mCampo.Nombre}' rows='5'  {ReadyOnly}> {mCampo.Valor} </textarea>
                    //        </div>";
                    campo += $@"<td>&nbsp;<p class='p-1'> {mCampo.Valor}</p></td>";
                    break;
                case TipoCampo.Numerico:
                    //campo += $@"
                    //            <div class='input-group mb-3'>
                    //                <input type='number' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}'  {ReadyOnly}>
                    //                </div>";
                    campo += $@"<td>&nbsp;<p class='p-1'> {mCampo.Valor}</p></td>";
                    break;
                case TipoCampo.email:
                    //campo += $@"
                    //            <div class='input-group mb-3'>
                    //                  <div class='input-group-prepend'>
                    //                    <span class='input-group-text'><i class='fa fa-envelope'></i></span>
                    //                  </div>
                    //                <input type='email' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}'  {ReadyOnly}>
                    //            </div>";
                    campo += $@"<td>&nbsp;<p class='p-1'> {mCampo.Valor}</p></td>";
                    break;

                case TipoCampo.ArchivoAdjunto:
                    
                    if (isPdf)
                    {

                        string baseUrl = "http://"+ HttpContext.Current.Request.Url.Authority;
                        if (!string.IsNullOrEmpty(mCampo.ValorUrl) && !string.IsNullOrEmpty( mCampo.Valor)){
                            campo += $@"<td>{baseUrl}{mCampo.ValorUrl}</td>";
                        }
                        else
                        {
                            campo += "<td></td>";
                        }
                    }
                           
                    else
                    {
                        campo += $@"<td>&nbsp; <a href='{mCampo.Valor}' ><i class='fa fa-paperclip'></i> Descargar archivo</a> </td>";
                    }
                    
                    //campo += $@"
                    //            <div class='input-group mb-3'>
                    //                Descargar archivo: <a href='{ mCampo.ValorUrl }' class='btn btn-info btn-circle'><i class='fa fa-paperclip'></i></a>
                    //            </div
                    //            
                    break;
                case TipoCampo.Catalogo:
                    try
                    {
                        if (GetTipoTabla(mCampo.CatalogoId))
                        {
                            campo += $@"<td>&nbsp;<code class='highlighter-rouge'> <a class='text-info' onclick='MostrarTablaDatos({mCampo.TablaFisicaId},{mCampo.Valor})'><i class='fa fa-table'></i> Ver información</a> </code></td>";
                        }
                        else
                        {
                            campo += $@"<td>&nbsp;<p class='p-1'> {GetNameCatalogo(int.Parse(mCampo.Valor), mCampo.CatalogoId)} </p></td>";
                        }


                    }
                    catch (Exception ex)
                    {
                    }

                    //campo += $@"<td>&nbsp;<code class='highlighter-rouge'> {valor}</code></td>";
                    //campo += $@"
                    //            <div class='input-group mb-3'>
                    //                  <div class='input-group-prepend'>
                    //                    <span class='input-group-text'><i class='fa fa-clipboard'></i></i></span>
                    //                  </div>
                    //                <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{valor}' {ReadyOnly}>
                    //            </div>";
                    break;
                case TipoCampo.CasillaVerificacion:

                    bool sChecked = mCampo.Valor == "1" || mCampo.Valor == "True" ? true : false;
                    //campo += $@"
                    //            <div class='input-group mb-3'>
                    //                <div class='i-checks'>
                    //                    <input type = 'checkbox' id='{ mCampo.Nombre }'  name='{ mCampo.Nombre }' value = '1' {sChecked} {ReadyOnly} onclick='javascript: return false;' /> 
                    //                        <label class='form-check-label' for='{ mCampo.Nombre }'> { mCampo?.Etiqueta }</label>
                    //                </div>
                    //            </div>";
                    if (sChecked)
                    {
                        campo += $@"<td>&nbsp;<span class='badge badge-primary'>Activo</span></td>";
                    }
                    else
                    {
                        campo += $@"<td>&nbsp;<span class='badge badge-danger'>Inactivo</span></td>";
                    }
                    break;
                default:
                    break;
            }
            campo += "</tr>";
            //campo += $@"<label for='{ mCampo.Nombre }' generated='true' class='error'></label><div class='input-group mb-0'></div>";

            return campo;

        }
        public static string GetCampoDetails(CampoCatalogo mCampo)
        {
            string campo = "";
            //PopOver
            string icono = "fa fa-question-circle";
            string popoverName = "popover" + mCampo.CampoCatalogoId.ToString();
            string poppover = "<i class='" + icono + "' data-toggle='popover' data-placement='top' data-content='" + mCampo.Ayuda + "' aria-describedby='" + popoverName + "'></i>";

            string ReadyOnly = "readonly";
            string valor = FormattingValue(mCampo.Valor, mCampo.TipoCampo);
            //Campos

            campo = $@" <div for='{mCampo.Nombre}' class='row col-lg-12 mb-2'>
                        <div class='col-md-6'> { mCampo?.Etiqueta} </div>
                        <div class='col-md-6 text-right'><a class='text-info' data-toggle='popover' data-placement='top' data-content='{ mCampo.Ayuda }' aria-describedby='{ popoverName}'> Información {poppover}</a></div>
                      </div>";
            switch (mCampo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.Alfanumerico:
                    campo += $@"
                            <div class='input-group mb-3'>
                                    <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Dinero:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>$</span>
                                    </div>
                                <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                                  <div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Porcentaje:
                    var opcionDecimales = mCampo._ConDecimales;

                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>%</span>
                                    </div>
                                <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >";
                    if (mCampo._ConDecimales == true)
                    {
                        campo += $@"<div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>";
                    }
                    campo += "</div>";
                    break;
                case TipoCampo.Decimal:
                    campo += $@"<div class='input-group mb-3'>
                                <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                                  <div class='input-group-append'>
                                    <span class='input-group-text'>.00</span>
                                  </div>
                                </div>";
                    break;
                case TipoCampo.Fecha:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'><i class='fa fa-calendar'></i></span>
                                  </div>
                                <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Hora:
                    campo += $@"
                            <div class='input-group mb-3'>
                                  <div class='input-group-prepend'>
                                    <span class='input-group-text'>Hora</span>
                                  </div>
                                <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.Hipervinculo:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-link'></i></span>
                                      </div>
                                    <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                                    </div>";
                    break;
                case TipoCampo.Telefono:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-phone'></i></span>
                                      </div>
                                    <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}' {ReadyOnly} >
                                </div>";
                    break;
                case TipoCampo.AreaTexto:
                    campo += $@"
                            <div class='input-group mb-3'>
                               <textarea class='form-control'  id='{mCampo.Nombre}'  name='{mCampo.Nombre}' rows='5'  {ReadyOnly}> {mCampo.Valor} </textarea>
                            </div>";
                    break;
                case TipoCampo.Numerico:
                    campo += $@"
                                <div class='input-group mb-3'>
                                    <input type='number' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}'  {ReadyOnly}>
                                    </div>";
                    break;
                case TipoCampo.email:
                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-envelope'></i></span>
                                      </div>
                                    <input type='email' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{mCampo.Valor}'  {ReadyOnly}>
                                </div>";
                    break;

                case TipoCampo.ArchivoAdjunto:
                    campo += $@"
                                <div class='input-group mb-3'>
                                    Descargar archivo: <a href='{ mCampo.ValorUrl }' class='btn btn-info btn-circle'><i class='fa fa-paperclip'></i></a>
                                </div>";
                    break;
                case TipoCampo.Catalogo:
                    try
                    {
                        valor = GetNameCatalogo(int.Parse(mCampo.Valor), mCampo.iCatalogoId);

                    }
                    catch (Exception ex)
                    {
                    }

                    campo += $@"
                                <div class='input-group mb-3'>
                                      <div class='input-group-prepend'>
                                        <span class='input-group-text'><i class='fa fa-clipboard'></i></i></span>
                                      </div>
                                    <input type='text' class='form-control' id='{mCampo.Nombre}'  name='{mCampo.Nombre}' value='{valor}' {ReadyOnly}>
                                </div>";
                    break;
                case TipoCampo.CasillaVerificacion:
                    string sChecked = mCampo.Valor == "1" || mCampo.Valor == "True" ? "checked" : "";
                    campo += $@"
                                <div class='input-group mb-3'>
                                    <div class='i-checks'>
                                        <input type = 'checkbox' id='{ mCampo.Nombre }'  name='{ mCampo.Nombre }' value = '1' {sChecked} {ReadyOnly} onclick='javascript: return false;' /> 
                                            <label class='form-check-label' for='{ mCampo.Nombre }'> { mCampo?.Etiqueta }</label>
                                    </div>
                                </div>";
                    break;
                default:
                    break;
            }
            campo += $@"<label for='{ mCampo.Nombre }' generated='true' class='error'></label><div class='input-group mb-0'></div>";
            return campo;

        }

        //tipo de archivos
        public static string GetAcceptFileTypeFile(int? iGrupoExtensionId = 0)
        {
            string resultado = "";
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                var mGrupoExtension = db.GrupoExtesiones.Where(x => x.GrupoExtensionId == iGrupoExtensionId).FirstOrDefault();
                if (mGrupoExtension != null)
                {
                    var mExtesionees = db.Extensiones.Where(x => x.Activo).ToList();
                    if (mExtesionees.Count > 0)
                    {
                        var first = true;
                        foreach (var item in mExtesionees)
                        {
                            if (!first)
                            {
                                resultado += ",";
                            }
                            resultado += "." + item.Nombre;

                            first = false;
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }


            return resultado;
        }

        //traer el nombre del catalogo 
        public static string GetNameCatalogo(int iCatalogoValorId = 0, int iCatalogoId = 0)
        {
            string sCatalogo = "";

            try
            {
                if (iCatalogoValorId != 0 && iCatalogoId != 0)
                {
                    ApplicationDbContext db = new ApplicationDbContext();
                    //var mCatalogos = db.Catalogoes.Where(x => x.CatalogoId == iCatalogoId).FirstOrDefault();
                    ////tenemos que buscar nuevamente en catalgoos porque es el catalogo dentro de otro catalogo
                    //var mCampoPrincipal = db.CampoCatalogo.Where(x => x.CatalogoId == mCatalogos.CatalogoId && x.Principal).FirstOrDefault();

                    //while (mCampoPrincipal.TipoCampo == TipoCampo.Catalogo){

                    //    mCatalogos = db.Catalogoes.Where(x => x.CatalogoId == mCampoPrincipal.iCatalogoId).FirstOrDefault();
                    //    //tenemos que buscar nuevamente en catalgoos porque es el catalogo dentro de otro catalogo
                    //    mCampoPrincipal = db.CampoCatalogo.Where(x => x.CatalogoId == mCatalogos.CatalogoId && x.Principal).FirstOrDefault();
                    //}
                    //var sQuery = $@"SELECT TablaFisicaId, {mCampoPrincipal.Nombre} as Valor FROM {mCatalogos.NombreTabla} WHERE Activo = 1 AND TablaFisicaId={iCatalogoValorId}";
                    var sQuery = $@"DECLARE	@return_value int EXEC	@return_value = [dbo].[BuscarCatalogo] @iCatalogoValorId = {iCatalogoValorId}, @iCatalogoId = {iCatalogoId}";
                    var Resultado = db.Database.SqlQuery<string>(sQuery).FirstOrDefault();
                    if (Resultado != null)
                    {
                        sCatalogo = Resultado;
                    }
                }
            }
            catch (Exception ex)
            {
                sCatalogo = ex.Message;
            }
            return sCatalogo;
        }

        //Traer el nobre de la tabla en donde essta el catalago
        public static string GetNombreTablaOCampoCatalago(int iCatalogoId = 0, bool sNombreCampo = false)
        {
            string sCatalogo = "";
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                if (iCatalogoId != 0)
                {
                    var mCatalogos = db.Catalogoes.Where(x => x.CatalogoId == iCatalogoId).FirstOrDefault();
                    //tenemos que buscar nuevamente en catalgoos porque es el catalogo dentro de otro catalogo
                    var mCampoPrincipal = db.CampoCatalogo.Where(x => x.CatalogoId == mCatalogos.CatalogoId && x.Principal).FirstOrDefault();
                    if (mCampoPrincipal != null)
                    {
                        while (mCampoPrincipal.TipoCampo == TipoCampo.Catalogo)
                        {

                            mCatalogos = db.Catalogoes.Where(x => x.CatalogoId == mCampoPrincipal.iCatalogoId).FirstOrDefault();
                            //tenemos que buscar nuevamente en catalgoos porque es el catalogo dentro de otro catalogo
                            mCampoPrincipal = db.CampoCatalogo.Where(x => x.CatalogoId == mCatalogos.CatalogoId && x.Principal).FirstOrDefault();
                        }
                        sCatalogo = sNombreCampo ? mCampoPrincipal.Nombre : mCatalogos.NombreTabla;
                    }


                }
            }
            catch (Exception ex)
            {
            }
            return sCatalogo;
        }

        public static TipoCampo GetTipoCampooCatalago(int iCatalogoId = 0)
        {
            var sCatalogo = new TipoCampo();
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                if (iCatalogoId != 0)
                {
                    var mCatalogos = db.Catalogoes.Where(x => x.CatalogoId == iCatalogoId).FirstOrDefault();
                    //tenemos que buscar nuevamente en catalgoos porque es el catalogo dentro de otro catalogo
                    var mCampoPrincipal = db.CampoCatalogo.Where(x => x.CatalogoId == mCatalogos.CatalogoId && x.Principal).FirstOrDefault();
                    if (mCampoPrincipal != null)
                    {
                        while (mCampoPrincipal.TipoCampo == TipoCampo.Catalogo)
                        {

                            mCatalogos = db.Catalogoes.Where(x => x.CatalogoId == mCampoPrincipal.iCatalogoId).FirstOrDefault();
                            //tenemos que buscar nuevamente en catalgoos porque es el catalogo dentro de otro catalogo
                            mCampoPrincipal = db.CampoCatalogo.Where(x => x.CatalogoId == mCatalogos.CatalogoId && x.Principal).FirstOrDefault();
                        }
                        sCatalogo = mCampoPrincipal.TipoCampo;
                    }


                }
            }
            catch (Exception ex)
            {
            }
            return sCatalogo;
        }



        public static string GetNombreCampo(int iCampoCatalagoId = 0)
        {
            string sCatalogo = "";
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                if (iCampoCatalagoId != 0)
                {
                    var mCatalogos = db.CampoCatalogo.Where(x => x.CampoCatalogoId == iCampoCatalagoId).FirstOrDefault();
                    if (mCatalogos != null)
                    {
                        return mCatalogos.Nombre;
                    }


                }
            }
            catch (Exception ex)
            {
            }
            return sCatalogo;
        }
        public static string GetNombreTabla(int iCatalogoId = 0, bool plantillas = false)
        {
            string sCatalogo = "";
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                if (iCatalogoId != 0)
                {
                    if (plantillas)
                    {
                        var mCatalogos = db.Plantillas.Where(x => x.PlantillaId == iCatalogoId).FirstOrDefault();
                        if (mCatalogos != null)
                        {
                            return mCatalogos.NombreTabla;
                        }
                    }
                    else
                    {
                        var mCatalogos = db.Catalogoes.Where(x => x.CatalogoId == iCatalogoId).FirstOrDefault();
                        if (mCatalogos != null)
                        {
                            return mCatalogos.NombreTabla;
                        }
                    }



                }
            }
            catch (Exception ex)
            {
            }
            return sCatalogo;
        }







        //CatalogosFill

        //public static string GetListCatalogo(int iCatalogoId = 0, string valor = "")
        //{
        //    string sOption = "";
        //    int iIdCatalagoValor = 0;
        //    var result = int.TryParse(valor, out iIdCatalagoValor);
        //    ApplicationDbContext db = new ApplicationDbContext();
        //    var CatalogoValor = db.CatalogoValores.Where(x => x.CatalogoId == iCatalogoId).ToList();
        //    if (CatalogoValor.Count > 0)
        //    {

        //        foreach (var item in CatalogoValor)
        //        {
        //            string selected = iIdCatalagoValor == item.CatalogoValorId ? "selected" : "";
        //            sOption += $@"<option value='{item.CatalogoValorId}' {selected}>{item.valor}</option>";
        //        }
        //    }

        //    return sOption;
        //}


        public static string GetListCatalogo(int iCatalogoId = 0, string valor = "", List<int> ListCatalagosId = null, bool VienedeCatalogo = false)
        {
            string sOption = "";
            int iIdCatalagoValor = 0;
            var result = int.TryParse(valor, out iIdCatalagoValor);
            ApplicationDbContext db = new ApplicationDbContext();

            try
            {
                var mCatalogos = db.Catalogoes.Where(x => x.CatalogoId == iCatalogoId).FirstOrDefault();

                if (iCatalogoId != 0 && mCatalogos != null && mCatalogos.NombreTabla.Length > 0)
                {
                    //tenemos que buscar nuevamente en catalgoos porque es el catalogo dentro de otro catalogo
                    var mCampoPrincipal = db.CampoCatalogo.Where(x => x.CatalogoId == mCatalogos.CatalogoId && x.Principal).FirstOrDefault();

                    var sQuery = "";
                    var VariableValor = mCampoPrincipal.TipoCampo == TipoCampo.Catalogo ? "ValorInt" : "Valor";
                    //iniciamos el qury para podr consultar.
                    sQuery = $@"SELECT TablaFisicaId, {mCampoPrincipal.Nombre} as {VariableValor} FROM {mCatalogos.NombreTabla} WHERE Activo = 1";
                    if (VienedeCatalogo)
                    {
                        if (ListCatalagosId.Count > 0)
                        {
                            sQuery += " AND TablaFisicaId IN (";
                            var Primero = true;
                            foreach (var item in ListCatalagosId)
                            {
                                if (!Primero)
                                {
                                    sQuery += ",";
                                }
                                sQuery += $@"{item}";
                                Primero = false;
                            }
                            sQuery += ")";
                        }

                    }
                    SqlConnection sqlConnectionsCatalogos = new SqlConnection(coneccion);
                    sqlConnectionsCatalogos.Open();
                    var Resultado = sqlConnectionsCatalogos.Query<RespuestaQuery>(sQuery).ToList();
                    sqlConnectionsCatalogos.Close();

                    switch (mCampoPrincipal.TipoCampo)
                    {
                        case TipoCampo.Catalogo:

                            if (Resultado.Count > 0)
                            {
                                sOption = GetListCatalogo(mCampoPrincipal.iCatalogoId, valor, Resultado.Select(x => x.ValorInt).ToList(), true);
                            }
                            break;
                        default:
                            if (Resultado.Count > 0)
                            {
                                foreach (var item in Resultado)
                                {
                                    string selected = iIdCatalagoValor == item.TablaFisicaId ? "selected" : "";
                                    sOption += $@"<option value='{item.TablaFisicaId}' {selected}>{item.Valor}</option>";
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {


            }
            return sOption;
        }

        public static List<RespuestaQuery> GetListCatalogos(string NombreTabla, string Nombrecolumna)
        {
            var sQuery = "";
            List<RespuestaQuery> Resultado = new List<RespuestaQuery>();
            ApplicationDbContext db = new ApplicationDbContext();

            try
            {

                if (NombreTabla.Length > 0 && Nombrecolumna.Length > 0)
                {
                    //iniciamos el qury para podr consultar.
                    sQuery = $@"SELECT TablaFisicaId, {Nombrecolumna} as Valor FROM {NombreTabla} WHERE Activo = 1";
                    SqlConnection sqlConnectionsCatalogos = new SqlConnection(coneccion);
                    sqlConnectionsCatalogos.Open();
                    Resultado = sqlConnectionsCatalogos.Query<RespuestaQuery>(sQuery).ToList();
                    sqlConnectionsCatalogos.Close();
                }
            }
            catch (Exception ex)
            {


            }
            return Resultado;
        }

        public static List<RespuestaQuery> GetListCatalogos(int iCatalogoId)
        {
            var sQuery = "";
            List<RespuestaQuery> Resultado = new List<RespuestaQuery>();
            ApplicationDbContext db = new ApplicationDbContext();

            try
            {
                var Catalogo = db.Catalogoes.Where(x => x.CatalogoId == iCatalogoId).FirstOrDefault();
                string Nombrecolumna = db.CampoCatalogo.Where(x => x.CatalogoId == iCatalogoId && x.Principal).FirstOrDefault().Nombre;

                if (Catalogo.NombreTabla.Length > 0 && Nombrecolumna.Length > 0)
                {
                    //iniciamos el qury para podr consultar.
                    sQuery = $@"SELECT TablaFisicaId, {Nombrecolumna} as Valor FROM {Catalogo.NombreTabla} WHERE Activo = 1";
                    SqlConnection sqlConnectionsCatalogos = new SqlConnection(coneccion);
                    sqlConnectionsCatalogos.Open();
                    Resultado = sqlConnectionsCatalogos.Query<RespuestaQuery>(sQuery).ToList();
                    sqlConnectionsCatalogos.Close();
                }
            }
            catch (Exception ex)
            {


            }
            return Resultado;
        }
        public static bool GetTipoTabla(int CatalogoId)
        {
            var resultado = false;
            ApplicationDbContext db = new ApplicationDbContext();

            try
            {
                var Catalogo = db.Catalogoes.Where(x => x.CatalogoId == CatalogoId).FirstOrDefault();
                if (Catalogo != null)
                {
                    return Catalogo.Tabla;
                }

            }
            catch (Exception ex)
            {


            }
            return resultado;
        }

        public static string GetNombreCatalogo(int CatalogoId)
        {
            var resultado = "";
            ApplicationDbContext db = new ApplicationDbContext();

            try
            {
                return db.Catalogoes.Where(x => x.CatalogoId == CatalogoId).FirstOrDefault().Nombre;


            }
            catch (Exception ex)
            {
            }
            return resultado;
        }
        public static string ExtensionMethod(this HtmlHelper helper)
        {
            UrlHelper urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            return urlHelper.Action("Action", "Controller");
        }
        public static string FormattingValue(string sValor, TipoCampo tipoCampo)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-Us", false);

            string valor = sValor;
            try
            {
                switch (tipoCampo)
                {
                    //case TipoCampo.Texto:
                    //    break;
                    //case TipoCampo.AreaTexto:
                    //    break;
                    //case TipoCampo.Numerico:
                    //    break;
                    //case TipoCampo.Alfanumerico:
                    //    break;
                    case TipoCampo.Dinero:
                        break;
                    case TipoCampo.Porcentaje:
                        break;
                    case TipoCampo.Decimal:
                        break;
                    case TipoCampo.Fecha:
                        //string sDate = sValor;
                        //DateTime oDate = Convert.ToDateTime(sDate);
                        //Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX", false);
                        //return oDate.ToLongDateString();
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX", false);
                        var dt = valor.ToUpper().ToDate(stringFormatDate);
                        return dt.HasValue ? dt.Value.ToLongDateString() : valor;
                    case TipoCampo.Hora:
                        break;
                    case TipoCampo.Hipervinculo:
                        break;
                    case TipoCampo.email:
                        break;
                    case TipoCampo.Telefono:
                        break;
                    case TipoCampo.ArchivoAdjunto:
                        break;
                    case TipoCampo.Catalogo:
                        break;
                    case TipoCampo.CasillaVerificacion:
                        break;
                }

            }
            catch (Exception ex)
            {

            }



            return valor;
        }

        public static string GetLeyArtiucloFraccionFromPlantilla(int? plantillaId = 0)
        {
            var Respuesta = "";
            ApplicationDbContext db = new ApplicationDbContext();


            try
            {
                var plantilla =  db.Plantillas.Where(x => x.PlantillaId == plantillaId).FirstOrDefault();
                if(plantilla != null)
                {
                    Respuesta = plantilla.NombreCorto + " - " + plantilla.NombreLargo;
                }
                //var fraccionPlantilla = db.PlantillaFraccions.Where(x => x.PlantillaId == plantillaId).FirstOrDefault();

                //if (fraccionPlantilla != null)
                //{
                //    string Fraccion = "";
                //    string Articulo = "";
                //    string Ley = "";
                //    var mFraccion = db.Fracciones.Where(x => x.FraccionId == fraccionPlantilla.FraccionId).FirstOrDefault();
                //    Fraccion = mFraccion != null ? mFraccion.Nombre : "";
                //    Articulo mArticulo = null;
                //    if (mFraccion != null)
                //    {
                //        mArticulo = db.Articulos.Where(x => x.ArticuloId == mFraccion.ArticuloId).FirstOrDefault();
                //        Articulo = mArticulo != null ? mArticulo.Nombre : "";
                //    }
                //    Ley mLey = null;
                //    if (mArticulo != null)
                //    {
                //        mLey = db.Leyes.Where(x => x.LeyId == mArticulo.LeyId).FirstOrDefault();
                //        Ley = mLey != null ? mLey.Nombre : "";
                //    }


                //    Respuesta = Fraccion + ", " + Articulo + ", " + Ley;

                //}
            }
            catch (Exception ex)
            {


            }
            return Respuesta;
        }




        #region MdoficarCampos

        //informaccion sobre el cambios
        public static string GetInfoOfUpdate(CampoCatalogo campoViejo, CampoCatalogo campoNuevo)
        {
            string respuesta = "";
            var ErrorCompatibilidad = false;
            try
            {
                var NombreTabla = GetNombreTabla(campoViejo.CatalogoId);
                var NombreCampo = GetNombreCampo(campoViejo.CampoCatalogoId);
                if (CheckIfExistTableAndColumn(NombreTabla, NombreCampo))
                {
                    //vemos si tiene registros
                    var Records = GetNumberOfRecords(NombreCampo, NombreTabla);
                    if (Records != null && Records > 0)
                    {

                        //Verificamos si no se exede de los registros
                        var MaxLenght = GetMMaxLenghOfInformation(NombreCampo, NombreTabla);
                        if (MaxLenght != null && MaxLenght > campoNuevo.Longitud)
                        {
                            if (campoNuevo.TipoCampo != TipoCampo.ArchivoAdjunto
                               && campoNuevo.TipoCampo != TipoCampo.CasillaVerificacion
                               && campoNuevo.TipoCampo != TipoCampo.Catalogo
                               && campoNuevo.TipoCampo != TipoCampo.Catalogo
                               && campoNuevo.TipoCampo != TipoCampo.Fecha
                               && campoNuevo.TipoCampo != TipoCampo.Hora)
                            {
                                ErrorCompatibilidad = true;
                            }
                        }

                        switch (campoViejo.TipoCampo)
                        {
                            case TipoCampo.Alfanumerico:
                                if (campoNuevo.TipoCampo != TipoCampo.AreaTexto
                                    && campoNuevo.TipoCampo != TipoCampo.Alfanumerico)
                                {
                                    ErrorCompatibilidad = true;
                                }

                                break;
                            case TipoCampo.AreaTexto:
                                if (campoNuevo.TipoCampo != TipoCampo.AreaTexto)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.CasillaVerificacion:
                                if (campoNuevo.TipoCampo != TipoCampo.CasillaVerificacion
                                    && campoNuevo.TipoCampo != TipoCampo.Numerico
                                    && campoNuevo.TipoCampo != TipoCampo.AreaTexto
                                    && campoNuevo.TipoCampo != TipoCampo.Alfanumerico)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Catalogo:
                                if (campoNuevo.TipoCampo != TipoCampo.Catalogo)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Decimal:
                                if (campoNuevo.TipoCampo != TipoCampo.Decimal
                                    && campoNuevo.TipoCampo != TipoCampo.Dinero
                                    && campoNuevo.TipoCampo != TipoCampo.Porcentaje)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                else if (campoNuevo.TipoCampo == TipoCampo.Porcentaje
                                   && campoNuevo._ConDecimales == false)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Dinero:
                                if (campoNuevo.TipoCampo != TipoCampo.Decimal
                                    && campoNuevo.TipoCampo != TipoCampo.Dinero
                                    && campoNuevo.TipoCampo != TipoCampo.Porcentaje)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                else if (campoNuevo.TipoCampo == TipoCampo.Porcentaje
                                   && campoNuevo._ConDecimales == false)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.email:
                                if (campoNuevo.TipoCampo != TipoCampo.email
                                    && campoNuevo.TipoCampo != TipoCampo.AreaTexto)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Fecha:
                                if (campoNuevo.TipoCampo != TipoCampo.Fecha)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Hipervinculo:
                                if (campoNuevo.TipoCampo != TipoCampo.Hipervinculo
                                    && campoNuevo.TipoCampo != TipoCampo.AreaTexto)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Hora:
                                if (campoNuevo.TipoCampo != TipoCampo.Hora
                                    && campoNuevo.TipoCampo != TipoCampo.AreaTexto)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Numerico:
                                if (campoNuevo.TipoCampo != TipoCampo.Numerico
                                     && campoNuevo.TipoCampo != TipoCampo.Porcentaje)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                else if (campoNuevo.TipoCampo == TipoCampo.Porcentaje
                                   && campoNuevo._ConDecimales == true)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Porcentaje:
                                if (campoNuevo.TipoCampo != TipoCampo.Porcentaje
                                    && campoNuevo.TipoCampo != TipoCampo.Numerico
                                    && campoNuevo.TipoCampo != TipoCampo.Decimal
                                    && campoNuevo.TipoCampo != TipoCampo.Dinero)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                else if (campoNuevo.TipoCampo == TipoCampo.Porcentaje
                                    && campoNuevo._ConDecimales != campoViejo._ConDecimales)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                else if (campoNuevo.TipoCampo == TipoCampo.Numerico
                                    && campoViejo._ConDecimales != false)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                else if (campoNuevo.TipoCampo == TipoCampo.Decimal
                                        && campoViejo._ConDecimales != true)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                else if (campoNuevo.TipoCampo == TipoCampo.Dinero
                                       && campoViejo._ConDecimales != true)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Telefono:
                                if (campoNuevo.TipoCampo != TipoCampo.Telefono
                                    && campoNuevo.TipoCampo != TipoCampo.AreaTexto)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Texto:
                                if (campoNuevo.TipoCampo != TipoCampo.Texto
                                    && campoNuevo.TipoCampo != TipoCampo.AreaTexto
                                    && campoNuevo.TipoCampo != TipoCampo.Alfanumerico)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                        }
                        respuesta = ErrorCompatibilidad ? $@"La modificación del campo que selecciono, 
                                                        no es compatible con la información que se encuentra registrada en la base de datos,
                                                        ¿Le gustaria borrar la información existente ({Records} registro(s) existente(s))?" : "";

                    }
                    respuesta += respuesta.Length > 0 ? ", si no quisiera borrar la información seleccione cancelar y cambie los valores para que este sea compatible." : "";
                }

            }
            catch (Exception ex)
            {
                respuesta = "Error al procesar la validacion de la modificación.";
            }
            return respuesta;
        }

        public static string GetInfoOfUpdate(Campo campoViejo, Campo campoNuevo)
        {
            string respuesta = "";
            var ErrorCompatibilidad = false;
            try
            {
                var NombreTabla = GetNombreTabla(campoViejo.PlantillaId, true);
                //var NombreCampo = GetNombreCampo(campoViejo.CampoCatalogoId);
                if (CheckIfExistTableAndColumn(NombreTabla, campoViejo.Nombre))
                {
                    //vemos si tiene registros
                    var Records = GetNumberOfRecords(campoViejo.Nombre, NombreTabla);
                    if (Records != null && Records > 0)
                    {

                        //Verificamos si no se exede de los registros
                        var MaxLenght = GetMMaxLenghOfInformation(campoViejo.Nombre, NombreTabla);
                        if (MaxLenght != null && MaxLenght > campoNuevo.Longitud)
                        {
                            if (campoNuevo.TipoCampo != TipoCampo.ArchivoAdjunto
                               && campoNuevo.TipoCampo != TipoCampo.CasillaVerificacion
                               && campoNuevo.TipoCampo != TipoCampo.Catalogo
                               && campoNuevo.TipoCampo != TipoCampo.Catalogo
                               && campoNuevo.TipoCampo != TipoCampo.Fecha
                               && campoNuevo.TipoCampo != TipoCampo.Hora)
                            {
                                ErrorCompatibilidad = true;
                            }
                        }

                        switch (campoViejo.TipoCampo)
                        {
                            case TipoCampo.Alfanumerico:
                                if (campoNuevo.TipoCampo != TipoCampo.AreaTexto
                                    && campoNuevo.TipoCampo != TipoCampo.Alfanumerico)
                                {
                                    ErrorCompatibilidad = true;
                                }

                                break;
                            case TipoCampo.AreaTexto:
                                if (campoNuevo.TipoCampo != TipoCampo.AreaTexto)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.CasillaVerificacion:
                                if (campoNuevo.TipoCampo != TipoCampo.CasillaVerificacion
                                    && campoNuevo.TipoCampo != TipoCampo.Numerico
                                    && campoNuevo.TipoCampo != TipoCampo.AreaTexto
                                    && campoNuevo.TipoCampo != TipoCampo.Alfanumerico)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Catalogo:
                                if (campoNuevo.TipoCampo != TipoCampo.Catalogo)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Decimal:
                                if (campoNuevo.TipoCampo != TipoCampo.Decimal
                                    && campoNuevo.TipoCampo != TipoCampo.Dinero
                                    && campoNuevo.TipoCampo != TipoCampo.Porcentaje)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                else if (campoNuevo.TipoCampo == TipoCampo.Porcentaje
                                   && campoNuevo._ConDecimales == false)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Dinero:
                                if (campoNuevo.TipoCampo != TipoCampo.Decimal
                                    && campoNuevo.TipoCampo != TipoCampo.Dinero
                                    && campoNuevo.TipoCampo != TipoCampo.Porcentaje)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                else if (campoNuevo.TipoCampo == TipoCampo.Porcentaje
                                   && campoNuevo._ConDecimales == false)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.email:
                                if (campoNuevo.TipoCampo != TipoCampo.email
                                    && campoNuevo.TipoCampo != TipoCampo.AreaTexto)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Fecha:
                                if (campoNuevo.TipoCampo != TipoCampo.Fecha)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Hipervinculo:
                                if (campoNuevo.TipoCampo != TipoCampo.Hipervinculo
                                    && campoNuevo.TipoCampo != TipoCampo.AreaTexto)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Hora:
                                if (campoNuevo.TipoCampo != TipoCampo.Hora
                                    && campoNuevo.TipoCampo != TipoCampo.AreaTexto)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Numerico:
                                if (campoNuevo.TipoCampo != TipoCampo.Numerico
                                     && campoNuevo.TipoCampo != TipoCampo.Porcentaje)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                else if (campoNuevo.TipoCampo == TipoCampo.Porcentaje
                                   && campoNuevo._ConDecimales == true)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Porcentaje:
                                if (campoNuevo.TipoCampo != TipoCampo.Porcentaje
                                    && campoNuevo.TipoCampo != TipoCampo.Numerico
                                    && campoNuevo.TipoCampo != TipoCampo.Decimal
                                    && campoNuevo.TipoCampo != TipoCampo.Dinero)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                else if (campoNuevo.TipoCampo == TipoCampo.Porcentaje
                                    && campoNuevo._ConDecimales != campoViejo._ConDecimales)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                else if (campoNuevo.TipoCampo == TipoCampo.Numerico
                                    && campoViejo._ConDecimales != false)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                else if (campoNuevo.TipoCampo == TipoCampo.Decimal
                                        && campoViejo._ConDecimales != true)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                else if (campoNuevo.TipoCampo == TipoCampo.Dinero
                                       && campoViejo._ConDecimales != true)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Telefono:
                                if (campoNuevo.TipoCampo != TipoCampo.Telefono
                                    && campoNuevo.TipoCampo != TipoCampo.AreaTexto)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                            case TipoCampo.Texto:
                                if (campoNuevo.TipoCampo != TipoCampo.Texto
                                    && campoNuevo.TipoCampo != TipoCampo.AreaTexto
                                    && campoNuevo.TipoCampo != TipoCampo.Alfanumerico)
                                {
                                    ErrorCompatibilidad = true;
                                }
                                break;
                        }
                        respuesta = ErrorCompatibilidad ? $@"La modificación del campo que selecciono, 
                                                        no es compatible con la información que se encuentra registrada en la base de datos,
                                                        ¿Le gustaria borrar la información existente ({Records} registro(s) existente(s))?" : "";

                    }
                    respuesta += respuesta.Length > 0 ? ", si no quisiera borrar la información seleccione cancelar y cambie los valores para que este sea compatible." : "";
                }

            }
            catch (Exception ex)
            {
                respuesta = "Error al procesar la validacion de la modificación.";
            }
            return respuesta;
        }



        public static string GetInfoOfRemove(Campo campo)
        {
            string respuesta = "";
            var ErrorCompatibilidad = false;
            try
            {
                var NombreTabla = GetNombreTabla(campo.PlantillaId, true);
                //var NombreCampo = GetNombreCampo(campoViejo.CampoCatalogoId);
                if (CheckIfExistTableAndColumn(NombreTabla, campo.Nombre))
                {
                    //vemos si tiene registros
                    var Records = GetNumberOfRecords(campo.Nombre, NombreTabla);
                    if (Records != null && Records > 0)
                    {

                        respuesta = $@"La eliminación logica del campo que selecciono afectará a {Records} registro(s) existente(s) que se encuentra en el campo,
                                                        ¿Le gustaria borrar la información existente ({Records} registro(s) existente(s))?";

                    }
                    respuesta += respuesta.Length > 0 ? ", si no quisiera borrar la información seleccione cancelar y cambie los valores o modifique el campo para mantner segura la información." : "";
                }

            }
            catch (Exception ex)
            {
                respuesta = "Error al procesar la validacion de la modificación.";
            }
            return respuesta;
        }
        public static string GetInfoOfRemove(CampoCatalogo campo)
        {
            string respuesta = "";
            try
            {
                var NombreTabla = GetNombreTabla(campo.CatalogoId);
                //var NombreCampo = GetNombreCampo(campoViejo.CampoCatalogoId);
                if (CheckIfExistTableAndColumn(NombreTabla, campo.Nombre))
                {
                    //vemos si tiene registros
                    var Records = GetNumberOfRecords(campo.Nombre, NombreTabla);
                    if (Records != null && Records > 0)
                    {

                        respuesta = $@"La eliminación logica del campo que selecciono afectará a {Records} registro(s) existente(s) que se encuentra en el campo,
                                                        ¿Le gustaria borrar la información existente ({Records} registro(s) existente(s))?";

                    }
                    respuesta += respuesta.Length > 0 ? ", si no quisiera borrar la información seleccione cancelar y cambie los valores o modifique el campo para mantner segura la información." : "";
                }

            }
            catch (Exception ex)
            {
                respuesta = "Error al procesar la validacion de la modificación.";
            }
            return respuesta;
        }

        //cambio borrando datos
        public static string RemoveInformationFromTable(CampoCatalogo campoViejo, CampoCatalogo campoNuevo)
        {
            string respuesta = "";
            var sQuery = "";
            var NombreTabla = GetNombreTabla(campoViejo.CatalogoId);
            var ValorCampo = GetAtributtesForDelete(campoViejo);
            var ValorCampoNuevo = GetAtributtesForDelete(campoNuevo);
            var Atributtes = GetAtributosInput(campoViejo);
            int noOfRowUpdated = -1;
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                var Records = GetNumberOfRecords(campoViejo.Nombre, NombreTabla);
                if (Records != null && Records > 0)
                {
                    //primero quitamos los contrains
                    var ListaContrain = GetContraintsForColumn(NombreTabla, campoViejo.Nombre);
                    if (ListaContrain.Count > 0)
                    {
                        foreach (var item in ListaContrain)
                        {
                            var R = RemoveContrains(item.Table, item.Constraint);
                            if (R.Length > 0)
                            {
                                return R;
                            }
                        }
                    }
                    //ponemos nullos los valores
                    sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoViejo.Nombre} {ValorCampo};";


                    noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                    if (noOfRowUpdated != -1)
                    {
                        return "No se pudo cambiar el valor de la columna.";
                    }
                    //siguiente hacemos el update
                    sQuery = $@"UPDATE { NombreTabla } SET { campoViejo.Nombre } = NULL;";
                    noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                    if (noOfRowUpdated <= 0)
                    {
                        return "No se pudo modificar el valor de la columna.";
                    }

                    //Cambiamos el nombre por el nuevo nombre
                    if (campoViejo.Nombre != campoNuevo.Nombre)
                    {
                        sQuery = $@"EXEC sp_RENAME '{NombreTabla}.{campoViejo.Nombre}' , '{campoNuevo.Nombre}', 'COLUMN'";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated != -1)
                        {
                            return "No se pudo cambiar el valor de la columna.";
                        }

                    }
                    if (campoViejo.TipoCampo != campoNuevo.TipoCampo || campoViejo.Longitud != campoNuevo.Longitud)
                    {
                        sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {ValorCampoNuevo};";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated != -1)
                        {
                            return "No se pudo cambiar el valor de la columna.";
                        }
                    }

                    if (campoNuevo.Requerido)
                    {
                        var defaultValue = GetDefaultValue(campoNuevo);
                        //siguiente hacemos el update
                        sQuery = $@"UPDATE { NombreTabla } SET { campoNuevo.Nombre } = {defaultValue} ;";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated <= 0)
                        {
                            return "No se pudo modificar el valor de la columna.";
                        }
                        var AtributtesNuevo = GetAttributosSinDefault(campoNuevo);
                        sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {AtributtesNuevo};";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated != -1)
                        {
                            return "No se pudo cambiar el valor de la columna.";
                        }

                    }

                }
                else
                {
                    //Cambiamos el nombre por el nuevo nombre
                    if (campoViejo.Nombre != campoNuevo.Nombre)
                    {
                        sQuery = $@"EXEC sp_RENAME '{NombreTabla}.{campoViejo.Nombre}' , '{campoNuevo.Nombre}', 'COLUMN'";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated != -1)
                        {
                            return "No se pudo cambiar el valor de la columna.";
                        }

                    }
                    if (campoViejo.TipoCampo != campoNuevo.TipoCampo || campoViejo.Longitud != campoNuevo.Longitud)
                    {
                        sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {ValorCampoNuevo};";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated != -1)
                        {
                            return "No se pudo cambiar el valor de la columna.";
                        }
                    }

                    if (campoNuevo.Requerido)
                    {


                        var AtributtesNuevo = GetAttributosSinDefault(campoNuevo);
                        sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {AtributtesNuevo};";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated != -1)
                        {
                            return "No se pudo cambiar el valor de la columna.";
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                respuesta = ex.Message;
            }
            return respuesta;
        }

        public static string RemoveInformationFromTable(Campo campoViejo, Campo campoNuevo)
        {
            string respuesta = "";
            var sQuery = "";
            var NombreTabla = GetNombreTabla(campoViejo.PlantillaId, true);
            var ValorCampo = GetAtributtesForDelete(campoViejo);
            var ValorCampoNuevo = GetAtributtesForDelete(campoNuevo);
            var Atributtes = GetAtributosInput(campoViejo);
            int noOfRowUpdated = -1;
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                var Records = GetNumberOfRecords(campoViejo.Nombre, NombreTabla);
                if (Records != null && Records > 0)
                {
                    //primero quitamos los contrains
                    var ListaContrain = GetContraintsForColumn(NombreTabla, campoViejo.Nombre);
                    if (ListaContrain.Count > 0)
                    {
                        foreach (var item in ListaContrain)
                        {
                            var R = RemoveContrains(item.Table, item.Constraint);
                            if (R.Length > 0)
                            {
                                return R;
                            }
                        }
                    }
                    //ponemos nullos los valores
                    sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoViejo.Nombre} {ValorCampo};";


                    noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                    if (noOfRowUpdated != -1)
                    {
                        return "No se pudo cambiar el valor de la columna.";
                    }
                    //siguiente hacemos el update
                    sQuery = $@"UPDATE { NombreTabla } SET { campoViejo.Nombre } = NULL;";
                    noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                    if (noOfRowUpdated <= 0)
                    {
                        return "No se pudo modificar el valor de la columna.";
                    }

                    //Cambiamos el nombre por el nuevo nombre
                    if (campoViejo.Nombre != campoNuevo.Nombre)
                    {
                        sQuery = $@"EXEC sp_RENAME '{NombreTabla}.{campoViejo.Nombre}' , '{campoNuevo.Nombre}', 'COLUMN'";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated != -1)
                        {
                            return "No se pudo cambiar el valor de la columna.";
                        }

                    }
                    if (campoViejo.TipoCampo != campoNuevo.TipoCampo || campoViejo.Longitud != campoNuevo.Longitud)
                    {
                        sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {ValorCampoNuevo};";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated != -1)
                        {
                            return "No se pudo cambiar el valor de la columna.";
                        }
                    }

                    if (campoNuevo.Requerido)
                    {
                        var defaultValue = GetDefaultValue(campoNuevo);
                        //siguiente hacemos el update
                        sQuery = $@"UPDATE { NombreTabla } SET { campoNuevo.Nombre } = {defaultValue} ;";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated <= 0)
                        {
                            return "No se pudo modificar el valor de la columna.";
                        }
                        var AtributtesNuevo = GetAttributosSinDefault(campoNuevo);
                        sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {AtributtesNuevo};";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated != -1)
                        {
                            return "No se pudo cambiar el valor de la columna.";
                        }

                    }

                }
                else
                {
                    //Cambiamos el nombre por el nuevo nombre
                    if (campoViejo.Nombre != campoNuevo.Nombre)
                    {
                        sQuery = $@"EXEC sp_RENAME '{NombreTabla}.{campoViejo.Nombre}' , '{campoNuevo.Nombre}', 'COLUMN'";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated != -1)
                        {
                            return "No se pudo cambiar el valor de la columna.";
                        }

                    }
                    if (campoViejo.TipoCampo != campoNuevo.TipoCampo || campoViejo.Longitud != campoNuevo.Longitud)
                    {
                        sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {ValorCampoNuevo};";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated != -1)
                        {
                            return "No se pudo cambiar el valor de la columna.";
                        }
                    }

                    if (campoNuevo.Requerido)
                    {


                        var AtributtesNuevo = GetAttributosSinDefault(campoNuevo);
                        sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {AtributtesNuevo};";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated != -1)
                        {
                            return "No se pudo cambiar el valor de la columna.";
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                respuesta = ex.Message;
            }
            return respuesta;
        }


        //Cambio sin borrar datos

        public static string ModificarCampoWithoutRemove(CampoCatalogo campoViejo, CampoCatalogo campoNuevo)
        {
            string respuesta = "";
            var sQuery = "";
            var NombreTabla = GetNombreTabla(campoViejo.CatalogoId);
            var ValorCampo = GetAtributtesForDelete(campoViejo);
            var ValorCampoNuevo = GetAtributtesForDelete(campoNuevo);
            var Atributtes = GetAtributosInput(campoViejo);
            int noOfRowUpdated = -1;
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                //verificamos si existe el camppo sino para crear
                if (!CheckIfExistTableAndColumn(NombreTabla, campoViejo.Nombre))
                {
                    var resp = AddColumToTable(NombreTabla, campoNuevo.Nombre, GetAtributosInput(campoNuevo));

                }
                //Cambiamos el nombre por el nuevo nombre
                if (campoViejo.Nombre.ToLower() != campoNuevo.Nombre.ToLower())
                {

                    sQuery = $@"EXEC sp_RENAME '{NombreTabla}.{campoViejo.Nombre}' , '{campoNuevo.Nombre}', 'COLUMN'";
                    noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                    if (noOfRowUpdated != -1)
                    {
                        return "No se pudo cambiar el valor de la columna.";
                    }

                }
                if (campoViejo.TipoCampo != campoNuevo.TipoCampo || campoViejo.Longitud != campoNuevo.Longitud)
                {
                    sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {ValorCampoNuevo};";
                    noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                    if (noOfRowUpdated != -1)
                    {
                        return "No se pudo cambiar el valor de la columna.";
                    }
                }

                if (campoNuevo.Requerido)
                {

                    if (!campoViejo.Requerido)
                    {
                        sQuery = $@"UPDATE { NombreTabla } SET { campoNuevo.Nombre } = {GetDefaultValue(campoNuevo)} WHERE {campoNuevo.Nombre} IS NULL;";
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated < 0)
                        {
                            return "No se pudo modificar el valor de la columna.";
                        }
                    }


                    var AtributtesNuevo = GetAttributosSinDefault(campoNuevo);
                    sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {AtributtesNuevo};";
                    noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                    if (noOfRowUpdated != -1)
                    {
                        return "No se pudo cambiar el valor de la columna.";
                    }

                }

                //var Records = GetNumberOfRecords(campoViejo.Nombre, NombreTabla);

                //if (Records != null && Records > 0)
                //{
                //    //primero quitamos los contrains
                //    //var ListaContrain = GetContraintsForColumn(NombreTabla, campoViejo.Nombre);
                //    //if (ListaContrain.Count > 0)
                //    //{
                //    //    foreach (var item in ListaContrain)
                //    //    {
                //    //        var R = RemoveContrains(item.Table, item.Constraint);
                //    //        if (R.Length > 0)
                //    //        {
                //    //            return R;
                //    //        }
                //    //    }
                //    //}
                //    //ponemos nullos los valores
                //    sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoViejo.Nombre} {ValorCampo};";


                //    noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                //    if (noOfRowUpdated != -1)
                //    {
                //        return "No se pudo cambiar el valor de la columna.";
                //    }
                //    //siguiente hacemos el update
                //    sQuery = $@"UPDATE { NombreTabla } SET { campoViejo.Nombre } = NULL;";
                //    noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                //    if (noOfRowUpdated <= 0)
                //    {
                //        return "No se pudo modificar el valor de la columna.";
                //    }

                //    //Cambiamos el nombre por el nuevo nombre
                //    if (campoViejo.Nombre != campoNuevo.Nombre)
                //    {
                //        sQuery = $@"EXEC sp_RENAME '{NombreTabla}.{campoViejo.Nombre}' , '{campoNuevo.Nombre}', 'COLUMN'";
                //        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                //        if (noOfRowUpdated != -1)
                //        {
                //            return "No se pudo cambiar el valor de la columna.";
                //        }

                //    }
                //    if (campoViejo.TipoCampo != campoNuevo.TipoCampo || campoViejo.Longitud != campoNuevo.Longitud)
                //    {
                //        sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {ValorCampoNuevo};";
                //        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                //        if (noOfRowUpdated != -1)
                //        {
                //            return "No se pudo cambiar el valor de la columna.";
                //        }
                //    }

                //    if (campoNuevo.Requerido)
                //    {
                //        var defaultValue = GetDefaultValue(campoNuevo);
                //        //siguiente hacemos el update
                //        sQuery = $@"UPDATE { NombreTabla } SET { campoNuevo.Nombre } = {defaultValue} ;";
                //        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                //        if (noOfRowUpdated <= 0)
                //        {
                //            return "No se pudo modificar el valor de la columna.";
                //        }
                //        var AtributtesNuevo = GetAttributosSinDefault(campoNuevo);
                //        sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {AtributtesNuevo};";
                //        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                //        if (noOfRowUpdated != -1)
                //        {
                //            return "No se pudo cambiar el valor de la columna.";
                //        }

                //    }

                //}
                //else
                //{

                //}
            }
            catch (Exception ex)
            {
                //return sQuery;
                respuesta = ex.Message;
            }
            return respuesta;
        }
        public static string ModificarCampoWithoutRemove(Campo campoViejo, Campo campoNuevo)
        {
            string respuesta = "";
            var sQuery = "";
            var NombreTabla = GetNombreTabla(campoViejo.PlantillaId, true);
            var ValorCampo = GetAtributtesForDelete(campoViejo);
            var ValorCampoNuevo = GetAtributtesForDelete(campoNuevo);
            var Atributtes = GetAtributosInput(campoViejo);
            int noOfRowUpdated = -1;
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                if (!CheckIfExistTableAndColumn(NombreTabla, campoViejo.Nombre))
                {
                    var resp = AddColumToTable(NombreTabla, campoNuevo.Nombre, GetAtributosInput(campoNuevo));

                }
                //primero quitamos los contrains
                var ListaContrain = GetContraintsForColumn(NombreTabla, campoViejo.Nombre);
                if (ListaContrain.Count > 0)
                {
                    foreach (var item in ListaContrain)
                    {
                        var R = RemoveContrains(item.Table, item.Constraint);
                        if (R.Length > 0)
                        {
                            return R;
                        }
                    }
                }
                //Cambiamos el nombre por el nuevo nombre
                if (campoViejo.Nombre.ToLower().Trim() != campoNuevo.Nombre.ToLower().Trim())
                {
                    sQuery = $@"EXEC sp_RENAME '{NombreTabla}.{campoViejo.Nombre}' , '{campoNuevo.Nombre}', 'COLUMN'";
                    noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                    if (noOfRowUpdated != -1)
                    {
                        return "No se pudo cambiar el valor de la columna.";
                    }

                }
                if (campoViejo.TipoCampo != campoNuevo.TipoCampo || campoViejo.Longitud != campoNuevo.Longitud)
                {
                    sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {ValorCampoNuevo};";
                    noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                    if (noOfRowUpdated != -1)
                    {
                        return "No se pudo cambiar el valor de la columna.";
                    }
                }

                if (campoNuevo.Requerido)
                {

                    if (!campoViejo.Requerido)
                    {
                        sQuery = $@"UPDATE { NombreTabla } SET { campoNuevo.Nombre } = {GetDefaultValue(campoNuevo)} WHERE {campoNuevo.Nombre} IS NULL;";
                        var noce = sQuery;
                        noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                        if (noOfRowUpdated < 0)
                        {
                            return "No se pudo modificar el valor de la columna.";
                        }
                    }


                    var AtributtesNuevo = GetAttributosSinDefault(campoNuevo);
                    sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {AtributtesNuevo};";
                    noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                    if (noOfRowUpdated != -1)
                    {
                        return "No se pudo cambiar el valor de la columna.";
                    }

                }

                //verificamos si no es requerido
                if (!campoNuevo.Requerido && campoViejo.Requerido)
                {
                    var AtributtesNuevo = GetAttributosSinDefault(campoNuevo);
                    sQuery = $@"ALTER TABLE {NombreTabla} ALTER COLUMN {campoNuevo.Nombre} {AtributtesNuevo};";
                    noOfRowUpdated = db.Database.ExecuteSqlCommand(sQuery);
                    if (noOfRowUpdated != -1)
                    {
                        return "No se pudo cambiar el valor de la columna.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta = ex.Message;
            }
            return respuesta;
        }


        public static string RemoveContrains(string Table, string ConstrainName)
        {
            string Respuesta = "";

            var Alter = $@"ALTER TABLE {Table} drop constraint {ConstrainName};";

            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                int noOfRowUpdated = db.Database.ExecuteSqlCommand(Alter);
                if (noOfRowUpdated != -1)
                {
                    Respuesta = "No se pude quitar los constraint de la base de datos";
                }
            }
            catch (Exception ex)
            {
                Respuesta = ex.Message;
            }

            return Respuesta;

        }

        public static List<AuxConstraints> GetContraintsForColumn(string TableName, string ColumnName)
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

        public static string GetAtributtesForDelete(CampoCatalogo campo)
        {
            string sOpciones = "";
            string sRequerido = "NULL";
            string sDecimmantes = campo._ConDecimales == true ? "2" : "0";

            switch (campo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.AreaTexto:
                case TipoCampo.Alfanumerico:
                case TipoCampo.Hora:
                case TipoCampo.email:
                case TipoCampo.Telefono:
                case TipoCampo.Hipervinculo:
                    if (campo.TipoCampo == TipoCampo.Hora)
                    {
                        campo.Longitud = 8;
                    }
                    sOpciones = $@"VARCHAR({campo.Longitud}) { sRequerido }";

                    break;

                case TipoCampo.ArchivoAdjunto:
                    sOpciones = $@"nvarchar(max) { sRequerido } ";
                    break;
                case TipoCampo.Numerico:
                case TipoCampo.Catalogo:
                    sOpciones = $@"INT { sRequerido }";

                    break;
                case TipoCampo.Dinero:
                case TipoCampo.Decimal:
                    sOpciones = $@"decimal({campo.Longitud},2) { sRequerido }";
                    break;
                case TipoCampo.Porcentaje:
                    sOpciones = $@"decimal({campo.Longitud},{sDecimmantes}) { sRequerido }";
                    break;
                case TipoCampo.Fecha:
                    sOpciones = $@"DATE { sRequerido } ";
                    break;
                case TipoCampo.CasillaVerificacion:
                    sOpciones = $@"BIT { sRequerido }";
                    break;
                default:
                    break;
            }

            return sOpciones;
        }

        public static string GetAtributtesForDelete(Campo campo)
        {
            string sOpciones = "";
            string sRequerido = "NULL";
            string sDecimmantes = campo._ConDecimales == true ? "2" : "0";

            switch (campo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.AreaTexto:
                case TipoCampo.Alfanumerico:
                case TipoCampo.Hora:
                case TipoCampo.email:
                case TipoCampo.Telefono:
                case TipoCampo.Hipervinculo:
                    if (campo.TipoCampo == TipoCampo.Hora)
                    {
                        campo.Longitud = 8;
                    }
                    sOpciones = $@"VARCHAR({campo.Longitud}) { sRequerido }";

                    break;

                case TipoCampo.ArchivoAdjunto:
                    sOpciones = $@"nvarchar(max) { sRequerido } ";
                    break;
                case TipoCampo.Numerico:
                case TipoCampo.Catalogo:
                    sOpciones = $@"INT { sRequerido }";

                    break;
                case TipoCampo.Dinero:
                case TipoCampo.Decimal:
                    sOpciones = $@"decimal({campo.Longitud},2) { sRequerido }";
                    break;
                case TipoCampo.Porcentaje:
                    sOpciones = $@"decimal({campo.Longitud},{sDecimmantes}) { sRequerido }";
                    break;
                case TipoCampo.Fecha:
                    sOpciones = $@"DATE { sRequerido } ";
                    break;
                case TipoCampo.CasillaVerificacion:
                    sOpciones = $@"BIT { sRequerido }";
                    break;
                default:
                    break;
            }

            return sOpciones;
        }


        public static string GetAtributosInput(CampoCatalogo campo)
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
        public static string GetAtributosInput(Campo campo)
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

        public static string GetAttributosSinDefault(CampoCatalogo campo)
        {
            string sOpciones = "";
            string sRequerido = campo.Requerido ? "NOT NULL " : "NULL";
            string sDecimmantes = campo._ConDecimales == true ? "2" : "0";
            //string sDefult = "";
            switch (campo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.AreaTexto:
                case TipoCampo.Alfanumerico:
                case TipoCampo.Hora:
                case TipoCampo.email:
                case TipoCampo.Telefono:
                case TipoCampo.Hipervinculo:
                    //sDefult = campo.Requerido ? " DEFAULT '' " : "";
                    if (campo.TipoCampo == TipoCampo.Hora)
                    {
                        campo.Longitud = 8;
                    }
                    sOpciones = $@"VARCHAR({campo.Longitud}) { sRequerido } ";

                    break;

                case TipoCampo.ArchivoAdjunto:
                    //sDefult = campo.Requerido ? " DEFAULT '' " : "";
                    sOpciones = $@"nvarchar(max) { sRequerido }";
                    break;
                case TipoCampo.Numerico:
                case TipoCampo.Catalogo:
                    //sDefult = campo.Requerido ? " DEFAULT 0 " : "";
                    sOpciones = $@"INT { sRequerido }";

                    break;
                case TipoCampo.Dinero:
                case TipoCampo.Decimal:
                    //sDefult = campo.Requerido ? " DEFAULT 0 " : "";
                    sOpciones = $@"decimal({campo.Longitud},2) { sRequerido }";
                    break;
                case TipoCampo.Porcentaje:
                    //sDefult = campo.Requerido ? " DEFAULT 0 " : "";
                    sOpciones = $@"decimal({campo.Longitud},{sDecimmantes}) { sRequerido }";
                    break;
                case TipoCampo.Fecha:
                    //sDefult = campo.Requerido ? " DEFAULT '' " : "";
                    sOpciones = $@"DATE { sRequerido } ";
                    break;
                case TipoCampo.CasillaVerificacion:
                    //sDefult = campo.Requerido ? " DEFAULT 0 " : "";
                    sOpciones = $@"BIT { sRequerido }";
                    break;
                default:
                    break;
            }

            return sOpciones;
        }
        public static string GetAttributosSinDefault(Campo campo)
        {
            string sOpciones = "";
            string sRequerido = campo.Requerido ? "NOT NULL " : "NULL";
            string sDecimmantes = campo._ConDecimales == true ? "2" : "0";
            //string sDefult = "";
            switch (campo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.AreaTexto:
                case TipoCampo.Alfanumerico:
                case TipoCampo.Hora:
                case TipoCampo.email:
                case TipoCampo.Telefono:
                case TipoCampo.Hipervinculo:
                    //sDefult = campo.Requerido ? " DEFAULT '' " : "";
                    if (campo.TipoCampo == TipoCampo.Hora)
                    {
                        campo.Longitud = 8;
                    }
                    sOpciones = $@"VARCHAR({campo.Longitud}) { sRequerido } ";

                    break;

                case TipoCampo.ArchivoAdjunto:
                    //sDefult = campo.Requerido ? " DEFAULT '' " : "";
                    sOpciones = $@"nvarchar(max) { sRequerido }";
                    break;
                case TipoCampo.Numerico:
                case TipoCampo.Catalogo:
                    //sDefult = campo.Requerido ? " DEFAULT 0 " : "";
                    sOpciones = $@"INT { sRequerido }";

                    break;
                case TipoCampo.Dinero:
                case TipoCampo.Decimal:
                    //sDefult = campo.Requerido ? " DEFAULT 0 " : "";
                    sOpciones = $@"decimal({campo.Longitud},2) { sRequerido }";
                    break;
                case TipoCampo.Porcentaje:
                    //sDefult = campo.Requerido ? " DEFAULT 0 " : "";
                    sOpciones = $@"decimal({campo.Longitud},{sDecimmantes}) { sRequerido }";
                    break;
                case TipoCampo.Fecha:
                    //sDefult = campo.Requerido ? " DEFAULT '' " : "";
                    sOpciones = $@"DATE { sRequerido } ";
                    break;
                case TipoCampo.CasillaVerificacion:
                    //sDefult = campo.Requerido ? " DEFAULT 0 " : "";
                    sOpciones = $@"BIT { sRequerido }";
                    break;
                default:
                    break;
            }

            return sOpciones;
        }


        public static string GetDefaultValue(CampoCatalogo campo, bool InsertData = false)
        {
            string sOpciones = "";
            switch (campo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.AreaTexto:
                case TipoCampo.Alfanumerico:
                case TipoCampo.Hora:
                case TipoCampo.email:
                case TipoCampo.Telefono:
                case TipoCampo.Hipervinculo:
                case TipoCampo.ArchivoAdjunto:
                case TipoCampo.Fecha:
                    sOpciones = $@"''";

                    break;
                case TipoCampo.Numerico:
                case TipoCampo.Catalogo:
                case TipoCampo.Dinero:
                case TipoCampo.Decimal:
                case TipoCampo.Porcentaje:
                case TipoCampo.CasillaVerificacion:
                    sOpciones = $@"0";

                    break;
                default:
                    break;
            }

            return sOpciones;
        }

        public static string GetDefaultValue(Campo campo)
        {
            string sOpciones = "";
            switch (campo.TipoCampo)
            {
                case TipoCampo.Texto:
                case TipoCampo.AreaTexto:
                case TipoCampo.Alfanumerico:
                case TipoCampo.Hora:
                case TipoCampo.email:
                case TipoCampo.Telefono:
                case TipoCampo.Hipervinculo:
                case TipoCampo.ArchivoAdjunto:
                case TipoCampo.Fecha:
                    sOpciones = $@"''";

                    break;
                case TipoCampo.Numerico:
                case TipoCampo.Catalogo:
                case TipoCampo.Dinero:
                case TipoCampo.Decimal:
                case TipoCampo.Porcentaje:
                case TipoCampo.CasillaVerificacion:
                    sOpciones = $@"0";

                    break;
                default:
                    break;
            }

            return sOpciones;
        }


        public static int? GetNumberOfRecords(string campo, string nombreTabla)
        {
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                string sQuery = $"SELECT COUNT({campo}) FROM {nombreTabla}";
                return db.Database.SqlQuery<int>(sQuery).FirstOrDefault();

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static int? GetMMaxLenghOfInformation(string campo, string nombreTabla)
        {
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                string sQuery = $"SELECT MAX(LEN({campo})) FROM {nombreTabla};";
                return db.Database.SqlQuery<int>(sQuery).FirstOrDefault();

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static bool CheckIfExistTable(string NombreTabla)
        {
            var bResult = false;
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
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
        public static bool CheckIfExistTableAndColumn(string NombreTabla, string Column)
        {
            var bResult = false;
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                string sQuery = $@"IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{NombreTabla}' AND COLUMN_NAME = '{Column}')
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


        public static bool CehckifCatalogoExist(Campo Campo, string sValor)
        {
            var bResult = false;
            try
            {
                var NombreTabla = GetNombreTablaOCampoCatalago(Campo.CatalogoId);
                var NombreColumna = GetNombreTablaOCampoCatalago(Campo.CatalogoId, true);
                var bLike = false;
                if (Campo.TipoCampo == TipoCampo.Texto || Campo.TipoCampo == TipoCampo.AreaTexto
                    || Campo.TipoCampo == TipoCampo.email || Campo.TipoCampo == TipoCampo.Alfanumerico)
                {
                    bLike = true;
                }
                ApplicationDbContext db = new ApplicationDbContext();
                string sQuery = $@"IF EXISTS(SELECT * FROM {NombreTabla} WHERE {NombreColumna} = {GetValueForSearch(GetTipoCampooCatalago(Campo.CatalogoId), sValor, bLike)} AND Activo=1)
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

        public static bool CehckifCatalogoExist(vmCampoForDatos Campo, string sValor)
        {
            var bResult = false;
            try
            {
                var NombreTabla = GetNombreTablaOCampoCatalago(Campo.CatalogoId);
                var NombreColumna = GetNombreTablaOCampoCatalago(Campo.CatalogoId, true);
                var bLike = false;
                if (Campo.TipoCampo == TipoCampo.Texto || Campo.TipoCampo == TipoCampo.AreaTexto
                    || Campo.TipoCampo == TipoCampo.email || Campo.TipoCampo == TipoCampo.Alfanumerico)
                {
                    bLike = true;
                }
                ApplicationDbContext db = new ApplicationDbContext();
                string sQuery = $@"IF EXISTS(SELECT * FROM {NombreTabla} WHERE {NombreColumna} = {GetValueForSearch(GetTipoCampooCatalago(Campo.CatalogoId), sValor, bLike)} AND Activo=1)
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
        public static bool CehckifCatalogoExist(CampoCatalogo Campo, string sValor)
        {
            var bResult = false;
            try
            {
                var NombreTabla = GetNombreTablaOCampoCatalago(Campo.iCatalogoId);
                var NombreColumna = GetNombreTablaOCampoCatalago(Campo.iCatalogoId, true);
                var bLike = false;
                if (Campo.TipoCampo == TipoCampo.Texto || Campo.TipoCampo == TipoCampo.AreaTexto
                    || Campo.TipoCampo == TipoCampo.email || Campo.TipoCampo == TipoCampo.Alfanumerico)
                {
                    bLike = true;
                }
                ApplicationDbContext db = new ApplicationDbContext();
                string sQuery = $@"IF EXISTS(SELECT * FROM {NombreTabla} WHERE {NombreColumna} = {GetValueForSearch(GetTipoCampooCatalago(Campo.iCatalogoId), sValor, bLike)} AND Activo=1)
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
        public static int GetIdFromCatalogo(Campo Campo, string sValor)
        {
            var bResult = 0;
            try
            {
                var NombreTabla = GetNombreTablaOCampoCatalago(Campo.CatalogoId);
                var NombreColumna = GetNombreTablaOCampoCatalago(Campo.CatalogoId, true);
                var bLike = false;
                if (Campo.TipoCampo == TipoCampo.Texto || Campo.TipoCampo == TipoCampo.AreaTexto
                    || Campo.TipoCampo == TipoCampo.email || Campo.TipoCampo == TipoCampo.Alfanumerico)
                {
                    bLike = true;
                }
                ApplicationDbContext db = new ApplicationDbContext();
                //string sQuery = $@"SELECT TablaFisicaId FROM {NombreTabla} WHERE {NombreColumna} = 'asdsadsada' AND Activo=1";

                string sQuery = $@"SELECT TablaFisicaId FROM {NombreTabla} WHERE {NombreColumna} = {GetValueForSearch(GetTipoCampooCatalago(Campo.CatalogoId), sValor, bLike)} AND Activo=1";
                bResult = db.Database.SqlQuery<int>(sQuery).FirstOrDefault();

            }
            catch (Exception ex)
            {

            }

            return bResult;
        }

        public static int GetIdFromCatalogo(CampoCatalogo Campo, string sValor)
        {
            var bResult = 0;
            try
            {
                var NombreTabla = GetNombreTablaOCampoCatalago(Campo.iCatalogoId);
                var NombreColumna = GetNombreTablaOCampoCatalago(Campo.iCatalogoId, true);
                var bLike = false;
                if (Campo.TipoCampo == TipoCampo.Texto || Campo.TipoCampo == TipoCampo.AreaTexto
                    || Campo.TipoCampo == TipoCampo.email || Campo.TipoCampo == TipoCampo.Alfanumerico)
                {
                    bLike = true;
                }
                ApplicationDbContext db = new ApplicationDbContext();
                //string sQuery = $@"SELECT TablaFisicaId FROM {NombreTabla} WHERE {NombreColumna} = 'asdsadsada' AND Activo=1";

                string sQuery = $@"SELECT TablaFisicaId FROM {NombreTabla} WHERE {NombreColumna} = {GetValueForSearch(GetTipoCampooCatalago(Campo.iCatalogoId), sValor, bLike)} AND Activo=1";
                bResult = db.Database.SqlQuery<int>(sQuery).FirstOrDefault();

            }
            catch (Exception ex)
            {

            }

            return bResult;
        }

        public static string GetValueForSearch(TipoCampo tipoCampo, string valor, bool bLike = false)
        {
            var respuesta = "";
            try
            {
                switch (tipoCampo)
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

                        break;
                    case TipoCampo.Numerico:
                    case TipoCampo.Catalogo:
                    case TipoCampo.Dinero:
                    case TipoCampo.Decimal:
                    case TipoCampo.Porcentaje:
                    case TipoCampo.CasillaVerificacion:
                        respuesta = $@"{ valor }";
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


        public static string modificarCampoErroneoPlantillas(int campoId)
        {
            string respuesta = "";
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                var campo = db.Campos.Where(x => x.CampoId == campoId).FirstOrDefault();
                if (campo != null)
                {
                    var campoNombre = campo.Nombre;
                    campo.Nombre = GenerarNombreCampos(campoNombre);
                    db.Entry(campo).State = EntityState.Modified;
                    db.SaveChanges();
                }



            }
            catch (Exception ex)
            {
                respuesta = ex.Message;
            }

            return respuesta = "";
        }

        public static string modificarCampoErroneoCatalogo(int campoId)
        {
            string respuesta = "";
            ApplicationDbContext db = new ApplicationDbContext();
            try
            {
                var campo = db.CampoCatalogo.Where(x => x.CampoCatalogoId == campoId).FirstOrDefault();
                if (campo != null)
                {
                    var campoNombre = campo.Nombre;
                    campo.Nombre = GenerarNombreCampos(campoNombre);
                    db.Entry(campo).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                respuesta = ex.Message;
            }

            return respuesta = "";
        }

        public static string GenerarNombreCampos(string nombre = "")
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

        public static string AddColumToTable(string Table, string ColumName, string sOpciones)
        {
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


        public static string PeriodoDesdeToString(this HtmlHelper helper, DateTime? PeriodoDesde)
        {
            return PeriodoDesde.HasValue ? PeriodoDesde.Value.ToString("dd", CultureInfo.CreateSpecificCulture("es-MX")) + " de " +
                   PeriodoDesde.Value.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-MX")) + " del " +
                   PeriodoDesde.Value.ToString("yyyy", CultureInfo.CreateSpecificCulture("es-MX")) : "";
        }

        public static string PeriodoHastaToString(this HtmlHelper helper, DateTime? PeriodoHasta, DateTime? PeriodoDesde)
        {
            return PeriodoHasta.HasValue ? "hasta " + PeriodoHasta.Value.ToString("dd", CultureInfo.CreateSpecificCulture("es-MX")) + " de " +
                PeriodoHasta.Value.ToString("MMMM", CultureInfo.CreateSpecificCulture("es-MX")) + " del " +
                PeriodoHasta.Value.ToString("yyyy", CultureInfo.CreateSpecificCulture("es-MX")) : PeriodoDesde.HasValue ? " en adelante" : "No registrado.";
        }



        public static TypeBuilder CreateTypeBuilder(
          string assemblyName, string moduleName, string typeName)
        {
            TypeBuilder typeBuilder = AppDomain
                .CurrentDomain
                .DefineDynamicAssembly(new AssemblyName(assemblyName),
                                       AssemblyBuilderAccess.Run)
                .DefineDynamicModule(moduleName)
                .DefineType(typeName, TypeAttributes.Public);
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            return typeBuilder;
        }

        public static void CreateAutoImplementedProperty(
            TypeBuilder builder, string propertyName, Type propertyType)
        {
            const string PrivateFieldPrefix = "m_";
            const string GetterPrefix = "get_";
            const string SetterPrefix = "set_";

            // Generate the field.
            FieldBuilder fieldBuilder = builder.DefineField(
                string.Concat(PrivateFieldPrefix, propertyName),
                              propertyType, FieldAttributes.Private);

            // Generate the property
            PropertyBuilder propertyBuilder = builder.DefineProperty(
                propertyName, System.Reflection.PropertyAttributes.HasDefault, propertyType, null);

            // Property getter and setter attributes.
            MethodAttributes propertyMethodAttributes =
                MethodAttributes.Public | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig;

            // Define the getter method.
            MethodBuilder getterMethod = builder.DefineMethod(
                string.Concat(GetterPrefix, propertyName),
                propertyMethodAttributes, propertyType, Type.EmptyTypes);

            // Emit the IL code.
            // ldarg.0
            // ldfld,_field
            // ret
            ILGenerator getterILCode = getterMethod.GetILGenerator();
            getterILCode.Emit(OpCodes.Ldarg_0);
            getterILCode.Emit(OpCodes.Ldfld, fieldBuilder);
            getterILCode.Emit(OpCodes.Ret);

            // Define the setter method.
            MethodBuilder setterMethod = builder.DefineMethod(
                string.Concat(SetterPrefix, propertyName),
                propertyMethodAttributes, null, new Type[] { propertyType });

            // Emit the IL code.
            // ldarg.0
            // ldarg.1
            // stfld,_field
            // ret
            ILGenerator setterILCode = setterMethod.GetILGenerator();
            setterILCode.Emit(OpCodes.Ldarg_0);
            setterILCode.Emit(OpCodes.Ldarg_1);
            setterILCode.Emit(OpCodes.Stfld, fieldBuilder);
            setterILCode.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterMethod);
            propertyBuilder.SetSetMethod(setterMethod);
        }

        public static IEnumerable<dynamic> DynamicListFromSql(this DbContext db, string Sql, Dictionary<string, object> Params)
        {
            using (var cmd = db.Database.Connection.CreateCommand())
            {
                cmd.CommandText = Sql;
                if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }

                foreach (KeyValuePair<string, object> p in Params)
                {
                    DbParameter dbParameter = cmd.CreateParameter();
                    dbParameter.ParameterName = p.Key;
                    dbParameter.Value = p.Value;
                    cmd.Parameters.Add(dbParameter);
                }

                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var row = new ExpandoObject() as IDictionary<string, object>;
                        for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                        {
                            row.Add(dataReader.GetName(fieldCount), dataReader[fieldCount]);
                        }
                        yield return row;
                    }
                }
            }
        }
        public static IEnumerable<dynamic> DynamicListFromSql(DbCommand cmd)
        {
            using (var dataReader = cmd.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    var row = new ExpandoObject() as IDictionary<string, object>;
                    for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                    {
                        row.Add(dataReader.GetName(fieldCount), dataReader[fieldCount]);
                    }
                    yield return row;
                }
            }

        }

        public static string getNombreUsuarioById(string sId = "")
        {
            string Nombre = "";

            try
            {
                if (sId == "")
                {
                    return "No registrado";
                }
                ApplicationDbContext db = new ApplicationDbContext();
                var usuario = db.Users.Where(x => x.Id == sId).FirstOrDefault();
                Nombre = usuario != null ? usuario.NombreCompleto : "";

            }
            catch (Exception ex)
            {

            }

            return Nombre;

        }

        public static string FormatDataFromRazor(TipoCampo tipoCampo, bool? ConDecimal, int iCatalogoId, object Value, bool forDownload = true, bool Tipo = false)
        {
            string Respuesta = "";

            try
            {
                switch (tipoCampo)
                {
                    case TipoCampo.Texto:
                        Respuesta = Value.ToString();
                        break;
                    case TipoCampo.AreaTexto:
                        Respuesta = ShortDescripcion(Value.ToString());
                        break;
                    case TipoCampo.Numerico:
                        if (Tipo)
                        {
                            Respuesta = Value.ToString();
                        }
                        else
                        {
                            Respuesta = Convert.ToInt32(Value).ToString("#,##0");
                        }
                        break;
                    case TipoCampo.Alfanumerico:
                        Respuesta = Value.ToString();
                        break;
                    case TipoCampo.Dinero:
                        if (Tipo)
                        {
                            Respuesta = Value.ToString();
                        }
                        else
                        {
                            Respuesta = $"${Convert.ToDecimal(Value).ToString("#,##0.00")}";
                        }
                        break;
                    case TipoCampo.Porcentaje:
                        if (Tipo)
                        {
                            if (ConDecimal == true)
                            {
                                Respuesta = $"{Convert.ToDecimal(Value).ToString("#0.00")}";
                            }
                            else
                            {
                                Respuesta = $"{ Convert.ToInt32(Value).ToString("#0")}";

                            }
                        }
                        else
                        {
                            if (ConDecimal == true)
                            {
                                Respuesta = $"{Convert.ToDecimal(Value).ToString("#,##0.00")}%";
                            }
                            else
                            {
                                Respuesta = $"{ Convert.ToInt32(Value).ToString("#,##0")}%";

                            }
                        }


                        break;
                    case TipoCampo.Decimal:
                        if (Tipo)
                        {
                            Respuesta = $"{Convert.ToDecimal(Value).ToString("#0.00")}";
                        }
                        else
                        {
                            Respuesta = $"{Convert.ToDecimal(Value).ToString("#,##0.00")}";
                        }

                        break;
                    case TipoCampo.Fecha:
                        if (Tipo)
                        {
                            Respuesta = Value.ToString();
                        }
                        else
                        {
                            Respuesta = GetFormatForSelect(TipoCampo.Fecha, Value.ToString());
                        }


                        //Respuesta = Convert.ToString(Value);
                        break;
                    case TipoCampo.Hora:
                        Respuesta = Value.ToString();
                        break;
                    case TipoCampo.Hipervinculo:
                        // var Link = "<i class='fa fa-link'>"+ Value.ToString() +"</i>";
                        if (forDownload)
                        {
                            Respuesta = $@"{Value.ToString()}";
                        }
                        else
                        {
                            Respuesta = $@"<a href='{Value.ToString()}' target='_blank' >{Value.ToString()}</a>";
                        }

                        break;
                    case TipoCampo.email:
                        Respuesta = Value.ToString();
                        break;
                    case TipoCampo.Telefono:
                        Respuesta = Value.ToString();
                        break;
                    case TipoCampo.ArchivoAdjunto:
                        Respuesta = Value.ToString();
                        if (forDownload)
                        {
                            Respuesta = $@"{ Value.ToString() }";
                        }
                        else
                        {
                            Respuesta = $@"<a href='{ Value.ToString() }' class='btn btn-info btn-circle'><i class='fa fa-paperclip'></i></a>";
                        }
                        break;
                    case TipoCampo.Catalogo:
                        if (Tipo)
                        {
                            Respuesta = Value.ToString();
                        }
                        else
                        {
                            Respuesta = GetNameCatalogo(Convert.ToInt32(Value), iCatalogoId);
                        }

                        break;
                    case TipoCampo.CasillaVerificacion:

                        if (Tipo)
                        {
                            Respuesta = Value.ToString();
                        }
                        else
                        {
                            if (forDownload)
                            {
                                if (Convert.ToBoolean(Value))
                                {
                                    Respuesta = $@"Activo";
                                }
                                else
                                {
                                    Respuesta = $@"Inactivo";
                                }
                            }
                            else
                            {
                                if (Convert.ToBoolean(Value))
                                {
                                    Respuesta = $@"<span class='label label-primary'>Activo</span>";
                                }
                                else
                                {
                                    Respuesta = $@"<span class='label label-danger'>Inactivo</span>";
                                }
                            }
                        }


                        break;
                }

            }
            catch (Exception ex)
            {
                Respuesta = "-";
            }

            return Respuesta;

        }

        public static string FormatDataBitacora(TipoCampo tipoCampo, bool? ConDecimal, int iCatalogoId, object Value)
        {
            string Respuesta = "";

            try
            {
                switch (tipoCampo)
                {
                    case TipoCampo.Texto:
                        Respuesta = Value.ToString();
                        break;
                    case TipoCampo.AreaTexto:
                        Respuesta = ShortDescripcion(Value.ToString());
                        break;
                    case TipoCampo.Numerico:
                        Respuesta = Convert.ToInt32(Value).ToString("#,##0");
                        break;
                    case TipoCampo.Alfanumerico:
                        Respuesta = Value.ToString();
                        break;
                    case TipoCampo.Dinero:
                        Respuesta = $"${Convert.ToDecimal(Value).ToString("#,##0.00")}";
                        break;
                    case TipoCampo.Porcentaje:
                        if (ConDecimal == true)
                        {
                            Respuesta = $"{Convert.ToDecimal(Value).ToString("#,##0.00")}%";
                        }
                        else
                        {
                            Respuesta = $"{ Convert.ToInt32(Value).ToString("#,##0")}%";
                        }
                        break;
                    case TipoCampo.Decimal:

                        Respuesta = $"{Convert.ToDecimal(Value).ToString("#,##0.00")}";


                        break;
                    case TipoCampo.Fecha:

                        Respuesta = GetFormatForSelect(TipoCampo.Fecha, Value.ToString());


                        //Respuesta = Convert.ToString(Value);
                        break;
                    case TipoCampo.Hora:
                        Respuesta = Value.ToString();
                        break;
                    case TipoCampo.Hipervinculo:
                        // var Link = "<i class='fa fa-link'>"+ Value.ToString() +"</i>";

                        Respuesta = $@"{Value.ToString()}";

                        //Respuesta = $@"<a href='{Value.ToString()}' target='_blank' >{Value.ToString()}</a>";


                        break;
                    case TipoCampo.email:
                        Respuesta = Value.ToString();
                        break;
                    case TipoCampo.Telefono:
                        Respuesta = Value.ToString();
                        break;
                    case TipoCampo.ArchivoAdjunto:
                        Respuesta = Value.ToString();
                        //Respuesta = $@"{ Value.ToString() }";
                        // Respuesta = $@"<a href='{ Value.ToString() }' class='btn btn-info btn-circle'><i class='fa fa-paperclip'></i></a>";

                        break;
                    case TipoCampo.Catalogo:
                        if (Value.ToString() != "0")
                        {
                            Respuesta = $"({Value.ToString()}) " + GetNameCatalogo(Convert.ToInt32(Value), iCatalogoId);
                        }
                        else
                        {
                            Respuesta = "";
                        }


                        break;
                    case TipoCampo.CasillaVerificacion:

                        if (Convert.ToInt32(Value) == 1)
                        {
                            Respuesta = $@"Activo";
                        }
                        else
                        {
                            Respuesta = $@"Inactivo";
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                Respuesta = "-";
            }

            return Respuesta;

        }
        public static string getStringBoolBitacora(bool activo)
        {
            try
            {
                return activo ? "Activo" : "Inactivo";
            }
            catch (Exception ex)
            {

            }
            return "No disponible";
        }
        public static string ShortDescripcion(string Valor = "")
        {
            if (Valor == "")
                return Valor;
            var WithOutHml = Regex.Replace(Valor, "<.*?>", String.Empty);
            var sLenght = WithOutHml.Length;
            return WithOutHml.Substring(0, (sLenght > 20 ? 20 : sLenght)) + (sLenght > 20 ? "..." : "");

        }

        //nuevos modulos
        public static List<ListaDescargar> getListadoToDownload(int TotalItems = 0)
        {
            var listaDescargar = new List<ListaDescargar>();
            try
            {
                var Inicial = 1;
                var multiplos = 1000;
                var partes = multiplos;

                if (TotalItems == 0)
                {
                    listaDescargar.Add(new ListaDescargar { Nombre = "0", Valor = "0" });
                    return listaDescargar;
                }

                if (partes >= TotalItems)
                {
                    listaDescargar.Add(new ListaDescargar { Nombre = $"1 - {TotalItems.ToString()}", Valor = $"1,{TotalItems}" });
                    return listaDescargar;
                }
                while (TotalItems >= partes)
                {
                    listaDescargar.Add(new ListaDescargar { Nombre = $"{Inicial} - {partes.ToString()}", Valor = $"{Inicial},{partes}" });
                    Inicial += multiplos;
                    partes += multiplos;
                }
                listaDescargar.Add(new ListaDescargar { Nombre = $"{Inicial} - {TotalItems.ToString()}", Valor = $"{Inicial},{TotalItems}" });
                return listaDescargar;

            }
            catch (Exception ex)
            {
                listaDescargar.Add(new ListaDescargar { Nombre = "0", Valor = "0" });
                return listaDescargar;

            }

        }

        public static int getTotalRowsPlantilla(string NombreTabla, int iOrganismoId, string sUsuarioId)
        {
            var respuesta = 0;
            try
            {

                ApplicationDbContext db = new ApplicationDbContext();
                string sQuery = $@"SELECT COUNT(TablaFisicaId) FROM {NombreTabla} WHERE OrganismoId={iOrganismoId} AND UsuarioId='{sUsuarioId}' AND Activo=1";
                respuesta = db.Database.SqlQuery<int>(sQuery).FirstOrDefault();

            }
            catch (Exception ex)
            {

            }

            return respuesta;
        }
        public static int getTotalRowsPlantilla(string NombreTabla, int iOrganismoId)
        {
            var respuesta = 0;
            try
            {

                ApplicationDbContext db = new ApplicationDbContext();
                string sQuery = $@"SELECT COUNT(TablaFisicaId) FROM {NombreTabla} WHERE OrganismoId={iOrganismoId} AND Activo=1";
                respuesta = db.Database.SqlQuery<int>(sQuery).FirstOrDefault();

            }
            catch (Exception ex)
            {

            }

            return respuesta;
        }

        public static int getTotalRowsPlantilla(string NombreTabla, int iOrganismoId, int Idregistro)
        {
            var respuesta = 0;
            try
            {

                ApplicationDbContext db = new ApplicationDbContext();
                string sQuery = $@"SELECT COUNT(TablaFisicaId) FROM {NombreTabla} WHERE Idregistro = {Idregistro} AND OrganismoId={iOrganismoId}";
                respuesta = db.Database.SqlQuery<int>(sQuery).FirstOrDefault();

            }
            catch (Exception ex)
            {

            }

            return respuesta;
        }



        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));

            return expando as ExpandoObject;
        }

        public static string Bitacora(List<cambiosCampos> cambios, string tabla = "", string tabla_publico = "", int registro_id = 0, string usuario_id = "", int accion = 1)
        {

            string respuesta = "";
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                var bitacora = new Bitacora();
                bitacora.cambios = JsonConvert.SerializeObject(cambios);
                bitacora.tabla = tabla;
                bitacora.tablaPublico = tabla_publico;
                bitacora.registroId = registro_id;
                switch (accion)
                {
                    case 1:
                        bitacora.nuevo = true;
                        break;
                    case 2:
                        bitacora.editar = true;
                        break;
                    case 3:
                        bitacora.eliminar = true;
                        break;
                    default:
                        bitacora.otro = true;
                        break;
                }
                bitacora.fechaCreacion = DateTime.Now;
                bitacora.usuarioId = usuario_id;
                db.Bitacora.Add(bitacora);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                respuesta = "Error en Bitacora: " + ex.Message;

            }

            return respuesta;
        }

        public static string Bitacora(List<List<cambiosCampos>> cambios, string tabla = "", string tabla_publico = "", int registro_id = 0, string usuario_id = "", int accion = 1)
        {

            string respuesta = "";
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();

                foreach (var item in cambios)
                {
                    var bitacora = new Bitacora();
                    bitacora.cambios = JsonConvert.SerializeObject(item);
                    bitacora.tabla = tabla;
                    bitacora.tablaPublico = tabla_publico;
                    bitacora.registroId = registro_id;
                    switch (accion)
                    {
                        case 1:
                            bitacora.nuevo = true;
                            break;
                        case 2:
                            bitacora.editar = true;
                            break;
                        case 3:
                            bitacora.eliminar = true;
                            break;
                        default:
                            bitacora.otro = true;
                            break;
                    }
                    bitacora.fechaCreacion = DateTime.Now;
                    bitacora.usuarioId = usuario_id;
                    db.Bitacora.Add(bitacora);
                    db.SaveChanges();

                }

            }
            catch (Exception ex)
            {
                respuesta = "Error en Bitacora: " + ex.Message;

            }

            return respuesta;
        }

        public static string FechaActualizacion(int iPlantillaId)
        {

            string respuesta = "";
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();

                var plantillas = db.Plantillas.Where(x => x.PlantillaId == iPlantillaId).FirstOrDefault();
                if (plantillas != null)
                {
                    plantillas.FechaModificacion = DateTime.Now;
                    db.Entry(plantillas).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                respuesta = "Error en fecha actualizacion: " + ex.Message;

            }

            return respuesta;
        }


        public static vmPlantillaCampos GetCamposForDatos(int iPlantillaId = 0, bool bRelevante = false)
        {
            vmPlantillaCampos result = new vmPlantillaCampos();
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                var sQuery = $@"spr_portal_consulta_catalogo_campo_relevante @PlantillaId, @Relevante";
                SqlMapper.AddTypeHandler(new vmCampoForDatosHandler());


                using (IDbConnection idb = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    result = idb.Query<vmPlantillaCampos>(
                        sQuery,
                        new { PlantillaId = iPlantillaId, Relevante = bRelevante })
                        .FirstOrDefault();
                    SqlMapper.ResetTypeHandlers();
                }
            } 
            catch (Exception ex)
            {
                //result.NombreTabla = ex.Message;
            }
            return result;
        }

        public static vmPlantillaCampos GetCamposForExcel(int iPlantillaId = 0)
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

        public static vmCatalagoExcel GetCamposCatalagoForExcel(int iCatalagoId = 0)
        {
            vmCatalagoExcel result = new vmCatalagoExcel();
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                var sQuery = $@"spr_portal_consulta_campos_tabla_excel @CatalagoId";
                SqlMapper.AddTypeHandler(new camposCatalagolaHandler());
                //SqlMapper.AddTypeHandler(new camposTablaHandler());


                using (IDbConnection idb = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    result = idb.Query<vmCatalagoExcel>(
                        sQuery,
                        new { CatalagoId = iCatalagoId })
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



        //public static string getUserName()
        //{
        //    string nombre = "";
        //    try
        //    {
        //        ApplicationDbContext db = new ApplicationDbContext();
        //        nombre = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault()?.NombreCompleto;
        //    }
        //    catch(Exception ex)
        //    {

        //    }

        //    return nombre;
        //}








        #endregion
    }
}
