using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Transparencia.FiltersClass
{
    public class ResultGuardarTablaFisica
    {
        public bool Result { get; set; }
        public string NombreTabla { get; set; }
        public string Valor { get; set; }
        public bool HandleError { get; set; }

    }

    public class PrepararTablas
    {
        public int CatalogoId { get; set; }
        public int Valor { get; set; }

    }

    public class ResultIndexes
    {
        public string index_name { get; set; }
        public string index_description { get; set; }
        public string index_keys { get; set; }

    }

    public class LstCampos
    {
        public int CampoId { get; set; }
        public int CampoCatalogoId { get; set; }
        public int AddTipoCampo { get; set; }
        public string AddNombre { get; set; }
        public bool AddConDecimales { get; set; }
        public string AddEtiqueta { get; set; }
        public int AddLongitud { get; set; }
        public int AddTipoFecha { get; set; }
        public int AddGrupoExtensionId { get; set; }
        public int AddSize { get; set; }
        public int AddCatalogoId { get; set; }
        public bool AddRequerido { get; set; }
        public bool AddPrincipal { get; set; }
        public string AddAyuda { get; set; }
        public int AddOrden { get; set; }
        public bool AddRelevante { get; set; }
        public int? AddCampoPNT { get; set; }
        public int? AddTipoCampoPNT { get; set; }
        public int? AddOrdenSeleccion { get; set; }

    }


    public class RespuestaQuery
    {
        public int TablaFisicaId { get; set; }
        public string Valor { get; set; }
        public int ValorInt { get; set; }
    }

    public class ListaDescargar
    {
        public string Valor { get; set; }
        public string Nombre { get; set; }
    }

    public class PNTListaCampos
    {
        public int CampoId { get; set; }
        public int IdCampoPNT { get; set; }
        public int IdTipoCampoPNT { get; set; }
    }
}