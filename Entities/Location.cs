using System;
using NetTopologySuite.Geometries;

namespace HuatanHub.Entities
{
    
    public class Location
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsInside { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public Point Lugar { get; set; }

        public String BatteryLevel { get; set; }
        public String SignalPower { get; set; }

        public Empleado Empleado { get; set; }
    }
}
