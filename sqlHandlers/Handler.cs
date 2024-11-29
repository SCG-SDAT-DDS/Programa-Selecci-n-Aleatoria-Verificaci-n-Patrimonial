using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Transparencia.Models;

namespace ApiTransparencia.sqlHandlers
{
    //public class periodosHandler : SqlMapper.TypeHandler<List<vmPeriodos>>
    //{
    //    public override List<vmPeriodos> Parse(object value)
    //    {
    //        return Newtonsoft.Json.JsonConvert.DeserializeObject<List<vmPeriodos>>(value.ToString());
    //    }

    //    public override void SetValue(IDbDataParameter parameter, List<vmPeriodos> value)
    //    {
    //        parameter.Value = value;
    //    }
    //}

    //public class tagsHandler : SqlMapper.TypeHandler<List<vmTags>>
    //{
    //    public override List<vmTags> Parse(object value)
    //    {
    //        return Newtonsoft.Json.JsonConvert.DeserializeObject<List<vmTags>>(value.ToString());
    //    }

    //    public override void SetValue(IDbDataParameter parameter, List<vmTags> value)
    //    {
    //        parameter.Value = value;
    //    }
    //}

    //public class plantillasHandler : SqlMapper.TypeHandler<List<vmPlantillas>>
    //{
    //    public override List<vmPlantillas> Parse(object value)
    //    {
    //        return Newtonsoft.Json.JsonConvert.DeserializeObject<List<vmPlantillas>>(value.ToString())
    //            .Select(x => new vmPlantillas
    //            {
    //                PlantillaId = x.PlantillaId,
    //                NombreLargo = x.NombreLargo.ToTitleFormatted(),
    //                NombreCorto = x.NombreCorto,
    //                Ley = x.Ley.ToTitleFormatted(),
    //                Articulo = x.Articulo.ToTitleFormatted(),
    //                Fraccion = x.Fraccion.ToTitleFormatted(),
    //                Ayuda = x.Ayuda,
    //                NombreOrganismo = x.NombreOrganismo.ToTitleFormatted(),
    //                OrganismoId = x.OrganismoId,
    //                Periodo = x.Periodo,
    //                Frecuencia = x.Frecuencia,
    //                Tags = x.Tags,
    //                _FechaActualizacion = x._FechaActualizacion,
    //                _PeriodoDesde = x._PeriodoDesde,
    //                _PeriodoHasta = x._PeriodoHasta

    //            }).ToList();
    //    }

    //    public override void SetValue(IDbDataParameter parameter, List<vmPlantillas> value)
    //    {
    //        parameter.Value = value;
    //    }
    //}

    //public class vmPlantillaTHHandler : SqlMapper.TypeHandler<List<vmPlantillaTH>>
    //{
    //    public override List<vmPlantillaTH> Parse(object value)
    //    {
    //        return Newtonsoft.Json.JsonConvert.DeserializeObject<List<vmPlantillaTH>>(value.ToString());
    //    }

    //    public override void SetValue(IDbDataParameter parameter, List<vmPlantillaTH> value)
    //    {
    //        parameter.Value = value;
    //    }
    //}


    //vmCampoForDatos
    public class vmCampoForDatosHandler : SqlMapper.TypeHandler<List<vmCampoForDatos>>
    {
        public override List<vmCampoForDatos> Parse(object value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<vmCampoForDatos>>(value.ToString());
        }

        public override void SetValue(IDbDataParameter parameter, List<vmCampoForDatos> value)
        {
            parameter.Value = value;
        }
    }

    //public class vmCampoCatalogoRelevanteHandler : SqlMapper.TypeHandler<vmCampoCatalogoRelevante>
    //{
    //    public override vmCampoCatalogoRelevante  Parse(object value)
    //    {
    //        return Newtonsoft.Json.JsonConvert.DeserializeObject<vmCampoCatalogoRelevante>(value.ToString());
    //    }

    //    public override void SetValue(IDbDataParameter parameter, vmCampoCatalogoRelevante value)
    //    {
    //        parameter.Value = value;
    //    }
    //}

    //datos

    public class vmRowDatosHandler : SqlMapper.TypeHandler<List<vmRowDatos>>
    {
        public override List<vmRowDatos> Parse(object value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<vmRowDatos>>(value.ToString());
        }

        public override void SetValue(IDbDataParameter parameter, List<vmRowDatos> value)
        {
            parameter.Value = value;
        }
    }

    public class vmcampoDatosHandler : SqlMapper.TypeHandler<List<vmcampoDatos>>
    {
        public override List<vmcampoDatos> Parse(object value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<vmcampoDatos>>(value.ToString());
        }

        public override void SetValue(IDbDataParameter parameter, List<vmcampoDatos> value)
        {
            parameter.Value = value;
        }
    }

    public class camposTablaHandler : SqlMapper.TypeHandler<List<camposTabla>>
    {
        public override List<camposTabla> Parse(object value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<camposTabla>>(value.ToString());
        }

        public override void SetValue(IDbDataParameter parameter, List<camposTabla> value)
        {
            parameter.Value = value;
        }
    }

    //Catalogos
    public class camposCatalagolaHandler : SqlMapper.TypeHandler<List<vmCatalagoCampos>>
    {
        public override List<vmCatalagoCampos> Parse(object value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<vmCatalagoCampos>>(value.ToString());
        }

        public override void SetValue(IDbDataParameter parameter, List<vmCatalagoCampos> value)
        {
            parameter.Value = value;
        }
    }

}