using Microsoft.AspNetCore.Mvc;
using InventarioRaServer.Services;
using InventarioRaServer.Models;
using InventarioRaServer.Tools;
using Microsoft.AspNetCore.SignalR;

namespace InventarioRaServer.Controllers;

[Route("[controller]")]
[ApiController]
public class InventarioController : ControllerBase
{
    private readonly IInventarioServicio _inventarioServicio;
    private readonly IHubContext<NotificationHub> _hubContext;

    public InventarioController(IInventarioServicio inventarioServicio, IHubContext<NotificationHub> hubContext)
    {
        _inventarioServicio = inventarioServicio;
        _hubContext = hubContext;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _inventarioServicio.GetAll();

        return result is not null ? Ok(result) : NotFound();
    }

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var result = _inventarioServicio.GetById(id);

        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Inventory inventory)
    {
        string id = _inventarioServicio.Insert(inventory);

        if (!string.IsNullOrEmpty(id))
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", $"{OperationType.Create}:{nameof(Inventory)}:{id}");
            return Ok();
        }
        return BadRequest();
    }

    [HttpPut]
    public async Task<IActionResult> Update(Inventory inventory)
    {
        var result = _inventarioServicio.Update(inventory);

        if (result)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", $"{OperationType.Update}:{nameof(Inventory)}:{inventory.Id}");
            return Ok(result);
        }
        return NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = _inventarioServicio.Delete(id);

        if (result)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", $"{OperationType.Delete}:{nameof(Inventory)}:{id}");
            return Ok(result);
        }
        return NotFound();
    }

    [HttpGet("article/{article}")]
    public IActionResult GetByArticle(string article)
    {
        var result = _inventarioServicio.GetByArticle(article);

        return result is not null ? Ok(result) : NotFound();
    }

    [HttpGet("articles")]
    public IActionResult GetAllArticles()
    {
        var result = _inventarioServicio.GetAllArticle();

        return result is not null ? Ok(result) : NotFound();
    }

    [HttpGet("exist")]
    public IActionResult Exist()
    {
        var result = _inventarioServicio.Exist;

        return Ok(result);
    }

    [HttpGet("totalstock")]
    public IActionResult TotalStock()
    {
        var result = _inventarioServicio.TotalStock;

        return Ok(result);
    }
}
