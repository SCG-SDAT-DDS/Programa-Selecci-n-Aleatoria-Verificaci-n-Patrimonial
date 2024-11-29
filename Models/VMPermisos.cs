using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class VMPermisos
    {
        public string Accion { get; set; }

        public string Controller { get; set; }
        
        public List<string> Role { get; set; }
    }
}