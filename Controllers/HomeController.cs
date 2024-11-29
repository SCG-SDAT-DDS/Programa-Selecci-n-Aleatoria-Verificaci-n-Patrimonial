using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Transparencia.Models;

namespace Transparencia.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            if (TempData["MensajePermiso"] != null)
            {
                return View();
            }
            var Enlace = HMTLHelperExtensions.GetRoles(User.Identity.Name, "Enlace");
            var UnidadAdministrativa = HMTLHelperExtensions.GetRoles(User.Identity.Name, "Unidad Administrativa");
            if (Enlace)
                return RedirectToAction("Index", "Plantillas");
            if(UnidadAdministrativa)
                return RedirectToAction("IndexPlantillas", "Plantillas");
            return View();
        }

    }
}