﻿using Microsoft.AspNetCore.Mvc;
using InventarioRaServer.Services;
using InventarioRaServer.Models;
using Microsoft.AspNetCore.SignalR;
using InventarioRaServer.Tools;

namespace InventarioRaServer.Controllers;

[Route("[controller]")]
[ApiController]
public class ClientesController : ControllerBase
{
    private readonly IClientesServicio _clientesServicio;
    private readonly IHubContext<NotificationHub> _hubContext;

    public ClientesController(IClientesServicio clientesServicio, IHubContext<NotificationHub> hubContext)
    {
        _clientesServicio = clientesServicio;
        _hubContext = hubContext;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _clientesServicio.GetAll();

        return result is not null ? Ok(result) : NotFound();
    }

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var result = _clientesServicio.GetById(id);

        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Client client)
    {
        string id = _clientesServicio.Insert(client);

        if (!string.IsNullOrEmpty(id))
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", $"{OperationType.Create}:{nameof(Client)}:{id}");
            return Ok(id);
        }
        return BadRequest();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)                                 
    {
        var result = _clientesServicio.Delete(id);

        if (result)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", $"{OperationType.Delete}:{nameof(Client)}:{id}");
            return Ok(result);
        }
        return NotFound();
    }
}
