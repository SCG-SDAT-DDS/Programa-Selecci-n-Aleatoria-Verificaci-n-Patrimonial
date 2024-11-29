using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Transparencia.Models
{
    public class PlantillaViewModel
    {
        public Plantilla Plantilla { get; set; }

        public IEnumerable<SelectListItem> AllPeriodos { get; set; }

        //public IEnumerable<SelectListItem> AllTags { get; set; }

        private List<int> _selectedPeriodo;

        //private List<int> _selectedFecuenciaConservacion;

        //private List<int> _selectedTag;

        public List<int> SelectedPeriodo
        {
            get
            {
                if (_selectedPeriodo == null)
                {
                    _selectedPeriodo = Plantilla.Periodos.Select(m => m.PeriodoId).ToList();
                }
                return _selectedPeriodo;
            }
            set { _selectedPeriodo = value; }
        }

        //public List<int> SelectedTag
        //{
        //    get
        //    {
        //        if (_selectedTag == null)
        //        {
        //            _selectedTag = Plantilla.Tags.Select(m => m.GrupoTagId).ToList();
        //        }
        //        return _selectedTag;
        //    }
        //    set { _selectedTag = value; }
        //}

        //public List<int> selectedFrecuenciaConservacion
        //{
        //    get
        //    {
        //        if (_selectedFecuenciaConservacion == null)
        //        {
        //            _selectedFecuenciaConservacion = Plantilla.Tags.Select(m => m.GrupoTagId).ToList();
        //        }
        //        return _selectedFecuenciaConservacion;
        //    }
        //    set { _selectedFecuenciaConservacion = value; }
        //}
        public PlantillaViewModel()
        {
            AllPeriodos = new List<SelectListItem>();
            //AllTags = new List<SelectListItem>();
        }
    }
}