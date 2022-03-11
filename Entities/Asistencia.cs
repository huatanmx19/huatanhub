using System;
using NetTopologySuite.Geometries;

namespace HuatanHub.Entities
{
    public class Asistencia : EntidadBase
    {
        public DateTime Fecha { get; set; }
        public DateTime? HoraEntrada { get; set; }
        public DateTime? HoraSalida { get; set; }
        public int SemanaAnio { get; set; }
        public int Anio { get; set; }
        public bool Falta { get; set; }
        public bool Retardo { get; set; }
        public bool Attendence { get; set; }
        public bool Incapacidad { get; set; }
        public bool NoLaboral { get; set; }
        public bool NoCheco { get; set; }
        public bool Incidencia { get; set; }
        public Point LugarEntrada { get; set; }
        public Point LugarSalida { get; set; }
        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; }
    }
}
