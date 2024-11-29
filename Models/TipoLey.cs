﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Transparencia.Models
{
    public class TipoLey: ICloneable
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Id de Ley")]
        public int TipoLeyId { get; set; }

        [StringLength(50, ErrorMessage = "El nombre solo acepta 50 caracteres.")]
        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Estatus")]
        public bool Activo { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}