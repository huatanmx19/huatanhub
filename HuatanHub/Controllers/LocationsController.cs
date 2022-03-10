using HuatanHub.Data;
using HuatanHub.Hubs;
using HuatanHub.Models.Request;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NetTopologySuite.Geometries;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HuatanApi.Tools;
using HuatanHub.Models.Response;
using HuatanHub.Tools;
using Location = HuatanHub.Entities.Location;

namespace HuatanHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {

        private readonly ApiContext _context;
        private readonly ILogger<LocationsController> _logger;
        private readonly IHubContext<LocationHub> _hubContext;

        public LocationsController(ApiContext context, ILogger<LocationsController> logger, IHubContext<LocationHub> hubContext)
        {

            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }


        /// <summary>
        /// Obtiene la lista de empleados activos hoy
        /// </summary>
        /// <returns></returns>
        [Route("today")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Location>>> GetTotales()
        {
            var today = DateTime.Today;

            var result = await _context.Locations.Where(x => x.Timestamp.Date == today).ToListAsync();

            return Ok(result);
        }


        [Route("actual-markers")]
        [HttpGet]
        public ActionResult<IEnumerable<MarkerResponse>> GetActivos()
        {
            // TODO: Configurar con una variable de entorno

            var backTime = Environment.GetEnvironmentVariable("DELTA_MINUTES") ?? "-60";
            var minutes = int.Parse(backTime);

            var delta = DateTime.Now.AddMinutes(minutes);

            var lastLocations = _context.Locations
                .Where(x => x.Timestamp > delta)
                .AsEnumerable()
                .OrderByDescending(x => x.Timestamp)
                .Distinct(new LocationEqualityComparer())
                .ToList();
                

            var result = lastLocations
                .Join(_context.Empleados
                        .Include(t => t.Tramo)
                        .Include(a =>a.Asistencias.Where(t => t.Fecha == DateTime.Today)),
                
                    location => location.EmpleadoId,
                    employee => employee.Id,
                    (l, e) =>
                        new MarkerResponse
                        {
                            Id = e.Id,
                            NumNomina = e.NumNomina,
                            Nombre = $"{e.Nombre} {e.Paterno} {e.Materno}",
                            Puesto = e.Puesto,
                            Telefono = e.Telefono,
                            BatteryLevel = l.BatteryLevel,
                            SignalPower = l.SignalPower,
                            IsInside = l.IsInside,
                            Lat = l.Lat,
                            Lng = l.Lng,
                            Timestamp = l.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                            TramoId = e.Tramo?.Id ?? 0,
                            TramoNombre = e.Tramo == null ? "":  e.Tramo.Nombre
                        }).ToList();

            return Ok(result);
        }


        /// <summary>
        /// Obtiene la lista de empleados activos 5 minutos
        /// </summary>
        /// <returns></returns>
        [HttpGet("tramo")]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocationTramo(int tramo)
        {
            return await _context.Locations.Take(50).ToListAsync();
        }

        /// <summary>
        /// Update the marker
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> PostLocation(LocationRequest data)
        {
            try
            {

                String date = DateTime.Now.Ticks.ToString();

                var location = new Location
                {
                    Timestamp = data.Timestamp.ToDateFromData("yyyy-MM-ddTHH:mm:ss.ffffff"),
                    EmpleadoId = data.Empleado.Id,
                    IsInside = data.IsInside,
                    Lat = data.Lat,
                    Lng = data.Lng,
                    BatteryLevel = data.BatteryLevel,
                    SignalPower = data.SignalPower,
                    Lugar = new Point(new Coordinate(data.Lng, data.Lat)) { SRID = 4326 },
                };


                var marker = new MarkerResponse
                {
                    Id = data.Empleado.Id,
                    NumNomina = data.Empleado.NumNomina,
                    Nombre = $"{data.Empleado.Nombre} {data.Empleado.Paterno} {data.Empleado.Materno}",
                    Puesto = data.Empleado.Puesto,
                    TramoNombre = data.Empleado.TramoNombre,
                    TramoId = data.Empleado.TramoId ?? 0,
                    Telefono = data.Empleado.Telefono,
                    Lat = data.Lat,
                    Lng = data.Lng,
                    IsInside = data.IsInside,
                    Timestamp = data.Timestamp,
                    BatteryLevel = data.BatteryLevel,
                    SignalPower = data.SignalPower
                };

                _context.Locations.Add(location);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("update-marker", marker);

                

                _logger.LogDebug($" [{DateTime.Now}] {data.Empleado.Id}  {data.Lat} {data.Lng}  ");

                return StatusCode(201);
            }
            catch (ApplicationException e)
            {
                _logger.LogError(e, "Ocurrio un error al crear el tramo", data);
                return StatusCode(500, e);
            }
        }
    }
}
