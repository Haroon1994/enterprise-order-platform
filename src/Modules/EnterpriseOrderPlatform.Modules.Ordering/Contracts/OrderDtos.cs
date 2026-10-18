namespace EnterpriseOrderPlatform.Modules.Ordering.Contracts;

public sealed record OrderLineResponse(string Sku, string Name, decimal UnitPrice, int Quantity);

public sealed record OrderResponse(Guid Id, string Status, decimal Total, string Currency, IReadOnlyList<OrderLineResponse> Lines);
