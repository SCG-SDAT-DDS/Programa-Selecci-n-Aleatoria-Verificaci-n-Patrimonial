using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Transparencia.Helpers;
using Transparencia.Models;

namespace Transparencia.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // No cuenta los errores de inicio de sesión para el bloqueo de la cuenta
            // Para permitir que los errores de contraseña desencadenen el bloqueo de la cuenta, cambie a shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Usuario, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    if(UserManager.FindByName(model.Usuario)?.Activo == false)
                    {
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        ModelState.AddModelError("error", "Por favor verifique que el usuario y contraseña sean los correctos, gracias.");
                        return View(model);
                    }
                    var id = UserManager.FindByName(model.Usuario)?.Id;
                    BitIniSesion(id);
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                    ModelState.AddModelError("error", "Por favor verifique que el usuario y contraseña sean los correctos, gracias.");
                    return View(model);
                default:
                    ModelState.AddModelError("", "Intento de inicio de sesión no válido.");
                    return View(model);
            }
        }
        
        // GET: /Account/Register
        //[AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        //[AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                    // Enviar correo electrónico con este vínculo
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirmar cuenta", "Para confirmar la cuenta, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // No revelar que el usuario no existe o que no está confirmado
                    return View("ForgotPasswordConfirmation");
                }

                // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                // Enviar correo electrónico con este vínculo
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Restablecer contraseña", "Para restablecer la contraseña, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // No revelar que el usuario no existe
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        
        //
        // POST: /Account/LogOff
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult LogOff()
        //{
        //    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        //    return RedirectToAction("Index", "Home");
        //}
        [AllowAnonymous]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }


        //[AllowAnonymous]
        //public ActionResult RecoverPassword(string sUsername = "")
        //{
        //    string sResponse = "";
        //    try
        //    {
        //        ApplicationDbContext db = new ApplicationDbContext();
        //        var tblUser = db.Users.Where(x=>x.UserName== sUsername).FirstOrDefault();
        //        if (tblUser == null)
        //        {
        //            return Json("El usuario no existe.", JsonRequestBehavior.AllowGet);
        //        }

        //        string sEmailSubject = "Recuperar contraseña";
        //        DateTime dtHoy = DateTime.Now;
        //        var vDatosCorreo = new AuxRecuperarContrasena(tblUser.NombreCompleto, dtHoy, tblUser.Email, SecurityPassword.Decrypt(tblUser.PasswordHash));
        //        string sMessageBody = ConvertViewToString("_RecuperarContrasenaTemplate", vDatosCorreo);

        //        sResponse = EnviarCorreo(sEmailSubject, sMessageBody, tblUser.Email);
        //    }
        //    catch (Exception ex)
        //    {
        //        sResponse = ex.Message;
        //    }
        //    return Json(sResponse, JsonRequestBehavior.AllowGet);
        //}

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


        //recuperar contraseña
        [AllowAnonymous]
        public ActionResult RecuperarContrasena(string key = "")
        {
            // string sResponse = "";
            try
            {

                if (key == "")
                {
                    return RedirectToAction("Login");
                }
                ApplicationDbContext db = new ApplicationDbContext();
                var tblUser = db.Users.Where(x => x.claveRecuperarPassword == key && x.Activo).FirstOrDefault();
                if (tblUser == null)
                {
                    return RedirectToAction("Login");
                }
                RecuperarContrasenaViewModel recoberyUser = new RecuperarContrasenaViewModel();
                recoberyUser.UserId = tblUser.Id;
                recoberyUser.Key = tblUser.claveRecuperarPassword;
                recoberyUser.Iformacion = "Hola <b>" + tblUser.NombreCompleto + "</b>, Por favor escriba la nueva contraseña en los campos de texto y acontinuación presione \"Cambiar contraseña\" ";
                
                return View(recoberyUser);

            }
            catch (Exception ex)
            {
                return RedirectToAction("Login");
            }
        }

        [AllowAnonymous]
        public ActionResult RecoverPassword(string sUsername = "")
        {
            string sResponse = "";
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                var tblUser = db.Users.Where(x => x.UserName == sUsername && x.Activo).FirstOrDefault();
                if (tblUser == null)
                {
                    return Json("No existe ninguna coincidencia con el usuario proporcionado", JsonRequestBehavior.AllowGet);
                }

                //var userManagerx = HttpContext.GetOwinContext().GetUserManager<ApplicationUser>();
                //var _UserManager = new ApplicationUser(new UserStore<ApplicationUser>(HttpContext.Get<ApplicationDbContext>()));
                string sEmailSubject = "Recuperación de contraseña";
                DateTime dtHoy = DateTime.Now;
                var key = RandomString(10);
                //var key = UserManager.GeneratePasswordResetToken(tblUser.Id);
                string url = Url.Action("RecuperarContrasena", "Account", new { key = key }, protocol: Request.Url.Scheme);
                var vDatosCorreo = new AuxRecuperarContrasena(tblUser.NombreCompleto, dtHoy, tblUser.Email, key, url);
                if (vDatosCorreo.Contrasena.Length > 0)
                {
                    tblUser.claveRecuperarPassword = vDatosCorreo.Contrasena;
                    db.Entry(tblUser).State = EntityState.Modified;
                    db.SaveChanges();
                }
                string sMessageBody = ConvertViewToString("_RecuperarContrasenaTemplate", vDatosCorreo);
                sResponse = HMTLHelperExtensions.EnviarCorreo(sEmailSubject, sMessageBody, tblUser.Email);
            }
            catch (Exception ex)
            {
                sResponse = ex.Message;
            }
            return Json(sResponse, JsonRequestBehavior.AllowGet);
        }

        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        [AllowAnonymous]
        [HttpPost]
        public ActionResult RecuperarContrasena(RecuperarContrasenaViewModel model)
        {
            // string sResponse = "";
            try
            {
                if (model == null)
                {
                    return RedirectToAction("Login");
                }
                if (ModelState.IsValid)
                {
                    ApplicationDbContext db = new ApplicationDbContext();
                    var tblUser = db.Users.Where(x => x.Id == model.UserId && x.Activo).FirstOrDefault();
                    if (tblUser == null)
                    {
                        return RedirectToAction("Login");
                    }
                    //var result = UserManager.ResetPassword(tblUser.Id, model.Key, model.NewPassword);
                    UserManager<IdentityUser> userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>());
                    var reset = userManager.RemovePassword(tblUser.Id);
                    //reset =  userManager.AddPassword(tblUser.Id, model.NewPassword);
                    tblUser.PasswordHash = UserManager.PasswordHasher.HashPassword(model.NewPassword);
                    try
                    {
                        tblUser.claveRecuperarPassword = "";
                        db.Entry(tblUser).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("error", ex.Message);
                    }

                    return RedirectToAction("Login");

                    //var errors = new IdentityResult(new string{ "Ocurrio   un   error al moento de procesar la solicitud" });
                    //AddErrors();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("error", ex.Message);
                return View();
            }
        }
        private void CreateRol(string NombreRole)
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));


            if (!roleManager.RoleExists(NombreRole))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = NombreRole;
                roleManager.Create(role);

            }
        }

        #region BitIniSesion
        private void BitIniSesion(string userId)
        {
            try
            {
                ApplicationDbContext db = new ApplicationDbContext();
                var browser = Request.Browser;
                var userAgent = Request.UserAgent;
                string ip = Request.UserHostAddress;
                var model = new BitIniSesion();
                model.usuarioId = userId;
                model.userAgent = userAgent;
                model.movil = browser.IsMobileDevice;
                model.browser = browser.Browser;
                model.browserVersion = browser.Version;
                model.fecha = DateTime.Now;
                model.os = OS();
                model.ip = ip;
                db.BitIniSesion.Add(model);
                db.SaveChanges();
                

            }
            catch(Exception ex)
            {

            }
        }

        private string OS()
        {
            var ua = Request.UserAgent;
            if (ua.Contains("Android"))
            {
                return "Android";
            }

            if (ua.Contains("iPhone"))
            {
                return "iPhone";
            }

            if (ua.Contains("iPad"))
            {
                return "iPad";
            }

            if (ua.Contains("Mac OS"))
            {
                return "Mac OS";
            }

            if (ua.Contains("Windows NT 10"))
            {
                return "Windows 10";
            }

            if (ua.Contains("Windows NT 6.3"))
            {
                return "Windows 8.1";
            }

            if (ua.Contains("Windows NT 6.2"))
            {
                return "Windows 8";
            }


            if (ua.Contains("Windows NT 6.1"))
            {
                return "Windows 7";
            }

            if (ua.Contains("Windows NT 6.0"))
            {
                return "Windows vista";
            }

            if (ua.Contains("Windows NT 5.1") || ua.Contains("Windows NT 5.2"))

            {
                return "Windows XP";
            }

            if (ua.Contains("Windows NT 5"))
            {
                return "Windows 2000";
            }

            if (ua.Contains("Windows NT 4"))
            {
                return "Windows NT4";
            }

            if (ua.Contains("Win 9x 4.90"))
            {
                return "Windows Me";
            }

            if (ua.Contains("Windows 98"))
            {
               return "Windows 98";
            }

            if (ua.Contains("Windows 95"))
            {
                return"Windows 95";
            }


            if (ua.Contains("Windows Phone"))
            {
               return "Windows Phone";
            }

            if (ua.Contains("Linux") && ua.Contains("KFAPWI"))
            {
                return "Kindle Fire";
            }

            if (ua.Contains("RIM Tablet") || (ua.Contains("BB") && ua.Contains("Mobile")))
            {
                return "Black Berry";
            }

            //fallback to basic platform:
            return Request.Browser.Platform + (ua.Contains("Mobile") ? " Mobile " : "");
        }
        #endregion

        #region Asistentes

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            var usuario = new Usuario
            {
                Activo = true,
                Email = "pablo12_jip3@hotmail.com",
                Id = new Guid().ToString(),
                OrganismoID=1,
                UnidadAdministrativaId =1,
                UserName = "User Adom"
            };

            Session["Application"] = usuario;

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }


           
            return RedirectToAction("Index", "Home");
        }

        
        #endregion
    }
}