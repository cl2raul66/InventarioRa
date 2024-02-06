using Microsoft.AspNetCore.Mvc;
using InventarioRaServer.Services;
using InventarioRaServer.Models;
using InventarioRaServer.Tools;
using Microsoft.AspNetCore.SignalR;

namespace InventarioRaServer.Controllers
{
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
            var result = _inventarioServicio.Insert(inventory);

            if (result)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Un nuevo inventario ha sido agregado");
                return CreatedAtAction(nameof(GetById), new { id = inventory.Id }, inventory);
            }
            return BadRequest("No se pudo insertar el inventario");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Inventory inventory)
        {
            var result = _inventarioServicio.Update(inventory);

            if (result)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Un inventario ha sido actualizado");
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
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Un inventario ha sido eliminado");
                return Ok(result);
            }
            return NotFound();
        }
    }
}
