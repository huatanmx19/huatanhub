namespace HuatanHub.Models.Response
{
    /// <summary>
    ///     Modelo de respuesta cuando se ingresa la hora de entrada/salida
    /// </summary>
    public class AsistenciaResponse
    {
        public string Nomina { get; set; }
        public int AsistenciaId { get; set; }
        public string Fecha { get; set; }
        public string DiaSemana { get; set; }
        public string HoraEntrada { get; set; }
        public string HoraSalida { get; set; }
        public int Semana { get; set; }
        public int Mes { get; set; }
        public bool Asistencia { get; set; }
        public bool Retardo { get; set; }
        public bool Falta { get; set; }
        public bool NoLaboral { get; set; }
        public bool Incapacidad { get; set; }
    }
}