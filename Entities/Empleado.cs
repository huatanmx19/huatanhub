using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuatanHub.Entities
{
    public class Empleado : EntidadBase
    {
        public string NumNomina { get; set; }
        public string Nombre { get; set; }
        public string Paterno { get; set; }
        public string Materno { get; set; }
        public string Puesto { get; set; }

        public int? TramoId { get; set; }
        public Tramo Tramo { get; set; }

        public string Telefono { get; set; }
        public string Operador { get; set; }
        public bool Status { get; set; } 

        [Column(TypeName = "decimal(5, 2)")]
        public decimal Monto { get; set; }
        public string DeviceId { get; set; }
        public string FcmToken { get; set; }

        public IEnumerable<Asistencia> Asistencias { get; set; }
    }
}
