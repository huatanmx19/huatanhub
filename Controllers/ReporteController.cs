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
        //public ActionResult<IEnumerable<ReporteResponse>> GetReporteTramo(int id)
        {
            //Ajuste
            SqlConnection con = new SqlConnection();
            con.ConnectionString =
              "Data Source=34.71.46.142;" +
              "Initial Catalog=huatan;" +
              "User id=sqlserver;" +
              "Password=qiGMdk8G3H7LqIG3;";
            //Ajuste
            //var reporte = new List<ReporteResponse>();

            //try
            //{
            var backTime = Environment.GetEnvironmentVariable("DELTA_MINUTES") ?? "-60";
            var minutes = int.Parse(backTime);

            var delta = DateTime.Now.AddMinutes(minutes);

            //////ORIGINAL - INICIO
            //var queryActivos = _context.Locations
            //    .Include(x => x.Empleado)
            //    .Where(x => x.Timestamp > delta);

            //if (id > 0)
            //    queryActivos = queryActivos.Where(x => x.Empleado.TramoId == id);

            //var activos = await queryActivos
            //      .GroupBy(x => x.EmpleadoId)
            //      .Select(g => new
            //      {
            //          id = g.Key,
            //          numData = g.Count()
            //      }).CountAsync();

            //var queryAttendanceToday = _context.Asistencias
            //    .Include(x => x.Empleado)
            //    .ThenInclude(x => x.Tramo)
            //    .Where(x => x.HoraEntrada != null)
            //    .Where(x => x.Fecha == DateTime.Today);

            //if (id > 0)
            //    queryAttendanceToday = queryAttendanceToday.Where(x => x.Empleado.TramoId == id);

            //var attendanceToday = await queryAttendanceToday.CountAsync();

            //var queryEmployeeTotal = _context.Empleados
            //    .Where(x => x.Active);

            //if (id > 0)
            //    queryEmployeeTotal = queryEmployeeTotal.Where(x => x.TramoId == id);

            //var employeeTotal = await queryEmployeeTotal.CountAsync();

            //var inactivos = attendanceToday - activos;

            //    var reporte = new ReporteResponse
            //{
            //    Total = employeeTotal,
            //    Asistencia = attendanceToday,
            //    Activos = activos,
            //    Inactivos = inactivos
            //};

            ////ORIGINAL - FIN

            //Ajuste
            //con.Open();
            //var command = new SqlCommand(qReporte.Reporte, con);
            //command.CommandType =  CommandType.StoredProcedure;
            //command.Parameters.AddWithValue("@id", id);
            //command.Parameters.AddWithValue("@fecha", delta);
            //using (var rd =  command.ExecuteReader())
            //{
            //    while (rd.Read())
            //        reporte.Add(new ReporteResponse
            //        {
            //            Total = rd.IsDBNull(rd.GetOrdinal("Total")) ? 0 : Convert.ToInt32(rd["Total"]),
            //            Asistencia = rd.IsDBNull(rd.GetOrdinal("Asistencia")) ? 0 : Convert.ToInt32(rd["Asistencia"]),
            //            Activos = rd.IsDBNull(rd.GetOrdinal("Activos")) ? 0 : Convert.ToInt32(rd["Activos"]),
            //            Inactivos = rd.IsDBNull(rd.GetOrdinal("Inactivos")) ? 0 : Convert.ToInt32(rd["Inactivos"])
            //        });
            //}
            var reporte = new List<ReporteResponse>();
            ////////////////////////////////////////////////////////////////
            using (SqlConnection connection = new SqlConnection(con.ConnectionString))
            {
                using (var command = new SqlCommand(qReporte.Reporte, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@fecha", delta);

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            reporte = new List<ReporteResponse>();
                            reporte.Add(new ReporteResponse
                            {
                                Total = reader.IsDBNull(reader.GetOrdinal("Total")) ? 0 : Convert.ToInt32(reader["Total"]),
                                Asistencia = reader.IsDBNull(reader.GetOrdinal("Asistencia")) ? 0 : Convert.ToInt32(reader["Asistencia"]),
                                Activos = reader.IsDBNull(reader.GetOrdinal("Activos")) ? 0 : Convert.ToInt32(reader["Activos"]),
                                Inactivos = reader.IsDBNull(reader.GetOrdinal("Inactivos")) ? 0 : Convert.ToInt32(reader["Inactivos"])
                            });
                        }
                    }
                }
            }
            return Ok(reporte);
            ////////////////////////////////////////////////////////////////

            //Original

            //}
            //catch (ApplicationException e)
            //{
            //    _logger.LogError(e.Message, e);
            //    return StatusCode(500);
            //}
        }
    }
}
