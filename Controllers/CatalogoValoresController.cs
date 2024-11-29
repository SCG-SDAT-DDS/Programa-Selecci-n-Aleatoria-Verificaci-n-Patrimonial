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
    public class CatalogoValoresController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CatalogoValores
        public ActionResult Index(int id=0)
        {
            if (id == 0)
            {
               return  RedirectToAction("Index", "Catalogos");
            }
            var catalogoValores = db.CatalogoValores.Where(x=>x.CatalogoId == id).Include(c => c.Catalogo);
            ViewBag.CatalogoId = id;
            getNameCatalogo(id);
            return View(catalogoValores.ToList());
        }

        private void getNameCatalogo(int id =0)
        {
            var catalogo = db.Catalogoes.Where(x => x.CatalogoId == id).FirstOrDefault();
            ViewBag.CatalogoName = "";
            if (catalogo != null)
            {
                ViewBag.CatalogoName = catalogo.Nombre;
            }
        }
        private void initCatalogo(int id = 0)
        {
            var catalogos = db.Catalogoes.Where(x => x.Activo && x.CatalogoId != id);
            ViewBag.Catalogos = catalogos.Select(o => new SelectListItem
            {
                Text = o.Nombre,
                Value = o.CatalogoId.ToString()
            });
            ViewBag.AllValor = new  List<SelectListItem>();


        }
        // GET: CatalogoValores/Details/5
        public ActionResult Details(int id=0)
        {
            if (id == 0)
            {
                return RedirectToAction("Index", "Catalogos");
            }
            CatalogoValor catalogoValor = db.CatalogoValores.Find(id);
            if (catalogoValor == null)
            {
                return HttpNotFound();
            }
            return View(catalogoValor);
        }

        // GET: CatalogoValores/Create
        public ActionResult Create(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Index", "Catalogos");
            }
            initCatalogo(id);
            var catalogosValor = new CatalogoValorViewModels();
            catalogosValor.CatalogoValor = new CatalogoValor();
            catalogosValor.CatalogoValor.CatalogoId = id;
            getNameCatalogo(id);
            return View(catalogosValor);
        }

        // POST: CatalogoValores/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CatalogoValorViewModels CatalogoValorViewModels)
        {
            if (ModelState.IsValid)
            {
                var updatedValor = new HashSet<int>(CatalogoValorViewModels.SelectedValor);
                var RowRelatedCatalogoValor =  CatalogoValorViewModels.CatalogoValor.RelatedCatalogoValor;
                if(RowRelatedCatalogoValor != null && RowRelatedCatalogoValor.Count() > 0)
                {
                    foreach (var item in RowRelatedCatalogoValor)
                    {
                        CatalogoValorViewModels.CatalogoValor.RelatedCatalogoValor.Remove(item);
                    }
                }

                foreach (var item in updatedValor)
                {
                    var vCatalagoValor = db.CatalogoValores.Where(x => x.CatalogoValorId == item).FirstOrDefault();
                    if(vCatalagoValor != null)
                        CatalogoValorViewModels.CatalogoValor.RelatedCatalogoValor.Add(vCatalagoValor);
                }
                db.CatalogoValores.Add(CatalogoValorViewModels.CatalogoValor);
                db.SaveChanges();
                return RedirectToAction("Index",new { id = CatalogoValorViewModels.CatalogoValor.CatalogoId });

            }
            initCatalogo(CatalogoValorViewModels.CatalogoValor.CatalogoId);
            ViewBag.CatalogoId = new SelectList(db.Catalogoes, "CatalogoId", "Nombre", CatalogoValorViewModels.CatalogoValor.CatalogoId);
            return View(CatalogoValorViewModels);
        }

        // GET: CatalogoValores/Edit/5
        public ActionResult Edit(int id=0)
        {
            if (id == 0)
            {
                return RedirectToAction("Index", "Catalogos");
            }
            //GrupoTag grupoTag = db.GrupoTags.Find(id);
            var CatalogoValorViewModels = new CatalogoValorViewModels
            {
                CatalogoValor = db.CatalogoValores.Include(i => i.RelatedCatalogoValor).First(i => i.CatalogoValorId == id),
            };
            if (CatalogoValorViewModels.CatalogoValor == null)
            {
                return HttpNotFound();
            }
            var catalogosValor = new CatalogoValorViewModels();
            catalogosValor.CatalogoValor = new CatalogoValor();
            catalogosValor.CatalogoValor.CatalogoId = id;
            getNameCatalogo(CatalogoValorViewModels.CatalogoValor.CatalogoId);
            initCatalogo(CatalogoValorViewModels.CatalogoValor.CatalogoId);
            return View(CatalogoValorViewModels);
        }

        // POST: CatalogoValores/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CatalogoValorViewModels CatalogoValorViewModels)
        {
            if (CatalogoValorViewModels == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (ModelState.IsValid)
            {

                var catalogovalores = db.CatalogoValores
                    .Include(i => i.RelatedCatalogoValor).First(i => i.CatalogoValorId == CatalogoValorViewModels.CatalogoValor.CatalogoValorId);

                if (TryUpdateModel(catalogovalores, "CatalogoValor", new string[] { "valor", "orden", "Activo" }))
                {
                    var newCatalagoValores = db.CatalogoValores.Where(m => CatalogoValorViewModels.SelectedValor.Contains(m.CatalogoValorId)).ToList();
                    var updatedValor = new HashSet<int>(CatalogoValorViewModels.SelectedValor);


                    if(catalogovalores.RelatedCatalogoValor != null && catalogovalores.RelatedCatalogoValor.Count > 0)
                    {
                        var noEstan = catalogovalores.RelatedCatalogoValor.Where(x => !updatedValor.Contains( x.CatalogoValorId)).ToList();
                        if(noEstan.Count > 0)
                        {
                            foreach (var item in noEstan)
                            {
                                catalogovalores.RelatedCatalogoValor.Remove(item);
                            }
                        }

                    }
                    if(updatedValor.Count > 0)
                    {
                        foreach (var item in updatedValor)
                        {
                            var existe = catalogovalores.RelatedCatalogoValor.Where(x => x.CatalogoValorId == item).Count();
                            if (existe == 0)
                            {
                                var vCatalagoValor = db.CatalogoValores.Where(x => x.CatalogoValorId == item).FirstOrDefault();
                                if (vCatalagoValor != null)
                                    catalogovalores.RelatedCatalogoValor.Add(vCatalagoValor);
                            }

                        }
                    }
                    

                    db.Entry(catalogovalores).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index",new { id = CatalogoValorViewModels.CatalogoValor.CatalogoId});
                }
            }
            getNameCatalogo(CatalogoValorViewModels.CatalogoValor.CatalogoId);
            initCatalogo(CatalogoValorViewModels.CatalogoValor.CatalogoId);
            return View(CatalogoValorViewModels);
        }

        // GET: CatalogoValores/Delete/5
        public ActionResult Delete(int id=0)
        {
            if (id == 0)
            {
                return RedirectToAction("Index", "Catalogos");
            }
            CatalogoValor catalogoValor = db.CatalogoValores.Find(id);
            if (catalogoValor == null)
            {
                return HttpNotFound();
            }
            getNameCatalogo(catalogoValor.CatalogoId);
            return View(catalogoValor);
        }

        // POST: CatalogoValores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id=0)
        {
            if (id == 0)
            {
                return RedirectToAction("Index", "Catalogos");
            }
           
            CatalogoValor catalogovalor = db.CatalogoValores
                  .Include(j => j.RelatedCatalogoValor)
                  .First(j => j.CatalogoValorId == id);
            var catalogoId = catalogovalor.CatalogoId;
            try
            {
                db.CatalogoValores.Remove(catalogovalor);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                getNameCatalogo(catalogovalor.CatalogoId);
                ModelState.AddModelError("", "Ocurrio un error, asegúrese que el elemento que desea eliminar no se en encuentre asignado en otro registro.");
                return View(catalogovalor);
            }
           
            return RedirectToAction("Index", new { id = catalogoId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult getDatosCatalogoById(int id)
        {
            return Json(db.CatalogoValores.Where(x=>x.CatalogoId==id && x.Activo).Select(x => new
            {
                Value = x.CatalogoValorId,
                Text = x.valor
            }).ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}
