using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Transparencia.Models
{
    public class GrupoTagsViewModel
    {
            public GrupoTag GrupoTag { get; set; }
            public IEnumerable<SelectListItem> AllTags { get; set; }

            private List<int> _selectedTags;
            public List<int> SelectedTags
            {
                get
                {
                    if (_selectedTags == null)
                    {
                        _selectedTags = GrupoTag.Tags.Select(m => m.TagId).ToList();
                    }
                    return _selectedTags;
                }
                set { _selectedTags = value; }
            }
        public GrupoTagsViewModel()
        {
            AllTags = new List<SelectListItem>();
        }

    }
}