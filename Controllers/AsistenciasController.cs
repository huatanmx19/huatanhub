using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HuatanApi.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuatanHub.Data;
using HuatanHub.Entities;
using HuatanHub.Hubs;
using HuatanHub.Models.Request;
using HuatanHub.Models.Response;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace HuatanHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsistenciasController : ControllerBase
    {
        private readonly ApiContext                     _context;
        private readonly ILogger<AsistenciasController> _logger;
        private readonly IHubContext<LocationHub>       _hubContext;

        public AsistenciasController(ApiContext context, ILogger<AsistenciasController> logger, IHubContext<LocationHub> hubContext)
        {
            _logger  = logger;
            _hubContext = hubContext;
            _context = context;
        }

        // GET: api/Asistencias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Asistencia>>> GetAsistencias()
        {
            try
            {
                return await _context.Asistencias
                    .AsQueryable()
                    .ToListAsync();
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e, "OCurrio un error al recuperar la lista de asistencias", e);
                return StatusCode(500, "OCurrio un error al recuperar la lista de asistencias");
            }
        }

        /// <summary>
        /// Registra la entrada y envia el evento para actualizar el marcador
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> PostAsistencia(AsistenciaRequest data)
        {
            // CORREGIR LAS HORAS
            try
            {
                // verificar la semana del año en el dispositivo y el server

                //var weekDevice = data.SemanaAnio;
                var weekServer  = ISOWeek.GetWeekOfYear(DateTime.Now);

                //_logger.LogInformation($"Week Device -> {weekDevice}");
                _logger.LogInformation($"Week Server -> {weekServer}");

                _logger.LogInformation($"Hora enviada desde el dispositivo: {data.DeviceTime}");

                Empleado empleado;

                if (!string.IsNullOrEmpty(data.Phone))
                {
                    empleado = await _context.Empleados
                        .Where(x => x.Telefono == data.Phone)
                        .Where(x=>x.Active)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    empleado = await _context.Empleados
                        .Where(x => x.NumNomina == data.NumNomina)
                        .FirstOrDefaultAsync();
                }

                if (empleado == null)
                {
                    _logger.LogWarning($"No se encuentra registrado {data.Phone}", data);
                    return NotFound("No se encuentra registrado");
                }

                var asistencia = new Asistencia();

                var horaEntrada = new DateTime(
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    DateTime.Now.Day,
                    08, 0, 0);

                var horaApertura = new DateTime(
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    DateTime.Now.Day,
                    06, 0, 0);

                var checkingTime = DateTime.Now;


                if (checkingTime > horaApertura)
                {
                    var diff    = checkingTime - horaEntrada;
                    var minutes = diff.TotalSeconds / 60.0;

                    if (minutes <= 15)
                    {
                        asistencia.Attendence = true;
                        asistencia.Falta      = false;
                        asistencia.Retardo    = false;
                    }

                    if (minutes > 15 && minutes <= 30)
                    {
                        asistencia.Attendence = false;
                        asistencia.Falta      = false;
                        asistencia.Retardo    = true;
                    }

                    if (minutes > 30)
                    {
                        asistencia.Attendence = false;
                        asistencia.Falta      = true;
                        asistencia.Retardo    = false;
                    }
                }
                else
                {
                    asistencia.Attendence = false;
                    asistencia.Falta      = false;
                    asistencia.Retardo    = false;
                }

                asistencia.LugarEntrada = new Point(new Coordinate(data.Lng, data.Lat)) {SRID = 4326};
                asistencia.EmpleadoId   = empleado.Id;
                asistencia.Fecha        = DateTime.Today;
                asistencia.HoraEntrada  = checkingTime;
                asistencia.SemanaAnio   = weekServer;
                asistencia.Anio         = checkingTime.Year;

                // TODO: CALCULAR LA HORA DE SALIDA

                await _context.Asistencias.AddAsync(asistencia);
                await _context.SaveChangesAsync();

                var result = new AsistenciaResponse
                {
                    AsistenciaId = asistencia.Id,
                    Nomina       = empleado.NumNomina,
                    Fecha        = asistencia.HoraEntrada.Value.ToDateString(),
                    HoraEntrada  = asistencia.HoraEntrada.Value.ToHourString(),
                    HoraSalida   = string.Empty,
                    Asistencia   = asistencia.Attendence,
                    Retardo      = asistencia.Retardo,
                    Falta        = asistencia.Falta,
                    NoLaboral    = asistencia.NoLaboral,
                    Incapacidad  = asistencia.Incapacidad,
                    Mes          = asistencia.Fecha.Month,
                    Semana       = asistencia.SemanaAnio
                };


                return StatusCode(201, result);
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e, "Ocurrio un error al registrar la entrada", data);
                return StatusCode(500, "Ocurrio un error al registrar la entrada");
            }
        }

        /// <summary>
        /// REGISTRA LA SALIDA
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsistencia(int id, AsistenciaRequest data)
        {
            try
            {
                var asistencia = await _context.Asistencias
                    .AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == id);

                asistencia.HoraSalida = DateTime.Now;
                asistencia.LugarSalida           = new Point(new Coordinate(data.Lng, data.Lat)) {SRID = 4326};
                _context.Entry(asistencia).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var result = new AsistenciaResponse
                {
                    AsistenciaId = asistencia.Id,
                    Fecha        = asistencia.HoraEntrada.HasValue ? asistencia.HoraEntrada.Value.ToDateString() : "",
                    HoraEntrada  = asistencia.HoraEntrada.HasValue ? asistencia.HoraEntrada.Value.ToHourString() : "",
                    HoraSalida   = asistencia.HoraSalida.Value.ToHourString(),
                    Asistencia   = asistencia.Attendence,
                    Retardo      = asistencia.Retardo,
                    Falta        = asistencia.Falta,
                    NoLaboral    = asistencia.NoLaboral,
                    Semana       = asistencia.SemanaAnio,
                    Mes          = asistencia.Fecha.Month
                };

                await _hubContext.Clients.All.SendAsync("remove-marker", data);

                // Put a gray location
                return Ok(result);
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e, "Ocurrio un error al registrar la salida de los empleados", e);
                return StatusCode(500, "Ocurrio un error al registrar la salida de los empleados");
            }
        }

        /// <summary>
        /// Endpoit for get the weekly resume of employee
        /// </summary>
        /// <param name="numNomina">Numero de nomina</param>
        /// <returns>JSN con el resumen de entrada/salida semanalmente</returns>
        [HttpGet("semana/{numNomina}")]
        public async Task<IActionResult> AistenciaSemana(string numNomina)
        {
            // TODO: SOLO EL MES ACTUAL
            // TODO: PASAR A UN EXCEL
            // TODO: TODOS LOS DIAS DE LA SEMANA
            // TODO: LOS DIAS INHABILES

            try
            {
                var year = DateTime.Now.Year;
                // obtenemos las semanas por
                var query = await _context.Asistencias
                    .Where(x => x.Fecha.Year == year)
                    .Where(e => e.Empleado.NumNomina == numNomina)
                    .Where(t => t.Fecha.Month == DateTime.Now.Month)
                    .ToListAsync();

                var semanas = query
                    .GroupBy(x => new
                    {
                        Semana = x.SemanaAnio,
                    });


                var result = new List<SemanaResponse>();

                foreach (var semana in semanas)
                {
                    var tmp = new SemanaResponse
                    {
                        Semana = semana.Key.Semana,
                        Mes    = semana.FirstOrDefault().Fecha.Month
                    };


                    var domingo = semana.FirstOrDefault(x => x.Fecha.DayOfWeek == DayOfWeek.Sunday);
                    if (domingo != null)
                    {
                        tmp.Domingo = new Dia
                        {
                            Entrada   = domingo.HoraEntrada.HasValue ? domingo.HoraEntrada.Value.ToHourString() : "",
                            Salida    = domingo.HoraSalida.HasValue ? domingo.HoraSalida.Value.ToHourString() : "",
                            DiaSemana = "Domingo",
                            Status    = SetStatus(domingo.Attendence, domingo.Retardo, domingo.Falta)
                        };
                    }
                    else
                    {
                        tmp.Domingo = new Dia
                        {
                            Entrada   = "",
                            Salida    = "",
                            DiaSemana = "Domingo",
                            Status    = ""
                        };
                    }


                    var lunes = semana.FirstOrDefault(x => x.Fecha.DayOfWeek == DayOfWeek.Monday);
                    if (lunes != null)
                    {
                        tmp.Lunes = new Dia
                        {
                            Entrada   = lunes.HoraEntrada.HasValue ? lunes.HoraEntrada.Value.ToHourString() : "",
                            Salida    = lunes.HoraSalida.HasValue ? lunes.HoraSalida.Value.ToHourString() : "",
                            DiaSemana = lunes.Fecha.ToString("dddd", new CultureInfo("es-MX")),
                            Status    = SetStatus(lunes.Attendence, lunes.Retardo, lunes.Falta)
                        };
                    }
                    else
                    {
                        tmp.Lunes = new Dia
                        {
                            Entrada   = "",
                            Salida    = "",
                            DiaSemana = "Lunes",
                            Status    = ""
                        };
                    }

                    var martes = semana.FirstOrDefault(x => x.Fecha.DayOfWeek == DayOfWeek.Tuesday);
                    if (martes != null)
                    {
                        tmp.Martes = new Dia
                        {
                            Entrada   = martes.HoraEntrada.HasValue ? martes.HoraEntrada.Value.ToHourString() : "",
                            Salida    = martes.HoraSalida.HasValue ? martes.HoraSalida.Value.ToHourString() : "",
                            DiaSemana = "Martes",
                            Status    = SetStatus(martes.Attendence, martes.Retardo, martes.Falta)
                        };
                    }
                    else
                    {
                        tmp.Martes = new Dia
                        {
                            Entrada   = "",
                            Salida    = "",
                            DiaSemana = "Martes",
                            Status    = ""
                        };
                    }

                    var miercoles = semana.FirstOrDefault(x => x.Fecha.DayOfWeek == DayOfWeek.Wednesday);
                    if (miercoles != null)
                    {
                        tmp.Miercoles = new Dia
                        {
                            Entrada = miercoles.HoraEntrada.HasValue ? miercoles.HoraEntrada.Value.ToHourString() : "",
                            Salida = miercoles.HoraSalida.HasValue ? miercoles.HoraSalida.Value.ToHourString() : "",
                            DiaSemana = "Miercoles",
                            Status = SetStatus(miercoles.Attendence, miercoles.Retardo, miercoles.Falta)
                        };
                    }
                    else
                    {
                        tmp.Miercoles = new Dia
                        {
                            Entrada   = "",
                            Salida    = "",
                            DiaSemana = "Miercoles",
                            Status    = ""
                        };
                    }

                    var jueves = semana.FirstOrDefault(x => x.Fecha.DayOfWeek == DayOfWeek.Thursday);
                    if (jueves != null)
                    {
                        tmp.Jueves = new Dia
                        {
                            Entrada   = jueves.HoraEntrada.HasValue ? jueves.HoraEntrada.Value.ToHourString() : "",
                            Salida    = jueves.HoraSalida.HasValue ? jueves.HoraSalida.Value.ToHourString() : "",
                            DiaSemana = "Jueves",
                            Status    = SetStatus(jueves.Attendence, jueves.Retardo, jueves.Falta)
                        };
                    }
                    else
                    {
                        tmp.Jueves = new Dia
                        {
                            Entrada   = "",
                            Salida    = "",
                            DiaSemana = "Jueves",
                            Status    = ""
                        };
                    }

                    var viernes = semana.FirstOrDefault(x => x.Fecha.DayOfWeek == DayOfWeek.Friday);
                    if (viernes != null)
                    {
                        tmp.Viernes = new Dia
                        {
                            Entrada   = viernes.HoraEntrada.HasValue ? viernes.HoraEntrada.Value.ToHourString() : "",
                            Salida    = viernes.HoraSalida.HasValue ? viernes.HoraSalida.Value.ToHourString() : "",
                            DiaSemana = "Viernes",
                            Status    = SetStatus(viernes.Attendence, viernes.Retardo, viernes.Falta)
                        };
                    }
                    else
                    {
                        tmp.Viernes = new Dia
                        {
                            Entrada   = "",
                            Salida    = "",
                            DiaSemana = "Viernes",
                            Status    = ""
                        };
                    }

                    var sabado = semana.FirstOrDefault(x => x.Fecha.DayOfWeek == DayOfWeek.Saturday);
                    if (sabado != null)
                    {
                        tmp.Sabado = new Dia
                        {
                            Entrada   = sabado.HoraEntrada.HasValue ? sabado.HoraEntrada.Value.ToHourString() : "",
                            Salida    = sabado.HoraSalida.HasValue ? sabado.HoraSalida.Value.ToHourString() : "",
                            DiaSemana = "Sabado",
                            Status    = SetStatus(sabado.Attendence, sabado.Retardo, sabado.Falta)
                        };
                    }
                    else
                    {
                        tmp.Sabado = new Dia
                        {
                            Entrada   = "",
                            Salida    = "",
                            DiaSemana = "Sabado",
                            Status    = ""
                        };
                    }


                    tmp.Asistencias = semana.Count(x => x.Attendence);
                    tmp.Retardos    = semana.Count(x => x.Retardo);
                    tmp.Faltas      = semana.Count(x => x.Falta);

                    result.Add(tmp);
                }

                return Ok(result);
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e, $"Ocurrio un error al obtener el reporte de asistencias", e);
                return StatusCode(500, $"Ocurrio un error al obtener el reporte de asistencias, {e}");
            }
        }


        
        private string[] GetHourDay(IEnumerable<Asistencia> asistencias)
        {
            string[] inOut = {"", "", "", "", "", "", ""};
            foreach (var x in asistencias)
            {
                switch (x.Fecha.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        inOut[0] = $"{x.HoraEntrada.ToHourString()} - {x.HoraSalida.ToHourString()}";
                        break;
                    case DayOfWeek.Tuesday:
                        inOut[1] = $"{x.HoraEntrada.ToHourString()} - {x.HoraSalida.ToHourString()}";
                        break;
                    case DayOfWeek.Wednesday:
                        inOut[2] = $"{x.HoraEntrada.ToHourString()} - {x.HoraSalida.ToHourString()}";
                        break;
                    case DayOfWeek.Thursday:
                        inOut[3] = $"{x.HoraEntrada.ToHourString()} - {x.HoraSalida.ToHourString()}";
                        break;
                    case DayOfWeek.Friday:
                        inOut[4] = $"{x.HoraEntrada.ToHourString()} - {x.HoraSalida.ToHourString()}";
                        break;
                    case DayOfWeek.Saturday:
                        inOut[5] = $"{x.HoraEntrada.ToHourString()} - {x.HoraSalida.ToHourString()}";
                        break;
                    case DayOfWeek.Sunday:
                        inOut[6] = $"{x.HoraEntrada.ToHourString()} - {x.HoraSalida.ToHourString()}";
                        break;
                    default:
                        break;
                }
            }

            return inOut;
        }

        

        private string SetStatus(bool asistencia, bool retardo, bool falta)
        {
            if (asistencia)
                return "asistencia";

            if (retardo)
                return "retardo";

            if (falta)
                return "falta";

            return string.Empty;
        }

        /// <summary>
        /// Obtiene el resumen de la semana de la app mobile
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("resumen/semana")]
        public async Task<IActionResult> ResumenSemana(ResumenRequest data)
        {
            try
            {
                var asistencia = await _context.Asistencias
                    .Where(w => w.SemanaAnio == data.Semana)
                    .Where(y => y.Anio == data.Anio)
                    .Where(e => e.EmpleadoId == data.EmpleadoId)
                    .ToListAsync();

                var result = asistencia
                    .OrderBy(x => x.Fecha.DayOfWeek)
                    .Select(x => new
                    {
                        Index     = (int) x.Fecha.DayOfWeek,
                        DiaSemana = x.Fecha.ToString("dddd", new CultureInfo("es-MX")).ToUpper(),
                        HoraEntrada = x.HoraEntrada.HasValue
                            ? x.HoraEntrada.Value.ToHourString()
                            : "",
                        HoraSalida = x.HoraSalida.HasValue
                            ? x.HoraSalida.Value.ToHourString()
                            : "",
                        Asistencia = x.Attendence,
                        Retardo    = x.Retardo,
                        Falta      = x.Falta
                    });
                return Ok(result);
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e, "Ocurrio un error al obtener las asistencias del empleado", e);
                return StatusCode(500, "Ocurrio un error al obtener las asistencias del empleado");
            }
        }


        [HttpPost("resumen/mes")]
        public async Task<IActionResult> ResumenMes(ResumenRequest data)
        {
            try
            {
                var query = await _context.Asistencias
                    .Where(x => x.Fecha.Year == data.Anio)
                    .Where(x => x.Fecha.Month == data.Mes)
                    .Where(e => e.Empleado.Id == data.EmpleadoId)
                    .ToListAsync();

                var result = query
                    .GroupBy(x => new
                    {
                        Month = x.Fecha.Month
                    })
                    .Select(c => new MesResponse
                    {
                        Asistencias = c.Count(d => d.Attendence),
                        Retardos    = c.Count(d => d.Retardo),
                        Faltas      = c.Count(d => d.Falta)
                    }).ToList();

                if (result.Any())
                    return Ok(result.FirstOrDefault());

                return Ok(new MesResponse
                {
                    Asistencias = 0,
                    Retardos    = 0,
                    Faltas      = 0
                });
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e, "Ocurrio un error al obtener el resumen mensual del empleado", e);
                return StatusCode(500, "Ocurrio un error al obtener el resumen mensual del empleado");
            }
        }


        [HttpGet("resumen/mes/{empleadoId}")]
        public async Task<IActionResult> GetResumenMes(int empleadoId)
        {
            try
            {
                var now = DateTime.Now;
                var empleado = await _context.Empleados.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == empleadoId);

                if (empleado == null)
                    return NotFound("El empleado no se encontro");

                var query = await _context.Asistencias
                    .Where(e => e.Empleado.Id == empleado.Id)
                    .Where(x => x.Fecha.Month == now.Month)
                    .Where(x => x.Fecha.Year == now.Year)
                    .ToListAsync();

                var result = query
                    .GroupBy(x => new
                    {
                        Month = x.Fecha.Month
                    })
                    .Select(c => new MesResponse
                    {
                        Asistencias = c.Count(d => d.Attendence),
                        Retardos    = c.Count(d => d.Retardo),
                        Faltas      = c.Count(d => d.Falta)
                    }).ToList();

                if (result.Any())
                    return Ok(result.FirstOrDefault());

                return Ok(new MesResponse
                {
                    Asistencias = 0,
                    Retardos    = 0,
                    Faltas      = 0
                });
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e, "Ocurrio un error al obtener el resumen mensual del empleado", e);
                return StatusCode(500, "Ocurrio un error al obtener el resumen mensual del empleado");
            }
        }

        [HttpGet("sincroniza/mes/{empleadoId}")]
        public async Task<ActionResult<List<AsistenciaResponse>>> SincronizaMes(int empleadoId)
        {
            try
            {
                var iniMonth = DateTime.Now.Month - 1;
                var empleado = await _context.Empleados.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == empleadoId);

                if (empleado == null)
                    return NotFound("El empleado no se encontro");

                var result = await _context.Asistencias
                    .Where(e => e.Empleado.Id == empleado.Id)
                    .Where(x => x.Fecha.Month >= iniMonth)
                    .OrderBy(x => x.Fecha)
                    .Select(asistencia => new AsistenciaResponse
                    {
                        AsistenciaId = asistencia.Id,
                        Fecha = asistencia.HoraEntrada.HasValue ? asistencia.HoraEntrada.Value.ToDateString() : "",
                        HoraEntrada =
                            asistencia.HoraEntrada.HasValue ? asistencia.HoraEntrada.Value.ToHourString() : "",
                        HoraSalida = asistencia.HoraSalida.HasValue ? asistencia.HoraSalida.Value.ToHourString() : "",
                        Asistencia = asistencia.Attendence,
                        Retardo    = asistencia.Retardo,
                        Falta      = asistencia.Falta,
                        Semana     = asistencia.SemanaAnio,
                        Mes        = asistencia.Fecha.Month
                    })
                    .ToListAsync();

                return Ok(result);
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e, "Ocurrio un error al obtener el resumen mensual del empleado", e);
                return StatusCode(500, "Ocurrio un error al obtener el resumen mensual del empleado");
            }
        }

        [HttpGet("resumen/mapa")]
        public async Task<ActionResult<IEnumerable<MapaAsistenciaResponse>>> GetAsistenciaMapa()
        {
            try
            {
                var asistencias = await _context.Asistencias
                    .Where(x => x.Fecha.DayOfYear == DateTime.Today.DayOfYear).ToListAsync();

                var empleados = await _context.Empleados.Where(x => x.Active).ToListAsync();

                var empleadoAsistencia = empleados
                    .GroupJoin(asistencias,
                        e => e.Id, a => a.EmpleadoId,
                        (e, a) => new
                        {
                            TramoId    = e.TramoId,
                            EmpleadoId = e.Id,
                            Asistencia = a
                        });


                var result = empleadoAsistencia
                    .GroupBy(x => x.TramoId)
                    .Select(x => new
                    {
                        TramoId       = x.Key,
                        NumTotal      = empleados.Count(e => e.TramoId == x.Key),
                        NumAsistencia = asistencias.Where(t=>t.Empleado.TramoId == x.Key).Count(a => a.Attendence),
                        NumFalta      = asistencias.Where(t=>t.Empleado.TramoId == x.Key).Count(f => f.Falta),
                        NumRetardo    = asistencias.Where(t=>t.Empleado.TramoId == x.Key).Count(r => r.Retardo)
                    })
                    .ToList();

              
                return Ok(result);
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e, "Ocurrio un error al obtener el resumen el reporte asistencia diaria", e);
                return StatusCode(500, "Ocurrio un error al obtener el resumen el reporte asistencia diaria");
            }
        }

        [HttpPost("fix")]
        public async Task<ActionResult> FixData(IEnumerable<FixDataRequest> data)
        {
            try
            {
                var affected = 0;
                foreach (var item in data)
                {
                    var oldEmpleado = await _context.Empleados.FirstOrDefaultAsync(x=>x.Telefono == item.Telefono);
                    var fixEmpleado = await _context.Empleados.FirstOrDefaultAsync(x=>x.Telefono == item.Telefono && x.Active);

                    var asistencias = await _context.Asistencias
                        .Where(x=>x.EmpleadoId ==  oldEmpleado.Id)
                        .Where(x=>x.Fecha.Year == 2021).ToListAsync();

                    foreach (var asistencia in asistencias)
                    {
                        asistencia.EmpleadoId = fixEmpleado.Id;                        
                    }

                    _context.UpdateRange(asistencias);
                    affected += _context.SaveChanges();                    
                }


                return Ok(affected);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }


        }
    }
}
