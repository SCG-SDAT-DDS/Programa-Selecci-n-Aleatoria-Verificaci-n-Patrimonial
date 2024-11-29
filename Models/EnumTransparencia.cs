using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    //public static class EnumTransparencia
    //{
        public enum FrecuenciaActualizacion 
        { 
            Ninguno = 0, 
            Mensual= 1,
            Bimestral = 2,
            Trimestral = 3,
            Semestral = 4,
            Anual = 5
        }

    public enum EstatusImportacio
    {
        Pendiente = 0,
        Procesando = 1,
        Terminado = 2,
        Error = 3
    }

    public enum OrdenSeleccionPublico
    {
        Primera=1,
        Segunda=2,
        Tercera=3
    }
    //}
}