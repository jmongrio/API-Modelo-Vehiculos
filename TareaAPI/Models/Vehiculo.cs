using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TareaAPI.Models
{
    public class Vehiculo
    {
        [Key]
        [Required]
        public String PlacaVehiculo { get; set; }
        [Required]
        public String IdModelo { get; set; }
        [Required]
        public int AnnoFafricacion { get; set; }
        [Required]
        public Boolean Disponible { get; set; }
    }
}
