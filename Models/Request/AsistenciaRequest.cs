namespace HuatanHub.Models.Request
{
    /// <summary>
    ///     Objeto que se recibe del dispositvo para registrar asistencia
    /// </summary>
    public class AsistenciaRequest
    {
        public string NumNomina { get; set; } 
        public string Phone { get; set; }
        public double Lat { get; set; }        
        public double Lng { get; set; }
        public string DeviceTime { get; set; }        
    }
}