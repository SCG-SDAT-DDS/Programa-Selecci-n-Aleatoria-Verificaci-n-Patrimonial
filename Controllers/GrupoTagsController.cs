using PagedList;
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
    public class GrupoTagsController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public bool? changeStatus(int id = 0)
        {
            var activo = false;
            try
            {
                if (id == 0)
                {
                    return null;
                }
                var Model = db.GrupoTags.Where(x=>x.GrupoTagId == id).Include(x=>x.Tags).FirstOrDefault();
                //bitacora
                ApplicationDbContext olddb = new ApplicationDbContext();
                var oldModel = olddb.GrupoTags.Where(x => x.GrupoTagId == id).Include(y => y.Tags).FirstOrDefault();
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
                this.CreateBitacora(oldModel, Model, Model.GrupoTagId);

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
        // GET: GrupoTags
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
            IPagedList<GrupoTag> vModel = null;

            var query = db.GrupoTags.Where(x =>
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

        // GET: GrupoTags/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GrupoTag grupoTag = db.GrupoTags.Find(id);
            if (grupoTag == null)
            {
                return HttpNotFound();
            }
            return View(grupoTag);
        }

        // GET: GrupoTags/Create
        public ActionResult Create()
        {
            var GrupoTags = new GrupoTag();

            var AllTags = db.Tags.Where(x=>x.Activo).ToList();

            ViewBag.AllTags = AllTags.Select(o => new SelectListItem
            {
                Text = o.Nombre,
                Value = o.TagId.ToString()
            });
            
            return View();
        }

        // POST: GrupoTags/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GrupoTagsViewModel GrupoTagsViewModel)
        {
            if (ModelState.IsValid)
            {
                var updatedJobTags = new HashSet<int>(GrupoTagsViewModel.SelectedTags);
                foreach (Tag tags in db.Tags.Where(x => x.Activo))
                {
                    if (!updatedJobTags.Contains(tags.TagId))
                    {
                        GrupoTagsViewModel.GrupoTag.Tags.Remove(tags);
                    }
                    else
                    {
                        GrupoTagsViewModel.GrupoTag.Tags.Add((tags));
                    }
                }
                

                db.GrupoTags.Add(GrupoTagsViewModel.GrupoTag);
                db.SaveChanges();
                //bitacora
                this.CreateBitacora(null, GrupoTagsViewModel.GrupoTag, GrupoTagsViewModel.GrupoTag.GrupoTagId);

                return RedirectToAction("Index");
            }

            return View(GrupoTagsViewModel);
        }
      
        // GET: GrupoTags/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //GrupoTag grupoTag = db.GrupoTags.Find(id);
            var grupoTag = new GrupoTagsViewModel
            {
                GrupoTag = db.GrupoTags.Include(i => i.Tags).First(i => i.GrupoTagId == id),
            };
            if (grupoTag.GrupoTag == null)
            {
                return HttpNotFound();
            }

            var allJobTagsList = db.Tags.Where(x=>x.Activo).ToList();
            grupoTag.AllTags = allJobTagsList.Select(o => new SelectListItem
            {
                Text = o.Nombre,
                Value = o.TagId.ToString()
            });

            return View(grupoTag);
        }

        // POST: GrupoTags/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GrupoTagsViewModel GrupoTagsViewModel)
        {
            if (GrupoTagsViewModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (ModelState.IsValid)
            {
                //para la bitacora
                ApplicationDbContext dbOld = new ApplicationDbContext();
                var oldModel = dbOld.GrupoTags.Where(x => x.GrupoTagId == GrupoTagsViewModel.GrupoTag.GrupoTagId).Include(x => x.Tags).FirstOrDefault();


                var grupoTags = db.GrupoTags
                    .Include(i => i.Tags).First(i => i.GrupoTagId == GrupoTagsViewModel.GrupoTag.GrupoTagId);
                if (TryUpdateModel(grupoTags, "GrupoTag", new string[] { "Nombre", "Descripcion","Activo","Icono","Color" }))
                {
                    var newTags = db.Tags.Where(m => GrupoTagsViewModel.SelectedTags.Contains(m.TagId)).ToList();
                    var updatedTags = new HashSet<int>(GrupoTagsViewModel.SelectedTags);

                    foreach (Tag tag in db.Tags)
                    {
                        if (!updatedTags.Contains(tag.TagId))
                        {
                            grupoTags.Tags.Remove(tag);
                        }
                        else
                        {
                            grupoTags.Tags.Add(tag);
                        }
                    }
                    db.Entry(grupoTags).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    //bitacora
                    this.CreateBitacora(oldModel, grupoTags, grupoTags.GrupoTagId);

                }

                return RedirectToAction("Index");
            }
            return View(GrupoTagsViewModel);
        }

        //// GET: GrupoTags/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    GrupoTag grupoTag = db.GrupoTags.Find(id);
        //    if (grupoTag == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(grupoTag);
        //}

        //// POST: GrupoTags/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    GrupoTag grupoTag = db.GrupoTags
        //          .Include(j => j.Tags)
        //          .First(j => j.GrupoTagId == id);
        //    db.GrupoTags.Remove(grupoTag);
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
        private void CreateBitacora(GrupoTag oldModel, GrupoTag newModel, int id = 0)
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
                //Descripción
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Descripción",
                    es_modificado = oldModel != null && oldModel.Descripcion != newModel.Descripcion ? true : false,
                    campo_nuevo = newModel.Descripcion,
                    campo_anterior = oldModel != null ? oldModel.Descripcion : null
                });

                //Tags
                string newTags = "";
                bool newFirst = true;
                string oldTags = "";
                bool oldFirst = true;
                if (newModel != null && newModel.Tags != null && newModel.Tags.Count > 0)
                {
                    foreach (var item in newModel.Tags)
                    {
                        newTags += !newFirst ? ", " : "";
                        newTags += $@"{item.Nombre}";
                        newFirst = false;
                    }
                }

                if (oldModel != null && oldModel.Tags != null && oldModel.Tags.Count > 0)
                {
                    foreach (var item in oldModel.Tags)
                    {
                        oldTags += !oldFirst ? ", " : "";
                        oldTags += $@"{item.Nombre}";
                        oldFirst = false;
                    }
                }

                //Tags
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Tags",
                    es_modificado = oldModel != null && oldTags != newTags ? true : false,
                    campo_nuevo = newTags,
                    campo_anterior = oldTags
                });

                //Activo
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Activo",
                    es_modificado = oldModel != null && oldModel.Activo != newModel?.Activo ? true : false,
                    campo_nuevo = HMTLHelperExtensions.getStringBoolBitacora(newModel.Activo),
                    campo_anterior = oldModel != null ? HMTLHelperExtensions.getStringBoolBitacora(oldModel.Activo) : null
                });

                //Icono
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Icono",
                    es_modificado = oldModel != null && oldModel.Icono != newModel.Icono ? true : false,
                    campo_nuevo = newModel.Icono,
                    campo_anterior = oldModel != null ? oldModel.Icono : null
                });

                //Color
                cambioCampos.Add(new cambiosCampos
                {
                    nombre_campo = "Color",
                    es_modificado = oldModel != null && oldModel.Color != newModel.Color ? true : false,
                    campo_nuevo = newModel.Color,
                    campo_anterior = oldModel != null ? oldModel.Color : null
                });



                //Bitacora
                var usuario = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                var res = HMTLHelperExtensions.Bitacora(cambioCampos, "GrupoTags", "Grupo de Tags", id, usuario?.Id, accion);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
