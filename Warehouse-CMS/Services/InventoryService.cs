using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;

public interface IInventoryService
{
    bool CheckStock(int productId, int requestedQuantity);
    void UpdateStock(int productId, int quantity, bool isAddition);
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
        if (product != null)
        {
            product.StockQuantity = isAddition
                ? product.StockQuantity + quantity
                : product.StockQuantity - quantity;
            _productRepository.Update(product);
        }
    }

    public List<Product> GetLowStockProducts(int threshold)
    {
        return _productRepository.GetAll().Where(p => p.StockQuantity < threshold).ToList();
    }
}
