namespace MagazaOtomasyonu.Models;

public sealed record SaleProductOption(
    int Id,
    string Name,
    string Category,
    string Size,
    string Color,
    decimal Price,
    int Stock)
{
    public override string ToString() => $"{Name} | stok {Stock}";
}

public sealed record SaleLine(int ProductId, string ProductName, int Quantity, decimal UnitPrice);

public sealed record SaleRequest(
    string CustomerName,
    string Note,
    decimal AmountPaid,
    IReadOnlyCollection<SaleLine> Lines);

public sealed record ReturnExchangeRequest(int SaleId, int SaleDetailId, int Quantity, int? ReplacementProductId, int ReplacementQuantity, string Settlement, string Reason);

public sealed record DashboardSummary(
    decimal TodaySales,
    decimal MonthSales,
    int TotalStock,
    decimal EstimatedProfit,
    decimal MonthCollections,
    decimal OpenDebt);

public sealed record ProductSaveRequest(
    int ProductId,
    string Name,
    string Category,
    string Size,
    string Color,
    decimal CostPrice,
    decimal SalePrice,
    int Stock);
