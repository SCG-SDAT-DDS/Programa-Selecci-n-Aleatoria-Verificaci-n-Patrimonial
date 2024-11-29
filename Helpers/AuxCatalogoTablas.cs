using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Transparencia.Models;

namespace Transparencia.Helpers
{
    public class AuxCatalogoTablas
    {
        public List<CampoCatalogo> campos { get; set; }

        public string camposString { get; set; }

        public string Validation { get; set; }

        public int CatalogoId { get; set; }

        public bool fromCreate { get; set;  }

    }
    public class AuxCatalogoTablaExcel
    {
        public int CatalogoId { get; set; }
        public int numeroExcel { get; set; }

        public string valor { get; set; }
    }

    public class AuxInsertCatalogo
    {

        public string Respuesta { get; set; }

        public int insertedId { get; set; }

    }

}