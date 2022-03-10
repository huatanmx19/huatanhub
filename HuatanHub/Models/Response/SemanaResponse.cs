namespace HuatanHub.Models.Response
{
    public class Mes
    {
        public int Asistencia { get; set; }
        public int Retardo { get; set; }
        public int Falta { get; set; }
    }

    public class SemanaResponse
    {
        public int Semana { get; set; }
        public int Mes { get; set; }
        public int Asistencias { get; set; }
        public int Retardos { get; set; }
        public int Faltas { get; set; }
        public Dia Lunes { get; set; }
        public Dia Martes { get; set; }
        public Dia Miercoles { get; set; }
        public Dia Jueves { get; set; }
        public Dia Viernes { get; set; }
        public Dia Sabado { get; set; }
        public Dia Domingo { get; set; }
    }

    public class SemanaEmpleadoResponse
    {
        public string NumNomina { get; set; }
        public string Nombre { get; set; }
        public string Puesto { get; set; }
        public string Telefono { get; set; }
        public int Semana { get; set; }
        public int Mes { get; set; }
        public int Asistencias { get; set; }
        public int Retardos { get; set; }
        public int Faltas { get; set; }
        public string Lunes { get; set; }
        public string Martes { get; set; }
        public string Miercoles { get; set; }
        public string Jueves { get; set; }
        public string Viernes { get; set; }
        public string Sabado { get; set; }
        public string Domingo { get; set; }
    }

    public class Dia
    {
        public string DiaSemana { get; set; }
        public string Entrada { get; set; }
        public string Salida { get; set; }
        public string Status { get; set; }
    }
}