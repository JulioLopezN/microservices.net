using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Models;
using Play.Common;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<Item> _itemsRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
    {
        _itemsRepository = itemsRepository;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync() 
    {
        var items = await _itemsRepository.GetAllAsync();
        var result = items.Select(x => x.AsDto());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id) 
    {
        var item = await _itemsRepository.GetAsync(id);
        if (item is null) return NotFound();
        
        return Ok(item.AsDto());
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateItemDto dto)
    {
        var item = new Item { 
            Name = dto.Name, 
            Description = dto.Description,
            Price = dto.Price, 
            CreatedDate = DateTimeOffset.Now
        };
        await _itemsRepository.CreateAsync(item);

        await _publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

        return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateItemDto dto)
    {
        var existingItem = await _itemsRepository.GetAsync(id);
        if (existingItem is null) return NotFound();

        existingItem.Name = dto.Name;
        existingItem.Description = dto.Description;
        existingItem.Price = dto.Price;

        await _itemsRepository.UpdateAsync(existingItem);

        await _publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var existingItem = await _itemsRepository.GetAsync(id);
        if (existingItem is null) return NotFound();

        await _itemsRepository.RemoveAsync(id);

        await _publishEndpoint.Publish(new CatalogItemDeleted(id));
        
        return NoContent();
    }
}