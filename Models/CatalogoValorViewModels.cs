using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Transparencia.Models
{
    public class CatalogoValorViewModels
    {
        public CatalogoValor CatalogoValor { get; set; }
        public IEnumerable<SelectListItem> AllValor { get; set; }

        private List<int> _selectedValor;
        public List<int> SelectedValor
        {
            get
            {
                if (_selectedValor == null)
                {
                    _selectedValor = CatalogoValor.RelatedCatalogoValor.Select(m => m.CatalogoValorId).ToList();
                }
                return _selectedValor;
            }
            set { _selectedValor = value; }
        }
        public CatalogoValorViewModels()
        {
            AllValor = new List<SelectListItem>();
        }
    }
}