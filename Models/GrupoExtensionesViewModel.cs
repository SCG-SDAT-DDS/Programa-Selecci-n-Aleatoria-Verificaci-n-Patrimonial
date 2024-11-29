using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Transparencia.Models
{
    public class GrupoExtensionesViewModel
    {
        public GrupoExtension GrupoExtension { get; set; }

        public IEnumerable<SelectListItem> AllExtensions { get; set; }

        private List<int> _selectedExtensions;
        public List<int> SelectedExtensions
        {
            get
            {
                if (_selectedExtensions == null)
                {
                    _selectedExtensions = GrupoExtension.Extensiones.Select(m => m.ExtensionId).ToList();
                }
                return _selectedExtensions;
            }
            set { _selectedExtensions = value; }
        }

        public GrupoExtensionesViewModel()
        {
            AllExtensions = new List<SelectListItem>();
        }
    }
}