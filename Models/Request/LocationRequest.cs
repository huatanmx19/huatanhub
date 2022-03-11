namespace HuatanHub.Models.Request
{
    public class LocationRequest
    {
        public EmpleadoRequest Empleado { get; set; }
        public string Timestamp { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public bool IsInside { get; set; }
        public string BatteryLevel { get; set; }
        public string SignalPower { get; set; }
    }


    public class EmpleadoRequest
    {

        public int Id { get; set; }
        public string NumNomina { get; set; }
        public string Nombre { get; set; }
        public string Paterno { get; set; }
        public string Materno { get; set; }
        public string Puesto { get; set; }
        public string Telefono { get; set; }

        public int? TramoId { get; set; }
        public string TramoNombre { get; set; }
    }
}
