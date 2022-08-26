using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Models.Dtos;

namespace Play.Inventory.Service;

public static class Extensions
{
    public static InventoryItemDto AsDto(this InventoryItem item, string name, string description)
    {
        return new InventoryItemDto(item.CatalogItemId, name, description, item.Quantity, item.AcquiredDate);
    }
}