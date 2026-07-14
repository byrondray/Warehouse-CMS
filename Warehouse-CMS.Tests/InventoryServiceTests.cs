using Moq;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;
using Xunit;

namespace Warehouse_CMS.Tests
{
    public class InventoryServiceTests
    {
        private static (InventoryService service, Mock<IProductRepository> repo) CreateService()
        {
            var repo = new Mock<IProductRepository>();
            return (new InventoryService(repo.Object), repo);
        }

        [Fact]
        public async Task DeductStock_ReturnsError_WhenProductNotFound()
        {
            var (service, repo) = CreateService();
            repo.Setup(r => r.GetByIdAsync(42)).ReturnsAsync((Product?)null);

            var error = await service.DeductStockForOrderItemAsync(
                new OrderItem { ProductId = 42, Quantity = 1 }
            );

            Assert.Contains("not found", error);
            repo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task DeductStock_ReturnsError_WhenInsufficientStock()
        {
            var (service, repo) = CreateService();
            var product = new Product
            {
                Id = 1,
                Name = "Hammer",
                StockQuantity = 3,
                Price = 10m,
            };
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            var error = await service.DeductStockForOrderItemAsync(
                new OrderItem { ProductId = 1, Quantity = 5 }
            );

            Assert.Contains("Insufficient stock", error);
            Assert.Equal(3, product.StockQuantity); // unchanged
            repo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task DeductStock_DeductsAndPersists_OnSuccess()
        {
            var (service, repo) = CreateService();
            var product = new Product
            {
                Id = 1,
                Name = "Hammer",
                StockQuantity = 10,
                Price = 14.99m,
            };
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            var item = new OrderItem { ProductId = 1, Quantity = 4 };
            var error = await service.DeductStockForOrderItemAsync(item);

            Assert.Null(error);
            Assert.Equal(6, product.StockQuantity); // 10 - 4
            Assert.Equal(14.99m, item.UnitPrice); // priced from the product
            repo.Verify(r => r.UpdateAsync(product), Times.Once);
        }

        [Fact]
        public async Task GetLowStockProducts_ReturnsRepositoryResults()
        {
            var (service, repo) = CreateService();
            var lowStock = new List<Product>
            {
                new Product { Id = 1, StockQuantity = 2 },
                new Product { Id = 2, StockQuantity = 0 },
            };
            repo.Setup(r => r.GetLowStockAsync(5)).ReturnsAsync(lowStock);

            var result = await service.GetLowStockProductsAsync(5);

            Assert.Equal(2, result.Count);
        }
    }
}
