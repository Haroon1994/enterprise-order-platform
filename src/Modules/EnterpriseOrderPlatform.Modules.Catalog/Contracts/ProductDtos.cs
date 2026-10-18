namespace EnterpriseOrderPlatform.Modules.Catalog.Contracts;

public sealed record CreateProductRequest(string? Sku, string? Name, decimal Price);

public sealed record ProductResponse(Guid Id, string Sku, string Name, decimal Price, string Currency);
