using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HuatanHub.Models.Request
{
    public class MarkerResponse
    {
        public int Id { get; set; }
        public string NumNomina { get; set; }
        public string Nombre { get; set; }
        public string Puesto { get; set; }
        public string Telefono { get; set; }
        
        
        public string BatteryLevel { get; set; }
        public string SignalPower { get; set; }

        public bool IsInside { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Timestamp { get; set; }

        public int TramoId { get; set; }
        public string TramoNombre { get; set; }
    }
}
