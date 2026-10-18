namespace EnterpriseOrderPlatform.Modules.Inventory.Contracts;

public sealed record SetStockRequest(int OnHand);

public sealed record StockResponse(string Sku, int OnHand, int Reserved, int Available);
