using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Transparencia.Models;

namespace Transparencia.Controllers
{
    public class BitIniSesionController : Controller
    {
        // GET: BitIniSesion
        public ActionResult Index(FiltrosPlantilla filtros, int PerPage = 10, int iPagina = 1)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            ViewBag.LstUsuarios = db.Users.ToList();

            var lstModel = db.BitIniSesion.Where(x=>
            ( filtros.UsuarioId == null || x.usuarioId == filtros.UsuarioId)
            && (filtros.PeriodoDesde == null || x.fecha >= filtros.PeriodoDesde)
            && (filtros.PeriodoHasta == null || x.fecha <= filtros.PeriodoHasta)).OrderByDescending(x=>x.fecha).ToPagedList(iPagina, PerPage);

            return View(lstModel);
        }

        // GET: BitIniSesion/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: BitIniSesion/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BitIniSesion/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: BitIniSesion/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: BitIniSesion/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: BitIniSesion/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: BitIniSesion/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
