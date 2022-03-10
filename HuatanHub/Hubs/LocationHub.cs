using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HuatanHub.Models;
using HuatanHub.Models.Request;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HuatanHub.Hubs
{
    public class LocationHub : Hub
    {
        private readonly ILogger<LocationHub> _logger;
        public LocationHub(ILogger<LocationHub> logger)
        {
            _logger = logger;
        }
        //public async Task UpdateLocation(LocationRequest data)
        //{
        //    _logger.LogDebug($"[[ {Context.ConnectionId} ]] [{DateTime.Now}] {data.EmpleadoId}  {data.Nombre} {DateTime.Now} ");
        //     await Clients.All.SendAsync("update-marker", data);
        //}

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;

            _logger.LogDebug($"Cliente conectado -> {connectionId} {DateTime.Now} ");

            await Clients.All.SendAsync("on-connected", connectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;

            _logger.LogDebug($"Cliente desconectado -> {connectionId} {DateTime.Now} ");

            await Clients.All.SendAsync("on-disconnected", connectionId);
            await base.OnDisconnectedAsync(exception);
        }


    }
}
