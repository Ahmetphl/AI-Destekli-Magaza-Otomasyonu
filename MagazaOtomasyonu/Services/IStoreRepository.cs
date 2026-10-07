using System.Data;
using MagazaOtomasyonu.Models;

namespace MagazaOtomasyonu.Services;

public interface IStoreRepository
{
    DataTable GetCategories();
    void AddCategory(string name);

    DashboardSummary GetDashboardSummary();
    DataTable GetSalesReport();
    DataTable GetBestSellingProducts(int limit);
    DataTable GetRecentSales(int limit);
    DataTable GetLowestStockProducts(int limit);

    IReadOnlyList<SaleProductOption> GetAvailableProducts();
    DataTable GetProducts();
    void SaveProduct(ProductSaveRequest product);
    void DeactivateProduct(int productId);
    DataTable GetStockProducts();
    void UpdateStock(int productId, int quantity);

    long CompleteSale(SaleRequest request);
    DataTable GetRecentSalesForEntry(int limit);
    DataTable GetOpenDebts();
    decimal? GetOutstandingBalance(int saleId);
    void RecordPayment(int saleId, decimal amount, string description);
    DataTable GetSaleDetails(int saleId);
    DataTable GetRecentReturns(int limit);
    void CompleteReturnExchange(ReturnExchangeRequest request);
}
