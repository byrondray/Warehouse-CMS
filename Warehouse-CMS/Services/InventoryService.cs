using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;

public interface IInventoryService
{
    bool CheckStock(int productId, int requestedQuantity);
    void UpdateStock(int productId, int quantity, bool isAddition);
    string? DeductStockForOrderItem(OrderItem item);
    List<Product> GetLowStockProducts(int threshold);
}

public class InventoryService : IInventoryService
{
    private readonly IProductRepository _productRepository;

    public InventoryService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public bool CheckStock(int productId, int requestedQuantity)
    {
        var product = _productRepository.GetById(productId);
        return product?.StockQuantity >= requestedQuantity;
    }

    public void UpdateStock(int productId, int quantity, bool isAddition)
    {
        var product = _productRepository.GetById(productId);
        if (product == null)
            return;

        var newQuantity = isAddition
            ? product.StockQuantity + quantity
            : product.StockQuantity - quantity;

        if (newQuantity < 0)
            throw new InvalidOperationException(
                $"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}, Requested: {quantity}"
            );

        product.StockQuantity = newQuantity;
        _productRepository.Update(product);
    }

    public string? DeductStockForOrderItem(OrderItem item)
    {
        var product = _productRepository.GetById(item.ProductId);
        if (product == null)
            return $"Product with ID {item.ProductId} not found";

        if (product.StockQuantity < item.Quantity)
            return $"Insufficient stock for product: {product.Name}. Available: {product.StockQuantity}, Requested: {item.Quantity}";

        product.StockQuantity -= item.Quantity;
        item.UnitPrice = product.Price;
        return null;
    }

    public List<Product> GetLowStockProducts(int threshold)
    {
        return _productRepository.GetAll().Where(p => p.StockQuantity < threshold).ToList();
    }
}
