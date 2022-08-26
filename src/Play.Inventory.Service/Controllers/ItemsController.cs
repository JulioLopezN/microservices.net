using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Models.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<InventoryItem> _itemsRepository;
    private readonly IRepository<CatalogItem> _catalogRepository;

    public ItemsController(IRepository<InventoryItem> itemsRepository, IRepository<CatalogItem> catalogRepository)
    {
        _itemsRepository = itemsRepository;
        _catalogRepository = catalogRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest();
        }

        var inventoryItemEntities = await _itemsRepository.GetAllAsync(item => item.UserId == userId);
        var catalogItemIds = inventoryItemEntities
            .Select(x => x.CatalogItemId)
            .Distinct()
            .ToList();
        var catalogItems = await _catalogRepository.GetAllAsync(x => catalogItemIds.Contains(x.Id));
        var catalogItemsIndexed = catalogItems.ToDictionary(x => x.Id);

        var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem =>
        {
            catalogItemsIndexed.TryGetValue(inventoryItem.CatalogItemId, out var catalogItem);
            return inventoryItem.AsDto(catalogItem?.Name, catalogItem?.Description);
        });

        return Ok(inventoryItemDtos);
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
    {
        var inventoryItem = await _itemsRepository.GetAsync(
            item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId);

        if (inventoryItem == null)
        {
            inventoryItem = new InventoryItem
            {
                CatalogItemId = grantItemsDto.CatalogItemId,
                UserId = grantItemsDto.UserId,
                Quantity = grantItemsDto.Quantity,
                AcquiredDate = DateTimeOffset.UtcNow
            };

            await _itemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += grantItemsDto.Quantity;
            await _itemsRepository.UpdateAsync(inventoryItem);
        }

        return Ok();
    }
}