using Microsoft.AspNetCore.Mvc;
using InventarioRaServer.Services;
using InventarioRaServer.Models;
using InventarioRaServer.Tools;
using Microsoft.AspNetCore.SignalR;

namespace InventarioRaServer.Controllers;

[Route("[controller]")]
[ApiController]
public class DespachosController : ControllerBase
{
    private readonly IDespachosServicio _despachosServicio;
    private readonly IHubContext<NotificationHub> _hubContext;

    public DespachosController(IDespachosServicio despachosServicio, IHubContext<NotificationHub> hubContext)
    {
        _despachosServicio = despachosServicio;
        _hubContext = hubContext;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _despachosServicio.GetAll();

        return result is not null ? Ok(result) : NotFound();
    }

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var result = _despachosServicio.GetById(id);

        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Dispatch dispatch)
    {
        var result = _despachosServicio.Insert(dispatch);

        if (result)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Un nuevo despacho ha sido agregado");
            return CreatedAtAction(nameof(GetById), new { id = dispatch.Id }, dispatch);
        }
        return BadRequest("No se pudo insertar el despacho");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = _despachosServicio.Delete(id);

        if (result)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Un despacho ha sido eliminado");
            return Ok(result);
        }
        return NotFound();
    }

    [HttpGet("byClientId/{clientId}")]
    public IActionResult GetAllByClientId(string clientId)
    {
        return Ok(_despachosServicio.GetAllByClientId(clientId));
    }

    [HttpGet("byInventoryId/{inventoryId}")]
    public IActionResult GetAllByInventoryId(string inventoryId)
    {
        return Ok(_despachosServicio.GetAllByInventoryId(inventoryId));
    }

    [HttpGet("byDate")]
    public IActionResult GetAllByDate(DateTime startDate, DateTime endDate)
    {
        return Ok(_despachosServicio.GetAllByDate(startDate, endDate));
    }

    [HttpGet("allInventoryIds")]
    public IActionResult GetAllInventoryIds()
    {
        return Ok(_despachosServicio.GetAllInventoryId());
    }

    [HttpGet("exist")]
    public IActionResult Exist()
    {
        return Ok(_despachosServicio.Exist);
    }
}
