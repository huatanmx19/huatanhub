using Microsoft.AspNetCore.Mvc;

using System;
using System.Linq;
using System.Threading.Tasks;
using HuatanHub.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using HuatanHub.Models.Response;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using HuatanHub.Data.Querys;
using Microsoft.IdentityModel.Protocols;
using System.Configuration;

namespace HuatanHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReporteController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly ILogger<ReporteController> _logger;

        public ReporteController(ApiContext context, ILogger<ReporteController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReporteResponse>> GetReporteTramo(int id)
        {
            //try
            //{
            var backTime = Environment.GetEnvironmentVariable("DELTA_MINUTES") ?? "-60";
            var minutes = int.Parse(backTime);

            var delta = DateTime.Now.AddMinutes(minutes);

            //////ORIGINAL - INICIO
            var queryActivos = _context.Locations
                .Include(x => x.Empleado)
                .Where(x => x.Timestamp > delta);

            if (id > 0)
                queryActivos = queryActivos.Where(x => x.Empleado.TramoId == id);

            var activos = await queryActivos
                  .GroupBy(x => x.EmpleadoId)
                  .Select(g => new
                  {
                      id = g.Key,
                      numData = g.Count()
                  }).CountAsync();

            var queryAttendanceToday = _context.Asistencias
                .Include(x => x.Empleado)
                .ThenInclude(x => x.Tramo)
                .Where(x => x.HoraEntrada != null)
                .Where(x => x.Fecha == DateTime.Today);

            if (id > 0)
                queryAttendanceToday = queryAttendanceToday.Where(x => x.Empleado.TramoId == id);

            var attendanceToday = await queryAttendanceToday
                .GroupBy(x => x.EmpleadoId)
                .CountAsync();

            var queryEmployeeTotal = _context.Empleados
                .Where(x => x.Active);

            if (id > 0)
                queryEmployeeTotal = queryEmployeeTotal.Where(x => x.TramoId == id);

            var employeeTotal = await queryEmployeeTotal.CountAsync();

            var inactivos = attendanceToday - activos;

            var reporte = new ReporteResponse
            {
                Total = employeeTotal,
                Asistencia = attendanceToday,
                Activos = activos,
                Inactivos = inactivos
            };

            //Original
            return Ok(reporte);
            //}
            //catch (ApplicationException e)
            //{
            //    _logger.LogError(e.Message, e);
            //    return StatusCode(500);
            //}
        }
    }
}
