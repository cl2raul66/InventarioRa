using Microsoft.AspNetCore.Mvc;
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

    [HttpGet("Exist")]
    public IActionResult Exist()
    {
        var result = _clientesServicio.Exist;

        return Ok(result);
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
        var result = _clientesServicio.Insert(client);

        if (result)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Un nuevo cliente ha sido agregado");
            return Ok(client);
        }
        return BadRequest("No se pudo insertar el cliente");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = _clientesServicio.Delete(id);

        if (result)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Un cliente ha sido eliminado");
            return Ok(result);
        }
        return NotFound();
    }
}
