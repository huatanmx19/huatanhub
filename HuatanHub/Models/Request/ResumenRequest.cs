namespace HuatanHub.Models.Request
{
    public class ResumenRequest
    {
        public int EmpleadoId { get; set; }
        public string DeviceId { get; set; }
        public int Semana { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
    }
}