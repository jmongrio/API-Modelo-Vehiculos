using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TareaAPI.Models
{
    public class Modelo
    {
        [Key]
        [Required]
        public String IdModelo { get; set; }
        [Required]
        public String NombreModelo { get; set; }
        [Required]
        public Boolean Traccion { get; set; }
        [Required]
        public int CantidadPuerta { get; set; }
    }
}