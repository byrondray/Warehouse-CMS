using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;

public interface IInventoryService
{
    Task<string?> DeductStockForOrderItemAsync(OrderItem item);
    Task<List<Product>> GetLowStockProductsAsync(int threshold);
}

public class InventoryService : IInventoryService
{
    private readonly IProductRepository _productRepository;

    public InventoryService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<string?> DeductStockForOrderItemAsync(OrderItem item)
    {
        var product = await _productRepository.GetByIdAsync(item.ProductId);
        if (product == null)
            return $"Product with ID {item.ProductId} not found";

        if (product.StockQuantity < item.Quantity)
            return $"Insufficient stock for product: {product.Name}. Available: {product.StockQuantity}, Requested: {item.Quantity}";

        product.StockQuantity -= item.Quantity;
        item.UnitPrice = product.Price;

        // Persist the stock change explicitly rather than relying on a later SaveChanges
        // (e.g. when the order is added) to flush this tracked entity by side effect.
        await _productRepository.UpdateAsync(product);
        return null;
    }

    public async Task<List<Product>> GetLowStockProductsAsync(int threshold)
    {
        return (await _productRepository.GetLowStockAsync(threshold)).ToList();
    }
}
