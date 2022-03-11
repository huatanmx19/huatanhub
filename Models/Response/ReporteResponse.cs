using System.Collections.Generic;
using HuatanHub.Models.Request;

namespace HuatanHub.Models.Response
{
    public class ReporteResponse
    {        
         public int Activos { get; set; }
        public int Inactivos { get; set; }
        public int Total { get; set; }
        public int Asistencia  { get; set; }
    }
}
